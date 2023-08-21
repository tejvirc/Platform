namespace Aristocrat.Monaco.Common
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>A task extensions helper class.</summary>
    public static class TaskExtensions
    {
        /// <summary>A Task extension method that fire and forgets the result.</summary>
        /// <exception cref="TimeoutException">Thrown when a Timeout error condition occurs.</exception>
        /// <param name="task">The task to act on.</param>
        /// <param name="exceptionHandler">(Optional) The exception handler.</param>
        public static void FireAndForget(this Task task, Action<AggregateException> exceptionHandler = null)
        {
            task.ContinueWith(
                t => { exceptionHandler?.Invoke(t.Exception); },
                TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>A Task extension method that times out after <paramref name="timeout" />.</summary>
        /// <exception cref="TimeoutException">Thrown when a Timeout error condition occurs.</exception>
        /// <param name="task">The task to act on.</param>
        /// <param name="timeout">
        ///     The time to wait before timing out
        /// </param>
        /// <returns>An asynchronous result.</returns>
        // ReSharper disable once UnusedMember.Global
        public static async Task TimeoutAfter(this Task task, TimeSpan timeout)
        {
            // Short-circuit #1: infinite timeout or task already completed
            if (task.IsCompleted || timeout == Timeout.InfiniteTimeSpan)
            {
                // Either the task has already completed or timeout will never occur.
                await task;
                return;
            }

            using (var source = new CancellationTokenSource())
            {
                var timeoutTask = Task.Delay(timeout, source.Token);
                if (task == await Task.WhenAny(task, timeoutTask))
                {
                    source.Cancel();
                    await task;
                    return;
                }

                throw new TimeoutException("The operation has timed out.");
            }
        }

        /// <summary>
        ///     A <see cref="TaskCompletionSource{TResult}"/> extension method that times out after <paramref name="timeout" />
        /// </summary>
        /// <exception cref="TimeoutException">Thrown when a Timeout error condition occurs.</exception>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <param name="taskCompletion">The <see cref="TaskCompletionSource{TResult}"/> to act on.</param>
        /// <param name="timeout">The time to wait before timing out, or -1 to wait indefinitely.</param>
        /// <returns>An asynchronous result.</returns>
        public static Task<TResult> TimeoutAfter<TResult>(this TaskCompletionSource<TResult> taskCompletion, TimeSpan timeout)
        {
            if (taskCompletion == null)
            {
                throw new ArgumentNullException(nameof(taskCompletion));
            }

            return taskCompletion.Task.TimeoutAfter(timeout);
        }

        /// <summary>A Task extension method that times out after <paramref name="timeout" />.</summary>
        /// <exception cref="TimeoutException">Thrown when a Timeout error condition occurs.</exception>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <param name="task">The task to act on.</param>
        /// <param name="timeout">The time to wait before timing out, or -1 to wait indefinitely.</param>
        /// <returns>An asynchronous result.</returns>
        // ReSharper disable once UnusedMember.Global
        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            // Short-circuit #1: infinite timeout or task already completed
            if (task.IsCompleted || timeout == Timeout.InfiniteTimeSpan)
            {
                // Either the task has already completed or timeout will never occur.
                return await task; // Very important in order to propagate exceptions
            }

            using (var source = new CancellationTokenSource())
            {
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, source.Token));
                if (completedTask == task)
                {
                    source.Cancel();
                    return await task; // Very important in order to propagate exceptions
                }

                throw new TimeoutException("The operation has timed out.");
            }
        }

        /// <summary>
        ///     Gets a MaxDegreeOfParallelism based on a percentage of the environments processor count
        /// </summary>
        /// <param name="percentOfProcessorCount"></param>
        /// <returns></returns>
        public static int MaxDegreeOfParallelism(double percentOfProcessorCount = 0.75)
        {
            return Convert.ToInt32(Math.Floor(Environment.ProcessorCount * percentOfProcessorCount));
        }

        /// <summary>
        ///     Allows an AutoResetEvent to be awaited
        /// </summary>
        /// <param name="event"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static Task WaitOneAsync(this AutoResetEvent @event, int timeout = Timeout.Infinite)
        {
            var source = new TaskCompletionSource<bool>();

            Task.Run(
                () =>
                {
                    if (@event.WaitOne(timeout))
                    {
                        source.TrySetResult(true);
                    }
                    else
                    {
                        source.TrySetCanceled();
                    }
                });

            return source.Task;
        }

        /// <summary>A Task extension method that wait for completion.</summary>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <param name="task">The task to act on.</param>
        /// <returns>A TResult.</returns>
        public static TResult WaitForCompletion<TResult>(this Task<TResult> task)
        {
            return task.GetAwaiter().GetResult();
        }

        /// <summary>A Task extension method that wait for completion.</summary>
        /// <param name="task">The task to act on.</param>
        public static void WaitForCompletion(this Task task)
        {
            task.GetAwaiter().GetResult();
        }

        /// <summary>
        ///     Task extension method that safely does not wait on execution of the task
        ///     and calls a callback function to log an exception.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="handler">Callback function.</param>
        public static Task HandleException(this Task task, Action<Exception> handler)
        {
            task.ContinueWith(
                t =>
                {
                    if (!t.IsFaulted)
                    {
                        return;
                    }

                    handler.Invoke(t.Exception);
                    t.Exception?.Handle(exception => true);
                },
                TaskContinuationOptions.OnlyOnFaulted);

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Runs the specified asynchronous function and returns on the current thread
        /// </summary>
        /// <param name="this">The asynchronous function to execute</param>
        public static void RunOnCurrentThread(this Func<Task> @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var current = SynchronizationContext.Current;
            try
            {
                // Establish the new context
                using (var context = new SingleThreadSynchronizationContext())
                {
                    SynchronizationContext.SetSynchronizationContext(context);

                    var task = @this();
                    if (task == null)
                    {
                        throw new InvalidOperationException("No task provided.");
                    }

                    task.ContinueWith(delegate { context.Complete(); }, TaskScheduler.Default);

                    context.RunOnCurrentThread();
                    task.GetAwaiter().GetResult();
                }
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(current);
            }
        }

        /// <summary>
        ///     Gets the current synchronization context.
        /// </summary>
        /// <returns>Current synchronization context.</returns>
        public static TaskScheduler GetSynchronizationContext()
        {
            // If there is no SyncContext for this thread (e.g. we are in a unit test
            // or console scenario instead of running in an app), then just use the
            // default scheduler because there is no UI thread to sync with.
            return SynchronizationContext.Current != null ? TaskScheduler.FromCurrentSynchronizationContext() : TaskScheduler.Current;
        }

        private sealed class SingleThreadSynchronizationContext : SynchronizationContext, IDisposable
        {
            private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> _queue =
                new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

            private bool _disposed;

            public override void Post(SendOrPostCallback callback, object state)
            {
                if (callback == null)
                {
                    throw new ArgumentNullException(nameof(callback));
                }

                _queue.Add(new KeyValuePair<SendOrPostCallback, object>(callback, state));
            }

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("Synchronously sending is not supported.");
            }

            public void RunOnCurrentThread()
            {
                foreach (var workItem in _queue.GetConsumingEnumerable())
                {
                    workItem.Key(workItem.Value);
                }
            }

            public void Complete()
            {
                _queue.CompleteAdding();
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _queue.Dispose();

                _disposed = true;
            }
        }
    }
}