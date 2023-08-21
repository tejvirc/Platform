namespace Aristocrat.Monaco.Hardware.EdgeLight.Device
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Contracts;
    using Hardware.Contracts.EdgeLighting;
    using log4net;
    using Packets;
    using Strips;
    using Vgt.Client12.Hardware.HidLibrary;

    internal sealed class EdgeLightDevice : IEdgeLightDevice
    {
        private const int StartBrightnessChannel = 0;
        private const int MaxBrightnessChannel = 4;
        public static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly List<IStrip> _physicalStrips = new List<IStrip>();
        private bool _connected;
        private bool _disposed;
        private bool _gen9Board;

        private IHidDevice _hidDevice;
        private bool _updateBrightness;
        private int _systemBrightness = EdgeLightConstants.MaxChannelBrightness;

        private int _writePacketLength;

        /// <summary>
        ///     The F/W gets the HID device from the HAL layer. So it has injected
        ///     the dependency onto
        /// </summary>
        /// <param name="device">The device that's needs to be opened ( Topper, Main, etc)</param>
        public EdgeLightDevice(IHidDevice device)
        {
            Initialize(device);
        }

        public IReadOnlyList<IStrip> PhysicalStrips => _physicalStrips;

        public BoardIds BoardId { get; set; } = BoardIds.InvalidBoardId;

        public ICollection<EdgeLightDeviceInfo> DevicesInfo => new List<EdgeLightDeviceInfo> { GetDeviceInfo() };

        public void RenderAllStripData()
        {
            _physicalStrips.ForEach(PushData);
            UpdateBrightness();

            void PushData(IStrip strip)
            {
                var chunksToSend = ChannelFrameBase.PrepareChunks(strip.FirmwareId(), strip.ColorBuffer.RgbBytes);
                chunksToSend.ForEach(x => WriteRequest(x));
            }
        }

        public bool LowPowerMode
        {
            set => SetPowerMode(value);
        }

        public event EventHandler<EventArgs> StripsChanged;

        public bool IsOpen
        {
            get => _connected;
            private set
            {
                if (_connected == value)
                {
                    return;
                }

                _connected = value;

                StripsChanged?.Invoke(this, EventArgs.Empty);
                ConnectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler<EventArgs> ConnectionChanged;

        public void Close()
        {
            UnSubscribeBrightnessEvents(_physicalStrips);
            _physicalStrips.Clear();
            IsOpen = false;
            _hidDevice?.CloseDevice();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Close();
            _hidDevice.Dispose();
            _disposed = true;
        }

        public bool CheckForConnection()
        {
            if (!_hidDevice.IsConnected)
            {
                if (!IsOpen)
                {
                    return false;
                }

                Close();
            }

            if (!_hidDevice.IsOpen)
            {
                Open();
            }

            return IsOpen;
        }

        public void NewStripsDiscovered(IEnumerable<IStrip> strips, bool removeOldStrips)
        {
            if (removeOldStrips)
            {
                UnSubscribeBrightnessEvents(_physicalStrips);
                _physicalStrips.Clear();
            }

            var stripList = strips.ToList();
            _physicalStrips.AddRange(stripList);
            SubscribeBrightnessEvents(stripList);
        }

        public void SetSystemBrightness(int brightness)
        {
            if (brightness != _systemBrightness)
            {
                _systemBrightness = brightness < 0 ? 0 :( brightness > 100 ? 100 : brightness);
                _updateBrightness = true;
            }
        }

        private void Open()
        {
            if (IsOpen && _hidDevice.IsConnected && _hidDevice.IsOpen)
            {
                return;
            }

            Close();
            if (!_hidDevice.IsConnected)
            {
                return;
            }

            Logger.Debug("Edge light device inserted.");
            _hidDevice.OpenDevice(
                DeviceMode.Overlapped,
                DeviceMode.Overlapped,
                ShareMode.ShareRead | ShareMode.ShareWrite);
            if (!_hidDevice.IsOpen)
            {
                Logger.Error($"Failed to open hid device {_hidDevice.DevicePath}.");
                return;
            }

            _writePacketLength = _hidDevice.Capabilities.OutputReportByteLength;

            ReadConfigurationFromFirmware();
        }

        private void Initialize(IHidDevice device)
        {
            _hidDevice = device;
        }

        private bool WriteRequest(IRequest request)
        {
            if (_hidDevice.Write(request.Data, 100))
            {
                return true;
            }

            Logger.ErrorFormat("Error writing request {0:X} {1}", request.Type, request.GetType());
            return false;
        }

        private void SetBarkeeperBrightness(int brightness, int stripId)
        {
            brightness = Math.Min(
                brightness,
                EdgeLightConstants.MaxChannelBrightness);

            if (!(CommandFactory.CreateRequest(RequestType.SetBarkeeperLedBrightness) is StripBrightnessControl
                request))
            {
                return;
            }

            request.StripId = (byte)stripId;
            request.Level = (byte)brightness;
            WriteRequest(request);
        }

        private void UpdateBrightness()
        {
            if (!_updateBrightness)
            {
                return;
            }

            var request = CommandFactory.CreateRequest<BrightnessControl>(RequestType.SetDeviceLedBrightness);
            request.SetBrightnessLevel(StartBrightnessChannel, MaxBrightnessChannel, _systemBrightness);
            WriteRequest(request);
            _updateBrightness = false;
        }

        private void SetPowerMode(bool lowPowerMode)
        {
            var request = CommandFactory.CreateRequest<LowPowerMode>(RequestType.SetLowPowerMode);
            request.Control = lowPowerMode;
            WriteRequest(request);
        }

        private void SubscribeBrightnessEvents(List<IStrip> strips)
        {
            strips.ForEach(Subscribe);

            void Subscribe(IStrip strip)
            {
                var handler = strip.IsBarKeeper()
                    ? Barkeeper_BrightnessChanged
                    : (EventHandler<EventArgs>)PhysicalStrip_BrightnessChanged;
                strip.BrightnessChanged += handler;
                handler(strip, EventArgs.Empty);
            }
        }

        private void UnSubscribeBrightnessEvents(List<IStrip> strips)
        {
            strips.ForEach(UnSubscribe);

            void UnSubscribe(IStrip strip)
            {
                var handler = strip.IsBarKeeper()
                    ? Barkeeper_BrightnessChanged
                    : (EventHandler<EventArgs>)PhysicalStrip_BrightnessChanged;
                strip.BrightnessChanged -= handler;
            }
        }

        private void Barkeeper_BrightnessChanged(object sender, EventArgs e)
        {
            if (!(sender is IStrip strip))
            {
                return;
            }

            SetBarkeeperBrightness(strip.Brightness, strip.FirmwareId());
        }

        private void PhysicalStrip_BrightnessChanged(object sender, EventArgs e)
        {
            _updateBrightness = true;
        }

        private void ReadConfigurationFromFirmware()
        {
            var request = CommandFactory.CreateRequest(RequestType.GetLedConfiguration);
            if (!WriteRequest(request) || !HandleResponse())
            {
                return;
            }

            IsOpen = true;
            StripsChanged?.Invoke(this, EventArgs.Empty);
            Logger.Debug("Edge light device opened.");
        }

        private bool HandleResponse()
        {
            var moreData = 1;
            do
            {
                var inboundData = new byte[_writePacketLength];
                var deviceData = _hidDevice.Read(250);
                if (deviceData.Status != HidDeviceData.ReadStatus.Success)
                {
                    Logger.Error($"Failed to read data. Status = {deviceData.Status}");
                    return false;
                }

                Array.Copy(deviceData.Data, inboundData, Math.Min(inboundData.Length, deviceData.Data.Length));
                Logger.Debug($"Inbound data: {inboundData[0]:X}");
                _gen9Board = inboundData[0] == (byte)ResponseType.AlternateLedStripConfigurationWithLocation;
                var response = CommandFactory.CreateResponse(inboundData);
                if (response is null)
                {
                    Logger.Error("Unable to create a response");
                    return false;
                }

                var handler = CommandFactory.CreateHandler(response);
                if (handler is null)
                {
                    Logger.Error("Unable to create a handle");
                    return false;
                }

                handler.Handle(response, this);
                moreData = handler.AdditionalReports + moreData - 1;
            } while (moreData > 0);

            return true;
        }

        private EdgeLightDeviceInfo GetDeviceInfo()
        {
            var info = new EdgeLightDeviceInfo();
            try
            {
                var data = new byte[1];
                _hidDevice?.ReadManufacturer(out data);
                info.Manufacturer = Encoding.Unicode.GetString(data).Trim('\0');

                _hidDevice?.ReadProduct(out data);
                info.Product = Encoding.Unicode.GetString(data).Trim('\0');

                _hidDevice?.ReadSerialNumber(out data);
                info.SerialNumber = Encoding.Unicode.GetString(data).Trim('\0');
                info.Version = _hidDevice?.Attributes.Version ?? 0;
                info.DeviceType = _physicalStrips.FirstOrDefault(x => x.LedCount > 0)?.GetDeviceType(_gen9Board) ?? ElDeviceType.None;
            }
            catch (Exception ex)
            {
                Logger.Error(
                    " Could not find the Manufacturer id" + ex);
            }

            return info;
        }
    }
}