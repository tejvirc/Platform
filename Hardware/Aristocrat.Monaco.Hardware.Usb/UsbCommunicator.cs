namespace Aristocrat.Monaco.Hardware.Usb
{
    using System;
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Communicator;
    using Contracts.Gds;
    using Contracts.Gds.CardReader;
    using Contracts.IdReader;
    using Contracts.SharedDevice;
    using log4net;

    /// <summary>An USB communicator class</summary>
    [HardwareDevice("GDS", DeviceType.NoteAcceptor)]
    [HardwareDevice("GDS", DeviceType.Printer)]
    public class UsbCommunicator :
        IDfuDriver,
        IGdsCommunicator
    {
        private const int LoopTimeout = 1000;
        private const int UicCardReaderSendInterval = 500; // UIC seems to need a break between messages

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private DfuDriver _dfuDriver;
        private HidDriver _hidDriver;
        private BlockingCollection<GdsSerializableMessage> _commandQueue = new BlockingCollection<GdsSerializableMessage>();
        private Task _transmitter;
        private bool _closing;
        private IDataReport _dataReportUnderConstruction;

        /// <summary>Gets or sets a value indicating whether the configured.</summary>
        /// <value>True if configured, false if not.</value>
        public bool IsConfigured { get; set; }

        /// <inheritdoc />
        public bool IsOpen { get; set; }

        /// <inheritdoc />
        public int VendorId { get; set; } = -1;

        /// <inheritdoc />
        public int ProductId { get; set; } = -1;

        /// <inheritdoc />
        public int ProductIdDfu { get; set; } = -1;

        /// <inheritdoc />
        public string Protocol => "GDS";

        /// <inheritdoc />
        public DeviceType DeviceType { get; set; }

        /// <inheritdoc />
        public string FirmwareVersion => string.Empty;

        /// <inheritdoc />
        public string FirmwareRevision => string.Empty;

        /// <inheritdoc />
        public int FirmwareCrc => -1;

        /// <inheritdoc />
        public string BootVersion => string.Empty;

        /// <inheritdoc />
        public string VariantName => string.Empty;

        /// <inheritdoc />
        public string VariantVersion => string.Empty;

        /// <inheritdoc />
        public bool IsDfuCapable => true;

        /// <inheritdoc />
        public string Manufacturer => _hidDriver?.Manufacturer?.Trim() ?? string.Empty;

        /// <inheritdoc />
        public string Model => _hidDriver?.Model?.Trim() ?? string.Empty;

        /// <inheritdoc />
        public string Firmware => _hidDriver?.Firmware?.Trim() ?? string.Empty;

        /// <inheritdoc />
        public string SerialNumber => _hidDriver?.SerialNumber?.Trim() ?? string.Empty;

        /// <inheritdoc/>
        public IDevice Device { get; set; }

        /// <inheritdoc />
        public event EventHandler<EventArgs> DeviceAttached;

        /// <inheritdoc />
        public event EventHandler<EventArgs> DeviceDetached;

        /// <inheritdoc />
        public virtual bool Close()
        {
            if (!IsConfigured)
            {
                return false;
            }

            if (!IsOpen)
            {
                return true;
            }

            try
            {
                _closing = true;
                if (_transmitter != null)
                {
                    try
                    {
                        _transmitter.Wait();
                    }
                    catch (AggregateException ae)
                    {
                        ae.Handle(
                            ex =>
                            {
                                Logger.Error($"Close exception: {ex}");
                                return true;
                            });
                    }

                    _transmitter = null;
                }

                // clear the command queue
                while (_commandQueue?.TryTake(out _) ?? false)
                { }

                IsOpen = !_hidDriver?.Close() ?? false;
                return true;
            }
            catch (IOException e)
            {
                Logger.Error($"Close: IOException {e}");
                return false;
            }
        }

        /// <inheritdoc />
        public virtual bool Open()
        {
            if (!IsConfigured)
            {
                return false;
            }

            if (IsOpen)
            {
                return true;
            }

            try
            {
                _closing = false;

                if (!_hidDriver?.Open() ?? true)
                {
                    return false;
                }

                IsOpen = true;
                Transmitter();
                return true;
            }
            catch (IOException e)
            {
                Logger.Error($"Open: IOException {e}");
                ResetConnection();
                return false;
            }
        }

        /// <inheritdoc />
        public bool Configure(IComConfiguration comConfiguration)
        {
            IsConfigured = false;
            if (string.IsNullOrEmpty(comConfiguration.UsbDeviceVendorId) ||
                string.IsNullOrEmpty(comConfiguration.UsbDeviceProductId) &&
                string.IsNullOrEmpty(comConfiguration.UsbDeviceProductIdDfu))
            {
                VendorId = -1;
                ProductId = -1;
                ProductIdDfu = -1;

                if (string.IsNullOrEmpty(comConfiguration.UsbDeviceVendorId))
                {
                    Logger.Error("Configure: Failed with no vendor Id");
                }
                else if (string.IsNullOrEmpty(comConfiguration.UsbDeviceProductId) &&
                         string.IsNullOrEmpty(comConfiguration.UsbDeviceProductIdDfu))
                {
                    Logger.Error("Configure: Failed with no product Id");
                }

                return false;
            }

            // Retrieve the configuration info for the Usb device.
            VendorId = int.Parse(comConfiguration.UsbDeviceVendorId.Substring(4), NumberStyles.HexNumber);
            ProductId = int.Parse(comConfiguration.UsbDeviceProductId.Substring(4), NumberStyles.HexNumber);
            ProductIdDfu = int.Parse(comConfiguration.UsbDeviceProductIdDfu.Substring(4), NumberStyles.HexNumber);

            _hidDriver = new HidDriver
            {
                VendorId = VendorId,
                ProductId = ProductId
            };
            _hidDriver.ReportReceived += DriverReportReceived;
            _hidDriver.DeviceAttached += DriverAttached;
            _hidDriver.DeviceDetached += DriverDetached;
            _hidDriver?.Initialize();
            IsConfigured = true;
            return true;
        }

        /// <inheritdoc />
        public virtual void ResetConnection()
        {
            try
            {
                Logger.Debug($"Resetting connection for {VendorId:X}...");

                var processInfo = new ProcessStartInfo("cmd.exe", $"/c devcon.bat {VendorId:X} -r")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                var process = Process.Start(processInfo);
                if (process != null)
                {
                    process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
                    process.BeginOutputReadLine();
                    process.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                    process.Close();
                }

                Logger.Debug($"Connection reset for {VendorId}");
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error($"InvalidOperationException: {ex.Message} {ex.InnerException?.Message}");
            }
            catch (ArgumentNullException ex)
            {
                Logger.Error($"ArgumentNullException: {ex.Message} {ex.InnerException?.Message}");
            }
            catch (FileNotFoundException ex)
            {
                Logger.Error($"FileNotFoundException: {ex.Message} {ex.InnerException?.Message}");
            }
            catch (Win32Exception ex)
            {
                Logger.Error(
                    $"System.ComponentModel.Win32Exception: [{ex.ErrorCode}] {ex.Message} {ex.InnerException?.Message}");
            }
            catch (SystemException ex)
            {
                Logger.Error($"SystemException: {ex.Message} {ex.InnerException?.Message}");
            }
            catch (Exception e)
            {
                Logger.Error($"Exception: {e.Message}");
            }
        }

        /// <inheritdoc />
        public bool InDfuMode => _dfuDriver?.InDfuMode ?? false;

        /// <inheritdoc />
        public bool CanDownload => _dfuDriver?.CanDownload ?? false;

        /// <inheritdoc />
        public bool IsDownloadInProgress => _dfuDriver?.IsDownloadInProgress ?? false;

        /// <inheritdoc />
        public event EventHandler<ProgressEventArgs> DownloadProgressed;

        /// <inheritdoc />
        public async Task<bool> EnterDfuMode()
        {
            if (!CanCommunicate())
            {
                return false;
            }

            if (_dfuDriver == null)
            {
                _dfuDriver = new DfuDriver
                {
                    VendorId = VendorId,
                    ProductId = ProductId,
                    ProductIdDfu = ProductIdDfu
                };
                _dfuDriver.DownloadProgressed += DriverDownloadProgressed;
            }

            if (_dfuDriver.InDfuMode)
            {
                return true;
            }

            OnDeviceDetached(EventArgs.Empty);
            if (await _dfuDriver.EnterDfuMode())
            {
                return true;
            }

            OnDeviceAttached(EventArgs.Empty);
            return false;
        }

        /// <inheritdoc />
        public async Task<bool> ExitDfuMode()
        {
            if (!IsConfigured)
            {
                return false;
            }

            if (_hidDriver?.IsOpen ?? true)
            {
                return false;
            }

            if (_dfuDriver == null)
            {
                return true;
            }

            if (!await _dfuDriver.ExitDfuMode())
            {
                return false;
            }

            _dfuDriver.DownloadProgressed -= DriverDownloadProgressed;
            _dfuDriver.Dispose();
            _dfuDriver = null;

            OnDeviceAttached(EventArgs.Empty);
            return true;
        }

        /// <inheritdoc />
        public async Task<DfuStatus> Download(Stream firmware)
        {
            if (!IsConfigured || (_hidDriver?.IsOpen ?? true))
            {
                return DfuStatus.ErrNotInDfu;
            }

            if (_dfuDriver == null)
            {
                return DfuStatus.ErrArgumentNull;
            }

            return await _dfuDriver.Download(firmware);
        }

        /// <inheritdoc />
        public void AbortDownload()
        {
            if (!IsConfigured)
            {
                return;
            }

            if (_hidDriver?.IsOpen ?? true)
            {
                return;
            }

            _dfuDriver?.AbortDownload();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public void SendMessage(GdsSerializableMessage message, CancellationToken token)
        {
            if (message.ReportId == GdsConstants.ReportId.CardReaderBehaviorResponse)
            {
                OnMessageReceived(new CardReaderBehavior
                {
                    IsPhysical = true,
                    IsEgmControlled = true,
                    ValidationMethod = IdValidationMethods.Host,
                    SupportedTypes = IdReaderTypes.MagneticCard | IdReaderTypes.SmartCard
                });

                return;
            }

            _commandQueue?.Add(message);
        }

        /// <inheritdoc />
        public event EventHandler<GdsSerializableMessage> MessageReceived;

        /// <summary>
        ///     Releases the unmanaged resources used by the Aristocrat.Monaco.Hardware.Usb.UsbCommunicator and optionally
        ///     releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     True to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();

                if (_commandQueue != null)
                {
                    _commandQueue.Dispose();
                    _commandQueue = null;
                }

                if (_hidDriver != null)
                {
                    _hidDriver.ReportReceived -= DriverReportReceived;
                    _hidDriver.DeviceAttached -= DriverAttached;
                    _hidDriver.DeviceDetached -= DriverDetached;
                    _hidDriver.Dispose();
                    _hidDriver = null;
                }

                if (_dfuDriver != null)
                {
                    _dfuDriver.DownloadProgressed -= DriverDownloadProgressed;
                    _dfuDriver.Dispose();
                    _dfuDriver = null;
                }
            }
        }

        /// <summary>Executes the device attached action.</summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected void OnDeviceAttached(EventArgs e)
        {
            DeviceAttached?.Invoke(this, e);
        }

        /// <summary>Executes the device detached action.</summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected void OnDeviceDetached(EventArgs e)
        {
            DeviceDetached?.Invoke(this, e);
        }

        /// <summary>Raises the download progress event.</summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected void OnDownloadProgressed(ProgressEventArgs e)
        {
            DownloadProgressed?.Invoke(this, e);
        }

        private bool CanCommunicate()
        {
            return IsConfigured && IsOpen;
        }

        private void DriverAttached(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    if (!InDfuMode)
                    {
                        OnDeviceAttached(e);
                    }
                });
        }

        private void DriverDetached(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    if (!InDfuMode)
                    {
                        OnDeviceDetached(e);
                    }
                });
        }

        private void DriverReportReceived(object sender, ReportEventArgs e)
        {
            Task.Run(
                () =>
                {
                    if (InDfuMode)
                    {
                        return;
                    }

                    try
                    {
                        var bytes = new byte[e.Buffer.Length + 1];
                        bytes[0] = e.ReportId;
                        Buffer.BlockCopy(e.Buffer, 0, bytes, 1, e.Buffer.Length);
                        var message = GdsSerializableMessage.Deserialize(bytes);

                        // IDataReports have to accumulate into one full message.
                        if (message is IDataReport dataMessage)
                        {
                            // Some data reports come in with Length that includes trailing 00's in the Data,
                            // so trim that Length.
                            while (dataMessage.Data[dataMessage.Length - 1] == 0)
                            {
                                dataMessage.Length--;
                            }

                            if (dataMessage.Index == 1 && _dataReportUnderConstruction == null)
                            {
                                _dataReportUnderConstruction = dataMessage;
                            }
                            else if (_dataReportUnderConstruction != null &&
                                dataMessage.Index == _dataReportUnderConstruction.Index + 1)
                            {
                                _dataReportUnderConstruction.Data += dataMessage.Data.Substring(0, dataMessage.Length);
                                _dataReportUnderConstruction.Length += dataMessage.Length;
                            }
                            else
                            {
                                Logger.Debug("Data report received out-of-sequence");
                                _dataReportUnderConstruction = null;
                            }

                            if (dataMessage.Length < message.MaxPacketSize)
                            {
                                if (_dataReportUnderConstruction != null)
                                {
                                    OnMessageReceived(_dataReportUnderConstruction as GdsSerializableMessage);
                                    _dataReportUnderConstruction = null;
                                }
                                else
                                {
                                    OnMessageReceived(message);
                                }
                            }
                        }
                        else
                        {
                            OnMessageReceived(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug($"Unknown message received: ID={e.Buffer[0]:X2} length={e.Buffer.Length}; {ex}");
                    }
                });
        }

        private void DriverDownloadProgressed(object sender, ProgressEventArgs e)
        {
            Task.Run(
                () =>
                {
                    if (InDfuMode)
                    {
                        OnDownloadProgressed(e);
                    }
                });
        }

        private void OnMessageReceived(GdsSerializableMessage message)
        {
            var invoker = MessageReceived;
            invoker?.Invoke(this, message);
        }

        private static string Packetize(string buffer, int position, int count)
        {
            position = Math.Min(position, buffer.Length);
            return position + count > buffer.Length ? buffer.Substring(position) : buffer.Substring(position, count);
        }

        private void Transmitter()
        {
            var isUicCardReader = Model == "MSR241U Card Reader ";
            GdsSerializableMessage.Initialize(isUicCardReader);

            _transmitter = Task.Run(
                async () =>
                {
                    try
                    {
                        // One device is known to need time between messages.
                        var sendInterval = isUicCardReader ? UicCardReaderSendInterval : 0;

                        while (!_closing)
                        {
                            var message = default(GdsSerializableMessage);
                            if (!_commandQueue?.TryTake(out message, LoopTimeout) ?? false)
                            {
                                continue;
                            }

                            if (message != default(GdsSerializableMessage))
                            {
                                Transmit(message);
                                await Task.Delay(sendInterval);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Transmitter error: {e}");
                    }
                });
        }

        private void Transmit(GdsSerializableMessage message)
        {
            if (message is IDataReport payload)
            {
                var position = 0;
                var index = 0;
                var content = payload.Data;

                // need to allow for equal; last packet must contain less than maxPacketSize characters (so 0-[maxPacketSize-1])
                while (position <= content.Length)
                {
                    var packet = Packetize(content, position, message.MaxPacketSize);
                    payload.Index = ++index;
                    payload.Data = packet;
                    payload.Length = packet.Length;
                    //Logger.Debug($"packetized {payload.Index} [{payload.Data}]");

                    TransmitToDriver(payload as GdsSerializableMessage);

                    position += message.MaxPacketSize;
                }
            }
            else
            {
                TransmitToDriver(message);
            }
        }

        private void TransmitToDriver(GdsSerializableMessage message)
        {
            using (var stream = new MemoryStream())
            {
                try
                {
                    message.Serialize(stream);
                }
                catch (Exception e)
                {
                    Logger.Error($"Transmit: exception serializing object {message}: {e}");
                }

                var command = stream.ToArray();

                _hidDriver.SetFeature(command);
            }
        }
    }
}