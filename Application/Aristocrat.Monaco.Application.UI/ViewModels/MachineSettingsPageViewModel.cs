namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Windows.Input;
    using Application.Helpers;
    using Application.Settings;
    using CommunityToolkit.Mvvm.Input;
    using Contracts;
    using Contracts.ConfigWizard;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Contracts.Tickets;
    using Hardware.Contracts;
    using Hardware.Contracts.Display;
    using Hardware.Contracts.HardMeter;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.Contracts;
    using Monaco.Localization.Properties;
    using OperatorMenu;

    /// <summary>
    ///     Contains logic for MachineSettingsPageViewModel.
    /// </summary>
    [CLSCompliant(false)]
    public class MachineSettingsPageViewModel : IdentityPageViewModel
    {
        private const string HardBootTimeKey = "System.HardBoot.Time";
        private const string SoftBootTimeKey = "System.SoftBoot.Time";

        private bool _isVariableDataLoaded;

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
        private string _displays;
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
            set => SetProperty(ref _ipAddress, value, nameof(IpAddress));
        }

        public string PhysicalAddress
        {
            get => _physicalAddress;
            set => SetProperty(ref _physicalAddress, value, nameof(PhysicalAddress));
        }

        public string ModelText
        {
            get => _modelText;
            set => SetProperty(ref _modelText, value, nameof(ModelText));
        }

        public string Jurisdiction
        {
            get => _jurisdiction;
            set => SetProperty(ref _jurisdiction, value, nameof(Jurisdiction));
        }

        public string CurrencySample
        {
            get => _currencySample;
            set => SetProperty(ref _currencySample, value, nameof(CurrencySample));
        }

        public bool IsVisibleForInspection
        {
            get => _isVisibleForInspection;
            set => SetProperty(ref _isVisibleForInspection, value, nameof(IsVisibleForInspection));
        }

        public string BiosVersion
        {
            get => _biosVersion;
            private set => SetProperty(ref _biosVersion, value, nameof(BiosVersion));
        }

        public string FpgaVersion
        {
            get => _fpgaVersion;
            private set => SetProperty(ref _fpgaVersion, value, nameof(FpgaVersion));
        }

        public string WindowsVersion
        {
            get => _windowsVersion;
            private set => SetProperty(ref _windowsVersion, value, nameof(WindowsVersion));
        }

        public string OsImageVersion
        {
            get => _osImageVersion;
            private set => SetProperty(ref _osImageVersion, value, nameof(OsImageVersion));
        }

        public string PlatformVersion
        {
            get => _platformVersion;
            private set => SetProperty(ref _platformVersion, value, nameof(PlatformVersion));
        }

        public string Electronics
        {
            get => _electronics;
            private set => SetProperty(ref _electronics, value, nameof(Electronics));
        }

        public string GraphicsCard
        {
            get => _graphicsCard;
            private set => SetProperty(ref _graphicsCard, value, nameof(GraphicsCard));
        }

        public string ButtonDeck
        {
            get => _buttonDeck;
            private set => SetProperty(ref _buttonDeck, value, nameof(ButtonDeck));
        }

        public string Displays
        {
            get => _displays;
            private set
            {
                if (!_displays?.Equals(value) ?? true)
                {
                    _displays = value;
                    OnPropertyChanged(nameof(Displays));
                }
            }
        }

        public string TouchScreens
        {
            get => _touchScreens;
            private set => SetProperty(ref _touchScreens, value, nameof(TouchScreens));
        }

        public string Lighting
        {
            get => _lighting;
            private set => SetProperty(ref _lighting, value, nameof(Lighting));
        }

        public string NoteAcceptorModel
        {
            get => _noteAcceptorModel;
            private set => SetProperty(ref _noteAcceptorModel, value, nameof(NoteAcceptorModel));
        }

        public string PrinterModel
        {
            get => _printerModel;
            private set => SetProperty(ref _printerModel, value, nameof(PrinterModel));
        }

        public string ReelController
        {
            get => _reelController;
            private set => SetProperty(ref _reelController, value, nameof(ReelController));
        }

        /// <summary>
        ///     Gets or sets the hard boot time value.
        /// </summary>
        public string HardBootTime
        {
            get => _hardBootTime;
            set => SetProperty(ref _hardBootTime, value, nameof(HardBootTime));
        }

        /// <summary>
        ///     Gets or sets the Soft boot time value.
        /// </summary>
        public string SoftBootTime
        {
            get => _softBootTime;
            set => SetProperty(ref _softBootTime, value, nameof(SoftBootTime));
        }

        [CustomValidation(typeof(MachineSettingsPageViewModel), nameof(ValidateHardMeterIsOperational))]
        public bool IsVariableDataLoaded
        {
            get => _isVariableDataLoaded;
            set => SetProperty(ref _isVariableDataLoaded, value, true);
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
                WizardNavigator.CanNavigateForward = !HasErrors;
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
            try
            {
                IsVariableDataLoaded = false;

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

                Displays = MachineSettingsUtilities.GetDisplayIdentifications(Localizer.For(CultureFor.Operator));

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
                if (string.IsNullOrEmpty(BiosVersion))
                {
                    BiosVersion = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable);
                }

                FpgaVersion = ioService.GetFirmwareVersion(FirmwareData.Fpga);
                if (string.IsNullOrEmpty(FpgaVersion))
                {
                    FpgaVersion = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable);
                }

                ModelText = ioService.DeviceConfiguration.Model;

                IsVisibleForInspection = PropertiesManager.GetValue(KernelConstants.IsInspectionOnly, false);
                Jurisdiction = PropertiesManager.GetValue(ApplicationConstants.JurisdictionKey, string.Empty);
                CurrencySample = PropertiesManager.GetValue(ApplicationConstants.CurrencyDescription, string.Empty);

                var osService = ServiceManager.GetInstance().GetService<IOSService>();

                WindowsVersion = Environment.OSVersion.Version.ToString();
                OsImageVersion = osService.OsImageVersion.ToString();

                PlatformVersion = PropertiesManager.GetValue(KernelConstants.SystemVersion, string.Empty);

                ReportVersions();

                if (IsVisibleForInspection)
                {
                    EventBus.Publish(new InspectionResultsChangedEvent(null));
                }
            }
            finally
            {
                IsVariableDataLoaded = true;
            }
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

        private void ReportVersions()
        {
            Inspection?.SetFirmwareVersion($"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ModelLabel)}: {ModelText}");
            Inspection?.SetFirmwareVersion($"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MacAddressLabel)}: {PhysicalAddress}");
            Inspection?.SetFirmwareVersion($"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Electronics)}: {Electronics}");
            Inspection?.SetFirmwareVersion($"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GraphicsCard)}: {GraphicsCard}");
            Inspection?.SetFirmwareVersion($"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BiosVersion)}: {BiosVersion}");
            Inspection?.SetFirmwareVersion($"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.FpgaVersion)}: {FpgaVersion}");
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

        public static ValidationResult ValidateHardMeterIsOperational(bool isLoadingCompleted, ValidationContext context)
        {
            var viewModel = (MachineSettingsPageViewModel)context.ObjectInstance;

            if (!isLoadingCompleted)
            {
                return ValidationResult.Success;
            }

            if (!viewModel.IsVisibleForInspection)
            {
                return ValidationResult.Success;
            }

            if (!viewModel.PropertiesManager.GetValue(HardwareConstants.HardMetersEnabledKey, false))
            {
                return ValidationResult.Success;
            }

            viewModel.Inspection?.SetTestName(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HardMeterLabel));

            if (ServiceManager.GetInstance().GetService<IHardMeter>().IsHardwareOperational)
            {
                return ValidationResult.Success;
            }

            viewModel.Inspection?.ReportTestFailure();

            return new ValidationResult(
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HardMeterError),
                new[] { Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HardMeterLabel) }
            );
        }
    }
}
