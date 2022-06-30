namespace Aristocrat.Monaco.Hhr.Client.WorkFlow
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive.Subjects;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Communication;
    using log4net;
    using Messages;
    using Polly;
    using Polly.Retry;
    using Protocol.Common.Logging;

    /// <summary>
    ///     Implementation of ICentralManager
    /// </summary>
    public class CentralManager : ICentralManager, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ILog ProtoLog = LogManager.GetLogger("Protocol");

        private readonly IMessageFlow _messageFlow;
        private readonly Subject<Response> _unsolicitedResponses = new Subject<Response>();
        private readonly List<IDisposable> _subscribersList = new List<IDisposable>();
        private readonly Dictionary<Command, Command> _validRequestResponsePair = new Dictionary<Command, Command>();
        private readonly ISequenceIdManager _sequenceIdManager;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);
        private readonly Subject<(Request, Type)> _requestSubject = new Subject<(Request, Type)>();
        private readonly Subject<(Request, Response)> _requestResponseSubject = new Subject<(Request, Response)>();
        private readonly ConcurrentDictionary<Request, CancellationTokenSource> _pendingRequests = new ConcurrentDictionary<Request, CancellationTokenSource>();

        private readonly ConcurrentDictionary<uint, TaskCompletionSource<Response>> _waitingForResponseQueue =
            new ConcurrentDictionary<uint, TaskCompletionSource<Response>>();

        private readonly List<MessageStatus> _messageStatusesToRetry = new List<MessageStatus>
        {
            MessageStatus.OtherError,
            MessageStatus.Disconnected,
            MessageStatus.PipelineError,
            MessageStatus.TimedOut,
            MessageStatus.UnableToSend,
            MessageStatus.UnexpectedResponse
        };

        private readonly List<Command> _commandsToNotCancelOnDisconnection = new List<Command>()
        {
            Command.CmdGameRecover, Command.CmdGamePlay
        };

        private ConnectionStatus _currentConnectionState;
        private bool _disposed;

        /// <summary>
        ///     Constructor that takes all the various bits and pieces that we use.
        /// </summary>
        /// <param name="tcpTransporter"></param>
        /// <param name="messageFlow"></param>
        /// <param name="sequenceIdManager"></param>
        public CentralManager(
            ITcpConnection tcpTransporter,
            IMessageFlow messageFlow,
            ISequenceIdManager sequenceIdManager)
        {
            var tcpConnection = tcpTransporter ?? throw new ArgumentNullException(nameof(tcpTransporter));
            _messageFlow = messageFlow ?? throw new ArgumentNullException(nameof(messageFlow));
            _sequenceIdManager = sequenceIdManager ?? throw new ArgumentNullException(nameof(sequenceIdManager));

            _subscribersList.Add(
                tcpConnection.IncomingBytes.Subscribe(
                    OnMessageReceived,
                    error => Logger.Error($"Error occurred while trying to receive message - {error}.")));

            _subscribersList.Add(
                tcpConnection.ConnectionStatus.Subscribe(
                    OnConnectionStateChanged,
                    error => Logger.Info($"Error occurred while trying to receive state - {error}.")));

            _validRequestResponsePair.Add(Command.CmdParameterGt, Command.CmdParameterRequest);
            _validRequestResponsePair.Add(Command.CmdGameRecoverResponse, Command.CmdGameRecover);
            _validRequestResponsePair.Add(Command.CmdRacePari, Command.CmdRacePariReq);
            _validRequestResponsePair.Add(Command.CmdCommand, Command.CmdCommand);
        }

        /// <inheritdoc />
        public async Task<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken token = default)
            where TRequest : Request where TResponse : Response
        {
            double retryInMs = 100;
            request.SequenceId = _sequenceIdManager.NextSequenceId;
            var tcs = new TaskCompletionSource<Response>();
            _waitingForResponseQueue.TryAdd(request.SequenceId, tcs);

            try
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                token = cts.Token;

                _requestSubject.OnNext((request, typeof(TResponse)));
                _pendingRequests.TryAdd(request, cts);

                var res = await Execute();

                _requestResponseSubject.OnNext((request, res));
                return res;
            }
            catch (UnexpectedResponseException ex)
            {
                _waitingForResponseQueue.TryRemove(request.SequenceId, out _);
                _requestResponseSubject.OnNext((request, ex.Response));
                throw;
            }
            catch (OperationCanceledException)
            {
                Logger.Warn("[SEND] Operation cancelled when trying to send message");
                _waitingForResponseQueue.TryRemove(request.SequenceId, out _);
                var unexpectedResponseException = UnexpectedResponseException(MessageStatus.Cancelled);
                _requestResponseSubject.OnNext((request, unexpectedResponseException.Response));
                throw unexpectedResponseException;
            }
            finally
            {
                _pendingRequests.TryRemove(request, out _);
            }

            AsyncRetryPolicy<TResponse> CreatePolicy()
            {
                return Policy<TResponse>
                    .HandleInner<UnexpectedResponseException>(ex => ShouldRetry(ex.Response))
                    .WaitAndRetryAsync(
                        request.RetryCount,
                        (_, _) => TimeSpan.FromMilliseconds(retryInMs),
                        (_, _, _) =>
                        {
                            request.SequenceId = _sequenceIdManager.NextSequenceId;
                            tcs = new TaskCompletionSource<Response>();
                            _waitingForResponseQueue.TryAdd(request.SequenceId, tcs);
                            Logger.Debug($"[SEND] Retrying again to send [{request}]");
                        });
            }

            async Task<TResponse> Execute()
            {
                return await CreatePolicy().ExecuteAsync(
                    async cancellationToken =>
                    {
                        if (_currentConnectionState.ConnectionState == ConnectionState.Disconnected)
                        {
                            Logger.Warn("[SEND] Not connected when trying to send message");
                            throw UnexpectedResponseException(MessageStatus.Disconnected);
                        }

                        bool result;
                        try
                        {
                            await _lock.WaitAsync(cancellationToken);
                            RequestModifiedHandler?.Invoke(this, request);

                            result = await _messageFlow.Send(request, cancellationToken);
                        }
                        catch (OperationCanceledException e)
                        {
                            Logger.Error("[SEND] Request cancelled during processing", e);
                            throw UnexpectedResponseException(MessageStatus.Cancelled);
                        }
                        catch (Exception e)
                        {
                            Logger.Error("[SEND] Unable to send request through pipeline ", e);
                            throw UnexpectedResponseException(MessageStatus.PipelineError);
                        }
                        finally
                        {
                            _lock.Release();
                        }

                        if (!result)
                        {
                            Logger.Warn($"[SEND] Unable to send [{request}]");
                            throw UnexpectedResponseException(MessageStatus.UnableToSend);
                        }

                        ProtoLog.Debug($"[SEND] Sent [{request.ToJson()}]");

                        // Response expected?
                        if (typeof(TResponse) == typeof(Response))
                        {
                            throw UnexpectedResponseException(MessageStatus.NoResponse);
                        }

                        var res = await WaitForResponse(request, tcs);

                        if (res is CloseTranErrorResponse closeTranErrorResponse)
                        {
                            Logger.Warn($"[RECV] Received close tran error Response [{closeTranErrorResponse}]");
                        }

                        if (!(res is TResponse expectedResponse))
                        {
                            Logger.Warn($"[RECV] Unexpected response received for Request [{request}] - res [{res?.MessageData()}");
                            throw UnexpectedResponseException(MessageStatus.UnexpectedResponse, res);
                        }

                        ProtoLog.Debug($"[RECV] Received [{res.MessageData()}]");

                        return expectedResponse;
                    }, token);
            }

            UnexpectedResponseException UnexpectedResponseException(MessageStatus status, Response response = null)
            {
                if (response != null && response.GetType() != typeof(Response))
                {
                    response.MessageStatus = status;
                }

                return new UnexpectedResponseException(response ?? PopulateResponse(request, status));
            }

            bool ShouldRetry(Response response)
            {
                if (response is CloseTranErrorResponse errorResponse && errorResponse.Status == Status.Retry)
                {
                    retryInMs = errorResponse.RetryTime.TotalMilliseconds;
                    return true;
                }

                return _messageStatusesToRetry.Contains(response.MessageStatus);
            }
        }

        /// <inheritdoc />
        public async Task Send<TRequest>(TRequest request, CancellationToken token = default) where TRequest : Request
        {
            try
            {
                await Send<TRequest, Response>(request, token);
            }
            catch (UnexpectedResponseException e)
            {
                if (e.Response.MessageStatus != MessageStatus.NoResponse)
                {
                    throw;
                }
            }
        }

        /// <inheritdoc />
        public IObservable<Response> UnsolicitedResponses => _unsolicitedResponses;

        /// <inheritdoc />
        public IObservable<(Request, Type)> RequestObservable => _requestSubject;

        /// <inheritdoc />
        public IObservable<(Request, Response)> RequestResponseObservable => _requestResponseSubject;

        /// <inheritdoc />
        public event EventHandler<Request> RequestModifiedHandler;

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Dispose Pattern to prevent reentry
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var disposable in _subscribersList)
                {
                    disposable.Dispose();
                }

                _requestSubject.Dispose();
                _lock.Dispose();
                _requestResponseSubject.Dispose();
                _unsolicitedResponses.Dispose();
            }

            _disposed = true;
        }

        private static Response PopulateResponse(Request request, MessageStatus status)
        {
            return new Response(request.Command) { MessageStatus = status, ReplyId = request.SequenceId };
        }

        private void OnConnectionStateChanged(ConnectionStatus status)
        {
            if (status != _currentConnectionState)
            {
                Logger.Debug($"[CONN] Connection state change - [{_currentConnectionState?.ConnectionState ?? ConnectionState.Disconnected} -> {status?.ConnectionState}]");
            }

            _currentConnectionState = status;

            if (status.ConnectionState != ConnectionState.Disconnected)
            {
                return;
            }

            foreach (var pendingRequest in _pendingRequests.Where(x => !_commandsToNotCancelOnDisconnection.Contains(x.Key.Command)))
            {
                pendingRequest.Value.Cancel();
            }
        }

        private async void OnMessageReceived(Packet message)
        {
            if (message == null)
            {
                return;
            }

            try
            {
                var response = await _messageFlow.Receive(message);
                ProtoLog.Debug($"[RECV] Bytes={message.Length}, {response}");

                if (response == null)
                {
                    return;
                }

                MatchResponseWithRequest(response);
            }
            catch (InvalidDataException exception)
            {
                Logger.Error($"Invalid command received from the server : {exception}");
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to parse packet from MessageFlow : {e}");
            }

            void MatchResponseWithRequest(Response response)
            {
                if (response.ReplyId == 0)
                {
                    ResponseWithZeroReplyId(response);
                }
                else
                {
                    ResponseWithReplyId(response);
                }
            }

            void ResponseWithZeroReplyId(Response response)
            {
                // There are some responses sent from server with Reply Id as 0, we picks from the queue whosoever is waiting for this response.
                var sequenceId = _pendingRequests
                    .Select(x => x.Key)
                    .FirstOrDefault(x => x.Command == _validRequestResponsePair[response.Command])?.SequenceId;
                var r = _waitingForResponseQueue
                    .Select(x => (KeyValuePair<uint, TaskCompletionSource<Response>>?)x)
                    .FirstOrDefault(x => x.Value.Key == sequenceId);

                if (r == null)
                {
                    UnsolicitedResponse(response);
                }
                else
                {
                    SetResult(r.Value.Value, response);
                }
            }

            void ResponseWithReplyId(Response response)
            {
                if (_waitingForResponseQueue.TryGetValue(response.ReplyId, out var requestTask))
                {
                    SetResult(requestTask, response);
                }
                else
                {
                    UnsolicitedResponse(response);
                }
            }

            void UnsolicitedResponse(Response response)
            {
                Logger.Warn($"[RECV] Received an Unsolicited response from Server {response}");
                _unsolicitedResponses.OnNext(response);
            }

            void SetResult(TaskCompletionSource<Response> tcs, Response response)
            {
                var success = tcs.TrySetResult(response);
                Logger.Debug(
                    success
                        ? $"Successfully marked Response {response} for Request with SequenceId - {response.ReplyId}"
                        : $"Unable to set Response {response} since the Request is already Complete or Timeout");
            }
        }

        private async Task<Response> WaitForResponse(Request request, TaskCompletionSource<Response> tcs)
        {
            using (var cts = new CancellationTokenSource())
            {
                cts.Token.Register(
                    () =>
                    {
                        Logger.Debug(
                            !tcs.TrySetResult(PopulateResponse(request, MessageStatus.TimedOut))
                                ? "Unable to Set timeout results as Task is already completed."
                                : $"Request {request} TimedOut.");
                    });

                cts.CancelAfter(TimeSpan.FromMilliseconds(request.TimeoutInMilliseconds));

                var response = await tcs.Task;

                _waitingForResponseQueue.TryRemove(request.SequenceId, out _);
                return response;
            }
        }
    }
}