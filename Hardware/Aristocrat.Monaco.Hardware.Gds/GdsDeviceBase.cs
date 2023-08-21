namespace Aristocrat.Monaco.Hardware.Gds
{
    using Contracts;
    using Contracts.Communicator;
    using Contracts.Gds;
    using Contracts.SharedDevice;
    using log4net;
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Dfu;
    using InvokerQueue = System.Collections.Concurrent.ConcurrentQueue<System.Action<Contracts.Gds.GdsSerializableMessage>>;
    using ReportQueue = System.Collections.Concurrent.BlockingCollection<(object Report, long Expiry)>;

    /// <summary>Base class for implementing Gaming Device Standards (GDS) logic.</summary>
    /// <seealso cref="T:DfuDeviceBase"/>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.IGdsDevice"/>
    public abstract class GdsDeviceBase : DfuDeviceBase, IGdsDevice
    {
        // multiply timeout by 2 to provide a little buffer
        /// <summary>The default response timeout.</summary>
        public const int DefaultResponseTimeout = 5000 * 2;

        /// <summary>The extended response timeout.</summary>
        public const int ExtendedResponseTimeout = 20000 * 2;
        /// <summary>The max retries.</summary>
        protected const int MaxRetry = 3;
        private const int RetryIntervalDefault = 500; // milliseconds

        private readonly ILog _logger;
        private readonly ConcurrentDictionary<Type, InvokerQueue> _router = new ConcurrentDictionary<Type, InvokerQueue>();
        private readonly ConcurrentDictionary<Type, ReportQueue> _reports = new ConcurrentDictionary<Type, ReportQueue>();
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private readonly object _openLock = new object();

        /// <summary>The communicator.</summary>
        protected IGdsCommunicator _communicator;

        private bool _connected;
        private bool _initialized;
        private bool _enabled;
        private bool _tryingToOpen;

        /// <summary>
        /// constructor
        /// </summary>
        protected GdsDeviceBase()
        {
            _logger = LogManager.GetLogger(GetType());

            RegisterCallback<PowerStatus>(PowerStatusReported);
            RegisterCallback<GatData>(GatDataReported);
            RegisterCallback<CrcData>(CrcDataReported);
            RegisterCallback<DeviceState>(DeviceStateReported);
        }

#if !(RETAIL)
        /// <summary>
        /// Get Communicator for automation.
        /// </summary>
        public IGdsCommunicator Communicator => _communicator;
#endif

        /// <inheritdoc/>
        public bool IsConnected
        {
            get => _connected;
            protected set
            {
                if (_connected == value)
                {
                    return;
                }

                _connected = value;
                if (value)
                {
                    OnConnected();
                }
                else
                {
                    OnDisconnected();
                }
            }
        }

        /// <inheritdoc/>
        public bool IsInitialized
        {
            get => _initialized;
            protected set
            {
                _initialized = value;
                if (value)
                {
                    OnInitialized();
                }
            }
        }

        /// <inheritdoc/>
        public bool IsEnabled
        {
            get => _enabled;
            protected set
            {
                _enabled = value;
                if (value)
                {
                    OnEnabled();
                }
                else
                {
                    OnDisabled();
                }
            }
        }

        /// <summary>
        /// GAT report
        /// </summary>
        public string GatReport { get; protected set; } = string.Empty;

        /// <inheritdoc/>
        public int Crc { get; protected set; }

        /// <inheritdoc/>
        public string Protocol => _communicator?.Protocol ?? string.Empty;

        /// <summary>Gets a value indicating whether the device has external power.</summary>
        /// <value>True if the device has external power, false if not.</value>
        public bool ExternalPower { get; protected set; }

        /// <summary>Gets a value indicating whether the device requires reset.</summary>
        /// <value>True if the device requires reset, false if not.</value>
        public bool RequiresReset { get; protected set; }

        /// <summary>
        ///     Gets the manufacturer.
        /// </summary>
        public string Manufacturer => _communicator?.Manufacturer ?? string.Empty;

        /// <summary>
        ///     Gets the model.
        /// </summary>
        public string Model => _communicator?.Model ?? string.Empty;

        /// <summary>
        ///     Gets the serial number.
        /// </summary>
        public string SerialNumber => _communicator?.SerialNumber ?? string.Empty;

        /// <summary>Gets or sets the reset delay interval.</summary>
        /// <value>The reset delay interval.</value>
        protected int RetryInterval { get; set; } = RetryIntervalDefault;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> Initialized;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> InitializationFailed;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> Enabled;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> Disabled;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> Connected;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> Disconnected;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> ResetSucceeded;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> ResetFailed;

        /// <inheritdoc/>
        public virtual bool Close()
        {
            IsConnected = !_communicator?.Close() ?? false;
            return IsConnected;
        }

        /// <inheritdoc/>
        public virtual bool Open()
        {
            lock (_openLock)
            {
                if (IsConnected)
                {
                    return true;
                }

                Close();

                if (!_communicator?.Open() ?? false)
                {
                    return false;
                }

                IsConnected = Reset().Result;
            }

            return IsConnected;
        }

        /// <inheritdoc/>
        public override async Task<bool> Initialize(ICommunicator communicator)
        {
            if (communicator is not IGdsCommunicator gdsCommunicator)
            {
                throw new ArgumentException("GDS devices must be initialized with IGdsCommunicator");
            }

            return await Initialize(gdsCommunicator);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> Initialize(IGdsCommunicator communicator)
        {
            IsInitialized = false;
            try
            {
                ResetDevice();

                _communicator = communicator;
                if (_communicator == null)
                {
                    throw new InvalidDataException(nameof(communicator));
                }

                _communicator.DeviceType = DeviceType;

                if (!await base.Initialize(communicator) && _communicator.IsDfuCapable)
                {
                    return false;
                }

                _communicator.DeviceAttached += DeviceAttached;
                _communicator.DeviceDetached += DeviceDetached;
                _communicator.MessageReceived += ReportReceived;
                IsInitialized = TryOpen();
                return IsInitialized;
            }
            finally
            {
                if (!IsInitialized)
                {
                    OnInitializationFailed();
                }
            }
        }

        /// <summary>Sends an Ack command to the devices.</summary>
        /// <param name="transactionId">Identifier for the transaction.</param>
        /// <param name="resync">True to resync.</param>
        public virtual void Ack(byte transactionId, bool resync)
        {
            SendCommand(
                new Ack
                {
                    Resync = resync,
                    TransactionId = transactionId
                });
        }

        /// <inheritdoc/>
        public virtual async Task<bool> Enable()
        {
            SendCommand<DeviceState>(new GdsSerializableMessage(GdsConstants.ReportId.Enable));
            var report = await WaitForReport<DeviceState>();
            return report?.Enabled ?? false;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> Disable()
        {
            SendCommand<DeviceState>(new GdsSerializableMessage(GdsConstants.ReportId.Disable));
            var report = await WaitForReport<DeviceState>();
            return report?.Disabled ?? false;
        }

        /// <inheritdoc/>
        public abstract Task<bool> SelfTest(bool nvm);

        /// <inheritdoc/>
        public virtual void UpdateConfiguration(IDeviceConfiguration internalConfiguration)
        {
            internalConfiguration.Manufacturer = _communicator?.Manufacturer ?? string.Empty;
            internalConfiguration.Model = _communicator?.Model ?? string.Empty;
            internalConfiguration.FirmwareBootVersion = _communicator?.BootVersion ?? string.Empty;
            internalConfiguration.FirmwareId = _communicator?.FirmwareVersion ?? string.Empty;
            internalConfiguration.FirmwareRevision = _communicator?.FirmwareRevision ?? string.Empty;
            internalConfiguration.FirmwareCyclicRedundancyCheck = $"0x{_communicator?.FirmwareCrc:X4}";
            internalConfiguration.VariantName = _communicator?.VariantName ?? string.Empty;
            internalConfiguration.VariantVersion = _communicator?.VariantVersion ?? string.Empty;
        }

        /// <summary>
        /// Read GAT report
        /// </summary>
        /// <returns>Task to read it</returns>
        public virtual async Task<string> RequestGatReport(int millisecondsTimeout = DefaultResponseTimeout)
        {
            _logger.Debug($"RequestGatReport - {DeviceType} - millisecondsTimeout {millisecondsTimeout}");

            if (IsEnabled) // this command is only allowed when the device is disabled
            {
                _logger.Warn($"RequestGatReport - {DeviceType} is enabled, skipping");
                return null;
            }

            GatReport = string.Empty;
            SendCommand<GatData>(new GdsSerializableMessage(GdsConstants.ReportId.RequestGatReport));

            GatReport = await WaitForDataReport<GatData>(millisecondsTimeout);
            _logger.Debug($"RequestGatReport - {DeviceType} - Gat Report is {GatReport}");
            return GatReport;
        }

        /// <inheritdoc/>
        public virtual async Task<int> CalculateCrc(int seed)
        {
            if (IsEnabled) // this command is only allowed when the device is disabled
            {
                return 0;
            }

            var restoreCrc = seed != 0;
            var currentCrc = Crc;

            Crc = 0;
            SendCommand(
                new CrcRequest
                {
                    Seed = (uint)seed
                });

            var report = await WaitForReport<CrcData>(ExtendedResponseTimeout);
            var result = report?.Result ?? 0;

            Crc = restoreCrc ? currentCrc : result;

            return result;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_tryingToOpen)
                {
                    _tryingToOpen = false;
                }

                Close();

                ResetDevice();

                _router.Clear();
                _reports.Clear();
            }

            base.Dispose(disposing);
        }

        /// <summary>Resets this device.</summary>
        /// <returns>An asynchronous result that yields true if it succeeds, false if it fails.</returns>
        protected abstract Task<bool> Reset();

        /// <summary>
        /// Send command to communicator
        /// </summary>
        /// <param name="message">Command message</param>
        /// <param name="token">The cancellation token</param>
        protected void SendCommand(GdsSerializableMessage message, CancellationToken token = default)
        {
            if (_communicator?.IsOpen ?? false)
            {
                _communicator.SendMessage(message, token);
            }
        }

        /// <summary>
        /// send command, define response
        /// </summary>
        /// <typeparam name="T">Type of expected response</typeparam>
        /// <param name="message">Command message</param>
        /// <param name="token">The cancellation token</param>
        protected void SendCommand<T>(GdsSerializableMessage message, CancellationToken token = default)
            where T : GdsSerializableMessage
        {
            ClearStaleReports<T>();

            SendCommand(message, token);
        }

        /// <summary>
        ///     Registers the callback when a report event is received from the device. If the first report ID matches then callback
        /// will be called.
        /// </summary>
        /// <typeparam name="T">Type of the payload.</typeparam>
        /// <param name="callback">The callback.</param>
        /// <remarks>Overload for messages with a payload.</remarks>
        protected void RegisterCallback<T>(Action<T> callback)
            where T : GdsSerializableMessage
        {
            var invokers = ReportInvokers(typeof(T));
            invokers.Enqueue(buffer =>
            {
                try
                {
                    callback(buffer as T);
                }
                catch (Exception e)
                {
                    _logger.Error($"ReportReceived: exception processing object {e}");
                }

            });
        }

        /// <summary>Wait for data report.</summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="millisecondsTimeout">(Optional) The milliseconds timeout.</param>
        /// <returns>An asynchronous result that yields a string.</returns>
        protected async Task<string> WaitForDataReport<T>(int millisecondsTimeout = DefaultResponseTimeout)
            where T : GdsSerializableMessage, IDataReport
        {
            var report = await WaitForReport<T>(millisecondsTimeout);
            return report?.Data ?? string.Empty;
        }

        /// <summary>Wait for report.</summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="millisecondsTimeout">(Optional) The milliseconds timeout.</param>
        /// <returns>An asynchronous result that yields a T.</returns>
        protected async Task<T> WaitForReport<T>(int millisecondsTimeout = DefaultResponseTimeout)
            where T : GdsSerializableMessage
        {
            return await ReceiveReport<T>(millisecondsTimeout, CancellationToken.None);
        }

        /// <summary>Wait for report.</summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>.</param>
        /// <returns>An asynchronous result that yields a T.</returns>
        protected async Task<T> WaitForReport<T>(CancellationToken cancellationToken)
            where T : GdsSerializableMessage
        {
            return await ReceiveReport<T>(DefaultResponseTimeout, cancellationToken);
        }

        /// <summary>Publish report so that senders are aware of it.</summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <param name="report">The report.</param>
        /// <param name="timeout">The timeout.</param>
        protected void PublishReport<T>(T report, int timeout = DefaultResponseTimeout)
        {
            var reports = ReportQueue<T>();
            reports.Add((report, _stopwatch.ElapsedMilliseconds + timeout));
        }

        /// <summary>Device attached.</summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="args">Event information.</param>
        protected virtual void DeviceAttached(object sender, EventArgs args)
        {
            _logger.Info($"DeviceAttached - {DeviceType} attached");

            if (TryOpen())
            {
                OnInitialized();
            }
        }

        /// <summary>Attempts to open the device communication</summary>
        /// <param name="retryLimit">The number of tries to attempt to open the device</param>
        /// <returns>Whether or not the device was opened</returns>
        protected bool TryOpen(int retryLimit = MaxRetry)
        {
            _tryingToOpen = true;
            var open = Open();
            var attempt = 0;
            while (!open && _tryingToOpen && ++attempt <= retryLimit)
            {
                _logger.Warn($"TryOpen - Attempting to reconnect to the attached device {DeviceType}.  Retry open attempt {attempt} after sleeping {RetryInterval}");
                Thread.Sleep(RetryInterval);
                open = _tryingToOpen ? Open() : IsConnected;
            }

            _tryingToOpen = false;
            return open;
        }

        /// <summary>Device detached.</summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="args">Event information.</param>
        protected virtual void DeviceDetached(object sender, EventArgs args)
        {
            _logger.Warn($"DeviceDetached - {DeviceType} has detached");
            Close();
        }

        /// <summary>Raises the <see cref="Disabled"/> event.</summary>
        protected virtual void OnDisabled()
        {
            Disabled?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the <see cref="Enabled"/> event.</summary>
        protected virtual void OnEnabled()
        {
            Enabled?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the <see cref="ResetFailed"/> event.</summary>
        protected virtual void OnFailedReset()
        {
            ResetFailed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the <see cref="ResetSucceeded"/> event.</summary>
        protected virtual void OnSuccessReset()
        {
            ResetSucceeded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the <see cref="Connected"/> event.</summary>
        protected virtual void OnConnected()
        {
            Connected?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the <see cref="Disconnected"/> event.</summary>
        protected virtual void OnDisconnected()
        {
            Crc = 0;

            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the <see cref="InitializationFailed"/> event.</summary>
        protected virtual void OnInitializationFailed()
        {
            InitializationFailed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the <see cref="Initialized"/> event.</summary>
        protected virtual void OnInitialized()
        {
            Initialized?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Executes the power status reported action.</summary>
        /// <param name="report">The report.</param>
        protected virtual void PowerStatusReported(PowerStatus report)
        {
            _logger.Debug($"PowerStatusReported: {report}");
            if (report.ExternalPower)
            {
                _logger.Info("PowerStatusReported: External power is connected");
            }
            else
            {
                _logger.Error("PowerStatusReported: External power is not connected");
            }

            if (report.RequiresReset)
            {
                _logger.Warn("PowerStatusReported: Reset is needed");
            }
            else
            {
                _logger.Info("PowerStatusReported: Reset is not needed");
            }

            ExternalPower = report.ExternalPower;
            RequiresReset = report.RequiresReset;
            if (RequiresReset)
            {
                _communicator?.ResetConnection();
            }
        }

        /// <summary>Called when a GAT data report is received.</summary>
        /// <param name="report">The report.</param>
        protected virtual void GatDataReported(GatData report)
        {
            _logger.Debug($"GatDataReported: {report}");
            PublishReport(report);
        }

        /// <summary>Called when a CRC data report is received.</summary>
        /// <param name="report">The report.</param>
        protected virtual void CrcDataReported(CrcData report)
        {
            _logger.Debug($"CrcData: {report}");
            PublishReport(report, ExtendedResponseTimeout);
        }

        /// <summary>Called when a device state report is received.</summary>
        /// <param name="report">The report.</param>
        protected virtual void DeviceStateReported(DeviceState report)
        {
            _logger.Debug($"DeviceState: {report}");
            if (report.Enabled)
            {
                IsEnabled = true;
            }

            // give disabled priority - last processed wins
            // technically Enabled and Disabled should never both be set
            if (report.Disabled)
            {
                IsEnabled = false;
            }

            PublishReport(report);
        }

        /// <summary>Which device type is this</summary>
        protected DeviceType DeviceType { get; set; }

        /// <summary>Clears any stale reports of given type.</summary>
        protected void ClearStaleReports<T>()
        {
            var reports = ReportQueue<T>();
            while (reports.Count > 0)
            {
                reports.TryTake(out var report, 0);
                _logger.Debug($"Cleared stale report {typeof(T).FullName}: {report}");
            }
        }

        private void ReportReceived(object sender, GdsSerializableMessage e)
        {
            var invokers = ReportInvokers(e.GetType());
            foreach (var invoker in invokers)
            {
                invoker(e);
            }
        }

        private async Task<T> ReceiveReport<T>(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                var reports = ReportQueue<T>();
                try
                {
                    while (reports.TryTake(out var report, millisecondsTimeout, cancellationToken))
                    {
                        if (report.Expiry < _stopwatch.ElapsedMilliseconds)
                        {
                            _logger.Warn($"ReceiveReport {report.Report.GetType()} EXPIRED");
                            continue; // expired report
                        }

                        return (T)report.Report;
                    }
                }
                catch (Exception e)
                {
                    _logger.Error($"ReceiveReport: {e}");
                }

                return default;
            }, CancellationToken.None);
        }

        private InvokerQueue ReportInvokers(Type reportType)
        {
            return _router.GetOrAdd(reportType, key => new InvokerQueue());
        }

        private ReportQueue ReportQueue<T>()
        {
            return _reports.GetOrAdd(typeof(T), key => new ReportQueue());
        }

        private void ResetDevice()
        {
            if (_communicator != null)
            {
                _communicator.DeviceAttached -= DeviceAttached;
                _communicator.DeviceDetached -= DeviceDetached;
                _communicator.MessageReceived -= ReportReceived;
                _communicator = null;
            }

            IsConnected = false;
        }
    }
}
