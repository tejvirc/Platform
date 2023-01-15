namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Timers;
    using System.Xml;
    using System.Xml.Serialization;
    using Application.Helpers;
    using ConfigWizard;
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Hardware.Contracts;
    using Hardware.Contracts.HardMeter;
    using Hardware.Contracts.IdReader;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.SerialPorts;
    using Hardware.Contracts.SharedDevice;
    using Helpers;
    using Kernel;
    using Kernel.Contracts;
    using Monaco.Localization.Properties;
    using Monaco.UI.Common.Extensions;
    using MVVM;
    using MVVM.Command;
    using IdReaderInspectionFailedEvent = Hardware.Contracts.IdReader.InspectionFailedEvent;
    using IdReaderInspectionSucceededEvent = Hardware.Contracts.IdReader.InspectedEvent;
    using NoteAcceptorDisconnectedEvent = Hardware.Contracts.NoteAcceptor.DisconnectedEvent;
    using NoteAcceptorInspectionFailedEvent = Hardware.Contracts.NoteAcceptor.InspectionFailedEvent;
    using NoteAcceptorInspectionSucceededEvent = Hardware.Contracts.NoteAcceptor.InspectedEvent;
    using PrinterDisconnectedEvent = Hardware.Contracts.Printer.DisconnectedEvent;
    using PrinterInspectionFailedEvent = Hardware.Contracts.Printer.InspectionFailedEvent;
    using PrinterInspectionSucceededEvent = Hardware.Contracts.Printer.InspectedEvent;
    using ReelInspectedEvent = Hardware.Contracts.Reel.InspectedEvent;
    using ReelInspectionFailedEvent = Hardware.Contracts.Reel.InspectionFailedEvent;
    using Aristocrat.Monaco.Hardware.Contracts.Door;
    using Contracts.TowerLight;
    using Kernel.Contracts.MessageDisplay;
    using Monaco.Common;

    [CLSCompliant(false)]
    public abstract class HardwareConfigBaseViewModel : ConfigWizardViewModelBase
    {
        private const string RS232 = "RS232";
        private const string IncludedDevicesPath = "/Platform/Discovery/Configuration";
        private const string CommunicatorDriversAddinPath = "/Hardware/CommunicatorDrivers";
        private const string HardMetersDeviceType = @"HardMeters";
        private const int DiscoveryTimeoutSeconds = 60;
        private const int MilliSecondsPerSecond = 1000;

        private readonly HardMeterMappingConfiguration _hardMeterMappingConfiguration;
        private readonly IServiceManager _serviceManager;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IHardMeter _hardMeter;

        // This contains all potential devices, even those disabled and not visible on the page
        private readonly Dictionary<DeviceType, DeviceConfigViewModel> _deviceConfigurationDictionary =
            new Dictionary<DeviceType, DeviceConfigViewModel>();

        private readonly Dictionary<DeviceType, bool> _deviceDiscoveryStatus = new Dictionary<DeviceType, bool>
        {
            { DeviceType.NoteAcceptor, false },
            { DeviceType.Printer, false },
            { DeviceType.IdReader, false },
            { DeviceType.ReelController, false }
        };

        private readonly Dictionary<DeviceType, string> _deviceLastValidated = new Dictionary<DeviceType, string>
        {
            { DeviceType.NoteAcceptor, string.Empty },
            { DeviceType.Printer, string.Empty },
            { DeviceType.IdReader, string.Empty },
            { DeviceType.ReelController , string.Empty }
        };

        private readonly DeviceAddinHelper _addinHelper = new DeviceAddinHelper();
        private readonly object _lockObject = new object();

        private readonly IHardwareConfiguration _hardwareConfiguration;

        private Timer _discoveryTimer;
        private bool _isValidating;
        private string _validationStatus;
        private bool _hardMetersEnabled;
        private bool _configurableHardMeters;
        private bool _configurableDoorOpticSensor;
        private bool _bellEnabled;
        private bool _configurableBell;
        private TickValueProperties _tickValue;
        private string _noteAcceptorManufacturer;
        private string _printerManufacturer;
        private string _idReaderManufacturer;
        private string _reelControllerManufacturer;
        private string _hardMeterMapSelection;
        private bool _configurableBellyPanelDoor;
        private bool _bellyPanelDoorEnabled;

        protected Action UpdateChanges;
        protected bool InitialHardMeter;
        protected bool DoorOpticSensorEnabled;

        private bool _configurableTowerLightTierTypes;
        private TowerLightTierTypes _towerLightTierTypeSelection;
        private readonly ConfigWizardConfiguration _configWizardConfiguration;

        protected HardwareConfigBaseViewModel(bool isWizardPage)
            : base(isWizardPage)
        {
            _serviceManager = ServiceManager.GetInstance();
            _hardMeterMappingConfiguration = _serviceManager.GetService<IHardMeterMappingConfigurationProvider>()
                .GetHardMeterMappingConfiguration(() => new HardMeterMappingConfiguration());
            _propertiesManager = _serviceManager.GetService<IPropertiesManager>();
            _hardMeter = _serviceManager.GetService<IHardMeter>();
            _hardwareConfiguration = _serviceManager.GetService<IHardwareConfiguration>();
            _configWizardConfiguration = _serviceManager.GetService<IConfigurationUtilitiesProvider>()
                .GetConfigWizardConfiguration(() => new ConfigWizardConfiguration());

            ValidateCommand = new ActionCommand<object>(
                _ => ValidateConfig(),
                _ =>
                {
                    InitialHardMeter = _hardMetersEnabled;
                    if (!CanValidate && IsWizardPage)
                    {
                        Task.Delay(100).ContinueWith(_ => MvvmHelper.ExecuteOnUI(() => UpdateScreen(true)));
                    }

                    return CanValidate;
                });

            InitialHardMeter = _hardMetersEnabled;

            InitializePage();
        }

        public ObservableCollection<string> HardMeterMapNames { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<Tuple<TowerLightTierTypes, string, bool>> AvailableTowerLightTierTypes { get; } = new();

        public string HardMeterMapSelection
        {
            get => _hardMeterMapSelection;
            set
            {
                _hardMeterMapSelection = value;
                UpdateHardMeterMappingSelection(value);
            }
        }

        public TowerLightTierTypes TowerLightConfigSelection
        {
            get => _towerLightTierTypeSelection;
            set => UpdateTowerLightTypeSelection(value, false);
        }

        /// <summary>
        ///     This contains only the devices visible on the page
        /// </summary>
        public ObservableCollection<DeviceConfigViewModel> Devices { get; set; } =
            new ObservableCollection<DeviceConfigViewModel>();

        public ActionCommand<object> ValidateCommand { get; set; }

        public bool IsValidating
        {
            get => _isValidating;
            set
            {
                if (_isValidating == value)
                {
                    return;
                }

                _isValidating = value;
                RaisePropertyChanged(nameof(IsValidating));
                RaisePropertyChanged(nameof(ConfigurableBell));
                RaisePropertyChanged(nameof(ConfigurableHardMeters));
                RaisePropertyChanged(nameof(ConfigurableDoorOpticSensor));
                RaisePropertyChanged(nameof(ConfigurableBellyPanelDoor));
                MvvmHelper.ExecuteOnUI(() => ValidateCommand.RaiseCanExecuteChanged());

                CheckValidatedStatus();

                EventBus.Publish(new EnableOperatorMenuEvent(!IsValidating));

                if (IsValidating)
                {
                    ValidationStatus = string.Empty;
                }
                else
                {
                    StopTimer();
                }
            }
        }

        public string ValidationStatus
        {
            get => _validationStatus;
            set
            {
                _validationStatus = value;
                RaisePropertyChanged(nameof(ValidationStatus));
            }
        }

        public bool ShowApplyButton => this is HardwareManagerPageViewModel && InputEnabled;

        public bool ShowValidateButton => !(this is HardwareManagerPageViewModel);

        public bool ShowHardMeters { get; private set; }

        public bool ConfigurableHardMeters
        {
            get => _configurableHardMeters && !IsValidating;
            set => _configurableHardMeters = value;
        }

        public bool HardMetersEnabled
        {
            get => _hardMetersEnabled;
            set
            {
                if (_hardMetersEnabled != value)
                {
                    _hardMetersEnabled = value;
                    RaisePropertyChanged(nameof(HardMetersEnabled), nameof(TickValueVisible));
                    UpdateChanges?.Invoke();

                    if (IsWizardPage)
                    {
                        Device_OnPropertyChanged(null, null);
                    }
                }
            }
        }

        public bool VisibleBell { get; private set; }

        public bool BellEnabled
        {
            get => _bellEnabled;
            set
            {
                if (_bellEnabled != value)
                {
                    _bellEnabled = value;
                    RaisePropertyChanged(nameof(BellEnabled));
                    UpdateChanges?.Invoke();

                    if (IsWizardPage)
                    {
                        Device_OnPropertyChanged(null, null);
                    }
                }
            }
        }

        public bool ConfigurableBell
        {
            get => _configurableBell && !IsValidating;
            set => _configurableBell = value;
        }

        public IList<TickValueProperties> TickValues { get; } = new List<TickValueProperties>
        {
            new TickValueProperties { Cents = 1 },
            new TickValueProperties { Cents = 10 },
            new TickValueProperties { Cents = 100 },
            new TickValueProperties { Cents = 1000 },
            new TickValueProperties { Cents = 10000 },
            new TickValueProperties { Cents = 100000 }
        };

        public TickValueProperties TickValue
        {
            get => _tickValue;
            set
            {
                UpdateHardMeterTickValue(value, HardMeterMapSelection);
                SetProperty(ref _tickValue, value, nameof(TickValue));
            }
        }

        public bool TickValueSelectable => IsWizardPage;

        public bool TickValueVisible => HardMetersEnabled && _propertiesManager.GetValue(
            ApplicationConstants.HardMeterTickValueConfigurable,
            false);

        public bool VisibleTowerLightTierTypes { get; private set; }

        public bool ConfigurableTowerLightTierTypes
        {
            get => _configurableTowerLightTierTypes && !IsValidating;
            set => _configurableTowerLightTierTypes = value;
        }

        public bool VisibleDoorOpticSensor { get; private set; }

        public bool ConfigurableDoorOpticSensor
        {
            get => _configurableDoorOpticSensor && !IsValidating;
            set => _configurableDoorOpticSensor = value;
        }

        public bool EnabledDoorOpticSensor
        {
            get => DoorOpticSensorEnabled;
            set => SetProperty(ref DoorOpticSensorEnabled, value, nameof(EnabledDoorOpticSensor));
        }

        public bool VisibleBellyPanelDoor { get; private set; }

        public bool ConfigurableBellyPanelDoor
        {
            get => _configurableBellyPanelDoor && !IsValidating;
            set => _configurableBellyPanelDoor = value;
        }

        public bool BellyPanelDoorEnabled
        {
            get => _bellyPanelDoorEnabled;
            set
            {
                if (_bellyPanelDoorEnabled != value)
                {
                    _bellyPanelDoorEnabled = value;
                    RaisePropertyChanged(nameof(BellyPanelDoorEnabled));
                    UpdateChanges?.Invoke();
                }
            }
        }

        protected virtual void Device_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (EnabledDevices.Any(
                d =>
                {
                    IDevice device = null;
                    return CheckHardware(d, ref device) != true;
                }))
            {
                ValidationStatus = string.Empty;
            }

            CheckValidatedStatus();

            MvvmHelper.ExecuteOnUI(() => ValidateCommand.RaiseCanExecuteChanged());
        }

        protected virtual void UndoSavedChanges()
        {
            // Nothing needed in base method
        }

        protected override void SaveChanges()
        {
            // abstract method
        }

        protected override void InitializeData()
        {
            foreach (var device in Devices.Where(d => d.IsVisible).ToList())
            {
                device.PropertyChanged += Device_OnPropertyChanged;
            }
        }

        protected override void Loaded()
        {
            IsValidating = false;

            SubscribeToEvents();
        }

        protected override void OnUnloaded()
        {
            StopTimer();
            EventBus.UnsubscribeAll(this);
            base.OnUnloaded();
        }

        protected override void OnInputEnabledChanged()
        {
            ValidateCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged(nameof(ShowApplyButton));
        }

        protected override void DisposeInternal()
        {
            foreach (var device in Devices.Where(d => d.IsVisible))
            {
                device.PropertyChanged -= Device_OnPropertyChanged;
            }

            StopTimer();

            base.DisposeInternal();
        }

        protected abstract void CheckValidatedStatus();

        // called from the Validate button handler to save current selections
        protected void SaveCurrentHardwareConfig(IEnumerable<DeviceConfigViewModel> devices = null)
        {
            if (devices == null)
            {
                // Save all devices, not just those enabled, to ensure that devices are properly disabled
                devices = _deviceConfigurationDictionary.Values;
            }

            var config = new List<ConfigurationData>();

            // Write all device configurations in dictionary to storage block.
            foreach (var device in devices)
            {
                var prop = device.DeviceType.GetEnabledPropertyName();
                if (!string.IsNullOrEmpty(prop))
                {
                    PropertiesManager.SetProperty(prop, device.Enabled);
                }

                if (!device.IsVisible)
                {
                    continue;
                }

                var make = device.Manufacturer ?? string.Empty;
                if (!string.IsNullOrEmpty(make))
                {
                    config.Add(
                        new ConfigurationData(
                            device.DeviceType,
                            device.Enabled,
                            device.Manufacturer ?? string.Empty,
                            device.Protocol ?? string.Empty,
                            device.Port ?? string.Empty));
                    if (device.Enabled)
                    {
                        switch (device.DeviceType)
                        {
                            case DeviceType.NoteAcceptor:
                                _propertiesManager.SetProperty(
                                    ApplicationConstants.NoteAcceptorManufacturer,
                                    device.Manufacturer ?? string.Empty);
                                break;
                            case DeviceType.Printer:
                                _propertiesManager.SetProperty(
                                    ApplicationConstants.PrinterManufacturer,
                                    device.Manufacturer ?? string.Empty);
                                break;
                            case DeviceType.IdReader:
                                _propertiesManager.SetProperty(
                                    ApplicationConstants.IdReaderManufacturer,
                                    device.Manufacturer ?? string.Empty);
                                break;
                            case DeviceType.ReelController:
                                _propertiesManager.SetProperty(
                                    ApplicationConstants.ReelControllerManufacturer,
                                    device.Manufacturer ?? string.Empty);
                                break;
                        }
                    }
                }

                if (!_deviceLastValidated[device.DeviceType].Equals(make))
                {
                    _deviceLastValidated[device.DeviceType] = string.Empty;
                }
            }

            _hardwareConfiguration.Apply(config, IsWizardPage);

            _propertiesManager.SetProperty(HardwareConstants.HardMetersEnabledKey, HardMetersEnabled);
            _propertiesManager.SetProperty(ApplicationConstants.ConfigWizardDoorOpticsEnabled, EnabledDoorOpticSensor);
            _propertiesManager.SetProperty(HardwareConstants.BellEnabledKey, BellEnabled);
            _propertiesManager.SetProperty(ApplicationConstants.ConfigWizardBellyPanelDoorEnabled, BellyPanelDoorEnabled);

            CheckBellyPanelDoor();
        }

        private void CheckBellyPanelDoor()
        {
            var disableManager = ServiceManager.GetInstance().GetService<ISystemDisableManager>();
            var door = ServiceManager.GetInstance().GetService<IDoorService>();

            if (BellyPanelDoorEnabled)
            {
                if (disableManager.CurrentDisableKeys.Contains(ApplicationConstants.BellyDoorDiscrepencyGuid))
                    disableManager.Enable(ApplicationConstants.BellyDoorDiscrepencyGuid);
            }
            else
            {
                if (door.GetDoorClosed((int)DoorLogicalId.Belly))
                {
                    disableManager.Disable(
                                ApplicationConstants.BellyDoorDiscrepencyGuid,
                                SystemDisablePriority.Immediate,
                                ResourceKeys.BellyDoorDiscrepancy,
                                CultureProviderType.Operator);

                    ServiceManager.GetInstance().GetService<IEventBus>().Publish(new BellyDoorDiscrepancyEvent());
                }
            }
        }

        internal void SetHardwareStatus(DeviceConfigViewModel config)
        {
            IDevice device = null;
            var validated = CheckHardware(config, ref device);

            if (!string.IsNullOrWhiteSpace(device?.Manufacturer))
            {
                SetDeviceStatusAndValidate(config.DeviceType, device.GetDeviceStatus(), validated);
            }
            else
            {
                SetDeviceStatusAndValidate(config, validated);
            }
        }

        internal IEnumerable<DeviceConfigViewModel> EnabledDevices => Devices.Where(d => d.Enabled);

        // Enable Validate button if not currently validating & any enabled devices have not yet been validated
        private bool CanValidate =>
            !IsValidating && InputEnabled &&
            !EnabledDevices.All(
                div => div.Status.Contains(
                    Localizer.For(CultureFor.Operator)
                        .GetString(ResourceKeys.ConnectedText))) &&
            EnabledDevices.Any(
                d =>
                {
                    IDevice device = null;
                    return CheckHardware(d, ref device) != true;
                });

        private bool Validated => !IsValidating && EnabledDevices.All(
            d =>
            {
                IDevice device = null;
                return CheckHardware(d, ref device);
            });

        private static ConfiguredDevices GetConfiguredDevices()
        {
            var node = MonoAddinsHelper.GetSingleSelectedExtensionNode<FilePathExtensionNode>(IncludedDevicesPath);

            var settings = new XmlReaderSettings();

            using (var reader = XmlReader.Create(node.FilePath, settings))
            {
                var serializer = new XmlSerializer(typeof(ConfiguredDevices));

                return (ConfiguredDevices)serializer.Deserialize(reader);
            }
        }

        private void UpdateHardMeterTickValue(TickValueProperties value, string selection)
        {
            var curMapping = _hardMeterMappingConfiguration.HardMeterMapping.FirstOrDefault(x => x.Name == selection);
            if (curMapping == null)
            {
                return;
            }

            _hardMeter.UpdateTickValues(
                curMapping.HardMeter.ToDictionary(
                    key => key.LogicalId,
                    v => v.TickValueConfigurable ? value.Cents : v.TickValue));

            _propertiesManager.SetProperty(ApplicationConstants.HardMeterTickValue, value.Cents);
        }

        private void UpdateHardMeterMappingSelection(string selection)
        {
            _propertiesManager.SetProperty(ApplicationConstants.HardMeterMapSelectionValue, selection);
            UpdateHardMeterTickValue(_tickValue, selection);
        }

        private void InitializePage()
        {
            LoadPlatformConfigurations();
            LoadDeviceConfigsFromStorage();

            if (IsWizardPage && Validated)
            {
                // Validation has succeeded for all enabled devices
                _validationStatus = Localizer.For(CultureFor.Operator)
                    .GetString(ResourceKeys.HardwareDiscoveryCompleteLabel);
            }

            var value = _propertiesManager.GetValue(ApplicationConstants.HardMeterTickValue, 100L);
            _tickValue = new TickValueProperties { Cents = value };
            if (IsWizardPage)
            {
                var hardMeterMapping =
                    _hardMeterMappingConfiguration.HardMeterMapping.SingleOrDefault(x => x.Default) ??
                    _hardMeterMappingConfiguration.HardMeterMapping.FirstOrDefault();

                if (hardMeterMapping != null)
                {
                    _hardMeter.UpdateTickValues(
                        hardMeterMapping.HardMeter
                            .ToDictionary(x => x.LogicalId, x => x.TickValue));

                    if (_hardMetersEnabled &&
                        _propertiesManager.GetValue(
                            ApplicationConstants.MachineSettingsImported,
                            ImportMachineSettings.None) != ImportMachineSettings.None)
                    {
                        HardMeterMapSelection = _propertiesManager.GetValue(
                            ApplicationConstants.HardMeterMapSelectionValue,
                            ApplicationConstants.HardMeterDefaultMeterMappingName);
                    }
                    else
                    {
                        HardMeterMapSelection = hardMeterMapping.Name;
                    }

                    UpdateHardMeterMappingSelection(HardMeterMapSelection);
                }
            }
            else
            {
                _hardMeterMapSelection = _propertiesManager.GetValue(
                    ApplicationConstants.HardMeterMapSelectionValue,
                    ApplicationConstants.HardMeterDefaultMeterMappingName);
            }

            UpdateTowerLightTypeSelection(null, true);
        }

        private void UpdateTowerLightTypeSelection(TowerLightTierTypes? selection, bool isInitializing)
        {
            if (isInitializing)
            {
                if (IsWizardPage)
                {
                    var @default = AvailableTowerLightTierTypes.FirstOrDefault(x => x.Item3)?.Item1 ??
                                   (AvailableTowerLightTierTypes.FirstOrDefault()?.Item1 ?? TowerLightTierTypes.Undefined);
                    var storedType = _propertiesManager.GetValue(ApplicationConstants.TowerLightTierTypeSelection, @default.ToString());
                    var storedParsed = selection ?? EnumParser.Parse<TowerLightTierTypes>(storedType).Result.GetValueOrDefault();

                    if (@default != TowerLightTierTypes.Undefined && storedParsed == TowerLightTierTypes.Undefined)
                    {
                        _towerLightTierTypeSelection = @default;
                    }
                    else
                    {
                        _towerLightTierTypeSelection = storedParsed;
                    }

                    _propertiesManager.SetProperty(ApplicationConstants.TowerLightTierTypeSelection, _towerLightTierTypeSelection.ToString());
                }
                else
                {
                    var storedType = _propertiesManager.GetValue(ApplicationConstants.TowerLightTierTypeSelection, TowerLightTierTypes.Undefined.ToString());

                    _towerLightTierTypeSelection = EnumParser.Parse<TowerLightTierTypes>(storedType).Result.GetValueOrDefault();
                }
                return;
            }

            if (ConfigurableTowerLightTierTypes)
            {
                _towerLightTierTypeSelection = selection.GetValueOrDefault();
                _propertiesManager.SetProperty(ApplicationConstants.TowerLightTierTypeSelection, _towerLightTierTypeSelection.ToString());
            }
        }


        /// <summary>Subscribe to events.</summary>
        private void SubscribeToEvents()
        {
            var eventBus = EventBus;

            eventBus.Subscribe<ExitRequestedEvent>(this, HandleEvent);

            eventBus.Subscribe<NoteAcceptorInspectionSucceededEvent>(this, HandleEvent);
            eventBus.Subscribe<NoteAcceptorDisconnectedEvent>(this, HandleEvent);
            eventBus.Subscribe<NoteAcceptorInspectionFailedEvent>(this, HandleEvent);

            eventBus.Subscribe<PrinterInspectionSucceededEvent>(this, HandleEvent);
            eventBus.Subscribe<PrinterDisconnectedEvent>(this, HandleEvent);
            eventBus.Subscribe<PrinterInspectionFailedEvent>(this, HandleEvent);

            eventBus.Subscribe<ReelInspectedEvent>(this, HandleEvent);
            eventBus.Subscribe<ReelInspectionFailedEvent>(this, HandleEvent);

            eventBus.Subscribe<IdReaderInspectionSucceededEvent>(this, HandleEvent);
            eventBus.Subscribe<IdReaderInspectionFailedEvent>(this, HandleEvent);
        }

        private void HandleEvent(IEvent e)
        {
            lock (_lockObject)
            {
                if (_discoveryTimer == null || !IsValidating)
                {
                    // Discovery has completed so stop handling events for now
                    return;
                }

                Logger.DebugFormat($"Handling {e.GetType()}");
                var errorText = string.Format(
                    CultureInfo.CurrentCulture,
                    $"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorText)} {e.GetType().Name}");

                switch (e)
                {
                    case ExitRequestedEvent _:
                        StopTimer();
                        break;
                    case NoteAcceptorInspectionSucceededEvent _:
                    {
                        if (_serviceManager.IsServiceAvailable<INoteAcceptor>())
                        {
                            var device = GetDevice<INoteAcceptor>();
                            SetDeviceStatusAndValidate(DeviceType.NoteAcceptor, GetUpdateStatus(device), true);
                        }

                        break;
                    }

                    case PrinterInspectionSucceededEvent _:
                    {
                        if (_serviceManager.IsServiceAvailable<IPrinter>())
                        {
                            var device = GetDevice<IPrinter>();
                            SetDeviceStatusAndValidate(DeviceType.Printer, GetUpdateStatus(device), true);
                        }

                        break;
                    }

                    case ReelInspectionFailedEvent _:
                    {
                        _deviceDiscoveryStatus[DeviceType.ReelController] = false;
                        SetDeviceStatusAndValidate(DeviceType.ReelController, errorText, false);
                        break;
                    }

                    case ReelInspectedEvent _:
                    {
                        if (_serviceManager.IsServiceAvailable<IReelController>())
                        {
                            var device = GetDevice<IReelController>();
                            SetDeviceStatusAndValidate(DeviceType.ReelController, GetUpdateStatus(device), true);
                        }

                        break;
                    }

                    case IdReaderInspectionSucceededEvent ev:
                    {
                        if (_serviceManager.IsServiceAvailable<IIdReaderProvider>())
                        {
                            var provider = _serviceManager.GetService<IIdReaderProvider>();
                            var device = provider.DeviceConfiguration(ev.IdReaderId);
                            if (device == null || device.Manufacturer == null)
                            {
                                break;
                            }

                            if (!device.Manufacturer.Contains(ApplicationConstants.Fake))
                            {
                                _deviceDiscoveryStatus[DeviceType.IdReader] = true;
                            }

                            SetDeviceStatusAndValidate(DeviceType.IdReader, GetUpdateStatus(device), true);
                        }

                        break;
                    }

                    case IdReaderInspectionFailedEvent _:
                        _deviceDiscoveryStatus[DeviceType.IdReader] = false;
                        SetDeviceStatusAndValidate(DeviceType.IdReader, errorText, false);
                        break;

                    case NoteAcceptorDisconnectedEvent _:
                    case NoteAcceptorInspectionFailedEvent _:
                        _deviceDiscoveryStatus[DeviceType.NoteAcceptor] = false;
                        SetDeviceStatusAndValidate(DeviceType.NoteAcceptor, errorText, false);
                        break;

                    case PrinterDisconnectedEvent _:
                    case PrinterInspectionFailedEvent _:
                        _deviceDiscoveryStatus[DeviceType.Printer] = false;
                        SetDeviceStatusAndValidate(DeviceType.Printer, errorText, false);
                        break;

                    default:
                        Logger.ErrorFormat($"Received unexpected event of type {e.GetType()}");
                        break;
                }
            }

            Task.Delay(1000).ContinueWith(_ => CheckDevicesAndUpdate());

            void CheckDevicesAndUpdate()
            {
                // stop the timer if all devices are done with validating
                if (EnabledDevices.All(
                    d =>
                    {
                        IDevice device = null;
                        CheckHardware(d, ref device);

                        return !d.Status.Equals(
                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Validating)) && device != null;
                    }))
                {
                    IsValidating = false;
                    StopTimer();
                }

                MvvmHelper.ExecuteOnUI(() => UpdateScreen());
            }
        }

        private IDevice GetDevice<T>() where T : IDeviceAdapter
        {
            var deviceAdapter = _serviceManager.TryGetService<T>();
            if (deviceAdapter == null)
            {
                Logger.Debug($"Couldn't find a device adapter service for {typeof(T)}");
                return null;
            }

            var device = deviceAdapter.DeviceConfiguration;
            CheckDevice(device, deviceAdapter);

            return device;
        }

        private void CheckDevice<T>(IDevice device, T deviceAdapter) where T : IDeviceAdapter
        {
            if (!device.Manufacturer.Contains(ApplicationConstants.Fake))
            {
                var deviceType = deviceAdapter.DeviceType;
                var deviceAdapterStatus = deviceAdapter.Connected && deviceAdapter.Initialized;
                _deviceDiscoveryStatus[deviceType] = deviceAdapterStatus;

                if (deviceAdapterStatus && _deviceConfigurationDictionary.TryGetValue(deviceType, out var deviceConfig))
                {
                    _deviceLastValidated[deviceType] = deviceConfig.Manufacturer;
                }
                else
                {
                    _deviceLastValidated[deviceType] = string.Empty;
                }
            }
        }

        private string GetUpdateStatus(IDevice device)
        {
            var result = device == null ? string.Empty : device.GetDeviceStatus();
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            ValidateConfig();
            return result;
        }

        private void SetDeviceStatusAndValidate(DeviceType type, string statusText, bool validated)
        {
            if (string.IsNullOrEmpty(statusText))
            {
                return;
            }

            if (_deviceConfigurationDictionary.TryGetValue(type, out var deviceConfig))
            {
                deviceConfig.Status = statusText;
            }

            if (validated)
            {
                _deviceLastValidated[type] = deviceConfig?.Manufacturer;
            }

            CheckValidatedStatus();
        }

        private void SetDeviceStatusAndValidate(DeviceConfigViewModel config, bool validated)
        {
            if (config != null)
            {
                string GetDeviceErrorName()
                {
                    switch (config.DeviceType)
                    {
                        case DeviceType.IdReader:
                            return nameof(IdReaderInspectionFailedEvent);
                        case DeviceType.NoteAcceptor:
                            return nameof(NoteAcceptorInspectionFailedEvent);
                        case DeviceType.Printer:
                            return nameof(PrinterInspectionFailedEvent);
                        case DeviceType.ReelController:
                            return nameof(ReelInspectionFailedEvent);
                    }

                    return string.Empty;
                }

                config.Status = validated
                    ? config.GetDeviceStatus()
                    : string.Format(
                        CultureInfo.CurrentCulture,
                        $"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorText)} {GetDeviceErrorName()}");
            }

            CheckValidatedStatus();
        }

        private bool CheckHardware(DeviceConfigViewModel config, ref IDevice device)
        {
            if (!_deviceLastValidated[config.DeviceType].Equals(string.Empty) &&
                !_deviceLastValidated[config.DeviceType].Equals(config.Manufacturer))
            {
                _deviceDiscoveryStatus[config.DeviceType] = false;
                return false;
            }

            var validated = true;

            void ValidateDevice(IDevice iDevice)
            {
                if (config.Manufacturer.Contains(ApplicationConstants.Fake) &&
                    iDevice != null &&
                    iDevice.Manufacturer.Contains(ApplicationConstants.Fake))
                {
                    return;
                }

                if (config.Manufacturer.Contains(ApplicationConstants.Fake) ||
                    iDevice == null ||
                    !config.Manufacturer.ToLower().Contains(iDevice.Manufacturer.ToLower()) ||
                    !iDevice.Protocol.ToLower().Contains(config.Protocol.ToLower()) ||
                    !iDevice.PortName.ToLower().Contains(config.Port.ToLower()))
                {
                    _deviceDiscoveryStatus[config.DeviceType] = false;
                }

                if (_deviceLastValidated[config.DeviceType].Equals(config.Manufacturer))
                {
                    _deviceDiscoveryStatus[config.DeviceType] = true;
                }

                validated = _deviceDiscoveryStatus[config.DeviceType];
            }

            switch (config.DeviceType)
            {
                case DeviceType.NoteAcceptor:
                    device = GetDevice<INoteAcceptor>();
                    ValidateDevice(device);
                    break;

                case DeviceType.Printer:
                    device = GetDevice<IPrinter>();
                    ValidateDevice(device);
                    break;
                case DeviceType.ReelController:
                    device = GetDevice<IReelController>();
                    ValidateDevice(device);
                    break;

                case DeviceType.IdReader:
                    var idReaderProvider = _serviceManager.TryGetService<IIdReaderProvider>();
                    var adapter = idReaderProvider?.Adapters.FirstOrDefault(
                        d => d.IdReaderType != IdReaderTypes.None);
                    if (adapter != null)
                    {
                        device = idReaderProvider.DeviceConfiguration(adapter.IdReaderId);
                        CheckDevice(device, adapter);
                        ValidateDevice(device);
                    }
                    else
                    {
                        _deviceDiscoveryStatus[config.DeviceType] = false;
                        validated = _deviceDiscoveryStatus[config.DeviceType];
                    }

                    break;
            }

            return validated;
        }

        private void LoadPlatformConfigurations()
        {
            var marketNames = _hardMeterMappingConfiguration.HardMeterMapping?.Select(x => x.Name).ToList() ?? new List<string>();

            HardMeterMapNames.AddRange(marketNames);

            if (!marketNames.Any())
            {
                HardMeterMapNames.Add("Default");
            }

            ConfigureTowerLight();

            var available = GetAvailableDevices();
            if (available?.Devices == null)
            {
                return;
            }

            foreach (var deviceType in _deviceDiscoveryStatus.Keys)
            {
                var deviceConfiguration = new DeviceConfigViewModel(deviceType);

                _deviceConfigurationDictionary.Add(deviceType, deviceConfiguration);
            }

            // Find available ports
            var sortedPorts = new List<string>();
            AddUSBCommunicatorIfAddinDeployed(sortedPorts);

            // Get system serial port names and sort them.
            var sortedSerialPorts = _serviceManager.GetService<ISerialPortsService>()
                .GetAllLogicalPortNames().ToList();

#if DEBUG
            if (!sortedSerialPorts.Any())
            {
                for (var i = 3; i < 16; i++)
                {
                    sortedSerialPorts.Add($"COM{i}");
                }
            }
#endif

            sortedSerialPorts.Sort((a, b) => a.ToPortNumber() - b.ToPortNumber());

            AddRS232CommunicatorAddin(sortedSerialPorts, sortedPorts);

            foreach (var deviceType in _deviceDiscoveryStatus.Keys)
            {
                if (_deviceConfigurationDictionary.TryGetValue(deviceType, out var device))
                {
                    foreach (var port in sortedPorts)
                    {
                        if (!string.IsNullOrEmpty(port) && port != "0") // put fake on last if collection empty
                        {
                            device.AddPort(port);
                        }
                    }
                }
            }

            var configuredDevices = GetConfiguredDevices();

            // Add enabled hardware configurations & set defaults from platform config file
            foreach (var config in available.Devices)
            {
                if (configuredDevices.Excluded.Any(c => c.Type == config.Type))
                {
                    continue;
                }

                if (configuredDevices.Included.Any() && configuredDevices.Included.All(d => d.Name != config.Name))
                {
                    continue;
                }

                if (Enum.TryParse(config.Type, out DeviceType deviceType) &&
                    _deviceConfigurationDictionary.TryGetValue(deviceType, out var device))
                {
                    if (config.Enabled && !config.Type.Contains(ApplicationConstants.Fake))
                    {
                        var deviceDefault = configuredDevices.Defaults.FirstOrDefault(d => d.Name == config.Name);
                        var hasDefaults = deviceDefault != null;
                        var isEnabled = deviceDefault?.Enabled ?? false;
                        var isRequired = false;
                        var canChange = true;

                        switch (deviceType)
                        {
                            case DeviceType.Printer:
                                isRequired = (bool)_propertiesManager.GetProperty(ApplicationConstants.ConfigWizardHardwarePageRequirePrinter, false);
                                break;
                            case DeviceType.ReelController when !IsWizardPage:
                                isRequired = true;
                                canChange = false;
                                break;
                        }

                        if (IsWizardPage &&
                            _propertiesManager.GetValue(
                                ApplicationConstants.MachineSettingsImported,
                                ImportMachineSettings.None) != ImportMachineSettings.None)
                        {
                            switch (deviceType)
                            {
                                case DeviceType.NoteAcceptor:
                                    isEnabled = _propertiesManager.GetValue(
                                        ApplicationConstants.NoteAcceptorEnabled,
                                        false);
                                    if (isEnabled)
                                    {
                                        _noteAcceptorManufacturer = _propertiesManager.GetValue(
                                            ApplicationConstants.NoteAcceptorManufacturer,
                                            string.Empty);
                                    }

                                    break;
                                case DeviceType.Printer:
                                    isEnabled = _propertiesManager.GetValue(
                                        ApplicationConstants.PrinterEnabled,
                                        false);
                                    if (isEnabled)
                                    {
                                        _printerManufacturer = _propertiesManager.GetValue(
                                            ApplicationConstants.PrinterManufacturer,
                                            string.Empty);
                                    }

                                    break;
                                case DeviceType.IdReader:
                                    isEnabled = _propertiesManager.GetValue(
                                        ApplicationConstants.IdReaderEnabled,
                                        false);
                                    if (isEnabled)
                                    {
                                        _idReaderManufacturer = _propertiesManager.GetValue(
                                            ApplicationConstants.IdReaderManufacturer,
                                            string.Empty);
                                    }

                                    break;
                                case DeviceType.ReelController:
                                    isEnabled = _propertiesManager.GetValue(
                                        ApplicationConstants.ReelControllerEnabled,
                                        false);
                                    if (isEnabled)
                                    {
                                        _reelControllerManufacturer = _propertiesManager.GetValue(
                                            ApplicationConstants.ReelControllerManufacturer,
                                            string.Empty);
                                    }

                                    break;
                            }
                        }

                        device.AddPlatformConfiguration(config, hasDefaults, isEnabled, isRequired, canChange);
                    }
                }
            }

            Devices.AddRange(_deviceConfigurationDictionary.Values.Where(d => d.IsVisible));

            ShowHardMeters = configuredDevices.Excluded.All(d => d.Type != HardMetersDeviceType);
            if (ShowHardMeters)
            {
                _hardMetersEnabled = _propertiesManager.GetValue(HardwareConstants.HardMetersEnabledKey, true);
            }

            var configurableHardMeters = _propertiesManager.GetValue(
                ApplicationConstants.ConfigWizardHardMetersConfigConfigurable,
                true);
            var canReconfigureHardMeters = _propertiesManager.GetValue(
                ApplicationConstants.ConfigWizardHardMetersConfigCanReconfigure,
                false);
            ConfigurableHardMeters =
                !IsWizardPage ? configurableHardMeters && canReconfigureHardMeters : configurableHardMeters;

            if (IsWizardPage &&
                _propertiesManager.GetValue(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None) !=
                ImportMachineSettings.None)
            {
                _hardMetersEnabled = _propertiesManager.GetValue(HardwareConstants.HardMetersEnabledKey, false);
            }

            _bellEnabled = _propertiesManager.GetValue(HardwareConstants.BellEnabledKey, false);
            ConfigurableBell = _propertiesManager.GetValue(ApplicationConstants.ConfigWizardBellConfigurable, false);
            VisibleBell = _propertiesManager.GetValue(ApplicationConstants.ConfigWizardBellVisible, false);
            DoorOpticSensorEnabled = _propertiesManager.GetValue(
                ApplicationConstants.ConfigWizardDoorOpticsEnabled,
                false);
            VisibleDoorOpticSensor = _propertiesManager.GetValue(
                ApplicationConstants.ConfigWizardDoorOpticsVisible,
                false);

            var configurableDoorOpticSensor = _propertiesManager.GetValue(
                ApplicationConstants.ConfigWizardDoorOpticsConfigurable,
                false);
            var canReconfigureDoorOpticSensor = _propertiesManager.GetValue(
                ApplicationConstants.ConfigWizardDoorOpticsCanReconfigure,
                false);
            ConfigurableDoorOpticSensor =
                !IsWizardPage
                    ? configurableDoorOpticSensor && canReconfigureDoorOpticSensor
                    : configurableDoorOpticSensor;

            _bellyPanelDoorEnabled = _propertiesManager.GetValue(
                ApplicationConstants.ConfigWizardBellyPanelDoorEnabled,
                false);
            VisibleBellyPanelDoor = _propertiesManager.GetValue(
                ApplicationConstants.ConfigWizardBellyPanelDoorVisible,
                false);
            var configurableBellyPanelDoor = _propertiesManager.GetValue(
                ApplicationConstants.ConfigWizardBellyPanelDoorConfigurable,
                false);
            var canReconfigureBellyPanelDoor = _propertiesManager.GetValue(
                ApplicationConstants.ConfigWizardBellyPanelDoorCanReconfigure,
                false);
            ConfigurableBellyPanelDoor =
                !IsWizardPage
                    ? canReconfigureBellyPanelDoor
                    : configurableBellyPanelDoor;

#if !(RETAIL)
            // Add Fake options last
            foreach (var device in Devices)
            {
                if (!device.Manufacturers.Contains(ApplicationConstants.Fake))
                {
                    Logger.Debug($"adding fake manufacturer for device {device.DeviceType} {device.DeviceName} with protocol {device.Protocol}");
                    device.Manufacturers.Add(ApplicationConstants.Fake);
                }

                if (string.IsNullOrEmpty(device.Protocol))
                {
                    device.Protocol = ApplicationConstants.Fake;
                }

                // *NOTE* No serial ports may be the case for some development laptop configurations.
                if (!device.Ports.Any())
                {
                    device.AddPort(ApplicationConstants.Fake);
                }
            }
#endif
            if (!IsWizardPage || _propertiesManager.GetValue(
                ApplicationConstants.MachineSettingsImported,
                ImportMachineSettings.None) == ImportMachineSettings.None)
            {
                return;
            }

            foreach (var device in Devices.Where(d => d.Enabled))
            {
                switch (device.DeviceType)
                {
                    case DeviceType.NoteAcceptor:
                        device.Manufacturer =
                            device.Manufacturers.FirstOrDefault(m => m.Equals(_noteAcceptorManufacturer));
                        break;
                    case DeviceType.Printer:
                        device.Manufacturer = device.Manufacturers.FirstOrDefault(m => m.Equals(_printerManufacturer));
                        break;
                    case DeviceType.IdReader:
                        device.Manufacturer = device.Manufacturers.FirstOrDefault(m => m.Equals(_idReaderManufacturer));
                        break;
                    case DeviceType.ReelController:
                        device.Manufacturer =
                            device.Manufacturers.FirstOrDefault(m => m.Equals(_reelControllerManufacturer));
                        Logger.Debug($"Reel controller manufacturer set to {device.Manufacturer}");
                        break;
                }
            }
        }

        private void ConfigureTowerLight()
        {
            if (_configWizardConfiguration.TowerLightTierType?.AvailableTowerLightTierType == null
                || !_configWizardConfiguration.TowerLightTierType.AvailableTowerLightTierType.Any())
            {
                Logger.Debug("No configurable Tower Light Tier Type found");
                return;
            }

            foreach (var item in _configWizardConfiguration.TowerLightTierType.AvailableTowerLightTierType)
            {
                var configuredItem = EnumParser.Parse<TowerLightTierTypes>(item.Type);
                if (!configuredItem.IsValid)
                {
                    Logger.Debug($"SignalDefinitions {item.Type} is not valid for {typeof(TowerLightTierTypes)}");
                    continue;
                }

                var tier = configuredItem.Result.GetValueOrDefault();
                var s = Tuple.Create(tier, Aristocrat.Monaco.Common.EnumExtensions.GetDescription(tier, typeof(TowerLightTierTypes)), item.IsDefault);
                AvailableTowerLightTierTypes.Add(s);
            }

            if (!AvailableTowerLightTierTypes.Any())
            {
                return;
            }

            VisibleTowerLightTierTypes = _configWizardConfiguration.TowerLightTierType.Visible;

            var configurableTowerLightConfig = _configWizardConfiguration.TowerLightTierType.Configurable;

            var canReconfigureTowerLightConfig = _configWizardConfiguration.TowerLightTierType.CanReconfigure;

            ConfigurableTowerLightTierTypes =
                !IsWizardPage
                    ? configurableTowerLightConfig && canReconfigureTowerLightConfig
                    : configurableTowerLightConfig;
        }

        private void AddRS232CommunicatorAddin(IEnumerable<string> sortedSerialPorts, List<string> sortedPorts)
        {
            if (_addinHelper.DoesDeviceImplementationExist(CommunicatorDriversAddinPath, RS232))
            {
                sortedPorts.AddRange(sortedSerialPorts);
            }
            else
            {
                // ReSharper disable once InconsistentlySynchronizedField
                Logger.WarnFormat(
                    $"No device implementations were found for {RS232} with extension {CommunicatorDriversAddinPath}");
            }
        }

        private void AddUSBCommunicatorIfAddinDeployed(ICollection<string> sortedPorts)
        {
            if (_addinHelper.DoesDeviceImplementationExist(CommunicatorDriversAddinPath, ApplicationConstants.USB))
            {
                // Add USB as first sorted port.
                sortedPorts.Add(ApplicationConstants.USB);
            }

            else
            {
                // ReSharper disable once InconsistentlySynchronizedField
                Logger.WarnFormat(
                    $"No device implementations were found for {ApplicationConstants.USB} with extension {CommunicatorDriversAddinPath}");
            }
        }

        private void LoadDeviceConfigsFromStorage()
        {
            var current = _hardwareConfiguration.GetCurrent();

            foreach (var device in Devices)
            {
                var config = current.FirstOrDefault(c => c.DeviceType == device.DeviceType);
                if (config == null)
                {
                    continue;
                }

                device.Enabled = config.Enabled;
                device.Manufacturer = config.Make;
                device.Protocol = config.Model;
                device.Port = config.Port;
            }
        }

        private void ValidateConfig(bool saveConfig = true)
        {
            if (EnabledDevices.Any(
                d => d.Port == Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailableText) ||
                     d.Protocol == Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailableText)))
            {
                foreach (var device in EnabledDevices.Where(d => string.IsNullOrEmpty(d.Status)))
                {
                    device.Status = device.Port ==
                                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailableText)
                        ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InvalidPort)
                        : device.Protocol == Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailableText)
                            ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InvalidProtocol)
                            : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotValidated);
                }

                return;
            }

            lock (_lockObject)
            {
                var devices = EnabledDevices.Where(
                    d =>
                    {
                        IDevice device = null;
                        return !CheckHardware(d, ref device);
                    }).ToList();

                foreach (var device in devices)
                {
                    device.Status = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Validating);
                }

                if (saveConfig && !IsValidating)
                {
                    SaveCurrentHardwareConfig();
                }

                IsValidating = true;

                if (devices.Count != 0)
                {
                    StartValidation(devices);
                }
                else
                {
                    MvvmHelper.ExecuteOnUI(() => IsValidating = false);
                    MvvmHelper.ExecuteOnUI(() => UpdateScreen());
                }
            }
        }

        private void StartValidation(IEnumerable<DeviceConfigViewModel> validatingDevices)
        {
            foreach (var device in validatingDevices)
            {
                _deviceDiscoveryStatus[device.DeviceType] = false;
            }

            StartTimer(DiscoveryTimeoutSeconds * MilliSecondsPerSecond);
        }

        private void UpdateScreen(bool clearValidation = false)
        {
            foreach (var deviceConfigVm in EnabledDevices)
            {
                IDevice device = null;
                var result = CheckHardware(deviceConfigVm, ref device);
                if (result && device != null)
                {
                    deviceConfigVm.Status = GetUpdateStatus(device);
                }
            }

            if (clearValidation)
            {
                if (Validated)
                {
                    ValidationStatus = Localizer.For(CultureFor.Operator)
                        .GetString(ResourceKeys.HardwareDiscoveryCompleteLabel);
                }

                return;
            }

            var done = EnabledDevices.All(
                d => d.Status.Contains(
                    Localizer.For(CultureFor.Operator)
                        .GetString(ResourceKeys.ConnectedText)));

            if (done || Validated)
            {
                IsValidating = false;

                // Validation has succeeded for all enabled devices
                ValidationStatus = Localizer.For(CultureFor.Operator)
                    .GetString(ResourceKeys.HardwareDiscoveryCompleteLabel);
                CheckValidatedStatus();
            }

            // Stop listening for device events if we are finished validating
            if (!IsValidating)
            {
                StopTimer();

                if (!done && EnabledDevices.Any(
                    d =>
                    {
                        IDevice device = null;
                        return CheckHardware(d, ref device) != true;
                    }))
                {
                    if (string.IsNullOrEmpty(ValidationStatus))
                    {
                        ValidationStatus = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ValidationFailed);
                    }

                    UndoSavedChanges();
                }
            }
        }

        /// <summary>Starts the discovery timer for given timeout value.</summary>
        /// <param name="timeout">Timeout value in milliseconds.</param>
        private void StartTimer(int timeout)
        {
            _discoveryTimer = new Timer();
            _discoveryTimer.Elapsed += OnDiscoveryTimeout;
            _discoveryTimer.Interval = timeout;
            _discoveryTimer.AutoReset = false;
            _discoveryTimer.Start();
        }

        private void OnDiscoveryTimeout(object sender, ElapsedEventArgs e)
        {
            Logger.WarnFormat(
                CultureInfo.CurrentCulture,
                $"Hardware discovery timed out after {DiscoveryTimeoutSeconds} seconds");

            foreach (var device in EnabledDevices.Where(
                d =>
                {
                    IDevice device = null;
                    return CheckHardware(d, ref device) != true;
                }))
            {
                device.Status = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotValidated);
            }

            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    IsValidating = false;

                    // If any devices have not had successful or failed validation, show timeout warning
                    if (EnabledDevices.Any(
                        d =>
                        {
                            IDevice device = null;
                            return CheckHardware(d, ref device) != true;
                        }))
                    {
                        ValidationStatus = string.Format(
                            CultureInfo.CurrentCulture,
                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.WarningText) + ": " +
                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DiscoveryTimeoutWarning) +
                            " {0} " +
                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Seconds),
                            DiscoveryTimeoutSeconds);
                    }
                });

            MvvmHelper.ExecuteOnUI(() => UpdateScreen());
        }

        private void StopTimer()
        {
            lock (_lockObject)
            {
                IsValidating = false;
                if (_discoveryTimer == null)
                {
                    return;
                }

                _discoveryTimer.Stop();
                _discoveryTimer.Elapsed -= OnDiscoveryTimeout;
                _discoveryTimer.Dispose();
                _discoveryTimer = null;
            }
        }

        private SupportedDevices GetAvailableDevices()
        {
            return _hardwareConfiguration.SupportedDevices;
        }

        public struct TickValueProperties
        {
            public long Cents { get; set; }
        }
    }
}