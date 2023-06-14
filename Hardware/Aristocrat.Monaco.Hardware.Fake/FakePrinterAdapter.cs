namespace Aristocrat.Monaco.Hardware.Fake
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Contracts.Communicator;
    using Contracts.IO;
    using Contracts.Printer;
    using Contracts.SharedDevice;
    using Kernel;
    using log4net;

    public class FakePrinterAdapter : IPrinterImplementation
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private static readonly TimeSpan FieldOfInterestDelay = TimeSpan.FromMilliseconds(50);
        private static readonly TimeSpan PrintingDelay = TimeSpan.FromMilliseconds(500);

        private readonly IEventBus _eventBus;
        private bool _disposed;

        /// <summary>
        ///     Construct a <see cref="FakePrinterAdapter" />
        /// </summary>
        public FakePrinterAdapter()
            : this(ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        /// <summary>
        ///     Construct a <see cref="FakePrinterAdapter" />
        ///     <param name="eventBus">The device option configuration values.</param>
        /// </summary>
        public FakePrinterAdapter(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            Logger.Debug("Constructed");
        }

        public bool IsOpen { get; set; }

        /// <inheritdoc />
        public bool IsConnected { get; set; }

        /// <inheritdoc />
        public bool IsInitialized { get; set; }

        /// <inheritdoc />
        public bool IsEnabled { get; set; }

        /// <inheritdoc />
        public int Crc => 0;

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

            //prevent a double subscription for the FakeDeviceConnectedEvent in the case of Open->Close->Open
            _eventBus?.UnsubscribeAll(this);

            _eventBus?.Subscribe<FakePrinterEvent>(this, HandleEvent);

            ////We assume the device will be opened by default
            _eventBus?.Subscribe<FakeDeviceConnectedEvent>(this, HandleEvent);

            return true;
        }

        /// <inheritdoc />
        public bool Close()
        {
            _eventBus?.UnsubscribeAll(this);
            _eventBus?.Subscribe<FakeDeviceConnectedEvent>(this, HandleEvent);

            return true;
        }

        /// <inheritdoc />
        public Task<bool> Enable()
        {
            IsEnabled = true;
            Logger.Info("Enabled fake printer adapter");
            OnEnabled(EventArgs.Empty);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> Disable()
        {
            IsEnabled = false;
            Logger.Warn("Disabled Fake printer adapter.");
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

        /// <summary>
        ///     Releases the unmanaged resources used by the Aristocrat.Monaco.Hardware.Fake.FakePrinterAdapter and optionally
        ///     releases the managed resources.
        /// </summary>
        /// resources.
        /// </param>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
            Logger.Info("Initialized fake printer adapter");
            OnInitialized(EventArgs.Empty);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> Detach()
        {
            IsConnected = false;
            Logger.Info("Disconnected fake printer adapter");
            OnEnabled(EventArgs.Empty);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> Reconnect()
        {
            IsConnected = true;
            Logger.Info("Enabled fake printer adapter");
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

        public Task<bool> Initialize(IGdsCommunicator communicator)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public bool CanRetract => true;

        /// <inheritdoc />
        public PrinterFaultTypes Faults { get; set; }

        /// <inheritdoc />
        public PrinterWarningTypes Warnings { get; set; }

        /// <inheritdoc />
        public event EventHandler<FaultEventArgs> FaultCleared;

        /// <inheritdoc />
        public event EventHandler<FaultEventArgs> FaultOccurred;

        /// <inheritdoc />
        public event EventHandler<FieldOfInterestEventArgs> FieldOfInterestPrinted;

        /// <inheritdoc />
        public event EventHandler<EventArgs> PrintCompleted;

        /// <inheritdoc />
        public event EventHandler<EventArgs> PrintIncomplete;

        /// <inheritdoc />
        public event EventHandler<EventArgs> PrintInProgress;

        /// <inheritdoc />
        public event EventHandler<WarningEventArgs> WarningCleared;

        /// <inheritdoc />
        public event EventHandler<WarningEventArgs> WarningOccurred;

        /// <inheritdoc />
        public Task<bool> DefineRegion(string region)
        {
            Logger.Info("Define region for fake printer adapter");
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> DefineTemplate(string template)
        {
            Logger.Info("Define template for fake printer adapter");
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> FormFeed()
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public async Task<bool> PrintTicket(string ticket)
        {
            Logger.Info("Printed ticket with fake printer adapter");
            await Task.Delay(PrintingDelay);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> PrintTicket(string ticket, Func<Task> onFieldOfInterest)
        {
            Logger.Info("Printed ticket with fake printer adapter");
            await Task.Delay(FieldOfInterestDelay);
            await (onFieldOfInterest?.Invoke() ?? Task.CompletedTask);
            await Task.Delay(PrintingDelay);
            return true;
        }

        /// <inheritdoc />
        public Task<bool> RetractTicket()
        {
            Logger.Info("Ticket retracted for fake printer adapter");
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<string> ReadMetrics()
        {
            Logger.Info("Read metrics for fake printer adapter");
            return Task.FromResult(string.Empty);
        }

        /// <inheritdoc />
        public Task<bool> TransferFile(GraphicFileType graphicType, int graphicIndex, Stream stream)
        {
            Logger.Info("File transferred for fake printer adapter");
            return Task.FromResult(true);
        }

        protected virtual void Dispose(bool disposing)
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

        /// <summary>Handle a <see cref="FakePrinterEvent" />.</summary>
        /// <param name="fakePrinterEvent">The <see cref="FakePrinterEvent" /> to handle.</param>
        protected void HandleEvent(FakePrinterEvent fakePrinterEvent)
        {
            HandleFaultChanged(fakePrinterEvent.ChassisOpen, PrinterFaultTypes.ChassisOpen);
            HandleFaultChanged(fakePrinterEvent.PaperEmpty, PrinterFaultTypes.PaperEmpty);
            HandleFaultChanged(fakePrinterEvent.PaperJam, PrinterFaultTypes.PaperJam);
            HandleFaultChanged(fakePrinterEvent.PrintHeadOpen, PrinterFaultTypes.PrintHeadOpen);
            HandleFaultChanged(fakePrinterEvent.TopOfForm, PrinterFaultTypes.PaperNotTopOfForm);

            HandleWarningChanged(fakePrinterEvent.PaperLow, PrinterWarningTypes.PaperLow);
            HandleWarningChanged(fakePrinterEvent.PaperInChute, PrinterWarningTypes.PaperInChute);
        }

        /// <summary>Handle a <see cref="FakeDeviceConnectedEvent" />.</summary>
        /// <param name="fakeDeviceConnectedEvent">The <see cref="FakeDeviceConnectedEvent" /> to handle.</param>
        protected void HandleEvent(FakeDeviceConnectedEvent fakeDeviceConnectedEvent)
        {
            if (fakeDeviceConnectedEvent.Type != DeviceType.Printer)
            {
                return;
            }

            if (!fakeDeviceConnectedEvent.Connected)
            {
                IsOpen = false;
                Logger.Debug("Closed fake printer adapter port");
            }
            else
            {
                IsOpen = true;
                Logger.Debug("Opened fake printer adapter port");
            }
        }

        /// <summary>Executes the <see cref="Initialized" /> action.</summary>
        protected void OnInitialized(EventArgs e)
        {
            Initialized?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="InitializationFailed" /> action.</summary>
        protected void OnInitializationFailed(EventArgs e)
        {
            InitializationFailed?.Invoke(this, e);
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

        /// <summary>Executes the <see cref="Connected" /> action.</summary>
        protected void OnConnected(FieldOfInterestEventArgs e)
        {
            Connected?.Invoke(this, e);
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

        /// <summary>Executes the <see cref="FaultCleared" /> action.</summary>
        protected void OnFaultCleared(FaultEventArgs e)
        {
            FaultCleared?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="OnFaultOccurred" /> action.</summary>
        protected void OnFaultOccurred(FaultEventArgs e)
        {
            FaultOccurred?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="WarningCleared" /> action.</summary>
        protected void OnWarningCleared(WarningEventArgs e)
        {
            WarningCleared?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="WarningOccurred" /> action.</summary>
        protected void OnWarningOccurred(WarningEventArgs e)
        {
            WarningOccurred?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="FieldOfInterestPrinted" /> action.</summary>
        protected void OnFieldOfInterestPrinted(FieldOfInterestEventArgs e)
        {
            FieldOfInterestPrinted?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="PrintCompleted" /> action.</summary>
        protected void OnPrintCompleted()
        {
            PrintCompleted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Executes the <see cref="PrintIncomplete" /> action.</summary>
        protected void OnPrintIncomplete()
        {
            PrintIncomplete?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Executes the <see cref="PrintInProgress" /> action.</summary>
        protected void OnPrintInProgress()
        {
            PrintInProgress?.Invoke(this, EventArgs.Empty);
        }

        private void HandleFaultChanged(bool state, PrinterFaultTypes fault)
        {
            if (state && (Faults & fault) == 0)
            {
                Faults |= fault;
                FaultOccurred?.Invoke(this, new FaultEventArgs { Fault = fault });
            }
            else if (!state && (Faults & fault) != 0)
            {
                Faults &= ~fault;
                FaultCleared?.Invoke(this, new FaultEventArgs { Fault = fault });
            }
        }

        private void HandleWarningChanged(bool state, PrinterWarningTypes warning)
        {
            if (state && (Warnings & warning) == 0)
            {
                Warnings |= warning;
                WarningOccurred?.Invoke(this, new WarningEventArgs { Warning = warning });
            }
            else if (!state && (Warnings & warning) != 0)
            {
                Warnings &= ~warning;
                WarningCleared?.Invoke(this, new WarningEventArgs { Warning = warning });
            }
        }
    }
}