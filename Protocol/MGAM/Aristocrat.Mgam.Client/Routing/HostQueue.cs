namespace Aristocrat.Mgam.Client.Routing
{
    using Attribute;
    using Common;
    using Logging;
    using Messaging;
    using Messaging.Translators;
    using Monaco.Common;
    using Options;
    using Protocol;
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    /// <summary>
    ///     Queues messages to be sent to the host.
    /// </summary>
    internal sealed class HostQueue : IHostQueue, IStartable, IDisposable
    {
        private const int SendTimeout = 5;
        private const int ReadIntervalTimeout = 100;

        private readonly ILogger _logger;
        private readonly IOptionsMonitor<ProtocolOptions> _options;
        private readonly IAttributeCache _attributes;
        private readonly IMessageTranslatorFactory _translators;
        private readonly IXmlMessageSerializer _serializer;
        private readonly ICompressor _compressor;
        private readonly ISecureTransporter _transporter;
        private readonly IBroadcastTransporter _broadcaster;
        private readonly IRequestHandler _requestHandler;
        private readonly ITransportPublisher _publisher;

        private readonly SafeCounter _counter = new();

        private readonly ActionBlock<Msg<Payload>> _receiveQueue;

        private readonly IObservable<EventPattern<ResponseEventArgs>> _broadcastResponses;
        private readonly Channel<IResponse> _responseQueue = Channel.CreateUnbounded<IResponse>();

        private SemaphoreSlim _requestLock = new(1);

        private SubscriptionList _subscriptions = new();

        private CancellationTokenSource _shutdown = new();

        private ManualResetEvent _disconnected = new(true);

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HostQueue"/> class.
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/>.</param>
        /// <param name="options"><see cref="IOptionsMonitor{TOptions}"/>.</param>
        /// <param name="attributes"><see cref="IAttributeCache"/>.</param>
        /// <param name="translators"><see cref="IMessageTranslatorFactory"/>.</param>
        /// <param name="serializer"><see cref="IXmlMessageSerializer"/>.</param>
        /// <param name="compressor"><see cref="ICompressor"/>.</param>
        /// <param name="transporter"><see cref="ISecureTransporter"/></param>
        /// <param name="broadcaster"><see cref="IBroadcastTransporter"/>.</param>
        /// <param name="requestHandler"><see cref="IRequestHandler"/>.</param>
        /// <param name="publisher"><see cref="ITransportPublisher"/>.</param>
        public HostQueue(
            ILogger<HostQueue> logger,
            IOptionsMonitor<ProtocolOptions> options,
            IAttributeCache attributes,
            IMessageTranslatorFactory translators,
            IXmlMessageSerializer serializer,
            ICompressor compressor,
            ISecureTransporter transporter,
            IBroadcastTransporter broadcaster,
            IRequestHandler requestHandler,
            ITransportPublisher publisher)
        {
            _logger = logger;
            _options = options;
            _attributes = attributes;
            _translators = translators;
            _serializer = serializer;
            _compressor = compressor;
            _transporter = transporter;
            _broadcaster = broadcaster;
            _requestHandler = requestHandler;
            _publisher = publisher;

            _broadcastResponses = Observable.FromEventPattern<ResponseEventArgs>(
                h => ResponseReceived += h,
                h => ResponseReceived -= h);

            _receiveQueue = new ActionBlock<Msg<Payload>>(
                async payload =>
                {
                    if (!_shutdown.IsCancellationRequested)
                    {
                        await Receive(payload);
                    }
                },
                new ExecutionDataflowBlockOptions
                {
                    TaskScheduler = TaskScheduler.Default,
                    EnsureOrdered = false,
                    BoundedCapacity = DataflowBlockOptions.Unbounded,
                    MaxMessagesPerTask = DataflowBlockOptions.Unbounded,
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                });

            _receiveQueue.Completion.ContinueWith(
                t =>
                {
                    if (!t.IsFaulted)
                    {
                        return;
                    }

                    var exception = t.Exception == null
                        ? new InvalidOperationException()
                        : (Exception)t.Exception.Flatten();
                    _logger.LogError(exception, "Incoming message handling faulted");
                });

            _subscriptions.Add(
                _broadcaster.Messages.Subscribe(
                    OnResponseReceived,
                    error => _logger.LogError(error, "Broadcast response failure")),
                _transporter.Messages.Subscribe(
                    OnResponseReceived,
                    error => _logger.LogError(error, "Point-to-point response failure")),
                _publisher.Subscribe(Observer.Create<TransportStatus>(OnTransportStatusChanged)));
        }

        /// <inheritdoc />
        ~HostQueue()
        {
            Dispose(false);
        }

        private event EventHandler<ResponseEventArgs> ResponseReceived;

        /// <inheritdoc />
        public async Task<IDisposable> Broadcast<TRequest, TResponse>(
            TRequest request,
            Action<TResponse> subscriber,
            CancellationToken cancellationToken)
            where TRequest : class, IRequest
            where TResponse : class, IResponse
        {
            if (_shutdown.IsCancellationRequested)
            {
                throw new InvalidOperationException("Shutting down");
            }

            _logger.LogDebug(
                $"Broadcasting {request.GetType().Name} request");

            var msg = Msg<IMessage>.Create(
                _counter.Next(),
                MessageChannel.Broadcast,
                MessageType.Request,
                request);

            await Send(msg, cancellationToken);

            return _broadcastResponses
                .Select(e => e.EventArgs.Response)
                .OfType<TResponse>()
                .Subscribe(Observer.Create(subscriber));
        }

        /// <inheritdoc />
        public async Task<MessageResult<TResponse>> Send<TRequest, TResponse>(
            TRequest request,
            CancellationToken cancellationToken)
            where TRequest : class, IRequest
            where TResponse : class, IResponse
        {
            if (_shutdown.IsCancellationRequested || _disconnected.WaitOne(TimeSpan.Zero))
            {
                return MessageResult<TResponse>.Create(MessageStatus.CommsLost);
            }

            using var lts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                _shutdown.Token);

            if (request is Messaging.KeepAlive)
            {
                return await SendRequest<TResponse>(request, cancellationToken);
            }

            if (!await _requestLock.WaitAsync(Timeout.InfiniteTimeSpan, lts.Token))
            {
                _logger.LogError($"Timeout waiting to send request {request.GetType().Name}");

                return MessageResult<TResponse>.Create(MessageStatus.SendError);
            }

            try
            {
                if (_disconnected.WaitOne(TimeSpan.Zero))
                {
                    return MessageResult<TResponse>.Create(MessageStatus.CommsLost);
                }

                return await SendRequest<TResponse>(request, cancellationToken);
            }
            finally
            {
                _requestLock.Release();
            }
        }

        /// <inheritdoc />
        public Task Start()
        {
            _logger.LogInfo($"Starting {GetType().Name}...");

            _logger.LogInfo($"Started {GetType().Name}...");

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public bool CanStart() => true;

        /// <inheritdoc />
        public Task Stop()
        {
            _logger.LogInfo($"Stopping {GetType().Name}...");

            _shutdown.Cancel();

            _responseQueue.Writer.Complete();
            _receiveQueue.Complete();

            Task.WaitAll(_responseQueue.Reader.Completion, _receiveQueue.Completion);

            _logger.LogInfo($"Stopped {GetType().Name}...");

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // ReSharper disable once UseNullPropagation
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_subscriptions != null)
                {
                    _subscriptions.Dispose();
                }

                if (_requestLock != null)
                {
                    _requestLock.Dispose();
                }

                if (_disconnected != null)
                {
                    _disconnected.Dispose();
                }

                if (_shutdown != null)
                {
                    _shutdown.Dispose();
                }
            }

            _subscriptions = null;
            _requestLock = null;
            _disconnected = null;
            _shutdown = null;

            _disposed = true;
        }

        private async Task<MessageResult<TResponse>> SendRequest<TResponse>(
            IMessage message,
            CancellationToken cancellationToken)
            where TResponse : class, IResponse
        {
            _logger.LogDebug($"Sending request {message.GetType().Name}");

            var msg = Msg<IMessage>.Create(
                _counter.Next(),
                MessageChannel.PointToPoint,
                MessageType.Request,
                message);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(SendTimeout));
            using var lts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                cts.Token);

            try
            {
                await Send(msg, lts.Token);

                _logger.LogDebug($"Sent request {msg}");
            }
            catch (AggregateException ex) when (Cancelled(ex))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogError(
                        ex.Flatten().InnerException,
                        $"Aborted sending request {msg}");
                    return MessageResult<TResponse>.Create(MessageStatus.Aborted);
                }

                _logger.LogError(
                    ex.Flatten().InnerException,
                    $"Timeout sending request {msg}, Timeout: {SendTimeout}");

                return MessageResult<TResponse>.Create(MessageStatus.TimedOut);
            }
            catch (OperationCanceledException ex)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogError(
                        ex,
                        $"Aborted sending request {msg}");
                    return MessageResult<TResponse>.Create(MessageStatus.Aborted);
                }

                _logger.LogError(
                    ex,
                    $"Timeout sending request {msg}, Timeout: {SendTimeout}");

                return MessageResult<TResponse>.Create(MessageStatus.TimedOut);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sending request {msg} failure");

                return MessageResult<TResponse>.Create(MessageStatus.SendError);
            }

            // Don't wait for keep-alive response
            if (typeof(TResponse) == typeof(Messaging.KeepAliveResponse))
            {
                return MessageResult<Messaging.KeepAliveResponse>.Create(
                        MessageStatus.Success,
                        new Messaging.KeepAliveResponse { ResponseCode = ServerResponseCode.Ok }) as
                    MessageResult<TResponse>;
            }

            return await WaitForResponse<TResponse>(msg, cancellationToken);
        }

        private async Task<MessageResult<TResponse>> WaitForResponse<TResponse>(
            Msg<IMessage> msg,
            CancellationToken cancellationToken)
            where TResponse : class, IResponse
        {
            _logger.LogDebug($"Waiting for {typeof(TResponse).Name} response for message {msg}");

            var connectionTimeout = (int)_attributes[AttributeNames.ConnectionTimeout];

            MessageResult<TResponse> result;

            using var cts = new CancellationTokenSource();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(connectionTimeout));
            using var lts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                timeout.Token,
                cts.Token);

            try
            {
                var responseTask = GetResponse<TResponse>(lts.Token);

                await Task.WhenAny(responseTask, _disconnected.AsTask(cts.Token));

                if (responseTask.IsCompleted)
                {
                    var response = responseTask.Result;

                    _logger.LogDebug(
                        $"Received {response.GetType().Name} response for message {msg}");

                    result = MessageResult<TResponse>.Create(MessageStatus.Success, response);
                }
                else
                {
                    _logger.LogDebug(
                        $"Connection lost waiting for {typeof(TResponse).Name} response for message {msg}");

                    result = MessageResult<TResponse>.Create(MessageStatus.CommsLost);
                }
            }
            catch (AggregateException ex) when (Cancelled(ex))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogError(
                        ex.Flatten().InnerException,
                        $"Aborted waiting for {typeof(TResponse).Name} response for message {msg}");
                    result = MessageResult<TResponse>.Create(MessageStatus.Aborted);
                }
                else
                {
                    _logger.LogError(
                        ex.Flatten().InnerException,
                        $"Timeout waiting for {typeof(TResponse).Name} response for message {msg}, Timeout: {timeout}");
                    result = MessageResult<TResponse>.Create(MessageStatus.TimedOut);
                }
            }
            catch (OperationCanceledException ex)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogError(
                        ex,
                        $"Aborted waiting for {typeof(TResponse).Name} response for message {msg}");
                    result = MessageResult<TResponse>.Create(MessageStatus.Aborted);
                }
                else
                {
                    _logger.LogError(
                        ex,
                        $"Timeout waiting for {typeof(TResponse).Name} response for message {msg}, Timeout: {timeout}");
                    result = MessageResult<TResponse>.Create(MessageStatus.TimedOut);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"Response error Waiting for {typeof(TResponse).Name} response for message {msg}");

                result = MessageResult<TResponse>.Create(MessageStatus.ResponseError);
            }
            finally
            {
                if (!cts.IsCancellationRequested)
                {
                    cts.Cancel();
                }
            }

            if (result.Status == MessageStatus.TimedOut)
            {
                // Don't allow any messages to be queued until reconnection
                _disconnected.Set();

                _publisher.Publish(
                    new TransportStatus(
                        TransportState.Up,
                        false,
                        _transporter?.EndPoint,
                        ConnectionState.Unchanged,
                        TransportFailure.Timeout));
            }

            return result;
        }

        private async Task<TResponse> GetResponse<TResponse>(CancellationToken cancellationToken)
            where TResponse : class, IResponse
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_responseQueue.Reader.TryRead(out var r))
                {
                    if (r is TResponse response)
                    {
                        return response;
                    }

                    _logger.LogWarn(
                        $"Incorrect response received, waiting for {typeof(TResponse).Name} received {r?.GetType().Name}");
                    // Discard any response that is not expected
                }

                await Task.Delay(TimeSpan.FromMilliseconds(ReadIntervalTimeout), CancellationToken.None);
            }

            throw new OperationCanceledException("Receive response cancelled");
        }

        private async Task Send(Msg<IMessage> msg, CancellationToken cancellationToken)
        {
            try
            {
                var message = msg.Value;

                var bytes = SerializeMessage(message, msg.MessageId, out var xml);
                if (bytes == null)
                {
                    return;
                }

                var threshold = (int)_attributes[AttributeNames.MessageCompressionThreshold];

                Payload payload;

                if (bytes.Length >= threshold)
                {
                    payload = await CompressAndPackMessage(bytes, msg.MessageId);
                }
                else
                {
                    payload = PackMessage(bytes, msg.MessageId);
                }

                var address = (msg.IsBroadcast ? _broadcaster.EndPoint : _transporter.EndPoint)?.GetAddress();

                _publisher.Publish(
                    _options.CurrentValue.NetworkAddress.GetAddress(),
                    address,
                    message,
                    xml);

                _logger.LogDebug($"Sending message\n{xml}");

                if (msg.IsBroadcast)
                {
                    await BroadcastMessage(payload, msg.MessageId, cancellationToken);
                }
                else
                {
                    await SendMessage(payload, msg.MessageId, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Send {msg} message failure");
            }
        }

        private async Task Receive(Msg<Payload> msg)
        {
            try
            {
                string xml;

                if (msg.Value.Format == XmlDataSetFormat.XmlCompressedDataSet)
                {
                    xml = await UnpackAndDecompressMessage(msg.Value);
                }
                else
                {
                    xml = UnpackMessage(msg.Value);
                }

                _logger.LogDebug($"Received message\n{xml}");

                var message = DeserializeMessage(xml, out var messageId);

                var address = (msg.IsBroadcast ? _broadcaster.EndPoint : _transporter.EndPoint)?.GetAddress();

                _publisher.Publish(
                    address,
                    _options.CurrentValue.NetworkAddress.GetAddress(),
                    message,
                    xml);

                if (message is IRequest)
                {
                    await HandleRequest(message, messageId);
                }
                else
                {
                    await HandleResponse(message, messageId, msg.IsBroadcast);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Receive {msg} message failure");
            }
        }

        private void OnResponseReceived(Payload? payload)
        {
            if (_shutdown.IsCancellationRequested)
            {
                return;
            }

            if (!payload.HasValue)
            {
                return;
            }

            if (!payload.Value.IsBroadcast)
            {
                _receiveQueue.Post(
                    Msg<Payload>.Create(
                        MessageChannel.PointToPoint,
                        MessageType.Response,
                        payload.Value));
            }
            else
            {
                _receiveQueue.Post(
                    Msg<Payload>.Create(
                        MessageChannel.Broadcast,
                        MessageType.Response,
                        payload.Value));
            }
        }

        private byte[] SerializeMessage(IMessage message, int messageId, out string xml)
        {
            xml = null;

            try
            {
                _logger.LogDebug(
                    $"Translating message {message.GetType().Name}, MessageId: {messageId}");

                var translator = _translators.Create(message.GetType());

                var xmlMessage = (XmlMessage)translator.Translate(message);

                if (xmlMessage == null)
                {
                    return null;
                }

                xmlMessage.Id = messageId;
                // xmlMessage.IdSpecified = true;

                xml = _serializer.Serialize(xmlMessage);

                return Encoding.UTF8.GetBytes(xml);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Serializing message failure");
                throw;
            }
        }

        private IMessage DeserializeMessage(string xml, out int messageId)
        {
            try
            {
                if (!_serializer.TryDeserialize(xml, out var xmlMessage))
                {
                    _publisher.Publish(
                        new TransportStatus(
                            TransportState.Up,
                            false,
                            _transporter?.EndPoint,
                            ConnectionState.Unchanged,
                            TransportFailure.Malformed));

                    throw new InvalidOperationException("Error Deserializing message");
                }

                messageId = xmlMessage.Id;

                var translator = _translators.Create(xmlMessage.GetType());

                return (IMessage)translator.Translate(xmlMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Deserializing message failure");
                throw;
            }
        }

        private async Task<Payload> CompressAndPackMessage(byte[] bytes, int messageId)
        {
            try
            {
                _logger.LogDebug(
                    $"Compressing and packaging message, MessageId: {messageId}, of size {bytes.Length}");

                var compressedBytes = await _compressor.Compress(bytes);

                var header =
                    Encoding.UTF8.GetBytes(
                        $"MGAM Payload: {ProtocolConstants.XmlCompressedDataSet}{Environment.NewLine}MGAM Size: {compressedBytes.Length}{Environment.NewLine}");

                var content = header.Concat(compressedBytes).ToArray();

                return new Payload
                {
                    Format = XmlDataSetFormat.XmlCompressedDataSet,
                    MessageSize = compressedBytes.Length,
                    Content = content
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Compressing and packaging message failure");
                throw;
            }
        }

        private Payload PackMessage(byte[] bytes, int messageId)
        {
            try
            {
                _logger.LogDebug(
                    $"Packaging message, MessageId: {messageId}, of size {bytes.Length}");

                var header =
                    Encoding.UTF8.GetBytes(
                        $"MGAM Payload: {ProtocolConstants.XmlDataSet}{Environment.NewLine}MGAM Size: {bytes.Length}{Environment.NewLine}");

                return new Payload
                {
                    Format = XmlDataSetFormat.XmlDataSet,
                    MessageSize = bytes.Length,
                    Content = header.Concat(bytes).ToArray()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Packaging message failure");
                throw;
            }
        }

        private async Task<string> UnpackAndDecompressMessage(Payload payload)
        {
            try
            {
                _logger.LogDebug(
                    $"Unpacking and decompressing of size {payload.MessageSize}");

                var bytes = await _compressor.Decompress(payload.Content);

                return Encoding.UTF8.GetString(bytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unpacking and decompressing message failure");
                throw;
            }
        }

        private string UnpackMessage(Payload payload)
        {
            try
            {
                _logger.LogDebug(
                    $"Unpacking message of size {payload.MessageSize}");

                return Encoding.UTF8.GetString(payload.Content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unpacking message failure");
                throw;
            }
        }

        private async Task BroadcastMessage(Payload payload, int messageId, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug(
                    $"Broadcasting message, MessageId {messageId}, of size {payload.MessageSize}");

                await _broadcaster.Broadcast(payload, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Broadcasting message failure");
                throw;
            }
        }

        private async Task SendMessage(Payload payload, int messageId, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug(
                    $"Sending message, MessageId {messageId}, of size {payload.MessageSize}");

                await _transporter.Send(payload, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sending message failure");
            }
        }

        private async Task HandleRequest(IMessage message, int messageId)
        {
            try
            {
                _logger.LogDebug(
                    $"Received {message.GetType().Name}, MessageId: {messageId}");

                if (!(message is IRequest request))
                {
                    throw new InvalidOperationException(
                        $"{message.GetType().Name} is not a request message or command");
                }

                var response = await _requestHandler.Receive(request);

                var msg = Msg<IMessage>.Create(
                    messageId,
                    MessageChannel.PointToPoint,
                    MessageType.Response,
                    response);

                await Send(msg, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Handling request message failure");
                throw;
            }
        }

        private async Task HandleResponse(IMessage message, int messageId, bool broadcast)
        {
            try
            {
                _logger.LogDebug(
                    $"Handling response {message.GetType().Name}, MessageId: {messageId}");

                if (!(message is IResponse response))
                {
                    throw new InvalidOperationException($"{message.GetType().Name} is not a response");
                }

                if (broadcast)
                {
                    RaiseBroadcastResponseReceived(response);
                }
                else if (response is Messaging.KeepAliveResponse keepAlive)
                {
                    switch (keepAlive.ResponseCode)
                    {
                        case ServerResponseCode.InvalidInstanceId:
                        case ServerResponseCode.VltServiceNotRegistered:
                        case ServerResponseCode.ServerError:
                            _publisher.Publish(
                                new TransportStatus(
                                    TransportState.Up,
                                    false,
                                    _transporter?.EndPoint,
                                    ConnectionState.Unchanged,
                                    TransportFailure.InvalidServerResponse));
                            break;
                    }
                }
                else
                {
                    await _responseQueue.Writer.WriteAsync(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Handling response message failure");
                throw;
            }
        }

        private void OnTransportStatusChanged(TransportStatus status)
        {
            if (status.IsBroadcast)
            {
                return;
            }

            switch (status.ConnectionState)
            {
                case ConnectionState.Connected:
                    _disconnected.Reset();
                    break;

                case ConnectionState.Lost:
                    _disconnected.Set();
                    while (_responseQueue.Reader.TryRead(out _)){}
                    break;
            }
        }

        private void RaiseBroadcastResponseReceived(IResponse response)
        {
            ResponseReceived?.Invoke(this, new ResponseEventArgs(response));
        }

        private static bool Cancelled(AggregateException ex)
        {
            return ex?.InnerExceptions.Any(
                e => e.GetType() == typeof(TaskCanceledException) ||
                     e.GetType() == typeof(OperationCanceledException)) ?? false;
        }

        private enum MessageChannel
        {
            Broadcast = 1,
            PointToPoint = 2
        }

        private enum MessageType
        {
            Request = 1,
            Response = 2
        }

        private class Msg<TValue>
        {
            private Msg(
                int id,
                MessageChannel channel,
                MessageType msgType,
                TValue value)
            {
                MessageId = id;
                MessageName = value?.GetType().Name;
                Channel = channel;
                MessageType = msgType;
                Value = value;
            }

            private string MessageName { get; }

            private MessageChannel Channel { get; }

            private MessageType MessageType { get; }

            public int MessageId { get; }

            public TValue Value { get; }

            public bool IsBroadcast => Channel == MessageChannel.Broadcast;

            public static Msg<TValue> Create(
                MessageChannel channel,
                MessageType type,
                TValue value)
            {
                return new(0, channel, type, value);
            }

            public static Msg<TValue> Create(
                int id,
                MessageChannel channel,
                MessageType type,
                TValue value)
            {
                return new(id, channel, type, value);
            }

            public override string ToString()
            {
                return $"[Msg: [MessageId={MessageId}, MessageName={MessageName}, MessageType={MessageType}, Channel={Channel}]]";
            }
        }
    }
}
