namespace Aristocrat.Monaco.Gaming.UI.Settings
{
    using System.Collections.ObjectModel;
    using Aristocrat.Monaco.Application.Contracts;
    using Contracts;
    using Hardware.Contracts.Audio;
    using MVVM.Model;

    /// <summary>
    ///     Gaming settings.
    /// </summary>
    internal class GamingSettings : BaseNotify
    {
        private string _jurisdiction;
        private bool _autoPlayAllowed;
        private bool _continuousPlayModeConfigurable;
        private bool _autoHoldConfigurable;
        private VolumeControlLocation _volumeControlLocation;
        private bool _showServiceButton;
        private VolumeScalar _relativeVolume;
        private bool _reelStopEnabled;
        private bool _freeSpinClearWinMeter;
        private string _winDestination;
        private double _reelSpeed;
        private bool _displayGamePayMessageUse;
        private string _displayGamePayMessageFormat;
        private long _wagerLimitsMaxTotalWager;
        private bool _wagerLimitsUse;
        private string _maximumGameRoundWinResetWinAmount;
        private bool _volumeLevelShowInHelpScreen;
        private bool _serviceUse;
        private bool _clockUseHInDisplay;
        private bool _kenoFreeGamesSelectionChange;
        private bool _kenoFreeGamesAutoPlay;
        private bool _initialZeroWagerUse;
        private bool _changeLineSelectionAtZeroCreditUse;
        private bool _gameDurationUseMarketGameTime;
        private bool _gameLogEnabled;
        private bool _gameLogOutcomeDetails;
        private bool _buttonAnimationGoodLuck;
        private GameStartMethodOption _gameStartMethod;
        private bool _allowZeroCreditCashout;

        private CensorshipSettings _censorship;
        private SlotSettings _slot;
        private KenoSettings _keno;
        private PokerSettings _poker;
        private BlackjackSettings _blackjack;
        private RouletteSettings _roulette;
        private GameAttractSettings _attractSettings;
        private ProgressiveLobbyIndicator _progressiveIndicator;

        public bool ButtonAnimationGoodLuck
        {
            get => _buttonAnimationGoodLuck;

            set => SetProperty(ref _buttonAnimationGoodLuck, value);
        }

        public bool AudioAudioChannels
        {
            get => _gameLogOutcomeDetails;

            set => SetProperty(ref _gameLogOutcomeDetails, value);
        }

        public bool GameLogOutcomeDetails
        {
            get => _gameLogOutcomeDetails;

            set => SetProperty(ref _gameLogOutcomeDetails, value);
        }

        public bool GameLogEnabled
        {
            get => _gameLogEnabled;

            set => SetProperty(ref _gameLogEnabled, value);
        }

        public bool GameDurationUseMarketGameTime
        {
            get => _gameDurationUseMarketGameTime;

            set => SetProperty(ref _gameDurationUseMarketGameTime, value);
        }

        public bool ChangeLineSelectionAtZeroCreditUse
        {
            get => _changeLineSelectionAtZeroCreditUse;

            set => SetProperty(ref _changeLineSelectionAtZeroCreditUse, value);
        }

        public bool InitialZeroWagerUse
        {
            get => _initialZeroWagerUse;

            set => SetProperty(ref _initialZeroWagerUse, value);
        }

        public bool KenoFreeGamesAutoPlay
        {
            get => _kenoFreeGamesAutoPlay;

            set => SetProperty(ref _kenoFreeGamesAutoPlay, value);
        }

        public bool KenoFreeGamesSelectionChange
        {
            get => _kenoFreeGamesSelectionChange;

            set => SetProperty(ref _kenoFreeGamesSelectionChange, value);
        }

        public bool ClockUseHInDisplay
        {
            get => _clockUseHInDisplay;

            set => SetProperty(ref _clockUseHInDisplay, value);
        }

        public bool ServiceUse
        {
            get => _serviceUse;

            set => SetProperty(ref _serviceUse, value);
        }

        public bool VolumeLevelShowInHelpScreen
        {
            get => _volumeLevelShowInHelpScreen;

            set => SetProperty(ref _volumeLevelShowInHelpScreen, value);
        }

        public string MaximumGameRoundWinResetWinAmount
        {
            get => _maximumGameRoundWinResetWinAmount;

            set => SetProperty(ref _maximumGameRoundWinResetWinAmount, value);
        }

        public bool WagerLimitsUse
        {
            get => _wagerLimitsUse;

            set => SetProperty(ref _wagerLimitsUse, value);
        }

        public long WagerLimitsMaxTotalWager
        {
            get => _wagerLimitsMaxTotalWager;

            set => SetProperty(ref _wagerLimitsMaxTotalWager, value);
        }

