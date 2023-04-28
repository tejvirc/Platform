namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using Contracts;
    using Hardware.Contracts.SharedDevice;
    using Helpers;
    using log4net;
    using Monaco.UI.Common.Extensions;
    using MVVM.ViewModel;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Reflection;
    using Contracts.Localization;
    using Monaco.Localization.Properties;
    using DeviceConfiguration = Models.DeviceConfiguration;

    [CLSCompliant(false)]
    public class DeviceConfigViewModel : BaseViewModel
    {
        private new static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly DeviceAddinHelper _addinHelper = new DeviceAddinHelper();
        private readonly List<string> _allPorts;
        private readonly DeviceConfiguration _config; // model
        private readonly List<SupportedDevicesDevice> _platformConfigs;
        private readonly bool _readOnly;

        private readonly Dictionary<DeviceType, string> _deviceAddInPaths = new Dictionary<DeviceType, string>
        {
            { DeviceType.NoteAcceptor, "/Hardware/NoteAcceptor/NoteAcceptorImplementations"},
            { DeviceType.Printer, "/Hardware/Printer/PrinterImplementations"},
            { DeviceType.ReelController , "/Hardware/ReelController/ReelControllerImplementations" },
            { DeviceType.IdReader, "/Hardware/IdReader/IdReaderImplementations"}
        };

        private string _port;
        private string _status;
        private bool _isDetectionComplete;

        public DeviceConfigViewModel(DeviceType type, bool readOnly = false)
        {
            _config = new DeviceConfiguration(false, string.Empty, string.Empty, 0);
            _port = string.Empty;
            _platformConfigs = new List<SupportedDevicesDevice>();
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
                }

                _config.Manufacturer = value ?? string.Empty;
                RaisePropertyChanged(nameof(Manufacturer));

                Logger.DebugFormat($"{DeviceType} Manufacturer {value} selected");
                RaisePropertyChanged(nameof(PortEnabled));
                RaisePropertyChanged(nameof(ProtocolEnabled));

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
                if (Protocol == ApplicationConstants.GDS && value != ApplicationConstants.USB)
                {
                    _config.Port = 0;
                }
                else
                {
                    _config.Port = value.ToPortNumber();
                }

                Status = string.Empty;
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

        public void StartDetection()
        {
            IsDetectionComplete = false;
            Status = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Searching);
        }

        public bool IsDetectionComplete
        {
            get => _isDetectionComplete;
            set => SetProperty(ref _isDetectionComplete, value, nameof(IsDetectionComplete));
        }

        public bool ContainsPlatformConfiguration(SupportedDevicesDevice config) => _platformConfigs.Contains(config);

        public void SetDetectedPlatformConfiguration(SupportedDevicesDevice config)
        {
            IsDetectionComplete = true;
            Manufacturer = config.Name;
            Protocol = config.Protocol;
            Port = config.Port;
            Status = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DeviceDetected);
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
                Protocol = ApplicationConstants.Fake;
            }
            else
            {
                var config = _platformConfigs.FirstOrDefault(c => c.Name == Manufacturer);
                if (config != null)
                {
                    var protocolExists = _deviceAddInPaths.ContainsKey(DeviceType) &&
                                         _addinHelper.DoesDeviceImplementationExist(
                                             _deviceAddInPaths[DeviceType],
                                             config.Protocol);

                    Protocol = protocolExists ? config.Protocol : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailableText);
                }
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
