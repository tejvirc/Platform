namespace Aristocrat.Monaco.Hardware.Fake
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Contracts.CardReader;
    using Contracts.Communicator;
    using Contracts.IdReader;
    using Contracts.IO;
    using Contracts.SharedDevice;
    using Kernel;
    using log4net;

    /// <summary>A Fake Id Reader adapter.</summary>
    public class FakeIdReaderAdapter : IIdReaderImplementation
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private static readonly TimeSpan LightingDelay = TimeSpan.FromMilliseconds(50);
        private readonly UnsupportedCommands _unsupported = UnsupportedCommands.None;
        private readonly IEventBus _eventBus;
        private readonly bool _isPhysical = true;
        private bool _inserted;
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the Aristocrat.Monaco.Hardware.Fake.FakeIdReaderAdapter class.
        ///     Construct a <see cref="FakeIdReaderAdapter" />
        /// </summary>
        public FakeIdReaderAdapter()
            : this(ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        /// <summary>
        ///     Construct a <see cref="FakeIdReaderAdapter" />
        ///     <param name="eventBus">The device option configuration values.</param>
        /// </summary>
        public FakeIdReaderAdapter(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            Logger.Debug("Constructed");
        }

        public bool IsOpen { get; set; }

        /// <inheritdoc />
        public int IdReaderId { get; set; }

        /// <inheritdoc />
        public bool IsEgmControlled { get; }

        /// <inheritdoc />
        public TrackData TrackData { get; set; }

        /// <inheritdoc />
        public IdReaderTypes IdReaderType { get; set; }

        /// <inheritdoc />
        public IdReaderTypes SupportedTypes { get; }

        /// <inheritdoc />
        public IdReaderTracks IdReaderTrack { get; set; }

        /// <inheritdoc />
        public IdReaderTracks SupportedTracks { get; }

        /// <inheritdoc />
        public IdValidationMethods ValidationMethod { get; set; }

        /// <inheritdoc />
        public IdValidationMethods SupportedValidation { get; }

        /// <inheritdoc />
        /// <summary>Gets a value indicating whether a card is inserted.</summary>
        /// <value>True if inserted, false if not.</value>
        public bool Inserted
        {
            get => _inserted;
            protected set
            {
                if (value == _inserted)
                {
                    return;
                }

                _inserted = value;
                if (value)
                {
                    OnIdPresented();
                }
                else
                {
                    OnIdCleared();
                }
            }
        }

        /// <inheritdoc />
        public IdReaderFaultTypes Faults { get; }

        /// <inheritdoc />
        public event EventHandler<FaultEventArgs> FaultCleared;

        /// <inheritdoc />
        public event EventHandler<FaultEventArgs> FaultOccurred;

        /// <inheritdoc />
        public event EventHandler<EventArgs> IdCleared;

        /// <inheritdoc />
        public event EventHandler<EventArgs> IdPresented;

        /// <inheritdoc />
        public event EventHandler<ValidationEventArgs> IdValidationRequested;

        /// <inheritdoc />
        public event EventHandler<EventArgs> ReadError;

        /// <inheritdoc />
        public void Eject()
        {
            ReleaseLatch();
            ClearBuffer();

            TrackData = null;
        }

        /// <inheritdoc />
        public void ValidationComplete()
        {
            Logger.Info("Validation complete for fake Id Reader adapter");
        }

        /// <inheritdoc />
        public void ValidationFailed()
        {
            Logger.Info("Validation failed for fake Id Reader adapter");
        }

        /// <inheritdoc />
        public Task<bool> SetIdNumber(string idNumber)
        {
            if (IsEgmControlled)
            {
                return Task.FromResult(false);
            }

            if (_isPhysical)
            {
                throw new NotSupportedException("Remote setting of ID is not supported at this time");
            }

            // assume the ID is validated since it is coming from the host - so do not request validation
            Inserted = true;

            TrackData = new TrackData { Track1 = idNumber };
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> Initialize(IGdsCommunicator communicator)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public bool IsConnected { get; set; }

        /// <inheritdoc />
        public bool IsInitialized { get; set; }

        /// <inheritdoc />
        public bool IsEnabled { get; set; }

        /// <inheritdoc />
        public int Crc => -1;

        /// <inheritdoc />
        public string Protocol { get; set; }

        /// <inheritdoc />
        public event EventHandler<EventArgs> Initialized;

        /// <inheritdoc />
        public event EventHandler<EventArgs> InitializationFailed;

        /// <inheritdoc />
        public event EventHandler<EventArgs> Enabled;

        /// <inheritdoc />
        public event EventHandler<EventArgs> Disabled;

        /// <inheritdoc />
        public event EventHandler<EventArgs> Connected;

        /// <inheritdoc />
        public event EventHandler<EventArgs> Disconnected;

        /// <inheritdoc />
        public event EventHandler<EventArgs> ResetSucceeded;

        /// <inheritdoc />
        public event EventHandler<EventArgs> ResetFailed;

        /// <inheritdoc />
        public bool Open()
        {
            if (IsOpen)
            {
                return true;
            }

            //prevent a double subscription for the FakeDeviceConnectedEvent
            _eventBus?.UnsubscribeAll(this);

            _eventBus?.Subscribe<FakeCardReaderEvent>(this, HandleEvent);

            ////We assume the device will be opened by default
            _eventBus?.Subscribe<FakeDeviceConnectedEvent>(this, HandleEvent);
            IsOpen = true;
            IsConnected = true;

            OnConnected();
            return true;
        }

        /// <inheritdoc />
        public bool Close()
        {
            _eventBus?.UnsubscribeAll(this);
            _eventBus?.Subscribe<FakeDeviceConnectedEvent>(this, HandleEvent);
            IsOpen = false;
            IsConnected = false;
            OnDisconnected();

            return true;
        }

        /// <inheritdoc />
        public Task<bool> Enable()
        {
            Logger.Info("Enabled fake Id Reader adapter");
            OnEnabled(EventArgs.Empty);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> Disable()
        {
            Logger.Warn("Disabled Fake Id Reader adapter.");
            OnDisable(EventArgs.Empty);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> SelfTest(bool nvm)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<int> CalculateCrc(int seed)
        {
            return Task.FromResult(0);
        }

        /// <inheritdoc />
        public void UpdateConfiguration(IDeviceConfiguration internalConfiguration)
        {
            Protocol = internalConfiguration.Protocol;
        }

        /// <inheritdoc />
        public int VendorId => 0;

        /// <inheritdoc />
        public int ProductId => 0;

        /// <inheritdoc />
        public bool IsDfuCapable => false;

        /// <inheritdoc />
        public bool IsDfuInProgress => false;

        /// <inheritdoc />
        public event EventHandler<ProgressEventArgs> DownloadProgressed;

        /// <inheritdoc />
        public Task<bool> Initialize(ICommunicator communicator)
        {
            Open();
            IsInitialized = true;
            Logger.Info("Initialized fake Id reader adapter");
            OnInitialized(EventArgs.Empty);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> Detach()
        {
            IsConnected = false;
            Logger.Info("Disconnected fake Id reader adapter");
            OnEnabled(EventArgs.Empty);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> Reconnect()
        {
            IsConnected = true;
            Logger.Info("Enabled fake Id reader adapter");
            OnEnabled(EventArgs.Empty);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<DfuStatus> Download(Stream firmware)
        {
            return Task.FromResult(DfuStatus.ErrUnknown);
        }

        /// <inheritdoc />
        public Task<DfuStatus> Upload(Stream firmware)
        {
            return Task.FromResult(DfuStatus.ErrUnknown);
        }

        /// <inheritdoc />
        public void Abort()
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases the latch.</summary>
        public void ReleaseLatch()
        {
            if (_unsupported.HasFlag(UnsupportedCommands.ReleaseLatchCommand))
            {
                Logger.Info("Released latch for fake Id reader adapter");
            }
        }

        /// <summary>Clears the buffer.</summary>
        public void ClearBuffer()
        {
            if (!IsEnabled) // this command is only allowed when the device is enabled
            {
                Logger.Info("Buffer cleared for fake Id reader adapter");
            }
        }

        /// <summary>Handle a <see cref="FakeCardReaderEvent" />.</summary>
        /// <param name="fakeCardReaderEvent">The <see cref="FakeCardReaderEvent" /> to handle.</param>
        protected void HandleEvent(FakeCardReaderEvent fakeCardReaderEvent)
        {
            var fakeCardData = fakeCardReaderEvent.CardValue;
            if (fakeCardReaderEvent.Action)
            {
                IdReaderTrack = IdReaderTracks.Track1;
                OnIdPresented();
                OnIdValidationRequested(
                    new ValidationEventArgs { TrackData = new TrackData { Track1 = fakeCardReaderEvent.CardValue } });
            }
            else
            {
                IdReaderTrack = IdReaderTracks.None;
                OnIdCleared();
            }

            Logger.Debug($"Fake Card Reader: {fakeCardData};");
        }

        /// <summary>Handle a <see cref="FakeDeviceConnectedEvent" />.</summary>
        /// <param name="fakeDeviceConnectedEvent">The <see cref="FakeDeviceConnectedEvent" /> to handle.</param>
        protected void HandleEvent(FakeDeviceConnectedEvent fakeDeviceConnectedEvent)
        {
            if (fakeDeviceConnectedEvent.Type != DeviceType.IdReader)
            {
                return;
            }

            if (!fakeDeviceConnectedEvent.Connected)
            {
                IsOpen = false;
                IsConnected = false;
                OnDisconnected();
                Logger.Debug("Closed fake Id Reader adapter port");
            }
            else
            {
                IsOpen = true;
                IsConnected = true;
                OnConnected();
                Logger.Debug("Opened fake Id Reader adapter port");
            }
        }

        /// <summary>Executes the <see cref="Initialized" /> action.</summary>
        protected void OnInitialized(EventArgs e)
        {
            Initialized?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="Enabled" /> action.</summary>
        protected void OnEnabled(EventArgs e)
        {
            Enabled?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="Disable" /> action.</summary>
        protected void OnDisable(EventArgs e)
        {
            Disabled?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="FaultCleared" /> event.</summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected void OnFaultCleared(FaultEventArgs e)
        {
            var invoker = FaultCleared;
            invoker?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="FaultOccurred" /> event.</summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected void OnFaultOccurred(FaultEventArgs e)
        {
            var invoker = FaultOccurred;
            invoker?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="IdPresented" /> event.</summary>
        protected void OnIdPresented()
        {
            var invoker = IdPresented;
            invoker?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the <see cref="IdCleared" /> event.</summary>
        protected void OnIdCleared()
        {
            TrackData = null;

            var invoker = IdCleared;
            invoker?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the <see cref="IdValidationRequested" /> event.</summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected void OnIdValidationRequested(ValidationEventArgs e)
        {
            var invoker = IdValidationRequested;
            invoker?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="ReadError" /> event.</summary>
        protected void OnReadError()
        {
            var invoker = ReadError;
            invoker?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Executes the <see cref="InitializationFailed" /> action.</summary>
        protected void OnInitializationFailed(EventArgs e)
        {
            InitializationFailed?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="Connected" /> action.</summary>
        protected void OnConnected()
        {
            Connected?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Executes the <see cref="Disconnected" /> action.</summary>
        protected void OnDisconnected()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Executes the <see cref="ResetSucceeded" /> action.</summary>
        protected void OnResetSucceeded()
        {
            ResetSucceeded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Executes the <see cref="ResetFailed" /> action.</summary>
        protected void OnResetFailed()
        {
            ResetFailed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Executes the <see cref="DownloadProgressed" /> action.</summary>
        protected void OnDownloadProgressed()
        {
            DownloadProgressed?.Invoke(this, (ProgressEventArgs)EventArgs.Empty);
        }

        protected void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        [Flags]
        private enum UnsupportedCommands
        {
            None = 0,
            DeviceStateReport = 1,
            GatReportCommand = 2,
            GetCountStatusCommand = 4,
            LatchModeCommand = 8,
            ReleaseLatchCommand = 16,
            UicLightControl = 32,
        }
    }
}