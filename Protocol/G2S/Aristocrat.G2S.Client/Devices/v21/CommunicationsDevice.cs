namespace Aristocrat.G2S.Client.Devices.v21
{
    using Diagnostics;
    using Newtonsoft.Json;
    using Protocol.v21;
    using Stateless;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     The <i>communications</i> class encompasses a set of messages used to monitor and control communications between a
    ///     host system and EGMs.
    /// </summary>
    /// <remarks>
    ///     The <i>communications</i> class is a multi-device class. The host associated with a particular
    ///     communications device is the owner of that device. Other hosts may be configured as guests and may subscribe to the
    ///     communications events of the EGM device.
    ///     <para />
    ///     A communications device is created to manage the communications
    ///     between an EGM and a single host. This EGM creates a device for each host using the <i>hostId</i> as the
    ///     <i>deviceId</i>. A <i>deviceId</i> of <b>0</b> (zero) is reserved for the class level commands.
    ///     <para />
    ///     A communications device is created to manage the communications between a single host and  EGMs. The communications
    ///     device manages two communication services: point-to-point and multicast. The point-to-point service is the primary
    ///     means for configuration control, transactions, information exchanges, and event reporting. The multicast service is
    ///     used by a host to send messages to a plurality of EGMs.
    /// </remarks>
    public class CommunicationsDevice : HostOrientedDevice<communications>, ICommunicationsDevice
    {
        private const bool NegotiateNamespaces = false;
        private const int ReconnectDelay = 1000;
        private const int DefaultCommunicationsInterval = 30000;
        private const int CommsOnlineRetryInterval = DefaultCommunicationsInterval;
        private const int DefaultKeepAliveInterval = 0;
        private const int MinSyncInterval = 15000;
        private const int DefaultSyncInterval = 15000;

        private readonly Uri _address;
        private readonly ICommunicationsStateObserver _communicationsStateObserver;

        private readonly StateMachine<t_commsStates, CommunicationTrigger> _state;

        private readonly object _timerLock = new object();
        private readonly ITransportStateObserver _transportStateObserver;
        private int _commsDisabledInterval = DefaultCommunicationsInterval;

        private Timer _commsOnlineTimer;
        private Timer _commsTimer;

        private ICommunicationContext _context;

        private bool _disposed;
        private IHostQueue _hostQueue;
        private int _keepAliveInterval = DefaultKeepAliveInterval;
        private bool _open;
        private bool _configurationChanged;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommunicationsDevice" /> class.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <param name="deviceObserver">An <see cref="IDeviceObserver" /> instance.</param>
        /// <param name="address">The address of the client.</param>
        /// <param name="requiredForPlay">Is the device required to play.</param>
        /// <param name="transportStateObserver">An <see cref="ITransportStateObserver" /> instance.</param>
        /// <param name="communicationsStateObserver">An <see cref="ICommunicationsStateObserver" /> instance.</param>
        public CommunicationsDevice(
            int deviceId,
            IDeviceObserver deviceObserver,
            Uri address,
            bool requiredForPlay,
            ITransportStateObserver transportStateObserver,
            ICommunicationsStateObserver communicationsStateObserver)
            : base(deviceId, deviceObserver)
        {
            _transportStateObserver = transportStateObserver;
            _communicationsStateObserver = communicationsStateObserver;
            _address = address;
            RequiredForPlay = requiredForPlay;

            SetDefaults();

            _state = new StateMachine<t_commsStates, CommunicationTrigger>(t_commsStates.G2S_closed);

            ConfigureStates();

            _commsOnlineTimer = new Timer(CommsOnline);
        }

        // This completely safe, but should only be used by the communications device. Everyone else has what they need...
        private IHostQueue HostQueue => _hostQueue ?? (_hostQueue = Queue as IHostQueue);

        private bool HostUnresponsive
            => Queue.SentElapsedTime > NoResponseTimer && Queue.ReceivedElapsedTime > NoResponseTimer;

        /// <inheritdoc />
        [JsonIgnore]
        public bool OutboundOverflow => HostQueue.OutboundQueueFull;

        /// <inheritdoc />
        [JsonIgnore]
        public bool InboundOverflow => HostQueue.InboundQueueFull;

        /// <inheritdoc />
        [JsonIgnore]
        public t_transportStates TransportState { get; private set; }

        /// <inheritdoc />
        [JsonIgnore]
        public t_commsStates State => _state.State;

        /// <inheritdoc />
        public int TimeToLive { get; private set; }

        /// <inheritdoc />
        public TimeSpan NoResponseTimer { get; protected set; }

        /// <inheritdoc />
        public bool DisplayFault { get; protected set; }

        /// <inheritdoc />
        public bool AllowMulticast { get; protected set; }

        /// <inheritdoc />
        public override void Open(IStartupContext context)
        {
            _context = context;

            Enabled = false;
            HostEnabled = false;

            _state.Fire(
                !_state.CanFire(CommunicationTrigger.Enabled)
                    ? CommunicationTrigger.Close
                    : CommunicationTrigger.Enabled);

            HostQueue.SessionTimeout = TimeSpan.FromMilliseconds(TimeToLive);

            _open = true;
            _configurationChanged = false;
        }

        /// <inheritdoc />
        public override void Close()
        {
            lock (_timerLock)
            {
                _commsOnlineTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            }

            EndCommsTimer();

            _open = false;

            // Just to prevent trying to close when we're already closed
            if (_state.CanFire(CommunicationTrigger.Close))
            {
                _state.Fire(CommunicationTrigger.Close);
            }
        }

        /// <inheritdoc />
        public override void ApplyOptions(DeviceOptionConfigValues optionConfigValues)
        {
            // Chapter 2 communications Class does not have section Communications Option Configuration
        }

        /// <inheritdoc />
        public override void RegisterEvents()
        {
            var deviceClass = this.PrefixedDeviceClass();

            // General protocol events
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_APE001);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_APE002);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_APE003);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_APE004);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_APE005);

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME001);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME002);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME003);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME004);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME005);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME006);

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME099); //// TODO
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME100);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME101);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME102);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME103);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME104);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME105);

            ////EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME110);
            ////EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME111);
            ////EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME112);
            ////EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME113);

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME120);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME121);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME122);

            ////EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME130);
            ////EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME131);
            ////EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME132);
            ////EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME140);
            ////EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME141);
            ////EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME150);
            ////EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.G2S_CME151);
        }

        /// <inheritdoc />
        public void SetKeepAlive(int interval)
        {
            if (_keepAliveInterval == interval)
            {
                return;
            }

            if (_state.State == t_commsStates.G2S_onLine)
            {
                CreateCommsTimer(interval, _ => KeepAlive());
            }

            _keepAliveInterval = interval;
        }

        /// <inheritdoc />
        public void TriggerStateChange(CommunicationTrigger trigger)
        {
            TriggerStateChange(trigger, string.Empty);
        }

        /// <inheritdoc />
        public void TriggerStateChange(CommunicationTrigger trigger, string message)
        {
            DisableText = message;

            _state.Fire(trigger);
        }

        /// <inheritdoc />
        public void Configure(
            long id,
            bool useDefaultConfig,
            bool requiredForPlay,
            int timeToLive,
            int noResponseTimer,
            bool displayFault)
        {
            if (ConfigurationId == id)
            {
                return;
            }

            ConfigurationId = id;
            ConfigDateTime = DateTime.UtcNow;
            ConfigComplete = true;

            if (UseDefaultConfig != useDefaultConfig)
            {
                UseDefaultConfig = useDefaultConfig;
                _configurationChanged = true;
            }

            if (RequiredForPlay != requiredForPlay)
            {
                RequiredForPlay = requiredForPlay;
                _configurationChanged = true;
            }

            if (TimeToLive != timeToLive)
            {
                TimeToLive = timeToLive;
                _configurationChanged = true;
            }

            if (NoResponseTimer != TimeSpan.FromMilliseconds(noResponseTimer))
            {
                NoResponseTimer = TimeSpan.FromMilliseconds(noResponseTimer);
                _configurationChanged = true;
            }

            if (DisplayFault != displayFault)
            {
                DisplayFault = displayFault;
                _configurationChanged = true;
            }
        }

        /// <inheritdoc />
        public void NotifyConfigurationChanged()
        {
            if (_configurationChanged)
            {
                ReportEvent(EventCode.G2S_CME005);
                _configurationChanged = false;
            }
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                lock (_timerLock)
                {
                    if (_commsTimer != null)
                    {
                        _commsTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                        _commsTimer.Dispose();
                        _commsTimer = null;
                    }

                    if (_commsOnlineTimer != null)
                    {
                        _commsOnlineTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

                        using (var handle = new ManualResetEvent(false))
                        {
                            if (_commsOnlineTimer.Dispose(handle))
                            {
                                var interval = TimeSpan.FromMilliseconds(
                                    (Queue?.SessionTimeout.TotalMilliseconds ??
                                     Constants.DefaultTimeout.TotalMilliseconds) * 1.5);
                                if (!handle.WaitOne(interval))
                                {
                                    SourceTrace.TraceError(
                                        G2STrace.Source,
                                        @"CommunicationsDevice.Dispose : Timed out disposing commsOnline timer
    Host Id : {0}",
                                        Owner);
                                }
                            }
                        }

                        _commsOnlineTimer = null;
                    }
                }

                SourceTrace.TraceInformation(
                    G2STrace.Source,
                    @"CommunicationsDevice.Dispose : Disposed of communications Device
    Host Id : {0}",
                    Owner);
            }

            _disposed = true;
        }

        /// <inheritdoc />
        protected override void ConfigureDefaults()
        {
            base.ConfigureDefaults();

            SetDefaults();
        }

        private void SetDefaults()
        {
            TimeToLive = (int)Constants.DefaultTimeout.TotalMilliseconds;
            NoResponseTimer = Constants.NoResponseTimer;
            DisplayFault = false;
        }

        private void CommsOnline(object context)
        {
            var command = new commsOnLine
            {
                equipmentType = Constants.EgmType,
                egmLocation = _address.ToString(),
                deviceReset = _context?.DeviceReset ?? false,
                deviceChanged = _context?.DeviceChanged ?? false,
                subscriptionLost = _context?.SubscriptionLost ?? false,
                metersReset = _context?.MetersReset ?? false,
                deviceStateChanged = _context?.DeviceStateChanged ?? false,
                deviceAccessChanged = _context?.DeviceAccessChanged ?? false,
                negotiateNamespaces = NegotiateNamespaces
            };

            var request = InternalCreateClass();
            request.Item = command;

            var session = SendRequest(request, 0, true);
            session.WaitForCompletion(); // wait for the session to complete

            HandleResponse(session.SessionState, session.Request.Error);

            if (session.SessionState == SessionStatus.Success)
            {
                if (session.Responses[0].IClass.Item is commsOnLineAck ack)
                {
                    _commsDisabledInterval = ack.syncTimer < MinSyncInterval ? DefaultSyncInterval : ack.syncTimer;
                }

                TriggerStateChange(CommunicationTrigger.Established);

                SourceTrace.TraceInformation(
                    G2STrace.Source,
                    @"CommunicationsDevice.CommsOnline : commsOnline successful
    Host Id : {0}",
                    Owner);
            }
            else if (_open)
            {
                if (session.SessionState == SessionStatus.TimedOut)
                {
                    SourceTrace.TraceInformation(
                        G2STrace.Source,
                        @"CommunicationsDevice.CommsOnline : commsOnline Timed Out.  Will try again
    Host Id : {0}",
                        Owner);

                    lock (_timerLock)
                    {
                        try
                        {
                            _commsOnlineTimer?.Change(TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);
                        }
                        catch (ObjectDisposedException)
                        {
                        }
                    }
                }
                else
                {
                    SourceTrace.TraceInformation(
                        G2STrace.Source,
                        @"CommunicationsDevice.CommsOnline : commsOnline Failed.  Will try again
    Host Id : {0}",
                        Owner);

                    lock (_timerLock)
                    {
                        try
                        {
                            _commsOnlineTimer?.Change(
                                TimeSpan.FromMilliseconds(CommsOnlineRetryInterval),
                                Timeout.InfiniteTimeSpan);
                        }
                        catch (ObjectDisposedException)
                        {
                        }
                    }
                }
            }
        }

        private void CommsClosing(string reason = null)
        {
            var command = new commsClosing { reason = reason };

            var request = InternalCreateClass();
            request.Item = command;

            var session = SendRequest(request, true);
            session.WaitForCompletion(); // wait for the session to complete

            // If this timed out or failed with an error (likely G2S_MSX003/G2S_MSX007) we MUST transition to the closed state
            // NOTE: The host should reply with G2S_MSX003 if we're not online, but the observed behavior is that the IGT host will reply with G2S_MSX007 if the host was restarted
            if (session.SessionState == SessionStatus.Success || session.SessionState == SessionStatus.TimedOut ||
                session.SessionState == SessionStatus.CommsLost || session.Request.Error.IsError)
            {
                _state.Fire(CommunicationTrigger.Disabled);
            }
        }

        private void CommsDisabled()
        {
            if (!_state.IsInState(t_commsStates.G2S_sync))
            {
                SourceTrace.TraceInformation(
                    G2STrace.Source,
                    @"CommunicationsDevice.CommsDisabled : Will not send comms disabled not in t_commsStates.G2S_sync
    Host Id : {0}
    Current State : {1}",
                    Owner,
                    _state.State);

                return;
            }

            var command = new commsDisabled();

            var request = InternalCreateClass();
            request.Item = command;

            var session = SendRequest(request, true);
            session.WaitForCompletion(); // wait for the session to complete

            HandleResponse(session.SessionState, session.Request.Error);

            if (session.SessionState == SessionStatus.Success && session.Responses.Count > 0)
            {
                if (session.Responses[0].IClass.Item is commsDisabledAck ack)
                {
                    SetSyncTimer(ack.syncTimer < MinSyncInterval ? DefaultSyncInterval : ack.syncTimer);
                }
            }
        }

        private void KeepAlive()
        {
            var interval = TimeSpan.FromMilliseconds(_keepAliveInterval);

            if (HostQueue.ReceivedElapsedTime <= interval && HostQueue.SentElapsedTime <= interval)
            {
                return;
            }

            var command = new keepAlive();

            var request = InternalCreateClass();
            request.Item = command;

            var session = SendRequest(request);
            session.WaitForCompletion(); // wait for the session to complete

            HandleResponse(session.SessionState, session.Request.Error);
        }

        private void CreateCommsTimer(int interval, TimerCallback timerCallback)
        {
            lock (_timerLock)
            {
                EndCommsTimer();

                if (interval > 0)
                {
                    _commsTimer = new Timer(timerCallback, null, interval, interval);

                    SourceTrace.TraceVerbose(
                        G2STrace.Source,
                        @"CommunicationsDevice : Created comms timer
    Host Id : {0}
    Method : {1}",
                        Owner,
                        timerCallback.Method);
                }
            }
        }

        private void EndCommsTimer()
        {
            lock (_timerLock)
            {
                SourceTrace.TraceVerbose(
                    G2STrace.Source,
                    @"CommunicationsDevice : Ending the current comms timer
    Host Id : {0}",
                    Owner);

                _commsTimer?.Dispose();
                _commsTimer = null;
            }
        }

        private void ConfigureStates()
        {
            _state.Configure(t_commsStates.G2S_closed)
                .OnEntry(OnClosed)
                .Permit(CommunicationTrigger.Enabled, t_commsStates.G2S_opening);

            _state.Configure(t_commsStates.G2S_opening)
                .OnEntry(OnOpening)
                .Permit(CommunicationTrigger.Established, t_commsStates.G2S_sync)
                .Permit(CommunicationTrigger.OutboundOverflow, t_commsStates.G2S_closing)
                .Permit(CommunicationTrigger.ConfigChange, t_commsStates.G2S_closing)
                .Permit(CommunicationTrigger.Close, t_commsStates.G2S_closing);

            _state.Configure(t_commsStates.G2S_sync)
                .OnEntryFrom(CommunicationTrigger.Established, _ => { _context = null; })
                .OnEntryFrom(
                    CommunicationTrigger.HostDisabled,
                    _ =>
                    {
                        HostQueue.DisableSend();

                        EndCommsTimer();

                        HostEnabled = false;
                        ReportEvent(EventCode.G2S_CME003);
                    })
                .OnEntry(OnSync)
                .OnExit(EndCommsTimer)
                .Permit(CommunicationTrigger.HostEnabled, t_commsStates.G2S_onLine)
                .Permit(CommunicationTrigger.OutboundOverflow, t_commsStates.G2S_closing)
                .Permit(CommunicationTrigger.Error, t_commsStates.G2S_closing)
                .Permit(CommunicationTrigger.ConfigChange, t_commsStates.G2S_closing)
                .Permit(CommunicationTrigger.Close, t_commsStates.G2S_closing);

            _state.Configure(t_commsStates.G2S_onLine)
                .OnEntryFrom(CommunicationTrigger.OutboundOverflow, _ => { })
                .OnEntry(OnOnline)
                .Permit(CommunicationTrigger.OutboundOverflow, t_commsStates.G2S_overflow)
                .Permit(CommunicationTrigger.Error, t_commsStates.G2S_closing)
                .Permit(CommunicationTrigger.ConfigChange, t_commsStates.G2S_closing)
                .Permit(CommunicationTrigger.Close, t_commsStates.G2S_closing)
                .Permit(CommunicationTrigger.HostDisabled, t_commsStates.G2S_sync);

            _state.Configure(t_commsStates.G2S_overflow)
                .OnEntry(_ => { })
                .Permit(CommunicationTrigger.OutboundOverflowCleared, t_commsStates.G2S_onLine)
                .Permit(CommunicationTrigger.HostDisabled, t_commsStates.G2S_sync)
                .Permit(CommunicationTrigger.Error, t_commsStates.G2S_closing)
                .Permit(CommunicationTrigger.ConfigChange, t_commsStates.G2S_closing)
                .Permit(CommunicationTrigger.InboundOverflow, t_commsStates.G2S_closing)
                .Permit(CommunicationTrigger.Close, t_commsStates.G2S_closing);

            _state.Configure(t_commsStates.G2S_closing)
                .OnEntry(OnClosing)
                .OnExit(OnExitClosing)
                .Permit(CommunicationTrigger.Disabled, t_commsStates.G2S_closed);

            _state.OnUnhandledTrigger(
                (state, trigger) =>
                {
                    SourceTrace.TraceError(
                        G2STrace.Source,
                        @"CommunicationsDevice : Invalid Communications State Transition
    Host Id : {0}
	State : {1}
	Trigger : {2}",
                        Owner,
                        state,
                        trigger);
                });

            _state.OnTransitioned(
                transition =>
                {
                    SourceTrace.TraceVerbose(
                        G2STrace.Source,
                        @"CommunicationsDevice : State Transition
    Host Id : {0}
	From : {1}
	To : {2}
	Trigger : {3}",
                        Owner,
                        transition.Source,
                        transition.Destination,
                        transition.Trigger);

                    _communicationsStateObserver?.Notify(this, transition.Destination);
                });
        }

        private void OnClosed(StateMachine<t_commsStates, CommunicationTrigger>.Transition transition)
        {
            EndCommsTimer();

            SetTransportState(t_transportStates.G2S_transportDown);

            HostQueue.DisableSend();

            Enabled = false;
            ReportEvent(EventCode.G2S_CME001);

            HostEnabled = false;
            ReportEvent(EventCode.G2S_CME003);

            if (_open)
            {
                // Restart comms if the device is still in the open state
                Task.Delay(ReconnectDelay).ContinueWith(
                    _ =>
                    {
                        if (_open && _state.CanFire(CommunicationTrigger.Enabled))
                        {
                            _state.Fire(CommunicationTrigger.Enabled);
                        }
                    });
            }
        }

        private void OnOpening(StateMachine<t_commsStates, CommunicationTrigger>.Transition transition)
        {
            HostQueue.DisableSend();

            HostEnabled = false;
            SetTransportState(t_transportStates.G2S_transportDown);

            Enabled = true;
            ReportEvent(EventCode.G2S_CME002);

            lock (_timerLock)
            {
                _commsOnlineTimer?.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
            }
        }

        private void OnSync(StateMachine<t_commsStates, CommunicationTrigger>.Transition transition)
        {
            SetTransportState(t_transportStates.G2S_transportUp);

            if (transition.Trigger == CommunicationTrigger.Established)
            {
                ReportEvent(EventCode.G2S_CME100);
            }

            // Trigger a commsDisabled (immediately)
            Task.Run(() => CommsDisabled());
            CreateCommsTimer(_commsDisabledInterval, _ => CommsDisabled());
        }

        private void OnOnline(StateMachine<t_commsStates, CommunicationTrigger>.Transition transition)
        {
            HostQueue.SetOnline();
            HostQueue.EnableSend(true);

            HostEnabled = true;
            ReportEvent(EventCode.G2S_CME004);

            CreateCommsTimer(_keepAliveInterval, _ => KeepAlive());
        }

        private void OnClosing(StateMachine<t_commsStates, CommunicationTrigger>.Transition transition)
        {
            NotifyConfigurationChanged();

            HostQueue.DisableSend();
            EndCommsTimer();

            HostEnabled = false;
            ReportEvent(EventCode.G2S_CME003);

            CommsClosing();
        }

        private void OnExitClosing(StateMachine<t_commsStates, CommunicationTrigger>.Transition transition)
        {
        }

        private void HandleResponse(SessionStatus sessionState, Error error)
        {
            switch (sessionState)
            {
                case SessionStatus.Success:
                    SetTransportState(t_transportStates.G2S_transportUp);
                    return;
                case SessionStatus.CommsLost:
                    OnCommunicationsLost();
                    return;
                case SessionStatus.RequestError:
                    HandleError(error.Code);
                    break;
            }
        }

        private void HandleError(string errorCode)
        {
            if (!_state.IsInState(t_commsStates.G2S_opening) &&
                (errorCode == ErrorCode.G2S_MSX003 || errorCode == ErrorCode.G2S_MSX007))
            {
                OnCommunicationsNotEstablished();
            }
            else if (errorCode == ErrorCode.G2S_MSX002 && HostUnresponsive)
            {
                SetTransportState(t_transportStates.G2S_hostUnreachable);
            }
        }

        private void ReportEvent(string eventCode)
        {
            var status = new commsStatus
            {
                configurationId = ConfigurationId,
                hostEnabled = HostEnabled,
                egmEnabled = Enabled,
                outboundOverflow = OutboundOverflow,
                inboundOverflow = InboundOverflow,
                g2sProtocol = true,
                commsState = State,
                transportState = TransportState.ToString()
            };

            var deviceList = new deviceList1
            {
                statusInfo =
                    new[] { new statusInfo { deviceClass = this.PrefixedDeviceClass(), deviceId = Id, Item = status } }
            };

            EventHandlerDevice.EventReport(
                this.PrefixedDeviceClass(),
                Id,
                eventCode,
                deviceList);
        }

        private void SetTransportState(t_transportStates newState)
        {
            if (TransportState == newState)
            {
                return;
            }

            SourceTrace.TraceVerbose(
                G2STrace.Source,
                @"CommunicationsDevice : SetTransportState
    Host Id : {0}
	From : {1}
	To : {2}",
                Owner,
                TransportState,
                newState);

            TransportState = newState;

            _transportStateObserver?.Notify(this, TransportState);

            switch (TransportState)
            {
                case t_transportStates.G2S_hostUnreachable:
                    ReportEvent(EventCode.G2S_CME120);

                    // This triggers a call into this method with state G2S_transportDown and changes TransportState, so we need to run this on another thread
                    Task.Run(() => OnCommunicationsNotEstablished());
                    break;
                case t_transportStates.G2S_transportUp:
                    ReportEvent(EventCode.G2S_CME121);
                    break;
                case t_transportStates.G2S_transportDown:
                    ReportEvent(EventCode.G2S_CME122);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newState));
            }
        }

        private void OnCommunicationsLost()
        {
            SourceTrace.TraceVerbose(
                G2STrace.Source,
                @"CommunicationsDevice : OnCommunicationsLost
	Host Id: {0}",
                Owner);

            if (NoResponseTimer == TimeSpan.Zero || HostUnresponsive)
            {
                SetTransportState(t_transportStates.G2S_hostUnreachable);
            }
        }

        private void OnCommunicationsNotEstablished()
        {
            SourceTrace.TraceVerbose(
                G2STrace.Source,
                @"CommunicationsDevice : OnCommunicationsNotEstablished
	Host Id: {0}",
                Owner);

            ReportEvent(EventCode.G2S_CME101);

            if (_state.CanFire(CommunicationTrigger.Error))
            {
                _state.Fire(CommunicationTrigger.Error);
            }
        }

        private void SetSyncTimer(int interval)
        {
            if (_commsDisabledInterval == interval)
            {
                return;
            }

            CreateCommsTimer(interval, _ => CommsDisabled());

            _commsDisabledInterval = interval;
        }
    }
}
