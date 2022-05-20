namespace Aristocrat.Monaco.Bingo.UI.ViewModels.OperatorMenu
{
    using System;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.UI.OperatorMenu;
    using Common;
    using Common.Storage.Model;
    using Humanizer;
    using Kernel;
    using Localization.Properties;

    public class BingoServerSettingsViewModel : OperatorMenuPageViewModelBase
    {
        private readonly IServerConfigurationProvider _serverConfigurationProvider;
        private readonly string _defaultBingoSetting;

        private string _voucherInLimit;
        private bool _isPlayerMayHideBingoCardSettingVisible;
        private bool _isDisplayBingoCardSettingVisible;
        private string _billAcceptanceLimit;
        private string _ticketReprint;
        private string _captureGameAnalytics;
        private string _alarmConfiguration;
        private string _playerMayHideBingoCard;
        private GameEndWinStrategy _gameEndingPrize;
        private ContinuousPlayMode _playButtonBehavior;
        private string _displayBingoCard;
        private string _bingoCardPlacement;
        private string _maximumVoucherValue;
        private string _minimumJackpotValue;
        private JackpotStrategy _jackpotStrategy;
        private JackpotDetermination _jackpotAmountDetermination;
        private string _printHandpayReceipt;
        private string _legacyBonusAllowed;
        private string _aftBonusingEnabled;
        private CreditsStrategy _creditsStrategy;
        private string _bankId;
        private string _zoneId;
        private string _position;
        private string _lapLevelIDs;
        private BingoType _bingoType;
        private string _serverVersion;
        private string _hideBingoCardWhenInactive;
        private string _waitForPlayersDuration;

        public BingoServerSettingsViewModel()
            : this(ServiceManager.GetInstance().GetService<IBingoDataFactory>())
        {
        }

        public BingoServerSettingsViewModel(IBingoDataFactory factory)
        {
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            _serverConfigurationProvider = factory.GetConfigurationProvider()
                                           ?? throw new NullReferenceException(nameof(_serverConfigurationProvider));

            _defaultBingoSetting = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Pending);
        }

        public string VoucherInLimit
        {
            get => _voucherInLimit;
            private set => SetProperty(ref _voucherInLimit, value);
        }

        public string BillAcceptanceLimit
        {
            get => _billAcceptanceLimit;
            private set => SetProperty(ref _billAcceptanceLimit, value);
        }

        public string TicketReprint
        {
            get => _ticketReprint;
            private set => SetProperty(ref _ticketReprint, value);
        }

        public string CaptureGameAnalytics
        {
            get => _captureGameAnalytics;
            private set => SetProperty(ref _captureGameAnalytics, value);
        }

        public string AlarmConfiguration
        {
            get => _alarmConfiguration;
            private set => SetProperty(ref _alarmConfiguration, value);
        }

        public string PlayerMayHideBingoCard
        {
            get => _playerMayHideBingoCard;
            private set => SetProperty(ref _playerMayHideBingoCard, value);
        }

        public bool IsPlayerMayHideBingoCardSettingVisible
        {
            get => _isPlayerMayHideBingoCardSettingVisible;
            private set => SetProperty(ref _isPlayerMayHideBingoCardSettingVisible, value);
        }

        public bool IsDisplayBingoCardSettingVisible
        {
            get => _isDisplayBingoCardSettingVisible;
            private set => SetProperty(ref _isDisplayBingoCardSettingVisible, value);
        }

        public GameEndWinStrategy GameEndingPrize
        {
            get => _gameEndingPrize;
            private set => SetProperty(ref _gameEndingPrize, value);
        }

        public ContinuousPlayMode PlayButtonBehavior
        {
            get => _playButtonBehavior;
            private set => SetProperty(ref _playButtonBehavior, value);
        }

        public string DisplayBingoCard
        {
            get => _displayBingoCard;
            private set => SetProperty(ref _displayBingoCard, value);
        }

        public string HideBingoCardWhenInactive
        {
            get => _hideBingoCardWhenInactive;
            private set => SetProperty(ref _hideBingoCardWhenInactive, value);
        }

        public string BingoCardPlacement
        {
            get => _bingoCardPlacement;
            private set => SetProperty(ref _bingoCardPlacement, value);
        }

        public string MaximumVoucherValue
        {
            get => _maximumVoucherValue;
            private set => SetProperty(ref _maximumVoucherValue, value);
        }

        public string MinimumJackpotValue
        {
            get => _minimumJackpotValue;
            private set => SetProperty(ref _minimumJackpotValue, value);
        }

        public JackpotStrategy JackpotStrategy
        {
            get => _jackpotStrategy;
            private set => SetProperty(ref _jackpotStrategy, value);
        }

        public JackpotDetermination JackpotAmountDetermination
        {
            get => _jackpotAmountDetermination;
            private set => SetProperty(ref _jackpotAmountDetermination, value);
        }

        public string PrintHandpayReceipt
        {
            get => _printHandpayReceipt;
            private set => SetProperty(ref _printHandpayReceipt, value);
        }

        public string LegacyBonusAllowed
        {
            get => _legacyBonusAllowed;
            private set => SetProperty(ref _legacyBonusAllowed, value);
        }

        public string AftBonusingEnabled
        {
            get => _aftBonusingEnabled;
            private set => SetProperty(ref _aftBonusingEnabled, value);
        }

        public CreditsStrategy CreditsStrategy
        {
            get => _creditsStrategy;
            private set => SetProperty(ref _creditsStrategy, value);
        }

        public string BankId
        {
            get => _bankId;
            private set => SetProperty(ref _bankId, value);
        }

        public string ZoneId
        {
            get => _zoneId;
            private set => SetProperty(ref _zoneId, value);
        }

        public string Position
        {
            get => _position;
            private set => SetProperty(ref _position, value);
        }

        public string LapLevelIDs
        {
            get => _lapLevelIDs;
            private set => SetProperty(ref _lapLevelIDs, value);
        }

        public BingoType BingoType
        {
            get => _bingoType;
            private set => SetProperty(ref _bingoType, value);
        }

        public string WaitForPlayersDuration
        {
            get => _waitForPlayersDuration;
            private set => SetProperty(ref _waitForPlayersDuration, value);
        }

        public string ServerVersion
        {
            get => _serverVersion;
            private set => SetProperty(ref _serverVersion, value);
        }

        protected override void OnLoaded()
        {
            var model = _serverConfigurationProvider.GetServerConfiguration() ?? new BingoServerSettingsModel();

            VoucherInLimit = model.VoucherInLimit?.CentsToDollars().FormattedCurrencyString() ?? _defaultBingoSetting;
            BillAcceptanceLimit = model.BillAcceptanceLimit?.CentsToDollars().FormattedCurrencyString() ??
                                   _defaultBingoSetting;
            TicketReprint = model.TicketReprint?.ToString() ?? _defaultBingoSetting;
            CaptureGameAnalytics = model.CaptureGameAnalytics?.ToString() ?? _defaultBingoSetting;
            AlarmConfiguration = model.AlarmConfiguration?.ToString() ?? _defaultBingoSetting;
            PlayerMayHideBingoCard = model.PlayerMayHideBingoCard ?? _defaultBingoSetting;
            GameEndingPrize = model.GameEndingPrize ?? GameEndWinStrategy.Unknown;
            PlayButtonBehavior = model.ReadySetGo ?? ContinuousPlayMode.Unknown;
            DisplayBingoCard = model.DisplayBingoCard?.ToString() ?? _defaultBingoSetting;
            HideBingoCardWhenInactive = model.HideBingoCardWhenInactive?.ToString() ?? _defaultBingoSetting;
            BingoCardPlacement = model.BingoCardPlacement ?? _defaultBingoSetting;
            MaximumVoucherValue = model.MaximumVoucherValue?.CentsToDollars().FormattedCurrencyString() ??
                                   _defaultBingoSetting;
            MinimumJackpotValue = model.MinimumJackpotValue?.CentsToDollars().FormattedCurrencyString() ??
                                   _defaultBingoSetting;
            JackpotStrategy = model.JackpotStrategy;
            JackpotAmountDetermination = model.JackpotAmountDetermination;
            PrintHandpayReceipt = model.PrintHandpayReceipt?.ToString() ?? _defaultBingoSetting;
            LegacyBonusAllowed = model.LegacyBonusAllowed ?? _defaultBingoSetting;
            AftBonusingEnabled = model.AftBonusingEnabled?.ToString() ?? _defaultBingoSetting;
            CreditsStrategy = model.CreditsStrategy ?? CreditsStrategy.Unknown;
            BankId = model.BankId ?? _defaultBingoSetting;
            ZoneId = model.ZoneId ?? _defaultBingoSetting;
            Position = model.Position ?? _defaultBingoSetting;
            LapLevelIDs = model.LapLevelIDs ?? _defaultBingoSetting;
            BingoType = model.BingoType;
            WaitForPlayersDuration = model.WaitingForPlayersMs?.Milliseconds().Humanize() ?? _defaultBingoSetting;

            var bingoServerVersion = PropertiesManager.GetValue(BingoConstants.BingoServerVersion, _defaultBingoSetting);
            ServerVersion = string.IsNullOrEmpty(bingoServerVersion)
                ? _defaultBingoSetting
                : bingoServerVersion;

            var cabinetControlsDisplayElements = PropertiesManager.GetValue(
                ApplicationConstants.CabinetControlsDisplayElements,
                false);
            IsPlayerMayHideBingoCardSettingVisible = cabinetControlsDisplayElements;
            IsDisplayBingoCardSettingVisible = cabinetControlsDisplayElements;
            base.OnLoaded();
        }
    }
}
