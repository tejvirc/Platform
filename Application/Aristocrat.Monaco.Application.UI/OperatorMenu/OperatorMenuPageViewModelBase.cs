namespace Aristocrat.Monaco.Application.UI.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using ConfigWizard;
    using Contracts;
    using Contracts.ConfigWizard;
    using Contracts.Extensions;
    using Contracts.HardwareDiagnostics;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Contracts.Tickets;
    using Events;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using LampTest;
    using log4net;
    using Monaco.Common;
    using Monaco.Localization.Properties;
    using Monaco.UI.Common;
    using Monaco.UI.Common.Events;
    using Monaco.UI.Common.Models;
    using MVVM;
    using MVVM.Command;
    using MVVM.ViewModel;

    public enum OperatorMenuPrintData
    {
        Main,
        CurrentPage,
        SelectedItem,
        Last15,
        Custom1,
        Custom2,
        Custom3
    }

    /// <summary>
    ///     All operator menu page ViewModels should inherit from this base class
    /// </summary>
    [CLSCompliant(false)]
    public abstract class OperatorMenuPageViewModelBase : BaseEntityViewModel, IOperatorMenuPageViewModel, ILiveSettingParent
    {
        private const string PlayedCount = "PlayedCount";
        private const string TestMode = "TestMode";
        private const string FieldAccess = "FieldAccess";
        private const string OnScreenKeyboardClassName = "IPTip_Main_Window";
        private const int WindowManagerSysCommand = 0x0112;
        private const int SysCommandClose = 0xF060;
        private static readonly object TicketGenerationLock = new object();
        protected new readonly ILog Logger;
        protected bool DefaultPrintButtonEnabled;

        private volatile bool _disposed;

        private IEventBus _eventBus;
        private IPropertiesManager _properties;
        private IOperatorMenuConfiguration _configuration;
        protected IOperatorMenuAccess Access;
        protected bool UseOperatorCultureForCurrencyFormatting;

        private int _firstVisibleElement = -1;
        private bool _initialized;
        private bool _inputEnabled;
        private string _inputStatusText;
        private string _fieldAccessStatusText;
        private string _printButtonStatusText;
        private bool _isLoadingData;
        private bool? _printerButtonsEnabledInternal;
        private int _recordsToBePrinted = -1;
        private bool _noGamesPlayed;
        private bool _pageSupportsMainPrintButton;
        private bool _testModeEnabled;
        private OperatorMenuAccessRestriction _testModeRestriction;
        private bool _fieldAccessEnabled;
        private OperatorMenuAccessRestriction _fieldAccessRestriction;
        private string _testWarningText;
        private bool _printButtonAccessEnabled = true;
        private CancellationTokenSource _printCancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        ///     Initialize OperatorMenuPageViewModelBase
        /// </summary>
        /// <param name="defaultPrintButtonEnabled">
        ///     Whether or not this page should have the audit menu main print button enabled by default.
        ///     GenerateTicketsForPrint MUST be implemented for the page if setting this to true.
        ///     This can be overridden by jurisdiction by setting PrintButtonEnabled in the OperatorMenu.config.xml for the specific page.
        /// </param>
        protected OperatorMenuPageViewModelBase(bool defaultPrintButtonEnabled = false)
        {
            Logger = LogManager.GetLogger(GetType());

            PrintSelectedButtonCommand = new ActionCommand<object>(_ => Print(OperatorMenuPrintData.SelectedItem));
            PrintCurrentPageButtonCommand = new ActionCommand<object>(_ => Print(OperatorMenuPrintData.CurrentPage));
            PrintLast15ButtonCommand = new ActionCommand<object>(_ => Print(OperatorMenuPrintData.Last15));
            LoadedCommand = new ActionCommand<object>(OnLoaded);
            UnloadedCommand = new ActionCommand<object>(OnUnloaded);
            EventViewerScrolledCommand = new ActionCommand<ScrollChangedEventArgs>(OnEventViewerScrolledCommand);
            ShowInfoPopupCommand = new ActionCommand<object>(ShowInfoPopup);
            DefaultPrintButtonEnabled = defaultPrintButtonEnabled;
            UseOperatorCultureForCurrencyFormatting = Configuration?.GetSetting(OperatorMenuSetting.UseOperatorCultureForCurrencyFormatting, false) ?? false;
            SetIgnoreProperties();
        }

        /// <summary>
        ///     Gets the print selected button command.
        /// </summary>
        /// <value>
        ///     The print selected button command.
        /// </value>
        public ICommand PrintSelectedButtonCommand { get; }

        /// <summary>
        ///     Gets the print CurrentPage button command.
        /// </summary>
        /// <value>
        ///     The print CurrentPage button command.
        /// </value>
        public ICommand PrintCurrentPageButtonCommand { get; }

        /// <summary>
        ///     Gets the print Last 15 entries button command.
        /// </summary>
        /// <value>
        ///     The print Lat 15 entries button command.
        /// </value>
        public ICommand PrintLast15ButtonCommand { get; }

        public ICommand LoadedCommand { get; set; }

        public ICommand UnloadedCommand { get; set; }

        public ICommand EventViewerScrolledCommand { get; }

        public ICommand ShowInfoPopupCommand { get; }

        // This allows us to disable specific fields as a group on a page (versus whole page) if the rule (FieldAccess)
        // is defined in the OperatorMenuConfig file (per Jurisdiction and per page) if not defined the default is true.
        // If you want the field disabled via rule use this property in the binding instead of InputEnabled (if defined in OperatorMenuConfig)
        public bool InputEnabledByRuleOverride => InputEnabled && FieldAccessEnabled;

        public virtual bool InputEnabled
        {
            get => _inputEnabled;
            set
            {
                if (_inputEnabled != value)
                {
                    _inputEnabled = value;

                    MvvmHelper.ExecuteOnUI(() =>
                    {
                        OnInputEnabledChanged();
                        RaisePropertyChanged(nameof(InputEnabled), nameof(InputEnabledByRuleOverride), nameof(IsInputEnabled));
                    });
                }
            }
        }

        public string InputStatusText
        {
            get => _inputStatusText;
            set
            {
                _inputStatusText = value;
                RaisePropertyChanged(nameof(InputStatusText));
                UpdateStatusText();
            }
        }

        public string FieldAccessStatusText
        {
            get => _fieldAccessStatusText;
            protected set
            {
                _fieldAccessStatusText = value;
                RaisePropertyChanged(nameof(FieldAccessStatusText));
                UpdateStatusText();
            }
        }

        public string PrintButtonStatusText
        {
            get => _printButtonStatusText;
            set
            {
                _printButtonStatusText = value;
                RaisePropertyChanged(nameof(PrintButtonStatusText));
                UpdateStatusText();
            }
        }

        public virtual bool TestModeEnabled
        {
            get => _testModeEnabled & TestModeEnabledSupplementary;
            set
            {
                if (_testModeEnabled != value)
                {
                    _testModeEnabled = value;
                    RaisePropertyChanged(nameof(TestModeEnabled));
                    OnTestModeEnabledChanged();

                    if (_testModeEnabled)
                    {
                        EventBus.Publish(new OperatorMenuPopupEvent(false));
                    }
                }
            }
        }

        public OperatorMenuAccessRestriction TestModeRestriction
        {
            get => _testModeRestriction;
            protected set
            {
                if (_testModeRestriction != value)
                {
                    _testModeRestriction = value;
                    RaisePropertyChanged(nameof(TestModeRestriction));
                    UpdateWarningMessage();
                }
            }
        }

        public bool FieldAccessEnabled
        {
            get => _fieldAccessEnabled;
            set
            {
                if (_fieldAccessEnabled != value)
                {
                    _fieldAccessEnabled = value;
                    RaisePropertyChanged(nameof(FieldAccessEnabled), nameof(InputEnabledByRuleOverride));
                    OnFieldAccessEnabledChanged();
                }
            }
        }

        public OperatorMenuAccessRestriction FieldAccessRestriction
        {
            get => _fieldAccessRestriction;
            protected set
            {
                if (_fieldAccessRestriction != value)
                {
                    _fieldAccessRestriction = value;
                    RaisePropertyChanged(nameof(FieldAccessRestriction));
                    SetFieldAccessRestrictionText();
                    OnFieldAccessRestrictionChange();
                }
            }
        }

        public bool PrintButtonAccessEnabled
        {
            get => _printButtonAccessEnabled;
            set
            {
                if (_printButtonAccessEnabled != value)
                {
                    _printButtonAccessEnabled = value;
                    RaisePropertyChanged(nameof(PrintButtonAccessEnabled),
                        nameof(PrinterButtonsEnabled),
                        nameof(MainPrintButtonEnabled));
                    UpdatePrinterButtons();
                }
            }
        }

        public virtual bool TestModeEnabledSupplementary => true;

        public bool GameIdle =>
            (!ServiceManager.GetInstance().TryGetService<IOperatorMenuGamePlayMonitor>()?.InGameRound ?? true) &&
            (!ServiceManager.GetInstance().TryGetService<IOperatorMenuGamePlayMonitor>()?.IsRecoveryNeeded ?? true);

        public bool NoGamesPlayed
        {
            get => _noGamesPlayed;
            private set
            {
                if (value != _noGamesPlayed)
                {
                    _noGamesPlayed = value;
                    RaisePropertyChanged(nameof(NoGamesPlayed));
                }
            }
        }

        /// <summary>
        ///     This can be used for all printer buttons other than the global Print button
        /// </summary>
        private bool PrinterButtonsEnabledInternal
        {
            get => _printerButtonsEnabledInternal ?? false;
            set
            {
                if (_printerButtonsEnabledInternal != value)
                {
                    _printerButtonsEnabledInternal = value;
                    RaisePropertyChanged(nameof(MainPrintButtonEnabled), nameof(PrinterButtonsEnabled));
                }

                UpdatePrinterButtons();
            }
        }

        public virtual bool PrinterButtonsEnabled => PrinterButtonsEnabledInternal && PrintButtonAccessEnabled;

        public bool PrintCurrentPageButtonVisible { get; private set; }

        public bool PrintSelectedButtonVisible { get; private set; }

        public bool PrintLast15ButtonVisible { get; private set; }

        /// <summary>
        ///     Gets or sets the First visible element in the page.
        /// </summary>
        public int FirstVisibleElement
        {
            get => _firstVisibleElement;
            set
            {
                _firstVisibleElement = value;
                RaisePropertyChanged(nameof(FirstVisibleElement));
            }
        }

        /// <summary>
        ///     Gets or sets the visible records in the view.
        /// </summary>
        public int RecordsToBePrinted
        {
            get => _recordsToBePrinted;
            set
            {
                _recordsToBePrinted = value;
                RaisePropertyChanged(nameof(RecordsToBePrinted));
            }
        }

        public string TestWarningText
        {
            get => _testWarningText;
            set
            {
                if (_testWarningText != value)
                {
                    _testWarningText = value;
                    RaisePropertyChanged(nameof(TestWarningText));
                }
            }
        }

        /// <summary>
        ///     For data that takes time to load, this can be used to display an indeterminate progress bar
        /// </summary>
        public virtual bool IsLoadingData
        {
            get => _isLoadingData;
            set
            {
                _isLoadingData = value;
                RaisePropertyChanged(nameof(IsLoadingData));
            }
        }

        /// <inheritdoc />
        public bool IsLoaded { get; private set; }

        // use in classes where needed to indicate there is no data in the view and printing should be disabled
        public virtual bool DataEmpty => false;

        public virtual bool MainPrintButtonEnabled => PageSupportsMainPrintButton && PrinterButtonsEnabled;

        public virtual bool PageSupportsMainPrintButton
        {
            get => _pageSupportsMainPrintButton;
            private set
            {
                if (_pageSupportsMainPrintButton != value)
                {
                    _pageSupportsMainPrintButton = value;
                    RaisePropertyChanged(nameof(MainPrintButtonEnabled));
                }
            }
        }

        /// <inheritdoc />
        public virtual bool CanCalibrateTouchScreens => true;

        public virtual bool PopupOpen { get; set; }

        protected IEventBus EventBus =>
            _eventBus ?? (_eventBus = ServiceManager.GetInstance().TryGetService<IEventBus>());

        protected IPropertiesManager PropertiesManager =>
            _properties ?? (_properties = ServiceManager.GetInstance().TryGetService<IPropertiesManager>());

        protected IOperatorMenuConfiguration Configuration =>
            _configuration ??
            (_configuration = ServiceManager.GetInstance().TryGetService<IOperatorMenuConfiguration>());

        protected IPrinter Printer => ServiceManager.GetInstance().TryGetService<IPrinter>();

        protected virtual bool IsContainerPage => false;

        /// <summary>
        /// Is this a temporary modal dialog?
        /// </summary>
        protected virtual bool IsModalDialog => false;

        protected OperatorMenuAccessRestriction AccessRestriction { get; private set; }

        protected bool ClearValidationOnUnload { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Override with all data initialization that should occur on initial page load only.
        ///     This method should primarily be used for loading large amounts of data because it will occur on a non-UI thread.
        /// </summary>
        protected virtual void InitializeData()
        {
            // Don't put any logic in the base method
            // For logic that should apply to all derived classes, use OnLoaded(object) method below inside the !_initialized block
        }

        /// <summary>
        ///     Override with all data initialization that should occur every time the page loads.
        /// </summary>
        protected virtual void OnLoaded()
        {
            // Don't put any logic in the base method
            // For logic that should apply to all derived classes, use OnLoaded(object) method below
        }

        protected virtual void UpdateWarningMessage()
        {
            switch (TestModeRestriction)
            {
                case OperatorMenuAccessRestriction.InGameRound:
                    TestWarningText = Localizer.For(CultureFor.Operator)
                        .GetString(ResourceKeys.TestModeDisabledStatusGame);
                    break;
                case OperatorMenuAccessRestriction.MainDoor:
                    TestWarningText = Localizer.For(CultureFor.Operator)
                        .GetString(ResourceKeys.TestModeDisabledStatusDoor);
                    break;
                case OperatorMenuAccessRestriction.ZeroCredits:
                    TestWarningText = Localizer.For(CultureFor.Operator)
                        .GetString(ResourceKeys.TestModeDisabledStatusCredits);
                    break;
            }
        }

        /// <summary>
        ///     Override with logic that should occur every time the PrinterButtonsEnabled state has changed
        /// </summary>
        protected virtual void UpdatePrinterButtons()
        {
            // Don't put any logic in the base method
        }

        protected virtual IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            Logger.Error($"{GetType()} does not implement GenerateTicketsForPrint and no ticket has been printed.");

            // no implementation for base class; this will print nothing
            return new List<Ticket>();
        }

        public CultureInfo CurrencyDisplayCulture => GetCurrencyDisplayCulture();

        protected virtual CultureInfo GetCurrencyDisplayCulture() => UseOperatorCultureForCurrencyFormatting ? Localizer.For(CultureFor.Operator).CurrentCulture : CurrencyExtensions.CurrencyCultureInfo;

        protected IEnumerable<Ticket> GeneratePrintVerificationTickets()
        {
            List<Ticket> tickets = null;
            var ticketCreator = ServiceManager.GetInstance().TryGetService<IVerificationTicketCreator>();
            if (ticketCreator != null)
            {
                tickets = new List<Ticket>();
                for (var i = 0; i < 3; i++)
                {
                    var baseTickets = SplitTicket(ticketCreator.Create(i));

                    if (baseTickets.Count > 1)
                    {
                        var secondTicket = baseTickets[1];
                        var ticket = ticketCreator.CreateOverflowPage(i);
                        secondTicket[TicketConstants.Left] = $"{ticket[TicketConstants.Left]}{secondTicket[TicketConstants.Left]}";
                        secondTicket[TicketConstants.Center] = $"{ticket[TicketConstants.Center]}{secondTicket[TicketConstants.Center]}";
                        secondTicket[TicketConstants.Right] = $"{ticket[TicketConstants.Right]}{secondTicket[TicketConstants.Right]}";
                    }
                    tickets.AddRange(baseTickets);
                }
            }

            return tickets;
        }

        /// <summary>
        ///     Override with any logic that should occur every time the page is unloaded.
        /// </summary>
        protected virtual void OnUnloaded()
        {
            // Don't put any logic in the base method
            // For logic that should apply to all derived classes, use OnUnloaded(object) method above
        }

        /// <summary>
        ///     Override with any class-specific logic that should occur when the Operator Menu window is closed.
        /// </summary>
        protected virtual void DisposeInternal()
        {
            // Don't put any logic in the base method
            // For logic that should apply to all derived classes, use Dispose(bool) method above
        }

        protected virtual void OnInputStatusChanged()
        {
        }

        protected virtual void OnInputEnabledChanged()
        {
            // Override this method to update related IsEnabled properties when InputEnabled changes
        }

        protected virtual void OnTestModeEnabledChanged()
        {
        }

        protected virtual void OnFieldAccessEnabledChanged()
        {
        }

        protected virtual void OnFieldAccessRestrictionChange()
        {
        }

        protected virtual void UpdateStatusText()
        {
            if (IsLoaded)
            {
                MvvmHelper.ExecuteOnUI(_UpdateStatusText);
            }
        }

        private void _UpdateStatusText()
        {
            if (IsContainerPage || IsModalDialog)
            {
                // TODO: Please refactor this. IsModalDialog was added to fix VLT-18567,
                // but there are better ways of fixing the issue than simply blocking
                // modal dialogs from updating the OperatorMenu warning status text.
                return;
            }

            // By default always display the Input Status text unless the Field Access Status text is not empty
            if (!string.IsNullOrEmpty(FieldAccessStatusText))
            {
                EventBus.Publish(new OperatorMenuWarningMessageEvent(FieldAccessStatusText));
            }
            else if (!string.IsNullOrEmpty(InputStatusText))
            {
                EventBus.Publish(new OperatorMenuWarningMessageEvent(InputStatusText));
            }
            else if (!string.IsNullOrEmpty(PrintButtonStatusText))
            {
                EventBus.Publish(new OperatorMenuWarningMessageEvent(PrintButtonStatusText));
            }
            else
            {
                EventBus.Publish(new OperatorMenuWarningMessageEvent());
            }
        }

        protected async void InitializeDataAsync()
        {
            if (_initialized || IsLoadingData)
            {
                // Don't reload if currently loading or already initialized
                return;
            }

            await Task.Run(
                () =>
                {
                    IsLoadingData = true;
                    InitializeData();
                    IsLoadingData = false;
                    _initialized = true;
                });

            RaisePropertyChanged(nameof(DataEmpty));
        }

        protected IEnumerable<T> GetItemsToPrint<T>(ICollection<T> itemsToPrint, OperatorMenuPrintData dataType)
        {
            if (dataType != OperatorMenuPrintData.Main && dataType != OperatorMenuPrintData.CurrentPage &&
                dataType != OperatorMenuPrintData.Last15)
            {
                return null;
            }

            var firstIndex = dataType == OperatorMenuPrintData.CurrentPage ? FirstVisibleElement : 0;

            if (dataType == OperatorMenuPrintData.Main || RecordsToBePrinted < 0)
            {
                // Print all items
                return itemsToPrint;
            }

            var recordsToPrint = dataType == OperatorMenuPrintData.Last15 ? 15 : RecordsToBePrinted;
            return itemsToPrint.Skip(firstIndex).Take(recordsToPrint);
        }

        protected void Print(OperatorMenuPrintData dataType, Action callback = null, bool isDiagnostic = false)
        {
            if (IsContainerPage)
            {
                // Container pages host multiple other pages but do not generate print data themselves.
                return;
            }

            PrinterButtonsEnabledInternal = false;
            EventBus.Publish(new OperatorMenuPrintJobStartedEvent());
            if (isDiagnostic) EventBus.Publish(new HardwareDiagnosticTestStartedEvent(HardwareDiagnosticDeviceCategory.Printer));

            var token = _printCancellationTokenSource.Token;

            Task.Run(
                () =>
                {
                    lock (TicketGenerationLock)
                    {
                        var baseTickets = GenerateTicketsForPrint(dataType)?.ToList();
                        token.ThrowIfCancellationRequested();

                        if (Printer != null && baseTickets != null && baseTickets.Any())
                        {
                            // Split tickets to make sure we don't lose data if the ticket is too long
                            var tickets = new List<Ticket>();
                            foreach (var ticket in baseTickets)
                            {
                                tickets.AddRange(SplitTicket(ticket));
                            }

                            Print(tickets);
                        }
                        else
                        {
                            EventBus.Publish(new OperatorMenuPrintJobCompletedEvent());
                            if (isDiagnostic) EventBus.Publish(new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.Printer));
                        }

                        callback?.Invoke();
                    }
                },
                token).FireAndForget(
                ex =>
                {
                    Logger.Error("Error while printing", ex?.InnerExceptions[0]);
                    EventBus.Publish(new OperatorMenuPrintJobCompletedEvent());
                    if (isDiagnostic)
                    {
                        EventBus.Publish(new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.Printer));
                    }
                });
        }

        /// <summary>
        ///     Display a center-screen popup that times out after the specified time
        /// </summary>
        /// <param name="text">Popup text to display</param>
        /// <param name="timeoutSeconds">Seconds after which to time out (defaults to 2 seconds if nothing is included here)</param>
        protected void ShowPopup(string text, int timeoutSeconds = 0)
        {
            EventBus.Publish(new OperatorMenuPopupEvent(true, text, null, timeoutSeconds));
        }

        protected void RegisterAccessRule(
            string name,
            string enabledProperty,
            string restrictionProperty,
            Action<bool, OperatorMenuAccessRestriction> callback = null)
        {
            var accessRuleSet = Configuration?.GetAccessRuleSet(this, name);
            if (!string.IsNullOrEmpty(accessRuleSet))
            {
                Access.RegisterAccessRule(
                    this,
                    accessRuleSet,
                    (access, restriction) =>
                    {
                        GetType().GetProperty(restrictionProperty)?.SetValue(this, restriction);
                        GetType().GetProperty(enabledProperty)?.SetValue(this, access);
                        callback?.Invoke(access, restriction);
                    });
            }
            else
            {
                // this access rule doesn't exist in this jurisdiction
                GetType().GetProperty(restrictionProperty)?.SetValue(this, OperatorMenuAccessRestriction.None);
                GetType().GetProperty(enabledProperty)?.SetValue(this, true);
            }
        }

        protected void SetInputStatus(bool inputEnabled, OperatorMenuAccessRestriction accessRestriction)
        {
            InputEnabled = inputEnabled;
            AccessRestriction = accessRestriction;

            // put logic in derived class if different warning messages are needed
            // called when page is loaded to set state based on bank/game state.
            if (TryGetWarningMessage(accessRestriction, out var warningMessage) && InputStatusText != warningMessage)
            {
                InputStatusText = warningMessage;

                if (Application.Current != null)
                {
                    MvvmHelper.ExecuteOnUI(OnInputStatusChanged);
                }
            }

            switch (accessRestriction)
            {
                case OperatorMenuAccessRestriction.GamesPlayed:
                case OperatorMenuAccessRestriction.InGameRound:
                case OperatorMenuAccessRestriction.GameLoaded:
                case OperatorMenuAccessRestriction.ZeroCredits:
                    InputEnabled = false;
                    break;
                case OperatorMenuAccessRestriction.LogicDoor:
                    CloseTouchScreenKeyboard();
                    break;
            }
        }

        private void SetFieldAccessRestrictionText()
        {
            if (TryGetWarningMessage(FieldAccessRestriction, out var warningMessage) &&
                FieldAccessStatusText != warningMessage)
            {
                FieldAccessStatusText = warningMessage;
            }
        }

        private static OperatorMenuAccessRestriction GetCurrentRestriction(
            OperatorMenuAccessRestriction accessRestriction,
            OperatorMenuAccessRestriction fieldAccessRestriction)
        {
            if (accessRestriction == OperatorMenuAccessRestriction.None
                && fieldAccessRestriction != OperatorMenuAccessRestriction.None)
            {
                return fieldAccessRestriction;
            }

            if (fieldAccessRestriction == OperatorMenuAccessRestriction.None
                && accessRestriction != OperatorMenuAccessRestriction.None)
            {
                return accessRestriction;
            }

            return fieldAccessRestriction == accessRestriction
                ? accessRestriction
                : OperatorMenuAccessRestriction.None;
        }
        protected virtual bool AllFieldsReadonly()
        {
            return !InputEnabled;
        }

        private bool TryGetWarningMessage(OperatorMenuAccessRestriction restriction, out string warningMessage)
        {
            warningMessage = null;

            switch (restriction)
            {
                case OperatorMenuAccessRestriction.None:
                    warningMessage = string.Empty;
                    return true;
                case OperatorMenuAccessRestriction.GamesPlayed:
                    warningMessage = AllFieldsReadonly()
                        ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GamesPlayedWarning)
                        : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SomeGamesPlayedWarning);
                    return true;
                case OperatorMenuAccessRestriction.InGameRound:
                    warningMessage = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EndGameRoundBeforeChange);
                    return true;
                case OperatorMenuAccessRestriction.GameLoaded:
                    warningMessage = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExitGameBeforeChange);
                    return true;
                case OperatorMenuAccessRestriction.ZeroCredits:
                    warningMessage = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.RemoveAllCreditsBeforeChange);
                    return true;
                case OperatorMenuAccessRestriction.MainDoor:
                    warningMessage = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OpenMainDoor);
                    return true;
                case OperatorMenuAccessRestriction.MainOpticDoor:
                    warningMessage = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OpenMainDoor);
                    return true;
                case OperatorMenuAccessRestriction.LogicDoor:
                    warningMessage = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OpenLogicDoor);
                    return true;
                case OperatorMenuAccessRestriction.JackpotKey:
                    warningMessage = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.JackpotKeyRequired);
                    return true;
                case OperatorMenuAccessRestriction.EKeyVerified:
                    warningMessage = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.VerifiedEKeyRequired);
                    return true;
                case OperatorMenuAccessRestriction.InitialGameConfigNotCompleteOrEKeyVerified:
                    warningMessage = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.VerifiedEKeyRequiredGameConfiguration);
                    return true;
                case OperatorMenuAccessRestriction.NoHardLockups:
                    warningMessage = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.RemoveAllHardLockupsBeforeChange);
                    return true;
                case OperatorMenuAccessRestriction.HostTechnician:
                    warningMessage = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TechnicianCardRequired);
                    return true;
                case OperatorMenuAccessRestriction.ProgInit:
                    warningMessage = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameDisabledForProgressiveInitialization);
                    return true;
                default:
                    return false;
            }
        }

        protected void SetPrintAccessStatus(bool printEnabled, OperatorMenuAccessRestriction restriction)
        {
            PrintButtonAccessEnabled = printEnabled;

            switch (restriction)
            {
                case OperatorMenuAccessRestriction.None:
                    PrintButtonStatusText = string.Empty;
                    break;
                case OperatorMenuAccessRestriction.InGameRound:
                    PrintButtonStatusText = Localizer.For(CultureFor.Operator)
                        .GetString(ResourceKeys.EndGameRoundBeforePrint);
                    break;
            }
        }

        protected void OnPrintButtonStatusChanged(PrintButtonStatusEvent evt)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                PrinterButtonsEnabledInternal = evt.Enabled;
                SetPrintAccessStatus(evt.Enabled, OperatorMenuAccessRestriction.None);
            });
        }


        protected void OnDialogClosed(DialogClosedEvent evt)
        {
            UpdateStatusText();
        }

        protected List<Ticket> TicketToList(Ticket ticket)
        {
            if (ticket == null)
            {
                return null;
            }

            return new List<Ticket> { ticket };
        }

        protected List<Ticket> SplitTicket(Ticket ticket)
        {
            if ((!ticket.Data?.Any() ?? false) || ticket[TicketConstants.Left] == null || ticket[TicketConstants.Center] == null || ticket[TicketConstants.Right] == null)
            {
                return new List<Ticket> { ticket };
            }

            var tickets = new List<Ticket>();
            var leftRemainder = ticket[TicketConstants.Left].TrimEnd().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var centerRemainder = ticket[TicketConstants.Center].TrimEnd().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var rightRemainder = ticket[TicketConstants.Right].TrimEnd().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var lineLimit = ServiceManager.GetInstance().TryGetService<IPropertiesManager>().GetValue(ApplicationConstants.AuditTicketLineLimit, 36);

            while (leftRemainder.Length > 0)
            {
                var leftKeep = leftRemainder.Take(lineLimit).ToArray();
                var centerKeep = centerRemainder.Take(lineLimit).ToArray();
                var rightKeep = rightRemainder.Take(lineLimit).ToArray();

                leftRemainder = leftRemainder.Skip(lineLimit).ToArray();
                centerRemainder = centerRemainder.Skip(lineLimit).ToArray();
                rightRemainder = rightRemainder.Skip(lineLimit).ToArray();

                if (leftRemainder.Length > 1)
                {
                    leftRemainder = leftRemainder.ToList().Prepend(string.Empty).ToArray();
                }
                if (centerRemainder.Length > 1)
                {
                    centerRemainder = centerRemainder.ToList().Prepend(string.Empty).ToArray();
                }
                if (rightRemainder.Length > 1)
                {
                    rightRemainder = rightRemainder.ToList().Prepend(string.Empty).ToArray();
                }

                tickets.Add(
                    new Ticket
                    {
                        [TicketConstants.Title] = ticket[TicketConstants.Title],
                        [TicketConstants.TicketType] = ticket[TicketConstants.TicketType],
                        [TicketConstants.Left] = string.Join(Environment.NewLine, leftKeep),
                        [TicketConstants.Center] = string.Join(Environment.NewLine, centerKeep),
                        [TicketConstants.Right] = string.Join(Environment.NewLine, rightKeep)
                    });
            }

            if (tickets.Count > 1)
            {
                Logger.Debug($"A printed ticket was too long and was split into {tickets.Count} tickets");
            }

            return tickets;
        }

        protected T GetConfigSetting<T>(Type pageType, string settingName, T defaultValue)
        {
            if (Configuration != null)
            {
                return Configuration.GetSetting(pageType, settingName, defaultValue);
            }

            return default(T);
        }

        protected T GetConfigSetting<T>(string settingName, T defaultValue)
        {
            if (Configuration != null)
            {
                return Configuration.GetSetting(this, settingName, defaultValue);
            }

            return default(T);
        }

        protected T GetGlobalConfigSetting<T>(string settingName, T defaultValue)
        {
            if (Configuration != null)
            {
                return Configuration.GetSetting(settingName, defaultValue);
            }

            return default(T);
        }

        protected static void CloseTouchScreenKeyboard()
        {
            // Try to find the on-screen keyboard, which since Windows 8 is not a normal window.
            // Search the interior of the screen in a grid pattern large enough to catch it.
            var div = 10;
            for (var x = SystemParameters.PrimaryScreenWidth / div;
                 x < SystemParameters.PrimaryScreenWidth;
                 x += SystemParameters.PrimaryScreenWidth / div)
            {
                for (var y = SystemParameters.PrimaryScreenHeight / div;
                     y < SystemParameters.PrimaryScreenHeight;
                     y += SystemParameters.PrimaryScreenHeight / div)
                {
                    // Is there a window at this point?
                    var hWnd = NativeMethods.WindowFromPoint(new System.Drawing.Point((int)x, (int)y));
                    if (hWnd == IntPtr.Zero)
                    {
                        continue;
                    }

                    // Does the window have a parent?
                    hWnd = NativeMethods.GetParent(hWnd);
                    if (hWnd == IntPtr.Zero)
                    {
                        continue;
                    }

                    // Is the parent's class called "IPTip_Main_Window"?
                    // 256 characters is the maximum class name length.
                    var className = new StringBuilder(256);
                    if (NativeMethods.GetClassName(hWnd, className, className.Capacity) != 0 &&
                        string.Compare(className.ToString(), OnScreenKeyboardClassName, true, CultureInfo.InvariantCulture) == 0)
                    {
                        // Found it!  Tell it to close.
                        NativeMethods.PostMessage(hWnd, WindowManagerSysCommand, (IntPtr)SysCommandClose, IntPtr.Zero);

                        return;
                    }
                }
            }
        }

        protected string GetBooleanDisplayText(bool value)
        {
            return value
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TrueText)
                : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.FalseText);
        }

        internal void OnLoaded(object page)
        {
            InitializeDataAsync();

            EventBus.Subscribe<PrintButtonClickedEvent>(this, OnPrintButtonClicked);
            EventBus.Subscribe<PrintButtonStatusEvent>(this, OnPrintButtonStatusChanged);
            EventBus.Subscribe<DialogClosedEvent>(this, OnDialogClosed);

            CheckPlayedCountMeter();

            var wizardPage = this is ConfigWizardViewModelBase wizard && wizard.IsWizardPage
                             || this is IConfigWizardNavigator
                             || this is IConfigWizardDialog dialog && dialog.IsInWizard;

            if (wizardPage)
            {
                SetInputStatus(true, OperatorMenuAccessRestriction.None);
                FieldAccessEnabled = true;
            }
            else
            {
                Access = ServiceManager.GetInstance().GetService<IOperatorMenuAccess>();
                var accessRuleSet = Configuration?.GetAccessRuleSet(this);
                if (string.IsNullOrEmpty(accessRuleSet))
                {
                    SetInputStatus(true, OperatorMenuAccessRestriction.None);
                }
                else
                {
                    Access.RegisterAccessRule(this, accessRuleSet, SetInputStatus);
                }

                var printAccessRuleSet = Configuration?.GetPrintAccessRuleSet(this);
                if (string.IsNullOrEmpty(printAccessRuleSet))
                {
                    SetPrintAccessStatus(true, OperatorMenuAccessRestriction.None);
                }
                else
                {
                    Access.RegisterAccessRule(this, printAccessRuleSet, SetPrintAccessStatus);
                }

                RegisterAccessRule(TestMode, nameof(TestModeEnabled), nameof(TestModeRestriction));
                RegisterAccessRule(FieldAccess, nameof(FieldAccessEnabled), nameof(FieldAccessRestriction));
            }

            PageSupportsMainPrintButton = Configuration?.GetPrintButtonEnabled(this, DefaultPrintButtonEnabled) ?? false;
            PrintCurrentPageButtonVisible = GetGlobalConfigSetting(OperatorMenuSetting.PrintCurrentPage, true);
            PrintLast15ButtonVisible = GetGlobalConfigSetting(OperatorMenuSetting.PrintLast15, true);
            PrintSelectedButtonVisible = GetGlobalConfigSetting(OperatorMenuSetting.PrintSelected, true);

            OnLoaded();
            RaisePropertyChanged(nameof(DataEmpty));
            EventBus.Publish(new OperatorMenuPageLoadedEvent(this));
            EventBus.Publish(new OperatorMenuPopupEvent(false));

            UpdateStatusText();

            TurnOffLamps();  // VLT-10029

            IsLoaded = true;
        }

        private void TurnOffLamps()
        {
            var lamp = LampTestUtilities.GetLampTest();

            if (lamp == null)
            {
                return;
            }

            var isButtonTestActive = ServiceManager.GetInstance()
                .TryGetService<IButtonService>()
                ?.IsTestModeActive ?? false;

            if (isButtonTestActive)
            {
                return;
            }

            lamp.SetEnabled(true);
            lamp.SetSelectedLamps(Contracts.LampTest.SelectedLamps.All, false);
        }

        private void CheckPlayedCountMeter()
        {
            var meterManager = ServiceManager.GetInstance().TryGetService<IMeterManager>();

            if (meterManager != null && meterManager.IsMeterProvided(PlayedCount))
            {
                var playedCountMeter = meterManager.GetMeter(PlayedCount);
                NoGamesPlayed = (playedCountMeter?.Lifetime ?? 0) == 0;
            }
        }

        private void OnEventViewerScrolledCommand(ScrollChangedEventArgs args)
        {
            FirstVisibleElement = (int)args.VerticalOffset;
            RecordsToBePrinted = (int)args.ViewportHeight;
        }

        private void OnPrintButtonClicked(PrintButtonClickedEvent evt)
        {
            if (evt.Cancel)
            {
                return;
            }

            Print(OperatorMenuPrintData.Main);
        }

        /// <summary>
        ///     Print a list of tickets and handle timeouts, cancelling, and errors
        /// </summary>
        private void Print(IReadOnlyCollection<Ticket> tickets)
        {
            var printJob = new OperatorMenuPrintJobEvent(tickets);
            Logger.Debug($"Printing {tickets.Count} tickets from the operator menu");
            EventBus.Publish(printJob);
        }

        private void ShowInfoPopup(object o)
        {
            if (o is object[] objects && objects.Length >= 2 && objects[0] is UIElement element &&
                objects[1] is string text)
            {
                EventBus.Publish(new OperatorMenuPopupEvent(!PopupOpen, text, element, 0, true));
            }
        }

        private void OnUnloaded(object page)
        {
            EventBus.UnsubscribeAll(this);
            CloseTouchScreenKeyboard();
            _printerButtonsEnabledInternal = null;
            Access?.UnregisterAccessRules(this);
            OnUnloaded();
            if (ClearValidationOnUnload)
            {
                ValidationHelper.ClearInvalid(page as OperatorMenuPage);
            }

            EventBus.Publish(new OperatorMenuPopupEvent(false));

            PopupOpen = false;
            IsLoaded = false;
        }

        private void SetIgnoreProperties()
        {
            IgnorePropertyForCommitted(
                new List<string>
                {
                    nameof(InputEnabled),
                    nameof(DataEmpty),
                    nameof(InputStatusText),
                    nameof(FieldAccessStatusText),
                    nameof(PrintButtonStatusText),
                    nameof(TestModeEnabled),
                    nameof(TestModeRestriction),
                    nameof(FieldAccessEnabled),
                    nameof(FieldAccessRestriction),
                    nameof(PrintButtonAccessEnabled),
                    nameof(TestModeEnabledSupplementary),
                    nameof(GameIdle),
                    nameof(NoGamesPlayed),
                    nameof(PrinterButtonsEnabled),
                    nameof(PrintCurrentPageButtonVisible),
                    nameof(PrintSelectedButtonVisible),
                    nameof(PrintLast15ButtonVisible),
                    nameof(FirstVisibleElement),
                    nameof(RecordsToBePrinted),
                    nameof(TestWarningText),
                    nameof(IsLoadingData),
                    nameof(IsLoaded),
                    nameof(MainPrintButtonEnabled),
                    nameof(PageSupportsMainPrintButton),
                    nameof(PopupOpen),
                    nameof(IsContainerPage),
                    nameof(AccessRestriction),
                    nameof(ClearValidationOnUnload)
                });
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                DisposeInternal();

                // ReSharper disable once UseNullPropagation
                if (_printCancellationTokenSource != null)
                {
                    _printCancellationTokenSource.Cancel();
                    _printCancellationTokenSource.Dispose();
                    _printCancellationTokenSource = null;
                }

                EventBus?.UnsubscribeAll(this);
                Access?.UnregisterAccessRules(this);
                LoadedCommand = null;
                UnloadedCommand = null;
            }

            _disposed = true;
        }

        private static class NativeMethods
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "0")]
            [DllImport("user32.dll")]
            public static extern IntPtr WindowFromPoint(System.Drawing.Point p);

            [DllImport("user32.dll")]
            public static extern Boolean PostMessage(IntPtr hWnd, Int32 msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll")]
            public static extern IntPtr GetParent(IntPtr hWnd);

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        }

        /// <summary>
        /// Enables live settings on this VM.
        /// </summary>
        public bool IsInputEnabled => InputEnabled;
    }
}
