namespace Aristocrat.G2S.Emdi.Host
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Net;
    using System.Net.WebSockets;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Extensions;
    using Handlers;
    using log4net;
    using Monaco.Application.Contracts.Media;
    using Monaco.Common;
    using Protocol.v21ext1b1;
    using Serialization;

    /// <summary>
    ///     Implements <see cref="IHost"/> interface
    /// </summary>
    public class HostService : IHost
    {
        private const int ResponseTimeout = 30;
        private const int IdleTimeout = 30;

        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IMediaProvider _media;
        private readonly IMessageSerializer _serializer;
        private readonly ICommandHandlerFactory _handlers;

        private readonly Timer _pulseTimer;

        private readonly CancellationTokenSource _shutdownToken = new CancellationTokenSource();

        private readonly ManualResetEvent _idleTimeout = new ManualResetEvent(false);
        private readonly ManualResetEvent _responseTimeout = new ManualResetEvent(false);

        private readonly ActionBlock<string> _inboundQueue;
        private readonly ActionBlock<mdMsg> _outboundQueue;
        private readonly ConcurrentExclusiveSchedulerPair _scheduler = new ConcurrentExclusiveSchedulerPair();

        private readonly SemaphoreSlim _sendCommandLock = new SemaphoreSlim(1);

        private ConcurrentQueue<HttpListenerContext> _connectionRequestQueue;

        private BlockingCollection<mdMsg> _messageResponseQueue;

        private CancellationTokenSource _connectionToken = new CancellationTokenSource(0);

        private HttpListener _listener;
        private WebSocket _client;

        private Task _listenerTask;
        private Task _receiverTask;

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HostService"/> class.
        /// </summary>
        /// <param name="media"><see cref="IMediaProvider"/> instance.</param>
        /// <param name="serializer"><see cref="IMessageSerializer"/> instance.</param>
        /// <param name="handlers"><see cref="ICommandHandlerFactory"/> instance.</param>
        public HostService(
            IMediaProvider media,
            IMessageSerializer serializer,
            ICommandHandlerFactory handlers)
        {
            _media = media ?? throw new ArgumentNullException(nameof(media));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));

            _pulseTimer = new Timer(OnPulseTimerElapsed, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            _inboundQueue = new ActionBlock<string>(
                async message =>
                {
                    if (!_shutdownToken.IsCancellationRequested)
                    {
                        await HandleInboundAsync(message);
                    }
                },
                new ExecutionDataflowBlockOptions
                {
                    TaskScheduler = _scheduler.ConcurrentScheduler,
                    EnsureOrdered = false,
                    BoundedCapacity = DataflowBlockOptions.Unbounded,
                    MaxMessagesPerTask = DataflowBlockOptions.Unbounded
                });

            _inboundQueue.Completion.ContinueWith(
                t =>
                {
                    if (!t.IsFaulted)
                    {
                        return;
                    }

                    var ex = t.Exception == null ? new InvalidOperationException() : (Exception)t.Exception.Flatten();
                    Logger.Error($"EMDI: Error INBOUND message queue on port {Config.Port}: {ex.Message}", ex);
                });

            _outboundQueue = new ActionBlock<mdMsg>(
                async message =>
                {
                    if (!_shutdownToken.IsCancellationRequested)
                    {
                        await HandleOutboundAsync(message);
                    }
                }, new ExecutionDataflowBlockOptions
                {
                    TaskScheduler = _scheduler.ExclusiveScheduler,
                    EnsureOrdered = true,
                    BoundedCapacity = DataflowBlockOptions.Unbounded,
                    MaxMessagesPerTask = DataflowBlockOptions.Unbounded,
                });

            _outboundQueue.Completion.ContinueWith(
                t =>
                {
                    if (!t.IsFaulted)
                    {
                        return;
                    }

                    var ex = t.Exception == null ? new InvalidOperationException() : (Exception)t.Exception.Flatten();
                    Logger.Error($"EMDI: Error OUTBOUND message queue on port {Config.Port}: {ex.Message}", ex);
                });
        }

        /// <inheritdoc />
        ~HostService()
        {
            Dispose(false);
        }

        private bool IsConnected => _client?.State == WebSocketState.Open;

        private (HostSession Inbound, HostSession Outbound) Session { get; } = (new HostSession(), new HostSession());

        private HostConfiguration Config { get; set; }

        /// <inheritdoc />
        public async Task StartAsync(HostConfiguration config)
        {
            try
            {
                if (Config != null)
                {
                    return;
                }

                Config = config;

                StartListener();
            }
            catch (AggregateException ex)
            {
                Logger.Error($"EMDI: Error starting host on port {config.Port}: {ex.InnerException?.Message}", ex);
            }
            catch (Exception ex)
            {
                Logger.Error($"EMDI: Error starting host on port {config.Port}: {ex.Message}", ex);
            }

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task StopAsync()
        {
            _shutdownToken.Cancel();

            if (!Task.WaitAll(new[] { _listenerTask ?? Task.CompletedTask, _receiverTask ?? Task.CompletedTask }, 2000))
            {
                Logger.Error($"EMDI: Error stopping host on port {Config.Port}");
            }

            await Task.CompletedTask;
        }

        private async Task ConnectAsync()
        {
            try
            {
                if (_connectionToken != null)
                {
                    _connectionToken.Dispose();
                    _connectionToken = null;
                }

                _connectionToken = new CancellationTokenSource();

                OnConnected();
            }
            catch (AggregateException ex)
            {
                Logger.Error($"EMDI: Error starting host on port {Config.Port}: {ex.InnerException?.Message}", ex);
            }
            catch (Exception ex)
            {
                Logger.Error($"EMDI: Error occured during connect on port {Config.Port}: {ex.Message}", ex);
            }

            await Task.CompletedTask;
        }

        private async Task DisconnectAsync(WebSocketCloseStatus status)
        {
            try
            {
                _connectionToken?.Cancel();

                CloseConnection(status);
                OnDisconnected();
            }
            catch (AggregateException ex)
            {
                Logger.Error($"EMDI: Error starting host on port {Config.Port}: {ex.InnerException?.Message}", ex);
            }
            catch (Exception ex)
            {
                Logger.Error($"EMDI: Error occured during disconnect on port {Config.Port}: {ex.Message}", ex);
            }

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<TResponse> SendCommandAsync<TGroup, TCommand, TResponse>(TCommand command)
            where TGroup : c_baseClass, new()
            where TCommand : c_baseCommand
            where TResponse : c_baseCommand
        {
            if (!IsConnected)
            {
                throw new MessageException(EmdiErrorCode.NoSession, $"Host is not connected on port {Config.Port}");
            }

            if (!await _sendCommandLock.WaitAsync(TimeSpan.FromSeconds(ResponseTimeout), _connectionToken.Token))
            {
                throw new MessageException(
                    EmdiErrorCode.NoSession,
                    $"Send command already pending on port {Config.Port}, session {Session.Outbound.SessionId}");
            }

            try
            {
                var message = new mdMsg();

                message.NewFunctionalGroup<TGroup>();
                message.SetSessionId(Session.Outbound.SessionId);
                message.SetCommand(command);

                await _outboundQueue.SendAsync(message);

                if (!_messageResponseQueue.TryTake(
                    out var response,
                    (int)TimeSpan.FromSeconds(ResponseTimeout).TotalMilliseconds,
                    _connectionToken.Token))
                {
                    _responseTimeout.Set();
                    throw new MessageException(
                        EmdiErrorCode.SessionExpired,
                        $"Timed out waiting for response on port {Config.Port}, session {message.GetSessionId()}");
                }

                if (!(response.GetCommand() is TResponse))
                {
                    throw new MessageException(
                        EmdiErrorCode.InvalidXml,
                        $"Invalid response received on port {Config.Port}, session {message.GetSessionId()}");
                }

                if (response.IsError())
                {
                    throw new MessageException(
                        response.GetErrorCode(),
                        $"Error response received on port {Config.Port}, session {message.GetSessionId()}");
                }

                return response.GetCommand();
            }
            finally
            {
                _sendCommandLock.Release();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void CloseConnection(WebSocketCloseStatus status, string description = "")
        {
            try
            {
                if (!IsConnected)
                {
                    return;
                }

                _client.CloseAsync(status, description, _shutdownToken.Token).Wait();
                _client.Dispose();
                _client = null;
            }
            catch (OperationCanceledException)
            {
                // Do nothing
            }
            catch (AggregateException ex) when (TaskCancelled(ex))
            {
                // Do nothing
            }
            catch (AggregateException ex)
            {
                Logger.Error(
                    $"EMDI: Error closing connection on port {Config.Port}, status: ${status}, description: ${description}: {ex.InnerException?.Message}",
                    ex);
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"EMDI: Error closing connection on port {Config.Port}, status: ${status}, description: ${description}: {ex.Message}",
                    ex);
                throw;
            }
        }

        private async Task ListenAsync()
        {
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://+:{Config.Port}/");
                _listener.Start();

                while (_listener.IsListening && !_shutdownToken.Token.IsCancellationRequested)
                {
                    var result = _listener.BeginGetContext(ListenerCallback, _listener);
                    await result.AsyncWaitHandle.AsTask(_shutdownToken.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // Do nothing
            }
            catch (AggregateException ex) when (TaskCancelled(ex))
            {
                // Do nothing
            }
            catch (AggregateException ex)
            {
                Logger.Error($"EMDI: Error listening for request on port {Config.Port}: {ex.InnerException?.Message}", ex);
            }
            catch (Exception ex)
            {
                Logger.Error($"EMDI: Error listening for request on port {Config.Port}: {ex.Message}", ex);
            }
            finally
            {
                _listener.Close();
            }
        }

        private void ListenerCallback(IAsyncResult result)
        {
            try
            {
                var listener = (HttpListener)result.AsyncState;

                if (!listener.IsListening)
                {
                    return;
                }

                var context = listener.EndGetContext(result);

                if (_shutdownToken.Token.IsCancellationRequested)
                {
                    context.Response.StatusCode = 502;
                    context.Response.Close();
                }
                else if (context.Request.IsWebSocketRequest)
                {
                    if (_connectionRequestQueue.Any())
                    {
                        context.Response.StatusCode = 503;
                        context.Response.Close();
                        return;
                    }

                    _connectionRequestQueue.Enqueue(context);

                    StartReceiver();
                }
                else
                {
                    context.Response.StatusCode = 501;
                    context.Response.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"EMDI: Error occured in listener handler on port {Config.Port}: {ex.Message}", ex);
            }
        }

        private void StartListener()
        {
            if (_listenerTask != null)
            {
                return;
            }

            _connectionRequestQueue = new ConcurrentQueue<HttpListenerContext>();

            _listenerTask = Task.Factory.StartNew(
                    async () => await ListenAsync(),
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default)
                .Unwrap();

            _listenerTask.FireAndForget(
                ex => Logger.Error(
                    $"EMDI: Error occured in listener thread on port {Config.Port}",
                    ex));
        }

        private void StartReceiver()
        {
            if (_receiverTask != null)
            {
                return;
            }

            _messageResponseQueue = new BlockingCollection<mdMsg>();

            _receiverTask = Task.Factory.StartNew(
                    async () => await ReceiveRequestAsync(),
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default)
                .Unwrap();

            _receiverTask.FireAndForget(
                ex => Logger.Error(
                    $"EMDI: Error occured in receive thread on port {Config.Port}",
                    ex));
        }

        private async Task ReceiveRequestAsync()
        {
            while (!_shutdownToken.IsCancellationRequested)
            {
                if (!_connectionRequestQueue.TryPeek(out var context))
                {
                    await Task.Delay(1000);
                    continue;
                }

                _client = (await context.AcceptWebSocketAsync(null))?.WebSocket;

                if (_client != null)
                {
                    var status = WebSocketCloseStatus.InternalServerError;

                    try
                    {
                        await ConnectAsync();
                        _connectionToken = new CancellationTokenSource();
                        status = await ReceiveMessagesAsync();
                    }
                    finally
                    {
                        await DisconnectAsync(status);
                    }
                }

                _connectionRequestQueue.TryDequeue(out _);
            }
        }

        private async Task<WebSocketCloseStatus> ReceiveMessagesAsync()
        {
            var status = WebSocketCloseStatus.EndpointUnavailable;

            try
            {
                var xml = new StringBuilder();

                var bytes = new byte[4096];

                while (IsConnected && !_shutdownToken.IsCancellationRequested)
                {
                    Array.Clear(bytes, 0, bytes.Length);

                    var result = await ReceiveAsync(bytes);

                    if (_shutdownToken.IsCancellationRequested)
                    {
                        status = WebSocketCloseStatus.EndpointUnavailable;
                        break;
                    }

                    if (_idleTimeout.WaitOne(0))
                    {
                        Logger.Error($"EMDI: Error idle timer expired on port {Config.Port}");
                        status = WebSocketCloseStatus.InternalServerError;
                        break;
                    }

                    if (_responseTimeout.WaitOne(0))
                    {
                        Logger.Error($"EMDI: Error response timer expired on port {Config.Port}");
                        status = WebSocketCloseStatus.InternalServerError;
                        break;
                    }

                    if (result == null)
                    {
                        Logger.Error($"EMDI: Error no data received on port {Config.Port}");
                        status = WebSocketCloseStatus.InternalServerError;
                        break;
                    }

                    ResetHeartbeatTimer();

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Logger.Warn($"EMDI: Warning client requested socket closure on port {Config.Port}");
                        status = result.CloseStatus.GetValueOrDefault();
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        Logger.Error($"EMDI: Error invalid message type received on port {Config.Port}");
                        status = WebSocketCloseStatus.InvalidMessageType;
                        break;
                    }

                    var s = Encoding.UTF8.GetString(bytes);

                    s = s.TrimEnd('\0')
                        .TrimEnd('\"')
                        .TrimStart('\"')
                        .Replace(@"\n", string.Empty)
                        .Replace(@"\", string.Empty);

                    xml.Append(s);

                    if (!result.EndOfMessage)
                    {
                        continue;
                    }

                    await _inboundQueue.SendAsync(xml.ToString());

                    xml.Clear();
                }
            }
            catch (AggregateException ex)
            {
                Logger.Error($"EMDI: Error receiving data on port {Config.Port}: {ex.InnerException?.Message}", ex);
                status = WebSocketCloseStatus.InternalServerError;
            }
            catch (Exception ex)
            {
                Logger.Error($"EMDI: Error receiving data on port {Config.Port}: {ex.Message}", ex);
                status = WebSocketCloseStatus.InternalServerError;
            }

            return status;
        }

        private async Task<WebSocketReceiveResult> ReceiveAsync(byte[] bytes)
        {
            WebSocketReceiveResult result = null;

            var cts = new CancellationTokenSource();

            try
            {
                var resultTask = _client.ReceiveAsync(new ArraySegment<byte>(bytes), cts.Token);

                await Task.WhenAny(
                    resultTask,
                    _idleTimeout.AsTask(cts.Token),
                    _responseTimeout.AsTask(cts.Token),
                    _shutdownToken.Token.WaitHandle.AsTask(cts.Token));

                if (resultTask.IsCompleted)
                {
                    result = await resultTask;
                }
            }
            catch (OperationCanceledException)
            {
                // Do nothing
            }
            catch (AggregateException ex) when (TaskCancelled(ex))
            {
                // Do nothing
            }
            finally
            {
                cts.Cancel();
            }

            return result;
        }

        private async Task HandleInboundAsync(string xml)
        {
            try
            {
                var message = _serializer.Deserialize(xml);

                Logger.Debug($"EMDI: Message from Content on port {Config.Port}, session {message.GetSessionId()}: {xml.FormatXml()}");

                mdMsg response = null;

                var functionalGroup = message.Item as c_baseClass;

                if (functionalGroup == null)
                {
                    throw new InvalidOperationException("Functional Group Not Found");
                }

                if (functionalGroup.cmdType == t_cmdType.request)
                {
                    response = await RouteRequestAsync(message);
                }
                else if (functionalGroup.cmdType == t_cmdType.response)
                {
                    await ProcessResponseAsync(message);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Message command type is not supported {message.GetCommandType()} on port {Config.Port}.");
                }

                if (response != null)
                {
                    await _outboundQueue.SendAsync(response);
                }
            }
            catch (AggregateException ex)
            {
                Logger.Error($"EMDI: Error receiving message on port {Config.Port}: {ex.InnerException?.Message}", ex);
            }
            catch (Exception ex)
            {
                Logger.Error($"EMDI: Error receiving message on port {Config.Port}: {ex.Message}", ex);
            }
        }

        private async Task HandleOutboundAsync(mdMsg message)
        {
            try
            {
                var xml = _serializer.Serialize(message);

                Logger.Debug(
                    $"EMDI: Message to Content on port {Config.Port}, session {message.GetSessionId()}: {xml.FormatXml()}");

                var bytes = Encoding.UTF8.GetBytes(xml);

                await _client.SendAsync(
                    new ArraySegment<byte>(bytes, 0, bytes.Length),
                    WebSocketMessageType.Text,
                    true,
                    _shutdownToken.Token);
            }
            catch (OperationCanceledException)
            {
                // Do nothing
            }
            catch (AggregateException ex) when (TaskCancelled(ex))
            {
                // Do nothing
            }
            catch (AggregateException ex)
            {
                Logger.Error(
                    $"EMDI: Error sending message on port {Config.Port}, session {message.GetSessionId()}: {ex.InnerException?.Message}",
                    ex);
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"EMDI: Error sending message on port {Config.Port}, session {message.GetSessionId()}: {ex.Message}",
                    ex);
            }

            await Task.CompletedTask;
        }

        private async Task<mdMsg> RouteRequestAsync(mdMsg message)
        {
            try
            {
                var functionalGroup = message.Item as c_baseClass;

                if (functionalGroup?.sessionId != Session.Inbound.SessionId)
                {
                    return message.CreateErrorResponse(EmdiErrorCode.SessionOrder);
                }

                var command = ((dynamic)functionalGroup).Item as c_baseCommand;

                if (command == null)
                {
                    return message.CreateErrorResponse(EmdiErrorCode.InvalidXml);
                }

                var supportedCommands = SupportedCommands.Get();

                if (!supportedCommands.TryGetValue(functionalGroup.GetType().Name, out var commands) ||
                    !commands.Contains(command.GetType().Name))
                {
                    return message.CreateErrorResponse(EmdiErrorCode.NotAllowed);
                }

                var context = new RequestContext(Session.Inbound, Config, message);

                var result = (CommandResult)await ExecuteCommandAsync(context, message.GetCommand());

                if (result.ErrorCode != EmdiErrorCode.NoError)
                {
                    return message.CreateErrorResponse(result.ErrorCode);
                }

                return result.Command != null ? message.CreateResponse(result.Command) : null;
            }
            finally
            {
                Session.Inbound.Inc();
            }
        }

        private async Task<CommandResult> ExecuteCommandAsync<TCommand>(RequestContext context, TCommand command)
            where TCommand : c_baseCommand
        {
            var handler = _handlers.Create<TCommand>(context);

            var requiresValid = handler.GetType()
                                    .GetCustomAttributes(false)
                                    .OfType<RequiresValidSessionAttribute>()
                                    .FirstOrDefault()?.Required ?? true;

            if (requiresValid && !context.Session.IsValid)
            {
                return new CommandResult(EmdiErrorCode.NoSession);
            }

            return await handler.ExecuteAsync(command);
        }

        private Task ProcessResponseAsync(mdMsg message)
        {
            try
            {
                _messageResponseQueue.Add(message);
                return Task.CompletedTask;
            }
            finally
            {
                Session.Outbound.Inc();
            }
        }

        private void StopHeartbeatTimer()
        {
            _pulseTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        private void ResetHeartbeatTimer()
        {
            _pulseTimer.Change(TimeSpan.FromSeconds(IdleTimeout), TimeSpan.FromSeconds(IdleTimeout));
        }

        private void ResetTimeouts()
        {
            _idleTimeout.Reset();
            _responseTimeout.Reset();
        }

        private void OnConnected()
        {
            ResetHeartbeatTimer();
            ResetTimeouts();
            Session.Inbound.Reset();
            Session.Outbound.Reset();
            FireConnected();
        }

        private void OnDisconnected()
        {
            StopHeartbeatTimer();
            FireDisconnected();
        }

        private void OnPulseTimerElapsed(object state)
        {
            StopHeartbeatTimer();
            _idleTimeout.Set();
        }

        private void FireConnected()
        {
            _media.SetEmdiConnected(Config.Port, true);
        }

        private void FireDisconnected()
        {
            _media.SetEmdiConnected(Config.Port, false);
        }

        private static bool TaskCancelled(AggregateException ex)
        {
            return ex?.InnerExceptions.Any(e => e.GetType() == typeof(TaskCanceledException)) ?? false;
        }

        /// <summary>
        /// 
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
                if (_pulseTimer != null)
                {
                    _pulseTimer.Dispose();
                }

                if (_client != null)
                {
                    _client.Dispose();
                }

                if (_shutdownToken != null)
                {
                    _shutdownToken.Dispose();
                }

                if (_listener != null)
                {
                    ((IDisposable)_listener).Dispose();
                }

                if (_messageResponseQueue != null)
                {
                    _messageResponseQueue.Dispose();
                }

                if (_sendCommandLock != null)
                {
                    _sendCommandLock.Dispose();
                }

                if (_idleTimeout != null)
                {
                    _idleTimeout.Dispose();
                }

                if (_responseTimeout != null)
                {
                    _responseTimeout.Dispose();
                }

                if (_listener != null)
                {
                    _listener.Close();
                }

                if (_connectionToken != null)
                {
                    _connectionToken.Dispose();
                }

                if (_inboundQueue != null)
                {
                    _inboundQueue.Complete();
                }

                if (_outboundQueue != null)
                {
                    _outboundQueue.Complete();
                }                
            }

            _disposed = true;
        }
    }
}
