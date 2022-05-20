namespace Aristocrat.G2S.Client
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Communications;
    using Diagnostics;
    using Protocol.v21;

    /// <summary>
    ///     HostQueue class
    /// </summary>
    internal class HostQueue : IHostQueue, ISessionSink, IDisposable
    {
        private readonly CommandQueue _commandQueue = new CommandQueue();
        private readonly ICommandDispatcher _dispatcher;
        private readonly IEgm _egm;
        private readonly ISendEndpointProvider _endpointProvider;
        private readonly int _hostId;
        private readonly IIdProvider<int> _idProvider;
        private bool _disposed;

        private ISessionManager _sessionManager = new SessionManager();

        /// <summary>
        ///     Initializes a new instance of the <see cref="HostQueue" /> class.
        /// </summary>
        /// <param name="hostId">The host identifier</param>
        /// <param name="egm">The egm</param>
        /// <param name="endpointProvider">The endpoint provider</param>
        /// <param name="dispatcher">The dispatcher</param>
        /// <param name="idProvider">The id provider</param>
        public HostQueue(
            int hostId,
            IEgm egm,
            ISendEndpointProvider endpointProvider,
            ICommandDispatcher dispatcher,
            IIdProvider<int> idProvider)
        {
            _hostId = hostId;
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _endpointProvider = endpointProvider ?? throw new ArgumentNullException(nameof(endpointProvider));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));

            SessionTimeout = DefaultSessionTimeout;
            RetryCount = Constants.DefaultRetryCount;
            TimeToLiveBehavior = Constants.DefaultTimeToLiveBehavior;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public event EventHandler<MessageHandledEventArgs> MessageSent;

        /// <inheritdoc />
        public event EventHandler<MessageHandledEventArgs> MessageReceived;

        /// <inheritdoc />
        public int RetryCount { get; internal set; }

        /// <inheritdoc />
        public TimeToLiveBehavior TimeToLiveBehavior { get; internal set; }

        /// <inheritdoc />
        public TimeSpan SessionTimeout { get; set; }

        /// <inheritdoc />
        public bool Online => _commandQueue.Online;

        /// <inheritdoc />
        public bool CanSend => _commandQueue.AllowSend;

        /// <inheritdoc />
        public int ReceivedCount => _commandQueue.ReceivedCount;

        /// <inheritdoc />
        public int SendCount => _commandQueue.SendCount;

        /// <inheritdoc />
        public TimeSpan ReceivedElapsedTime =>
            TimeSpan.FromSeconds(
                (Stopwatch.GetTimestamp() - _commandQueue.ReceivedTimeStamp) * (1.0 / Stopwatch.Frequency));

        /// <inheritdoc />
        public TimeSpan SentElapsedTime =>
            TimeSpan.FromSeconds(
                (Stopwatch.GetTimestamp() - _commandQueue.SentTimeStamp) * (1.0 / Stopwatch.Frequency));

        /// <inheritdoc />
        public bool OutboundQueueFull => _commandQueue.SendCount >= CommandQueue.MaxQueueSize;

        /// <inheritdoc />
        public bool InboundQueueFull => _commandQueue.ReceivedCount >= CommandQueue.MaxQueueSize;

        /// <inheritdoc />
        public void SendNotification(IClass notification)
        {
            SendNotification(notification, DefaultSessionTimeout);
        }

        /// <inheritdoc />
        public void SendNotification(IClass notification, TimeSpan sessionTimeout)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            // Override the session type here, since it will be a request by default
            notification.sessionType = SessionType.Notification;
            notification.timeToLive = (int)sessionTimeout.TotalMilliseconds;

            InternalQueueCommand(ClassCommand.Create(notification, _hostId, _egm.Id), false);
        }

        /// <inheritdoc />
        public Session SendRequest(IClass request)
        {
            return SendRequest(request, null);
        }

        /// <inheritdoc />
        public Session SendRequest(IClass request, TimeSpan sessionTimeout)
        {
            return SendRequest(request, null, RetryCount, sessionTimeout);
        }

        /// <inheritdoc />
        public Session SendRequest(IClass request, bool alwaysSend)
        {
            return SendRequest(request, RetryCount, alwaysSend);
        }

        /// <inheritdoc />
        public Session SendRequest(IClass request, int retryCount, bool alwaysSend)
        {
            return InternalSendRequest(request, null, retryCount, SessionTimeout, alwaysSend);
        }

        /// <inheritdoc />
        public Session SendRequest(IClass request, SessionCallback callback)
        {
            return SendRequest(request, callback, RetryCount);
        }

        /// <inheritdoc />
        public Session SendRequest(IClass request, SessionCallback callback, int retryCount)
        {
            return SendRequest(request, callback, retryCount, SessionTimeout);
        }

        /// <inheritdoc />
        public Session SendRequest(IClass request, SessionCallback callback, int retryCount, TimeSpan sessionTimeout)
        {
            return InternalSendRequest(request, callback, retryCount, sessionTimeout, false);
        }

        /// <inheritdoc />
        public void SendResponse(ClassCommand response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (response.Error.IsError)
            {
                response.GenerateError(Constants.DefaultSchema);
            }

            lock (response.Responses)
            {
                foreach (var r in response.Responses)
                {
                    InternalQueueCommand(r, true);
                }
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            Clear(false);
        }

        /// <inheritdoc />
        public void Clear(bool clearInbound)
        {
            _commandQueue.Clear(clearInbound, () => _sessionManager.CompleteAll(_hostId, SessionStatus.Aborted));
        }

        /// <inheritdoc />
        public ClassCommand Dequeue()
        {
            return _commandQueue.Dequeue();
        }

        /// <inheritdoc />
        public void DisableSend()
        {
            _commandQueue.AllowSend = false;
            _commandQueue.Online = false;
        }

        /// <inheritdoc />
        public void EnableSend(bool startPump)
        {
            if (_commandQueue.AllowSend)
            {
                return;
            }

            _commandQueue.AllowSend = true;
            if (startPump)
            {
                Task.Run(HandleSend);
            }
        }

        /// <inheritdoc />
        public bool Enqueue(ClassCommand command)
        {
            if (command == null)
            {
                SourceTrace.TraceError(G2STrace.Source, @"EgmQueue.Enqueue : Trying to enqueue null command");

                throw new ArgumentNullException(nameof(command));
            }

            bool result;
            try
            {
                result = _commandQueue.Enqueue(command);
            }
            finally
            {
                // Set queue.SetSendFlag to true here to avoid a possible threading condition.
                // Setting SetSendFlag to true inside the message pump method only risks having the flag set
                // too late and ending up with multiple message pumps on a single queue.
                if (_commandQueue.SetSendFlag())
                {
                    Task.Run(HandleSend);
                }
            }

            return result;
        }

        /// <inheritdoc />
        public void SetOnline()
        {
            _commandQueue.Online = true;
        }

        /// <inheritdoc />
        public ClassCommand Peek()
        {
            return _commandQueue.Peek();
        }

        /// <inheritdoc />
        public ClassCommand Process()
        {
            return _commandQueue.Process();
        }

        /// <inheritdoc />
        public ClassCommand PeekProcess()
        {
            return _commandQueue.PeekProcess();
        }

        /// <inheritdoc />
        public void Received(ClassCommand command)
        {
            if (command == null)
            {
                SourceTrace.TraceError(
                    G2STrace.Source,
                    @"EgmQueue.Received : Trying to receive null command");
                throw new ArgumentNullException(nameof(command));
            }

            try
            {
                _commandQueue.Received(command);

                if (UpdatesDateTime(command))
                {
                    _commandQueue.ReceivedTimeStamp = Stopwatch.GetTimestamp();
                }
            }
            finally
            {
                // We set queue.SetReceiveFlag to true here to avoid a possible threading condition.
                // Setting SetReceiveFlag to true inside the message pump method only risks having the flag set
                // too late and ending up with multiple message pumps on a single queue.
                if (_commandQueue.SetReceiveFlag())
                {
                    Task.Run(HandleReceive);
                }
            }
        }

        /// <inheritdoc />
        public void Receive(ClassCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (command.HostId != _hostId)
            {
                return;
            }

            Received(command);
        }

        /// <inheritdoc />
        public TimeSpan DefaultSessionTimeout => Constants.DefaultTimeout;

        /// <inheritdoc />
        public bool QueueCommand(ClassCommand command)
        {
            // NOTE: This is called by the Session.OnExpire in the event of a retry.
            //  We're going to send this regardless of the queue state.  If there is an error the queue will be flushed
            return InternalQueueCommand(command, true);
        }

        /// <inheritdoc />
        public void OnSessionCompleted(object sender, SessionEventArgs e)
        {
            _sessionManager.Complete(e.SessionId);
        }

        /// <summary>
        ///     Clears the send flag
        /// </summary>
        internal void ClearSendFlag()
        {
            _commandQueue.ClearSendFlag();
        }

        /// <summary>
        ///     Clears the receive flag
        /// </summary>
        internal void ClearReceiveFlag()
        {
            _commandQueue.ClearReceiveFlag();
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _sessionManager.CompleteAll(_hostId, SessionStatus.Aborted);
                _sessionManager.Dispose();
            }

            _sessionManager = null;

            _disposed = true;
        }

        private static bool UpdatesDateTime(ClassCommand command)
        {
            return command.ClassName == "communications" && command.IClass.sessionType == SessionType.Request;
        }

        private Session InternalSendRequest(
            IClass request,
            SessionCallback callback,
            int retryCount,
            TimeSpan sessionTimeout,
            bool alwaysSend)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            request.timeToLive = (int)sessionTimeout.TotalMilliseconds;

            var command = ClassCommand.Create(request, _hostId, _egm.Id);
            var session = _sessionManager.Create(this, NextSessionId(), command, callback, retryCount, sessionTimeout);

            InternalQueueCommand(command, alwaysSend);

            return session;
        }

        private bool InternalQueueCommand(ClassCommand command, bool alwaysQueue)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (!alwaysQueue && !CanSend)
            {
                OnRequestError(command, new Error(ErrorCode.G2S_MSX003));
                return false;
            }

            try
            {
                return Enqueue(command);
            }
            catch (InvalidOperationException e)
            {
                OnRequestError(command, new Error(ErrorCode.G2S_MSX999, e.Message));
                return false;
            }
        }

        private async Task HandleReceive()
        {
            try
            {
                ClassCommand command;

                while ((command = PeekProcess()) != null)
                {
                    try
                    {
                        var session = _sessionManager.GetById(command.IClass.sessionId);

                        // if we are getting a response to a request update the session
                        if (command.IClass.sessionType == SessionType.Response)
                        {
                            session?.AddResponse(command);
                        }
                        else
                        {
                            var respond = await _dispatcher.Dispatch(command);
                            if (!respond)
                            {
                                continue;
                            }

                            // Sort out the response
                            if (command.Error.IsError || command.Responses.Any())
                            {
                                SendResponse(command);
                            }
                        }

                        OnReceivedCommand(command);
                    }
                    finally
                    {
                        Process();
                    }
                }
            }
            finally
            {
                ClearReceiveFlag();
            }
        }

        private async Task HandleSend()
        {
            try
            {
                var endpoint = _endpointProvider.GetEndpoint(_hostId);
                if (endpoint == null)
                {
                    return;
                }

                ClassCommand command;
                while ((command = Dequeue()) != null)
                {
                    command.CommandId = NextCommandId(_hostId);

                    var response = await endpoint.Send(command);

                    // Comms failed...
                    if (response == null)
                    {
                        _commandQueue.Clear(false, () => _sessionManager.CompleteAll(_hostId, SessionStatus.CommsLost));
                        break;
                    }

                    OnSentCommand(command);

                    var error = new Error(response.errorCode, response.errorText);
                    if (!error.IsError)
                    {
                        if (UpdatesDateTime(command))
                        {
                            _commandQueue.SentTimeStamp = Stopwatch.GetTimestamp();
                        }

                        continue;
                    }

                    // TODO: We really don't have a good way of dealing with this at the moment, so we're just going to let the request continue and wait for a response from the host
                    //  If the EGM receives a G2S_MSX003 Communications Not Online error from the host, the EGM MUST generate a G2S_CME101 Comms Not Established event and remain in the opening state.
                    //   A G2S_MSX003 error is an acceptable response from a host because the commsOnLine request that has been sent may have not been processed at the application level
                    //   before the g2sAck containing the G2S_MSX003 error code was sent.
                    if (error.Code == ErrorCode.G2S_MSX003 && command.IClass.Item is commsOnLine)
                    {
                        continue;
                    }

                    command.Error.Code = error.Code;
                    command.Error.Text = error.Text;
                    _sessionManager.Complete(command.SessionId, SessionStatus.RequestError);

                    // Inbound Command Queue Full
                    if (error.Code == ErrorCode.G2S_MSX006)
                    {
                        // NOTE: From the G2S text - After receiving a G2S_MSX006 error, the originator of the original
                        // message MUST wait until its normal resend timer has expired and then resend a new message.
                        Thread.Sleep(SessionTimeout);
                    }
                    else if (error.Code == ErrorCode.G2S_MSX003)
                    {
                        // Complete all pending sessions with comms lost?
                        _commandQueue.Clear(false, () => _sessionManager.CompleteAll(_hostId, SessionStatus.CommsLost));
                    }
                }
            }
            finally
            {
                ClearSendFlag();
            }
        }

        private long NextSessionId()
        {
            return _idProvider.NextSessionId();
        }

        private long NextCommandId(int hostId)
        {
            return _idProvider.NextCommandId(hostId);
        }

        private void OnRequestError(ClassCommand command, Error error)
        {
            SourceTrace.TraceWarning(
                G2STrace.Source,
                @"HostQueue.OnRequestError : Request error occurred
	EgmId : {0} 
	Class : {1} 
    Command : {2}
	ErrorCode : {2}
	ErrorText : {3}",
                command.EgmId,
                command.ClassName,
                command.CommandId,
                error.Code,
                error.Text);

            command.Error.Code = error.Code;
            command.Error.Text = error.Text;

            _sessionManager.Complete(command.SessionId, error);
        }

        private void OnSentCommand(ClassCommand command)
        {
            MessageSent?.Invoke(this, new MessageHandledEventArgs(command));
        }

        private void OnReceivedCommand(ClassCommand command)
        {
            MessageReceived?.Invoke(this, new MessageHandledEventArgs(command));
        }
    }
}
