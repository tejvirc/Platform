namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;
    using Application.Helpers;
    using Application.Settings;
    using CommunityToolkit.Mvvm.Input;
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Contracts.Tickets;
    using Hardware.Contracts;
    using Hardware.Contracts.Display;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.Contracts;
    using OperatorMenu;

    /// <summary>
    ///     Contains logic for MachineSettingsPageViewModel.
    /// </summary>
    [CLSCompliant(false)]
    public class MachineSettingsPageViewModel : IdentityPageViewModel
    {
        private const string HardBootTimeKey = "System.HardBoot.Time";
        private const string SoftBootTimeKey = "System.SoftBoot.Time";

        private string _hardBootTime;
        private string _softBootTime;
        private string _ipAddress;
        private string _physicalAddress;
        private string _modelText;

        private string _jurisdiction;
        private string _currencySample;
        private bool _isVisibleForInspection;

        private string _electronics;
        private string _graphicsCard;
        private string _buttonDeck;
        private string _touchScreens;
        private string _lighting;
        private string _noteAcceptorModel;
        private string _printerModel;
        private string _reelController;
        private string _biosVersion;
        private string _fpgaVersion;
        private string _windowsVersion;
        private string _osImageVersion;
        private string _platformVersion;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MachineSettingsPageViewModel" /> class.
        /// </summary>
        public MachineSettingsPageViewModel(bool isWizard) : base(isWizard)
        {
            DefaultPrintButtonEnabled = true;

            VisibilityChangedCommand = new RelayCommand<object>(OnVisibilityChanged);
        }

        /// <summary>
        ///     Gets the command that fires when page unloaded.
        /// </summary>
        public ICommand VisibilityChangedCommand { get; }

        public string IpAddress
        {
            get => _ipAddress;
            set
            {
                if (value != _ipAddress)
                {
                    _ipAddress = value;
                    OnPropertyChanged(nameof(IpAddress));
                }
            }
        }

        public string PhysicalAddress
        {
            get => _physicalAddress;
            set
            {
                if (_physicalAddress == value)
                {
                    return;
                }

                _physicalAddress = value;
                OnPropertyChanged(nameof(PhysicalAddress));
            }
        }

        public string ModelText
        {
            get => _modelText;
            set
            {
                _modelText = value;
                OnPropertyChanged(nameof(ModelText));
            }
        }

        public string Jurisdiction
        {
            get => _jurisdiction;
            set
            {
                _jurisdiction = value;
                OnPropertyChanged(nameof(Jurisdiction));
            }
        }

        public string CurrencySample
        {
            get => _currencySample;
            set
            {
                _currencySample = value;
                OnPropertyChanged(nameof(CurrencySample));
            }
        }

        public bool IsVisibleForInspection
        {
            get => _isVisibleForInspection;
            set
            {
                _isVisibleForInspection = value;
                OnPropertyChanged(nameof(IsVisibleForInspection));
            }
        }

        public string BiosVersion
        {
            get => _biosVersion;
            private set
            {
                if (_biosVersion != value)
                {
                    _biosVersion = value;
                    OnPropertyChanged(nameof(BiosVersion));
                }
            }
        }

        public string FpgaVersion
        {
            get => _fpgaVersion;
            private set
            {
                if (_fpgaVersion != value)
                {
                    _fpgaVersion = value;
                    OnPropertyChanged(nameof(FpgaVersion));
                }
            }
        }

        public string WindowsVersion
        {
            get => _windowsVersion;
            private set
            {
                if (_windowsVersion != value)
                {
                    _windowsVersion = value;
                    OnPropertyChanged(nameof(WindowsVersion));
                }
            }
        }

        public string OsImageVersion
        {
            get => _osImageVersion;
            private set
            {
                if (_osImageVersion != value)
                {
                    _osImageVersion = value;
                    OnPropertyChanged(nameof(OsImageVersion));
                }
            }
        }

        public string PlatformVersion
        {
            get => _platformVersion;
            private set
            {
                if (_platformVersion != value)
                {
                    _platformVersion = value;
                    OnPropertyChanged(nameof(PlatformVersion));
                }
            }
        }

        public string Electronics
        {
            get => _electronics;
            private set
            {
                if (_electronics != value)
                {
                    _electronics = value;
                    OnPropertyChanged(nameof(Electronics));
                }
            }
        }

        public string GraphicsCard
        {
            get => _graphicsCard;
            private set
            {
                if (_graphicsCard != value)
                {
                    _graphicsCard = value;
                    OnPropertyChanged(nameof(GraphicsCard));
                }
            }
        }

        public string ButtonDeck
        {
            get => _buttonDeck;
            private set
            {
                if (_buttonDeck != value)
                {
                    _buttonDeck = value;
                    OnPropertyChanged(nameof(ButtonDeck));
                }
            }
        }

        public string TouchScreens
        {
            get => _touchScreens;
            private set
            {
                if (!_touchScreens?.Equals(value) ?? true)
                {
                    _touchScreens = value;
                    OnPropertyChanged(nameof(TouchScreens));
                }
            }
        }

        public string Lighting
        {
            get => _lighting;
            private set
            {
                if (!_lighting?.Equals(value) ?? true)
                {
                    _lighting = value;
                    OnPropertyChanged(nameof(Lighting));
                }
            }
        }

        public string NoteAcceptorModel
        {
            get => _noteAcceptorModel;
            private set
            {
                if (_noteAcceptorModel != value)
                {
                    _noteAcceptorModel = value;
                    OnPropertyChanged(nameof(NoteAcceptorModel));
                }
            }
        }

        public string PrinterModel
        {
            get => _printerModel;
            private set
            {
                if (_printerModel != value)
                {
                    _printerModel = value;
                    OnPropertyChanged(nameof(PrinterModel));
                }
            }
        }

        public string ReelController
        {
            get => _reelController;
            private set
            {
                if (_reelController != value)
                {
                    _reelController = value;
                    OnPropertyChanged(nameof(ReelController));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the hard boot time value.
        /// </summary>
        public string HardBootTime
        {
            get => _hardBootTime;
            set
            {
                if (_hardBootTime != value)
                {
                    _hardBootTime = value;
                    OnPropertyChanged(nameof(HardBootTime));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the Soft boot time value.
        /// </summary>
        public string SoftBootTime
        {
            get => _softBootTime;
            set
            {
                if (_softBootTime != value)
                {
                    _softBootTime = value;
                    OnPropertyChanged(nameof(SoftBootTime));
                }
            }
        }

        protected override void Loaded()
        {
            base.Loaded();
            UpdateBootTimeInformation();
        }

        /// <summary>
        /// todo cleanup the conflicting existing logic in SaveData() versus base.SaveChanges(), and SerialNumber/AssetNumber, so this can be cleaned up.
        /// </summary>
        protected override void OnUnloaded()
        {
            Unsubscribe();
            SaveData();
            ClearVariableErrors();
            ValidateAll();

            base.OnUnloaded();
        }

        protected override void SetupNavigation()
        {
            if (WizardNavigator != null)
            {
                WizardNavigator.CanNavigateForward = true;
            }
        }

        protected override void SaveChanges()
        {
        }

        protected override void OnInputEnabledChanged()
        {
            OnPropertyChanged(nameof(SerialNumberWarningEnabled));
            OnPropertyChanged(nameof(AssetNumberWarningEnabled));
            SetWarningText();
        }

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            if (dataType != OperatorMenuPrintData.Main)
            {
                return null;
            }

            SaveData(); // Save data before printing so we get current information
            var ticketCreator = ServiceManager.GetInstance()
                .TryGetService<IMachineInfoTicketCreator>();

            return ticketCreator?.Create();
        }

        private void UpdateBootTimeInformation()
        {
            var time = ServiceManager.GetInstance().GetService<ITime>();
            var zone = PropertiesManager.GetValue<TimeZoneInfo>(ApplicationConstants.TimeZoneKey, null);

            var softBootTime = PropertiesManager.GetValue(SoftBootTimeKey, DateTime.UtcNow);
            Logger.Debug($"property manager softBootTime: {softBootTime}");
            var softBootTimeLocalTime = time.GetLocationTime(softBootTime, zone).ToString();
            Logger.Debug($"softBootTimeLocalTime: {softBootTimeLocalTime}");
            SoftBootTime = softBootTimeLocalTime;

            var hardBootTime = PropertiesManager.GetValue(HardBootTimeKey, DateTime.UtcNow);
            Logger.Debug($"property manager hardBootTime: {hardBootTime}");
            var hardBootTimeLocalTime = time.GetLocationTime(hardBootTime, zone).ToString();
            Logger.Debug($"hardBootTimeLocalTime: {hardBootTimeLocalTime}");
            HardBootTime = hardBootTimeLocalTime;
        }

        private void SaveData()
        {
            if (HasErrors)
            {
                LoadVariableData();
                return;
            }

            if (SaveVariableData())
            {
                EventBus.Publish(new OperatorMenuSettingsChangedEvent());
            }
        }

        protected override void LoadVariableData()
        {
            // Left Side
            base.LoadVariableData();

            PhysicalAddress = NetworkInterfaceInfo.DefaultPhysicalAddress;
            IpAddress = NetworkInterfaceInfo.DefaultIpAddress?.ToString();

            // Right Side
            var ioService = ServiceManager.GetInstance().GetService<IIO>();
            Electronics = ioService.GetElectronics();

            GraphicsCard = ServiceManager.GetInstance()
                .GetService<IDisplayService>()
                .GraphicsCard;

            ButtonDeck = MachineSettingsUtilities.GetButtonDeckIdentification(Localizer.For(CultureFor.Operator));

            TouchScreens = MachineSettingsUtilities.GetTouchScreenIdentificationWithoutVbd(Localizer.For(CultureFor.Operator));

            Lighting = MachineSettingsUtilities.GetLightingIdentification(Localizer.For(CultureFor.Operator));

            NoteAcceptorModel = ServiceManager.GetInstance()
                .TryGetService<INoteAcceptor>()
                ?.DeviceConfiguration
                .GetDeviceStatus(false);

            PrinterModel = ServiceManager.GetInstance()
                .TryGetService<IPrinter>()
                ?.DeviceConfiguration
                .GetDeviceStatus(false);

            ReelController = ServiceManager.GetInstance()
                .TryGetService<IReelController>()
                ?.DeviceConfiguration
                .GetDeviceStatus(false);

            BiosVersion = ioService.GetFirmwareVersion(FirmwareData.Bios);
            FpgaVersion = ioService.GetFirmwareVersion(FirmwareData.Fpga);
            ModelText = ioService.DeviceConfiguration.Model;

            IsVisibleForInspection = PropertiesManager.GetValue(KernelConstants.IsInspectionOnly, false);
            Jurisdiction = PropertiesManager.GetValue(ApplicationConstants.JurisdictionKey, string.Empty);
            CurrencySample = PropertiesManager.GetValue(ApplicationConstants.CurrencyDescription, string.Empty);

            var osService = ServiceManager.GetInstance().GetService<IOSService>();

            WindowsVersion = Environment.OSVersion.Version.ToString();
            OsImageVersion = osService.OsImageVersion.ToString();

            PlatformVersion = PropertiesManager.GetValue(KernelConstants.SystemVersion, string.Empty);
        }

        private bool SaveVariableData()
        {
            return (SerialNumber.IsDirty && PropertyHasChanges(SerialNumber.EditedValue, ApplicationConstants.SerialNumber))
                       | (AssetNumber.IsDirty && PropertyHasChanges(AssetNumber.EditedValue, ApplicationConstants.MachineId, (uint)0))
                       | (Area.IsDirty && PropertyHasChanges(Area.EditedValue, ApplicationConstants.Area, true))
                       | (Zone.IsDirty && PropertyHasChanges(Zone.EditedValue, ApplicationConstants.Zone, true))
                       | (Bank.IsDirty && PropertyHasChanges(Bank.EditedValue, ApplicationConstants.Bank, true))
                       | (Position.IsDirty && PropertyHasChanges(Position.EditedValue, ApplicationConstants.Position, true))
                       | (Location.IsDirty && PropertyHasChanges(Location.EditedValue, ApplicationConstants.Location, true))
                       | (DeviceName.IsDirty && PropertyHasChanges(DeviceName.EditedValue, ApplicationConstants.CalculatedDeviceName, true));
        }

        // on an UnLoaded call an attempt to save was made, if they are not saved due to error clear
        // it since the field is reverted to its initial value that was valid
        private void ClearVariableErrors()
        {
            if (PropertyHasErrors(nameof(SerialNumber)))
            {
                ClearErrors(nameof(SerialNumber));
            }
            if (PropertyHasErrors(nameof(AssetNumber)))
            {
                ClearErrors(nameof(AssetNumber));
            }
        }

        private bool PropertyHasChanges(string propertyValue, string settingName, bool blankOk = false) =>
            PropertyHasChanges(propertyValue, settingName, string.Empty, blankOk);

        private bool PropertyHasChanges(string propertyValue, string settingName, object defaultValue, bool blankOk = false)
        {
            bool hasChanges;

            try
            {
                hasChanges = (blankOk || !string.IsNullOrWhiteSpace(propertyValue))
                             && PropertiesManager.GetValue(settingName, defaultValue)?.ToString() != propertyValue;

                if (hasChanges)
                {
                    var valueToSave = (defaultValue is string)
                        ? propertyValue ?? string.Empty
                        : Convert.ChangeType(propertyValue, defaultValue.GetType());

                    PropertiesManager.SetProperty(settingName, valueToSave, true);
                }
            }
            catch (Exception exception) when (exception is InvalidCastException ||
                                              exception is FormatException ||
                                              exception is OverflowException)
            {
                hasChanges = false;
                Logger.Error(
                    $"Tried to convert {settingName} with a value of {propertyValue} to a type of {defaultValue.GetType()} and it failed",
                    exception);
            }

            return hasChanges;
        }

        private void OnVisibilityChanged(object obj)
        {
            UpdateBootTimeInformation();
        }
    }
}
