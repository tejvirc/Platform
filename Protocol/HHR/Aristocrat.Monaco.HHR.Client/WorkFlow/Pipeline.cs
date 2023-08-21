namespace Aristocrat.Monaco.Hhr.Client.WorkFlow
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using log4net;

    /// <summary>
    ///     Create a Dataflow pipeline
    /// </summary>
    /// <typeparam name="TInput">Input type of Pipeline.</typeparam>
    /// <typeparam name="TOutput">Output type of Pipeline.</typeparam>
    internal class Pipeline<TInput, TOutput>
    {
        private readonly List<IDataflowBlock> _dataflowBlocks = new List<IDataflowBlock>();
        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Pipeline<TInput, TOutput> AddBlock<TLocalInput, TLocalOutput>(
            Func<TLocalInput, TLocalOutput> stepFunc,
            Action<Exception> error = null)
        {
            var transformBlock =
                new TransformBlock<MessageWrapper<TLocalInput, TOutput>, MessageWrapper<TLocalOutput, TOutput>>(
                    input =>
                    {
                        try
                        {
                            return input.TaskCompletionSource.Task.IsFaulted
                                ? Failure<TLocalOutput>(input.TaskCompletionSource)
                                : Success(stepFunc(input.Value), input.TaskCompletionSource);
                        }
                        catch (Exception e)
                        {
                            error?.Invoke(e);
                            input.TaskCompletionSource.SetException(e);
                            return Failure<TLocalOutput>(input.TaskCompletionSource);
                        }
                    });

            AddDataflowBlock(transformBlock);

            return this;
        }

        /// <summary>
        ///     Adds block in pipeline.
        /// </summary>
        /// <typeparam name="TLocalInput">Input type of Block.</typeparam>
        /// <typeparam name="TLocalOutput">Output type of Block.</typeparam>
        /// <param name="stepFunc">Function to execute on this block.</param>
        /// <param name="error">Error action if block fails to perform stepFunc.</param>
        /// <returns>Returns updated Pipeline.</returns>
        public Pipeline<TInput, TOutput> AddBlock<TLocalInput, TLocalOutput>(
            Func<TLocalInput, Task<TLocalOutput>> stepFunc,
            Action<Exception> error = null)
        {
            var transformBlock =
                new TransformBlock<MessageWrapper<TLocalInput, TOutput>, MessageWrapper<TLocalOutput, TOutput>>(
                    async input =>
                    {
                        try
                        {
                            return input.TaskCompletionSource.Task.IsFaulted
                                ? Failure<TLocalOutput>(input.TaskCompletionSource)
                                : Success(await stepFunc(input.Value), input.TaskCompletionSource);
                        }
                        catch (Exception e)
                        {
                            input.TaskCompletionSource.SetException(e);
                            error?.Invoke(e);
                            return Failure<TLocalOutput>(input.TaskCompletionSource);
                        }
                    });

            AddDataflowBlock(transformBlock);

            return this;
        }

        /// <summary>
        ///     Create pipeline by adding ActionBlock in the end of pipeline, which doesn't returns an output
        ///     but sets output into TaskCompletionSource.
        /// </summary>
        /// <returns>Pipeline.</returns>
        public Pipeline<TInput, TOutput> CreatePipeline()
        {
            var resultStep = new ActionBlock<MessageWrapper<TOutput, TOutput>>(
                message =>
                {
                    if (message.TaskCompletionSource.Task.IsFaulted)
                    {
                        return;
                    }

                    if (!message.TaskCompletionSource.TrySetResult(message.Value))
                    {
                        _logger.Info("Unable to set result since Task is already completed.");
                    }
                });
            var resultBlock = _dataflowBlocks.Last() as ISourceBlock<MessageWrapper<TOutput, TOutput>>;
            resultBlock.LinkTo(resultStep);
            return this;
        }

        /// <summary>
        ///     Execute pipeline with given input.
        /// </summary>
        /// <param name="input">Input to flow into pipeline.</param>
        /// <param name="token">A token to interrupt pipeline if required.</param>
        /// <returns>Output of pipeline.</returns>
        public async Task<TOutput> Execute(TInput input, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<TOutput>();
            var source = _dataflowBlocks.First() as ITargetBlock<MessageWrapper<TInput, TOutput>>;

            token.Register(
                () =>
                {
                    if (!tcs.TrySetException(new OperationCanceledException("Pipeline flow cancelled.")))
                    {
                        _logger.Error("Unable to set exception to Task since the Task is already complete.");
                    }
                });

            try
            {
                if (!await source.SendAsync(new MessageWrapper<TInput, TOutput>(input, tcs), token))
                {
                    tcs.TrySetCanceled(token);
                    _logger.Error("Unable to Send data to pipeline ");
                }
            }
            catch (Exception e)
            {
                _logger.Error("Unable to send data through pipeline.", e);
                tcs.TrySetCanceled(token);
            }

            return await tcs.Task;
        }

        private MessageWrapper<TIn, TOutput> Failure<TIn>(TaskCompletionSource<TOutput> tcs)
        {
            return new MessageWrapper<TIn, TOutput>(default(TIn), tcs);
        }

        private MessageWrapper<TIn, TOutput> Success<TIn>(TIn value, TaskCompletionSource<TOutput> tcs)
        {
            return new MessageWrapper<TIn, TOutput>(value, tcs);
        }

        private void AddDataflowBlock<TIn, TOut>(
            TransformBlock<MessageWrapper<TIn, TOutput>, MessageWrapper<TOut, TOutput>> block)
        {
            if (_dataflowBlocks.Count > 0)
            {
                var lastBlock = _dataflowBlocks.Last();
                var sourceBlock = lastBlock as ISourceBlock<MessageWrapper<TIn, TOutput>>;
                sourceBlock?.LinkTo(
                    block,
                    new DataflowLinkOptions { PropagateCompletion = true },
                    message => !message.TaskCompletionSource.Task.IsFaulted);
                sourceBlock?.LinkTo(
                    DataflowBlock.NullTarget<MessageWrapper<TIn, TOutput>>(),
                    new DataflowLinkOptions { PropagateCompletion = true },
                    message => message.TaskCompletionSource.Task.IsFaulted);
            }

            _dataflowBlocks.Add(block);
        }
    }
}