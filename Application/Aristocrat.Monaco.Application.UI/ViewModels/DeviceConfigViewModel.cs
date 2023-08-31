namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Contracts.Localization;
    using Hardware.Contracts.SerialPorts;
    using Hardware.Contracts.SharedDevice;
    using Helpers;
    using Kernel;
    using Localization;
    using log4net;
    using Monaco.Localization.Properties;
    using Monaco.UI.Common.Extensions;
    using MVVM.ViewModel;
    using DeviceConfiguration = Models.DeviceConfiguration;

    [CLSCompliant(false)]
    public class DeviceConfigViewModel : BaseViewModel
    {
        private new static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ISerialPortsService SerialPortsService = ServiceManager.GetInstance().TryGetService<ISerialPortsService>();

        private readonly DeviceAddinHelper _addinHelper = new DeviceAddinHelper();
        private readonly List<string> _allPorts;
        private readonly DeviceConfiguration _config; // model
        private readonly List<SupportedDevicesDevice> _platformConfigs;
        private readonly HashSet<SupportedDevicesDevice> _fakeConfigs;
        private readonly bool _readOnly;

        private readonly Dictionary<DeviceType, string> _deviceAddInPaths = new Dictionary<DeviceType, string>
        {
            { DeviceType.NoteAcceptor, "/Hardware/NoteAcceptor/NoteAcceptorImplementations"},
            { DeviceType.Printer, "/Hardware/Printer/PrinterImplementations"},
            { DeviceType.ReelController , "/Hardware/ReelController/ReelControllerImplementations" },
            { DeviceType.IdReader, "/Hardware/IdReader/IdReaderImplementations"},
            { DeviceType.CoinAcceptor, "/Hardware/CoinAcceptor/CoinAcceptorImplementations"},
            { DeviceType.Hopper, "/Hardware/Hopper/HopperImplementations"}
        };

        private string _port;
        private string _status;
        private bool _isDetectionComplete;
        private bool _isDetectionFailure;

        public DeviceConfigViewModel(DeviceType type, bool readOnly = false)
        {
            _config = new DeviceConfiguration(false, string.Empty, string.Empty, 0);
            _port = string.Empty;
            _platformConfigs = new List<SupportedDevicesDevice>();
            _fakeConfigs = new HashSet<SupportedDevicesDevice>();
            _status = string.Empty;

            // This will only be true when saving originally persisted devices in Hardware Manager audit menu page
            _readOnly = readOnly;

            DeviceType = type;

            _allPorts = new List<string>();

            Manufacturers = new ObservableCollection<string>();
            Ports = new ObservableCollection<string>();
            Ports.CollectionChanged += Ports_OnCollectionChanged;
        }

        private void Ports_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(Ports));
        }

        public DeviceType DeviceType { get; set; }

        public bool IsVisible => _readOnly || _platformConfigs.Any(c => c.Enabled);

        public string DeviceName
        {
            get
            {
                switch (DeviceType)
                {
                    case DeviceType.IdReader: return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.IdReaderLabel);
                    case DeviceType.NoteAcceptor: return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoteAcceptorLabel);
                    case DeviceType.Printer: return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PrinterLabel);
                    case DeviceType.ReelController: return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReelControllerLabel);
                    case DeviceType.CoinAcceptor: return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CoinAcceptorLabel);
                    case DeviceType.Hopper: return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HopperLabel);
                    default: return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Unknown);
                }
            }
        }

        public bool Enabled
        {
            get => _config.Enabled;
            set
            {
                if (_config.Enabled == value)
                {
                    return;
                }

                _config.Enabled = value;
                RaisePropertyChanged(nameof(Enabled));
                Status = string.Empty;
                StatusType = DeviceState.None;
                IsDetectionComplete = false;
                IsDetectionFailure = false;

                var message = DeviceType + (value ? " enabled" : " disabled");
                Logger.DebugFormat(message);
                RaisePropertyChanged(nameof(ManufacturerEnabled));
                RaisePropertyChanged(nameof(PortEnabled));
                RaisePropertyChanged(nameof(ProtocolEnabled));
                RaisePropertyChanged(nameof(StatusEnabled));

                if (string.IsNullOrEmpty(Manufacturer))
                {
                    Manufacturer = Manufacturers.FirstOrDefault();
                }
            }
        }

        public bool ManufacturerEnabled => Enabled && Manufacturers.Any();

        public string Manufacturer
        {
            get => _config.Manufacturer;
            set
            {
                if (_config.Manufacturer != value)
                {
                    Status = string.Empty;
                    StatusType = DeviceState.None;
                }

                _config.Manufacturer = value ?? string.Empty;
                RaisePropertyChanged(nameof(Manufacturer));

                Logger.DebugFormat($"{DeviceType} Manufacturer {value} selected");
                RaisePropertyChanged(nameof(PortEnabled));
                RaisePropertyChanged(nameof(ProtocolEnabled));
                IsDetectionFailure = false;

                ResetProtocols();
                ResetPortSelections();
            }
        }

        public ObservableCollection<string> Manufacturers { get; set; }

        public bool ProtocolEnabled => Enabled && !Manufacturer.Contains(ApplicationConstants.Fake) &&
                                       !string.IsNullOrEmpty(Manufacturer);

        public string Protocol
        {
            get => _config.Protocol;
            set
            {
                if (_config.Protocol == value)
                {
                    return;
                }

                _config.Protocol = value ?? string.Empty;
                Status = string.Empty;
                StatusType = DeviceState.None;
                RaisePropertyChanged(nameof(Protocol));
                Logger.DebugFormat($"{DeviceType} Protocol {Protocol} selected");

                ResetPortSelections();
            }
        }

        public bool PortEnabled => Enabled && !Manufacturer.Contains(ApplicationConstants.Fake) &&
                                   !string.IsNullOrEmpty(Manufacturer);

        public string Port
        {
            get => _port;
            set
            {
                if ((Protocol == ApplicationConstants.GDS && value != ApplicationConstants.USB) || value == ApplicationConstants.NA)
                {
                    _config.Port = 0;
                }
                else
                {
                    _config.Port = value.ToPortNumber();
                }

                Status = string.Empty;
                StatusType = DeviceState.None;
                SetProperty(ref _port, value, nameof(Port));
                Logger.DebugFormat($"{DeviceType} Port {Port} selected");
            }
        }

        public ObservableCollection<string> Ports { get; }

        public bool StatusEnabled => Enabled;

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value, nameof(Status));
        }

        public DeviceState StatusType { get; set; }

        public string StatusFromType
        {
            get
            {
                switch (StatusType)
                {
                    case DeviceState.None:
                        return string.Empty;
                    case DeviceState.ConnectedText:
                        return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ConnectedText);
                    case DeviceState.InvalidPort:
                        return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InvalidPort);
                    case DeviceState.InvalidProtocol:
                        return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InvalidProtocol);
                    case DeviceState.ConnectedToPortName:
                        return $" {Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ConnectedToText)} {SerialPortsService.PhysicalToLogicalName(Port)}";
                    case DeviceState.DeviceDetected:
                        return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DeviceDetected);
                    case DeviceState.ErrorText:
                        return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorText);
                    case DeviceState.Failed:
                        return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Failed);
                    case DeviceState.FullDeviceSignature:
                        return $"{Manufacturer} {Protocol} {Port}";
                    case DeviceState.HardwareDiscoveryCompleteLabel:
                        return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HardwareDiscoveryCompleteLabel);
                    case DeviceState.InvalidDeviceDetectedTemplate:
                        return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InvalidDeviceDetectedTemplate);
                    case DeviceState.Manufacturer:
                        return Manufacturer;
                    case DeviceState.NoDeviceDetected:
                        return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoDeviceDetected);
                    case DeviceState.NotAvailable:
                        return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailableText);
                    case DeviceState.NotValidated:
                        return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotValidated);
                    case DeviceState.Searching:
                        return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Searching);
                    case DeviceState.Validating:
                        return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Validating);
                    default:
                        return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Error);
                }
            }
        }

        public void AddPlatformConfiguration(SupportedDevicesDevice config, bool defaultConfig, bool enabled = true, bool isRequired = false, bool canChange = true)
        {
            if (!_platformConfigs.Contains(config) && !config.Name.Contains(ApplicationConstants.Fake))
            {
                IsRequired = isRequired;
                CanChange = canChange;
                _platformConfigs.Add(config);
                AddManufacturer(config.Name);

                if (defaultConfig)
                {
                    if (!string.IsNullOrEmpty(Manufacturer))
                    {
                        Logger.Warn($"Multiple default configurations found for {DeviceType} -- using first default");
                    }

                    Manufacturer = config.Name;
                    Enabled = enabled;
                }
            }

            RaisePropertyChanged(nameof(IsVisible));
        }

        public void AddFakeConfiguration(SupportedDevicesDevice config)
        {
            _fakeConfigs.Add(config);
        }

        public void StartDetection()
        {
            IsDetectionComplete = false;
            Status = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Searching);
            StatusType = DeviceState.Searching;
        }

        public bool IsDetectionComplete
        {
            get => _isDetectionComplete;
            set => SetProperty(ref _isDetectionComplete, value, nameof(IsDetectionComplete));
        }

        public bool IsDetectionFailure
        {
            get => _isDetectionFailure;
            set => SetProperty(ref _isDetectionFailure, value, nameof(IsDetectionFailure));
        }

        public bool ContainsPlatformConfiguration(SupportedDevicesDevice config) => _platformConfigs.Contains(config);

        public void SetDetectedPlatformConfiguration(SupportedDevicesDevice config)
        {
            IsDetectionComplete = true;
            IsDetectionFailure = false;
            Manufacturer = config.Name;
            Protocol = config.Protocol;
            Port = config.Port;
            Status = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DeviceDetected);
            StatusType = DeviceState.DeviceDetected;
        }

        public void RefreshProps()
        {
            RaisePropertyChanged(nameof(Manufacturer));
            RaisePropertyChanged(nameof(Protocol));
            RaisePropertyChanged(nameof(Port));
            RaisePropertyChanged(nameof(Ports));
            RaisePropertyChanged(nameof(Status));
            RaisePropertyChanged(nameof(StatusType));
            RaisePropertyChanged(nameof(DeviceName));
            RaisePropertyChanged(nameof(StatusFromType));
        }

        private void AddManufacturer(string manufacturer)
        {
            if (string.IsNullOrEmpty(manufacturer))
            {
                return;
            }

            manufacturer = manufacturer.Trim();
            if (!Manufacturers.Contains(manufacturer))
            {
                Manufacturers.Add(manufacturer);
            }
        }

        public void AddPort(string port)
        {
            if (string.IsNullOrEmpty(port))
            {
                return;
            }

            port = port.Trim();
            if (port != "0" && !_allPorts.Contains(port))
            {
                _allPorts.Add(port);
            }
        }

        public bool IsRequired { get; private set; }

        public bool CanChange { get; private set; }

        private void ResetProtocols()
        {
            if (Manufacturer.Contains(ApplicationConstants.Fake))
            {
                var config = _fakeConfigs.FirstOrDefault(c => c.Name == Manufacturer);
                if (config != null)
                {
                    var protocolExists = CheckIfProtocolExists(config.Protocol);

                    Protocol = protocolExists ? config.Protocol : ApplicationConstants.Fake;
                }
                else
                {
                    Protocol = ApplicationConstants.Fake;
                }
            }
            else
            {
                var config = _platformConfigs.FirstOrDefault(c => c.Name == Manufacturer);
                if (config != null)
                {
                    var protocolExists = CheckIfProtocolExists(config.Protocol);

                    Protocol = protocolExists ? config.Protocol : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailableText);
                }
            }

            bool CheckIfProtocolExists(string protocol)
            {
                return _deviceAddInPaths.ContainsKey(DeviceType) &&
                       _addinHelper.DoesDeviceImplementationExist(
                           _deviceAddInPaths[DeviceType],
                           protocol);
            }
        }

        private void ResetPortSelections()
        {
            Ports.Clear();
            if (Manufacturer.Contains(ApplicationConstants.Fake))
            {
                Port = ApplicationConstants.Fake;
            }
            else
            {
#if RETAIL
// In Retail mode only allow port specified in platform configs as a port choice for this make
                SetPortToConfigOption(true);
#else
                var selectedDevice = _platformConfigs.FirstOrDefault(x => x.Name == Manufacturer);
                if (selectedDevice?.Port == ApplicationConstants.USB)
                {
                    // Only USB should be available for USB devices
                    if (_allPorts.Contains(ApplicationConstants.USB))
                    {
                        Ports.Add(ApplicationConstants.USB);
                        Port = ApplicationConstants.USB;
                    }
                }
                else if (selectedDevice?.Port == ApplicationConstants.NA)
                {
                    Ports.Add(ApplicationConstants.NA);
                    Port = ApplicationConstants.NA;
                }
                else
                {
                    // Only COM ports should be available for Serial devices
                    Ports.AddRange(_allPorts.Where(p => !p.Contains(ApplicationConstants.USB)));
                    SetPortToConfigOption(false);
                }
#endif

                SetDefaultPort();
            }
        }

        private void SetPortToConfigOption(bool addToPortsList)
        {
            var config = _platformConfigs.FirstOrDefault(c => c.Name == Manufacturer);
            if (config != null)
            {
                if (addToPortsList && !Ports.Contains(config.Port))
                {
                    Ports.Add(config.Port);
                }
                Port = config.Port;
            }
        }

        private void SetDefaultPort()
        {
            if (!Ports.Any())
            {
                Ports.Add(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailableText));
                Port = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailableText);
            }
        }
    }
}
