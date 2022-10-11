namespace Aristocrat.Monaco.Hardware.Usb
{
    using Contracts.Communicator;
    using HidSharp;
    using log4net;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using static System.FormattableString;

    /// <summary>A HID driver.</summary>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Usb.IHid" />
    [SuppressMessage("Microsoft.Design", "CA2213", Justification = "_stream is disposed on Close() method")]
    public class HidDriver : IHidDriver
    {
        private const int DefaultExitTimeout = 10000; //milliseconds

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool _closing;

        private HidDevice _device;
        private HidStream _stream;
        private Thread _thread; // thread for reading reports

        /// <summary>
        ///     Identifier of the Vendor
        /// </summary>
        public virtual int VendorId { get; set; }

        /// <summary>
        ///     Identifier of the Product
        /// </summary>
        public virtual int ProductId { get; set; }

        /// <summary>
        ///     string identifying manufacturer of the device
        /// </summary>
        public virtual string Manufacturer => _device?.GetManufacturer() ?? string.Empty;

        /// <summary>
        ///     string identifying model number of the device
        /// </summary>
        public virtual string Model => _device?.GetProductName() ?? string.Empty;

        /// <summary>
        ///     string identifying firmware version of the device
        /// </summary>
        public virtual string Firmware => Invariant($"{_device?.ReleaseNumber ?? new Version()}");

        /// <summary>
        ///     string identifying serial number of the device
        /// </summary>
        public virtual string SerialNumber => _device?.GetSerialNumber();

        /// <summary>
        ///     True if driver is open for HID
        /// </summary>
        public virtual bool IsOpen => _stream != null;

        /// <inheritdoc />
        public int InputReportLength => _device?.GetMaxInputReportLength() ?? 0;

        /// <inheritdoc />
        public int OutputReportLength => _device?.GetMaxOutputReportLength() ?? 0;

        /// <inheritdoc />
        public int FeatureReportLength => _device?.GetMaxFeatureReportLength() ?? 0;

        /// <inheritdoc />
        public event EventHandler<ReportEventArgs> ReportReceived;

        /// <summary>
        /// Raised when device is attached.
        /// </summary>
        public event EventHandler<EventArgs> DeviceAttached;

        /// <summary>
        /// Raised when device is detached.
        /// </summary>
        public event EventHandler<EventArgs> DeviceDetached;

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Initializes a new instance of the Aristocrat.Monaco.Hardware.Usb.HidDriver class.</summary>
        public virtual void Initialize()
        {
            DeviceList.Local.Changed += DevicesChanged;
            _device = DeviceList.Local.GetHidDeviceOrNull(VendorId, ProductId);
        }

        /// <summary>
        /// closes the driver.
        /// </summary>
        /// <returns>True, if close was successful. False, otherwise.</returns>
        public virtual bool Close()
        {
            if (_thread != null)
            {
                _closing = true;
                if (!_thread.Join(DefaultExitTimeout))
                {
#pragma warning disable SYSLIB0006 // Type or member is obsolete
                    _thread.Abort();
#pragma warning restore SYSLIB0006 // Type or member is obsolete
                } 

                _thread = null;
            } 

            _stream?.Dispose();
            _stream = null;
            Logger.Info("Close: closed HID");
            return true;
        }

        /// <summary>
        /// Opens driver for hid communication.
        /// </summary>
        /// <returns>True if open was successful.False, otherwise.</returns>
        public virtual bool Open()
        {
            if (_device == null)
            {
                return false;
            }

            if (_stream != null)
            {
                return true; // already open
            }

            _stream = _device.Open();
            if (_stream == null)
            {
                return false;
            }

            _stream.ReadTimeout = Timeout.Infinite;

            _closing = false;
            Logger.Info("Open: opened HID");

            _thread = new Thread(ReadReports)
            {
                Name = $"{GetType()}.ReportReader",
                IsBackground = true,
            };
            _thread.Start();
            return true;
        }

        /// <inheritdoc/>
        public virtual byte[] GetFeature(byte reportId)
        {
            if (_stream == null)
            {
                return null;
            }

            var report = new byte[FeatureReportLength + 1];
            report[0] = reportId;

            try
            {
                _stream.GetFeature(report);
                //Logger.Debug($"GetFeature: {BitConverter.ToString(report)}");
                return report;
            }
            catch (IOException e)
            {
                Logger.Error($"GetFeature: IOException {e}");
                //var _ = Reset();
                return null;
            }
        }

        /// <inheritdoc />
        public virtual bool SetFeature(byte[] buffer)
        {
            if (_stream == null || buffer == null)
            {
                return false;
            }

            if (buffer.Length > FeatureReportLength || buffer.Length <= 0)
            {
                return false;
            }

            var report = new byte[FeatureReportLength];
            Buffer.BlockCopy(buffer, 0, report, 0, buffer.Length);
            try
            {
                //Logger.Debug($"SetFeature: {BitConverter.ToString(report)}");
                _stream.SetFeature(report);
                return true;
            }
            catch (IOException e)
            {
                Logger.Error($"SetFeature: IOException {e}");
                //var _ = Reset();
                return false;
            }
        }

        /// <inheritdoc />
        public bool SetOutputReport(byte[] buffer)
        {
            try
            {
                _stream?.Write(buffer);
                //Logger.Debug($"SetOutputReport: {BitConverter.ToString(buffer)}");
                return _stream != null;
            }
            catch (IOException e)
            {
                Logger.Error($"SetOutputReport: IOException {e}");
                //var _ = Reset();
                return false;
            }
        }

        /// <inheritdoc />
        public byte[] GetInputReport(byte reportId)
        {
            if (_stream == null)
            {
                return null;
            }

            var report = new byte[InputReportLength + 1];
            report[0] = reportId;

            try
            {
                _stream.Read(report, 0, report.Length);
                //Logger.Debug($"GetInputReport: {BitConverter.ToString(report)}");
                return report;
            }
            catch (IOException e)
            {
                Logger.Error($"GetInputReport: IOException {e}");
                //var _ = Reset();
                return null;
            }
        }

        /// <summary>Raises the <see cref="ReportReceived" /> event.</summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected virtual void OnReportReceived(ReportEventArgs e)
        {
            var invoker = ReportReceived;
            invoker?.Invoke(this, e);
        }

        /// <summary>
        ///     Releases the unmanaged resources used by the Aristocrat.Monaco.Hardware.Usb.HidDriver and optionally releases
        ///     the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     True to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DeviceList.Local.Changed -= DevicesChanged;
                Close();
            }
        }

        /// <summary>Raises the <see cref="DeviceAttached" /> event.</summary>
        protected virtual void OnDeviceAttached()
        {
            var invoker = DeviceAttached;
            invoker?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the <see cref="DeviceDetached" /> event.</summary>
        protected virtual void OnDeviceDetached()
        {
            var invoker = DeviceDetached;
            invoker?.Invoke(this, EventArgs.Empty);
        }

        private void DevicesChanged(object sender, DeviceListChangedEventArgs args)
        {
            var device = DeviceList.Local.GetHidDeviceOrNull(VendorId, ProductId);
            if (device == null && _device != null)
            {
                _device = null;
                OnDeviceDetached();
            }
            else if (device != null && _device == null)
            {
                _device = device;
                OnDeviceAttached();
            }
        }

        private void ReadReports()
        {
            if (_device == null)
            {
                return;
            }

            try
            {
                while (_stream != null && !_closing)
                {
                    var buffer = new byte[InputReportLength];

                    try
                    {
                        var read = _stream.Read(buffer, 0, buffer.Length);
                        //Logger.Debug($"ReadReports: {BitConverter.ToString(buffer)}");

                        OnReportReceived(new ReportEventArgs(buffer, 0, read));
                    }
                    catch (TimeoutException)
                    {
                    }
                }
            }
            catch (IOException e)
            {
                Logger.Error($"ReadReports: IOException {e}");
                //var _ = Reset();
            }
            catch (ObjectDisposedException)
            {
            }
            catch (ThreadAbortException)
            {
            }
        }
    }
}