        public string WinDestination
        {
            get => _winDestination;

            set => SetProperty(ref _winDestination, value);
        }

        public bool FreeSpinClearWinMeter
        {
            get => _freeSpinClearWinMeter;

            set => SetProperty(ref _freeSpinClearWinMeter, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether auto play is allowed.
        /// </summary>
        public bool AutoPlayAllowed
        {
            get => _autoPlayAllowed;

            set => SetProperty(ref _autoPlayAllowed, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether continuous play mode is configurable.
        /// </summary>
        public bool ContinuousPlayModeConfigurable
        {
            get => _continuousPlayModeConfigurable;

            set => SetProperty(ref _continuousPlayModeConfigurable, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether auto hold is configurable.
        /// </summary>
        public bool AutoHoldConfigurable
        {
            get => _autoHoldConfigurable;

            set => SetProperty(ref _autoHoldConfigurable, value);
        }

        /// <summary>
        ///     Gets or sets the progressive lobby indicator type
        /// </summary>
        public ProgressiveLobbyIndicator ProgressiveIndicator
        {
            get => _progressiveIndicator;
            set => SetProperty(ref _progressiveIndicator, value);
        }

        /// <summary>
        ///     Gets or sets the value of jurisdiction
        /// </summary>
        public string Jurisdiction
        {
            get => _jurisdiction;

            set => SetProperty(ref _jurisdiction, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether to show the player volume.
        /// </summary>
        public VolumeControlLocation VolumeControlLocation
        {
            get => _volumeControlLocation;

            set => SetProperty(ref _volumeControlLocation, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether to show the service button.
        /// </summary>
        public bool ShowServiceButton
        {
            get => _showServiceButton;

            set => SetProperty(ref _showServiceButton, value);
        }

        /// <summary>
        ///     Gets or sets the relative volume.
        /// </summary>
        public VolumeScalar RelativeVolume
        {
            get => _relativeVolume;

            set => SetProperty(ref _relativeVolume, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether the reel stop is enabled.
        /// </summary>
        public bool ReelStopEnabled
        {
            get => _reelStopEnabled;

            set => SetProperty(ref _reelStopEnabled, value);
        }

        public double ReelSpeed
        {
            get => _reelSpeed;

            set => SetProperty(ref _reelSpeed, value);
        }

        public bool DisplayGamePayMessageUse
        {
            get => _displayGamePayMessageUse;

            set => SetProperty(ref _displayGamePayMessageUse, value);
        }

        public string DisplayGamePayMessageFormat
        {
            get => _displayGamePayMessageFormat;

            set => SetProperty(ref _displayGamePayMessageFormat, value);
        }

        /// <summary>
        ///     Gets or sets the game start method.
        /// </summary>
        public GameStartMethodOption GameStartMethod
        {
            get => _gameStartMethod;

            set => SetProperty(ref _gameStartMethod, value);
        }

        /// <summary>
        ///     Gets or sets a collection of games.
        /// </summary>
        public ObservableCollection<GameSettings> Games { get; set; }

        /// <summary>
        ///     Gets or sets the the censorship settings.
        /// </summary>
        public CensorshipSettings Censorship
        {
            get => _censorship;

            set => SetProperty(ref _censorship, value);
        }

        /// <summary>
        ///     Gets or sets the slot settings.
        /// </summary>
        public SlotSettings Slot
        {
            get => _slot;

            set => SetProperty(ref _slot, value);
        }

        /// <summary>
        ///     Gets or sets the keno settings.
        /// </summary>
        public KenoSettings Keno
        {
            get => _keno;

            set => SetProperty(ref _keno, value);
        }

        /// <summary>
        ///     Gets or sets the poker settings.
        /// </summary>
        public PokerSettings Poker
        {
            get => _poker;

            set => SetProperty(ref _poker, value);
        }

        /// <summary>
        ///     Gets or sets the blackjack settings.
        /// </summary>
        public BlackjackSettings Blackjack
        {
            get => _blackjack;

            set => SetProperty(ref _blackjack, value);
        }

        /// <summary>
        ///     Gets or sets the Roulette settings.
        /// </summary>
        public RouletteSettings Roulette
        {
            get => _roulette;

            set => SetProperty(ref _roulette, value);
        }

        /// <summary>
        ///     Gets or sets the attract settings.
        /// </summary>
        public GameAttractSettings AttractSettings
        {
            get => _attractSettings;

            set => SetProperty(ref _attractSettings, value);
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether zero credit cashout is allowed.
        /// </summary>
        public bool AllowZeroCreditCashout
        {
            get => _allowZeroCreditCashout;

            set => SetProperty(ref _allowZeroCreditCashout, value);
        }
    }
}