namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Accounting.Contracts;
    using Common;
    using Contracts;
    using Contracts.Extensions;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Events;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.SharedDevice;
    using Hardware.Contracts.Ticket;
    using Helpers;
    using Kernel;
    using Monaco.Localization.Properties;
    using Monaco.UI.Common.Extensions;
    using Microsoft.Toolkit.Mvvm.Input;
    //using MVVM.Command;
    using OperatorMenu;

    [CLSCompliant(false)]
    public class PrinterViewModel : DeviceViewModel
    {
        private bool _updateDeviceInformation;
        private bool _formFeedActive;
        private string _selectedPrintLanguage;
        private bool _printLanguageSelectable;
        private bool _printLanguageOverrideIsChecked;
        private readonly string[] _playerTicketSelectableLocales;

        public PrinterViewModel() : base(DeviceType.Printer)
        {
            var playerAvailableLocales = (string[])PropertiesManager.GetProperty(ApplicationConstants.LocalizationPlayerAvailable, new[] { CultureInfo.CurrentCulture.Name });
            PlayerLocalesAvailable = playerAvailableLocales.Length > 1;
            PrintLanguages = new ObservableCollection<Tuple<string, string>>();

            var playerTicketSelectionArrayEntry = new []
            {
                new PlayerTicketSelectionArrayEntry
                {
                    Locale=CultureInfo.CurrentCulture.Name,
                    CurrencyValueLocale=CultureInfo.CurrentCulture.Name,
                    CurrencyWordsLocale=CultureInfo.CurrentCulture.Name
                }
            };

            // maybe use this as original, and make a new property
            _playerTicketSelectableLocales = PropertiesManager.GetValue(ApplicationConstants.LocalizationPlayerTicketSelectable,
                playerTicketSelectionArrayEntry).Select(a => a.Locale).ToArray();

            ShowPrintLanguageSettings = (bool)PropertiesManager.GetProperty(ApplicationConstants.LocalizationPlayerTicketLanguageSettingVisible, false);
            ShowOperatorOverrideCheckBox = (bool)PropertiesManager.GetProperty(ApplicationConstants.LocalizationPlayerTicketLanguageSettingShowCheckBox, false);

            FormFeedButtonCommand = new RelayCommand<object>(FormFeedButtonClicked);
            PrintDiagnosticButtonCommand = new RelayCommand(() => Print(OperatorMenuPrintData.Main, isDiagnostic: true));
            PrintTestTicketCommand = new RelayCommand(() => Print(OperatorMenuPrintData.Custom1, isDiagnostic: true));
            SelfTestClearButtonCommand = new RelayCommand<object>(SelfTestClearNvmButtonClicked);
            SelfTestButtonCommand = new RelayCommand<object>(SelfTestButtonClicked);
        }

        public bool PlayerLocalesAvailable { get; set; }

        /// <summary>
        ///     A collection of (Name, DisplayName) from CultureInfo
        /// </summary>
        public ObservableCollection<Tuple<string, string>> PrintLanguages { get; }

        public string SelectedPrintLanguage
        {
            get => _selectedPrintLanguage;
            set
            {
                _selectedPrintLanguage = value;

                var playerTicketLocale = PropertiesManager.GetValue(
                    ApplicationConstants.LocalizationPlayerTicketLocale,
                    string.Empty);
                var playerTicketCultureInfo = CultureInfo.GetCultureInfo(playerTicketLocale);

                foreach (string locale in _playerTicketSelectableLocales)
                {
                    var localeCultureInfo = CultureInfo.GetCultureInfo(locale);
                    if (localeCultureInfo.DisplayName == _selectedPrintLanguage)
                    {
                        if (!localeCultureInfo.Equals(playerTicketCultureInfo))
                        {
                            PropertiesManager.SetProperty(
                                ApplicationConstants.LocalizationPlayerTicketLocale,
                                localeCultureInfo.Name);

                            TicketCurrencyExtensions.PlayerTicketLocale = localeCultureInfo.Name;

                            break;
                        }
                    }
                }

                RaisePropertyChanged(nameof(SelectedPrintLanguage));
            }
        }

        public bool ShowPrintLanguageSettings { get; }

        public bool ShowOperatorOverrideCheckBox { get; }

        public bool PrintLanguageOverrideIsEnabled => PrintLanguageSelectable && InputEnabled;

        public bool PrintLanguageOverrideIsChecked
        {
            get => _printLanguageOverrideIsChecked;
            set
            {
                _printLanguageOverrideIsChecked = value;
                if (!_printLanguageOverrideIsChecked)
                {
                    var propertyName = PlayerLocalesAvailable
                        ? ApplicationConstants.LocalizationPlayerCurrentCulture
                        : ApplicationConstants.LocalizationPlayerTicketDefault;
                    var playerCurrentCulture = PropertiesManager.GetValue(propertyName, string.Empty);
                    if (!playerCurrentCulture.IsEmpty())
                    {
                        var playerCultureInfo = CultureInfo.GetCultureInfo(playerCurrentCulture);

                        // ReSharper disable once PatternAlwaysOfType
                        if (GetDisplayNameFromName(playerCultureInfo.Name) is string language)
                        {
                            SelectedPrintLanguage = language;

                            TicketCurrencyExtensions.PlayerTicketLocale = playerCultureInfo.Name;
                        }
                    }
                }

                PropertiesManager.SetProperty(ApplicationConstants.LocalizationPlayerTicketOverride, _printLanguageOverrideIsChecked);
                RaisePropertyChanged(nameof(PrintLanguageOverrideIsChecked));
                RaisePropertyChanged(nameof(SelectedPrintLanguageIsEnabled));
            }
        }

        public bool PrintLanguageSelectable
        {
            get => _printLanguageSelectable;
            set
            {
                _printLanguageSelectable = value;
                UpdateEnabledProperties();
            }
        }

        public bool SelectedPrintLanguageIsEnabled => PrintLanguageSelectable && PrintLanguageOverrideIsChecked && InputEnabled;

        public bool Uninitialized => Printer?.LogicalState == PrinterLogicalState.Uninitialized;

        public bool DiagnosticsEnabled { get; set; }

        public new bool TestModeEnabledSupplementary => Printer?.CanPrint ?? false;

        public ICommand FormFeedButtonCommand { get; }

        public ICommand PrintDiagnosticButtonCommand { get; }

        public ICommand PrintTestTicketCommand { get; }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            PrintLanguages.Clear();
            PrintLanguageSelectable = _playerTicketSelectableLocales.Length > 1;

            PrintLanguageOverrideIsChecked = (bool)PropertiesManager.GetProperty(ApplicationConstants.LocalizationPlayerTicketOverride, true);
            foreach (string locale in _playerTicketSelectableLocales)
            {
                var localeCultureInfo = CultureInfo.GetCultureInfo(locale);

                var localizationService = ServiceManager.GetInstance().GetService<ILocalization>();
                var playerTicketCultureProvider = localizationService.GetProvider(CultureFor.PlayerTicket);
                if (playerTicketCultureProvider.IsCultureAvailable(localeCultureInfo))
                {
                    PrintLanguages.Add(new Tuple<string, string>(localeCultureInfo.Name, localeCultureInfo.DisplayName));
                }
                else
                {
                    Logger.Error($"Configuration error {CultureFor.PlayerTicket} locale {localeCultureInfo} not available");
                }
            }

            var playerTicketLocale = (string)PropertiesManager.GetProperty(ApplicationConstants.LocalizationPlayerTicketLocale, string.Empty);
            var playerTicketCultureInfo = CultureInfo.GetCultureInfo(playerTicketLocale);

            // ReSharper disable once PatternAlwaysOfType
            if (GetDisplayNameFromName(playerTicketCultureInfo.Name) is string language)
            {
                SelectedPrintLanguage = language;

                TicketCurrencyExtensions.PlayerTicketLocale = playerTicketCultureInfo.Name;
            }

            SelfTestCurrentState = SelfTestState.None;

            DiagnosticsEnabled = GetGlobalConfigSetting(OperatorMenuSetting.HardwareDiagnosticsEnabled, false);

            SetPortNames();

            UpdateStatus();
            SelfTestCurrentState = SelfTestState.None;
            UpdateScreen();
            UpdateWarningMessage();
        }

        /// <summary>
        ///     Updates the values shown on the operator screen.
        /// </summary>
        protected override void UpdateScreen()
        {
            lock (EventLock)
            {
                if (EventHandlerStopped)
                {
                    return;
                }

                if (ServiceManager.GetInstance().IsServiceAvailable<IPrinter>() == false)
                {
                    return;
                }

                var printer = ServiceManager.GetInstance().GetService<IPrinter>();
                if (printer.DeviceConfiguration is null || string.IsNullOrEmpty(printer.DeviceConfiguration.Protocol))
                {
                    return;
                }

                const string gdsProtocol = "GDS";

                if (DiagnosticsEnabled &&
                    printer.LogicalState != PrinterLogicalState.Uninitialized &&
                    printer.DeviceConfiguration.Protocol == gdsProtocol)
                {
                    ShowDiagnostics = true;
                }

                if (Uninitialized)
                {
                    SetDeviceInformationUnknown();
                }
                else if (_updateDeviceInformation)
                {
                    _updateDeviceInformation = false;
                    SelfTestCurrentState = SelfTestState.None;
                    UpdateStatus();
                }

                SetStateInformation(false);
            }
        }

        protected override void UpdatePrinterButtons()
        {
            RaisePropertyChanged(nameof(TestModeEnabled));
            UpdateWarningMessage();
        }

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            if (dataType == OperatorMenuPrintData.Custom1)
            {
                // Print test ticket with VOID header
                var ticket = VoucherTicketsCreator.CreateDemonstrationCashOutTicket(
                    new VoucherOutTransaction(0, DateTime.Now, 0, AccountType.Cashable, "0", 0, string.Empty), true);
                return new List<Ticket> { ticket };
            }

            if (dataType != OperatorMenuPrintData.Main)
            {
                return null;
            }

            return CreateInformationTicket(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PrinterLabel), GetPrinterDiagnosticInfo());
        }

        protected override void StartEventHandler()
        {
            StartEventHandler(Printer);
        }

        protected override void SubscribeToEvents()
        {
            EventBus.Subscribe<DisabledEvent>(this, HandleEvent);
            EventBus.Subscribe<EnabledEvent>(this, HandleEvent);
            EventBus.Subscribe<InspectedEvent>(this, HandleEvent);
            EventBus.Subscribe<PrintStartedEvent>(this, HandleEvent);
            EventBus.Subscribe<SelfTestPassedEvent>(this, HandleEvent);
            EventBus.Subscribe<SelfTestFailedEvent>(this, HandleEvent);

            EventBus.Subscribe<InspectionFailedEvent>(this, ErrorEvent);
            EventBus.Subscribe<DisconnectedEvent>(this, ErrorEvent);
            EventBus.Subscribe<ConnectedEvent>(this, ErrorClearEvent);
            EventBus.Subscribe<ResolverErrorEvent>(this, ErrorEvent);
            EventBus.Subscribe<TransferStatusEvent>(this, ErrorEvent);
            EventBus.Subscribe<LoadingRegionsAndTemplatesEvent>(this, ErrorClearEvent);

            EventBus.Subscribe<HardwareFaultClearEvent>(this, ClearFault);
            EventBus.Subscribe<HardwareFaultEvent>(this, SetFault);
            EventBus.Subscribe<HardwareWarningClearEvent>(this, ClearWarning);
            EventBus.Subscribe<HardwareWarningEvent>(this, SetWarning);
            EventBus.Subscribe<PrintCompletedEvent>(this, OnPrintCompleted);
        }

        protected override void OnInputStatusChanged()
        {
            UpdateEnabledProperties();
        }

        protected override void OnInputEnabledChanged()
        {
            UpdateEnabledProperties();
        }

        private void UpdateEnabledProperties()
        {
            RaisePropertyChanged(nameof(SelectedPrintLanguageIsEnabled));
            RaisePropertyChanged(nameof(PrintLanguageOverrideIsEnabled));
        }

        private void FormFeedButtonClicked(object obj)
        {
            Logger.Debug("Form Feed btn clicked");

            if (PrinterButtonsEnabled)
            {
                _formFeedActive = true;
                EventBus.Publish(new OperatorMenuPrintJobStartedEvent());
                Task.Run(
                    async () =>
                    {
                        if (Printer != null)
                        {
                            await Printer?.FormFeed();
                            EventBus.Publish(new OperatorMenuPrintJobCompletedEvent());
                            _formFeedActive = false;
                        }
                    });
            }
        }

        private void SelfTestButtonClicked(object obj)
        {
            RunSelfTest(false);
        }

        private void SelfTestClearNvmButtonClicked(object obj)
        {
            RunSelfTest(true);
        }

        /// <summary>Runs the self test</summary>
        /// <param name="clearNvm">True if NVM should be cleared</param>
        private void RunSelfTest(bool clearNvm)
        {
            if (Printer == null)
            {
                Logger.Warn("Printer service unavailable");
                return;
            }

            SelfTestCurrentState = SelfTestState.Running;
            EventBus.Publish(new OperatorMenuPrintJobStartedEvent());
            Printer?.SelfTest(clearNvm);
        }

        private void SetDeviceInformation()
        {
            if (Printer != null)
            {
                SetDeviceInformation(Printer.DeviceConfiguration);
                SetActivationDateTime();
            }
        }

        private string GetDisplayNameFromName(string name)
        {
            return PrintLanguages.FirstOrDefault(language => language.Item1 == name)?.Item2;
        }
        private string GetPrinterDiagnosticInfo()
        {
            var tempString = string.Empty;

            tempString += Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ManufacturerLabel) + ": ";
            tempString += string.IsNullOrEmpty(ManufacturerText)
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable)
                : ManufacturerText;

            tempString += "\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ModelLabel) + ": ";
            tempString += string.IsNullOrEmpty(ModelText)
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable)
                : ModelText;

            tempString += "\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SerialNumberLabel) + ": ";
            tempString += string.IsNullOrEmpty(SerialNumberText)
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable)
                : SerialNumberText;

            tempString += "\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.FirmwareVersionLabel) + ": ";
            tempString += string.IsNullOrEmpty(FirmwareVersionText)
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable)
                : FirmwareVersionText;

            tempString += "\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.FirmwareRevisionLabel) + ": ";
            tempString += string.IsNullOrEmpty(FirmwareRevisionText)
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable)
                : FirmwareRevisionText;

            tempString += "\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.FirmwareCRCLabel) + ": ";
            tempString += string.IsNullOrEmpty(FirmwareCrcText)
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable)
                : FirmwareCrcText;

            tempString += "\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProtocolLabel) + ": ";
            tempString += string.IsNullOrEmpty(ProtocolText)
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable)
                : ProtocolText;

            if (!ProtocolText.Contains(ApplicationConstants.Fake))
            {
                tempString += "\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Port) + ": ";
                tempString += PortText;
            }

            tempString += "\n\n";

            tempString += "\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.StateLabel) + ": ";
            tempString += string.IsNullOrEmpty(StateText)
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable)
                : StateText;

            tempString += "\n\n";
            tempString += Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PlatformVersionLabel) + ": ";
            tempString += Assembly.GetExecutingAssembly().GetName().Version;
            tempString += "\n" + Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OSVersionLabel) + ": ";
            tempString += $"{Environment.OSVersion.Platform}-{Environment.OSVersion.Version}";

            tempString += "\n\n";
            tempString += "Aristocrat Technologies, INC.";
            return tempString;
        }

        private void SetPortNames()
        {
            if (Printer != null)
            {
                SetPortNames(Printer.DeviceConfiguration, out _);
            }
        }

        private void SetStateInformation(bool updateStatus)
        {
            var printer = ServiceManager.GetInstance().TryGetService<IPrinter>();

            var logicalState = printer?.LogicalState ?? PrinterLogicalState.Disabled;

            StateText = logicalState.ToString();

            switch (logicalState)
            {
                case PrinterLogicalState.Disabled:
                case PrinterLogicalState.Disconnected:
                    StateCurrentMode = StateMode.Error;
                    StatusCurrentMode = StatusMode.Error;
                    break;
                case PrinterLogicalState.Uninitialized:
                    StateCurrentMode = StateMode.Uninitialized;
                    StatusCurrentMode = StatusMode.Error;
                    break;
                case PrinterLogicalState.Inspecting:
                    StateCurrentMode = StateMode.Processing;
                    break;
                case PrinterLogicalState.Printing:
                    StateCurrentMode = StateMode.Processing;
                    StatusCurrentMode = StatusMode.Working;
                    break;
                default:
                    StateCurrentMode = StateMode.Normal;
                    break;
            }

            if (updateStatus)
            {
                StatusText = StatusCurrentMode != StatusMode.None && StateCurrentMode != StateMode.Normal
                    ? StatusCurrentMode.ToString()
                    : string.Empty;
            }
        }

        /// <summary>Sets last error status.</summary>
        /// <param name="faults">The faults.</param>
        /// <param name="warnings">The warnings.</param>
        /// <returns>true if last error status set; false otherwise.</returns>
        private bool SetLastErrorStatus(PrinterFaultTypes faults, PrinterWarningTypes warnings)
        {
            foreach (PrinterFaultTypes value in Enum.GetValues(typeof(PrinterFaultTypes)))
            {
                if (!faults.HasFlag(value))
                {
                    continue;
                }

                if (PrinterEventsDescriptor.FaultTexts.ContainsKey(value))
                {
                    UpdateStatusError(PrinterEventsDescriptor.FaultTexts[value], false);
                }
            }

            foreach (PrinterWarningTypes value in Enum.GetValues(typeof(PrinterFaultTypes)))
            {
                if (!warnings.HasFlag(value))
                {
                    continue;
                }

                if (PrinterEventsDescriptor.WarningTexts.ContainsKey(value))
                {
                    UpdateStatusError(PrinterEventsDescriptor.WarningTexts[value], false);
                }
            }

            if (string.IsNullOrEmpty(StatusText))
            {
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoErrors);
            }

            return true;
        }

        private void SetActivationDateTime()
        {
            var visibility = GetConfigSetting(OperatorMenuSetting.ActivationTimeVisible, false);
            if (visibility)
            {
                var time = ServiceManager.GetInstance().GetService<ITime>();
                ActivationVisible = true;
                var activationTime = Printer?.ActivationTime ?? DateTime.MinValue;
                ActivationTime = activationTime == DateTime.MinValue
                    ? string.Empty
                    : time.GetLocationTime(activationTime).ToString(CultureInfo.CurrentCulture);
            }
            else
            {
                ActivationVisible = false;
            }
        }

        private void UpdateStatusErrorMode()
        {
            if (StatusCurrentMode == StatusMode.Error)
            {
                return;
            }

            var temp = StatusText;

            var containsWarningText = PrinterEventsDescriptor.WarningTexts
                .Any(pair => !string.IsNullOrEmpty(pair.Value) && temp.Contains(pair.Value));

            if (containsWarningText)
            {
                StatusCurrentMode = StatusMode.Warning;
            }

            var containsFaultTexts = PrinterEventsDescriptor.FaultTexts
                .Any(pair => !string.IsNullOrEmpty(pair.Value) && temp.Contains(pair.Value));

            if (containsFaultTexts)
            {
                StatusCurrentMode = StatusMode.Error;
            }
        }

        private void UpdateStatus()
        {
            if (ServiceManager.GetInstance().IsServiceAvailable<IPrinter>())
            {
                var printer = ServiceManager.GetInstance().GetService<IPrinter>();

                SetStateInformation(true);

                if (printer.LogicalState != PrinterLogicalState.Uninitialized)
                {
                    if (!printer.Enabled)
                    {
                        if (string.IsNullOrEmpty(StatusText) &&
                            !printer.Enabled &&
                            (printer.ReasonDisabled & DisabledReasons.Error) == 0)
                        {
                            StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DisabledByText) + printer.ReasonDisabled;
                            StatusCurrentMode = StatusMode.Warning;
                        }
                        else
                        {
                            StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DisabledByText) + printer.ReasonDisabled;
                            var useRebootText = !SetLastErrorStatus(printer.Faults, printer.Warnings);

                            var storageService = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
                            const string blockName = "Aristocrat.Monaco.Hardware.Services.PrinterService";
                            if (storageService.BlockExists(blockName))
                            {
                                var block = storageService.GetBlock(blockName);
                                if ((bool)block["InProgress"])
                                {
                                    var ticketTitle = (string)block["TicketTitle"];
                                    var ticketValidationNumber = (string)block["TicketValidationNumber"];
                                    var ticketValue = (string)block["TicketValue"];
                                    if (string.IsNullOrEmpty(ticketValidationNumber) == false &&
                                        string.IsNullOrEmpty(ticketValue) == false)
                                    {
                                        UpdateStatusError(
                                            (useRebootText ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.RebootText) + " " : string.Empty) +
                                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.WhilePrintingText) + "\n" + ticketTitle + "\n" +
                                            ticketValidationNumber.GetFormattedBarcode() + "\n" + ticketValue,
                                            false);
                                    }
                                    else
                                    {
                                        UpdateStatusError(
                                            (useRebootText ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.RebootText) + " " : string.Empty) +
                                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.WhilePrintingText) + "\n" + ticketTitle,
                                            false);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        SetLastErrorStatus(printer.Faults, printer.Warnings);
                    }

                    SetDeviceInformation();
                }
                else
                {
                    SetDeviceInformationUnknown();
                    SetLastErrorStatus(printer.Faults, printer.Warnings);
                }
            }
            else
            {
                SetStateInformation(true);
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable);
                Logger.Warn("Printer unavailable");
            }
        }

        private void OnPrintCompleted(PrintCompletedEvent evt)
        {
            if (StatusCurrentMode == StatusMode.Working)
            {
                var temp = StatusText;
                StatusText = temp.Contains(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PrintStartedText)) ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PrintCompletedText) : string.Empty;

                StatusCurrentMode = StatusMode.Normal;
                if (Printer != null)
                {
                    SetLastErrorStatus(Printer.Faults, Printer.Warnings);
                }
            }
        }

        private void HandleEvent(IEvent theEvent)
        {
            if (ServiceManager.GetInstance().IsServiceAvailable<IPrinter>() == false)
            {
                return;
            }

            var eventType = theEvent.GetType();
            var printer = ServiceManager.GetInstance().GetService<IPrinter>();

            if (typeof(DisabledEvent) == eventType)
            {
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DisabledByText) + printer.ReasonDisabled;

                if ((printer.ReasonDisabled & DisabledReasons.Error) == 0)
                {
                    StatusCurrentMode = StatusMode.Warning;
                }
                else
                {
                    SetLastErrorStatus(printer.Faults, printer.Warnings);
                }

            }
            else if (typeof(EnabledEvent) == eventType)
            {
                var enabledEvent = (EnabledEvent)theEvent;


                if (_formFeedActive)
                {
                    _formFeedActive = false;
                    EventBus.Publish(new OperatorMenuPrintJobCompletedEvent());
                }

                if ((enabledEvent.Reasons & EnabledReasons.Reset) > 0 ||
                    (enabledEvent.Reasons & EnabledReasons.Operator) > 0)
                {
                    StatusText = string.Empty;
                    if (string.IsNullOrEmpty(printer.LastError))
                    {
                        StatusCurrentMode = StatusMode.Normal;
                    }
                    else
                    {
                        SetLastErrorStatus(printer.Faults, printer.Warnings);
                    }

                    return;
                }

                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EnabledByText) + enabledEvent.Reasons;
                StatusCurrentMode = StatusMode.Normal;
            }
            else if (typeof(InspectedEvent) == eventType)
            {
                UpdateStatusError(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InspectionFailedText), true);

                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InspectedText);
                StatusCurrentMode = StatusMode.Normal;

                _updateDeviceInformation = true;
            }
            else if (typeof(PrintStartedEvent) == eventType)
            {
                StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PrintStartedText);
                StatusCurrentMode = StatusMode.Working;

            }
            else if (typeof(InspectionFailedEvent) == eventType)
            {
                UpdateStatusError(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InspectionFailedText), false);
                _updateDeviceInformation = true;
            }
            else if (typeof(SelfTestPassedEvent) == eventType)
            {
                SelfTestCurrentState = SelfTestState.Passed;
                EventBus.Publish(new OperatorMenuPrintJobCompletedEvent());
            }
            else if (typeof(SelfTestFailedEvent) == eventType)
            {
                SelfTestCurrentState = SelfTestState.Failed;
                EventBus.Publish(new OperatorMenuPrintJobCompletedEvent());
            }
        }

        private void ErrorEvent(IEvent @event)
        {
            UpdateStatusError(@event.ToString(), false);
        }

        private void ErrorClearEvent(IEvent @event)
        {
            UpdateStatusError(@event.ToString(), true);
        }

        private void ClearFault(HardwareFaultClearEvent @event)
        {
            foreach (PrinterFaultTypes value in Enum.GetValues(typeof(PrinterFaultTypes)))
            {
                if (!@event.Fault.HasFlag(value))
                {
                    continue;
                }

                if (PrinterEventsDescriptor.FaultTexts.ContainsKey(value))
                {
                    UpdateStatusError(PrinterEventsDescriptor.FaultTexts[value], true);
                }
            }
        }

        private void ClearWarning(HardwareWarningClearEvent @event)
        {
            foreach (PrinterWarningTypes value in Enum.GetValues(typeof(PrinterWarningTypes)))
            {
                if (!@event.Warning.HasFlag(value))
                {
                    continue;
                }

                if (PrinterEventsDescriptor.WarningTexts.ContainsKey(value))
                {
                    UpdateStatusError(PrinterEventsDescriptor.WarningTexts[value], true);
                }
            }
        }

        private void SetFault(HardwareFaultEvent @event)
        {
            UpdateStatus();
        }

        private void SetWarning(HardwareWarningEvent @event)
        {
            UpdateStatus();
        }

        /// <summary>Updates status content with given text.</summary>
        /// <param name="text">The text to update.</param>
        /// <param name="removeText">Flag indicating whether to clear text.</param>
        internal void UpdateStatusError(string text, bool removeText)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var temp = StatusText;

            if (removeText)
            {
                if (text.Contains(StatusText))
                {
                    text = temp;
                }

                if (temp.Contains(text))
                {
                    Logger.DebugFormat($"CLEAR {text}");

                    var temp2 = "\n" + text;
                    var temp3 = text + "\n";
                    if (temp.Contains(temp2))
                    {
                        temp = temp.Replace(temp2, string.Empty);
                    }
                    else if (temp.Contains(temp3))
                    {
                        temp = temp.Replace(temp3, string.Empty);
                    }
                    else if (!string.IsNullOrEmpty(text))
                    {
                        temp = temp.Replace(text, string.Empty);
                    }

                    StatusText = temp;
                    Logger.DebugFormat($"CONTENT {StatusText}");

                    if (!string.IsNullOrEmpty(temp) && !temp.Contains(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EnabledByText)))
                    {
                        UpdateStatusErrorMode();
                    }
                    else
                    {
                        StatusCurrentMode = StatusMode.Normal;
                    }

                    if (string.IsNullOrEmpty(StatusText))
                    {
                        StatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoErrors);
                    }
                }
            }
            else
            {
                if (!temp.Contains(text))
                {
                    Logger.DebugFormat($"ADD {text}");
                    if (!string.IsNullOrEmpty(temp))
                    {
                        if (temp.Contains(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EnabledByText)) ||
                            temp.Contains(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PrintStartedText)) ||
                            temp.Contains(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PrintCompletedText)))
                        {
                            temp = string.Empty;
                        }
                        else
                        {
                            temp += "\n";
                        }
                    }

                    temp += text;

                    StatusText = temp;
                    Logger.DebugFormat($"CONTENT {StatusText}");

                    UpdateStatusErrorMode();
                }
            }
        }
    }
}
