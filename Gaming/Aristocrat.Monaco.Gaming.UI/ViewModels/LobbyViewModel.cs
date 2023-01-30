namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts;
    using Application.Contracts.EdgeLight;
    using Application.Contracts.Localization;
    using Commands;
    using Contracts;
    using Contracts.Lobby;
    using Contracts.Models;
    using Contracts.Tickets;
    using Converters;
    using Hardware.Contracts.ButtonDeck;
    using Hardware.Contracts.EdgeLighting;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Touch;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using Models;
    using Monaco.UI.Common;
    using Monaco.UI.Common.Extensions;
    using MVVM;
    using MVVM.Command;
    using MVVM.ViewModel;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;
    using Accounting.Contracts.Handpay;
    using Application.Contracts.Drm;
    using Cabinet.Contracts;
    using Common;
    using Contracts.Events;
    using Contracts.InfoBar;
    using Contracts.PlayerInfoDisplay;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Button;
    using Timers;
    using Utils;
    using Vgt.Client12.Application.OperatorMenu;
    using Views.Controls;
    using Views.Lobby;
    using Size = System.Windows.Size;
#if !(RETAIL)
    using Vgt.Client12.Testing.Tools;
    using Events;
#endif

    /// <summary>
    ///     Defines the LobbyViewModel class
    /// </summary>
    public partial class LobbyViewModel : BaseEntityViewModel, IMessageDisplayHandler, IDisposable, IPlayerInfoDisplayScreensContainer
    {
        private const double IdleTimerIntervalSeconds = 15.0;
        private const double IdleTextTimerIntervalSeconds = 30.0;
        private const double RotateTopImageIntervalInSeconds = 10.0;
        private const double RotateTopperImageIntervalInSeconds = 10.0;
        private const double RotateSoftErrorTextIntervalInSeconds = 3.0;
        private const double PrintHelplineWaitTimeInSeconds = 15.0;
        private const double VbdConfirmationTimeOutInSeconds = 30.0;
        private const double DebugCurrencyIntervalInSeconds = 2.0;
        private const double CashOutMinimumIntervalInSeconds = 3.0;
        private const double MaxDebugCurrencyAllowed = 100.0;
        private const double MaximumBlinkingIdleTextWidth = 1000;
        private const double NewGameTimerIntervalInHours = 1;
        private const byte DefaultAlertVolume = 100;
        private const double PaperInChuteAlertVolumeRate = 0.8;
        private const string ResponsibleGamingPropNameDialogVisible = "IsTimeLimitDialogVisible";
        private const string ResponsibleGamingPropNameDialogPending = "ShowTimeLimitDlgPending";
        private const string ResponsibleGamingPropNameDialogState = "TimeLimitDialogState";
        private const string ResponsibleGamingPropNameDialogResourceKey = "ResponsibleGamingDialogResourceKey";
        private const int DebugCashAmount = 20;
        private const int DemonstrationCashInAmount = 10;
        private const string TopImageDefaultResourceKey = "TopBackground";
        private const string TopImageAlternateResourceKey = "TopBackgroundAlternate";
        private const string LobbyIdleTextDefaultResourceKey = "LobbyIdleTextDefault";
        private const string TopperImageDefaultResourceKey = "TopperBackground";
        private const string TopperImageAlternateResourceKey = "TopperBackgroundAlternate";
        private new static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private const string IdleTextFamilyName = "Segoe UI";
        private const double OpacityNone = 0.0;
        private const double OpacityFifth = 0.2;
        private const double OpacityHalf = 0.5;
        private const double OpacityFull = 1.0;

        private readonly IBank _bank;
        private readonly IButtonDeckFilter _buttonDeckFilter;
        private readonly IButtonLamps _buttonLamps;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly IGameHistory _gameHistory;
        private readonly IGameOrderSettings _gameOrderSettings;
        private readonly IGameRecovery _gameRecovery;
        private readonly IGameDiagnostics _gameDiagnostics;
        private readonly IGamePlayState _gameState;
        private readonly IGameStorage _gameStorage;
        private readonly ILobbyStateManager _lobbyStateManager;
        private readonly IMessageDisplay _messageDisplay;
        private readonly IOperatorMenuLauncher _operatorMenu;
        private readonly IPropertiesManager _properties;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly ITransferOutHandler _transferOutHandler;
        private readonly IRuntimeFlagHandler _runtime;
        private readonly IGameService _gameService;
        private readonly ISessionInfoService _sessionInfoService;
        private readonly IAttendantService _attendant;
        private readonly IEdgeLightingStateManager _edgeLightingStateManager;
        private readonly ICashableLockupProvider _cashableLockupProvider;
        private readonly ICabinetDetectionService _cabinetDetectionService;
        private readonly IAudio _audio;
        private readonly ICashoutController _cashoutController;
        private readonly IAttractConfigurationProvider _attractInfoProvider;
        private readonly IPlayerInfoDisplayManager _playerInfoDisplayManager;
        private readonly IReserveService _reserveService;
        // Broadcasting platform messages to a game

        private readonly DisplayableMessage _disableCountdownMessage;
        private readonly string _disableCountdownTimeFormat = "m\\:ss";
        private bool _broadcastDisableCountdownMessagePending;
        private bool _justCashedOut;

        private IResponsibleGaming _responsibleGaming;
        private IEventBus _eventBus;

        private readonly AgeWarningTimer _ageWarningTimer;

        private ITimer _idleTextTimer;
        private ITimer _idleTimer;
        private ITimer _disableCountdownTimer;
        private ITimer _attractTimer;
        private ITimer _printHelplineTicketTimer;
        private ITimer _renderTimer;
        private ITimer _rotateTopImageTimer;
        private ITimer _rotateTopperImageTimer;
        private ITimer _rotateSoftErrorTextTimer;
        private ITimer _responsibleGamingInfoTimeOutTimer;
        private ITimer _vbdConfirmationTimeOutTimer;
        private ITimer _debugCurrencyTimer;
        private ITimer _cashOutTimer;
        private ITimer _newGameTimer;

        private readonly object _responsibleGamingLockObject = new object();
        private readonly object _resizeLock = new object();
        private readonly object _attractLock = new object();
        private readonly object _edgeLightLock = new object();

        private bool _disposed;
        private bool _inPrintHelplineTicketWaitPeriod;
        private bool _isBottomAttractFeaturePlaying;
        private bool _isBottomAttractVisible;
        private bool _isBottomLoadingScreenVisible;
        private bool _isDisabledCountdownMessageSuppressed;
        private bool _isIdleTextPaused;
        private bool _isScrollingIdleTextVisible;
        private bool _isInLobby = true;
        private bool _isLobbyVisible = true;
        private bool _isVbdMalfunctionOverlayVisible;
        private bool _isPrimaryLanguageSelected = true;
        private bool _mainInfoBarOpenRequested;
        private bool _isResponsibleGamingInfoDlgVisible;
        private bool _isShowingAlternateTopImage;
        private bool _isShowingAlternateTopperImage;
        private bool _isTopAttractFeaturePlaying;
        private bool _isTopLoadingScreenVisible;
        private bool _isTopperLoadingScreenVisible;
        private bool _isTopScreenRenderingDisabled;
        private bool _isTopperScreenRenderingDisabled;
        private bool _isVbdCashOutDialogVisible;
        private bool _isVbdServiceDialogVisible;
        private bool _isVbdRenderingDisabled = true;
        private bool _largeGameIconsEnabled;
        private bool _lobbyActivated;
        private bool _multiLanguageEnabled;
        private bool _normalGameExitReceived;
        private bool _printingHelplineTicket;
        private bool _printingHelplineWhileResponsibleGamingReset;
        private bool _responsibleGamingInfoWhileResponsibleGamingReset;
        private bool _printingReprintTicket;
        private bool _responsibleGamingCashOutInProgress;
        private bool _responsibleGamingDialogResetWhenOperatorMenuEntered;
        private bool _recoveryOnStartup;
        private bool _gameLaunchOnStartup;
        private bool _isDebugCurrencyButtonVisible;
        private bool _disableDebugCurrency;
        private bool _nextAttractModeLanguageIsPrimary = true;
        private bool _lastInitialAttractModeLanguageIsPrimary = true;
        private bool _initialLanguageEventSent;
        private bool _vbdServiceButtonDisabled;
        private bool _isDemonstrationMode;
        private bool _serviceButtonVisible;
        private bool _volumeButtonVisible;
        private string _bottomAttractVideoPath;
        private double _chooseGameOffsetY;
        private double _cashoutDialogOpacity;
        private bool _cashoutDialogHidden;
        private GameInfo _selectedGame;
        private bool _isInitialStartup = true;

        private double _credits;
        private int _currentAttractIndex;
        private int _denomFilter = -1;

        private TimeSpan _disableCountdownTimeRemaining;
        private bool _disabledOnStartup;

        // Subset of _gameList that is being viewed.
        private int _displayedPageNumber = 1;
        private GameType _gameFilter = GameType.Undefined;
        private ObservableCollection<GameInfo> _gameList = new ObservableCollection<GameInfo>();
        private string _gameLoadingScreenPath;
        private int _gamesPerPage;
        private string _idleText;
        private BannerDisplayMode _bannerDisplayMode;

        private GameInfo _launchGameAfterAgeWarning;
        private LobbyButtonDeckRenderer _lobbyButtonDeckRenderer;

        private string _mainMessage = "Select a Game";
        private string _paidMeterValue = string.Empty;

        private int _paid;
        private LobbyCashOutDialogState _cashOutDialogState = LobbyCashOutDialogState.Hidden;
        private ResponsibleGamingSessionState _responsibleGamingSessionState = ResponsibleGamingSessionState.Stopped;
        private string _topAttractVideoPath;
        private LobbyVbdVideoState _vbdVideoState = LobbyVbdVideoState.Disabled;
        private DateTime _denomCheck = DateTime.UtcNow;
        private int _gameCount;
        private bool _disableLobbyInput;
        private double _aspectRatio;
        private double _mainInfoBarHeight;
        private double _replayNavigationBarHeight;
        private double _gameControlHeight;
        private double _gameControlWidth;
        private string _topImageResourceKey;
        private int _consecutiveAttractCount;
        private int _attractModeTopImageIndex;
        private bool _attractMode; // we will set this when we enter an attract video
                                   // and clear it when someone interacts with the machine
        private bool _isAttractModePlaying;
        private int _currentNotificationIndex;
        private string _currentNotificationText = string.Empty;
        private bool? _cashInStartZeroCredits;
        private readonly ConcurrentQueue<bool> _forcedCashOutData = new ConcurrentQueue<bool>();
        private IEdgeLightToken _edgeLightStateToken;
        private EdgeLightState? _currentEdgeLightState;
        private bool _canOverrideEdgeLight;
        private bool _isTopperAttractFeaturePlaying;
        private string _topperAttractVideoPath;
        private string _topperImageResourceKey;
        private int _attractModeTopperImageIndex;
        private string _topperLobbyVideoPath;
        private readonly Dictionary<Sound, string> _soundFilePathMap = new Dictionary<Sound, string>();
        private bool _playCollectSound;
        private MenuSelectionPayOption _selectedMenuSelectionPayOption;
        private bool _vbdInfoBarOpenRequested;
        private bool _isGambleFeatureActive;

        /****** UPI ******/
        /* TODO: Make UpiViewModel to break up this class */

        /// <summary>
        ///     Initializes a new instance of the <see cref="LobbyViewModel" /> class.
        /// </summary>
        public LobbyViewModel()
            : this(
                ServiceManager.GetInstance().TryGetService<IEventBus>(),
                ServiceManager.GetInstance().TryGetService<IPropertiesManager>(),
                ServiceManager.GetInstance().TryGetService<IButtonDeckDisplay>(),
                ServiceManager.GetInstance().TryGetService<IBank>(),
                ServiceManager.GetInstance().TryGetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().TryGetService<IButtonDeckFilter>(),
                ServiceManager.GetInstance().TryGetService<IContainerService>(),
                ServiceManager.GetInstance().TryGetService<ITransferOutHandler>(),
                ServiceManager.GetInstance().TryGetService<IMessageDisplay>(),
                ServiceManager.GetInstance().TryGetService<ISessionInfoService>(),
                ServiceManager.GetInstance().TryGetService<IAttendantService>(),
                ServiceManager.GetInstance().TryGetService<IEdgeLightingStateManager>(),
                ServiceManager.GetInstance().TryGetService<ICabinetDetectionService>(),
                ServiceManager.GetInstance().TryGetService<IAudio>(),
                ServiceManager.GetInstance().TryGetService<ICashoutController>(),
                ServiceManager.GetInstance().TryGetService<IAttractConfigurationProvider>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LobbyViewModel" /> class.
        ///     We need this extra constructor to unit test with mocks.
        /// </summary>
        public LobbyViewModel(
            IEventBus eventBus,
            IPropertiesManager properties,
            IButtonDeckDisplay buttonDeckDisplay,
            IBank bank,
            ISystemDisableManager systemDisableManager,
            IButtonDeckFilter buttonDeckFilter,
            IContainerService containerService,
            ITransferOutHandler transferOutHandler,
            IMessageDisplay messageDisplay,
            ISessionInfoService sessionInfoService,
            IAttendantService attendant,
            IEdgeLightingStateManager edgeLightingStateManager,
            ICabinetDetectionService cabinetDetectionService,
            IAudio audio,
            ICashoutController cashoutController,
            IAttractConfigurationProvider attractInfoProvider)
        {
            if (buttonDeckDisplay == null)
            {
                throw new ArgumentNullException(nameof(buttonDeckDisplay));
            }

            if (containerService == null)
            {
                throw new ArgumentNullException(nameof(containerService));
            }

            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _systemDisableManager =
                systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _buttonDeckFilter = buttonDeckFilter ?? throw new ArgumentNullException(nameof(buttonDeckFilter));
            _transferOutHandler = transferOutHandler ?? throw new ArgumentNullException(nameof(transferOutHandler));
            _messageDisplay = messageDisplay ?? throw new ArgumentNullException(nameof(messageDisplay));
            _sessionInfoService = sessionInfoService ?? throw new ArgumentNullException(nameof(sessionInfoService));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _attendant = attendant ?? throw new ArgumentNullException(nameof(attendant));
            _edgeLightingStateManager = edgeLightingStateManager ?? throw new ArgumentNullException(nameof(edgeLightingStateManager));
            _cabinetDetectionService = cabinetDetectionService ?? throw new ArgumentNullException(nameof(cabinetDetectionService));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
            _cashoutController = cashoutController ?? throw new ArgumentNullException(nameof(cashoutController));
            _attractInfoProvider = attractInfoProvider ?? throw new ArgumentNullException(nameof(attractInfoProvider));
            ProgressiveLabelDisplay = new ProgressiveLobbyIndicatorViewModel(this);

            _operatorMenu = containerService.Container.GetInstance<IOperatorMenuLauncher>();
            _gameRecovery = containerService.Container.GetInstance<IGameRecovery>();
            _gameDiagnostics = containerService.Container.GetInstance<IGameDiagnostics>();
            _gameHistory = containerService.Container.GetInstance<IGameHistory>();
            _buttonLamps = containerService.Container.GetInstance<IButtonLamps>();
            _runtime = containerService.Container.GetInstance<IRuntimeFlagHandler>();
            _gameStorage = containerService.Container.GetInstance<IGameStorage>();
            _gameState = containerService.Container.GetInstance<IGamePlayState>();
            _gameOrderSettings = containerService.Container.GetInstance<IGameOrderSettings>();
            _commandFactory = containerService.Container.GetInstance<ICommandHandlerFactory>();
            _gameService = containerService.Container.GetInstance<IGameService>();
            _cashableLockupProvider = containerService.Container.GetInstance<ICashableLockupProvider>();
            _reserveService = containerService.Container.GetInstance<IReserveService>();

            PlayerInfoDisplayMenuViewModel = new PlayerInfoDisplayMenuViewModel(containerService.Container.GetInstance<IPlayerInfoDisplayFeatureProvider>());
            var factory = containerService.Container.GetInstance<IPlayerInfoDisplayManagerFactory>();
            _playerInfoDisplayManager = factory.Create(this);

            _disableCountdownMessage = new DisplayableMessage(
                () => DisableCountdownMessage,
                DisplayableMessageClassification.Informative,
                DisplayableMessagePriority.Immediate);

            _lobbyStateManager = containerService.Container.GetInstance<ILobbyStateManager>();
            _lobbyStateManager.OnStateEntry = OnStateEntry;
            _lobbyStateManager.OnStateExit = OnStateExit;
            _lobbyStateManager.GameLoadedWhileDisabled = GameLoadedWhileDisabled;
            _lobbyStateManager.UpdateLobbyUI = UpdateUI;
            _lobbyStateManager.UpdateLamps = UpdateLampsCashOutFinished;

            _ageWarningTimer = new AgeWarningTimer(_lobbyStateManager);

            Config = _properties.GetValue<LobbyConfiguration>(GamingConstants.LobbyConfig, null);
            LargeGameIconsEnabled = Config.LargeGameIconsEnabled;
            MultiLanguageEnabled = Config.MultiLanguageEnabled;
            GameLoadingScreenPath = "pack://siteOfOrigin:,,,/" + Config.DefaultLoadingScreenFilename;
            _gamesPerPage = Config.MaxDisplayedGames;

            if (Config.ResponsibleGamingTimeLimitEnabled)
            {
                _responsibleGaming = containerService.Container.GetInstance<IResponsibleGaming>();
                _responsibleGaming.OnStateChange += ResponsibleGamingStateChanged;
                _responsibleGaming.Initialize();

                // Propagate to lobby property for data binding.
                _responsibleGaming.PropertyChanged += ResponsibleGamingOnPropertyChanged;
                _responsibleGaming.ForceCashOut += OnForceCashOut;
                _responsibleGaming.OnForcePendingCheck += ForcePendingResponsibleGamingCheck;
            }

            ClockTimer = new ClockTimer(Config, _responsibleGaming, _runtime, _lobbyStateManager);

            if (MultiLanguageEnabled)
            {
                var localeCode = _properties.GetValue(GamingConstants.SelectedLocaleCode, GamingConstants.EnglishCultureCode)
                    .ToUpperInvariant();

                if (string.IsNullOrEmpty(localeCode) || Config.LocaleCodes.Length == 1 ||
                    localeCode == Config.LocaleCodes[0].ToUpperInvariant())
                {
                    _properties.SetProperty(GamingConstants.SelectedLocaleCode, ActiveLocaleCode);
                }
                else
                {
                    IsPrimaryLanguageSelected = false;
                }
            }
            else
            {
                _properties.SetProperty(GamingConstants.SelectedLocaleCode, GamingConstants.EnglishCultureCode);
            }

            if (buttonDeckDisplay.DisplayCount != 0 || buttonDeckDisplay.IsSimulated)
            {
                _lobbyButtonDeckRenderer = new LobbyButtonDeckRenderer(
                    buttonDeckDisplay,
                    Config.LcdInsertMoneyVideoLanguage1);
            }

            Logger.Debug("Initializing the lobby");

            GameList = new ObservableCollection<GameInfo>();

            GameSelectCommand = new ActionCommand<object>(LaunchGameFromUi);
            PreviousPageCommand = new ActionCommand<object>(PrevPage);
            NextPageCommand = new ActionCommand<object>(NextPage);
            AddCreditsCommand = new ActionCommand<object>(BankPressed);
            CashOutCommand = new ActionCommand<object>(CashOutPressed);
            ServiceCommand = new ActionCommand<object>(ServicePressed);
            AddDebugCashCommand = new ActionCommand<object>(AddDebugCashPressed);
            VbdCashoutDlgYesNoCommand = new ActionCommand<object>(VbdCashoutDlgYesNoPressed);
            VbdServiceDlgYesNoCommand = new ActionCommand<object>(VbdServiceDlgYesNoPressed);
            DenomFilterPressedCommand = new ActionCommand<object>(DenomFilterPressed);
            TimeLimitDlgCommand = new ActionCommand<object>(TimeLimitAccepted);
            ResponsibleGamingDialogOpenCommand = new ActionCommand<object>(ResponsibleGamingDialogOpenButtonPressed);
            PrintHelplineMessageCommand = new ActionCommand<object>(OnPrintHelplineMessage, CanPrintHelplineMessage);
            IdleTextScrollingCompletedCommand = new ActionCommand<object>(OnIdleTextScrollingCompleted);
            CashOutWrapperMouseDownCommand = new ActionCommand<object>(OnCashOutWrapperMouseDown);
            UpiPreviewMouseDownCommand = new ActionCommand<object>(OnUpiPreviewMouseDown);
            UserInteractionCommand = new ActionCommand<object>(obj => OnUserInteraction());
            ExitResponsibleGamingInfoCommand = new ActionCommand<object>(obj => ExitResponsibleGamingInfoDialog());
            TouchResponsibleGamingInfoCommand = new ActionCommand<object>(obj => TouchResponsibleGamingInfoDialog());
            ClockSwitchCommand = new ActionCommand<object>(OnClockSwitchPressed);
            GameTabPressedCommand = new ActionCommand<object>(OnGameTabPressed);
            DenominationPressedCommand = new ActionCommand<object>(OnDenominationPressed);
            DenominationForSpecificGamePressedCommand = new ActionCommand<object[]>(OnDenominationForSpecificGamePressed);
            SubTabPressedCommand = new ActionCommand<object>(OnSubTabPressed);
            ReturnToLobbyCommand = new ActionCommand<object>(ReturnToLobbyButtonPressed);
            CashOutFromPlayerMenuPopupCommand = new ActionCommand<object>(CashoutFromPlayerPopUpMenu);
            ResponsibleGaming = new ResponsibleGamingViewModel(this);
            ReplayRecovery = new ReplayRecoveryViewModel(_eventBus, _gameDiagnostics, _properties, _commandFactory);
            PlayerMenuPopupViewModel = new PlayerMenuPopupViewModel();

            MessageOverlayDisplay = new MessageOverlayViewModel(PlayerMenuPopupViewModel, _playerInfoDisplayManager);
            MessageOverlayDisplay.PropertyChanged += MessageOverlayDisplay_OnPropertyChanged;

            LoadGameInfo();

            _idleTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(IdleTimerIntervalSeconds) };
            _idleTimer.Tick += IdleTimer_Tick;

            _idleTextTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(IdleTextTimerIntervalSeconds) };
            _idleTextTimer.Tick += IdleTextTimer_Tick;

            _attractTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(Config.AttractTimerIntervalInSeconds) };
            _attractTimer.Tick += AttractTimer_Tick;

            _renderTimer = new DispatcherTimerAdapter(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(30.0) };
            _renderTimer.Tick += RenderTimerTick;

            if (Config.ResponsibleGamingInfo.Timeout > 0)
            {
                _responsibleGamingInfoTimeOutTimer =
                    new DispatcherTimerAdapter
                    {
                        Interval = TimeSpan.FromSeconds(Config.ResponsibleGamingInfo.Timeout)
                    };
            }
            else
            {
                _responsibleGamingInfoTimeOutTimer = new DispatcherTimerAdapter();
            }

            _responsibleGamingInfoTimeOutTimer.Tick += ResponsibleGamingInfoTimeOutTimerOnTick;

            _vbdConfirmationTimeOutTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(VbdConfirmationTimeOutInSeconds) };
            _vbdConfirmationTimeOutTimer.Tick += VbdConfirmationTimeOutTimerOnTick;

            _disableCountdownTimer = new DispatcherTimerAdapter(DispatcherPriority.Render) { Interval = TimeSpan.FromSeconds(1.0) };
            _disableCountdownTimer.Tick += DisableCountdownTimerTick;

            _rotateTopImageTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(RotateTopImageIntervalInSeconds) };
            _rotateTopImageTimer.Tick += RotateTopImageTimerTick;

            _rotateTopperImageTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(RotateTopperImageIntervalInSeconds) };
            _rotateTopperImageTimer.Tick += RotateTopperImageTimerTick;

            _rotateSoftErrorTextTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(RotateSoftErrorTextIntervalInSeconds) };
            _rotateSoftErrorTextTimer.Tick += RotateSoftErrorTextTimerTick;

            _printHelplineTicketTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(PrintHelplineWaitTimeInSeconds) };
            _printHelplineTicketTimer.Tick += PrintHelplineTicketTimerTick;

            _debugCurrencyTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(DebugCurrencyIntervalInSeconds) };
            _debugCurrencyTimer.Tick += DebugCurrencyTimerTick;

            _cashOutTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(CashOutMinimumIntervalInSeconds) };
            _cashOutTimer.Tick += CashOutTimerTick;

            if (Config.DaysAsNew > 0)
            {
                EvaluateGamesForNew();
                // If DaysAsNew == 0 then do not calculate if games are new because the host will do this
                _newGameTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromHours(NewGameTimerIntervalInHours) };
                _newGameTimer.Tick += NewGameTimer_Tick;
                _newGameTimer.Start();
            }

            IsResponsibleGamingInfoVisible = Config.ResponsibleGamingInfo.ButtonPlacement == ResponsibleGamingInfoButtonPlacement.Header;
            DisplayVbdServiceButton = Config.VbdDisplayServiceButton;

            UpdatePaidMeterValue(_sessionInfoService.GetSessionPaidValue());

            messageDisplay.AddMessageDisplayHandler(this);

            WireDesignerData();

            _lobbyStateManager.Initialize();

            SubscribeToEvents();

            Credits = OverlayMessageUtils.ToCredits(bank.QueryBalance());

            //if recovery is needed or we don't have zero credits, forget about the age warning until we hit attract mode
            _ageWarningTimer.AgeWarningNeeded = Config.DisplayAgeWarning && !_gameHistory.IsRecoveryNeeded && HasZeroCredits;

            SendLanguageChangedEvent(true);

            IsDemonstrationMode = _properties.GetValue(ApplicationConstants.DemonstrationMode, false);

            Volume = new LobbyVolumeViewModel(OnUserInteraction);

            TopperLobbyVideoPath = Config.TopperLobbyVideoFilename;

            MenuSelectionPayOptions = new List<MenuSelectionPayOption>((MenuSelectionPayOption[])Enum.GetValues(typeof(MenuSelectionPayOption)));

            LoadSoundFiles();

            Logger.Debug("Lobby initialization complete");
        }

        public string TopperTitle => GamingConstants.TopperWindowTitle;

        public string TopTitle => GamingConstants.TopWindowTitle;

        public string MainTitle => GamingConstants.MainWindowTitle;

        public string VbdTitle => GamingConstants.VbdWindowTitle;

        public bool IsTabView => _lobbyStateManager?.IsTabView ?? false;

        /// <summary>
        ///     Is the current tab hosting extra large game icons
        /// </summary>
        public bool IsExtraLargeGameIconTabActive => GameTabInfo.SelectedCategory == GameCategory.LightningLink;

        /// <summary>
        ///     Gets the game selected command
        /// </summary>
        public ICommand GameSelectCommand { get; }

        /// <summary>
        ///     Gets the previous page command
        /// </summary>
        public ICommand PreviousPageCommand { get; }

        /// <summary>
        ///     Gets the next page command
        /// </summary>
        public ICommand NextPageCommand { get; }

        /// <summary>
        ///     Gets the command to insert credits
        /// </summary>
        public ICommand AddCreditsCommand { get; }

        /// <summary>
        ///     Gets the cash out command
        /// </summary>
        public ICommand CashOutCommand { get; }

        /// <summary>
        ///     Gets the service command
        /// </summary>
        public ICommand ServiceCommand { get; }

        /// <summary>
        ///     Gets the AddDebugCashCommand
        /// </summary>
        public ICommand AddDebugCashCommand { get; }

        /// <summary>
        ///     Gets the VbdCashoutDlgYesNoCommand
        /// </summary>
        public ICommand VbdCashoutDlgYesNoCommand { get; }

        /// <summary>
        ///     Gets the VbdServiceDlgYesNoCommand
        /// </summary>
        public ICommand VbdServiceDlgYesNoCommand { get; }

        /// <summary>
        ///     Gets the denom filter pressed command
        /// </summary>
        public ICommand DenomFilterPressedCommand { get; }

        /// <summary>
        ///     Gets the responsible gaming dialog open command
        /// </summary>
        public ICommand ResponsibleGamingDialogOpenCommand { get; }

        /// <summary>
        ///     Gets the print helpline message command
        /// </summary>
        public ActionCommand<object> PrintHelplineMessageCommand { get; }

        /// <summary>
        ///     Gets the time limit dialog command
        /// </summary>
        public ICommand TimeLimitDlgCommand { get; }

        /// <summary>
        ///     Gets or sets action command that idle text scrolling completed event.
        /// </summary>
        public ICommand IdleTextScrollingCompletedCommand { get; set; }

        public ICommand CashOutWrapperMouseDownCommand { get; set; }

        public ICommand UpiPreviewMouseDownCommand { get; set; }

        public ICommand UserInteractionCommand { get; set; }

        public ICommand ExitResponsibleGamingInfoCommand { get; set; }

        public ICommand TouchResponsibleGamingInfoCommand { get; set; }

        public ICommand ClockSwitchCommand { get; set; }

        public ICommand GameTabPressedCommand { get; set; }

        public ICommand DenominationPressedCommand { get; set; }

        public ICommand DenominationForSpecificGamePressedCommand { get; set; }

        public ICommand SubTabPressedCommand { get; set; }

        /// <summary>
        ///     Command to return to the lobby when in the game
        /// </summary>
        public ICommand ReturnToLobbyCommand { get; set; }

        /// <summary>
        ///     Command to cashout from player menu pop up
        /// </summary>
        public ICommand CashOutFromPlayerMenuPopupCommand { get; set; }

        /// <summary>
        ///     Gets the object that handles RG info logic.
        /// </summary>
        public ResponsibleGamingViewModel ResponsibleGaming { get; }

        public ProgressiveLobbyIndicatorViewModel ProgressiveLabelDisplay { get; }

        public MessageOverlayViewModel MessageOverlayDisplay { get; }

        private ResponsibleGamingSessionState ResponsibleGamingSessionState
        {
            get => _responsibleGamingSessionState;
            set
            {
                _responsibleGamingSessionState = value;
                ClockTimer.ResponsibleGamingSessionState = value;
            }
        }

        /// <summary>
        ///     Gets the object that handles ReplayRecovery info logic.
        /// </summary>
        public ReplayRecoveryViewModel ReplayRecovery { get; private set; }

        /// <summary>
        ///     Gets or sets the game list
        /// </summary>
        public ObservableCollection<GameInfo> GameList
        {
            get => _gameList;

            set
            {
                if (_gameList != value)
                {
                    if (_gameList != null)
                    {
                        _gameList.CollectionChanged -= GameList_CollectionChanged;
                    }

                    _gameList = value;
                    RefreshDisplayedGameList();
                    RefreshAttractGameList();

                    _gameList.CollectionChanged += GameList_CollectionChanged;

                    RaisePropertyChanged(nameof(GameList));
                    RaisePropertyChanged(nameof(MarginInputs));
                    RaisePropertyChanged(nameof(IsSingleTabView));
                    RaisePropertyChanged(nameof(IsSingleDenomDisplayed));
                    RaisePropertyChanged(nameof(IsSingleGameDisplayed));
                }
            }
        }

        /// <summary>
        ///     Gets the displayed game list
        /// </summary>
        public ObservableCollection<GameInfo> DisplayedGameList { get; } = new ObservableCollection<GameInfo>();

        /// <summary>
        ///     Gets the Attract game list
        /// </summary>
        public ObservableCollection<IAttractDetails> AttractList { get; } = new ObservableCollection<IAttractDetails>();

        /// <summary>
        ///     Gets or sets a value indicating whether demonstration mode is enabled
        /// </summary>
        public bool IsDemonstrationMode
        {
            get => _isDemonstrationMode;
            set => SetProperty(ref _isDemonstrationMode, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether multi language is enabled
        /// </summary>
        public bool MultiLanguageEnabled
        {
            get => _multiLanguageEnabled;
            set => SetProperty(ref _multiLanguageEnabled, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether VBD rendering is disabled (as it is in system lockup).
        /// </summary>
        public bool IsVbdRenderingDisabled
        {
            get => _isVbdRenderingDisabled;
            set => SetProperty(ref _isVbdRenderingDisabled,
                value,
                nameof(IsVbdRenderingDisabled),
                nameof(IsLobbyVbdVisible),
                nameof(IsGameVbdVisible),
                nameof(IsLobbyVbdBackgroundBlank));
        }

        public IPlayerInfoDisplayManager PlayerInfoDisplayManager => _playerInfoDisplayManager;

        public bool IsLobbyVbdBackgroundBlank => VbdVideoState == LobbyVbdVideoState.Disabled;

        public bool IsGameLoading => CurrentState == LobbyState.GameLoading || CurrentState == LobbyState.GameLoadingForDiagnostics;

        /// <summary> Gets a value indicating whether the vbd is visible in the game. </summary>
        public bool IsGameVbdVisible => _isInitialStartup || (!IsInLobby && !IsVbdRenderingDisabled);

        /// <summary>
        ///     Gets or sets a value indicating whether top screen rendering is disabled (as it is in system lockup).
        /// </summary>
        public bool IsTopScreenRenderingDisabled
        {
            get => _isTopScreenRenderingDisabled;
            set => SetProperty(ref _isTopScreenRenderingDisabled, value, nameof(IsTopScreenRenderingDisabled), nameof(IsLobbyTopScreenVisible), nameof(IsGameTopScreenVisible));
        }

        /// <summary>
        ///     Gets or sets a value indicating whether topper screen rendering is disabled (as it is in system lockup).
        /// </summary>
        public bool IsTopperScreenRenderingDisabled
        {
            get => _isTopperScreenRenderingDisabled;
            set => SetProperty(
                ref _isTopperScreenRenderingDisabled,
                value,
                nameof(IsTopperScreenRenderingDisabled),
                nameof(IsLobbyTopperScreenVisible),
                nameof(IsGameTopperScreenVisible),
                nameof(IsLobbyTopperVideoVisible));
        }

        /// <summary> Gets a value indicating whether the top screen is visible in the lobby. </summary>
        public bool IsLobbyTopScreenVisible => IsInLobby && !IsTopScreenRenderingDisabled;

        /// <summary> Gets a value indicating whether the topper screen is visible in the lobby. </summary>
        public bool IsLobbyTopperScreenVisible => IsInLobby && !IsTopperScreenRenderingDisabled;

        /// <summary> Gets a value indicating whether the topper screen is visible in the lobby. </summary>
        public bool IsLobbyTopperVideoVisible => IsLobbyTopperScreenVisible &&
                                                 !IsTopperAttractFeaturePlaying &&
                                                 !string.IsNullOrEmpty(TopperLobbyVideoPath);

        /// <summary> Gets a value indicating whether the top screen is visible in the game. </summary>
        public bool IsGameTopScreenVisible => !IsInLobby && !IsTopScreenRenderingDisabled;

        /// <summary> Gets a value indicating whether the topper screen is visible in the game. </summary>
        public bool IsGameTopperScreenVisible => !IsInLobby && !IsTopperScreenRenderingDisabled;

        /// <summary>
        ///     Gets or sets a value indicating whether large game icons are enabled
        /// </summary>
        public bool LargeGameIconsEnabled
        {
            get => _largeGameIconsEnabled;
            set => SetProperty(ref _largeGameIconsEnabled, value);
        }

        /// <summary>
        ///     Gets or sets the displayed page number
        /// </summary>
        public int DisplayedPageNumber
        {
            get => _displayedPageNumber;
            set => SetProperty(ref _displayedPageNumber, value);
        }

        /// <summary>
        ///     Gets or sets the games per page
        /// </summary>
        public int GamesPerPage
        {
            get => _gamesPerPage;
            set => SetProperty(ref _gamesPerPage, value);
        }

        /// <summary>
        ///     Gets the page count
        /// </summary>
        public int PageCount => (_gameList.Count + (_gamesPerPage - 1)) / _gamesPerPage;

        /// <summary>
        ///     Gets or sets the credits
        /// </summary>
        public double Credits
        {
            get => _credits;

            set
            {
                if (_credits.Equals(value))
                {
                    return;
                }

                _credits = value;
                RaisePropertyChanged(nameof(Credits));
                RaisePropertyChanged(nameof(HasZeroCredits));
                RaisePropertyChanged(nameof(CashOutEnabled));
                RaisePropertyChanged(nameof(CashOutEnabledInPlayerMenu));
                RaisePropertyChanged(nameof(FormattedCredits));
                RaisePropertyChanged(nameof(ShowAttractMode));
                RaisePropertyChanged(nameof(IsDebugMoneyEnabled));
                RaisePropertyChanged(nameof(IsBlinkingIdleTextVisible));
                RaisePropertyChanged(nameof(StartIdleTextBlinking));
                RaisePropertyChanged(nameof(IsScrollingIdleTextEnabled));
                RaisePropertyChanged(nameof(IsCashOutButtonLit));
                UpdateLcdButtonDeckVideo();
                UpdateLamps();
            }
        }

        private double RedeemableCredits => _credits;

        /// <summary>
        ///     Gets the credits as a formatted string to display the currency.
        /// </summary>
        public string FormattedCredits => Credits.FormattedCurrencyString();

        /// <summary>
        ///     Gets a value indicating whether the EGM has zero credits.
        /// </summary>
        public bool HasZeroCredits => _credits.Equals(0.0);

        /// <summary>
        ///     Gets or sets the paid meter
        /// </summary>
        public int Paid
        {
            get => _paid;
            set => SetProperty(ref _paid, value);
        }

        /// <summary>
        ///     Gets or sets the main message text
        /// </summary>
        public string MainMessage
        {
            get => _mainMessage;
            set => SetProperty(ref _mainMessage, value);
        }

        /// <summary>
        ///     Gets a value indicating whether we are in a cashable lockup state or not.
        ///     This is used by the MessageOverlay class to control the visibility of the on screen cash out
        ///     button when a tilt is present.
        /// </summary>
        public bool CanCashoutInLockup => _cashableLockupProvider.CanCashoutInLockup(_systemDisableManager.IsDisabled && MessageOverlayDisplay.IsLockupMessageVisible, CashOutEnabled, ExecuteOnUserCashOut);

        /// <summary>
        ///     When Media Players are resizing, disable Lobby input and Service button
        /// </summary>
        public bool MediaPlayersResizing
        {
            get => _disableLobbyInput;
            set => SetProperty(ref _disableLobbyInput, value);
        }

        public bool VbdServiceButtonDisabled
        {
            get => _vbdServiceButtonDisabled;
            set => SetProperty(ref _vbdServiceButtonDisabled, value);
        }

        public bool ServiceButtonVisible
        {
            get => _serviceButtonVisible;
            set => SetProperty(ref _serviceButtonVisible, value);
        }

        public bool VolumeButtonVisible
        {
            get => _volumeButtonVisible;
            set => SetProperty(ref _volumeButtonVisible, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the virtual button deck is disabled.
        /// </summary>
        public bool IsVirtualButtonDeckDisabled =>
            MessageOverlayDisplay.IsLockupMessageVisible || MessageOverlayDisplay.IsCashingOutDlgVisible ||
            MessageOverlayDisplay.IsCashingInDlgVisible ||
            MessageOverlayDisplay.IsReplayRecoveryDlgVisible;

        /// <summary>
        ///     Gets a value indicating whether the game is visible or not
        /// </summary>
        public bool IsGameVisible => !IsInLobby;

        /// <summary>
        ///     Gets or sets a value indicating whether the lobby is visible or not
        /// </summary>
        public bool IsInLobby
        {
            get => _isInLobby;

            set
            {
                if (_isInLobby != value)
                {
                    _isInLobby = value;
                    ClockTimer.IsInLobby = value;
                    RaisePropertyChanged(nameof(IsInLobby));
                    RaisePropertyChanged(nameof(IsIdleAttractVideoVisible));
                    RaisePropertyChanged(nameof(IsBottomAttractVisible));
                    RaisePropertyChanged(nameof(IsLobbyVbdVisible));
                    RaisePropertyChanged(nameof(IsGameVbdVisible));
                    RaisePropertyChanged(nameof(IsLobbyVbdBackgroundBlank));
                    RaisePropertyChanged(nameof(IsLobbyTopScreenVisible));
                    RaisePropertyChanged(nameof(IsGameTopScreenVisible));
                    RaisePropertyChanged(nameof(IsLobbyTopperScreenVisible));
                    RaisePropertyChanged(nameof(IsGameTopperScreenVisible));
                    RaisePropertyChanged(nameof(IsGameVisible));
                    RaisePropertyChanged(nameof(IsIdleTextBlinking));
                    RaisePropertyChanged(nameof(StartIdleTextBlinking));
                    RaisePropertyChanged(nameof(IsLobbyTopperVideoVisible));
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating that we want to hide the lobby in a place it is normally visible
        ///     Initial place this is used is recovery on boot
        /// </summary>
        public bool IsLobbyVisible
        {
            get => _isLobbyVisible;
            set => SetProperty(ref _isLobbyVisible, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the lobby activated or not
        /// </summary>
        public bool LobbyActivated
        {
            get => _lobbyActivated;
            set => SetProperty(ref _lobbyActivated, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether attract mode is currently playing.
        /// </summary>
        public bool IsAttractModePlaying
        {
            get => _isAttractModePlaying;
            set => SetProperty(
                ref _isAttractModePlaying,
                value,
                nameof(IsAttractModePlaying),
                nameof(IsCashOutButtonLit)
            );
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the top attract feature is playing or not
        /// </summary>
        public bool IsTopAttractFeaturePlaying
        {
            get => _isTopAttractFeaturePlaying;
            set => SetProperty(ref _isTopAttractFeaturePlaying, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the topper attract feature is playing or not
        /// </summary>
        public bool IsTopperAttractFeaturePlaying
        {
            get => _isTopperAttractFeaturePlaying;
            set => SetProperty(
                ref _isTopperAttractFeaturePlaying,
                value,
                nameof(IsTopperAttractFeaturePlaying),
                nameof(IsLobbyTopperVideoVisible));
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the bottom attract feature is playing or not
        /// </summary>
        public bool IsBottomAttractFeaturePlaying
        {
            get => _isBottomAttractFeaturePlaying;
            set => SetProperty(ref _isBottomAttractFeaturePlaying, value);
        }

        /// <summary>
        ///     Gets a value indicating whether only one Tab is present
        /// </summary>
        public bool IsSingleTabView => GameTabInfo?.TabCount == 1;

        /// <summary>
        ///     Gets a value indicating whether only one denomination is available for player selection in the current tab
        /// </summary>
        public bool IsSingleDenomDisplayed => GameTabInfo?.Denominations.Count == 1;

        /// <summary>
        ///     Gets a value indicating whether icon of only one game is displayed in lobby
        /// </summary>
        public bool IsSingleGameDisplayed => DisplayedGameList?.Count == 1;

        /// <summary>
        ///     Gets or sets a value indicating whether the bottom attract feature is visible or not
        /// </summary>
        public bool IsBottomAttractVisible
        {
            get => _isBottomAttractVisible;
            set => SetProperty(ref _isBottomAttractVisible,
                value,
                nameof(IsBottomAttractVisible),
                nameof(IsMainInfoBarVisible));
        }


        /// <summary>
        ///     Gets or sets a value indicating whether a request has been received for the Main screen InfoBar to be shown
        ///     Note that the InfoBar is not always visible, even when a request is present. <see cref="IsMainInfoBarVisible" />
        /// </summary>
        public bool MainInfoBarOpenRequested
        {
            get => _mainInfoBarOpenRequested;
            set
            {
                SetProperty(
                ref _mainInfoBarOpenRequested,
                value,
                nameof(MainInfoBarOpenRequested),
                nameof(GameControlHeight),
                nameof(IsMainInfoBarVisible));

                _eventBus.Publish(new GameControlSizeChangedEvent(GameControlHeight));
            }
        }

        /// <summary>
        ///     Returns <c>true</c> if Main screen InfoBar is visible; otherwise, <c>false</c>.
        /// </summary>
        public bool IsMainInfoBarVisible => MainInfoBarOpenRequested && !IsBottomLoadingScreenVisible && !IsBottomAttractVisible && !MessageOverlayDisplay.IsReplayRecoveryDlgVisible;

        /// <summary>
        ///     Gets or sets a value indicating whether a request has been received for the VBD screen InfoBar to be shown
        ///     Note that the InfoBar is not always visible, even when a request is present. <see cref="IsVbdInfoBarVisible" />
        /// </summary>
        public bool VbdInfoBarOpenRequested
        {
            get => _vbdInfoBarOpenRequested;
            set
            {
                SetProperty(
                ref _vbdInfoBarOpenRequested,
                value,
                nameof(VbdInfoBarOpenRequested),
                nameof(GameControlHeight),
                nameof(IsVbdInfoBarVisible));

                _eventBus.Publish(new GameControlSizeChangedEvent(GameControlHeight));
            }
        }

        /// <summary>
        ///     Returns <c>true</c> if VBD InfoBar is visible; otherwise, <c>false</c>.
        /// </summary>
        public bool IsVbdInfoBarVisible => VbdInfoBarOpenRequested && !IsBottomLoadingScreenVisible && !IsBottomAttractVisible && !MessageOverlayDisplay.IsReplayRecoveryDlgVisible;

        /// <summary>
        ///     Gets or sets the Paid meter's text
        /// </summary>
        public string PaidMeterValue
        {
            get => _paidMeterValue;
            set => SetProperty(ref _paidMeterValue, value, nameof(PaidMeterValue), nameof(IsPaidMeterVisible));
        }

        public bool IsTimeLimitDlgVisible => _responsibleGaming?.IsTimeLimitDialogVisible ?? false;

        /// <summary>
        ///     Gets a value indicating whether the responsible gaming info dialog is visible
        /// </summary>
        public bool IsResponsibleGamingInfoDlgVisible
        {
            get => _isResponsibleGamingInfoDlgVisible;
            private set => SetProperty(ref _isResponsibleGamingInfoDlgVisible, value);
        }


        /// <summary>
        ///     Gets a value indicating whether the top idle attract video is visible.
        /// </summary>
        public bool IsIdleAttractVideoVisible => IsInLobby && Config.HasIdleAttractVideo;

        /// <summary>
        ///     Gets or sets the top attract video path
        /// </summary>
        public string TopAttractVideoPath
        {
            get => _topAttractVideoPath;
            set => SetProperty(ref _topAttractVideoPath, value);
        }

        /// <summary>
        ///     Gets or sets the topper attract video path
        /// </summary>
        public string TopperAttractVideoPath
        {
            get => _topperAttractVideoPath;
            set => SetProperty(ref _topperAttractVideoPath, value);
        }

        /// <summary>
        ///     Gets or sets the topper attract video path
        /// </summary>
        public string TopperLobbyVideoPath
        {
            get => _topperLobbyVideoPath;
            set => SetProperty(
                ref _topperLobbyVideoPath,
                value,
                nameof(TopperLobbyVideoPath),
                nameof(IsLobbyTopperVideoVisible));
        }

        /// <summary>
        ///     Gets or sets the bottom attract video path
        /// </summary>
        public string BottomAttractVideoPath
        {
            get => _bottomAttractVideoPath;
            set => SetProperty(ref _bottomAttractVideoPath, value);
        }

        /// <summary>
        ///     Gets or sets the game loading screen path
        /// </summary>
        public string GameLoadingScreenPath
        {
            get => _gameLoadingScreenPath;
            set => SetProperty(ref _gameLoadingScreenPath, value);
        }

        /// <summary>
        ///     Gets or sets the current attract index
        /// </summary>
        public int CurrentAttractIndex
        {
            get => _currentAttractIndex;
            set => SetProperty(ref _currentAttractIndex, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the bottom loading screen is visible
        /// </summary>
        public bool IsBottomLoadingScreenVisible
        {
            get => _isBottomLoadingScreenVisible;
            set => SetProperty(
                ref _isBottomLoadingScreenVisible,
                value,
                nameof(IsBottomLoadingScreenVisible),
                nameof(CashOutEnabled),
                nameof(CashOutEnabledInPlayerMenu),
                nameof(IsMainInfoBarVisible),
                nameof(IsVbdInfoBarVisible),
                nameof(ResponsibleGamingInfoEnabled));
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the top loading screen is visible.
        ///     Some jurisdictions do not have a separate top loading screen.
        /// </summary>
        public bool IsTopLoadingScreenVisible
        {
            get => _isTopLoadingScreenVisible;
            set => SetProperty(ref _isTopLoadingScreenVisible, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the topper loading screen is visible.
        ///     Some jurisdictions do not have a separate topper loading screen.
        /// </summary>
        public bool IsTopperLoadingScreenVisible
        {
            get => _isTopperLoadingScreenVisible;
            set => SetProperty(ref _isTopperLoadingScreenVisible, value);
        }

        /// <summary>
        ///     Gets or sets the game filter
        /// </summary>
        public GameType GameFilter
        {
            get => _gameFilter;

            set
            {
                if (_gameFilter != value)
                {
                    _gameFilter = value;
                    OnUserInteraction();
                    RaisePropertyChanged(nameof(GameFilter));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the denomination filter
        /// </summary>
        public int DenomFilter
        {
            get => _denomFilter;
            set => SetProperty(ref _denomFilter, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the primary language is selected.
        /// </summary>
        public bool IsPrimaryLanguageSelected
        {
            get => _isPrimaryLanguageSelected;

            set
            {
                if (_isPrimaryLanguageSelected != value)
                {
                    Logger.Debug($"Setting Language.  Primary: {value}");
                    _isPrimaryLanguageSelected = value;
                    ClockTimer.IsPrimaryLanguageSelected = value;

                    if (Config.MultiLanguageEnabled)
                    {
                        _properties.SetProperty(GamingConstants.SelectedLocaleCode, ActiveLocaleCode);
                    }

                    LanguageChanged?.Invoke(this, EventArgs.Empty);

                    OnLanguageChanged();

                    RaisePropertyChanged(nameof(IsPrimaryLanguageSelected));
                    RaisePropertyChanged(nameof(ActiveLocaleCode));
                    RaisePropertyChanged(nameof(FormattedCredits));
                    RaisePropertyChanged(nameof(DisableCountdownMessage));
                    RaisePropertyChanged(nameof(LanguageButtonResourceKey));
                    RaisePropertyChanged(nameof(PaidMeterLabel));

                    UpdateLcdButtonDeckVideo();
                    UpdatePaidMeterValue(_sessionInfoService.GetSessionPaidValue());
                }
            }
        }

        /// <summary>
        ///     Gets the active locale code.
        /// </summary>
        public string ActiveLocaleCode => IsPrimaryLanguageSelected ? Config.LocaleCodes[0] : Config.LocaleCodes[1];

        /// <summary>
        ///     Gets a value indicating whether the cash out button is enabled.
        /// </summary>
        public bool CashOutEnabled =>
            RedeemableCredits > 0.0 && !IsBottomLoadingScreenVisible &&
            !ContainsAnyState(LobbyState.CashOut, LobbyState.CashOutFailure, LobbyState.AgeWarningDialog) &&
            !MessageOverlayDisplay.ShowVoucherNotification;

        /// <summary>
        ///     Gets a value indicating whether the cash out button is enabled in the player menu
        /// </summary>
        public bool CashOutEnabledInPlayerMenu => CashOutEnabled && (!_gameState.InGameRound || _isGambleFeatureActive);

        /// <summary>
        ///     Gets a value indicating whether the cash out button is lit (it's enabled or in attract mode).
        /// </summary>
        public bool IsCashOutButtonLit => CashOutEnabled || IsAttractModePlaying;

        /// <summary>
        ///     Gets a value indicating whether the cash out button is enabled.
        /// </summary>
        public bool ResponsibleGamingInfoEnabled =>
            !IsBottomLoadingScreenVisible &&
            !ContainsAnyState(LobbyState.ResponsibleGamingTimeLimitDialog);

        /// <summary>
        ///     Gets a value indicating whether the Service is requested.
        /// </summary>
        public bool IsServiceRequested => _attendant.IsServiceRequested;

        public bool IsDebugMoneyEnabled => Credits < MaxDebugCurrencyAllowed &&
            (_vbdVideoState == LobbyVbdVideoState.InsertMoney || _vbdVideoState == LobbyVbdVideoState.ChooseGame) && !_disableDebugCurrency;

        /// <summary>
        ///     True if the return to lobby button is enabled, false otherwise
        /// </summary>
        public bool ReturnToLobbyAllowed => (!_gameHistory.IsRecoveryNeeded && _gameState.Idle || _isGambleFeatureActive) && !_transferOutHandler.InProgress &&
                                            !_gameHistory.HasPendingCashOut && !ContainsAnyState(LobbyState.Chooser);

        /// <summary>
        ///     Controls whether the machine can be put into reserve
        /// </summary>
        public bool ReserveMachineAllowed => RedeemableCredits > 0.0 && !_gameHistory.IsRecoveryNeeded && _gameState.Idle && !_transferOutHandler.InProgress
                                             && !_gameHistory.HasPendingCashOut && !ContainsAnyState(LobbyState.Chooser);

        /// <summary>
        ///     Gets or sets the VBD video state while in the lobby.
        /// </summary>
        public LobbyVbdVideoState VbdVideoState
        {
            get => _vbdVideoState;
            set => SetProperty(ref _vbdVideoState, value, nameof(VbdVideoState), nameof(IsLobbyVbdBackgroundBlank));
        }

        public int GameCount
        {
            get => _gameCount;
            set
            {
                _gameCount = value;
                _lobbyStateManager.IsSingleGame = _lobbyStateManager.AllowGameInCharge || UniqueThemeIds <= 1;
                RaisePropertyChanged(nameof(GameCount));
                RaisePropertyChanged(nameof(MarginInputs));
            }
        }

        private bool UseSmallIcons => !IsTabView && GameCount > 8;

        /// <summary>
        ///     Gets or sets the idle text.
        /// </summary>
        public string IdleText
        {
            get => _idleText;

            set
            {
                if (_idleText != value)
                {
                    _idleText = value;
                    RaisePropertyChanged(nameof(IdleText));
                    UpdateIdleTextSettings();
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the idle text is visible or not
        /// </summary>
        public bool IsScrollingIdleTextVisible
        {
            get => _isScrollingIdleTextVisible;
            set => SetProperty(ref _isScrollingIdleTextVisible, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the idle text is paused or not
        /// </summary>
        public bool IsIdleTextPaused
        {
            get => _isIdleTextPaused;
            set => SetProperty(ref _isIdleTextPaused, value);
        }

        /// <summary>
        ///     Gets or sets the y offset for the choose game image.  This only changes internally based on game icon layout.
        /// </summary>
        public double ChooseGameOffsetY
        {
            get => _chooseGameOffsetY;

            set
            {
                if (Math.Abs(_chooseGameOffsetY - value) > 0.001)
                {
                    _chooseGameOffsetY = value;
                    RaisePropertyChanged(nameof(ChooseGameOffsetY));
                }
            }
        }

        public GameGridMarginInputs MarginInputs
        {
            get
            {
                var gameCount = DisplayedGameList?.Count ?? 0;
                var (rows, cols) = IsExtraLargeGameIconTabActive
                    ? GameRowColumnCalculator.ExtraLargeIconRowColCount
                    : GameRowColumnCalculator.CalculateRowColCount(gameCount);
                return new GameGridMarginInputs(
                    gameCount,
                    IsTabView,
                    DisplayedGameList?.Reverse().Take(rows <= 0 ? 0 : gameCount - ((rows - 1) * cols))
                        .Any(x => x.HasProgressiveLabelDisplay) ?? false,
                    GameControlHeight,
                    IsExtraLargeGameIconTabActive,
                    DisplayedGameList?.FirstOrDefault()?.GameIconSize ?? Size.Empty,
                    ProgressiveLabelDisplay.MultipleGameAssociatedSapLevelTwoEnabled,
                    DisplayedGameList?.Any(g => g.HasProgressiveLabelDisplay) ?? false);
            }
        }

        /// <summary>
        ///     Gets or sets the bottom window handle
        /// </summary>
        public IntPtr GameBottomHwnd { get; set; } = IntPtr.Zero;

        /// <summary>
        ///     Gets or sets the top window handle
        /// </summary>
        public IntPtr GameTopHwnd { get; set; } = IntPtr.Zero;

        /// <summary>
        ///     Gets or sets the topper window handle
        /// </summary>
        public IntPtr GameTopperHwnd { get; set; } = IntPtr.Zero;

        /// <summary>
        ///     Gets or sets the virtual button deck window handle.  This is null on systems without VBDs.
        /// </summary>
        public IntPtr GameVirtualButtonDeckHwnd { get; set; } = IntPtr.Zero;

        /// <summary>
        ///     Gets the lobby configuration options
        /// </summary>
        public LobbyConfiguration Config { get; }

        public ResponsibleGamingMode ResponsibleGamingMode => _responsibleGaming?.ResponsibleGamingMode ?? ResponsibleGamingMode.Continuous;

        public bool IsResponsibleGamingInfoFullScreen => Config.ResponsibleGamingInfo.FullScreen;

        public bool IsDebugCurrencyButtonVisible
        {
            get => _isDebugCurrencyButtonVisible;
            private set => SetProperty(ref _isDebugCurrencyButtonVisible, value);
        }

        private bool ShowAttractMode => IsAttractEnabled()
                                        && HasZeroCredits
                                        && !IsIdleTextScrolling
                                        && !MessageOverlayDisplay.ShowVoucherNotification
                                        && !MessageOverlayDisplay.ShowProgressiveGameDisabledNotification
                                        && !(_playerInfoDisplayManager?.IsActive()).GetValueOrDefault();

        public LobbyState CurrentState => _lobbyStateManager.CurrentState;

        private LobbyState BaseState => _lobbyStateManager.BaseState;

        /// <summary>
        ///     Gets or sets the view associated with this VM.  This is kind of a hack to get around some
        ///     binding performance issues (ViewModel shouldn't have access to UI element).
        ///     But binding to window visibility was causing flickering problems and so we decided to
        ///     just recreate the OverlayWindow and Close it as needed.
        /// </summary>
        internal LobbyView LobbyView { get; set; }

        public ResponsibleGamingDialogState ResponsibleGamingCurrentDialogState =>
            _responsibleGaming?.TimeLimitDialogState ?? ResponsibleGamingDialogState.Initial;

        public string ResponsibleGamingDialogResourceKey => _responsibleGaming?.ResponsibleGamingDialogResourceKey;

        public ObservableCollection<DisplayableMessage> NotificationMessages { get; } =
            new ObservableCollection<DisplayableMessage>();

        /// <summary>
        ///     Gets a value indicating whether notification text should be displayed
        /// </summary>
        public bool IsNotificationTextVisible => (Config.DisplaySoftErrors || Config.DisplayInformationMessages) &&
                                                 !string.IsNullOrEmpty(CurrentNotificationText);

        /// <summary>
        ///     Gets or sets the current notification text.
        /// </summary>
        public string CurrentNotificationText
        {
            get => _currentNotificationText;
            set => SetProperty(ref _currentNotificationText, value, nameof(CurrentNotificationText), nameof(IsNotificationTextVisible));
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the VBD Cash Out Dialog is Visible
        /// </summary>
        public bool IsVbdCashOutDialogVisible
        {
            get => _isVbdCashOutDialogVisible;
            set => SetProperty(ref _isVbdCashOutDialogVisible, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the VBD Service Dialog is Visible
        /// </summary>
        public bool IsVbdServiceDialogVisible
        {
            get => _isVbdServiceDialogVisible;
            set => SetProperty(ref _isVbdServiceDialogVisible, value);
        }

        /// <summary>
        ///     Gets or sets if Malfunction message should show on VBD
        /// </summary>
        public bool IsVbdMalfunctionOverlayVisible
        {
            get => _isVbdMalfunctionOverlayVisible;
            set => SetProperty(ref _isVbdMalfunctionOverlayVisible, value);
        }

        /// <summary>
        ///     Gets a value indicating if the VBD for the Lobby is Visible
        /// </summary>
        public bool IsLobbyVbdVisible => IsInLobby && !IsVbdRenderingDisabled;

        /// <summary>
        ///     Determines if the Responsible Gaming Info Button is visible.  Value received from config file
        /// </summary>
        public bool IsResponsibleGamingInfoVisible { get; }

        /// <summary>
        ///     Determines if the Service Button on the VBD is visible.  Value received from config file.
        /// </summary>
        public bool DisplayVbdServiceButton { get; }

        public TimeSpan DisableCountdownTimeRemaining
        {
            get => _disableCountdownTimeRemaining;
            set => SetProperty(ref _disableCountdownTimeRemaining, value);
        }

        public string DisableCountdownMessage => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DisableCountdownMessage);

        public bool IsPaidMeterVisible => PaidMeterValue != string.Empty;

        public string LanguageButtonResourceKey => GetCurrentLanguageButtonResourceKey();

        public string TopImageResourceKey
        {
            get => _topImageResourceKey;
            set => SetProperty(ref _topImageResourceKey, value);
        }

        public string TopperImageResourceKey
        {
            get => _topperImageResourceKey;
            set => SetProperty(ref _topperImageResourceKey, value);
        }

        // ReSharper disable once InconsistentNaming
        public int RGInfoRowSpan => Config.ResponsibleGamingInfo.FullScreen ? 4 : 3;

        private bool IsIdleTextScrolling => LobbyBannerDisplayMode == BannerDisplayMode.Scrolling;

        public bool IsBlinkingIdleTextVisible => !IsIdleTextScrolling && (!Config.HideIdleTextOnCashIn || HasZeroCredits) && !IsTabView;

        public bool IsScrollingIdleTextEnabled => IsIdleTextScrolling && (!Config.HideIdleTextOnCashIn || HasZeroCredits) && !IsTabView;

        public bool IsIdleTextBlinking => IsInLobby && !IsInState(LobbyState.Disabled);

        public bool StartIdleTextBlinking => IsBlinkingIdleTextVisible && IsIdleTextBlinking;

        private long LastDenom => _properties.GetValue(GamingConstants.SelectedDenom, 0L);

        public bool IsDisableCountdownMessageSuppressed
        {
            get => _isDisabledCountdownMessageSuppressed;
            set => SetProperty(ref _isDisabledCountdownMessageSuppressed, value);
        }

        public string PaidMeterLabel => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PaidMeterLabel);

        public ClockTimer ClockTimer { get; }

        private LobbyCashOutDialogState CashOutDialogState
        {
            get => _cashOutDialogState;
            set
            {
                _cashOutDialogState = value;
                Logger.Debug($"CashOutDialogState: {value}");
            }
        }


        public double GameControlWidth
        {
            get => _gameControlWidth;
            set => SetProperty(ref _gameControlWidth, value);
        }

        /// <summary>
        /// The replay navigation bar's current height (including DPI scaling).
        /// </summary>
        public double ReplayNavigationBarHeight
        {
            get => _replayNavigationBarHeight;
            set
            {
                SetProperty(ref _replayNavigationBarHeight, value, nameof(ReplayNavigationBarHeight), nameof(GameControlHeight));
                _eventBus.Publish(new GameControlSizeChangedEvent(GameControlHeight));
            }
        }

        public double GameControlHeight
        {
            get
            {
                if (MessageOverlayDisplay.IsReplayRecoveryDlgVisible)
                {
                    return _gameControlHeight - ReplayNavigationBarHeight;
                }
                if (MainInfoBarOpenRequested)
                {
                    return _gameControlHeight - _mainInfoBarHeight;
                }

                return _gameControlHeight;
            }
            set
            {
                SetProperty(ref _gameControlHeight, value);

                _mainInfoBarHeight = Math.Max(InfoBarViewModel.BarHeightMinimum, _gameControlHeight * InfoBarViewModel.BarHeightFraction);
                Logger.Debug($"Calculate Infobar height => {_mainInfoBarHeight}, aspect ratio = {_aspectRatio}");
                if (_aspectRatio < 1)
                {
                    // A portrait window is used for two virtual windows (main and top)
                    _mainInfoBarHeight /= 2;
                    Logger.Debug($"Recalculate Infobar height => {_mainInfoBarHeight} because aspect ratio = {_aspectRatio}");
                }
                _eventBus.Publish(new InfoBarSetHeightEvent(_mainInfoBarHeight, DisplayRole.Main));
                RaisePropertyChanged(nameof(MarginInputs));
            }
        }

        private BannerDisplayMode LobbyBannerDisplayMode
        {
            get => _bannerDisplayMode;
            set => SetProperty(ref _bannerDisplayMode,
                value,
                nameof(LobbyBannerDisplayMode),
                nameof(IsIdleTextScrolling),
                nameof(IsBlinkingIdleTextVisible),
                nameof(StartIdleTextBlinking),
                nameof(IsScrollingIdleTextEnabled));
        }

        public bool PreserveGameLayoutSideMargins => Config?.PreserveGameLayoutSideMargins ?? false;

        public GameTabInfoViewModel GameTabInfo { get; } = new GameTabInfoViewModel();

        public PlayerMenuPopupViewModel PlayerMenuPopupViewModel { get; }

        public PlayerInfoDisplayMenuViewModel PlayerInfoDisplayMenuViewModel { get; }

        public LobbyVolumeViewModel Volume { get; }

        public bool IsInOperatorMenu => _operatorMenu.IsShowing;

        public bool IsSingleGameMode => (_lobbyStateManager?.AllowGameInCharge ?? false) && UniqueThemeIds <= 1;

        private int UniqueThemeIds => (GameList?.Where(g => g.Enabled).Select(o => o.ThemeId).Distinct().Count() ?? 0);

        /// <summary>
        ///     Dispose
        /// </summary>
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        public void DisplayMessage(DisplayableMessage displayableMessage)
        {
            Logger.Debug($"Displaying message: {displayableMessage}");

            switch (displayableMessage.Classification)
            {
                case DisplayableMessageClassification.SoftError when Config.DisplaySoftErrors:
                    DisplayNotificationMessage(displayableMessage);
                    break;
                case DisplayableMessageClassification.Informative when Config.DisplayInformationMessages:
                    DisplayNotificationMessage(displayableMessage);
                    break;
                case DisplayableMessageClassification.HardError:
                    MessageOverlayDisplay.AddHardErrorMessage(displayableMessage);
                    break;
            }

            Logger.Debug("Displayed message");
        }

        public void RemoveMessage(DisplayableMessage displayableMessage)
        {
            switch (displayableMessage.Classification)
            {
                case DisplayableMessageClassification.SoftError when Config.DisplaySoftErrors:
                    RemoveNotificationMessage(displayableMessage);
                    break;
                case DisplayableMessageClassification.Informative when Config.DisplayInformationMessages:
                    RemoveNotificationMessage(displayableMessage);
                    break;
                case DisplayableMessageClassification.HardError:
                    MessageOverlayDisplay.RemoveHardErrorMessage(displayableMessage);
                    break;
            }
        }

        public void DisplayStatus(string message)
        {
        }

        public void ClearMessages()
        {
            Logger.Debug("Clearing messages");
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (Config.DisplaySoftErrors)
                    {
                        NotificationMessages.Clear();
                        _rotateSoftErrorTextTimer?.Stop();
                        CurrentNotificationText = string.Empty;
                    }

                    MessageOverlayDisplay.HardErrorMessages.Clear();
                    MessageOverlayDisplay.ForceBuildLockupText = false;

                    HandleMessageOverlayText();
                });
        }

        /// <summary>
        ///     Raised when the language changes.
        /// </summary>
        public event EventHandler LanguageChanged;

        /// <summary>
        ///     Handles the Loaded event for the corresponding View.  There is a time lag between the
        ///     constructor and the Loaded event, so it makes more sense to start timers and such in
        ///     OnLoaded.
        /// </summary>
        public void OnLoaded()
        {
            // we call ChangeLanguageSkin before this is called from LobbyViewModel.xaml.cs
            // So any text that needs to be localized from resources can be updated once
            // we are here.
            Logger.Debug("Lobby OnLoaded() complete");
            RaisePropertyChanged(nameof(PaidMeterLabel));
        }

        /// <summary>
        ///     Handles the Hwnd Loaded event for the corresponding View.
        /// </summary>
        public void OnHwndLoaded()
        {
            if (IsTabView)
            {
                GameTabInfo.GameTypeChanged = g => RefreshDisplayedGameList();
                GameTabInfo.DenominationChanged = g => RefreshDisplayedGameList(false, false);
                GameTabInfo.SubTabChanged = g => RefreshDisplayedGameList(true, false);
                SetTabViewToDefault();
            }

            RaisePropertyChanged(nameof(PreserveGameLayoutSideMargins));
            var idleText = (string)_properties.GetProperty(GamingConstants.IdleText, string.Empty);
            if (string.IsNullOrWhiteSpace(IdleText))
            {
                idleText = (string)LobbyView.TryFindResource(LobbyIdleTextDefaultResourceKey) ?? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.IdleTextDefault);
                _properties.SetProperty(GamingConstants.IdleText, idleText);
            }

            IdleText = idleText;

            var cabinetType = _cabinetDetectionService.Type.ToString();
            Logger.Debug($"Cabinet Type: {cabinetType}");

            if (!Config.DisableMalfunctionMessage)
            {
                IsVbdMalfunctionOverlayVisible = (bool)_properties.GetProperty(
                    ApplicationConstants.EnabledMalfunctionMessage,
                    false);
                Logger.Debug($"IsVbdMalfunctionOverlayVisible: {IsVbdMalfunctionOverlayVisible}");
            }

            _disabledOnStartup = _systemDisableManager.IsDisabled;
            _recoveryOnStartup = _gameHistory.IsRecoveryNeeded;
            IsDisableCountdownMessageSuppressed = _recoveryOnStartup;
            //gotta recover BEFORE we load properties from responsible gaming
            if (_gameHistory.IsRecoveryNeeded && !_systemDisableManager.DisableImmediately)
            {
                if (_properties.GetValue(GamingConstants.AutocompleteSet, false))
                {
                    _properties.SetProperty(GamingConstants.AutocompleteExpired, true);
                    DisableCountdownTimeRemaining = TimeSpan.Zero;

                    LobbyView.CreateAndShowDisableCountdownWindow();
                    _broadcastDisableCountdownMessagePending = true;
                }

                SendTrigger(LobbyTrigger.InitiateRecovery, false);
            }
            else
            {
                if (!_systemDisableManager.IsDisabled)
                {
                    _commandFactory.Create<CheckBalance>().Handle(new CheckBalance());
                }

                if (_properties.GetValue(GamingConstants.LaunchGameAfterReboot, false) && _bank.QueryBalance() != 0 &&
                    !_lobbyStateManager.AllowSingleGameAutoLaunch && !_gameHistory.IsRecoveryNeeded)
                {
                    var lastGameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
                    var lastGameDenom = _properties.GetValue(GamingConstants.SelectedDenom, 0L);

                    var game = GameList.FirstOrDefault(g => g.GameId == lastGameId && g.Denomination == lastGameDenom);
                    if (game != null)
                    {
                        _gameLaunchOnStartup = true;
                        SendTrigger(LobbyTrigger.LaunchGame, game);
                    }
                    else
                    {
                        SendTrigger(LobbyTrigger.LobbyEnter);
                    }
                }
                else
                {
                    SendTrigger(LobbyTrigger.LobbyEnter);
                }
            }

            if (!_gameHistory.IsRecoveryNeeded && _bank.QueryBalance() == 0)
            {
                // this will typically be unnecessary, but in the event there was a power hit or crash during printing we need to ensure rg is reset
                _responsibleGaming?.EndResponsibleGamingSession();
            }
            else
            {
                //and we have to do responsible gaming before disabled
                _responsibleGaming?.LoadPropertiesFromPersistentStorage();

                // now check--if we aren't in a responsible gaming session, but we have a bank balance and are NOT in a game session, something is wrong
                if (!_gameHistory.IsRecoveryNeeded && _bank.QueryBalance() != 0 &&
                    ResponsibleGamingSessionState == ResponsibleGamingSessionState.Stopped && _gameState.Idle)
                {
                    Logger.Debug("Detected money on machine but no Responsible Gaming Session.  Calling OnInitialCurrencyIn()");
                    _responsibleGaming?.OnInitialCurrencyIn();
                }
            }

            // Did we boot up in a disabled state?  Should not be recovering if we are disabled
            if (_systemDisableManager.IsDisabled && !_gameRecovery.IsRecovering && !_gameLaunchOnStartup)
            {
                SendTrigger(LobbyTrigger.Disable);

                HandleMessageOverlayText();
            }

            var gameInstaller = ServiceManager.GetInstance().TryGetService<IGameInstaller>();
            if (gameInstaller != null)
            {
                gameInstaller.UninstallStartedEventHandler += UninstallerStartedEvent;
            }

            GetVolumeButtonVisible();
            GetServiceButtonVisible();

            _eventBus.Publish(new LobbyInitializedEvent());

            _operatorMenu.EnableKey(ApplicationConstants.OperatorMenuInitializationKey);

            if ((bool)_properties.GetProperty(ApplicationConstants.ShowMode, false))
            {
                SetShowModeLobbyLabel();
#if !(RETAIL)
                IsDebugCurrencyButtonVisible = true;
#endif
            }

            var drm = ServiceManager.GetInstance().TryGetService<IDigitalRights>();
            if (drm?.IsDeveloper ?? false)
            {
                SetShowDeveloperLabel();
            }

            Logger.Debug("Lobby OnHwndLoaded() complete");
        }

        /// <summary>
        ///     Load the game information
        /// </summary>
        public void LoadGameInfo()
        {
            if (InDesigner)
            {
                return;
            }

            var games = _properties.GetValues<IGameDetail>(GamingConstants.Games).ToList();
            // Do not crash if game manifest does not provide the metadata for the expected locales.
            // This will just render bad data.
            foreach (var game in games)
            {
                if (!game.LocaleGraphics.ContainsKey(ActiveLocaleCode))
                {
                    game.LocaleGraphics.Add(ActiveLocaleCode, new LocaleGameGraphics());
                }
            }

            SetGameOrderFromConfig(games);

            var gameList = GetOrderedGames(games);

            GameTabInfo.SetupGameTypeTabs(gameList);
            ProgressiveLabelDisplay.UpdateProgressiveIndicator(gameList);

            GameList = gameList;
        }

        private void DisplayNotificationMessage(DisplayableMessage displayableMessage)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (NotificationMessages.Any(x => x.Equals(displayableMessage)))
                    {
                        return;
                    }

                    NotificationMessages.Add(displayableMessage);
                    if (NotificationMessages.Count != 1)
                    {
                        return;
                    }

                    _rotateSoftErrorTextTimer?.Start();
                    _currentNotificationIndex = 0;
                    CurrentNotificationText = NotificationMessages[0].Message;
                });
        }

        private void RemoveNotificationMessage(DisplayableMessage displayableMessage)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    NotificationMessages.Remove(x => x.Equals(displayableMessage));
                    if (NotificationMessages.Count != 0)
                    {
                        return;
                    }

                    _rotateSoftErrorTextTimer?.Stop();
                    CurrentNotificationText = string.Empty;
                });
        }

        private ObservableCollection<GameInfo> GetOrderedGames(IReadOnlyCollection<IGameDetail> games)
        {
            GameCount = games.Where(g => g.Enabled).Sum(g => g.ActiveDenominations.Count());
            ChooseGameOffsetY = UseSmallIcons ? 25.0 : 50.0;

            var gameCombos = (from game in games
                              from denom in game.ActiveDenominations
                              where game.Active
                              select new GameInfo
                              {
                                  GameId = game.Id,
                                  Name = game.ThemeName,
                                  InstallDateTime = game.InstallDate,
                                  DllPath = game.GameDll,
                                  ImagePath = UseSmallIcons ? game.LocaleGraphics[ActiveLocaleCode].SmallIcon : game.LocaleGraphics[ActiveLocaleCode].LargeIcon,
                                  TopPickImagePath = UseSmallIcons ? game.LocaleGraphics[ActiveLocaleCode].SmallTopPickIcon : game.LocaleGraphics[ActiveLocaleCode].LargeTopPickIcon,
                                  TopAttractVideoPath = game.LocaleGraphics[ActiveLocaleCode].TopAttractVideo,
                                  TopperAttractVideoPath = game.LocaleGraphics[ActiveLocaleCode].TopperAttractVideo,
                                  BottomAttractVideoPath = game.LocaleGraphics[ActiveLocaleCode].BottomAttractVideo,
                                  LoadingScreenPath = game.LocaleGraphics[ActiveLocaleCode].LoadingScreen,
                                  HasProgressiveOrBonusValue = !string.IsNullOrEmpty(game.DisplayMeterName),
                                  ProgressiveOrBonusValue = GetProgressiveOrBonusValue(game.Id, denom),
                                  ProgressiveIndicator = ProgressiveLobbyIndicator.Disabled,
                                  Denomination = denom,
                                  BetOption = game.Denominations.Single(d => d.Value == denom).BetOption,
                                  FilteredDenomination = Config.MinimumWagerCreditsAsFilter ? game.MinimumWagerCredits * denom : denom,
                                  GameType = game.GameType,
                                  GameSubtype = game.GameSubtype,
                                  PlatinumSeries = false,
                                  Enabled = game.Enabled,
                                  AttractHighlightVideoPath = !string.IsNullOrEmpty(game.DisplayMeterName) ? Config.AttractVideoWithBonusFilename : Config.AttractVideoNoBonusFilename,
                                  UseSmallIcons = UseSmallIcons,
                                  LocaleGraphics = game.LocaleGraphics,
                                  ThemeId = game.ThemeId,
                                  IsNew = GameIsNew(game.GameTags),
                                  Category = game.Category,
                                  SubCategory = game.SubCategory,
                                  RequiresMechanicalReels = game.MechanicalReels > 0
                              }).ToList();

            return new ObservableCollection<GameInfo>(
                gameCombos.OrderBy(game => _gameOrderSettings.GetPositionPriority(game.ThemeId))
                    .ThenBy(g => g.Denomination));
        }

        private void SetGameOrderFromConfig(IEnumerable<IGameDetail> games)
        {
            var distinctThemeGames = games.GroupBy(p => p.ThemeId).Select(g => g.FirstOrDefault(e => e.Active)).ToList();

            _gameOrderSettings.SetGameOrderFromConfig(distinctThemeGames.Select(g => (new GameInfo { InstallDateTime = g.InstallDate, ThemeId = g.ThemeId }) as IGameInfo).ToList(),
                Config.DefaultGameDisplayOrderByThemeId.ToList());
        }

        /// <summary>
        ///     Launches the game from the UI.
        /// </summary>
        /// <param name="info">The GameInfo for the game</param>
        public void LaunchGameFromUi(object info)
        {
            OnUserInteraction();
            PlayAudioFile(Sound.Touch);
            if (info is GameInfo game)
            {
                if (IsTabView)
                {
                    SetSelectedGame(game);
                }

                if (CurrentState == LobbyState.AgeWarningDialog)
                {
                    _launchGameAfterAgeWarning = game;
                }
                else
                {
                    if (_systemDisableManager.IsDisabled && CurrentState != LobbyState.Disabled && !_gameRecovery.IsRecovering)
                    {
                        Logger.Debug("LaunchGameFromUi triggering disable instead of game launch");
                        SendTrigger(LobbyTrigger.Disable);
                    }
                    else
                    {
                        _lobbyStateManager.AllowGameAutoLaunch = !_systemDisableManager.DisableImmediately;

                        Logger.Debug($"LaunchGameFromUI. GameReady={GameReady}. CurrentState={CurrentState}.");
                        if (!GameReady && !IsInState(LobbyState.GameLoading)) // GameReady will be true if game process has not exited
                        {
                            SendTrigger(LobbyTrigger.LaunchGame, game);
                        }
                        else
                        {
                            Logger.Debug("Rejecting Game Launch because runtime process has not yet exited.");
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Post touch event
        /// </summary>
        /// <param name="ti">The TouchInfo for the touch event</param>
        public void PostTouchEvent(TouchInfo ti)
        {
            _eventBus.Publish(new TouchEvent(ti));
        }

        public void ExitResponsibleGamingInfoDialog()
        {
            SendTrigger(LobbyTrigger.ResponsibleGamingInfoExit);
        }

        public void TouchResponsibleGamingInfoDialog()
        {
            var strategy = Config.ResponsibleGamingInfo.ExitStrategy;

            if ((strategy & ResponsibleGamingInfoExitStrategy.TouchScreen) != ResponsibleGamingInfoExitStrategy.TouchScreen)
            {
                return;
            }

            SendTrigger(LobbyTrigger.ResponsibleGamingInfoExit);
        }

        public void OnResponsibleGamingInfoNavigationPressed()
        {
            _responsibleGamingInfoTimeOutTimer?.Stop();
            _responsibleGamingInfoTimeOutTimer?.Start();
        }

        public void ResizeGameWindow(Size newSize, Size layoutSize)
        {
            lock (_resizeLock)
            {
                // Maintain content aspect ratio when resizing game
                _aspectRatio = layoutSize.Width / layoutSize.Height;

                if (newSize.Width / _aspectRatio < newSize.Height)
                {
                    GameControlHeight = Math.Floor(newSize.Width / _aspectRatio);
                    GameControlWidth = Math.Floor(newSize.Width);
                }
                else
                {
                    GameControlHeight = Math.Floor(newSize.Height);
                    GameControlWidth = Math.Floor(newSize.Height * _aspectRatio);
                }

                _eventBus.Publish(new GameControlSizeChangedEvent(GameControlHeight));
            }
        }

        private void OnStateEntry(LobbyState state, object obj)
        {
            Logger.Debug($"OnStateEntry to [{state}]");
            switch (state)
            {
                case LobbyState.Chooser:
                    MvvmHelper.ExecuteOnUI(OnChooserEnter);
                    break;
                case LobbyState.ChooserScrollingIdleText:
                    OnChooserShowingIdleTextEnter();
                    break;
                case LobbyState.ChooserIdleTextTimer:
                    OnChooserIdleTextTimerEnter();
                    break;
                case LobbyState.Attract:
                    OnAttractEnter();
                    break;
                case LobbyState.GameLoading:
                    if (obj is GameInfo info)
                    {
                        OnGameLoading(info);
                    }

                    break;
                case LobbyState.GameLoadingForDiagnostics:
                    if (obj is GameInfo gameInfo)
                    {
                        OnGameLoadingForReplay(gameInfo);
                    }

                    break;
                case LobbyState.Game:
                case LobbyState.GameDiagnostics:
                    OnGameLoaded();
                    break;
                case LobbyState.ResponsibleGamingInfo:
                    OnResponsibleGamingInfoEnter();
                    break;
                case LobbyState.Disabled:
                    OnDisabled();
                    break;
                case LobbyState.Recovery:
                    if (obj is bool processExited)
                    {
                        OnRecovery(processExited);
                    }

                    break;
                case LobbyState.RecoveryFromStartup:
                    OnRecoveryFromStartup();
                    break;
                case LobbyState.ResponsibleGamingTimeLimitDialog:
                    OnResponsibleGamingTimeLimitDialogEnter();
                    break;
                case LobbyState.AgeWarningDialog:
                    _ageWarningTimer.OnAgeWarningDialogEnter();
                    UpdateUI();
                    break;
                case LobbyState.PrintHelpline:
                    OnPrintHelplineEnter();
                    break;
                case LobbyState.CashIn:
                    OnCashInEnter();
                    break;
                case LobbyState.CashOut:
                    OnCashOutEnter();
                    break;
            }
        }

        private void OnStateExit(LobbyState state, object obj)
        {
            Logger.Debug($"OnStateExit from [{state}]");

            switch (state)
            {
                case LobbyState.Chooser:
                    OnChooserExit();
                    break;
                case LobbyState.Attract:
                    OnAttractExit();
                    break;
                case LobbyState.GameDiagnostics:
                    OnGameReplayExit();
                    break;
                case LobbyState.ResponsibleGamingInfo:
                    OnResponsibleGamingInfoExit();
                    break;
                case LobbyState.Disabled:
                    OnEnabled();
                    break;
                case LobbyState.AgeWarningDialog:
                    _ageWarningTimer.OnAgeWarningDialogExit();
                    break;
                case LobbyState.CashIn:
                    OnCashInExit();
                    break;
                case LobbyState.CashOut:
                    OnCashOutExit();
                    break;
                case LobbyState.Startup:
                    _isInitialStartup = false;
                    break;
            }
        }

        private void TryLaunchSingleGame()
        {
            var disableByOperatorManager = ServiceManager.GetInstance().GetService<IDisableByOperatorManager>();
            if (_gameHistory.IsRecoveryNeeded && ContainsAnyState(LobbyState.Disabled) || disableByOperatorManager.DisabledByOperator)
            {
                Logger.Debug($"TryLaunchSingleGame: IsRecoveryNeeded={_gameHistory.IsRecoveryNeeded}, " +
                             $"ContainsDisabledState={ContainsAnyState(LobbyState.Disabled)}, " +
                             $"DisabledByOperator={disableByOperatorManager.DisabledByOperator}");
                return;
            }

            if (_lobbyStateManager.AllowSingleGameAutoLaunch)
            {
                if (!GameReady && !IsInState(LobbyState.GameLoading))
                {
                    Logger.Debug("Automatically launch single game");
                    var currentGame = GameCount == 1 ? GameList.Single(g => g.Enabled) : GetSelectedGame();

                    if (currentGame != null)
                    {
                        LaunchGameFromUi(currentGame);
                        if (_systemDisableManager.IsDisabled)
                        {
                            SendTrigger(LobbyTrigger.Disable);
                            HandleMessageOverlayText();
                        }
                    }
                }
            }

            GameInfo GetSelectedGame()
            {
                return GameList.FirstOrDefault(g => g.GameId == _properties.GetValue(GamingConstants.SelectedGameId, 0)) ??
                       GameList.FirstOrDefault(g => g.Denomination == _properties.GetValue(GamingConstants.SelectedDenom, 0L)) ??
                       GameList.FirstOrDefault(g => g.Enabled);
            }
        }

        private void OnChooserEnter()
        {
            Logger.Debug("Entering Chooser");
            Debug.Assert(!(CurrentState == LobbyState.Chooser && IsIdleTextScrolling));
            MessageOverlayDisplay.ShowProgressiveGameDisabledNotification = false;

            UpdateUI();

            PlayerMenuPopupViewModel.IsMenuVisible = false;

            // Reset data trigger.
            GameLoadingScreenPath = "pack://siteOfOrigin:,,,/" + Config.DefaultLoadingScreenFilename;
            if (!_attractMode)
            {
                SetAlternatingTopImageResourceKey();
                SetAlternatingTopperImageResourceKey();
                _attractModeTopImageIndex = 0;
            }

            if (DateTime.UtcNow - _denomCheck >= TimeSpan.FromSeconds(IdleTimerIntervalSeconds))
            {
                // Reset denom filter if we've been away from the Chooser for more than IdleTimerIntervalSeconds
                DenomFilter = -1;
            }

            // Update any progressive values displayed the lobby each time we enter the lobby
            foreach (var game in GameList)
            {
                game.ProgressiveOrBonusValue = GetProgressiveOrBonusValue(game.GameId, game.Denomination);
                ProgressiveLabelDisplay.UpdateGameProgressiveText(game);
                ProgressiveLabelDisplay.UpdateGameAssociativeSapText(game);
            }

            ProgressiveLabelDisplay.UpdateMultipleGameAssociativeSapText();

            UpdateLamps();
            UpdateLcdButtonDeckRenderSetting(true);

            var selectedDenomViewInfo = GameTabInfo?.Denominations?.FirstOrDefault(d => d.Denomination == LastDenom);
            if (selectedDenomViewInfo != null)
            {
                GameTabInfo.SetSelectedDenomination(selectedDenomViewInfo);
            }

            _renderTimer?.Stop();
            _renderTimer?.Start();

            _idleTimer?.Stop();
            _idleTimer?.Start();

            StartAttractTimer();

            if (!_gameRecovery.IsRecovering)
            {
                _operatorMenu.EnableKey(GamingConstants.OperatorMenuDisableKey);
            }

            if (_launchGameAfterAgeWarning != null)
            {
                SendTrigger(LobbyTrigger.LaunchGame, _launchGameAfterAgeWarning);
            }

            _launchGameAfterAgeWarning = null;

            TryLaunchSingleGame();
        }

        private void OnChooserExit()
        {
            Logger.Debug("Exiting Chooser");
            _idleTimer?.Stop();
            _attractTimer?.Stop();
            _denomCheck = DateTime.UtcNow;
        }

        private void OnChooserShowingIdleTextEnter()
        {
            Logger.Debug("Entering Chooser showing Idle Text");
        }

        private void OnChooserIdleTextTimerEnter()
        {
            Logger.Debug("Waiting on Timer to Show Idle Text");
            _idleTextTimer?.Start();
        }

        private void OnAttractEnter()
        {
            if (_lobbyStateManager.AllowSingleGameAutoLaunch)
            {
                _eventBus.Publish(new ControlGameRenderingEvent(false));
            }

            Logger.Debug("Entering Attract Mode.");

            _attractMode = true;

            IsAttractModePlaying = true;

            /* make the _canOverrideEdgeLight true only if Lighting Override on Idle and Zero Credits
               is enabled in Hardware->Lighting */

            var edgeLightingAttractModeOverrideSelection = _properties.GetValue(
                ApplicationConstants.EdgeLightingAttractModeColorOverrideSelectionKey,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LightingOverrideTransparent));

            if (edgeLightingAttractModeOverrideSelection != Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LightingOverrideTransparent))
            {
                _canOverrideEdgeLight = true;
            }

            UpdateUI();

            PlayerMenuPopupViewModel.IsMenuVisible = false;

            IsPrimaryLanguageSelected = true;
            UpdateLcdButtonDeckRenderSetting(true);
            UpdateLcdButtonDeckDisableSetting(false);
            SetAttractVideos();

            //once we enter attract mode, the next time the user interacts with the machine we need to show the age warning dialog.
            _ageWarningTimer.AgeWarningNeeded = Config.DisplayAgeWarning;

            _lobbyStateManager.AllowGameAutoLaunch = true;

            _eventBus.Publish(new AttractModeEntered());
        }

        private void OnAttractExit()
        {
            Logger.Debug("Exiting Attract Mode.");

            IsAttractModePlaying = false;

            StopAndUnloadAttractVideo();

            // Increment to next attract mode video.
            bool wrap = AdvanceAttractIndex();
            if (Config.AlternateAttractModeLanguage)
            {
                _nextAttractModeLanguageIsPrimary = !_nextAttractModeLanguageIsPrimary;
            }

            SetEdgeLighting();

            if (wrap && Config.AlternateAttractModeLanguage)
            {
                _nextAttractModeLanguageIsPrimary = !_lastInitialAttractModeLanguageIsPrimary;
                _lastInitialAttractModeLanguageIsPrimary = _nextAttractModeLanguageIsPrimary;
            }

            SetAttractVideos();

            _consecutiveAttractCount = 0;

            SetTabViewToDefault();

            _eventBus.Publish(new AttractModeExited());
        }

        private void UninstallerStartedEvent(object sender, EventArgs eventArgs)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    OnUserInteraction();

                    OnUninstallStarted();

                    var args = eventArgs as InstallerEventArgs;
                    args?.WaitHandle.Set();
                });
        }

        private void OnUninstallStarted()
        {
            Logger.Debug("Entering install Mode.");
            StopAndUnloadAttractVideo();
        }

        private void OnGameLoading(GameInfo game)
        {
            Logger.Debug("Entering On Game Loading");

            if (game == null)
            {
                return;
            }

            if (File.Exists(game.DllPath) == false)
            {
                return;
            }

            Logger.Debug($"Preparing to launch game {game.Name} with an id of {game.GameId}");

            UpdateUI();

            if (!string.IsNullOrEmpty(game.LoadingScreenPath))
            {
                GameLoadingScreenPath = "file:///" + game.LoadingScreenPath;
            }

            // Disable operator menu during game loading.
            if (!_systemDisableManager.IsDisabled)
            {
                _operatorMenu.DisableKey(GamingConstants.OperatorMenuDisableKey);
            }

            UpdateLcdButtonDeckRenderSetting(false);

            _normalGameExitReceived = false;

            _eventBus.Publish(
                new GameSelectedEvent(
                    game.GameId,
                    game.Denomination,
                    game.BetOption,
                    _gameDiagnostics.IsActive,
                    GameBottomHwnd,
                    GameTopHwnd,
                    GameVirtualButtonDeckHwnd,
                    GameTopperHwnd));
        }

        private void OnGameLoadingForReplay(GameInfo game)
        {
            Logger.Debug("Entering On Game Loading For Replay");
            OnGameLoading(game);
        }

        private void OnGameLoaded()
        {
            Logger.Debug("Game loaded");
            _recoveryOnStartup = false;

            // Keep it disabled if we are replaying.
            if (!_gameDiagnostics.IsActive)
            {
                IsDisableCountdownMessageSuppressed = false;
            }
            else
            {
                UpdateLamps();
                _buttonLamps.EnableLamps();
            }

            UpdateLcdButtonDeckRenderSetting(false);
            UpdateLcdButtonDeckDisableSetting(false);
            UpdateUI();

            var softLockupButNotRecovery = _systemDisableManager.IsDisabled && !_gameRecovery.IsRecovering;
            var singleGameAndAttract = _lobbyStateManager.AllowSingleGameAutoLaunch && _attractMode;

            if (_systemDisableManager.DisableImmediately ||
                (singleGameAndAttract || _gameLaunchOnStartup) &&
                softLockupButNotRecovery)
            {
                SendTrigger(LobbyTrigger.Disable);
            }

            _gameLaunchOnStartup = false;
        }

        private void OnResponsibleGamingInfoEnter()
        {
            Logger.Debug("Entering Responsible Gaming Info dialog.");

            UpdateUI();

            _responsibleGamingInfoTimeOutTimer?.Stop(); // This should already be stopped, but why risk it?
            _responsibleGamingInfoTimeOutTimer?.Start();
        }

        private void OnResponsibleGamingInfoExit()
        {
            Logger.Debug("Exiting Responsible Gaming Info dialog.");
            _responsibleGamingInfoTimeOutTimer?.Stop();
        }

        private void OnGameReplayExit()
        {
            Logger.Debug("Game replay exited.");
            _buttonLamps.DisableLamps();
            UpdateUI();
        }

        private void OnDisabled()
        {
            // VLT-6699 : force an update to the timer so it is not blank
            ClockTimer.UpdateTime();

            Logger.Debug("Disabled State Entered");

            //VTL-4001 --if We get a lockup during a game load, we need to initiate the game shutdown.  We can't just wait for
            //the game loaded event to do so
            if (_lobbyStateManager.PreviousState == LobbyState.GameLoading && !IsSingleGameMode)
            {
                InitiateGameShutdown();
            }

            if (IsSingleGameMode && IsInOperatorMenu && GameReady && !ContainsAnyState(LobbyState.GameDiagnostics))
            {
                // Single game if operator menu is rapidly enabled and disabled can get stuck with the game open
                InitiateGameShutdown();
            }

            // Hide the sub text when we are disabled
            MessageOverlayDisplay.MessageOverlayData.IsDialogFadingOut = false;
            if (ContainsAnyState(LobbyState.CashOutFailure))
            {
                _lobbyStateManager.RemoveFlagState(LobbyState.CashOutFailure);
            }

            CheckHideCashoutDialog();

            UpdateUI();

            PlayerMenuPopupViewModel.IsMenuVisible = false;

            // TODO: Not sure if we should clear in lockup.Maybe just clear when audit menu is visible.
            // Lockup should just pause everything.

            UpdateLcdButtonDeckDisableSetting(true);
            _buttonLamps.DisableLamps();
            UpdateLcdButtonDeckRenderSetting(!IsGameRenderingToLcdButtonDeck() && !IsInOperatorMenu);

            _ageWarningTimer.Stop();

            CheckForExitGame();

            if (_lobbyStateManager.ResetAttractOnInterruption && CurrentAttractIndex != 0)
            {
                ResetAttractIndex();
                SetAttractVideos();
            }
        }

        private void OnEnabled()
        {
            // VLT-6699 : force an update to the timer so it is not blank
            ClockTimer.UpdateTime();

            //only run this code if we are currently in the disabled state, not one of the disabled sub-states.
            if (CurrentState == LobbyState.Disabled)
            {
                Logger.Debug("Disabled State Exited");

                CheckRestoreCashoutDialog();

                UpdateLcdButtonDeckDisableSetting(false);
                UpdateLcdButtonDeckRenderSetting(!IsGameRenderingToLcdButtonDeck() && !IsInOperatorMenu);
                UpdateLamps();
                _buttonLamps.EnableLamps();

                if (CanLaunchReplayOrRecovery())
                {
                    LaunchGameOrRecovery();
                }

                _disabledOnStartup = false;
                _lobbyStateManager.UnexpectedGameExitWhileDisabled = false; //clear value since we are leaving disabled state.

                // VLT-4742: Check for a max-cash-out when we come out of disabled state
                if (_gameState.Idle && !_transferOutHandler.InProgress && !_gameHistory.IsRecoveryNeeded && !_gameHistory.HasPendingCashOut)
                {
                    Logger.Debug("Checking for Max Balance coming out of lock-up");
                    _commandFactory.Create<CheckBalance>().Handle(new CheckBalance());
                }

                ClockTimer.RestartClockTimer();
            }
        }

        private void CheckHideCashoutDialog()
        {
            if (MessageOverlayDisplay.IsCashingOutDlgVisible && _systemDisableManager.DisableImmediately)
            {
                _cashoutDialogHidden = true;
                _cashoutDialogOpacity = MessageOverlayDisplay.MessageOverlayData.Opacity;
            }
        }

        private void CheckRestoreCashoutDialog()
        {
            if (_cashoutDialogHidden && !_systemDisableManager.DisableImmediately)
            {
                _cashoutDialogHidden = false;
                MessageOverlayDisplay.MessageOverlayData.Opacity = _cashoutDialogOpacity;
            }
        }

        private void GameLoadedWhileDisabled()
        {
            Logger.Debug("Game Loaded While Disabled");
            // VLT-3079: If lockup is created during game loading, just abort the
            // game.  Otherwise we get some weird platform/game sync bugs.
            // This should rarely happen in field, and if a lockup is generated,
            // aborting the game isn't really a bad thing.
            InitiateGameShutdown();

            //don't enable the key if we are aborting a recovery
            if (!_gameRecovery.IsRecovering)
            {
                _operatorMenu.EnableKey(GamingConstants.OperatorMenuDisableKey);
            }
        }

        private void InitiateGameShutdown()
        {
            Logger.Debug("InitiateGameShutdown");
            if (_lobbyStateManager.IsLoadingGameForRecovery)
            {
                //if we are trying to recover, just terminate the game process.  Don't shut down nicely.
                _eventBus.Publish(new TerminateGameProcessEvent());
            }
            else
            {
                _gameService?.ShutdownBegin();
            }
        }

        private void OnRecovery(bool processExited)
        {
            Logger.Debug($"Entering OnRecovery.  Process Exited: {processExited}");
            var lastGameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
            var lastGameDenom = _properties.GetValue(GamingConstants.SelectedDenom, 0L);

            var game = GameList.FirstOrDefault(g => g.GameId == lastGameId && g.Denomination == lastGameDenom);
            if (game != null && _gameRecovery.TryStartRecovery(lastGameId, processExited))
            {
                Logger.Debug("Initiating recovery");
                ResetResponsibleGamingDialog();

                _eventBus.Publish(new RecoveryStartedEvent());
                // VLT-4530:  Stop the Responsible Game Timer while we attempt to recover
                _responsibleGaming?.OnGamePlayDisabled();
                SendTrigger(LobbyTrigger.LaunchGame, game);
            }
            else
            {
                //recovery didn't start because TryStartRecovery said we didn't need to.
                Logger.Debug("Failed to start recovery");
                Logger.Debug("Most likely because we didn't need to recover");
                Logger.Debug($"Game History Is Recovery Needed: {_gameHistory.IsRecoveryNeeded}");
                Debug.Assert(
                    !_gameHistory.IsRecoveryNeeded,
                    "Game History IsRecoveryNeeded is true, but we failed to start recovery in OnRecovery");

                SendTrigger(LobbyTrigger.LobbyEnter);
            }

            PlayerMenuPopupViewModel.IsMenuVisible = false;
        }

        private void OnRecoveryFromStartup()
        {
            Logger.Debug("Entering OnRecoveryFromStartup.");
            UpdateUI();
        }

        private void OnResponsibleGamingTimeLimitDialogEnter()
        {
            Logger.Debug("Entering OnResponsibleGamingTimeLimitDialog.");
            _eventBus.Publish(new TimeLimitDialogVisibleEvent(_responsibleGaming?.SessionCount == 0, _responsibleGaming?.IsSessionLimitHit ?? false));

            UpdateUI();
        }

        private void OnPrintHelplineEnter()
        {
            Logger.Debug("OnPrintHelpline Entered");
            OnUserInteraction();
            UpdateUI();
        }

        private void OnCashInEnter()
        {
            Logger.Debug("Entering OnCashIn");
            _attractTimer?.Stop();
            OnUserInteraction();
        }

        private void OnCashInExit()
        {
            Logger.Debug("Exiting OnCashIn");
            MessageOverlayDisplay.IsCashingInDlgVisible = false;
            if (BaseState == LobbyState.Chooser)
            {
                StartAttractTimer(); // In Case Cash-In Failed and Bank == 0
            }

            OnUserInteraction();
        }

        private void OnCashOutEnter()
        {
            Logger.Debug("Entering OnCashOut");
            _justCashedOut = true;
            PlayerMenuPopupViewModel.IsMenuVisible = false;
            OnUserInteraction();
        }

        private void OnCashOutExit()
        {
            Logger.Debug("Exiting OnCashOut");
            _lobbyStateManager.CashOutState = LobbyCashOutState.Undefined;
            if (BaseState == LobbyState.Chooser)
            {
                StartAttractTimer();
            }

            OnUserInteraction();
        }

        private void UpdateUI()
        {
            Logger.Debug($"Entering UpdateUI, state: {CurrentState}");

            if (_gameRecovery == null ||
                _gameHistory == null ||
                _lobbyStateManager == null ||
                _runtime == null ||
                _idleTextTimer == null)
            {
                Logger.Warn("Bailing from UpdateUI due to NULL reference.  We are probably shutting down.");
                return;
            }

            HandleMessageOverlayText();

            MediaPlayersResizing = ContainsAnyState(LobbyState.MediaPlayerResizing);

            VbdServiceButtonDisabled = ContainsAnyState(LobbyState.MediaPlayerResizing);

            ReplayRecovery.IsReplayNavigationVisible = MessageOverlayDisplay.IsReplayRecoveryDlgVisible &&
                (CurrentState == LobbyState.GameLoadingForDiagnostics || CurrentState == LobbyState.GameDiagnostics);

            ReplayRecovery.MessageText = (_gameRecovery.IsRecovering || _lobbyStateManager.IsLoadingGameForRecovery)
                                         && CurrentState == LobbyState.GameLoading
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.RecoveringText)
                : string.Empty;

            if (CurrentState == LobbyState.GameLoading ||
                ContainsAnyState(LobbyState.CashOut, LobbyState.CashIn, LobbyState.PrintHelpline, LobbyState.AgeWarningDialog))
            {
                // VLT-4248:  Clear Cash-Out Dialog on VBD when we load a game.
                // VLT-4169: Hide VBD Cash Out dialog if we are in a bill-in situation.
                IsVbdCashOutDialogVisible = false;
                ShowVbdServiceConfirmationDialog(false);
            }

            SetLobbyFlags();

            if (IsSingleGameMode)
            {
                IsTopScreenRenderingDisabled = (IsInState(LobbyState.Disabled) || IsInOperatorMenu) &&
                                                !ContainsAnyState(LobbyState.GameDiagnostics, LobbyState.GameLoadingForDiagnostics);
            }
            else
            {
                IsTopScreenRenderingDisabled = IsInState(LobbyState.Disabled) && (IsInLobby || IsInOperatorMenu) &&
                                                              !ContainsAnyState(LobbyState.GameDiagnostics, LobbyState.GameLoadingForDiagnostics);
            }

            IsTopperScreenRenderingDisabled = IsTopScreenRenderingDisabled;

            IsBottomLoadingScreenVisible = CurrentState == LobbyState.GameLoading ||
                                           CurrentState == LobbyState.GameLoadingForDiagnostics;

            IsTopLoadingScreenVisible = CurrentState == LobbyState.GameLoading ||
                                           CurrentState == LobbyState.GameLoadingForDiagnostics;

            IsTopperLoadingScreenVisible = CurrentState == LobbyState.GameLoading ||
                                           CurrentState == LobbyState.GameLoadingForDiagnostics;

            HandleIdleText();

            IsResponsibleGamingInfoDlgVisible = ContainsAnyState(LobbyState.ResponsibleGamingInfo);

            SetEdgeLighting();

            //Don't change the underlying state of these things when we shift to Disabled
            if (CurrentState != LobbyState.Disabled)
            {
                MessageOverlayDisplay.MessageOverlayData.Opacity = OpacityHalf;
                ReplayRecovery.BackgroundOpacity = OpacityFifth;

                if (Config.RotateTopImage)
                {
                    if (ShouldRotateTopImage())
                    {
                        if (_rotateTopImageTimer != null && !_rotateTopImageTimer.IsEnabled)
                        {
                            _isShowingAlternateTopImage = false;
                            SetAlternatingTopImageResourceKey();
                            _rotateTopImageTimer.Start();
                        }
                    }
                    else
                    {
                        _rotateTopImageTimer?.Stop();
                    }
                }

                if (Config.RotateTopperImage)
                {
                    if (ShouldRotateTopperImage())
                    {
                        if (_rotateTopperImageTimer != null && !_rotateTopperImageTimer.IsEnabled)
                        {
                            _isShowingAlternateTopperImage = false;
                            SetAlternatingTopperImageResourceKey();
                            _rotateTopperImageTimer.Start();
                        }
                    }
                    else
                    {
                        _rotateTopperImageTimer?.Stop();
                    }
                }

                if (CurrentState == LobbyState.GameLoadingForDiagnostics || CurrentState == LobbyState.GameDiagnostics)
                {
                    MessageOverlayDisplay.MessageOverlayData.Opacity = OpacityNone; // VLT-2919: Override opacity so not so dark
                    ReplayRecovery.BackgroundOpacity = _gameDiagnostics.AllowInput ? OpacityNone : 0.05;
                }
            }
            else //CurrentState == LobbyState.Disabled.
            {
                // VLT-4326: Do not include all Disabled states here because we handle Replay stuff in the above code block
                ReplayRecovery.BackgroundOpacity = OpacityFifth;
                MessageOverlayDisplay.MessageOverlayData.Opacity = _gameHistory.IsGameFatalError || _cashoutDialogHidden ? OpacityFull : OpacityHalf;
                _rotateTopImageTimer?.Stop();
                _rotateTopperImageTimer?.Stop();
                IsVbdCashOutDialogVisible = false;
                ShowVbdServiceConfirmationDialog(false);
            }

            if (CurrentState == LobbyState.Attract)
            {
                IsTopAttractFeaturePlaying = true;
                IsTopperAttractFeaturePlaying = true;
                IsBottomAttractVisible = Config.BottomAttractVideoEnabled;
                IsBottomAttractFeaturePlaying = Config.BottomAttractVideoEnabled;
            }
            else
            {
                IsTopAttractFeaturePlaying = false;
                IsTopperAttractFeaturePlaying = false;
                IsBottomAttractVisible = false;
                IsBottomAttractFeaturePlaying = false;
            }

            var disableButtons = !IsInState(LobbyState.GameDiagnostics) &&
                                (IsInState(LobbyState.Disabled) ||
                                 MessageOverlayDisplay.ShowProgressiveGameDisabledNotification ||
                                 MessageOverlayDisplay.ShowVoucherNotification ||
                                 ContainsAnyState(
                                     LobbyState.CashOut,
                                     LobbyState.CashIn,
                                     LobbyState.CashOutFailure,
                                     LobbyState.PrintHelpline,
                                     LobbyState.MediaPlayerOverlay));

            EnableButtons(!disableButtons);

            SetVbdGameInput(
                ContainsAnyState(LobbyState.CashOut, LobbyState.PrintHelpline),
                ContainsAnyState(LobbyState.CashIn),
                IsTimeLimitDlgVisible,
                IsVbdServiceDialogVisible);

            IsVbdRenderingDisabled = (IsInState(LobbyState.Disabled) || _systemDisableManager.DisableImmediately || _recoveryOnStartup) &&
                                      CurrentState != LobbyState.GameLoadingForDiagnostics &&
                                      CurrentState != LobbyState.GameDiagnostics
                                     || ContainsAnyState(LobbyState.MediaPlayerOverlay)
                                     || MessageOverlayDisplay.MessageOverlayData.DisplayForPopUp;

            Logger.Debug($"IsVbdRenderingDisabled = {IsVbdRenderingDisabled}.  " +
                         $"CurrentState: {CurrentState}, " +
                         $"DisableImmediately: {_systemDisableManager.DisableImmediately}, " +
                         $"IsDisabled: {_systemDisableManager.IsDisabled}, " +
                         $"_recoveryOnStartup: {_recoveryOnStartup}");

            try
            {
                VbdVideoState = GetVbdVideoState();
            }
            catch (FileLoadException ex)
            {
                Logger.Fatal($"Bink Video Exception: {ex.Message} {ex.InnerException}");
            }

            // do responsible gaming last after we have done the state transition
            if (_responsibleGaming?.ShowTimeLimitDlgPending ?? false)
            {
                OnResponsibleGamingDialogPending();
            }

            RaisePropertiesChanged();

            PrintHelplineMessageCommand.RaiseCanExecuteChanged();

            Logger.Debug($"Leaving UpdateUI, state: {CurrentState}");
        }

        public void RaisePropertiesChanged()
        {
            RaisePropertyChanged(nameof(CashOutEnabled));
            RaisePropertyChanged(nameof(CashOutEnabledInPlayerMenu));
            RaisePropertyChanged(nameof(ResponsibleGamingInfoEnabled));
            RaisePropertyChanged(nameof(IsIdleTextBlinking));
            RaisePropertyChanged(nameof(StartIdleTextBlinking));
            RaisePropertyChanged(nameof(IsDebugMoneyEnabled));
            RaisePropertyChanged(nameof(IsNotificationTextVisible));
            RaisePropertyChanged(nameof(IsVirtualButtonDeckDisabled));
            RaisePropertyChanged(nameof(CanCashoutInLockup));
            RaisePropertyChanged(nameof(IsGameLoading));
            RaisePropertyChanged(nameof(IsServiceRequested));
            RaisePropertyChanged(nameof(ReturnToLobbyAllowed));
            RaisePropertyChanged(nameof(ReserveMachineAllowed));

#if !(RETAIL)
            _eventBus?.Publish(new CashoutButtonStatusEvent(CashOutEnabledInPlayerMenu));
#endif
        }

        private void MessageOverlayDisplay_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_disposed)
            {
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(MessageOverlayDisplay.IsReplayRecoveryDlgVisible):
                    RaisePropertyChanged(nameof(IsVirtualButtonDeckDisabled));
                    RaisePropertyChanged(nameof(GameControlHeight));
                    RaisePropertyChanged(nameof(IsMainInfoBarVisible));
                    _eventBus.Publish(new GameControlSizeChangedEvent(GameControlHeight));
                    break;
                case nameof(MessageOverlayDisplay.IsLockupMessageVisible):
                    RaisePropertyChanged(nameof(IsVirtualButtonDeckDisabled));
                    RaisePropertyChanged(nameof(CanCashoutInLockup));
                    break;
                case nameof(MessageOverlayDisplay.MessageOverlayData):
                    RaisePropertiesChanged();
                    break;
            }
        }

        private void HandleIdleText()
        {
            var newIdleTextVisible = BaseState == LobbyState.ChooserScrollingIdleText && IsScrollingIdleTextEnabled && !_disabledOnStartup;
            var newIdleTextPaused = CurrentState == LobbyState.Disabled;

            // VLT-6593:  We have an issue when the idle text is already paused, but we go from idle text invisible to visible.
            // The data triggers don't handle this properly.  So if we detect this transition, don't actually go visible.  Leave
            // the idle text invisible until we un-pause.

            if (newIdleTextVisible && !IsScrollingIdleTextVisible && newIdleTextPaused && IsIdleTextPaused)
            {
                Logger.Debug("Trying to move Idle Text to from invisible to visible while disabled.  Prevent this.");
                newIdleTextVisible = false;
            }

            IsScrollingIdleTextVisible = newIdleTextVisible;
            IsIdleTextPaused = newIdleTextPaused;

            if (BaseState != LobbyState.ChooserIdleTextTimer || CurrentState == LobbyState.Disabled)
            {
                // stopping the idle text timer here instead of in the state transitions to prevent it from stopping on state transitions
                // this keeps the text popping in while other dialogs are over the chooser
                _idleTextTimer?.Stop();
            }
        }

        private void SetLobbyFlags()
        {
            SetIsInLobby();
            // Hide the lobby in specific situations, such as recovery on boot
            // Note:  Always show lobby during game load for loading screen EXCEPT when we are disabled (VLT-6544)

            IsLobbyVisible = IsInLobby && ((!_recoveryOnStartup && !IsSingleGameMode) ||
                                           (ContainsAnyState(LobbyState.GameLoading) && !ContainsAnyState(LobbyState.Disabled)) ||
                                           ContainsAnyState(LobbyState.GameLoadingForDiagnostics));

            if (IsSingleGameMode)
            {
                RaisePropertyChanged(nameof(IsLobbyVbdVisible));
                RaisePropertyChanged(nameof(IsGameVbdVisible));
                RaisePropertyChanged(nameof(IsLobbyVbdBackgroundBlank));
            }
        }

        private void SetIsInLobby()
        {
            var baseState = BaseState;

            IsInLobby =
                   (baseState == LobbyState.Chooser ||
                   baseState == LobbyState.ChooserScrollingIdleText ||
                   baseState == LobbyState.ChooserIdleTextTimer ||
                   baseState == LobbyState.Attract ||
                   baseState == LobbyState.Recovery ||
                   baseState == LobbyState.GameLoading) &&
                   !_lobbyStateManager.ContainsAnyState(LobbyState.GameDiagnostics);
        }

        private bool CanLaunchReplayOrRecovery()
        {
            var baseState = BaseState;

            return baseState == LobbyState.Chooser ||
                   baseState == LobbyState.ChooserScrollingIdleText ||
                   baseState == LobbyState.ChooserIdleTextTimer ||
                   baseState == LobbyState.Attract ||
                   baseState == LobbyState.Recovery ||
                   baseState == LobbyState.RecoveryFromStartup ||
                   (baseState == LobbyState.GameLoading && _lobbyStateManager.IsLoadingGameForRecovery);
        }

        private bool ShouldRotateTopImage()
        {
            var baseState = BaseState;

            return baseState == LobbyState.Chooser ||
                   baseState == LobbyState.ChooserScrollingIdleText ||
                   baseState == LobbyState.ChooserIdleTextTimer ||
                   baseState == LobbyState.GameLoading;
        }

        private bool ShouldRotateTopperImage()
        {
            var baseState = BaseState;

            return baseState == LobbyState.Chooser ||
                   baseState == LobbyState.ChooserScrollingIdleText ||
                   baseState == LobbyState.ChooserIdleTextTimer ||
                   baseState == LobbyState.GameLoading;
        }
        private LobbyVbdVideoState GetVbdVideoState()
        {
            LobbyVbdVideoState state;

            // first override everything for Manitoba Learn About VLT Overlays
            if (CurrentState == LobbyState.PrintHelpline)
            {
                state = LobbyVbdVideoState.Disabled;
            }
            else if (IsTimeLimitDlgVisible)
            {
                // VLT-4319: If Responsible Gaming is ALC Mode, always show Blank Graphic in VBD during Responsible Gaming Dialogs.
                state = (_responsibleGaming.IsSessionLimitHit ||
                        ResponsibleGamingMode == ResponsibleGamingMode.Continuous ||
                        ResponsibleGamingCurrentDialogState == ResponsibleGamingDialogState.PlayBreak1 ||
                        ResponsibleGamingCurrentDialogState == ResponsibleGamingDialogState.PlayBreak2 ||
                        CurrentState == LobbyState.ResponsibleGamingInfoLayeredGame) &&
                        CurrentState != LobbyState.ResponsibleGamingInfoLayeredLobby
                    ? LobbyVbdVideoState.Disabled
                    : LobbyVbdVideoState.ChooseTime;
            }
            // Set Disabled state for a variety of states where we want a blank background, including when the Lobby VBD isn't visible
            else if (ContainsAnyState(LobbyState.CashOut, LobbyState.CashOutFailure) ||
                     CurrentState == LobbyState.GameLoading ||
                     CurrentState == LobbyState.GameLoadingForDiagnostics ||
                     CurrentState == LobbyState.Game ||
                     ContainsAnyState(LobbyState.AgeWarningDialog) ||
                     _launchGameAfterAgeWarning != null ||
                     !IsLobbyVbdVisible ||
                     IsSingleGameMode)
            {
                state = LobbyVbdVideoState.Disabled;
            }
            else
            {
                state = HasZeroCredits ? LobbyVbdVideoState.InsertMoney : LobbyVbdVideoState.ChooseGame;
            }

            return state;
        }

        /// <summary>
        ///     If there is no user interaction, then we stay in attract mode until the game specific
        ///     attract video completes.
        /// </summary>
        internal void OnGameAttractVideoCompleted()
        {
            // Have to run this on a separate thread because we are triggering off an event from the video
            // and we end up making changes to the video control (loading new video).  The Bink Video Control gets very upset
            // if we try to do that on the same thread.

            if (!PlayAdditionalConsecutiveAttractVideo())
            {
                RotateTopImageForAttractMode();
                RotateTopperImageForAttractMode();
                Task.Run(
                    () => { MvvmHelper.ExecuteOnUI(() => SendTrigger(LobbyTrigger.AttractVideoComplete)); });
            }
        }

        /// <summary>
        ///     Indicates that the user has interacted with the lobby.
        /// </summary>
        internal void OnUserInteraction()
        {
            Logger.Debug($"OnUserInteraction, state: {CurrentState}");

            // Reset idle timer when user interacted with lobby.
            if (_idleTimer != null && _idleTimer.IsEnabled)
            {
                _idleTimer.Stop();
                _idleTimer.Start();
            }

            _attractMode = false;
            if (_attractTimer != null && _attractTimer.IsEnabled)
            {
                StartAttractTimer();
            }

            // Don't display Age Warning while the inserting cash dialog is up.
            if (_ageWarningTimer.CheckForAgeWarning() == AgeWarningCheckResult.False)
            {
                if (CurrentState == LobbyState.Attract)
                {
                    SendTrigger(LobbyTrigger.AttractModeExit);
                }
            }

            if (_lobbyStateManager.ResetAttractOnInterruption && CurrentAttractIndex != 0)
            {
                ResetAttractIndex();
                SetAttractVideos();
            }

            _lobbyStateManager.OnUserInteraction();
            SetEdgeLighting();
        }

        /// <summary>
        ///     Cleanup.
        /// </summary>
        /// <param name="disposing">True if disposing; false if finalizing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            Logger.Debug("Disposing of LobbyViewModel");
            if (disposing)
            {
                _messageDisplay.RemoveMessageDisplayHandler(this);
                _eventBus?.UnsubscribeAll(this);

                _playerInfoDisplayManager.Dispose();

                Logger.Debug("Stopping Timers");
                _ageWarningTimer.Dispose();
                ClockTimer.Dispose();

                _attractTimer?.Stop();
                _idleTimer?.Stop();
                _idleTextTimer?.Stop();
                _renderTimer?.Stop();
                _responsibleGamingInfoTimeOutTimer?.Stop();
                _vbdConfirmationTimeOutTimer?.Stop();
                _disableCountdownTimer?.Stop();
                _rotateTopImageTimer?.Stop();
                _rotateTopperImageTimer?.Stop();
                _rotateSoftErrorTextTimer?.Stop();
                _printHelplineTicketTimer?.Stop();
                _debugCurrencyTimer?.Stop();
                _cashOutTimer?.Stop();
                _newGameTimer?.Stop();

                if (_responsibleGaming != null)
                {
                    Logger.Debug("Disposing Responsible Gaming");
                    _responsibleGaming.OnStateChange -= ResponsibleGamingStateChanged;
                    _responsibleGaming.PropertyChanged -= ResponsibleGamingOnPropertyChanged;
                    _responsibleGaming.ForceCashOut -= OnForceCashOut;
                    _responsibleGaming.OnForcePendingCheck -= ForcePendingResponsibleGamingCheck;
                    _responsibleGaming.Dispose();
                    _responsibleGaming = null;
                }

                if (_lobbyButtonDeckRenderer != null)
                {
                    Logger.Debug("Disposing Lobby Button Deck Render");
                    _lobbyButtonDeckRenderer.Dispose();
                    _lobbyButtonDeckRenderer = null;
                }

                var gameInstaller = ServiceManager.GetInstance().TryGetService<IGameInstaller>();
                if (gameInstaller != null)
                {
                    gameInstaller.UninstallStartedEventHandler -= UninstallerStartedEvent;
                }

                ReplayRecovery?.Dispose();
                ReplayRecovery = null;

                _lobbyStateManager.Dispose();

                if (_gameList != null)
                {
                    _gameList.CollectionChanged -= GameList_CollectionChanged;
                }
            }

            _eventBus = null;
            _attractTimer = null;
            _idleTimer = null;
            _idleTextTimer = null;
            _renderTimer = null;
            _responsibleGamingInfoTimeOutTimer = null;
            _vbdConfirmationTimeOutTimer = null;
            _disableCountdownTimer = null;
            _rotateTopImageTimer = null;
            _rotateTopperImageTimer = null;
            _rotateSoftErrorTextTimer = null;
            _printHelplineTicketTimer = null;
            _debugCurrencyTimer = null;
            _cashOutTimer = null;
            _newGameTimer = null;

            _disposed = true;
        }

        private void OnResponsibleGamingDialogPending(bool allowDialogWhileDisabled = false)
        {
            Logger.Debug($"OnResponsibleGamingDialogPending: allowDialogWhileDisabled: {allowDialogWhileDisabled}");
            // We allow the dialog while disabled if the dialog was up when the system went into the disabled state
            // and then we removed it to go into the operator menu
            lock (_responsibleGamingLockObject)
            {
                if (_responsibleGaming == null || !_responsibleGaming.ShowTimeLimitDlgPending ||
                    IsTimeLimitDlgVisible)
                {
                    // IsTimeLimitDialogVisible turns to TRUE before ShowTimeLimitDlgPending goes to FALSE
                    return;
                }

                // these are all the places we want to delay the responsible gaming dialog
                // Disabled state (includes Replay & GameLoadingForReplay), Game Loading, Cash coming in, Cashing out,
                // Recovery, any state pre-recovery after a reboot and in-game during a game round.
                var showResponsibleGamingDialog = !_responsibleGaming.SpinGuard &&
                                                  (!IsInState(LobbyState.Disabled) || allowDialogWhileDisabled) &&
                                                  CurrentState != LobbyState.GameLoading &&
                                                  !ContainsAnyState(
                                                          LobbyState.CashOut,
                                                          LobbyState.AgeWarningDialog,
                                                          LobbyState.PrintHelpline,
                                                          LobbyState.CashIn,
                                                          LobbyState.ResponsibleGamingInfoLayeredLobby,
                                                          LobbyState.ResponsibleGamingInfoLayeredGame,
                                                          LobbyState.MediaPlayerOverlay,
                                                          LobbyState.MediaPlayerResizing) &&
                                                  !IsInState(LobbyState.Recovery) &&
                                                  !_gameRecovery.IsRecovering &&
                                                  !_ageWarningTimer.AgeWarningNeeded &&
                                                  !(CurrentState == LobbyState.Game && _gameState.InGameRound) &&
                                                  (!_systemDisableManager.IsDisabled || allowDialogWhileDisabled) &&
                                                  !_gameHistory.IsRecoveryNeeded &&
                                                  !HasZeroCredits;

                Logger.Debug($"Show Responsible Gaming Dialog: {showResponsibleGamingDialog}");

                if (showResponsibleGamingDialog)
                {
                    if (HasZeroCredits)
                    {
                        // if the user has zero credits, end this responsible gaming session
                        Logger.Debug("Zero Credits--End Responsible Gaming Session");
                        _responsibleGaming.EndResponsibleGamingSession();
                    }
                    else
                    {
                        // otherwise show the dialog
                        Logger.Debug("Show Responsible Gaming Dialog");
                        _responsibleGaming.ShowDialog(allowDialogWhileDisabled);
                    }
                    IsVbdCashOutDialogVisible = false; //VLT-12355:  Cancel the VBD cashout dialog on Responsible Gaming banner
                }
            }
        }

        private void ResponsibleGamingOnPropertyChanged(
            object sender,
            PropertyChangedEventArgs propertyChangedEventArgs)
        {
            // Keep these in sync.
            if (propertyChangedEventArgs.PropertyName == ResponsibleGamingPropNameDialogVisible)
            {
                RaisePropertyChanged(nameof(IsTimeLimitDlgVisible));
                RaisePropertyChanged(nameof(ResponsibleGamingCurrentDialogState));
                ClockTimer.UpdateSessionTimeText();

                if (IsTimeLimitDlgVisible)
                {
                    SendTrigger(LobbyTrigger.ResponsibleGamingTimeLimitDialog);
                }
                else
                {
                    SendTrigger(LobbyTrigger.ResponsibleGamingTimeLimitDialogDismissed);
                    _eventBus.Publish(new TimeLimitDialogHiddenEvent(false, false));
                }
            }
            else if (propertyChangedEventArgs.PropertyName == ResponsibleGamingPropNameDialogPending)
            {
                if (_responsibleGaming?.ShowTimeLimitDlgPending ?? false)
                {
                    Task.Run(
                        () =>
                        {
                            MvvmHelper.ExecuteOnUI(
                                () =>
                                    OnResponsibleGamingDialogPending()); //need to thread this to allow async locking in responsible gaming to work.
                        });
                }
            }
            else if (propertyChangedEventArgs.PropertyName == ResponsibleGamingPropNameDialogState)
            {
                RaisePropertyChanged(nameof(ResponsibleGamingCurrentDialogState));
            }
            else if (propertyChangedEventArgs.PropertyName == ResponsibleGamingPropNameDialogResourceKey)
            {
                RaisePropertyChanged(nameof(ResponsibleGamingDialogResourceKey));
            }
        }

        private void EnableButtons(bool enable)
        {
            _buttonDeckFilter.FilterMode = enable ? ButtonDeckFilterMode.Normal : ButtonDeckFilterMode.Lockup;
        }

        private void PrevPage(object obj)
        {
            OnUserInteraction();

            if (DisplayedPageNumber == 1)
            {
                DisplayedPageNumber = PageCount;
            }
            else
            {
                --DisplayedPageNumber;
            }

            RefreshDisplayedGameList();
        }

        private void NextPage(object obj)
        {
            OnUserInteraction();

            if (DisplayedPageNumber == PageCount)
            {
                DisplayedPageNumber = 1;
            }
            else
            {
                ++DisplayedPageNumber;
            }

            RefreshDisplayedGameList();
        }

        private void LaunchGameOrRecovery()
        {
            Logger.Debug("LaunchGameOrRecovery Method");

            if (_gameHistory.IsRecoveryNeeded)
            {
                Logger.Debug("Game history says Recovery is needed");

                // If we go into recovery with relaunch pending, do not relaunch anymore.
                _gameDiagnostics.RelaunchGameId = 0;

                if (_disabledOnStartup && _properties.GetValue(GamingConstants.AutocompleteSet, false))
                {
                    _properties.SetProperty(GamingConstants.AutocompleteExpired, true);
                    DisableCountdownTimeRemaining = TimeSpan.Zero;

                    LobbyView.CreateAndShowDisableCountdownWindow();
                    _broadcastDisableCountdownMessagePending = true;
                }

                Logger.Debug("Sending Initiate Recovery Trigger");
                SendTrigger(
                    LobbyTrigger.InitiateRecovery,
                    _lobbyStateManager
                        .UnexpectedGameExitWhileDisabled); // ask runtime if it is idle if the game exited while we were disabled.
            }
            else if (_gameDiagnostics.RelaunchGameId != 0)
            {
                // This is supposed to be called when we Replay a game when another game is up
                // therefore we have to relaunch the original game.  Not sure this can actually happen.

                Logger.Debug("Game Replay Relaunch Game ID exists");
                var game = GameList.FirstOrDefault(g => g.GameId == _gameDiagnostics.RelaunchGameId);
                Logger.Debug("Sending Launch Game Trigger");
                SendTrigger(LobbyTrigger.LaunchGame, game);
                _gameDiagnostics.RelaunchGameId = 0;
            }
            else
            {
                _commandFactory.Create<CheckBalance>().Handle(new CheckBalance());
            }
        }

        private void BankPressed(object obj)
        {
            if (IsDemonstrationMode)
            {
                Logger.Debug($"Bank Click - INSERT MONEY : ${DemonstrationCashInAmount}");
                _eventBus.Publish(new DemonstrationCurrencyEvent(DemonstrationCashInAmount));
            }
        }

        private void CashOutPressed(object obj)
        {
            // TODO:  Not sure about time limit dialog here
            if (!(IsTimeLimitDlgVisible && (_responsibleGaming?.IsSessionLimitHit ?? false)) &&
                obj != null && obj.ToString().ToUpperInvariant() == "VBD" && _cabinetDetectionService.IsTouchVbd())
            {
                IsVbdCashOutDialogVisible = true;
                PlayAudioFile(Sound.Touch);
            }
            else
            {
                if (CurrentState == LobbyState.ResponsibleGamingInfo)
                {
                    ExitResponsibleGamingInfoDialog();
                }

                ExecuteOnUserCashOut();
            }
        }

        private void ServicePressed(object obj)
        {
            _attendant.OnServiceButtonPressed();
            RaisePropertyChanged(nameof(IsServiceRequested));
            PlayAudioFile(Sound.Touch);
        }

        private void AddDebugCashPressed(object obj)
        {
#if !(RETAIL)
            _eventBus.Publish(new DebugNoteEvent(DebugCashAmount));
            _disableDebugCurrency = true;
            _debugCurrencyTimer?.Start();
#endif
        }

        private void VbdCashoutDlgYesNoPressed(object obj)
        {
            if (obj.ToString().Equals("YES", StringComparison.InvariantCultureIgnoreCase))
            {
                ExecuteOnUserCashOut();
            }

            IsVbdCashOutDialogVisible = false;
        }

        private void VbdServiceDlgYesNoPressed(object obj)
        {
            if (obj.ToString().Equals("YES", StringComparison.InvariantCultureIgnoreCase))
            {
                _attendant.IsServiceRequested = true;
                RaisePropertyChanged(nameof(IsServiceRequested));
            }

            ShowVbdServiceConfirmationDialog(false);
        }

        private void DenomFilterPressed(object obj)
        {
            OnUserInteraction();
        }

        private void TimeLimitAccepted(object obj)
        {
            if (!_systemDisableManager.IsDisabled)
            {
                DismissResponsibleGamingDialog(int.Parse((string)obj));
            }
        }

        private void ResponsibleGamingDialogOpenButtonPressed(object obj)
        {
            OnUserInteraction();

            // Exit the dialog if it is up (Act as toggle button)
            if (ContainsAnyState(LobbyState.ResponsibleGamingInfo))
            {
                SendTrigger(LobbyTrigger.ResponsibleGamingInfoExit);
            }
            else
            {
                ResponsibleGaming.InfoPageIndex = 0;
                SendTrigger(LobbyTrigger.ResponsibleGamingInfoButton);
            }
        }

        private void ReturnToLobbyButtonPressed(object obj)
        {
            Logger.Debug("Return to lobby");
            PlayAudioFile(Sound.Touch);
            PlayerMenuPopupViewModel.IsMenuVisible = false;
            _runtime.SetRequestExitGame(true);
        }

        private void CashoutFromPlayerPopUpMenu(object obj)
        {
            Logger.Debug("Cashout Button Pressed from player pop up menu");
            PlayerMenuPopupViewModel.IsMenuVisible = false;
            _eventBus.Publish(new DownEvent((int)ButtonLogicalId.Collect));
        }

        private void RefreshDisplayedGameList(bool generateDenominationList = true, bool generateSubTabList = true)
        {
            if (_gameList != null)
            {
                IEnumerable<GameInfo> subset = _gameList.ToList();
                if (IsTabView)
                {
                    var gameListTypes = GameTabInfo.ConvertCategoryToDefaultGame(GameTabInfo.SelectedCategory);
                    subset = subset.Where(o =>
                    {
                        if (o.Category != GameCategory.Undefined)
                            return o.Category == GameTabInfo.SelectedCategory;

                        return gameListTypes.Contains(o.GameType);
                    }).ToList();

                    if (generateSubTabList)
                    {
                        var subTypes = subset.Select(x =>
                        {
                            if (x.SubCategory != GameSubCategory.Undefined)
                                return SubTabInfoViewModel.GetSubTypeText(x.SubCategory);

                            if (gameListTypes.Count > 1 && gameListTypes.Contains(x.GameType))
                                return x.GameType.ToString();

                            return x.GameSubtype;
                        }
                        );
                        GameTabInfo.SetSubTabs(subTypes.Distinct());
                    }

                    var subType = GameTabInfo.SelectedSubTabText;
                    if (!string.IsNullOrEmpty(subType))
                    {
                        subset = subset.Where(o =>
                        {
                            if (o.SubCategory != GameSubCategory.Undefined)
                                return o.SubCategory == SubTabInfoViewModel.ConvertToSubTab(subType);

                            if (gameListTypes.Count > 1)
                                return string.Compare(o.GameType.ToString(), subType, true, CultureInfo.InvariantCulture) == 0;

                            return string.Compare(o.GameSubtype, subType, true, CultureInfo.InvariantCulture) == 0;
                        });
                    }

                    if (generateDenominationList)
                    {
                        var denominations = subset.Select(o => o.Denomination).Distinct();
                        GameTabInfo.SetDenominations(denominations);
                    }

                    subset = subset.Where(o => o.Denomination == GameTabInfo.SelectedDenomination).ToList();
                }

                // Take the subset that will fit on a page.
                subset = subset
                        .Where(g => g.Enabled)
                        .Skip((DisplayedPageNumber - 1) * GamesPerPage)
                        .Take(GamesPerPage);

                DisplayedGameList.Clear();

                // When the tab is hosting extra large icons (i.e., for Lightning Link), a list of denoms PER game will appear
                // below the icon for the user to pick.
                if (IsExtraLargeGameIconTabActive)
                {
                    var distinctGameNames = _gameList.Where(g => g.Category == GameCategory.LightningLink)
                        .Select(g => g.Name)
                        .Distinct()
                        .Take(2); // Currently, there is only room for 2 extra large icons
                    foreach (var gameName in distinctGameNames)
                    {
                        // Each game will have multiple entries in the _gameList for each of its denoms. Take just one of those entries
                        // and set the denoms for it
                        var game = _gameList.First(g => g.Name == gameName);
                        var denominations = _gameList.Where(g => g.Name == gameName).Select(d => d.Denomination);
                        game.SetDenominations(denominations);
                        DisplayedGameList.Add(game);
                    }
                }
                else
                {
                    // Add subset of GameInfo items to display.
                    foreach (var gi in subset)
                    {
                        DisplayedGameList.Add(gi);
                    }
                }
            }

            if (IsTabView)
            {
                var gameToSelect = DisplayedGameList.FirstOrDefault(game => game.GameId == _selectedGame?.GameId);
                if (gameToSelect is null)
                {
                    SelectFirstDisplayedGame();
                }
                else
                {
                    SetSelectedGame(gameToSelect);
                }
            }

            RaisePropertyChanged(nameof(MarginInputs));
            RaisePropertyChanged(nameof(IsExtraLargeGameIconTabActive));
            RaisePropertyChanged(nameof(IsSingleTabView));
            RaisePropertyChanged(nameof(IsSingleDenomDisplayed));
            RaisePropertyChanged(nameof(IsSingleGameDisplayed));
        }

        private void SelectFirstDisplayedGame()
        {
            var firstEnabledGame = DisplayedGameList.FirstOrDefault(game => game.Enabled);
            SetSelectedGame(firstEnabledGame);
        }

        private void RefreshAttractGameList()
        {
            AttractList.Clear();

            if (Config.HasAttractIntroVideo)
            {
                AttractList.Add(new AttractVideoDetails
                {
                    BottomAttractVideoPath = Config.BottomAttractIntroVideoFilename,
                    TopAttractVideoPath = Config.TopAttractIntroVideoFilename,
                    TopperAttractVideoPath = Config.TopperAttractIntroVideoFilename
                });
            }

            AttractList.AddRange(GetAttractGameInfoList());

            CheckAndResetAttractIndex();
        }

        private bool IsAttractEnabled()
        {
            return _attractInfoProvider.IsAttractEnabled;
        }

        private IEnumerable<GameInfo> GetAttractGameInfoList()
        {
            if (!IsAttractEnabled())
            {
                return new List<GameInfo>();
            }

            IEnumerable<GameInfo> subset = _gameList
                .Where(g => g.Enabled)
                .DistinctBy(g => g.ThemeId).ToList();

            if (subset.DistinctBy(g => g.GameSubtype).Count() > 1)
                subset = subset.OrderBy(g => g.GameType).ThenBy(g => SubTabInfoViewModel.ConvertToSubTab(g.GameSubtype));

            var attractSequence = _attractInfoProvider.GetAttractSequence().Where(ai => ai.IsSelected).ToList();

            var configuredAttractGameInfo =
                (from ai in attractSequence
                 join g in subset on new { ai.ThemeId, ai.GameType } equals new { g.ThemeId, g.GameType }
                 select g).ToList();

            return configuredAttractGameInfo;

        }


        private void SetSelectedGame(GameInfo gameInfo)
        {
            // Unselect the previous selected game
            if (_selectedGame != null)
            {
                _selectedGame.IsSelected = false;
            }

            _selectedGame = gameInfo;

            // Select the new game, if there is one
            if (_selectedGame != null)
            {
                _selectedGame.IsSelected = true;
            }

        }

        private void NavigateSelectionTo(SelectionNavigation navigationOption)
        {
            switch (navigationOption)
            {
                case SelectionNavigation.NextGame:
                case SelectionNavigation.PreviousGame:
                    if (_selectedGame == null || DisplayedGameList.Count == 0)
                    {
                        return;
                    }

                    var selectedGameIndex = DisplayedGameList.IndexOf(_selectedGame);
                    if (selectedGameIndex < 0)
                    {
                        // Reset game selection if selected game is not currently in view
                        selectedGameIndex = 0;
                    }

                    selectedGameIndex = (navigationOption == SelectionNavigation.NextGame)
                        ? (selectedGameIndex + 1) % DisplayedGameList.Count
                        : selectedGameIndex - 1;

                    // Wrap selection back around to last game if navigating left while first game is selected
                    if (selectedGameIndex < 0)
                    {
                        selectedGameIndex = DisplayedGameList.Count - 1;
                    }

                    var gameToSelect = DisplayedGameList[selectedGameIndex];

                    SetSelectedGame(gameToSelect);
                    break;

                default:
                    Logger.Error("Invalid navigation option",
                        new ArgumentOutOfRangeException(nameof(navigationOption), navigationOption, null));
                    break;
            }
        }

        private enum SelectionNavigation
        {
            NextGame,
            PreviousGame,
        }

        private void OnLanguageChanged()
        {
            // Fixed defect VLT-3714.  Do not need to LoadGameInfo (takes along long time) when toggling between the languages.
            // We just need to change what is needed the currency format and the icon.  This way it will be fast.
            foreach (var game in GameList)
            {
                game.ProgressiveOrBonusValue = GetProgressiveOrBonusValue(game.GameId, game.Denomination);
                ProgressiveLabelDisplay.UpdateGameProgressiveText(game);
                if (IsExtraLargeGameIconTabActive)
                {
                    ProgressiveLabelDisplay.UpdateGameAssociativeSapText(game);
                }

                game.SelectLocaleGraphics(ActiveLocaleCode);
            }

            if (IsExtraLargeGameIconTabActive)
            {
                ProgressiveLabelDisplay.UpdateMultipleGameAssociativeSapText();
            }

            ClockTimer.UpdateTime();
            SendLanguageChangedEvent();
        }

        private void GameList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Handler for when the collection changes (e.g., items added, removed, moved, etc.).
            // This does not get raised when an individual item gets modified.  However, because
            // we use a DataTemplate when binding to ItemsControl, the ItemsControl will update
            // automatically when we modify an individual item because the GameInfo item implements
            // INotifyPropertyChanged.  Also note that _displayedGameList references a subset of
            // items in _gameList, so changes to _gameList[k] affects the _displayedGameList,
            // provided _displayedGameList contains _gameList[k].
            RefreshDisplayedGameList();
            RefreshAttractGameList();
        }

        private void NewGameTimer_Tick(object sender, EventArgs e)
        {
            EvaluateGamesForNew();
        }

        private void EvaluateGamesForNew()
        {
            // Only calculate IsNew if we have a positive DaysAsNew configuration setting
            if (Config.DaysAsNew <= 0) return;

            // Figure out if any games are no longer new.
            foreach (var gi in _gameList)
            {
                if (IsGameTagNewGame(gi.GameId))
                {
                    gi.IsNew = true;
                }
                else
                {
                    if (gi.InstallDateTime > DateTime.MinValue)
                    {
                        var ts = DateTime.UtcNow - gi.InstallDateTime;
                        gi.IsNew = ts.TotalDays < Config.DaysAsNew;
                    }
                    else
                    {
                        gi.IsNew = false;
                    }
                }
            }
        }

        private bool IsGameTagNewGame(int gameId)
        {
            var game = _properties.GetValues<IGameDetail>(GamingConstants.Games).FirstOrDefault(o => o.Id == gameId);

            if (game == null)
            {
                return false;
            }

            return GameIsNew(game.GameTags);
        }

        private void SetGameAsNew(GameInfo game, IEnumerable<string> gameTags)
        {
            if (game == null || gameTags == null)
            {
                return;
            }

            game.IsNew = GameIsNew(gameTags);
        }

        private bool GameIsNew(IEnumerable<string> gameTags)
        {
            // NewGame is the string that should be used to tag a game as new
            return gameTags != null && gameTags.Any(t => t != null && t.ToLower().Equals(GameTag.NewGame.ToString().ToLower()));
        }

        private void IdleTimer_Tick(object sender, EventArgs e)
        {
            // Restore filter defaults after certain amount of time.
            DenomFilter = -1;
            GameFilter = GameType.Undefined;
        }

        private void IdleTextTimer_Tick(object sender, EventArgs e)
        {
            _idleTextTimer?.Stop();
            SendTrigger(LobbyTrigger.IdleTextTimer);
        }

        private void AttractTimer_Tick(object sender, EventArgs e)
        {
            if (ShowAttractMode)
            {
                SendTrigger(LobbyTrigger.AttractTimer);
            }
        }

        private void ResponsibleGamingInfoTimeOutTimerOnTick(object sender, EventArgs eventArgs)
        {
            SendTrigger(LobbyTrigger.ResponsibleGamingInfoTimeOut);
        }

        private void VbdConfirmationTimeOutTimerOnTick(object sender, EventArgs e)
        {
            MvvmHelper.ExecuteOnUI(() => ShowVbdServiceConfirmationDialog(false));
        }

        private void RenderTimerTick(object sender, EventArgs e)
        {
            _lobbyButtonDeckRenderer?.Update();

        }

        private ButtonLampState SetLampState(LampName lampName, bool? on)
        {
            Logger.Debug($"Set {lampName} {on}");
            var lampState = on.HasValue ? on.Value ? LampState.On : LampState.Off : (LampState?)null;

            if (lampState.HasValue)
            {
                return new ButtonLampState((int)lampName, lampState.Value);
            }

            return null;
        }

        private string GetProgressiveOrBonusValue(int gameId, long denomId)
        {
            var game = _properties.GetValues<IGameDetail>(GamingConstants.Games).SingleOrDefault(g => g.Id == gameId);
            if (string.IsNullOrEmpty(game?.DisplayMeterName))
            {
                return string.Empty;
            }

            var currentValue = game.InitialValue;

            var meter =
                _gameStorage.GetValues<InGameMeter>(gameId, denomId, GamingConstants.InGameMeters)
                    .FirstOrDefault(m => m.MeterName == game.DisplayMeterName);
            if (meter != null)
            {
                currentValue = meter.Value;
            }

            return (currentValue / CurrencyExtensions.CurrencyMinorUnitsPerMajorUnit).FormattedCurrencyString();
        }

        private void DismissResponsibleGamingDialog(int choiceIndex)
        {
            IsVbdCashOutDialogVisible = false; //VLT-4680:  Cancel the VBD cashout dialog on Responsible Gaming interactions
            ShowVbdServiceConfirmationDialog(false);
            _responsibleGaming?.AcceptTimeLimit(choiceIndex);
        }

        public bool IsInState(LobbyState state)
        {
            return _lobbyStateManager.IsInState(state);
        }

        public bool ContainsAnyState(params LobbyState[] states)
        {
            return _lobbyStateManager.ContainsAnyState(states);
        }

        private bool GameReady => _lobbyStateManager.BaseState == LobbyState.GameLoading ||
                                  _lobbyStateManager.BaseState == LobbyState.Game;

        private void SendTrigger(LobbyTrigger trigger)
        {
            _lobbyStateManager.SendTrigger(trigger);
        }

        private void SendTrigger(LobbyTrigger trigger, object param)
        {
            _lobbyStateManager.SendTrigger(trigger, param);
        }

        private void OnIdleTextScrollingCompleted(object obj)
        {
            SendTrigger(LobbyTrigger.IdleTextScrollingComplete);
        }

        private void OnCashOutWrapperMouseDown(object obj)
        {
            if (IsResponsibleGamingInfoDlgVisible && !CashOutEnabled && !IsResponsibleGamingInfoFullScreen)
            {
                ExitResponsibleGamingInfoDialog();
                OnUserInteraction();
                if (obj is MouseButtonEventArgs e)
                {
                    e.Handled = true;
                }
            }
        }

        private void OnUpiPreviewMouseDown(object obj)
        {
            if (IsResponsibleGamingInfoDlgVisible && !IsResponsibleGamingInfoFullScreen)
            {
                ExitResponsibleGamingInfoDialog();
                OnUserInteraction();
                if (obj is MouseButtonEventArgs e)
                {
                    e.Handled = true;
                }
            }
        }

        private void ExecuteOnUserCashOut()
        {
            _cashoutController.GameRequestedCashout();
        }

        private void OnForceCashOut(object sender, EventArgs e)
        {
            Logger.Debug("OnForceCashOut");
            _responsibleGamingCashOutInProgress = true;
        }

        private void SetAttractVideos()
        {
            IAttractDetails attract = null;

            if (AttractList.Count > 0)
            {
                attract = AttractList[CurrentAttractIndex];
            }

            if (Config.AlternateAttractModeLanguage)
            {
                Logger.Debug($"Next Attract Mode Video will be in Primary Language: {_nextAttractModeLanguageIsPrimary}");
                var languageIndex = _nextAttractModeLanguageIsPrimary ? 0 : 1;

                TopAttractVideoPath =
                    attract?.GetTopAttractVideoPathByLocaleCode(Config.LocaleCodes[languageIndex]).NullIfEmpty() ??
                    Config.DefaultTopAttractVideoFilename;

                TopperAttractVideoPath =
                    attract?.GetTopperAttractVideoPathByLocaleCode(Config.LocaleCodes[languageIndex]).NullIfEmpty() ??
                    Config.DefaultTopperAttractVideoFilename;

                if (Config.BottomAttractVideoEnabled)
                {
                    BottomAttractVideoPath =
                        attract?.GetBottomAttractVideoPathByLocaleCode(Config.LocaleCodes[languageIndex]).NullIfEmpty() ??
                        Config.DefaultTopAttractVideoFilename;
                }
            }
            else
            {
                TopAttractVideoPath = attract?.TopAttractVideoPath.NullIfEmpty() ?? Config.DefaultTopAttractVideoFilename;

                TopperAttractVideoPath =
                    attract?.TopperAttractVideoPath.NullIfEmpty() ??
                    Config.DefaultTopperAttractVideoFilename;

                if (Config.BottomAttractVideoEnabled)
                {
                    BottomAttractVideoPath =
                        attract?.BottomAttractVideoPath.NullIfEmpty() ?? Config.DefaultTopAttractVideoFilename;
                }
            }
        }

        private bool ResetResponsibleGamingDialog(bool resetDueToOperatorMenu = false)
        {
            var dialogReset = false;
            if (IsTimeLimitDlgVisible)
            {
                Logger.Debug("Resetting Responsible Gaming Dialog");
                _responsibleGaming?.ResetDialog(resetDueToOperatorMenu);
                dialogReset = true;
            }

            return dialogReset;
        }

        private void HandleMessageOverlayText()
        {
            MessageOverlayDisplay.HandleMessageOverlayText(string.Empty);
        }

        private void BroadcastInitialDisableCountdownMessage()
        {
            _disableCountdownMessage.MessageCallback =
                () => $"{DisableCountdownMessage} {DisableCountdownTimeRemaining.ToString(_disableCountdownTimeFormat)}";

            _messageDisplay.DisplayMessage(_disableCountdownMessage);

            if (DisableCountdownTimeRemaining.TotalSeconds <= 0)
            {
                _eventBus.Publish(new DisableCountdownTimerExpiredEvent());
            }
        }

        private void StartDisableCountdownTimer(TimeSpan countdownTime)
        {
            if (_disableCountdownTimer != null && !_disableCountdownTimer.IsEnabled)
            {
                DisableCountdownTimeRemaining = countdownTime;

                if (GameReady)
                {
                    BroadcastInitialDisableCountdownMessage();
                }
                else
                {
                    LobbyView.CreateAndShowDisableCountdownWindow();
                    // In case the 'disable countdown' timer starts in the lobby and someone manages to start the game
                    // we can pick up the countdown with platform messages to the game
                    _broadcastDisableCountdownMessagePending = true;
                }

                _disableCountdownTimer?.Start();
            }
        }

        private void StopDisableCountdownTimer()
        {
            _disableCountdownTimer?.Stop();
            _messageDisplay.RemoveMessage(_disableCountdownMessage);

            // In case we're still waiting for 'Disable countdown' message to be passed on to game
            _broadcastDisableCountdownMessagePending = false;

            // If game is not ready we're still in lobby when this is called. Probably we
            // can do without this check and just issue this call anyway
            // because it shouldn't matter even in case the window was already closed
            if (!GameReady)
            {
                LobbyView.CloseDisableCountdownWindow();
            }
        }

        private void DisableCountdownTimerTick(object sender, EventArgs e)
        {
            DisableCountdownTimeRemaining = DisableCountdownTimeRemaining > TimeSpan.Zero
                ? DisableCountdownTimeRemaining.Add(new TimeSpan(0, 0, 0, -1))
                : TimeSpan.Zero;

            _disableCountdownMessage.MessageCallback =
                () => $"{DisableCountdownMessage} {DisableCountdownTimeRemaining.ToString(_disableCountdownTimeFormat)}";

            if (DisableCountdownTimeRemaining.TotalSeconds <= 0)
            {
                _disableCountdownTimer?.Stop();

                _eventBus.Publish(new DisableCountdownTimerExpiredEvent());
            }
        }

        private void ResponsibleGamingStateChanged(object sender, ResponsibleGamingSessionStateEventArgs e)
        {
            Logger.Debug($"Responsible Gaming State Changed: {e.State}");
            ResponsibleGamingSessionState = e.State;

            Task.Run(DoUiUpdatesForResponsibleGamingStateChange);
        }

        private void DoUiUpdatesForResponsibleGamingStateChange()
        {
            MvvmHelper.ExecuteOnUI(() => ClockTimer.UpdateClockTimer());
        }

        private void ForcePendingResponsibleGamingCheck(object sender, EventArgs e)
        {
            MvvmHelper.ExecuteOnUI(() => OnResponsibleGamingDialogPending());
        }

        private void PrintHelplineTicketTimerTick(object sender, EventArgs e)
        {
            _inPrintHelplineTicketWaitPeriod = false;
            PrintHelplineMessageCommand.RaiseCanExecuteChanged();
        }

        private void RotateTopImageTimerTick(object sender, EventArgs e)
        {
            _isShowingAlternateTopImage = !_isShowingAlternateTopImage;
            SetAlternatingTopImageResourceKey();
        }

        private void RotateTopperImageTimerTick(object sender, EventArgs e)
        {
            _isShowingAlternateTopperImage = !_isShowingAlternateTopperImage;
            SetAlternatingTopperImageResourceKey();
        }

        private void RotateSoftErrorTextTimerTick(object sender, EventArgs e)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (NotificationMessages.Count == 0)
                    {
                        CurrentNotificationText = string.Empty;
                    }
                    else
                    {
                        _currentNotificationIndex = (_currentNotificationIndex + 1) % NotificationMessages.Count;
                        CurrentNotificationText = NotificationMessages[_currentNotificationIndex].Message;
                    }
                });
        }

        private void DebugCurrencyTimerTick(object sender, EventArgs e)
        {
            _disableDebugCurrency = false;
            RaisePropertyChanged(nameof(IsDebugMoneyEnabled));
        }

        private void CashOutTimerTick(object sender, EventArgs e)
        {
            Logger.Debug("Cash Out Timer Tick");
            _cashOutTimer?.Stop();

            switch (CashOutDialogState)
            {
                case LobbyCashOutDialogState.Visible:
                    CashOutDialogState = LobbyCashOutDialogState.VisiblePendingCompletedEvent;
                    break;
                case LobbyCashOutDialogState.VisiblePendingTimeout:
                    // We were waiting for timer to hide dialog
                    ClearCashOutDialog(true);
                    break;
            }
        }

        private void CashInStarted(CashInType cashInType, bool showDialog = true)
        {
            _cashInStartZeroCredits = HasZeroCredits;
            Logger.Debug($"Cash In Started at Zero Credits: {_cashInStartZeroCredits}");
            MessageOverlayDisplay.CashInType = cashInType;
            MessageOverlayDisplay.IsCashingInDlgVisible = showDialog;
            MvvmHelper.ExecuteOnUI(() => _lobbyStateManager.AddFlagState(LobbyState.CashIn, cashInType));
        }

        private void CashInFinished()
        {
            var current = _bank.QueryBalance();
            Logger.Debug($"Cash In Started at Zero Credits: {_cashInStartZeroCredits} Cash In Current: {current}");
            if (_cashInStartZeroCredits.HasValue && _cashInStartZeroCredits.Value && current > 0)
            {
                if (IsInState(LobbyState.Chooser))
                {
                    // if we have been cycling through top images due to attract mode and we get money in, we need
                    // to reset to the default top image
                    SetAlternatingTopImageResourceKey();
                    SetAlternatingTopperImageResourceKey();
                }

                // RG needs to start a new session before we remove the CashIn flag.
                _responsibleGaming?.OnInitialCurrencyIn();
            }
            _cashInStartZeroCredits = null;
            _canOverrideEdgeLight = false;
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    _lobbyStateManager.RemoveFlagState(LobbyState.CashIn);
                    if (ContainsAnyState(LobbyState.CashOutFailure))
                    {
                        _lobbyStateManager.RemoveFlagState(LobbyState.CashOutFailure);
                    }
                });
            MessageOverlayDisplay.IsCashingInDlgVisible = false;
        }

        private string GetCurrentLanguageButtonResourceKey()
        {
            Logger.Debug("GetCurrentLanguageButtonResourceKey entered");
            if (!Config.MultiLanguageEnabled)
            {
                return string.Empty;
            }

            // return the opposite language of whatever is selected, since we show the language the button will switch you TO.
            // Note:  This will need to be updated to support more than 2 languages.
            return Config.LanguageButtonResourceKeys[IsPrimaryLanguageSelected ? 1 : 0];
        }

        private void SetVbdGameInput(bool cashingOut, bool validatingBill, bool displayResponsibleGamingDialog, bool displayOverlay)
        {
            Logger.Debug(
                $"SetVbdGameInput called.  cashingOut: {cashingOut}, validatingBill: {validatingBill}, displayRGDialog: {displayResponsibleGamingDialog}, displayOverlay: {displayOverlay}");

            _runtime.SetCashingOut(cashingOut);
            _runtime.SetValidatingBillNote(validatingBill);
            _runtime.SetDisplayingRGDialog(displayResponsibleGamingDialog);
            _runtime.SetDisplayingOverlay(displayOverlay);
        }

        private void OnClockSwitchPressed(object obj)
        {
            ClockTimer.ChangeClockState();
        }

        private void OnGameTabPressed(object obj)
        {
            if (obj is GameTabInfo gameTabInfo && GameTabInfo.SelectedCategory != gameTabInfo.Category)
            {
                MvvmHelper.ExecuteOnUI(() => GameTabInfo.SelectTab(gameTabInfo));
                PlayAudioFile(Sound.Touch);
            }
        }

        private void OnDenominationPressed(object obj)
        {
            if (obj is DenominationInfoViewModel denominationInfo)
            {
                MvvmHelper.ExecuteOnUI(() => GameTabInfo.SetSelectedDenomination(denominationInfo));
            }
            PlayAudioFile(Sound.Touch);
        }

        private void OnDenominationForSpecificGamePressed(object[] obj)
        {
            var gameName = (string)obj[0];
            var denom = (long)obj[1];

            var selectedGame = _gameList.FirstOrDefault(g => g.Name == gameName && g.Denomination == denom);

            Logger.Debug($"gameId: {selectedGame?.GameId}, gameName: {gameName}, denom: {denom}");

            LaunchGameFromUi(selectedGame);

            PlayAudioFile(Sound.Touch);
        }

        private void OnSubTabPressed(object obj)
        {
            if (obj is SubTabInfoViewModel subTabInfo)
            {
                MvvmHelper.ExecuteOnUI(() => GameTabInfo.SelectSubTab(subTabInfo));
            }
            PlayAudioFile(Sound.Touch);
        }

        private bool CanPrintHelplineMessage(object obj)
        {
            return Config.ResponsibleGamingInfo.PrintHelpline && !_inPrintHelplineTicketWaitPeriod &&
                   !ContainsAnyState(LobbyState.PrintHelpline) &&
                   ContainsAnyState(
                           LobbyState.ResponsibleGamingInfo,
                           LobbyState.ResponsibleGamingInfoLayeredLobby,
                           LobbyState.ResponsibleGamingInfoLayeredGame);
        }

        private void OnPrintHelplineMessage(object obj)
        {
            var printer = ServiceManager.GetInstance().TryGetService<IPrinter>();
            var ticketCreator = ServiceManager.GetInstance().TryGetService<IHelplineTicketCreator>();

            if (printer != null && ticketCreator != null)
            {
                SendTrigger(LobbyTrigger.PrintHelpline);
                var tempString = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HelpLineMessage);
                var ticket = ticketCreator.Create(tempString);

                printer.Print(ticket);
                _printingHelplineTicket = true;
            }
        }

        private void SetupTimeRemainingDataForRuntime()
        {
            ClockTimer.SetDisplayingTimeRemainingFlag();
        }

        private void CheckForExitGame()
        {
            var systemDisableCount = _systemDisableManager.CurrentDisableKeys.Count;
            bool disabledOnlyWithLiveAuthentication = (systemDisableCount == 1) &&
                                                      _systemDisableManager.CurrentDisableKeys.Contains(
                                                          ApplicationConstants.LiveAuthenticationDisableKey);
            if (HasZeroCredits && CurrentState == LobbyState.Disabled &&
                GameReady && _gameState.Idle && !_lobbyStateManager.AllowSingleGameAutoLaunch
                && (systemDisableCount > 0 && !disabledOnlyWithLiveAuthentication))
            {
                _gameService?.ShutdownBegin();
            }
        }

        private void SetShowModeLobbyLabel()
        {
            _eventBus.Publish(
                new InfoOverlayTextEvent
                {
                    Text = "Show",
                    TextGuid = Guid.NewGuid(),
                    Location = InfoLocation.TopLeft,
                    Clear = false
                });
        }

        private void SetShowDeveloperLabel()
        {
            _eventBus.Publish(
                new InfoOverlayTextEvent
                {
                    Text = "Developer",
                    TextGuid = Guid.NewGuid(),
                    Location = InfoLocation.TopLeft,
                    Clear = false
                });
        }

        private void SetEdgeLighting()
        {
            lock (_edgeLightLock)
            {
                Logger.Debug($"Current EdgeLightState:  {_currentEdgeLightState}");
                EdgeLightState? newState;

                if (ContainsAnyState(LobbyState.CashOut) && _lobbyStateManager.CashOutState != LobbyCashOutState.Undefined)
                {
                    newState = EdgeLightState.Cashout;
                }
                else if (!ContainsAnyState(LobbyState.Disabled) &&
                         (_attractMode && !Config.EdgeLightingOverrideUseGen8IdleMode ||
                          _canOverrideEdgeLight && Config.EdgeLightingOverrideUseGen8IdleMode))
                {
                    newState = EdgeLightState.AttractMode;
                }
                else if (BaseState == LobbyState.Game)
                {
                    newState = null;
                }
                else
                {
                    newState = EdgeLightState.Lobby;
                }

                if (newState != _currentEdgeLightState)
                {
                    _edgeLightingStateManager.ClearState(_edgeLightStateToken);
                    if (newState.HasValue)
                    {
                        _edgeLightStateToken = _edgeLightingStateManager.SetState(newState.Value);
                    }
                    _currentEdgeLightState = newState;
                    Logger.Debug($"New EdgeLightState:  {_currentEdgeLightState}");
                }
            }
        }

        private void UpdatePaidMeterValue(double paidCashAmount)
        {
            if (Config.DisplayPaidMeter)
            {
                var paidCashAmountText = paidCashAmount > 0 ?
                    paidCashAmount.FormattedCurrencyString() :
                    string.Empty;
                PaidMeterValue = paidCashAmountText;
            }
        }

        private void DetermineBashLampState(ref IList<ButtonLampState> buttonsLampState)
        {
            bool? state;
            if (BaseState == LobbyState.GameLoading)
            {
                state = null;
            }
            else if (ContainsAnyState(LobbyState.GameLoadingForDiagnostics, LobbyState.GameDiagnostics))
            {
                state = true;
            }
            else if (ContainsAnyState(LobbyState.Disabled, LobbyState.MediaPlayerOverlay))
            {
                state = false;
            }
            else if (BaseState == LobbyState.Game)
            {
                state = _justCashedOut ? CashOutEnabled : null;
            }
            else
            {
                state = true;
            }

            buttonsLampState.Add(SetLampState(LampName.Bash, state));
        }

        private void DetermineCollectLampState(ref IList<ButtonLampState> buttonsLampState)
        {
            bool? state;
            if (BaseState == LobbyState.GameLoading)
            {
                state = null;
            }
            else if (ContainsAnyState(LobbyState.Disabled, LobbyState.MediaPlayerOverlay))
            {
                state = false;
            }
            else if (BaseState == LobbyState.Game)
            {
                state = _justCashedOut ? CashOutEnabled : null;
            }
            else
            {
                state = CashOutEnabled;
            }

            buttonsLampState.Add(SetLampState(LampName.Collect, state));
        }

        private void DetermineNavLampStates(ref IList<ButtonLampState> buttonsLampState)
        {
            bool? state;
            if (BaseState == LobbyState.GameLoading)
            {
                state = null;
            }
            else if (ContainsAnyState(LobbyState.Disabled, LobbyState.MediaPlayerOverlay))
            {
                state = false;
            }
            else if (BaseState == LobbyState.Game)
            {
                state = _justCashedOut ? CashOutEnabled : null;
            }
            else
            {
                state = true;
            }

            buttonsLampState.Add(SetLampState(LampName.Bet3, state)); // prev game
            buttonsLampState.Add(SetLampState(LampName.Bet4, state)); // prev tab
            buttonsLampState.Add(SetLampState(LampName.Bet5, state)); // inc denom
            buttonsLampState.Add(SetLampState(LampName.Playline5, state)); //next tab
            buttonsLampState.Add(SetLampState(LampName.Playline4, state)); //next game
        }

        private void DetermineUnusedLampStates(ref IList<ButtonLampState> buttonsLampState)
        {
            bool? state = false;
            if ((BaseState == LobbyState.Game && !ContainsAnyState(LobbyState.Disabled, LobbyState.MediaPlayerOverlay)) ||
                BaseState == LobbyState.GameLoading)
            {
                state = _justCashedOut ? CashOutEnabled : null;
            }

            buttonsLampState.Add(SetLampState(LampName.Bet1, state));
            buttonsLampState.Add(SetLampState(LampName.Bet2, state));
        }

        private void UpdateLamps()
        {
            IList<ButtonLampState> buttonsLampState = new List<ButtonLampState>();
            DetermineUnusedLampStates(ref buttonsLampState);
            DetermineNavLampStates(ref buttonsLampState);
            DetermineBashLampState(ref buttonsLampState);
            DetermineCollectLampState(ref buttonsLampState);
            _buttonLamps?.SetLampState(buttonsLampState);
        }

        private void UpdateLampsCashOutFinished()
        {
            UpdateLamps();
            _justCashedOut = false;
        }

        private void SendLanguageChangedEvent(bool initializing = false)
        {
            Logger.Debug($"SendLanguageChangedEvent.  Initializing: {initializing}");
            // if this is called from the initializing code, we only want to send the event if we haven't previously sent an event.
            if (!initializing || !_initialLanguageEventSent)
            {
                Logger.Debug($"Publishing PlayerLanguageChangedEvent with LocaleCode:  {ActiveLocaleCode}");
                _eventBus.Publish(new PlayerLanguageChangedEvent(ActiveLocaleCode));

                // todo let player culture provider manage multi-language support for lobby
                _properties.SetProperty(ApplicationConstants.LocalizationPlayerCurrentCulture, ActiveLocaleCode);
            }

            _initialLanguageEventSent = true;
        }

        private void LoadSoundFiles()
        {
            _soundFilePathMap.Add(Sound.Touch, _properties.GetValue(ApplicationConstants.TouchSoundKey, string.Empty));
            _soundFilePathMap.Add(Sound.CoinIn, _properties.GetValue(ApplicationConstants.CoinInSoundKey, string.Empty));
            _soundFilePathMap.Add(Sound.CoinOut, _properties.GetValue(ApplicationConstants.CoinOutSoundKey, string.Empty));
            _soundFilePathMap.Add(Sound.FeatureBell, _properties.GetValue(ApplicationConstants.FeatureBellSoundKey, string.Empty));
            _soundFilePathMap.Add(Sound.Collect, _properties.GetValue(ApplicationConstants.CollectSoundKey, string.Empty));
            _soundFilePathMap.Add(Sound.PaperInChute, _properties.GetValue(ApplicationConstants.PaperInChuteSoundKey, string.Empty));

            foreach (var filePath in _soundFilePathMap.Values.Where(filePath => !string.IsNullOrEmpty(filePath)))
            {
                _audio.Load(Path.GetFullPath(filePath));
            }
        }

        private void PlayAudioFile(Sound sound, Action callback = null)
        {
            if (_audio == null)
            {
                return;
            }

            _soundFilePathMap.TryGetValue(sound, out var filePath);
            if (!string.IsNullOrEmpty(filePath))
            {
                _audio.Play(filePath, Volume.GetVolume(_audio), SpeakerMix.All, callback);
            }
        }

        private void PlayLoopingAlert(Sound sound, int loopCount) => PlayLoopingAudioFile(
            sound,
            loopCount,
            _properties.GetValue(ApplicationConstants.AlertVolumeKey, DefaultAlertVolume));

        private void PlayLoopingAudioFile(Sound sound, int loopCount, float volume)
        {
            _soundFilePathMap.TryGetValue(sound, out var filePath);

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            if (sound == Sound.PaperInChute)
            {
                if (_audio.IsPlaying(filePath))
                {
                    return;
                }

                volume = (float)(volume * PaperInChuteAlertVolumeRate);
            }
            _audio.Play(filePath, loopCount, volume);
        }

        private void StopSound(Sound sound)
        {
            _soundFilePathMap.TryGetValue(sound, out var filePath);
            if (!string.IsNullOrEmpty(filePath))
            {
                _audio.Stop(filePath);
            }
        }

        private void PlayGameWinHandPaySound()
        {
            _playCollectSound = true;

            var callback = new Action(
                () =>
                {
                    if (_playCollectSound)
                    {
                        PlayAudioFile(Sound.Collect);
                    }
                });

            PlayAudioFile(Sound.FeatureBell, callback);
        }

        private void SetLanguage(string localeCode)
        {
            if (string.IsNullOrWhiteSpace(localeCode))
                return;

            //G2S uses _ instead of - in their Locale Codes
            localeCode = localeCode.Replace('_', '-');

            int languageIndex = -1;

            for (int i = 0; i < Config.LocaleCodes.Length && languageIndex == -1; i++)
            {
                if (string.Compare(Config.LocaleCodes[i], localeCode, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    languageIndex = i;
                }
            }

            if (languageIndex == -1)
            {
                // couldn't find the language.  Search TwoLetterISOLanguageName
                for (int i = 0; i < Config.LocaleCodes.Length && languageIndex == -1; i++)
                {
                    var culture = new CultureInfo(Config.LocaleCodes[i]);
                    if (string.Compare(culture.TwoLetterISOLanguageName, localeCode, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        languageIndex = i;
                    }
                }
            }

            if (languageIndex != -1)
            {
                IsPrimaryLanguageSelected = languageIndex == 0;
            }
        }

        private void ShowVbdServiceConfirmationDialog(bool show)
        {
            if (show)
            {
                // Ignore show request if another VBD dialog is present already
                if (IsVbdCashOutDialogVisible)
                {
                    return;
                }

                // Restart the time-out timer
                _vbdConfirmationTimeOutTimer?.Stop();
                _vbdConfirmationTimeOutTimer?.Start();

                if (!IsVbdServiceDialogVisible)
                {
                    IsVbdServiceDialogVisible = true;
                    UpdateUI();
                }
            }
            else
            {
                _vbdConfirmationTimeOutTimer?.Stop();

                if (IsVbdServiceDialogVisible)
                {
                    IsVbdServiceDialogVisible = false;
                    UpdateUI();
                }
            }
        }

        private bool PlayAdditionalConsecutiveAttractVideo()
        {
            if (!Config.HasAttractIntroVideo || CurrentAttractIndex != 0 || AttractList.Count <= 1)
            {
                _consecutiveAttractCount++;
                Logger.Debug($"Consecutive Attract Video count: {_consecutiveAttractCount}");

                if (_consecutiveAttractCount >= Config.ConsecutiveAttractVideos ||
                    _consecutiveAttractCount >= _gameCount)
                {
                    Logger.Debug("Stopping attract video sequence");
                    return false;
                }

                Logger.Debug("Starting another attract video");
            }

            Task.Run(
                () =>
                {
                    if (AttractList.Count <= 1)
                    {
                        StopAndUnloadAttractVideo();
                    }

                    AdvanceAttractIndex();
                    SetAttractVideos();
                });

            return true;
        }

        private void StopAndUnloadAttractVideo()
        {
            Logger.Debug("StopAndUnloadAttractVideo");
            TopAttractVideoPath = null;
            BottomAttractVideoPath = null;
            IsBottomAttractFeaturePlaying = false;
            IsTopAttractFeaturePlaying = false;
            IsTopperAttractFeaturePlaying = false;
        }

        private void StartAttractTimer()
        {
            if (_attractTimer != null)
            {
                _attractTimer.Stop();

                // When in single game mode, the game is in charge of display attract sequences, not the platform
                if (_lobbyStateManager.AllowSingleGameAutoLaunch)
                {
                    return;
                }

                if (!IsIdleTextScrolling && HasZeroCredits)
                {
                    var interval = _attractMode
                        ? Config.AttractSecondaryTimerIntervalInSeconds
                        : Config.AttractTimerIntervalInSeconds;

                    Logger.Debug($"Starting Attract Timer: {interval} seconds");

                    _attractTimer.Interval = TimeSpan.FromSeconds(interval);
                    _attractTimer.Start();
                }
            }
        }

        private void SetAlternatingTopImageResourceKey()
        {
            TopImageResourceKey = _isShowingAlternateTopImage ? TopImageAlternateResourceKey : TopImageDefaultResourceKey;
        }

        private void SetAlternatingTopperImageResourceKey()
        {
            TopperImageResourceKey = _isShowingAlternateTopperImage ? TopperImageAlternateResourceKey : TopperImageDefaultResourceKey;
        }

        // Note, I am now calling "Attract Mode" the time from the first attract video playing till
        // someone touches the machine.  We don't actually have an attract mode state, but we are
        // setting an internal attract mode flag to facilitate different behavior for the Lobby/Chooser
        // during this time while the machine is running unattended.
        private void RotateTopImageForAttractMode()
        {
            if (Config.RotateTopImageAfterAttractVideo != null && Config.RotateTopImageAfterAttractVideo.Length > 0)
            {
                if (_attractModeTopImageIndex < 0 ||
                    _attractModeTopImageIndex >= Config.RotateTopImageAfterAttractVideo.Length)
                {
                    // just a safety
                    _attractModeTopImageIndex = 0;
                }

                Logger.Debug($"Setting Top Image Index: {_attractModeTopImageIndex} Resource ID: {Config.RotateTopImageAfterAttractVideo[_attractModeTopImageIndex]}");

                TopImageResourceKey = Config.RotateTopImageAfterAttractVideo[_attractModeTopImageIndex];
                _attractModeTopImageIndex++;
                if (_attractModeTopImageIndex >= Config.RotateTopImageAfterAttractVideo.Length)
                {
                    _attractModeTopImageIndex = 0;
                }
            }
        }

        private void RotateTopperImageForAttractMode()
        {
            if (Config.RotateTopperImageAfterAttractVideo != null && Config.RotateTopperImageAfterAttractVideo.Length > 0)
            {
                if (_attractModeTopperImageIndex < 0 ||
                    _attractModeTopperImageIndex >= Config.RotateTopperImageAfterAttractVideo.Length)
                {
                    // just a safety
                    _attractModeTopperImageIndex = 0;
                }

                Logger.Debug($"Setting Topper Image Index: {_attractModeTopperImageIndex} Resource ID: {Config.RotateTopperImageAfterAttractVideo[_attractModeTopperImageIndex]}");

                TopperImageResourceKey = Config.RotateTopperImageAfterAttractVideo[_attractModeTopperImageIndex];
                _attractModeTopperImageIndex++;
                if (_attractModeTopperImageIndex >= Config.RotateTopperImageAfterAttractVideo.Length)
                {
                    _attractModeTopperImageIndex = 0;
                }
            }
        }
        // returns true if we wrap around to index 0
        private bool AdvanceAttractIndex()
        {
            lock (_attractLock)
            {
                CurrentAttractIndex++;
                Logger.Debug($"Advancing Attract Index to {CurrentAttractIndex}");
                return CheckAndResetAttractIndex();
            }
        }

        private bool CheckAndResetAttractIndex()
        {
            lock (_attractLock)
            {
                if (CurrentAttractIndex >= AttractList.Count)
                {
                    ResetAttractIndex();
                    return true;
                }

                return false;
            }
        }

        private void ResetAttractIndex()
        {
            lock (_attractLock)
            {
                CurrentAttractIndex = 0;
            }
        }

        private void UpdateIdleTextSettings()
        {
            LobbyBannerDisplayMode = MeasureIdleText(IdleText).Width <= MaximumBlinkingIdleTextWidth
                ? BannerDisplayMode.Blinking
                : BannerDisplayMode.Scrolling;

            LobbyTrigger trigger = LobbyBannerDisplayMode == BannerDisplayMode.Blinking
                ? LobbyTrigger.SetLobbyIdleTextStatic
                : LobbyTrigger.SetLobbyIdleTextScrolling;

            _lobbyStateManager.SendTrigger(trigger);
        }

        private Size MeasureIdleText(string idleText)
        {
            var formattedText = new FormattedText(
                idleText,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily(IdleTextFamilyName), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
                32,
                Brushes.Black,
                new NumberSubstitution(),
                1);

            return new Size(formattedText.Width, formattedText.Height);
        }

        private void ClearCashOutDialog(bool success)
        {
            _cashOutTimer?.Stop();
            _lobbyStateManager.RemoveFlagState(LobbyState.CashOut, success);
            CashOutDialogState = LobbyCashOutDialogState.Hidden;
            MessageOverlayDisplay.UpdateCashoutButtonState(false);
            MessageOverlayDisplay.LastCashOutForcedByMaxBank = false;
        }

        private void UpdateLcdButtonDeckVideo()
        {
            if (_lobbyButtonDeckRenderer != null)
            {
                if (HasZeroCredits && IsPrimaryLanguageSelected)
                {
                    _lobbyButtonDeckRenderer.VideoFilename = Config.LcdInsertMoneyVideoLanguage1;
                }
                else if (!HasZeroCredits && IsPrimaryLanguageSelected)
                {
                    _lobbyButtonDeckRenderer.VideoFilename = Config.LcdChooseVideoLanguage1;
                }
                else if (HasZeroCredits && !IsPrimaryLanguageSelected)
                {
                    _lobbyButtonDeckRenderer.VideoFilename = Config.LcdInsertMoneyVideoLanguage2;
                }
                else if (!HasZeroCredits && !IsPrimaryLanguageSelected)
                {
                    _lobbyButtonDeckRenderer.VideoFilename = Config.LcdChooseVideoLanguage2;
                }
            }
        }

        private void UpdateLcdButtonDeckRenderSetting(bool renderSetting)
        {
            if (_lobbyButtonDeckRenderer != null)
            {
                _lobbyButtonDeckRenderer.RenderingEnabled = renderSetting;
                Logger.Debug($"LobbyButtonDeckRenderer Render Setting is {renderSetting}");
            }
        }

        private void UpdateLcdButtonDeckDisableSetting(bool disableSetting)
        {
            if (_lobbyButtonDeckRenderer != null)
            {
                _lobbyButtonDeckRenderer.IsSystemDisabled = disableSetting;
                Logger.Debug($"LobbyButtonDeckRenderer Disable Setting is {disableSetting}");
            }
        }

        private bool IsGameRenderingToLcdButtonDeck()
        {
            if (_lobbyStateManager.CurrentState == LobbyState.GameLoadingForDiagnostics ||
                _lobbyStateManager.CurrentState == LobbyState.GameDiagnostics ||
                _lobbyStateManager.CurrentState == LobbyState.GameLoading ||
                _lobbyStateManager.CurrentState == LobbyState.Game)
            {
                Logger.Debug("IsGameRenderingToLcdButtonDeck returns true");
                return true;
            }

            Logger.Debug("IsGameRenderingToLcdButtonDeck returns false");
            return false;
        }

        private void GetVolumeButtonVisible()
        {
            var volumeControlLocation = (VolumeControlLocation)_properties.GetValue(
                ApplicationConstants.VolumeControlLocationKey,
                ApplicationConstants.VolumeControlLocationDefault);

            VolumeButtonVisible = volumeControlLocation == VolumeControlLocation.Lobby ||
                                  volumeControlLocation == VolumeControlLocation.LobbyAndGame;
        }

        private void GetServiceButtonVisible()
        {
            ServiceButtonVisible = _properties.GetValue(GamingConstants.ShowServiceButton, false);
        }

        private void HandleLcdButtonDeckButtonPress(LcdButtonDeckLobby lobbyAction)
        {
            if (!IsTabView)
            {
                return;
            }

            switch (lobbyAction)
            {
                case LcdButtonDeckLobby.PreviousGame:
                    NavigateSelectionTo(SelectionNavigation.PreviousGame);
                    PlayAudioFile(Sound.Touch);
                    break;
                case LcdButtonDeckLobby.PreviousTab:
                    GameTabInfo.NextPreviousTab(false);
                    PlayAudioFile(Sound.Touch);
                    break;
                case LcdButtonDeckLobby.ChangeDenom:
                    GameTabInfo.IncrementSelectedDenomination();
                    PlayAudioFile(Sound.Touch);
                    break;
                case LcdButtonDeckLobby.NextTab:
                    GameTabInfo.NextPreviousTab(true);
                    PlayAudioFile(Sound.Touch);
                    break;
                case LcdButtonDeckLobby.NextGame:
                    NavigateSelectionTo(SelectionNavigation.NextGame);
                    PlayAudioFile(Sound.Touch);
                    break;
                case LcdButtonDeckLobby.CashOut:
                    CashOutPressed(new object());
                    break;
                case LcdButtonDeckLobby.LaunchGame:
                    if (_selectedGame != null)
                    {
                        LaunchGameFromUi(_selectedGame);
                    }
                    break;
            }
        }

        private GameInfo ToGameInfo(IGameDetail game, long denomId)
        {
            return new GameInfo
            {
                GameId = game.Id,
                Name = game.ThemeName,
                InstallDateTime = game.InstallDate,
                DllPath = game.GameDll,
                ImagePath = UseSmallIcons
                    ? game.LocaleGraphics[ActiveLocaleCode].SmallIcon
                    : game.LocaleGraphics[ActiveLocaleCode].LargeIcon,
                TopAttractVideoPath = game.LocaleGraphics[ActiveLocaleCode].TopAttractVideo,
                BottomAttractVideoPath = game.LocaleGraphics[ActiveLocaleCode].BottomAttractVideo,
                LoadingScreenPath = game.LocaleGraphics[ActiveLocaleCode].LoadingScreen,
                HasProgressiveOrBonusValue = !string.IsNullOrEmpty(game.DisplayMeterName),
                ProgressiveOrBonusValue = GetProgressiveOrBonusValue(game.Id, denomId),
                Denomination = denomId,
                BetOption = game.Denominations.Single(d => d.Value == denomId).BetOption,
                FilteredDenomination = Config.MinimumWagerCreditsAsFilter ? game.MinimumWagerCredits * denomId : denomId,
                GameType = GameType.Slot, // TODO: This value is available. Do we need to force it to be "Slot"?
                GameSubtype = game.GameSubtype,
                PlatinumSeries = false,
                Enabled = game.Enabled,
                AttractHighlightVideoPath = !string.IsNullOrEmpty(game.DisplayMeterName)
                    ? Config.AttractVideoWithBonusFilename
                    : Config.AttractVideoNoBonusFilename,
                UseSmallIcons = UseSmallIcons,
                LocaleGraphics = game.LocaleGraphics,
                ThemeId = game.ThemeId,
                IsNew = GameIsNew(game.GameTags),
                RequiresMechanicalReels = game.MechanicalReels > 0
            };
        }

        private void SetTabViewToDefault()
        {
            if (IsTabView)
            {
                var lastGameSelected = _properties.GetValue(GamingConstants.SelectedGameId, 0);

                //Return to the last game selected in the lobby by default
                if (lastGameSelected != 0)
                {
                    var game = GameList.SingleOrDefault(g => g.GameId == lastGameSelected && g.Denomination == LastDenom);
                    if (game != null)
                    {
                        var category = game.Category != GameCategory.Undefined ? game.Category : GameTabInfo.ConvertGameToDefaultCategory(game.GameType);
                        var subcategory = game.SubCategory != GameSubCategory.Undefined ? SubTabInfoViewModel.GetSubTypeText(game.SubCategory) : category == GameCategory.Table ? game.GameType.ToString() : game.GameSubtype;

                        var gameTab = GameTabInfo.Tabs.SingleOrDefault(g => g.Category == category);
                        if (gameTab != null)
                        {
                            GameTabInfo.SelectTab(gameTab.TabIndex);
                            var subTab = GameTabInfo.SubTabs.SingleOrDefault(t => t.TypeText == subcategory);
                            GameTabInfo.SelectSubTab(subTab);
                            var selectedDenomViewInfo = GameTabInfo?.Denominations?.FirstOrDefault(d => d.Denomination == LastDenom);
                            SetSelectedGame(game);
                            if (selectedDenomViewInfo != null)
                            {
                                GameTabInfo.SetSelectedDenomination(selectedDenomViewInfo);
                            }

                            return;
                        }
                    }
                }

                GameTabInfo.SetDefaultTab();
                SelectFirstDisplayedGame();
            }
        }

        private void RequestInfoBarOpen(DisplayRole displayTarget, bool open)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                switch (displayTarget)
                {
                    case DisplayRole.Main:
                        MainInfoBarOpenRequested = open;
                        break;
                    case DisplayRole.VBD:
                        VbdInfoBarOpenRequested = open;
                        break;
                }
            });
        }

        public List<MenuSelectionPayOption> MenuSelectionPayOptions { get; set; }

        public MenuSelectionPayOption SelectedMenuSelectionPayOption
        {
            get => _selectedMenuSelectionPayOption;
            set
            {
                _selectedMenuSelectionPayOption = value;
                switch (_selectedMenuSelectionPayOption)
                {
                    case MenuSelectionPayOption.PayByHand:
                        _eventBus.Publish(new RemoteKeyOffEvent(KeyOffType.LocalHandpay, 0, 0, 0, false));
                        break;
                    case MenuSelectionPayOption.PayToCredit:
                        _eventBus.Publish(new RemoteKeyOffEvent(KeyOffType.LocalCredit, 0, 0, 0, false));
                        break;
                    case MenuSelectionPayOption.ReturnToLockup:
                        _eventBus.Publish(new RemoteKeyOffEvent(KeyOffType.Unknown, 0, 0, 0, false));
                        break;
                }

                RaisePropertyChanged(nameof(SelectedMenuSelectionPayOption));
            }
        }

        private void DenominationSelectionChanged(int gameId, long denomination)
        {
            var selectedGame = DisplayedGameList.FirstOrDefault(game => game.GameId == gameId);
            var selectedDenomViewInfo = GameTabInfo?.Denominations?.FirstOrDefault(d => d.Denomination == denomination);
            if (selectedDenomViewInfo == null || selectedGame == null)
            {
                return;
            }
            SetSelectedGame(selectedGame);
            GameTabInfo.SetSelectedDenomination(selectedDenomViewInfo);
        }

        IList<IPlayerInfoDisplayViewModel> IPlayerInfoDisplayScreensContainer.AvailablePages
        {
            get
            {
                return new IPlayerInfoDisplayViewModel[] { PlayerInfoDisplayMenuViewModel };
            }
        }
    }
}