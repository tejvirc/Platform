namespace Aristocrat.Monaco.Hardware
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO.Ports;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Serialization;
    using Aristocrat.Monaco.Kernel.Contracts;
    using Contracts;
    using Contracts.Communicator;
    using Contracts.IdReader;
    using Contracts.NoteAcceptor;
    using Contracts.Persistence;
    using Contracts.Printer;
    using Contracts.Reel;
    using Contracts.SharedDevice;
    using Kernel;
    using log4net;
    using NoteAcceptor;
    using Printer;
    using Reel;

    public class HardwareConfiguration : IHardwareConfiguration, IDisposable
    {
        private const string AddinConfigurationsSelected = "Mono.SelectedAddinConfigurationHashCode";
        private const string SupportedDevicesPath = @".\SupportedDevices.xml";
        private const PersistenceLevel Level = PersistenceLevel.Static;
        private const int DefaultBaudRate = 9600;
        private const int ReelControllerInspectionTimeout = 30000;  // Reel controller needs more time to home the reels during inspection

        private readonly Dictionary<string, string> _deviceProtocolUpdateMap = new Dictionary<string, string> { { "Epic950", "EpicTTL" } };

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly TimeSpan InspectionTimeout = TimeSpan.FromSeconds(1);

        private readonly IPersistentStorageAccessor _accessor;
        private readonly IDeviceRegistryService _deviceRegistry;
        private readonly IEventBus _bus;
        private readonly ISystemDisableManager _disableManager;
        private readonly IPropertiesManager _propertiesManager;
        private readonly object _serviceRegistrationLock = new object();

        private bool _disposed;
        private bool _shuttingDown;

        public HardwareConfiguration()
            : this(ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IDeviceRegistryService>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        public HardwareConfiguration(
            IPersistentStorageManager storage,
            IDeviceRegistryService deviceRegistry,
            IEventBus bus,
            ISystemDisableManager disableManager,
            IPropertiesManager propertiesManager)
        {
            if (storage == null)
            {
                throw new ArgumentNullException(nameof(storage));
            }

            _deviceRegistry = deviceRegistry ?? throw new ArgumentNullException(nameof(deviceRegistry));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));

            _accessor = storage.GetAccessor(Level, Name, Enum.GetValues(typeof(DeviceType)).Length);

            SupportedDevices = GetAllDevices();
        }

        public SupportedDevices SupportedDevices { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IReadOnlyCollection<ConfigurationData> GetCurrent()
        {
            var hasProtocolMismatch = false;

            var current = new List<ConfigurationData>();

            var types = Enum.GetValues(typeof(DeviceType));
            foreach (var item in types)
            {
                try
                {
                    var protocolName = (string)_accessor[(int)item, "Protocol"];

                    if (string.IsNullOrEmpty(protocolName))
                    {
                        continue;
                    }

                    if (_deviceProtocolUpdateMap.ContainsKey(protocolName))
                    {
                        var oldProtocolName = protocolName;
                        protocolName = _deviceProtocolUpdateMap[protocolName];
                        hasProtocolMismatch = true;
                        Logger.Info($"Found old protocol name {oldProtocolName} for {(DeviceType)item}, replaced with {protocolName}");
                    }

                    current.Add(new ConfigurationData(
                        (DeviceType)item,
                        (bool)_accessor[(int)item, "Enabled"],
                        (string)_accessor[(int)item, "Make"],
                        protocolName,
                        (string)_accessor[(int)item, "Port"]));
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to create config data", ex);
                }
            }

            if (hasProtocolMismatch)
            {
                _disableManager.Disable(HardwareConstants.HardwareProtocolMismatchDisabledKey, SystemDisablePriority.Immediate, () => Properties.Resources.ProtocolMismatch);
            }

            return current;
        }

        public void Apply(IReadOnlyCollection<ConfigurationData> configuration, bool inspect = true)
        {
            var currentDevices = GetCurrent();
            var inspectedDevices = new List<ConfigurationData>();
            using (var transaction = _accessor.StartTransaction())
            {
                foreach (var config in configuration)
                {
                    var current = currentDevices.FirstOrDefault(
                        d => d.DeviceType == config.DeviceType && d.Enabled == config.Enabled
                                                               && d.Make.Equals(config.Make) &&
                                                               d.Model.Equals(config.Model) &&
                                                               d.Port.Equals(config.Port));
                    if (current == null)
                    {
                        transaction[(int)config.DeviceType, "DeviceType"] = config.DeviceType;
                        transaction[(int)config.DeviceType, "Enabled"] = config.Enabled;
                        transaction[(int)config.DeviceType, "Make"] = config.Make;
                        transaction[(int)config.DeviceType, "Protocol"] = config.Model;
                        transaction[(int)config.DeviceType, "Port"] = config.Port;
                    }
                    else
                    {
                        inspectedDevices.Add(current);
                    }

                }

                transaction.Commit();
            }

            Logger.Info("Device configuration has been updated");

            if(inspect)
            {
                HandleDeviceInspectionAsync(GetCurrent(), inspectedDevices);
            }
        }

        private IComConfiguration Configuration(ConfigurationData data)
        {
            try
            {
                if (!data.Enabled)
                {
                    return null;
                }

                var element = SupportedDevices.Devices.FirstOrDefault(
                    d => d.Type == data.DeviceType.ToString() && d.Name == data.Make && d.Protocol == data.Model);

                if (element == null)
                {
                    return null;
                }

                var config = new ComConfiguration
                {
                    Mode = element.Mode,
                    Protocol = element.Protocol,
                    PortName = data.Port,
                    Name = element.Name,
                    MaxPollTimeouts = _propertiesManager.GetValue(
                        HardwareConstants.MaxFailedPollCount,
                        HardwareConstants.DefaultMaxFailedPollCount)
                };

                SetDeviceData(config, element);

                return config;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                return null;
            }
        }

        public string Name => typeof(HardwareConfiguration).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(IHardwareConfiguration) };

        public void Initialize()
        {
            _bus.Subscribe<PropertyChangedEvent>(
                this,
                _ => HandleDeviceInspectionAsync(GetCurrent()),
                e => e.PropertyName == AddinConfigurationsSelected);
            _bus.Subscribe<ExitRequestedEvent>(this, _ => _shuttingDown = true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _bus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void HandleDeviceInspectionAsync(IEnumerable<ConfigurationData> configuration, IList<ConfigurationData> inspectedDevices = null)
        {
            if (!_shuttingDown)
            {
                Task.Run(() => HandleDeviceInspection(configuration, inspectedDevices));
            }
        }

        private void HandleDeviceInspection(IEnumerable<ConfigurationData> configuration, IList<ConfigurationData> inspectedDevices)
        {
            Parallel.ForEach(configuration, data =>
            {
                IDeviceAdapter adapter = null;

                var inspectedDevice = inspectedDevices?.Any(
                    d => d.DeviceType == data.DeviceType && d.Enabled == data.Enabled
                                                         && d.Make.Equals(data.Make) &&
                                                         d.Model.Equals(data.Model) &&
                                                         d.Port.Equals(data.Port)) ?? false;

                var config = Configuration(data);

                switch (data.DeviceType)
                {
                    case DeviceType.IdReader:
                        lock (_serviceRegistrationLock)
                        {
                            var provider = _deviceRegistry.GetDevice<IIdReaderProvider>();
                            if (provider != null)
                            {
                                if (!inspectedDevice || provider.Adapters.Any(a => !a.Connected))
                                {
                                    adapter = HandleProviderRegistration(provider, config, data.Make);
                                }
                            }
                        }

                        break;
                    case DeviceType.NoteAcceptor:
                        adapter = HandleServiceRegistration<INoteAcceptor>(new NoteAcceptorAdapter(), config, data, inspectedDevice);
                        break;
                    case DeviceType.Printer:
                        adapter = HandleServiceRegistration<IPrinter>(new PrinterAdapter(), config, data, inspectedDevice);
                        break;
                    case DeviceType.ReelController:
                        adapter = HandleServiceRegistration<IReelController>(
                            new ReelControllerAdapter(),
                            config,
                            data,
                            inspectedDevice);
                        break;
                }

                if (adapter != null && (!adapter.Connected || !adapter.Initialized))
                {
                    adapter.ServiceProtocol = config.Protocol;
                    adapter.Initialize();
                    adapter.Inspect(config, data.DeviceType == DeviceType.ReelController ? ReelControllerInspectionTimeout : (int)InspectionTimeout.TotalMilliseconds);
                }
            });
        }

        private static IDeviceAdapter HandleProviderRegistration<T>(IDeviceProvider<T> provider, IComConfiguration configuration, string name)
            where T : IDeviceAdapter
        {
            provider.ClearAdapters();

            return configuration != null ? provider.CreateAdapter(name) : null;
        }

        private IDeviceAdapter HandleServiceRegistration<T>(T service, IComConfiguration configuration, ConfigurationData data, bool inspectedDevice)
            where T : IDeviceAdapter
        {
            lock (_serviceRegistrationLock)
            {
                var current = _deviceRegistry.GetDevice<T>();

                if (inspectedDevice && (current?.Connected ?? false))
                {
                    return current;
                }

                if ((configuration == null && current != null) ||
                    (current != null && (!data.Make.Contains(current.DeviceConfiguration.Manufacturer) ||
                                         !data.Port.Equals(current.DeviceConfiguration.Mode))))
                {
                    _deviceRegistry.RemoveDevice(current);
                    if (configuration == null)
                    {
                        return null;
                    }

                    current = default(T);
                }

                if (configuration != null && current == null)
                {
                    _deviceRegistry.AddDevice(service);

                    return service;
                }

                return current;
            }
        }

        private static SupportedDevices GetAllDevices()
        {
            var settings = new XmlReaderSettings();

            using (var reader = XmlReader.Create(SupportedDevicesPath, settings))
            {
                var serializer = new XmlSerializer(typeof(SupportedDevices));

                return (SupportedDevices)serializer.Deserialize(reader);
            }
        }

        private static void SetDeviceData(IComConfiguration configuration, c_device element)
        {
            for (var i = 0; i < element.Items.Count(); i++)
            {
                switch (element.ItemsElementName[i])
                {
                    case ItemsChoiceType.BaudRate:
                        {
                            if (!string.IsNullOrEmpty(element.Items[i]))
                            {
                                if (!int.TryParse(element.Items[i], NumberStyles.Number, CultureInfo.InvariantCulture, out var baudRate))
                                {
                                    baudRate = DefaultBaudRate;
                                }

                                configuration.BaudRate = baudRate;
                            }

                            break;
                        }

                    case ItemsChoiceType.Parity:
                        if (!string.IsNullOrEmpty(element.Items[i]))
                        {
                            if (Enum.TryParse<Parity>(element.Items[i], out var parity))
                            {
                                configuration.Parity = parity;
                            }
                        }

                        break;
                    case ItemsChoiceType.DataBits:
                        if (!string.IsNullOrEmpty(element.Items[i]))
                        {
                            configuration.DataBits = int.Parse(element.Items[i], CultureInfo.InvariantCulture);
                        }

                        break;
                    case ItemsChoiceType.StopBits:
                        if (!string.IsNullOrEmpty(element.Items[i]))
                        {
                            if (Enum.TryParse<StopBits>(element.Items[i], out var stopBits))
                            {
                                configuration.StopBits = stopBits;
                            }
                        }

                        break;
                    case ItemsChoiceType.Handshake:
                        if (!string.IsNullOrEmpty(element.Items[i]))
                        {
                            if (Enum.TryParse<Handshake>(element.Items[i], out var handshake))
                            {
                                configuration.Handshake = handshake;
                            }
                        }

                        break;
                    case ItemsChoiceType.USBClassGUID:
                        configuration.UsbClassGuid = element.Items[i];
                        break;
                    case ItemsChoiceType.USBVendorId:
                        configuration.UsbDeviceVendorId = element.Items[i];
                        break;
                    case ItemsChoiceType.USBProductId:
                        configuration.UsbDeviceProductId = element.Items[i];
                        break;
                    case ItemsChoiceType.USBProductIdDfu:
                        configuration.UsbDeviceProductIdDfu = element.Items[i];
                        break;
                    default:
                        Logger.Warn($"Parsed unhandled ItemsElementName {element.ItemsElementName[i]}");
                        break;
                }
            }
        }
    }
}