namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Timers;
    using System.Windows;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using Accounting.Contracts;
    using Contracts;
    using Contracts.Extensions;
    using Contracts.Input;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Events;
    using Hardware.Contracts;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.Display;
    using Hardware.Contracts.KeySwitch;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Touch;
    using Kernel;
    using Kernel.Contracts;
    using Localization;
    using log4net;
    using Monaco.Localization.Properties;
    using Monaco.UI.Common;
    using Monaco.UI.Common.Events;
    using MVVM;
    using MVVM.Command;
    using MVVM.ViewModel;
    using OperatorMenu;
    using Vgt.Client12.Application.OperatorMenu;
    using Views;

    /// <summary>
    ///     A DiscovererViewModel contains the logic for MenuSelectionWindow.xaml.cs
    /// </summary>
    /// <seealso cref="BaseEntityViewModel" />
    [CLSCompliant(false)]
    public sealed class MenuSelectionViewModel : BaseEntityViewModel, IOperatorMenuConfigObject, IDisposable
    {
        private const double DayTimerIntervalSeconds = 1.0;
        private const string DemoModeProperty = "System.DemoMode";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IOperatorMenuLauncher _operatorMenuLauncher;
        private readonly ITime _timeService;
        private readonly IMeterManager _meterManager;
        private readonly IButtonService _buttonService;
        private readonly IOperatorMenuConfiguration _configuration;
        private readonly IOperatorMenuGamePlayMonitor _gamePlayMonitor;
        private readonly ISerialTouchService _serialTouchService;
        private readonly ILocalization _localization;

        private readonly ButtonDeckNavigator _buttonNavigator = new ButtonDeckNavigator();
        private readonly object _printLock = new object();
        private readonly object _exitLock = new object();
        private readonly IDialogService _dialogService;
        private IPrinter _printer;

        private OperatorMenuPrintHandler _operatorMenuPrintHandler;
        private string _creditBalanceContent;
        private bool _creditBalanceVisible;
        private DateTime _currentDateTime;
        private ITimer _dayTimer;
        private bool _disposed;
        private bool _exitButtonFocused;
        private bool _dataEmpty = true;
        private bool _mainPrintButtonEnabled;
        private bool _isLoadingData;
        private string _operatorMenuLabelContent;
        private string _pageTitleContent;
        private bool _printButtonEnabled;
        private bool _printButtonEnabledInternal = true;
        private string _printStatusText = string.Empty;
        private string _role = ApplicationConstants.DefaultRole;
        private IOperatorMenuPageLoader _selectedItem;
        private bool _showCancelPrintButton;
        private bool _cancelButtonEnabled;
        private bool _printingAllowed;
        private bool _popupOpen;
        private UIElement _popupPlacementTarget;
        private string _popupText;
        private int _popupTimeoutSeconds;
        private Timer _popupTimer;
        private TouchCalibrationConfirmationViewModel _touchConfirmationDialog;
        private TouchCalibrationErrorViewModel _touchErrorDialog;
        private bool _showExitButton;
        private bool _exitRequested;
        private string _lastEnteredEventRole;
        private readonly object _lock = new object();
        private readonly object _languageSwitchLock = new object();
        private bool _keySwitchExitOverridesButton;
        private string _warningMessageText;
        private bool _calibrationAccess;
        private bool _showToggleLanguageButton;
        private string _toggleLanguageButtonText;
        private CultureInfo _primaryCulture;
        private CultureInfo _secondaryCulture;
        private bool _useOperatorCultureForCurrencyFormatting;

        public MenuSelectionViewModel()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().TryGetService<IPrinter>(),
                ServiceManager.GetInstance().GetService<IMeterManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IOperatorMenuLauncher>(),
                ServiceManager.GetInstance().GetService<ITime>(),
                ServiceManager.GetInstance().GetService<IButtonService>(),
                ServiceManager.GetInstance().GetService<IDialogService>(),
                ServiceManager.GetInstance().GetService<IOperatorMenuConfiguration>(),
                ServiceManager.GetInstance().GetService<IOperatorMenuGamePlayMonitor>(),
                ServiceManager.GetInstance().GetService<ISerialTouchService>(),
                ServiceManager.GetInstance().GetService<ILocalization>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MenuSelectionViewModel" /> class.
        /// </summary>
        public MenuSelectionViewModel(
            IEventBus eventBus,
            IPrinter printer,
            IMeterManager meterManager,
            IPropertiesManager propertiesManager,
            IOperatorMenuLauncher operatorMenuLauncher,
            ITime timeService,
            IButtonService buttonService,
            IDialogService dialogService,
            IOperatorMenuConfiguration configuration,
            IOperatorMenuGamePlayMonitor gamePlayMonitor,
            ISerialTouchService serialTouchService,
            ILocalization localization)
        {
            _eventBus = eventBus;
            _printer = printer;
            _meterManager = meterManager;
            _propertiesManager = propertiesManager;
            _operatorMenuLauncher = operatorMenuLauncher;
            _timeService = timeService;
            _buttonService = buttonService;
            _dialogService = dialogService;
            _configuration = configuration;
            _gamePlayMonitor = gamePlayMonitor;
            _serialTouchService = serialTouchService;
            _localization = localization;

            ShowExitButton = _configuration.GetSetting(OperatorMenuSetting.ShowExitButton, false);
            ShowToggleLanguageButton = _configuration.GetSetting(OperatorMenuSetting.ShowToggleLanguageButton, false);

            _keySwitchExitOverridesButton = _configuration.GetSetting(OperatorMenuSetting.KeySwitchExitOverridesButton, false);
            _useOperatorCultureForCurrencyFormatting = _configuration.GetSetting(OperatorMenuSetting.UseOperatorCultureForCurrencyFormatting, false);

            MenuItems = new ObservableCollection<IOperatorMenuPageLoader>();

            HandleLoadedCommand = new ActionCommand<object>(HandleLoaded);
            HandleContentRenderedCommand = new ActionCommand<object>(HandleContentRendered);
            HandleClosingCommand = new ActionCommand<object>(HandleClosing);
            LanguageChangedCommand = new ActionCommand<object>(HandleLanguageChangedCommand);
            PrintButtonCommand = new ActionCommand<object>(HandlePrintButtonCommand);
            ExitButtonCommand = new ActionCommand<object>(_ => ExitMenu());
            HelpButtonCommand = new ActionCommand<object>(HandleHelpButtonCommand);

            ConfigureSubscriptions();

            EnableOperatorMenuButtons();
            SetupLanguageSwitching();

            var access = ServiceManager.GetInstance().GetService<IOperatorMenuAccess>();
            var accessRuleSet = _configuration?.GetAccessRuleSet(this);
            if (access.HasTechnicianMode)
            {
                access.RegisterAccessRule(
                    this,
                    ApplicationConstants.TechnicianRole,
                    (_, _) =>
                    {
                        SetOperatorMenuRole(access.TechnicianMode);
                        access.UnregisterAccessRules(this);
                    });
            }
            else
            {
                SetOperatorMenuRole(access.TechnicianMode);
            }

            if (string.IsNullOrEmpty(accessRuleSet))
            {
                _calibrationAccess = true;
            }
            else
            {
                access.RegisterAccessRule(
                    this,
                    accessRuleSet,
                    UpdateAccess);
            }

            LoadMenus();

            if (_printer != null)
            {
                _operatorMenuPrintHandler = new OperatorMenuPrintHandler();
                _operatorMenuPrintHandler.PrinterStatusMessageUpdated += message => { PrintStatusText = message; };
                PrintStatusText = _operatorMenuPrintHandler.PrinterStatusMessage;
                _operatorMenuPrintHandler.PrintButtonStatusUpdated += OnPrintButtonStatusUpdated;
            }
            else
            {
                PrintButtonEnabledInternal = false;
                ShowCancelPrintButton = false;
                PrintStatusText = null;
            }

            UpdateDateTime();

            _dayTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(DayTimerIntervalSeconds) };
            _dayTimer.Tick += DayTimerTick;

            _dialogService = ServiceManager.GetInstance().GetService<IDialogService>();

            TopMost = _propertiesManager.GetValue("display", string.Empty) != "windowed";
        }

        /// <summary>
        ///     Gets or sets the print status text.
        /// </summary>
        /// <value>
        ///     The print status text.
        /// </value>
        public string PrintStatusText
        {
            get => _printStatusText;
            set
            {
                if (_printStatusText != value)
                {
                    _printStatusText = value;
                    RaisePropertyChanged(nameof(PrintStatusText));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the content of the page title.
        /// </summary>
        /// <value>
        ///     The content of the page title.
        /// </value>
        public string PageTitleContent
        {
            get => _pageTitleContent;

            set
            {
                if (_pageTitleContent != value)
                {
                    _pageTitleContent = value;
                    RaisePropertyChanged(nameof(PageTitleContent));
                    RaisePropertyChanged(nameof(IsPageTitleVisible));
                    RefreshPageOperatorLabel();
                }
            }
        }

        public bool IsPageTitleVisible => !string.IsNullOrEmpty(PageTitleContent);

        /// <summary>
        ///     Gets or sets a value indicating whether the main Print button is enabled
        /// </summary>
        public bool PrintButtonEnabled
        {
            get => _printButtonEnabled;

            set
            {
                _printButtonEnabled = value;
                RaisePropertyChanged(nameof(PrintButtonEnabled));
            }
        }

        /// <summary>
        ///     Value for whether printing is allowed on a page regardless of main print button stats
        /// </summary>
        public bool PrintingAllowed
        {
            get => _printingAllowed;

            set
            {
                if (_printingAllowed != value)
                {
                    _printingAllowed = value;
                    PublishPrintButtonStatusEvent();
                }
            }
        }

        /// <summary>
        ///     Gets or sets the content of the operator menu label.
        /// </summary>
        /// <value>
        ///     The content of the operator menu label.
        /// </value>
        public string OperatorMenuLabelContent
        {
            get => _operatorMenuLabelContent;

            set
            {
                if (_operatorMenuLabelContent != value)
                {
                    _operatorMenuLabelContent = value;
                    RaisePropertyChanged(nameof(OperatorMenuLabelContent));
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether [credit balance visible].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [credit balance visible]; otherwise, <c>false</c>.
        /// </value>
        public bool CreditBalanceVisible
        {
            get => _creditBalanceVisible;

            set
            {
                if (_creditBalanceVisible != value)
                {
                    _creditBalanceVisible = value;
                    RaisePropertyChanged(nameof(CreditBalanceVisible));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the content of the credit balance.
        /// </summary>
        /// <value>
        ///     The content of the credit balance.
        /// </value>
        public string CreditBalanceContent
        {
            get => _creditBalanceContent;

            set
            {
                if (_creditBalanceContent != value)
                {
                    _creditBalanceContent = value;
                    RaisePropertyChanged(nameof(CreditBalanceContent));
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether [exit button focused].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [exit button focused]; otherwise, <c>false</c>.
        /// </value>
        public bool ExitButtonFocused
        {
            get => _exitButtonFocused;

            set
            {
                if (_exitButtonFocused != value)
                {
                    _exitButtonFocused = value;
                    RaisePropertyChanged(nameof(ExitButtonFocused));
                }
            }
        }


        /// <summary>
        ///     Gets or sets the current date time.
        /// </summary>
        public DateTime CurrentDateTime
        {
            get => _currentDateTime;

            set
            {
                if (_currentDateTime != value)
                {
                    _currentDateTime = value;
                    RaisePropertyChanged(nameof(CurrentDateTime));
                }
            }
        }

        public IOperatorMenuPageLoader SelectedItem
        {
            get => _selectedItem;

            set
            {
                if (_selectedItem == value || value == null)
                {
                    return;
                }

                if (_selectedItem != null)
                {
                    _selectedItem.OnEnabledChanged -= SelectedItemEnabledChanged;
                }

                SubscribeToSelectedItemPropertyChanged(false);

                _selectedItem = value;
                RaisePropertyChanged(nameof(SelectedItem));
                RaisePropertyChanged(nameof(CanCalibrateTouchScreens));
                IsLoadingData = false;

                if (_selectedItem != null)
                {
                    _selectedItem.OnEnabledChanged += SelectedItemEnabledChanged;

                    if (!_selectedItem.IsMultiPage)
                    {
                        PageTitleContent = _selectedItem.PageName;
                    }
                }

                SubscribeToSelectedItemPropertyChanged(true);
            }
        }

        public bool IsLoadingData
        {
            get => _isLoadingData;
            set
            {
                if (_isLoadingData != value)
                {
                    _isLoadingData = value;
                    RaisePropertyChanged(nameof(IsLoadingData));
                    SetPrintButtonEnabled();
                }
            }
        }

        public bool DataEmpty
        {
            get => _dataEmpty;
            set
            {
                if (_dataEmpty != value)
                {
                    _dataEmpty = value;
                    RaisePropertyChanged(nameof(DataEmpty));
                    SetPrintButtonEnabled();
                }
            }
        }

        /// <summary>
        ///     Gets or sets the ability to cancel printing in the UI
        /// </summary>
        public bool ShowCancelPrintButton
        {
            get => _showCancelPrintButton;

            set
            {
                if (_showCancelPrintButton != value)
                {
                    _showCancelPrintButton = value;
                    RaisePropertyChanged(nameof(ShowCancelPrintButton));

                    if (_showCancelPrintButton)
                    {
                        CancelButtonEnabled = true;
                    }
                }
            }
        }

        public bool CancelButtonEnabled
        {
            get => _cancelButtonEnabled;
            set
            {
                _cancelButtonEnabled = value;
                RaisePropertyChanged(nameof(CancelButtonEnabled));
            }
        }

        public bool TopMost { get; }

        public ObservableCollection<IOperatorMenuPageLoader> MenuItems { get; }

        public ICommand HandleLoadedCommand { get; set; }

        public ICommand HandleClosingCommand { get; set; }

        public ICommand HandleContentRenderedCommand { get; set; }

        public ICommand LanguageChangedCommand { get; set; }

        public ICommand PrintButtonCommand { get; set; }

        public ICommand ExitButtonCommand { get; set; }

        public ICommand HelpButtonCommand { get; set; }

        public string SoftwareVersion { get; private set; }

        public string DemoModeText { get; private set; }

        public ObservableCollection<string> SupportedLanguages { get; }

        public string WarningMessageText
        {
            get => _warningMessageText;
            set => SetProperty(ref _warningMessageText, value, nameof(WarningMessageText));
        }

        public string PopupText
        {
            get => _popupText;
            private set
            {
                if (_popupText != value)
                {
                    _popupText = value;
                    RaisePropertyChanged(nameof(PopupText));
                    PopupOpen = !string.IsNullOrEmpty(PopupText);
                }
            }
        }

        public bool PopupOpen
        {
            get => _popupOpen;
            set
            {
                if (_popupOpen != value)
                {
                    _popupOpen = value;
                    RaisePropertyChanged(nameof(PopupOpen));

                    if (_popupOpen)
                    {
                        Popup_OnOpened();
                        if (PopupCloseOnLostFocus)
                        {
                            Touch.FrameReported += Touch_FrameReported;
                        }
                    }
                    else
                    {
                        TimerElapsed(null, null);
                        PopupText = null;
                        if (PopupCloseOnLostFocus)
                        {
                            Touch.FrameReported -= Touch_FrameReported;
                        }
                    }

                    if (SelectedItem?.ViewModel != null)
                    {
                        SelectedItem.ViewModel.PopupOpen = _popupOpen;
                    }
                }
            }
        }

        public int PopupTimeoutSeconds
        {
            get => _popupTimeoutSeconds > 0 ? _popupTimeoutSeconds : PopupPlacementTarget == null ? 2 : 20;
            set => _popupTimeoutSeconds = value;
        }

        public bool PopupCloseOnLostFocus { get; private set; }

        public bool PopupStaysOpen => PopupPlacementTarget != null;

        public int PopupFontSize => PopupPlacementTarget == null ? 24 : 16;

        public PlacementMode PopupPlacement => PopupPlacementTarget == null ? PlacementMode.Center : PlacementMode.Left;

        public UIElement PopupPlacementTarget
        {
            get => _popupPlacementTarget;
            set
            {
                _popupPlacementTarget = value;
                RaisePropertyChanged(nameof(PopupPlacement));
                RaisePropertyChanged(nameof(PopupPlacementTarget));
                RaisePropertyChanged(nameof(PopupFontSize));
            }
        }

        public bool CanCalibrateTouchScreens => !_dialogService.IsDialogOpen &&
                                                _calibrationAccess &&
                                                (SelectedItem?.ViewModel?.CanCalibrateTouchScreens ?? false);

        public bool ShowMainDoorText => !_dialogService.IsDialogOpen &&
                                        !_calibrationAccess &&
                                        (SelectedItem?.ViewModel?.CanCalibrateTouchScreens ?? false);

        public bool ShowExitButton
        {
            get => _showExitButton;
            set
            {
                if (_showExitButton != value)
                {
                    _showExitButton = value;
                    RaisePropertyChanged(nameof(ShowExitButton));
                }
            }
        }

        public bool ShowToggleLanguageButton
        {
            get => _showToggleLanguageButton;
            set
            {
                if (_showToggleLanguageButton != value)
                {
                    _showToggleLanguageButton = value;
                    RaisePropertyChanged(nameof(ShowToggleLanguageButton));
                }
            }
        }

        public string ToggleLanguageButtonText
        {
            get => _toggleLanguageButtonText;
            set
            {
                if (_toggleLanguageButtonText != value)
                {
                    _toggleLanguageButtonText = value;
                    RaisePropertyChanged(nameof(ToggleLanguageButtonText));
                }
            }
        }

        private bool PrintButtonEnabledInternal
        {
            get => _printButtonEnabledInternal;

            set
            {
                _printButtonEnabledInternal = value;
                SetPrintButtonEnabled();
            }
        }

        private bool MainPrintButtonEnabled
        {
            get => _mainPrintButtonEnabled;
            set
            {
                if (_mainPrintButtonEnabled != value)
                {
                    _mainPrintButtonEnabled = value;
                    RaisePropertyChanged(nameof(MainPrintButtonEnabled));
                    SetPrintButtonEnabled();
                }
            }
        }

        /// <summary>
        ///     Dispose of managed objects used by this class
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Event fired when text is changed.</summary>
        public event EventHandler<EventArgs> LoadFinished;

        /// <summary>
        ///     Raised when the displays have changed
        /// </summary>
        public event EventHandler DisplayChanged;

        /// <summary>
        ///     Gets the window dimensions.
        /// </summary>
        /// <param name="windowedScreenHeightPropertyName">Name of the windowed screen height property.</param>
        /// <param name="defaultWindowedHeight">Default height of the windowed.</param>
        /// <param name="windowedScreenWidthPropertyName">Name of the windowed screen width property.</param>
        /// <param name="defaultWindowedWidth">Default width of the windowed.</param>
        /// <returns>
        ///     Window dimensions. Item one is height. Item two is width.
        /// </returns>
        public Tuple<int, int> GetWindowDimensions(
            string windowedScreenHeightPropertyName,
            string defaultWindowedHeight,
            string windowedScreenWidthPropertyName,
            string defaultWindowedWidth)
        {
            var height =
                int.Parse(
                    (string)_propertiesManager.GetProperty(windowedScreenHeightPropertyName, defaultWindowedHeight),
                    CultureInfo.InvariantCulture);
            var width =
                int.Parse(
                    (string)_propertiesManager.GetProperty(windowedScreenWidthPropertyName, defaultWindowedWidth),
                    CultureInfo.InvariantCulture);

            return new Tuple<int, int>(height, width);
        }

        /// <summary>
        ///     Setups the properties.
        /// </summary>
        public void SetupProperties()
        {
            // Get the system version from the properties manager
            var systemVersion = (string)_propertiesManager.GetProperty(
                KernelConstants.SystemVersion,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Unknown));
            var versionBuilder = new StringBuilder(20);
            versionBuilder.Append(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Version)).Append(" ")
                .Append(systemVersion);
            SoftwareVersion = versionBuilder.ToString();
            var demoModeActive = (bool)_propertiesManager.GetProperty(DemoModeProperty, false);
            DemoModeText = demoModeActive
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DemoModeText)
                : string.Empty;
        }

        public void SetInitialSelectedMenuItem()
        {
            SelectedItem = MenuItems.FirstOrDefault();
        }

        private void ConfigureSubscriptions()
        {
            _eventBus.Subscribe<SystemDownEvent>(this, HandleEvent);
            _eventBus.Subscribe<UpdateOperatorMenuRoleEvent>(this, e => UpdateOperatorMenuRole(e.IsTechnicianRole));
            _eventBus.Subscribe<TouchDisplayDisconnectedEvent>(this, HandleTouchScreenEvent);
            _eventBus.Subscribe<OperatorMenuPrintJobEvent>(this, HandleOperatorMenuPrintJob);
            _eventBus.Subscribe<OperatorMenuPageLoadedEvent>(this, OnPageLoaded);
            _eventBus.Subscribe<OperatorMenuPrintJobStartedEvent>(this, OnPrintJobStarted);
            _eventBus.Subscribe<OperatorMenuPrintJobCompletedEvent>(this, OnPrintJobCompleted);
            _eventBus.Subscribe<OperatorMenuPopupEvent>(this, OnShowPopup);
            _eventBus.Subscribe<OperatorMenuWarningMessageEvent>(this, OnUpdateWarningMessage);
            _eventBus.Subscribe<ServiceAddedEvent>(this, UpdatePrinter);
            _eventBus.Subscribe<OffEvent>(this, HandleEvent);
            _eventBus.Subscribe<DisplayConnectedEvent>(this, HandleEvent);
            _eventBus.Subscribe<TouchCalibrationCompletedEvent>(this, HandleEvent);
            _eventBus.Subscribe<SerialTouchCalibrationCompletedEvent>(this, HandleEvent);
            _eventBus.Subscribe<DialogOpenedEvent>(this, _ => RaisePropertyChanged(nameof(CanCalibrateTouchScreens)));
            _eventBus.Subscribe<DialogClosedEvent>(this, _ => RaisePropertyChanged(nameof(CanCalibrateTouchScreens)));
        }

        private void OnPrintButtonStatusUpdated(PrintButtonStatus status)
        {
            switch (status)
            {
                case PrintButtonStatus.Print:
                    PrintButtonEnabledInternal = true;
                    ShowCancelPrintButton = false;
                    break;
                case PrintButtonStatus.PrintDisabled:
                    PrintButtonEnabledInternal = false;
                    ShowCancelPrintButton = false;
                    break;
                case PrintButtonStatus.Cancel:
                    PrintButtonEnabledInternal = false;
                    ShowCancelPrintButton = true;
                    CancelButtonEnabled = true;
                    break;
                case PrintButtonStatus.CancelDisabled:
                    PrintButtonEnabledInternal = false;
                    ShowCancelPrintButton = true;
                    CancelButtonEnabled = false;
                    break;
            }
        }

        // VLT-7149 & VLT-9138 : when the audit menu is up and a touch screen is disconnected the screen becomes
        // unresponsive so catch the disconnect event and close the menu and reopen
        private void HandleTouchScreenEvent(IEvent theEvent)
        {
            Log.Info("Operator Menu exited to reset touch screen(s) upon disconnection");
            ExitMenu();
            _operatorMenuLauncher.Show();
        }

        private void OnShowPopup(OperatorMenuPopupEvent evt)
        {
            if (evt.PopupOpen)
            {
                PopupCloseOnLostFocus = evt.CloseOnLostFocus;
                PopupPlacementTarget = evt.TargetElement;
                PopupTimeoutSeconds = evt.PopupTimeoutSeconds;
                PopupText = evt.PopupText;
            }
            else
            {
                PopupOpen = false;
            }
        }

        private void Popup_OnOpened()
        {
            _popupTimer = new Timer(TimeSpan.FromSeconds(PopupTimeoutSeconds).TotalMilliseconds) { Enabled = true };

            _popupTimer.Elapsed += TimerElapsed;
            _popupTimer.Start();
        }

        private void TimerElapsed(object sender, EventArgs e)
        {
            PopupOpen = false;
            _popupTimer.Stop();
            _popupTimer.Elapsed -= TimerElapsed;
            _popupTimer.Enabled = false;
        }

        private void OnUpdateWarningMessage(OperatorMenuWarningMessageEvent e)
        {
            WarningMessageText = e.Message;
        }

        /// <summary>
        ///     Sets the buttons that are needed to navigate in the operator menu to enabled.
        /// </summary>
        private void EnableOperatorMenuButtons()
        {
            // This will enable the buttons that are needed to use the operator menu.
            // They are: Cash Out - 1 (Scroll up); Call Attendant - 14(Scroll Down);
            ////         Play (Spin on 3RM) - 0 (Next[Tab]); Bet1 - 3 (Previous[Shift Tab]);
            ////         Bet Max(Bet 3 on 3RM) - 2 (Select); BetB - 9 (Left Arrow);
            ////         BetC - 10 (Right Arrow)
            _buttonService.Enable(
                new Collection<int>
                {
                    1,
                    14,
                    0,
                    3,
                    2,
                    9,
                    10
                });
        }

        /// <summary>
        ///     Converts the transaction amount from millicents into dollars
        /// </summary>
        /// <param name="amount">The amount to convert from millicents to dollars</param>
        /// <returns>The transaction amount in dollars.</returns>
        private decimal GetDollarAmount(long amount)
        {
            var multiplier = _propertiesManager.GetValue(
                ApplicationConstants.CurrencyMultiplierKey,
                ApplicationConstants.DefaultCurrencyMultiplier);

            return amount / (decimal)multiplier;
        }

        private void HandleLoaded(object obj)
        {
            SubscribeToEvents();
            LoadFinished?.Invoke(this, EventArgs.Empty);
            RefreshPageOperatorLabel();

            ShowCancelPrintButton = false;
            UpdateCreditBalance();
            CreditBalanceVisible = _configuration.GetSetting(OperatorMenuSetting.CreditBalanceVisible, false);
            _buttonNavigator.Initialize();
            _dayTimer.Start();
        }

        private void HandleContentRendered(object obj)
        {
            // check on load to see if the operator key has been turned back off while we were loading
            if (NoOperatorKey())
            {
                ExitMenu();
            }
            else
            {
                SetInitialSelectedMenuItem();
                PublishEnteredEvent();
            }
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<ExitRequestedEvent>(this, _ => UnsubscribeFromEvents());
            _eventBus.Subscribe<PageTitleEvent>(this, HandlePageTitleEvent);
            _eventBus.Subscribe<BankBalanceChangedEvent>(this, HandleBankBalanceChanged);
            _eventBus.Subscribe<EnableOperatorMenuEvent>(this, HandleEnableOperatorMenuEvent);
        }

        private void UnsubscribeFromEvents()
        {
            _eventBus.UnsubscribeAll(this);
        }

        private void SubscribeToSelectedItemPropertyChanged(bool subscribe)
        {
            var vm = _selectedItem?.ViewModel;
            if (vm != null)
            {
                if (subscribe)
                {
                    DataEmpty = vm.DataEmpty;
                    IsLoadingData = vm.IsLoadingData;
                    MainPrintButtonEnabled = vm.MainPrintButtonEnabled;
                    vm.PropertyChanged += SelectedItem_OnPropertyChanged;
                }
                else
                {
                    vm.PropertyChanged -= SelectedItem_OnPropertyChanged;
                }
            }
        }

        private void SelectedItem_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is IOperatorMenuPageViewModel vm)
            {
                if (e.PropertyName == nameof(IsLoadingData))
                {
                    IsLoadingData = vm.IsLoadingData;
                }
                else if (e.PropertyName == nameof(DataEmpty))
                {
                    DataEmpty = vm.DataEmpty;
                }
                else if (e.PropertyName == nameof(MainPrintButtonEnabled))
                {
                    MainPrintButtonEnabled = vm.MainPrintButtonEnabled;
                }
                else if (e.PropertyName == nameof(vm.CanCalibrateTouchScreens))
                {
                    RaisePropertyChanged(nameof(CanCalibrateTouchScreens));
                    RaisePropertyChanged(nameof(ShowMainDoorText));
                }
            }
        }

        private void RefreshPageOperatorLabel()
        {
            var menuTitle = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OperatorMenu);

            OperatorMenuLabelContent = _configuration.GetSetting(OperatorMenuSetting.ShowOperatorRole, false)
                ? $"{GetRoleText()} {menuTitle}" // Only necessary for VLT markets.
                : $"{menuTitle}";
        }

        private void HandleHelpButtonCommand(object obj)
        {
            // TODO: This functionality will implement later.
        }

        private void ExitMenu()
        {
            lock (_exitLock)
            {
                if (_exitRequested)
                {
                    return;
                }

                _exitRequested = true;
            }

            Log.Debug("Exit from Operator Menu has been requested");

            CancelPrint();

            // This will schedule a callback to close the window
            _operatorMenuLauncher.Close();
        }

        private void HandlePrintButtonCommand(object obj)
        {
            if (!ShowCancelPrintButton)
            {
                if (_printer?.Enabled ?? false)
                {
                    _eventBus.Publish(new PrintButtonClickedEvent());
                    OnPrintJobStarted(null);
                }
            }
            else
            {
                CancelPrint();
            }
        }

        private void SetupLanguageSwitching()
        {
            var provider = _localization.GetProvider(CultureFor.Operator);
            var cultures = provider.AvailableCultures;

            CultureInfo currentCulture = provider.CurrentCulture;
            CultureInfo defaultCulture;

            if (provider is OperatorCultureProvider operatorCultureProvider)
            {
                defaultCulture = operatorCultureProvider.DefaultCulture;
            }
            else
            {
                defaultCulture = provider.AvailableCultures.FirstOrDefault();
            }

            _primaryCulture = defaultCulture;

            if (cultures.Count <= 1)
            {
                ShowToggleLanguageButton = false;
                return;
            }
            else if (cultures.Count == 2)
            {
                _secondaryCulture = cultures.FirstOrDefault(
                    c => !c.Equals(defaultCulture) && !c.Equals(currentCulture));
            }
            else
            {
                // We always include en-US in operator cultures even if not explicitly configured.
                // Filter out en-US if it's in our list of operator cultures and we have more than two.
                // We can revisit later if we ever need to have en-US and another culture be switched between but want
                // the list of operator cultures to be more than two (probably a niche scenario).

                _secondaryCulture = cultures.FirstOrDefault(
                    c => !c.Equals(defaultCulture) && !c.Equals(currentCulture) &&
                            !c.Equals(new CultureInfo(ApplicationConstants.DefaultLanguage)));
            }

            if (_secondaryCulture == null && currentCulture.Equals(defaultCulture))
            {
                ShowToggleLanguageButton = false;
                return;
            }
            else if (_secondaryCulture == null && !currentCulture.Equals(defaultCulture))
            {
                _secondaryCulture = currentCulture;
            }

            UpdateToggleLanguageButton();
        }

        private void UpdateToggleLanguageButton()
        {
            if (!ShowToggleLanguageButton)
            {
                return;
            }

            var otherCulture = GetTogglableCulture();

            if (otherCulture != null)
            {
                var buttonText = otherCulture.TextInfo.ToTitleCase(otherCulture.NativeName);

                // Strip off locale name unless both primary and secondary cultures have same language name
                if (!_primaryCulture.ThreeLetterISOLanguageName.Equals(_secondaryCulture.ThreeLetterISOLanguageName, StringComparison.OrdinalIgnoreCase))
                {
                    buttonText = buttonText.Split(' ').FirstOrDefault();
                }

                ToggleLanguageButtonText = buttonText;
            }
        }

        private void HandleClosing(object obj)
        {
            UnsubscribeFromEvents();
            _dayTimer?.Stop();
            _lastEnteredEventRole = null;
        }

        private void HandleLanguageChangedCommand(object obj)
        {
            lock (_languageSwitchLock)
            {
                var otherCulture = GetTogglableCulture();

                if (otherCulture != null)
                {
                    _propertiesManager.SetProperty(ApplicationConstants.LocalizationOperatorCurrentCulture, otherCulture.Name);

                    UpdateToggleLanguageButton();
                    UpdateCreditBalance();

                    if (_selectedItem != null && !_selectedItem.IsMultiPage)
                    {
                        PageTitleContent = _selectedItem.PageName;
                    }
                }
            }
        }

        private CultureInfo GetTogglableCulture()
        {
            return Localizer.For(CultureFor.Operator).CurrentCulture.Equals(_primaryCulture) ? _secondaryCulture : _primaryCulture;
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                SubscribeToSelectedItemPropertyChanged(false);

                _selectedItem = null;

                foreach (var item in MenuItems)
                {
                    item.Dispose();
                }

                MenuItems?.Clear();
                UnsubscribeFromEvents();

                _buttonNavigator.Dispose();

                _dayTimer?.Stop();

                // ReSharper disable once UseNullPropagation
                if (_operatorMenuPrintHandler != null)
                {
                    _operatorMenuPrintHandler.Dispose();
                }
                // ReSharper disable once UseNullPropagation
                if (_touchErrorDialog != null)
                {
                    _touchErrorDialog.Dispose();
                }
            }

            _dayTimer = null;
            _operatorMenuPrintHandler = null;
            _touchErrorDialog = null;
            _disposed = true;
        }

        private void CancelPrint()
        {
            if (ShowCancelPrintButton)
            {
                if (ShowCancelPrintButton)
                {
                    _operatorMenuPrintHandler?.CancelPrint();
                }
            }
        }

        private void DayTimerTick(object sender, EventArgs e)
        {
            UpdateDateTime();
        }

        private void UpdateDateTime()
        {
            if (_timeService != null)
            {
                CurrentDateTime = _timeService.GetLocationTime(DateTime.UtcNow);
            }
        }

        private void LoadMenus()
        {
            const string menuExtensionPointPath = "/Application/OperatorMenu/MainMenu";

            Log.Info("Loading the submenus...");

            var nodes = MonoAddinsHelper.GetSelectedNodes<OperatorMenuTypeExtensionNode>(menuExtensionPointPath);

            foreach (var node in nodes)
            {
                var menu = (IOperatorMenuPageLoader)node.CreateInstance();
                menu.Initialize();

                Log.Debug($"Loading submenu: {node.Id}: {menu.PageName}");

                if (menu.IsVisible)
                {
                    MenuItems.Add(menu);
                }
            }

            Log.Info("Loading the submenus...completed!");
        }

        private void EnableDisableMenuItems()
        {
        }

        private void EnableAllMenuItems()
        {
            foreach (var menuItem in MenuItems)
            {
                menuItem.IsEnabled = true;
            }
        }

        /// <summary>
        ///     VLT-4284:  This disables everything except the currently selected item in the menu
        ///     We use this when we are printing a large batch of items and we don't want
        ///     the user to change menu items until they either complete printing or
        ///     cancel the print job.
        /// </summary>
        private void DisableAllMenuItemsExceptSelected()
        {
            var menuItems = MenuItems.Where(o => o != SelectedItem);
            foreach (var menuItem in menuItems)
            {
                menuItem.IsEnabled = false;
            }
        }

        private void SelectedItemEnabledChanged(object sender, EventArgs e)
        {
            if (!SelectedItem.IsEnabled)
            {
                var newMenu = MenuItems.FirstOrDefault(o => o.IsEnabled);
                if (newMenu != null)
                {
                    SelectedItem = newMenu;
                }
            }
        }

        private void SetOperatorMenuRole(bool technicianRole)
        {
            _role = technicianRole ? ApplicationConstants.TechnicianRole : ApplicationConstants.DefaultRole;
            _propertiesManager.SetProperty(ApplicationConstants.RolePropertyKey, _role);
        }

        private void UpdateOperatorMenuRole(bool technicianRole)
        {
            SetOperatorMenuRole(technicianRole);

            // Did the exit button close the operator menu?
            if (!_operatorMenuLauncher.IsShowing)
            {
                return;
            }

            // send another operator menu entered event with the new role for logging purposes
            PublishEnteredEvent();

            var meterName = _role + "Access";
            if (_meterManager != null && _meterManager.IsMeterProvided(meterName))
            {
                _meterManager.GetMeter(meterName)?.Increment(1);
            }

            RefreshPageOperatorLabel();
        }

        private void HandleEvent(SystemDownEvent downEvent)
        {
            // If we can not calibrate the touch screens, or are in game replay, or the calibration dialog
            // is already open, or we do not have access to calibrate, or we have disabled serial touch via
            // command line, then do not allow touch calibration popup.
            if (!CanCalibrateTouchScreens ||
                _gamePlayMonitor.InReplay ||
                _dialogService.IsDialogOpen ||
                !_calibrationAccess ||
                _propertiesManager.GetValue(HardwareConstants.SerialTouchDisabled, "false") == "true")
            {
                return;
            }

            if (downEvent.LogicalId == (int)ButtonLogicalId.Play && downEvent.Enabled == false)
            {
                MvvmHelper.ExecuteOnUI(
                    () =>
                    {
                        _touchConfirmationDialog =
                            new TouchCalibrationConfirmationViewModel { EventHandle = downEvent };

                        var result = _dialogService.ShowDialog<TouchConfirmationView>(
                            this,
                            _touchConfirmationDialog,
                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TouchCalibrationDialogTitle),
                            DialogButton.Cancel);

                        if (result.HasValue)
                        {
                            _touchConfirmationDialog = null;
                        }
                    });
            }
        }

        private void HandleEvent(TouchCalibrationCompletedEvent e)
        {
            _touchConfirmationDialog?.CancelCommand.Execute(null);
            _touchErrorDialog?.CancelCommand.Execute(null);

            if (e.Success)
            {
                Log.Info("Restarting System now that touch calibration is complete.");
                _eventBus.Publish(new ExitRequestedEvent(ExitAction.Restart));
            }
            else if (!string.IsNullOrEmpty(e.Error))
            {
                Log.Error(e.Error);
                ShowTouchErrorDialog(e.DisplayMessage);
            }
        }


        private void HandleEvent(SerialTouchCalibrationCompletedEvent e)
        {
            if (_serialTouchService.PendingCalibration)
            {
                Log.Info("Requesting reboot with pending serial touch calibration.");
                _eventBus.Publish(new ExitRequestedEvent(ExitAction.Reboot));
                return;
            }

            _touchConfirmationDialog?.CancelCommand.Execute(null);
            _touchErrorDialog?.CancelCommand.Execute(null);

            Log.Info("Restarting System now that serial touch calibration is complete.");
            _eventBus.Publish(new ExitRequestedEvent(ExitAction.Restart));
        }

        private void HandleEvent(OffEvent theEvent)
        {
            if (!ShowExitButton
                && (theEvent.LogicalId == OperatorMenuLauncher.OperatorKeySwitch1LogicalId
                    || theEvent.LogicalId == OperatorMenuLauncher.OperatorKeySwitch2LogicalId))
            {
                var vm = _selectedItem?.ViewModel;

                if (vm != null && vm is OperatorMenuMultiPageViewModelBase multiPage)
                {
                    if (multiPage.SelectedPage != null &&
                        multiPage.SelectedPage.ViewModel.GetType() == typeof(KeyPageViewModel))
                    {
                        // Don't exit the operator menu via key turn while on the keys page
                        return;
                    }
                }

                ExitMenu();
            }
        }

        private void HandleEvent(DisplayConnectedEvent evt)
        {
            DisplayChanged?.Invoke(this, EventArgs.Empty);
            if (_touchErrorDialog != null)
            {
                _touchErrorDialog?.CancelCommand.Execute(null);
                var message = _touchErrorDialog?.ErrorText;
                _dialogService.DismissOpenedDialog();
                ShowTouchErrorDialog(message);
            }
        }

        private void ShowTouchErrorDialog(string message)
        {
            MvvmHelper.ExecuteOnUI(
            () =>
            {
                _touchErrorDialog = new TouchCalibrationErrorViewModel(message);

                if (_operatorMenuLauncher.IsShowing)
                {
                    var result = _dialogService.ShowDialog<TouchCalibrationErrorView>(
                        this,
                        _touchErrorDialog,
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TouchCalibrationErrorTitle),
                        DialogButton.None);

                    if (result.HasValue)
                    {
                        _touchErrorDialog = null;
                    }
                }
                else
                {
                    Log.Debug("Operator Menu is closed and Touch Calibration will not continue.");
                }
            });
        }

        private bool NoOperatorKey()
        {
            if (ShowExitButton && !_keySwitchExitOverridesButton)
            {
                return false;
            }

            var keyswitch = ServiceManager.GetInstance().GetService<IKeySwitch>();
            var action1 = keyswitch.GetKeySwitchAction(OperatorMenuLauncher.OperatorKeySwitch1LogicalId);
            var action2 = keyswitch.GetKeySwitchAction(OperatorMenuLauncher.OperatorKeySwitch2LogicalId);

            if (action1 == KeySwitchAction.Off && action2 == KeySwitchAction.Off)
            {
                return true;
            }

            return false;
        }

        private void HandleBankBalanceChanged(IEvent theEvent)
        {
            UpdateCreditBalance();
        }

        private void HandlePageTitleEvent(PageTitleEvent data)
        {
            if (string.IsNullOrEmpty(data.Content))
            {
                PageTitleContent = string.Empty;
                return;
            }

            PageTitleContent = data.Content;
        }

        private void HandleEnableOperatorMenuEvent(EnableOperatorMenuEvent evt)
        {
            if (evt.Enable)
            {
                EnableDisableMenuItems();
                SetPrintButtonEnabled();
            }
            else
            {
                DisableAllMenuItemsExceptSelected();
                PrintButtonEnabled = false;
            }
        }

        private void UpdateCreditBalance()
        {
            var balance = (long)_propertiesManager.GetProperty(PropertyKey.CurrentBalance, 0L);
            var dollarAmount = GetDollarAmount(balance);
            var culture = _useOperatorCultureForCurrencyFormatting ?
                Localizer.For(CultureFor.Operator).CurrentCulture : CurrencyExtensions.CurrencyCultureInfo;

            CreditBalanceContent = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CreditBalance) + ": " +
                                   dollarAmount.FormattedCurrencyString(false, culture);
        }

        private void HandleOperatorMenuPrintJob(OperatorMenuPrintJobEvent printJob)
        {
            _operatorMenuPrintHandler?.PrintTickets(printJob.TicketsToPrint);
        }

        private void PublishPrintButtonStatusEvent()
        {
            var message = new PrintButtonStatusEvent(PrintingAllowed);
            _eventBus.Publish(message);
        }

        private void SetPrintButtonEnabled()
        {
            // printing can be allowed even when the main button is disabled
            PrintingAllowed = PrintButtonEnabledInternal && _printer != null && _printer.CanPrint && _printer.Enabled &&
                              !IsLoadingData && !DataEmpty;

            PrintButtonEnabled = PrintingAllowed && MainPrintButtonEnabled;
        }

        private void OnPageLoaded(OperatorMenuPageLoadedEvent loadedEvent)
        {
            SetPrintButtonEnabled();
            PublishPrintButtonStatusEvent();
        }

        private void OnPrintJobStarted(OperatorMenuPrintJobStartedEvent evt)
        {
            lock (_printLock)
            {
                PrintButtonEnabledInternal = false;
                DisableAllMenuItemsExceptSelected();
                SetPrintButtonEnabled();
            }
        }

        private void OnPrintJobCompleted(OperatorMenuPrintJobCompletedEvent evt)
        {
            lock (_printLock)
            {
                EnableAllMenuItems();
            }
        }

        private void UpdatePrinter(ServiceAddedEvent e)
        {
            if (e.ServiceType == typeof(IPrinter))
            {
                _printer = ServiceManager.GetInstance().TryGetService<IPrinter>();
            }
        }

        private void Touch_FrameReported(object sender, TouchFrameEventArgs e)
        {
            if (PopupOpen && PopupCloseOnLostFocus)
            {
                PopupOpen = false;
            }
        }

        private void PublishEnteredEvent()
        {
            lock (_lock)
            {
                if (_role == _lastEnteredEventRole)
                {
                    return;
                }
                _lastEnteredEventRole = _role;

                var currentOperatorId = (string)_propertiesManager.GetProperty(ApplicationConstants.CurrentOperatorId, string.Empty);
                _eventBus.Publish(new OperatorMenuEnteredEvent(GetRoleText(), currentOperatorId));
            }
        }

        private string GetRoleText()
        {
            return _role == ApplicationConstants.DefaultRole || string.IsNullOrEmpty(_role)
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MenuTitleRoleAdmin)
                : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MenuTitleRoleTechnician);
        }

        private void UpdateAccess(bool access, OperatorMenuAccessRestriction restriction)
        {

            _calibrationAccess = access;

            RaisePropertyChanged(nameof(CanCalibrateTouchScreens));
            RaisePropertyChanged(nameof(ShowMainDoorText));
        }

        /// <inheritdoc />
        ~MenuSelectionViewModel()
        {
            Dispose(false);
        }
    }
}