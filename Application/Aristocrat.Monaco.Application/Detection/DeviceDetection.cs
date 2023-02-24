namespace Aristocrat.Monaco.Application.Detection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Cabinet.Contracts;
    using Contracts;
    using Contracts.Detection;
    using Hardware.Contracts;
    using Hardware.Contracts.SerialPorts;
    using Kernel;
    using log4net;
    using DeviceType = Hardware.Contracts.SharedDevice.DeviceType;

    public class DeviceDetection : IDeviceDetection
    {
        private const string UsbPort = "USB";
        private const string VidCode = "VID_";
        private const string PidCode = "PID_";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IHardwareConfiguration _hardwareConfiguration;
        private readonly ISerialPortsService _serialPortsService;
        private readonly Dictionary<string, object> _comPortLocks = new ();
        private readonly Dictionary<DeviceType, List<SupportedDevicesDevice>> _usbSupportedDevices = new();
        private readonly Dictionary<DeviceType, List<SupportedDevicesDevice>> _serialSupportedDevices = new();
        private readonly List<(int vid, int pid)> _usbInstances = new ();

        private CancellationTokenSource _tokenSource;

        public DeviceDetection()
            : this(ServiceManager.GetInstance().TryGetService<IEventBus>(),
                  ServiceManager.GetInstance().TryGetService<IHardwareConfiguration>(),
                  ServiceManager.GetInstance().TryGetService<ISerialPortsService>())
        {
        }

        public DeviceDetection(
            IEventBus eventBus,
            IHardwareConfiguration hardwareConfiguration,
            ISerialPortsService serialPortsService)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _hardwareConfiguration = hardwareConfiguration ?? throw new ArgumentNullException(nameof(hardwareConfiguration));
            _serialPortsService = serialPortsService ?? throw new ArgumentNullException(nameof(serialPortsService));

            _comPortLocks.Clear();
            foreach (var port in _serialPortsService.GetAllLogicalPortNames())
            {
                _comPortLocks[port] = new object();
            }

            _usbSupportedDevices.Clear();
            _serialSupportedDevices.Clear();
            foreach (DeviceType type in Enum.GetValues(typeof(DeviceType)))
            {
                _usbSupportedDevices[type] = new ();
                _serialSupportedDevices[type] = new ();
            }
            foreach (var supportedDevice in _hardwareConfiguration.SupportedDevices.Devices)
            {
                if (supportedDevice.Protocol == ApplicationConstants.Fake)
                {
                    continue;
                }

                var deviceType = (DeviceType)Enum.Parse(typeof(DeviceType), supportedDevice.Type);
                if (supportedDevice.Port == UsbPort)
                {
                    _usbSupportedDevices[deviceType].Add(supportedDevice);
                }
                else
                {
                    _serialSupportedDevices[deviceType].Add(supportedDevice);
                }
            }
        }

        public string Name => typeof(OperatorLockupResetService).Name;

        public ICollection<Type> ServiceTypes => new[] { typeof(IDeviceDetection) };

        public void Initialize()
        {
        }

        public void BeginDetection(IEnumerable<DeviceType> deviceTypes)
        {
            Logger.Debug($"BeginDetection of {string.Join(", ", deviceTypes)}");
            _tokenSource = new CancellationTokenSource();

            GetUsbInstances();

            foreach (var deviceType in deviceTypes)
            {
                Task.Run(() => SearchDeviceType(deviceType, _tokenSource), _tokenSource.Token);
            }
        }

        public void CancelDetection()
        {
            Logger.Debug("CancelDetection");
            _tokenSource.Cancel();
        }

        private void SearchDeviceType(DeviceType deviceType, CancellationTokenSource tokenSource)
        {
            try
            {
                if (_usbSupportedDevices[deviceType].Any() && SearchUsbDeviceType(deviceType))
                {
                    return;
                }

                if (_serialSupportedDevices[deviceType].Any() && SearchSerialDeviceType(deviceType, tokenSource))
                {
                    return;
                }

                _eventBus.Publish(new DeviceNotDetectedEvent(deviceType));
            }
            catch (Exception ex)
            {
                Logger.Debug($"{deviceType} search exception: {ex.Message}");
                _eventBus.Publish(new DeviceNotDetectedEvent(deviceType));
            }
        }

        private bool SearchUsbDeviceType(DeviceType deviceType)
        {
            foreach (var (vid, pid) in _usbInstances)
            {
                foreach (var usbDevice in _usbSupportedDevices[deviceType])
                {
                    if (ExtractVidPidFromSupportedDevice(usbDevice) == (vid, pid))
                    {
                        Logger.Debug($"Detected (vid={vid.ToString("X4")}, pid={pid.ToString("X4")})");
                        _eventBus.Publish(new DeviceDetectedEvent(usbDevice));
                        return true;
                    }
                }
            }

            return false;
        }

        private bool SearchSerialDeviceType(DeviceType deviceType, CancellationTokenSource tokenSource)
        {
            // Which serial ports are we going to look at?
            var comPorts = _serialSupportedDevices[deviceType]
                .Where(d => _comPortLocks.ContainsKey(d.Port))
                .Select(d => d.Port)
                .Distinct()
                .ToList();

            if (HardwareFamilyIdentifier.Identify() == HardwareFamily.Unknown)
            {
                comPorts.AddRange(_comPortLocks.Keys);
                comPorts = comPorts.Distinct().ToList();
            }
            Logger.Debug($"{deviceType} ports: {string.Join("/", comPorts.ToArray())}");

            // Which protocols will we try?
            var protocols = _serialSupportedDevices[deviceType]
                .Select(d => d.Protocol)
                .Distinct();
            Logger.Debug($"{deviceType} protocols: {string.Join("/", protocols.ToArray())}");

            foreach (var comPort in comPorts)
            {
                lock (_comPortLocks[comPort])
                {
                    Logger.Debug($"Start {deviceType} {comPort}");
                    foreach (var protocol in protocols)
                    {
                        var serialDevice = SearchSerialProtocolAndPort(deviceType, protocol, comPort, tokenSource);
                        if (serialDevice is not null)
                        {
                            _eventBus.Publish(new DeviceDetectedEvent(serialDevice));
                            return true;
                        }
                    }
                    Logger.Debug($"Finish {deviceType} {comPort}");
                }
            }

            return false;
        }

        private SupportedDevicesDevice SearchSerialProtocolAndPort(DeviceType deviceType, string protocol, string port, CancellationTokenSource tokenSource)
        {
            // TODO:
            // ask the Hardware.Serial library to search for a device
            // that responds to this protocol on the designated port.
            Logger.Debug($"wait on {deviceType} {protocol} {port}");
            Task.Delay(1000, tokenSource.Token).Wait();
            return null;
        }

        private void GetUsbInstances()
        {
            const string SearchScope = "root\\WMI";
            const string UsbSearchQuery = "SELECT * FROM MSWmi_PnPInstanceNames WHERE InstanceName LIKE 'USB%'";

            _usbInstances.Clear();

            try
            {
                var searcher = new ManagementObjectSearcher(SearchScope, UsbSearchQuery);
                foreach (var foundObject in searcher.Get())
                {
                    var instanceName = foundObject["InstanceName"].ToString();
                    if (!instanceName.Contains(VidCode) ||
                        !instanceName.Contains(PidCode))
                    {
                        continue;
                    }

                    (int vid, int pid) = ExtractVidPidFromInstanceName(instanceName);

                    if (_usbInstances.Contains((vid, pid)))
                    {
                        continue;
                    }

                    _usbInstances.Add((vid, pid));
                    Logger.Debug($"Windows sees unique USB InstanceName {instanceName}");
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Error in ManagementObjectSearcher: {ex.Message}");
            }
        }

        private (int vid, int pid) ExtractVidPidFromInstanceName(string instanceName)
        {
            const int codeLength = 4;
            const int baseSixteen = 16;

            var vid = Convert.ToInt32(instanceName.Substring(instanceName.IndexOf(VidCode) + VidCode.Length, codeLength), baseSixteen);
            var pid = Convert.ToInt32(instanceName.Substring(instanceName.IndexOf(PidCode) + PidCode.Length, codeLength), baseSixteen);

            return (vid, pid);
        }

        private (int vid, int pid) ExtractVidPidFromSupportedDevice(SupportedDevicesDevice device)
        {
            const int codeLength = 4;
            const int baseSixteen = 16;

            var vid = -1;
            var pid = -1;

            for (var index = 0; index < device.Items.Length; index++)
            {
                switch (device.ItemsElementName[index])
                {
                    case ItemsChoiceType.USBVendorId:
                        vid = Convert.ToInt32(device.Items[index].Substring(codeLength), baseSixteen);
                        break;
                    case ItemsChoiceType.USBProductId:
                        pid = Convert.ToInt32(device.Items[index].Substring(codeLength), baseSixteen);
                        break;
                    default:
                        break;
                }
            }

            return (vid, pid);
        }
    }
}
