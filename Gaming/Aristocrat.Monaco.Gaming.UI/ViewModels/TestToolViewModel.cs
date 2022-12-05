namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using Application.Contracts;
    using Application.Contracts.Media;
    using Cabinet.Contracts;
    using Contracts;
    using Contracts.InfoBar;
    using Contracts.Lobby;
    using Hardware.Contracts.Bell;
    using Hardware.Contracts.CardReader;
    using Hardware.Contracts.Gds;
    using Hardware.Contracts.Gds.NoteAcceptor;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.TowerLight;
    using Kernel;
    using Monaco.UI.Common.Controls;
    using MVVM;
    using MVVM.Command;
    using MVVM.ViewModel;
    using DisabledEvent = Hardware.Contracts.NoteAcceptor.DisabledEvent;
    using EnabledEvent = Hardware.Contracts.NoteAcceptor.EnabledEvent;
#if !(RETAIL)
    using Vgt.Client12.Testing.Tools;
#endif

    /// <summary>
    ///     Defines the TestToolViewModel class
    /// </summary>
    public class TestToolViewModel : BaseEntityViewModel
    {
        // List of supported currencies and their respective eNums can be found here: https://svn.ali.global/WinnersWorldStudio/tools/CSU
        private enum LocationCode : uint
        {
            Usa = 1,
            Philippines = 2,
            Malaysia = 3,
            Bahamas = 4,
            CostaRica = 5,
            Curacao = 6,
            DominicanRep = 7,
            Paraguay = 8,
            Guatemala = 9,
            Honduras = 10,
            Nicaragua = 11,
            Europe = 22,
            SouthAfrica = 30,
            Croatia = 32,
            Poland = 35,
            Sweden = 39,
            SwitzerlandItalian = 40,
            SwitzerlandGerman = 41,
            SwitzerlandFrench = 42,
            Gibraltar = 48,
            Morocco = 50,
            Romania = 51,
            Georgia = 57,
            Ukraine = 70,
            OtherDecimalWithoutSymbol = 81,
            OtherNonDecimalWithoutSymbol = 82
        }

        // Language tags can be found here: https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c
        private readonly IDictionary<LocationCode, CultureInfo> _locationToCultureInfoMap = new Dictionary<LocationCode, CultureInfo>
        {
            {LocationCode.Usa, new CultureInfo("en-US")},
            {LocationCode.Philippines, new CultureInfo("fil-PH")},
            {LocationCode.Malaysia, new CultureInfo("ms-MY")},
            {LocationCode.Bahamas, new CultureInfo("en-BS")},
            {LocationCode.CostaRica, new CultureInfo("es-CR")},
            {LocationCode.Curacao, new CultureInfo("nl-CW")},
            {LocationCode.DominicanRep, new CultureInfo("es-DO")},
            {LocationCode.Paraguay, new CultureInfo("es-PY")},
            {LocationCode.Guatemala, new CultureInfo("es-GT")},
            {LocationCode.Honduras, new CultureInfo("es-HN")},
            {LocationCode.Nicaragua, new CultureInfo("es-NI")},
            {LocationCode.Europe, new CultureInfo("de-DE")},
            {LocationCode.SouthAfrica, new CultureInfo("en-ZA")},
            {LocationCode.Croatia, new CultureInfo("hr-HR")},
            {LocationCode.Poland, new CultureInfo("pl-PL")},
            {LocationCode.Sweden, new CultureInfo("sv-SE")},
            {LocationCode.SwitzerlandItalian, new CultureInfo("it-CH")},
            {LocationCode.SwitzerlandGerman, new CultureInfo("de-CH")},
            {LocationCode.SwitzerlandFrench, new CultureInfo("fr-CH")},
            {LocationCode.Gibraltar, new CultureInfo("en-GI")},
            {LocationCode.Morocco, new CultureInfo("fr-MA")},
            {LocationCode.Romania, new CultureInfo("ro-RO")},
            {LocationCode.Georgia, new CultureInfo("ka-GE")},
            {LocationCode.Ukraine, new CultureInfo("uk-UA")},
            {LocationCode.OtherDecimalWithoutSymbol, new CultureInfo("de-DE")},
            {LocationCode.OtherNonDecimalWithoutSymbol, new CultureInfo("de-DE")},
        };

        private readonly IDictionary<LocationCode, string> _locationToCurrencyDescription =
            new Dictionary<LocationCode, string>
            {
                { LocationCode.Usa, @"US Dollar USD $1,000.00" },
                { LocationCode.Philippines, @"Philippine Piso PHP ₱1,000.00" },
                { LocationCode.Malaysia, @"Malaysian Ringgit MYR RM1,000.00" },
                { LocationCode.Bahamas, @"Bahamian Dollar BSD B$1.000,00" },
                { LocationCode.CostaRica, @"Costa Rican Colón CRC C1.000,00" },
                { LocationCode.Curacao, @"Netherlands Antillean Guilder ANG ƒ1,000.00" },
                { LocationCode.DominicanRep, @"Dominican Peso DOP RD$1.000,00" },
                { LocationCode.Paraguay, @"Paraguayan Guarani PYG G1.000,00" },
                { LocationCode.Guatemala, @"Guatemalan Quetzal GTQ Q1,000.00" },
                { LocationCode.Honduras, @"Honduran Lempira HNL L1,000.00" },
                { LocationCode.Nicaragua, @"Nicaraguan Córdoba NIO C$1,000.00" },
                { LocationCode.Europe, @"Euro EUR €1.000,00" },
                { LocationCode.SouthAfrica, @"South African Rand ZAR R1,000.00" },
                { LocationCode.Croatia, @"Croatian Kuna HRK 1.000,00KN" },
                { LocationCode.Poland, @"Polish Zloty PLN 1.000,00zł" },
                { LocationCode.Sweden, @"Swedish Krona SEK kr1,000.00" },
                { LocationCode.SwitzerlandItalian, @"Swiss Franc CHF FrS1,000.00" },
                { LocationCode.SwitzerlandGerman, @"Swiss Franc CHF CHF1,000.00" },
                { LocationCode.SwitzerlandFrench, @"Swiss Franc CHF SFr1,000.00" },
                { LocationCode.Gibraltar, @"Gibraltar Pound GIP £1,000.00" },
                { LocationCode.Morocco, @"Moroccan Dirham MAD MAD1,000.00" },
                { LocationCode.Romania, @"Romanian Leu RON lei1.000,00" },
                { LocationCode.Georgia, @"Georgian Lari GEL GEL1,000.00" },
                { LocationCode.Ukraine, @"Ukrainian Hryvnia UAH UAH1.000,00" },
                { LocationCode.OtherDecimalWithoutSymbol, @"Euro EUR 1,000.00" },
                { LocationCode.OtherNonDecimalWithoutSymbol, @"Euro EUR 1,000" },
            };

        private const int LobbyPlayTimeDialogTimeoutInSecondsDefault = 60;
        private const int LobbyPlayTimeDialogTimeoutInSecondsManitobaDefault = 5;
        private readonly Guid _infoBarOwnershipKey1 = new Guid("{1C51E4EF-3E4B-45C8-8530-6119FC6EBE95}");
        private readonly Guid _infoBarOwnershipKey2 = new Guid("{5E029486-0894-43AB-9543-7AD5DD19232C}");
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;
        private readonly ResponsibleGamingMode _responsibleGamingMode;
        private readonly LobbyConfiguration _config;
        private readonly DisplayableMessage _displayableMessage;
        private readonly IMessageDisplayHandler _platformMessageBroadcaster;
        private readonly ITowerLight _towerLight;
        private readonly SolidColorBrush _bellBrushRinging = new SolidColorBrush(Colors.White);
        private readonly SolidColorBrush _bellBrushSilent = new SolidColorBrush(Colors.White);

        private SolidColorBrush _bellBrush;
        private string _elapsedTime;
        private string _elapsedTimeSet;
        private string _sessionCount;
        private string _sessionCountSet;
        private bool _isTimeInSeconds;
        private string _playBreak1;
        private string _playBreak1Set;
        private string _playBreak2;
        private string _playBreak2Set;
        private string _playBreak3;
        private string _playBreak3Set;
        private string _playBreak4;
        private string _playBreak4Set;
        private string _responsibleGamingDialogTimeout;
        private string _responsibleGamingDialogTimeoutSet;

        private string _timeLimit1;
        private string _timeLimit1Set;
        private string _timeLimit2;
        private string _timeLimit2Set;
        private string _timeLimit3;
        private string _timeLimit3Set;
        private string _timeLimit4;
        private string _timeLimit4Set;
        private string _timeLimit5;
        private string _timeLimit5Set;
        private bool _timeLimit5Visible;
        private string _timeLimitInterval;
        private string _largeWinLimit;

        private string _addPlatformMessage;
        private string _removePlatformMessage;
        private string _currencySwitchStatusText;

        private bool _isTowerLightOn1;
        private bool _isTowerLightOn2;
        private bool _isTowerLightOn3;
        private bool _isTowerLightOn4;
        private FlashState _towerLightFlashStatus1;
        private FlashState _towerLightFlashStatus2;
        private FlashState _towerLightFlashStatus3;
        private FlashState _towerLightFlashStatus4;
        private bool _isAuditMenuWindowSelected;
        private bool _isLobbyWindowSelected;
        private bool _isRgDialogSelected;

        private string _infoBarMessage;
        private string _selectedInfoBarFontColor;
        private string _selectedCountry;
        private string _selectedInfoBarBackgroundColor;
        private string _selectedInfoBarRegion;
        private string _selectedInfoBarLocation;

        private TrackData _selectedMagneticCard;
        private string _track1Data;
        private string _eNumValue;
        private string _cardStatusText = "Card Removed";
        private bool _noteAcceptorEnabled;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TestToolViewModel" /> class.
        /// </summary>
        public TestToolViewModel()
        {
            _properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            _config = (LobbyConfiguration)_properties.GetProperty(GamingConstants.LobbyConfig, null);
            _responsibleGamingMode = _config.ResponsibleGamingMode;

            var containerService = ServiceManager.GetInstance().TryGetService<IContainerService>();
            if (null != containerService)
            {
                _platformMessageBroadcaster = containerService.Container.GetInstance<IMessageDisplayHandler>();
            }

            _towerLight = ServiceManager.GetInstance().TryGetService<ITowerLight>();

            TimeLimit5Visible = _responsibleGamingMode == ResponsibleGamingMode.Continuous;

            InsertBillCommand = new ActionCommand<object>(InsertBill);
            InsertVoucherCommand = new ActionCommand<object>(InsertVoucher);
            CashOutCommand = new ActionCommand<object>(CashOut);
            SetTimeLimitsCommand = new ActionCommand<object>(SetTimeLimits, CanSetTimeLimits);
            CreateTimeIntervalCommand = new ActionCommand<object>(CreateTimeInterval, CanCreateTimeInterval);
            SetElapsedTimeCommand = new ActionCommand<object>(SetElapsedTime, CanSetElapsedTime);
            SetSessionCountCommand = new ActionCommand<object>(SetSessionCount, CanSetSessionCount);
            SetResponsibleGamingDialogTimeoutCommand = new ActionCommand<object>(
                SetResponsibleGamingDialogTimeout,
                CanSetResponsibleGamingDialogTimeout);
            FullClearCommand = new ActionCommand<object>(FullClear);
            PartialClearCommand = new ActionCommand<object>(PartialClear);
            ResetDefaultsCommand = new ActionCommand<object>(ResetDefaults);
            TogglePlayerCommand = new ActionCommand<string>(x => TogglePlayer(int.Parse(x)));

            AddPlatformMessageCommand = new ActionCommand<object>(AddPlatformMessage, CanUpdatePlatformMessage);
            RemovePlatformMessageCommand = new ActionCommand<object>(RemovePlatformMessage, CanUpdatePlatformMessage);
            ClearAllPlatformMessagesCommand = new ActionCommand<object>(ClearAllPlatformMessages, CanUpdatePlatformMessage);


            SetLargeWinLimitCommand = new ActionCommand<object>(OverrideLargeWinLimit);

            SetTowerLightFlashStateCommand = new ActionCommand<object>(SetTowerLightFlashState);

            InitTowerLightComboBoxes();

            IsAuditMenuWindowSelected = true;

            // InfoBar Tab
            DisplayInfoBarMessageCommand = new ActionCommand<object>(_ => DisplayInfoBarMessage());
            DisplayInfoBarDoubleMessageCommand = new ActionCommand<object>(_ => DisplayInfoBarDoubleMessage());
            DisplayInfoBarStaticMessageCommand = new ActionCommand<object>(_ => DisplayInfoBarStaticMessage());
            CloseInfoBarCommand = new ActionCommand<object>(_ => CloseInfoBar());
            SelectedInfoBarFontColor = InfoBarColor.White.ToString();
            SelectedInfoBarBackgroundColor = InfoBarColor.Black.ToString();
            SelectedInfoBarRegion = InfoBarRegion.Center.ToString();
            SelectedInfoBarLocation = DisplayRole.Main.ToString();
            InfoBarMessage = "This is a test message ABC 123...";

            // Card Reader
            InsertCardCommand = new ActionCommand<object>(_ => InsertCard());
            RemoveCardCommand = new ActionCommand<object>(_ => RemoveCard());

            // Currency
            CurrencySwitchUsingCountryCommand = new ActionCommand<object>(_ => CurrencySwitchUsingCountry());
            CurrencySwitchUsingEnumCommand = new ActionCommand<object>(_ => CurrencySwitchUsingEnum());

            SetDefaults();

            _displayableMessage = new DisplayableMessage(
                () => string.Empty,
                DisplayableMessageClassification.SoftError,
                DisplayableMessagePriority.Immediate);

            NoteAcceptorEnabled = ServiceManager.GetInstance().TryGetService<INoteAcceptor>()?.Enabled ?? false;
            _eventBus.Subscribe<EnabledEvent>(this, _ => NoteAcceptorEnabled = true);
            _eventBus.Subscribe<DisabledEvent>(this, _ => NoteAcceptorEnabled = false);
            _eventBus.Subscribe<TowerLightOffEvent>(this, evt => HandleTowerLightEvent(evt.LightTier, false, evt.FlashState));
            _eventBus.Subscribe<TowerLightOnEvent>(this, evt => HandleTowerLightEvent(evt.LightTier, true, evt.FlashState));
            _eventBus.Subscribe<PrintFakeTicketEvent>(this, HandlePrintFakeTicketEvent);

            var bell = ServiceManager.GetInstance().TryGetService<IBell>();
            if (bell != null)
            {
                var animation = new ColorAnimationUsingKeyFrames { RepeatBehavior = RepeatBehavior.Forever };
                animation.KeyFrames.Add(new SplineColorKeyFrame(Colors.Orange, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(80))));
                animation.KeyFrames.Add(new SplineColorKeyFrame(Colors.White, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(160))));
                _bellBrushRinging.BeginAnimation(SolidColorBrush.ColorProperty, animation);

                _eventBus.Subscribe<RingStartedEvent>(this, _ => BellColor = _bellBrushRinging);
                _eventBus.Subscribe<RingStoppedEvent>(this, _ => BellColor = _bellBrushSilent);
            }
        }

        public SolidColorBrush BellColor
        {
            get => _bellBrush;
            set
            {
                _bellBrush = value;
                RaisePropertyChanged(nameof(BellColor));
            }
        }

        private void DisplayInfoBarMessage()
        {
            var fontColor = (InfoBarColor)Enum.Parse(typeof(InfoBarColor), SelectedInfoBarFontColor);
            var backgroundColor = (InfoBarColor)Enum.Parse(typeof(InfoBarColor), SelectedInfoBarBackgroundColor);
            var region = (InfoBarRegion)Enum.Parse(typeof(InfoBarRegion), SelectedInfoBarRegion);
            var location = (DisplayRole)Enum.Parse(typeof(DisplayRole), SelectedInfoBarLocation);

            var infoBarEvent = new InfoBarDisplayTransientMessageEvent(
                _infoBarOwnershipKey1,
                InfoBarMessage,
                fontColor,
                backgroundColor,
                region,
                location);

            _eventBus.Publish(infoBarEvent);
        }

        private void DisplayInfoBarDoubleMessage()
        {
            var fontColor = (InfoBarColor)Enum.Parse(typeof(InfoBarColor), SelectedInfoBarFontColor);
            var backgroundColor = (InfoBarColor)Enum.Parse(typeof(InfoBarColor), SelectedInfoBarBackgroundColor);
            var region = (InfoBarRegion)Enum.Parse(typeof(InfoBarRegion), SelectedInfoBarRegion);
            var location = (DisplayRole)Enum.Parse(typeof(DisplayRole), SelectedInfoBarLocation);

            var infoBarEvent1 = new InfoBarDisplayTransientMessageEvent(
                _infoBarOwnershipKey1,
                InfoBarMessage,
                TimeSpan.FromSeconds(10),
                fontColor,
                backgroundColor,
                region,
                location);

            var infoBarEvent2 = new InfoBarDisplayTransientMessageEvent(
                _infoBarOwnershipKey2,
                "This is a 2nd message sent in parallel, lasting 2 sec longer.",
                TimeSpan.FromSeconds(12),
                fontColor,
                backgroundColor,
                region,
                location);

            _eventBus.Publish(infoBarEvent1);
            _eventBus.Publish(infoBarEvent2);
        }

        private void DisplayInfoBarStaticMessage()
        {
            var fontColor = (InfoBarColor)Enum.Parse(typeof(InfoBarColor), SelectedInfoBarFontColor);
            var backgroundColor = (InfoBarColor)Enum.Parse(typeof(InfoBarColor), SelectedInfoBarBackgroundColor);
            var region = (InfoBarRegion)Enum.Parse(typeof(InfoBarRegion), SelectedInfoBarRegion);
            var location = (DisplayRole)Enum.Parse(typeof(DisplayRole), SelectedInfoBarLocation);

            var infoBarEvent1 = new InfoBarDisplayStaticMessageEvent(
                _infoBarOwnershipKey1,
                InfoBarMessage,
                fontColor,
                backgroundColor,
                region,
                location);

            _eventBus.Publish(infoBarEvent1);
        }

        private void CloseInfoBar()
        {
            var location = (DisplayRole)Enum.Parse(typeof(DisplayRole), SelectedInfoBarLocation);
            _eventBus.Publish(typeof(InfoBarCloseEvent), new InfoBarCloseEvent(location));
        }

        private void InsertCard()
        {
            var trackData = new TrackData { Track1 = Track1Data };
            _eventBus.Publish(new FakeCardReaderEvent(0, trackData.Track1, true));
            CardStatusText = $"{Track1Data} Inserted";
        }

        private void RemoveCard()
        {
            _eventBus.Publish(new FakeCardReaderEvent(0, string.Empty, false));
            CardStatusText = "Card Removed";
        }

        /// <summary>
        ///     Gets the insert credits command
        /// </summary>
        public ICommand InsertBillCommand { get; }

        /// <summary>
        ///     Voucher barcode to insert
        /// </summary>
        public string VoucherBarcode { get; set; }

        /// <summary>
        ///     Get the command to insert a random barcode voucher
        /// </summary>
        public ICommand InsertVoucherCommand { get; }

        /// <summary>
        ///     Gets the Cash out command
        /// </summary>
        public ICommand CashOutCommand { get; }

        public ActionCommand<object> SetLargeWinLimitCommand { get; }

        /// <summary>
        ///     Gets the set time limits command
        /// </summary>
        public ActionCommand<object> SetTimeLimitsCommand { get; }

        public ActionCommand<object> CreateTimeIntervalCommand { get; }

        public ActionCommand<object> SetElapsedTimeCommand { get; }

        public ActionCommand<object> SetSessionCountCommand { get; }

        public ActionCommand<object> SetResponsibleGamingDialogTimeoutCommand { get; }

        public ICommand DisplayInfoBarMessageCommand { get; }

        public ICommand CurrencySwitchUsingCountryCommand { get; }

        public ICommand CurrencySwitchUsingEnumCommand { get; }

        public ICommand DisplayInfoBarDoubleMessageCommand { get; }

        public ICommand DisplayInfoBarStaticMessageCommand { get; }

        public ICommand CloseInfoBarCommand { get; set; }

        public ICommand FullClearCommand { get; set; }

        public ICommand PartialClearCommand { get; set; }

        public ICommand ResetDefaultsCommand { get; set; }

        public ICommand TogglePlayerCommand { get; set; }

        public ActionCommand<object> AddPlatformMessageCommand { get; }

        public ActionCommand<object> RemovePlatformMessageCommand { get; }

        public ICommand ClearAllPlatformMessagesCommand { get; }

        public ICommand SetTowerLightFlashStateCommand { get; }

        public ICommand InsertCardCommand { get; }

        public ICommand RemoveCardCommand { get; }

        public string LargeWinLimit
        {
            get => _largeWinLimit;

            set
            {
                if (_largeWinLimit != value)
                {
                    _largeWinLimit = value;
                    RaisePropertyChanged((nameof(LargeWinLimit)));
                }
            }
        }

        public string TimeLimit1
        {
            get => _timeLimit1;

            set
            {
                if (_timeLimit1 != value)
                {
                    _timeLimit1 = value;
                    RaisePropertyChanged(nameof(TimeLimit1));
                    SetTimeLimitsCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string TimeLimit2
        {
            get => _timeLimit2;

            set
            {
                if (_timeLimit2 != value)
                {
                    _timeLimit2 = value;
                    RaisePropertyChanged(nameof(TimeLimit2));
                    SetTimeLimitsCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string TimeLimit3
        {
            get => _timeLimit3;

            set
            {
                if (_timeLimit3 != value)
                {
                    _timeLimit3 = value;
                    RaisePropertyChanged(nameof(TimeLimit3));
                    SetTimeLimitsCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string TimeLimit4
        {
            get => _timeLimit4;

            set
            {
                if (_timeLimit4 != value)
                {
                    _timeLimit4 = value;
                    RaisePropertyChanged(nameof(TimeLimit4));
                    SetTimeLimitsCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string TimeLimit5
        {
            get => _timeLimit5;

            set
            {
                if (_timeLimit5 != value)
                {
                    _timeLimit5 = value;
                    RaisePropertyChanged(nameof(TimeLimit5));
                    SetTimeLimitsCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string TimeLimit1Set
        {
            get => _timeLimit1Set;

            set
            {
                if (_timeLimit1Set != value)
                {
                    _timeLimit1Set = value;
                    RaisePropertyChanged(nameof(TimeLimit1Set));
                }
            }
        }

        public string TimeLimit2Set
        {
            get => _timeLimit2Set;

            set
            {
                if (_timeLimit2Set != value)
                {
                    _timeLimit2Set = value;
                    RaisePropertyChanged(nameof(TimeLimit2Set));
                }
            }
        }

        public string TimeLimit3Set
        {
            get => _timeLimit3Set;

            set
            {
                if (_timeLimit3Set != value)
                {
                    _timeLimit3Set = value;
                    RaisePropertyChanged(nameof(TimeLimit3Set));
                }
            }
        }

        public string TimeLimit4Set
        {
            get => _timeLimit4Set;

            set
            {
                if (_timeLimit4Set != value)
                {
                    _timeLimit4Set = value;
                    RaisePropertyChanged(nameof(TimeLimit4Set));
                }
            }
        }

        public string TimeLimit5Set
        {
            get => _timeLimit5Set;

            set
            {
                if (_timeLimit5Set != value)
                {
                    _timeLimit5Set = value;
                    RaisePropertyChanged(nameof(TimeLimit5Set));
                }
            }
        }

        public string PlayBreak1
        {
            get => _playBreak1;

            set
            {
                if (_playBreak1 != value)
                {
                    _playBreak1 = value;
                    RaisePropertyChanged(nameof(PlayBreak1));
                    SetTimeLimitsCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string PlayBreak2
        {
            get => _playBreak2;

            set
            {
                if (_playBreak2 != value)
                {
                    _playBreak2 = value;
                    RaisePropertyChanged(nameof(PlayBreak2));
                    SetTimeLimitsCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string PlayBreak3
        {
            get => _playBreak3;

            set
            {
                if (_playBreak3 != value)
                {
                    _playBreak3 = value;
                    RaisePropertyChanged(nameof(PlayBreak3));
                    SetTimeLimitsCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string PlayBreak4
        {
            get => _playBreak4;

            set
            {
                if (_playBreak4 != value)
                {
                    _playBreak4 = value;
                    RaisePropertyChanged(nameof(PlayBreak4));
                    SetTimeLimitsCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string PlayBreak1Set
        {
            get => _playBreak1Set;

            set
            {
                if (_playBreak1Set != value)
                {
                    _playBreak1Set = value;
                    RaisePropertyChanged(nameof(PlayBreak1Set));
                }
            }
        }

        public string PlayBreak2Set
        {
            get => _playBreak2Set;

            set
            {
                if (_playBreak2Set != value)
                {
                    _playBreak2Set = value;
                    RaisePropertyChanged(nameof(PlayBreak2Set));
                }
            }
        }

        public string PlayBreak3Set
        {
            get => _playBreak3Set;

            set
            {
                if (_playBreak3Set != value)
                {
                    _playBreak3Set = value;
                    RaisePropertyChanged(nameof(PlayBreak3Set));
                }
            }
        }

        public string PlayBreak4Set
        {
            get => _playBreak4Set;

            set
            {
                if (_playBreak4Set != value)
                {
                    _playBreak4Set = value;
                    RaisePropertyChanged(nameof(PlayBreak4Set));
                }
            }
        }

        public string TimeLimitInterval
        {
            get => _timeLimitInterval;

            set
            {
                if (_timeLimitInterval != value)
                {
                    _timeLimitInterval = value;
                    RaisePropertyChanged(nameof(TimeLimitInterval));
                    CreateTimeIntervalCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool TimeLimit5Visible
        {
            get => _timeLimit5Visible;

            set
            {
                if (_timeLimit5Visible != value)
                {
                    _timeLimit5Visible = value;
                    RaisePropertyChanged(nameof(TimeLimit5Visible));
                }
            }
        }

        public bool IsTimeInSeconds
        {
            get => _isTimeInSeconds;

            set
            {
                if (_isTimeInSeconds != value)
                {
                    _isTimeInSeconds = value;
                    RaisePropertyChanged(nameof(IsTimeInSeconds));
                    RecalculateTimes(!value);
                }
            }
        }

        public string ElapsedTime
        {
            get => _elapsedTime;

            set
            {
                if (_elapsedTime != value)
                {
                    _elapsedTime = value;
                    RaisePropertyChanged(nameof(ElapsedTime));
                    SetElapsedTimeCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string ElapsedTimeSet
        {
            get => _elapsedTimeSet;

            set
            {
                if (_elapsedTimeSet != value)
                {
                    _elapsedTimeSet = value;
                    RaisePropertyChanged(nameof(ElapsedTimeSet));
                }
            }
        }

        public string SessionCount
        {
            get => _sessionCount;

            set
            {
                if (_sessionCount != value)
                {
                    _sessionCount = value;
                    RaisePropertyChanged(nameof(SessionCount));
                    SetSessionCountCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string SessionCountSet
        {
            get => _sessionCountSet;

            set
            {
                if (_sessionCountSet != value)
                {
                    _sessionCountSet = value;
                    RaisePropertyChanged(nameof(SessionCountSet));
                }
            }
        }

        public string ResponsibleGamingDialogTimeout
        {
            get => _responsibleGamingDialogTimeout;

            set
            {
                if (_responsibleGamingDialogTimeout != value)
                {
                    _responsibleGamingDialogTimeout = value;
                    RaisePropertyChanged(nameof(ResponsibleGamingDialogTimeout));
                    SetResponsibleGamingDialogTimeoutCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string ResponsibleGamingDialogTimeoutSet
        {
            get => _responsibleGamingDialogTimeoutSet;

            set
            {
                if (_responsibleGamingDialogTimeoutSet != value)
                {
                    _responsibleGamingDialogTimeoutSet = value;
                    RaisePropertyChanged(nameof(ResponsibleGamingDialogTimeoutSet));
                }
            }
        }

        public string AddPlatformMessageText
        {
            get => (string.IsNullOrEmpty(_addPlatformMessage)) ? null : _addPlatformMessage;

            set
            {
                if (_addPlatformMessage != value)
                {
                    _addPlatformMessage = value;
                    RaisePropertyChanged(nameof(AddPlatformMessageText));
                    AddPlatformMessageCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string RemovePlatformMessageText
        {
            get => (string.IsNullOrEmpty(_removePlatformMessage)) ? null : _removePlatformMessage;

            set
            {
                if (_removePlatformMessage != value)
                {
                    _removePlatformMessage = value;
                    RaisePropertyChanged(nameof(RemovePlatformMessageText));
                    RemovePlatformMessageCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string CurrencySwitchStatusText
        {
            get => (string.IsNullOrEmpty(_currencySwitchStatusText)) ? "N/A" : _currencySwitchStatusText;

            set
            {
                if (_currencySwitchStatusText != value)
                {
                    _currencySwitchStatusText = value;
                    RaisePropertyChanged(nameof(CurrencySwitchStatusText));
                }
            }
        }

        public List<string> TowerLightTierList { get; set; }

        public List<string> TowerLightFlashStatusList { get; set; }

        public int TowerLightTierSelectedIndex { get; set; }

        public int TowerLightFlashStatusSelectedIndex { get; set; }

        public bool IsTowerLightOn1
        {
            get => _isTowerLightOn1;

            set
            {
                if (_isTowerLightOn1 != value)
                {
                    _isTowerLightOn1 = value;
                    RaisePropertyChanged(nameof(IsTowerLightOn1));
                }
            }
        }

        public bool IsTowerLightOn2
        {
            get => _isTowerLightOn2;

            set
            {
                if (_isTowerLightOn2 != value)
                {
                    _isTowerLightOn2 = value;
                    RaisePropertyChanged(nameof(IsTowerLightOn2));
                }
            }
        }

        public bool IsTowerLightOn3
        {
            get => _isTowerLightOn3;

            set
            {
                if (_isTowerLightOn3 != value)
                {
                    _isTowerLightOn3 = value;
                    RaisePropertyChanged(nameof(IsTowerLightOn3));
                }
            }
        }

        public bool IsTowerLightOn4
        {
            get => _isTowerLightOn4;

            set
            {
                if (_isTowerLightOn4 != value)
                {
                    _isTowerLightOn4 = value;
                    RaisePropertyChanged(nameof(IsTowerLightOn4));
                }
            }
        }

        public FlashState TowerLightFlashStatus1
        {
            get => _towerLightFlashStatus1;

            set
            {
                if (_towerLightFlashStatus1 != value)
                {
                    _towerLightFlashStatus1 = value;
                    RaisePropertyChanged(nameof(TowerLightFlashStatus1));
                }
            }
        }

        public FlashState TowerLightFlashStatus2
        {
            get => _towerLightFlashStatus2;

            set
            {
                if (_towerLightFlashStatus2 != value)
                {
                    _towerLightFlashStatus2 = value;
                    RaisePropertyChanged(nameof(TowerLightFlashStatus2));
                }
            }
        }

        public FlashState TowerLightFlashStatus3
        {
            get => _towerLightFlashStatus3;

            set
            {
                if (_towerLightFlashStatus3 != value)
                {
                    _towerLightFlashStatus3 = value;
                    RaisePropertyChanged(nameof(TowerLightFlashStatus3));
                }
            }
        }

        public FlashState TowerLightFlashStatus4
        {
            get => _towerLightFlashStatus4;

            set
            {
                if (_towerLightFlashStatus4 != value)
                {
                    _towerLightFlashStatus4 = value;
                    RaisePropertyChanged(nameof(TowerLightFlashStatus4));
                }
            }
        }

        public string InfoBarMessage
        {
            get => _infoBarMessage;

            set
            {
                if (_infoBarMessage == value)
                {
                    return;
                }

                _infoBarMessage = value;
                RaisePropertyChanged(nameof(InfoBarMessage));
            }
        }

        public string[] InfoBarLocations => Enum.GetNames(typeof(DisplayRole));

        public string[] InfoBarColors => Enum.GetNames(typeof(InfoBarColor));

        public string[] InfoBarRegions => Enum.GetNames(typeof(InfoBarRegion));

        public string[] CountryNames => Enum.GetNames(typeof(LocationCode));

        public string SelectedCountry
        {
            get => _selectedCountry;

            set
            {
                if (_selectedCountry == value)
                {
                    return;
                }

                _selectedCountry = value;
                RaisePropertyChanged(nameof(SelectedCountry));
            }
        }

        public string EnumValue
        {
            get => _eNumValue;

            set
            {
                if (_eNumValue == value)
                {
                    return;
                }

                _eNumValue = value;
                RaisePropertyChanged(nameof(EnumValue));
            }
        }

        public string SelectedInfoBarFontColor
        {
            get => _selectedInfoBarFontColor;

            set
            {
                if (_selectedInfoBarFontColor == value)
                {
                    return;
                }

                _selectedInfoBarFontColor = value;
                RaisePropertyChanged(nameof(SelectedInfoBarFontColor));
            }
        }

        public string SelectedInfoBarBackgroundColor
        {
            get => _selectedInfoBarBackgroundColor;

            set
            {
                if (_selectedInfoBarBackgroundColor == value)
                {
                    return;
                }

                _selectedInfoBarBackgroundColor = value;
                RaisePropertyChanged(nameof(SelectedInfoBarBackgroundColor));
            }
        }

        public string SelectedInfoBarRegion
        {
            get => _selectedInfoBarRegion;

            set
            {
                if (_selectedInfoBarRegion == value)
                {
                    return;
                }

                _selectedInfoBarRegion = value;
                RaisePropertyChanged(nameof(SelectedInfoBarRegion));
            }
        }

        public string SelectedInfoBarLocation
        {
            get => _selectedInfoBarLocation;

            set
            {
                if (_selectedInfoBarLocation == value)
                {
                    return;
                }

                _selectedInfoBarLocation = value;
                RaisePropertyChanged(nameof(SelectedInfoBarRegion));
            }
        }

        /// <summary>
        ///     Gets a value indicating whether note acceptor is enabled.
        /// </summary>
        public bool NoteAcceptorEnabled
        {
            get => _noteAcceptorEnabled;
            private set
            {
                if (_noteAcceptorEnabled == value)
                {
                    return;
                }

                _noteAcceptorEnabled = value;
                RaisePropertyChanged(nameof(NoteAcceptorEnabled));
            }
        }

        public bool PlayBreaksVisible => _responsibleGamingMode == ResponsibleGamingMode.Segmented;

        public bool SessionCountVisible => _responsibleGamingMode == ResponsibleGamingMode.Segmented;

        public bool IsAuditMenuWindowSelected
        {
            get => _isAuditMenuWindowSelected;
            set
            {
                if (_isAuditMenuWindowSelected != value)
                {
                    _isAuditMenuWindowSelected = value;
                    RaisePropertyChanged(nameof(IsAuditMenuWindowSelected));
                }
            }
        }

        public bool IsLobbyWindowSelected
        {
            get => _isLobbyWindowSelected;
            set
            {
                if (_isLobbyWindowSelected != value)
                {
                    _isLobbyWindowSelected = value;
                    RaisePropertyChanged(nameof(IsLobbyWindowSelected));
                }
            }
        }

        public bool IsRgDialogSelected
        {
            get => _isRgDialogSelected;
            set
            {
                if (_isRgDialogSelected != value)
                {
                    _isRgDialogSelected = value;
                    RaisePropertyChanged(nameof(IsRgDialogSelected));
                }
            }
        }

        public Dictionary<string, TrackData> MagneticCards { get; } = new Dictionary<string, TrackData>
        {
            { "NYL Technician Employee 123", new TrackData {Track1 = "EMP123"} },
            { "NYL Operator Employee 321", new TrackData {Track1 = "EMP321"} },
            { "NYL Player 456", new TrackData {Track1 = "PLY456"} },
            { "NYL Player 654", new TrackData {Track1 = "PLY654"} }
        };

        public TrackData SelectedMagneticCard
        {
            get => _selectedMagneticCard;

            set
            {
                if (_selectedMagneticCard == value)
                {
                    return;
                }

                _selectedMagneticCard = value;
                RaisePropertyChanged(nameof(SelectedMagneticCard));
                Track1Data = SelectedMagneticCard.Track1;
            }
        }

        public string Track1Data
        {
            get => _track1Data;

            set
            {
                if (_track1Data == value)
                {
                    return;
                }

                _track1Data = value;
                RaisePropertyChanged(nameof(Track1Data));
            }
        }

        public string CardStatusText
        {
            get => _cardStatusText;
            set
            {
                _cardStatusText = value;
                RaisePropertyChanged(nameof(CardStatusText));
            }
        }

        private void OverrideLargeWinLimit(object parameter)
        {
            if (LargeWinLimit != null && !string.IsNullOrEmpty(LargeWinLimit))
            {
                var limit = ParseLongText(LargeWinLimit);
                if (limit != null)
                {
                    _properties.SetProperty("Cabinet.LargeWinLimit", limit);
                }
            }
        }

        private void InsertBill(object parameter)
        {
#if !(RETAIL)
            var currency = int.Parse((string)parameter);
            _eventBus.Publish(new DebugNoteEvent(currency));
#endif
        }

#if !(RETAIL)
        private byte _bnaTicketTransactionId = 0;
#endif
        private void InsertVoucher(object parameter)
        {
#if !(RETAIL)
            var barcode = VoucherBarcode;
            if (string.IsNullOrWhiteSpace(barcode))
            {
                barcode = DateTime.UtcNow.Ticks.ToString();
                barcode = barcode.Substring(barcode.Length - 18);
            }

            try
            {
                _bnaTicketTransactionId++;
            }
            catch (OverflowException)
            {
                _bnaTicketTransactionId = 0;
            }

            _eventBus.Publish(new FakeDeviceMessageEvent
            {
                Message = new TicketValidated
                {
                    ReportId = GdsConstants.ReportId.NoteAcceptorAcceptNoteOrTicket,
                    TransactionId = _bnaTicketTransactionId,
                    Code = barcode
                }
            });
#endif
        }

        private void CashOut(object parameter)
        {
            // not implemented
        }

        private void FullClear(object parameter)
        {
            var storage = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
            storage.Clear(PersistenceLevel.Static);
        }

        private void PartialClear(object parameter)
        {
            var storage = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
            storage.Clear(PersistenceLevel.Critical);
        }

        private bool CanSetTimeLimits(object parameter)
        {
            var timeLimits = ParseTimeLimits();
            var playBreaks = new List<double>();
            if (timeLimits.Count > 0)
            {
                playBreaks = ParsePlayBreaks(timeLimits);
            }

            return timeLimits.Count > 0 && playBreaks.Count > 0;
        }

        private void SetTimeLimits(object parameter)
        {
            var timeLimitsList = ParseTimeLimits();
            if (_responsibleGamingMode != ResponsibleGamingMode.Continuous)
            {
                timeLimitsList.RemoveAt(4);
            }

            var playBreaksList = ParsePlayBreaks(timeLimitsList);

            if (IsTimeInSeconds)
            {
                for (var i = 0; i < timeLimitsList.Count; i++)
                {
                    timeLimitsList[i] = timeLimitsList[i] / 60.0;
                }

                for (var i = 0; i < playBreaksList.Count; i++)
                {
                    playBreaksList[i] = playBreaksList[i] / 60.0;
                }
            }

            var timeLimits = timeLimitsList.ToArray();
            var playBreaks = playBreaksList.ToArray();
            _properties.SetProperty(LobbyConstants.RGTimeLimitsInMinutes, timeLimits);
            _properties.SetProperty(LobbyConstants.RGPlayBreaksInMinutes, playBreaks);
            TimeLimit1Set = TimeLimit1;
            TimeLimit2Set = TimeLimit2;
            TimeLimit3Set = TimeLimit3;
            TimeLimit4Set = TimeLimit4;
            TimeLimit5Set = TimeLimit5;
            PlayBreak1Set = PlayBreak1;
            PlayBreak2Set = PlayBreak2;
            PlayBreak3Set = PlayBreak3;
            PlayBreak4Set = PlayBreak4;
        }

        private bool CanCreateTimeInterval(object parameter)
        {
            return ParseDoubleText(TimeLimitInterval).HasValue;
        }

        private void CreateTimeInterval(object parameter)
        {
            var interval = ParseDoubleText(TimeLimitInterval);
            if (interval != null)
            {
                TimeLimit1 = interval.ToString();
                TimeLimit2 = (interval * 2).ToString();
                TimeLimit3 = (interval * 3).ToString();
                TimeLimit4 = (interval * 4).ToString();
                TimeLimit5 = (interval * 5).ToString();
            }
        }

        private bool CanSetElapsedTime(object parameter)
        {
            return ParseDoubleText(ElapsedTime, true).HasValue;
        }

        private bool CanUpdatePlatformMessage(object parameter)
        {
            return true;
        }

        private void SetElapsedTime(object parameter)
        {
            var elapsedTimeInSeconds = 0;

            var elapsedTime = ParseDoubleText(ElapsedTime, true);
            if (elapsedTime != null)
            {
                if (IsTimeInSeconds)
                {
                    elapsedTimeInSeconds = (int)Math.Round(elapsedTime.Value, MidpointRounding.AwayFromZero);
                }
                else
                {
                    elapsedTimeInSeconds = (int)Math.Round(elapsedTime.Value * 60, MidpointRounding.AwayFromZero);
                }
            }

            _properties.SetProperty(LobbyConstants.LobbyPlayTimeElapsedInSecondsOverride, elapsedTimeInSeconds);
            ElapsedTimeSet = ElapsedTime;
        }

        private bool CanSetSessionCount(object parameter)
        {
            if (Int32.TryParse(SessionCount, out int sessionCount))
            {
                return sessionCount > 0 && sessionCount <= 2;
            }

            return false;
        }

        private void SetSessionCount(object parameter)
        {
            if (Int32.TryParse(SessionCount, out int sessionCount))
            {
                _properties.SetProperty(LobbyConstants.LobbyPlayTimeSessionCountOverride, sessionCount);
                SessionCountSet = SessionCount;
            }
        }

        private bool CanSetResponsibleGamingDialogTimeout(object parameter)
        {
            return ParseDoubleText(ResponsibleGamingDialogTimeout).HasValue;
        }

        private void SetResponsibleGamingDialogTimeout(object parameter)
        {
            var dialogTimeoutInSeconds = 0;

            var dialogTimeout = ParseDoubleText(ResponsibleGamingDialogTimeout);
            if (dialogTimeout != null)
            {
                if (IsTimeInSeconds)
                {
                    dialogTimeoutInSeconds = (int)Math.Round(dialogTimeout.Value, MidpointRounding.AwayFromZero);
                }
                else
                {
                    dialogTimeoutInSeconds = (int)Math.Round(dialogTimeout.Value * 60, MidpointRounding.AwayFromZero);
                }
            }

            _properties.SetProperty(LobbyConstants.LobbyPlayTimeDialogTimeoutInSeconds, dialogTimeoutInSeconds);
            ResponsibleGamingDialogTimeoutSet = ResponsibleGamingDialogTimeout;
        }

        private void AddPlatformMessage(object parameter)
        {
            if (_platformMessageBroadcaster != null && AddPlatformMessageText != null)
            {
                _displayableMessage.MessageCallback = () => AddPlatformMessageText;
                _platformMessageBroadcaster.DisplayMessage(_displayableMessage);
            }
        }

        private void RemovePlatformMessage(object parameter)
        {
            if (_platformMessageBroadcaster != null && RemovePlatformMessageText != null)
            {
                _platformMessageBroadcaster.RemoveMessage(_displayableMessage);
            }
        }

        private void ClearAllPlatformMessages(object parameter)
        {
            _platformMessageBroadcaster?.ClearMessages();
        }

        private void SetDefaults()
        {
            IsTimeInSeconds = true;
            TimeLimitInterval = "15";
            CreateTimeInterval(new object());
            PlayBreak1 = "0";
            PlayBreak2 = "0";
            PlayBreak3 = "30";
            PlayBreak4 = "30";

            TowerLightTierSelectedIndex = 0;
            TowerLightFlashStatusSelectedIndex = 0;
            IsTowerLightOn1 = false;
            IsTowerLightOn2 = false;
            IsTowerLightOn3 = false;
            IsTowerLightOn4 = false;
            TowerLightFlashStatus1 = FlashState.Off;
            TowerLightFlashStatus2 = FlashState.Off;
            TowerLightFlashStatus3 = FlashState.Off;
            TowerLightFlashStatus4 = FlashState.Off;
        }

        private void ResetDefaults(object parameter)
        {
            var timeLimits = new double[_config.ResponsibleGamingTimeLimits.Length];
            var playBreaks = new double[_config.ResponsibleGamingPlayBreaks.Length];
            //have to deep copy since I'm going to alter it to seconds (possibly) later in function.
            //don't want to alter value in _config.
            _config.ResponsibleGamingTimeLimits.CopyTo(timeLimits, 0);
            _config.ResponsibleGamingPlayBreaks.CopyTo(playBreaks, 0);
            _properties.SetProperty(LobbyConstants.RGTimeLimitsInMinutes, _config.ResponsibleGamingTimeLimits);
            _properties.SetProperty(LobbyConstants.RGPlayBreaksInMinutes, _config.ResponsibleGamingPlayBreaks);
            var dialogTimeoutDefault = _responsibleGamingMode == ResponsibleGamingMode.Continuous
                ? LobbyPlayTimeDialogTimeoutInSecondsDefault
                : LobbyPlayTimeDialogTimeoutInSecondsManitobaDefault;
            _properties.SetProperty(
                LobbyConstants.LobbyPlayTimeDialogTimeoutInSeconds,
                dialogTimeoutDefault);
            _properties.SetProperty(LobbyConstants.LobbyPlayTimeElapsedInSecondsOverride, null);
            double timeout = dialogTimeoutDefault;

            if (IsTimeInSeconds)
            {
                for (var i = 0; i < timeLimits.Length; i++)
                {
                    timeLimits[i] *= 60;
                }

                for (var i = 0; i < playBreaks.Length; i++)
                {
                    playBreaks[i] *= 60;
                }
            }
            else
            {
                timeout = timeout / 60.0;
            }

            TimeLimit1Set = timeLimits[0].ToString(CultureInfo.InvariantCulture);
            TimeLimit2Set = timeLimits[1].ToString(CultureInfo.InvariantCulture);
            TimeLimit3Set = timeLimits[2].ToString(CultureInfo.InvariantCulture);
            TimeLimit4Set = timeLimits[3].ToString(CultureInfo.InvariantCulture);
            if (timeLimits.Length == 5)
            {
                TimeLimit5Set = timeLimits[4].ToString(CultureInfo.InvariantCulture);
            }

            PlayBreak1Set = playBreaks[0].ToString(CultureInfo.InvariantCulture);
            PlayBreak2Set = playBreaks[1].ToString(CultureInfo.InvariantCulture);
            PlayBreak3Set = playBreaks[2].ToString(CultureInfo.InvariantCulture);
            PlayBreak4Set = playBreaks[3].ToString(CultureInfo.InvariantCulture);

            ResponsibleGamingDialogTimeoutSet = timeout.ToString(CultureInfo.InvariantCulture);
            ElapsedTimeSet = 0.ToString();
        }

        private void TogglePlayer(int id)
        {
            if (id == 0)
            {
                _eventBus.Publish(new ToggleMediaPlayerTestEvent(1));
                _eventBus.Publish(new ToggleMediaPlayerTestEvent(2));
            }
            else
            {
                _eventBus.Publish(new ToggleMediaPlayerTestEvent(id));
            }
        }

        private List<double> ParseTimeLimits()
        {
            var timeLimits = new List<double>();
            if ((ParseDoubleText(TimeLimit1, timeLimits) &&
                 ParseDoubleText(TimeLimit2, timeLimits) &&
                 ParseDoubleText(TimeLimit3, timeLimits) &&
                 ParseDoubleText(TimeLimit4, timeLimits) &&
                 ParseDoubleText(TimeLimit5, timeLimits)) &&
                (timeLimits[0] < timeLimits[1] && timeLimits[1] < timeLimits[2] && timeLimits[2] < timeLimits[3]))
            {
                var parsed = true;
                if (_responsibleGamingMode == ResponsibleGamingMode.Continuous &&
                    timeLimits[3] >= timeLimits[4])
                {
                    parsed = false;
                }

                if (parsed)
                {
                    return timeLimits;
                }
            }

            return new List<double>();
        }

        private List<double> ParsePlayBreaks(List<double> timeLimits)
        {
            if (_responsibleGamingMode == ResponsibleGamingMode.Continuous)
            {
                return new List<double> { 0.0, 0.0, 0.0, 0.0 };
            }

            var playBreaks = new List<double>();
            if (ParseDoubleText(PlayBreak1, playBreaks, true) &&
                ParseDoubleText(PlayBreak2, playBreaks, true) &&
                ParseDoubleText(PlayBreak3, playBreaks, true) &&
                ParseDoubleText(PlayBreak4, playBreaks, true) &&
                (playBreaks[0] < timeLimits[0] && playBreaks[1] < timeLimits[1] &&
                 playBreaks[2] < timeLimits[2] && playBreaks[3] < timeLimits[3]))
            {
                return playBreaks;
            }

            return new List<double>();
        }

        private long? ParseLongText(string text, bool allowZero = false)
        {
            long? limit = null;

            if (long.TryParse(text, out var tempLimit) &&
                (tempLimit > 0 || allowZero && tempLimit == 0))
            {
                limit = tempLimit;
            }

            return limit;
        }

        private double? ParseDoubleText(string text, bool allowZero = false)
        {
            double? timeInterval = null;
            if (double.TryParse(text, out var tempInterval) &&
                (tempInterval > 0 || allowZero && tempInterval.Equals(0)))
            {
                timeInterval = tempInterval;
            }

            return timeInterval;
        }

        private bool ParseDoubleText(string text, List<double> list, bool allowZero = false)
        {
            if (double.TryParse(text, out double value) &&
                (value > 0 || allowZero))
            {
                list.Add(value);
                return true;
            }

            return false;
        }

        private void RecalculateTimes(bool timeInMinutes)
        {
            TimeLimit1 = RecalculateTextTimes(TimeLimit1, timeInMinutes);
            TimeLimit2 = RecalculateTextTimes(TimeLimit2, timeInMinutes);
            TimeLimit3 = RecalculateTextTimes(TimeLimit3, timeInMinutes);
            TimeLimit4 = RecalculateTextTimes(TimeLimit4, timeInMinutes);
            TimeLimit5 = RecalculateTextTimes(TimeLimit5, timeInMinutes);
            TimeLimitInterval = RecalculateTextTimes(TimeLimitInterval, timeInMinutes);
            ElapsedTime = RecalculateTextTimes(ElapsedTime, timeInMinutes, true);
            ResponsibleGamingDialogTimeout = RecalculateTextTimes(ResponsibleGamingDialogTimeout, timeInMinutes);
            TimeLimit1Set = RecalculateTextTimes(TimeLimit1Set, timeInMinutes);
            TimeLimit2Set = RecalculateTextTimes(TimeLimit2Set, timeInMinutes);
            TimeLimit3Set = RecalculateTextTimes(TimeLimit3Set, timeInMinutes);
            TimeLimit4Set = RecalculateTextTimes(TimeLimit4Set, timeInMinutes);
            TimeLimit5Set = RecalculateTextTimes(TimeLimit5Set, timeInMinutes);
            ElapsedTimeSet = RecalculateTextTimes(ElapsedTimeSet, timeInMinutes, true);
            ResponsibleGamingDialogTimeoutSet = RecalculateTextTimes(ResponsibleGamingDialogTimeoutSet, timeInMinutes);
            PlayBreak1 = RecalculateTextTimes(PlayBreak1, timeInMinutes);
            PlayBreak2 = RecalculateTextTimes(PlayBreak2, timeInMinutes);
            PlayBreak3 = RecalculateTextTimes(PlayBreak3, timeInMinutes);
            PlayBreak4 = RecalculateTextTimes(PlayBreak4, timeInMinutes);
            PlayBreak1Set = RecalculateTextTimes(PlayBreak1Set, timeInMinutes);
            PlayBreak2Set = RecalculateTextTimes(PlayBreak2Set, timeInMinutes);
            PlayBreak3Set = RecalculateTextTimes(PlayBreak3Set, timeInMinutes);
            PlayBreak4Set = RecalculateTextTimes(PlayBreak4Set, timeInMinutes);
        }

        private string RecalculateTextTimes(string text, bool changeToMinutes, bool allowZero = false)
        {
            var value = ParseDoubleText(text, allowZero);
            if (value.HasValue)
            {
                if (changeToMinutes)
                {
                    value = value.Value / 60.0;
                }
                else
                {
                    value *= 60;
                }

                text = value.ToString();
            }

            return text;
        }

        private void InitTowerLightComboBoxes()
        {
            if (_towerLight != null)
            {
                TowerLightTierList = Enum.GetNames(typeof(LightTier)).ToList();
                TowerLightFlashStatusList = Enum.GetNames(typeof(FlashState)).ToList();
            }
        }

        private void SetTowerLightFlashState(object parameter)
        {
            if (_towerLight != null)
            {
                var lightTier = (LightTier)TowerLightTierSelectedIndex;
                var flashState = (FlashState)TowerLightFlashStatusSelectedIndex;
                _towerLight.SetFlashState(lightTier, flashState, Timeout.InfiniteTimeSpan);
            }
        }

        private void HandleTowerLightEvent(LightTier lightTier, bool lightOn, FlashState flashState)
        {
            if (lightTier == LightTier.Tier1)
            {
                IsTowerLightOn1 = lightOn;
                TowerLightFlashStatus1 = flashState;
            }
            else if (lightTier == LightTier.Tier2)
            {
                IsTowerLightOn2 = lightOn;
                TowerLightFlashStatus2 = flashState;
            }
            else if (lightTier == LightTier.Tier3)
            {
                IsTowerLightOn3 = lightOn;
                TowerLightFlashStatus3 = flashState;
            }
            else if (lightTier == LightTier.Tier4)
            {
                IsTowerLightOn4 = lightOn;
                TowerLightFlashStatus4 = flashState;
            }
        }

        private void CurrencySwitchUsingCountry()
        {
            Enum.TryParse(SelectedCountry, out LocationCode eNum);
            CurrencySwitch(eNum);
        }

        private void CurrencySwitchUsingEnum()
        {
            Enum.TryParse(EnumValue, out LocationCode eNum);
            CurrencySwitch(eNum);
        }

        private void CurrencySwitch(LocationCode eNum)
        {
            if (!Enum.IsDefined(typeof(LocationCode), eNum))
            {
                CurrencySwitchStatusText = "Given eNum was invalid.";
                return;
            }
            var cultureInfo = _locationToCultureInfoMap[eNum];
            var currencyDescription = _locationToCurrencyDescription[eNum]; // Used to deduce the correct overload from Unregulated Class III Currencies XML.
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            propertiesManager.SetProperty(ApplicationConstants.CurrencyDescription, currencyDescription);
            var region = new RegionInfo(cultureInfo.Name);
            propertiesManager.SetProperty(ApplicationConstants.CurrencyId, region.ISOCurrencySymbol);
            CurrencySwitchStatusText =
                $@"Switched to {currencyDescription}.";
            // CurrencyCultureProvider->Initialize() will be triggered.
        }

        private void HandlePrintFakeTicketEvent(PrintFakeTicketEvent evt)
        {
#if !(RETAIL)
            // Display message box with print data -- this can only come from the Fake printer
            if (_properties.GetValue("DisplayFakePrinterTickets", "false") == "true")
            {
                MvvmHelper.ExecuteOnUI(() =>
                {
                    if (evt.TicketText == null)
                    {
                        return;
                    }

                    var text = evt.TicketText.Replace("<D>", "|");
                    text = text.Replace("</D>", "");
                    text = text.Replace("-----", "-");
                    var ticketSections = text.Split('|');
                    new ScrollableMessageBox(ticketSections, "Print Ticket").ShowDialog();
                });
            }
#endif
        }
    }
}
