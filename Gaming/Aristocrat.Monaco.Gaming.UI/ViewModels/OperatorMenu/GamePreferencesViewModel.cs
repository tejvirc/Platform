namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows.Input;

    using Application.Contracts;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.OperatorMenu;
    using Utils;
    using Contracts;
    using Contracts.Lobby;
    using Contracts.Models;
    using Contracts.Progressives;
    using Events;
    using Hardware.Contracts.Audio;
    using Kernel;
    using Localization.Properties;
    using Monaco.UI.Common.Extensions;
    using MVVM.Command;
    using Progressives;
    using Views.OperatorMenu;

    public class GamePreferencesViewModel : OperatorMenuPageViewModelBase
    {
        private const string BackgroundPreviewFilesRelativePath = @"../Jurisdiction/DefaultAssets/ui/Images/CardTableBackgroundPreviews/";
        private const string AutoHoldField = "AutoHoldField";

        private readonly IGameCategoryService _gameCategory;
        private readonly GameType _currentSelectedGameType;
        private readonly IDialogService _dialogService;
        private readonly IAudio _audio;
        private readonly ILocalization _localization;
        private readonly IPlayerCultureProvider _playerCultureProvider;
        private readonly IGameProvider _gameProvider;

        private bool _censorViolentContent;
        private bool _censorDrugUseContent;
        private bool _censorSexualContent;
        private bool _censorOffensiveContent;
        private bool _enableVideo;
        private bool _enableAudio;
        private bool _enableLighting;
        private VolumeControlLocation _volumeControlLocation;
        private bool _showServiceButton;
        private bool _showTopPickBanners;
        private bool _showTopPickBannersVisible;
        private VolumeScalar _lobbyVolumeScalar;
        private bool _slotEnableAutoPlay;
        private bool _isButtonContinuousPlayChecked;
        private bool _isDoubleTapForceReelStopChecked;
        private VolumeScalar _slotVolumeScalar;
        private bool _kenoEnableAutoPlay;
        private bool _kenoShowPlayerSpeedButton;
        private int _kenoSpeedLevel;
        private int _kenoDefaultSpeedLevel;
        private VolumeScalar _kenoVolumeScalar;
        private bool _pokerEnableAutoHold;
        private bool _pokerShowPlayerSpeedButton;
        private int _pokerSpeedLevel;
        private int _pokerDefaultSpeedLevel;
        private VolumeScalar _pokerVolumeScalar;
        private string _pokerBackgroundColor;
        private VolumeScalar _blackjackVolumeScalar;
        private VolumeScalar _rouletteVolumeScalar;
        private GameStartMethodOption _gameStartMethod;
        private bool _showProgramPinRequired;
        private string _showProgramPin;
        private bool _showProgramEnableResetCredits;
        private bool _attractOptionsEnabled;
        private bool _attractEnabled;
        private bool _logicDoorEnabled;
        private OperatorMenuAccessRestriction _logicDoorAccessRestriction;

        private string _dingSoundFilePath;
        private string _slotSoundFilePath;
        private string _kenoSoundFilePath;
        private string _cardSoundFilePath;
        private ProgressiveLobbyIndicator _progressiveIndicator;

        public GamePreferencesViewModel()
        {
            if (!InDesigner)
            {
                _dialogService = ServiceManager.GetInstance().GetService<IDialogService>();
            }

            var containerService = ServiceManager.GetInstance().TryGetService<IContainerService>();
            var lobbyStateManager = containerService.Container
                ?.GetInstance<ILobbyStateManager>();
            IsShowProgramPinConfigurable = lobbyStateManager?.BaseState != LobbyState.Game;

            _localization = ServiceManager.GetInstance().GetService<ILocalization>();
            _playerCultureProvider = _localization.GetProvider(CultureFor.Player) as
                                         IPlayerCultureProvider ??
                                     throw new ArgumentNullException(nameof(_playerCultureProvider));
            _gameProvider = containerService.Container?.GetInstance<IGameProvider>() ??
                            throw new ArgumentNullException(nameof(_gameProvider));

            var progressiveConfiguration = ServiceManager.GetInstance().GetService<IProgressiveConfigurationProvider>();
            _gameCategory = ServiceManager.GetInstance().GetService<IGameCategoryService>();
            var selectedGameId = PropertiesManager.GetValue(GamingConstants.SelectedGameId, -1);
            _audio = ServiceManager.GetInstance().GetService<IAudio>();

            _currentSelectedGameType = _gameProvider.GetGame(selectedGameId)?.GameType ?? GameType.Undefined;

            var autoPlayAllowed =
                PropertiesManager.GetValue(GamingConstants.AutoPlayAllowed, true); //jurisdiction allow auto play or not
            var playerSpeedButtonEnabled =
                PropertiesManager.GetValue(GamingConstants.ShowPlayerSpeedButtonEnabled, true);

            SlotOptionsEnabled = _gameProvider.GetAllGames().Any(a => a.GameType == GameType.Slot) &&
                                 PropertiesManager.GetValue(GamingConstants.ReelStopConfigurable, true);
            SlotAllowedAutoPlay = SlotOptionsEnabled && autoPlayAllowed;

            KenoOptionsEnabled = _gameProvider.GetAllGames().Any(a => a.GameType == GameType.Keno);
            KenoAllowedAutoPlay = KenoOptionsEnabled && autoPlayAllowed;
            KenoPlayerSpeedButtonEnabled = KenoOptionsEnabled && playerSpeedButtonEnabled;
            var pokerGames = _gameProvider.GetAllGames().Where(a => a.GameType == GameType.Poker).ToList();
            PokerOptionsEnabled = pokerGames.Any();
            LoadPokerBackgroundColors();
            LoadBackgroundPreviewFiles();
            BlackjackOptionsEnabled = _gameProvider.GetAllGames().Any(a => a.GameType == GameType.Blackjack);
            RouletteOptionsEnabled = _gameProvider.GetAllGames().Any(game => game.GameType == GameType.Roulette);
            PokerPlayerSpeedButtonEnabled = PokerOptionsEnabled && playerSpeedButtonEnabled;

            ProgressiveOptionsEnabled = progressiveConfiguration.ViewProgressiveLevels()
                .Any(x => x.LevelType != ProgressiveLevelType.Sap);
            ProgressiveOptionsVisible = PropertiesManager.GetValue(
                GamingConstants.ProgressiveLobbyIndicatorType,
                ProgressiveLobbyIndicator.Disabled) != ProgressiveLobbyIndicator.Disabled;


            IsButtonContinuousPlayConfigurable = PropertiesManager.GetValue(
                GamingConstants.ContinuousPlayModeConfigurable,
                true);

            IsButtonContinuousPlayConfigurable = true;

            EditableCensorship = PropertiesManager.GetValue(GamingConstants.CensorshipEditable, false);
            CensorshipEnforced = PropertiesManager.GetValue(GamingConstants.CensorshipEnforced, false);
            DemoModeEnabled = PropertiesManager.GetValue(ApplicationConstants.ShowMode, false);
            AttractOptionsEnabled = _gameProvider.GetEnabledGames().Any();
            AttractEnabled = PropertiesManager.GetValue(GamingConstants.AttractModeEnabled, true);

            AutoHoldConfigurable = PokerOptionsEnabled &&
                                   PropertiesManager.GetValue(GamingConstants.AutoHoldConfigurable, true);
            ChangeGameLobbyLayoutCommand = new ActionCommand<object>(ChangeGameLobbyLayout_Clicked);
            AttractModeCustomizeCommand = new ActionCommand<object>(AttractModeCustomizeButton_Clicked);
            AttractModePreviewCommand = new ActionCommand<object>(AttractModePreviewButton_Clicked);
            PreviewBackgroundCommand = new ActionCommand<object>(BackgroundPreviewButton_Clicked);

            IsGameStartMethodSettingVisible = PropertiesManager.GetValue(
                GamingConstants.GameStartMethodSettingVisible,
                true);
            IsGameStartMethodConfigurable = PropertiesManager.GetValue(
                GamingConstants.GameStartMethodConfigurable,
                false);
            GameStartMethod = PropertiesManager.GetValue(GamingConstants.GameStartMethod, GameStartMethodOption.Bet);
            ShowProgramPinRequired = PropertiesManager.GetValue(GamingConstants.ShowProgramPinRequired, true);
            ShowProgramPin = PropertiesManager.GetValue(
                GamingConstants.ShowProgramPin,
                GamingConstants.DefaultShowProgramPin);
            ShowProgramEnableResetCredits = PropertiesManager.GetValue(
                GamingConstants.ShowProgramEnableResetCredits,
                true);

            MinimumGameRoundDuration = PropertiesManager.GetValue(GamingConstants.MinimumGameRoundDuration, GamingConstants.DefaultMinimumGameRoundDurationMs);
            MaximumGameRoundDuration = PropertiesManager.GetValue(GamingConstants.MaximumGameRoundDuration, GamingConstants.DefaultMaximumGameRoundDurationMs);

            IsShowProgramPinConfigurable = lobbyStateManager?.BaseState != LobbyState.Game;
        
        	// initialize language options
            LanguageOptions = new ObservableCollection<LanguageOptionViewModel>();
            InitializeLanguageOptions();
        }

        public List<GameStartMethodInfo> GameStartMethods => new List<GameStartMethodInfo>
        {
            new GameStartMethodInfo(
                GameStartMethodOption.Bet,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameStartMethodBetButton)),
            new GameStartMethodInfo(
                GameStartMethodOption.LineOrReel,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameStartMethodLineOrReelButton))
        };

        public IEnumerable<ProgressLobbyIndicatorInfo> ProgressLobbyIndicatorOptions => new List<ProgressLobbyIndicatorInfo>
        {
            new ProgressLobbyIndicatorInfo(
                ProgressiveLobbyIndicator.ProgressiveLabel,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProgressiveLobbyIndicatorLabel)),
            new ProgressLobbyIndicatorInfo(
                ProgressiveLobbyIndicator.ProgressiveValue,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProgressiveLobbyIndicatorValue))
        };

        public IEnumerable<VolumeControlLocationInfo> VolumeControlLocationOptions => new List<VolumeControlLocationInfo>
        {
            new VolumeControlLocationInfo(
                VolumeControlLocation.Lobby,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.VolumeControlLocationLobbyLabel)),
            new VolumeControlLocationInfo(
                VolumeControlLocation.Game,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.VolumeControlLocationGameLabel)),
            new VolumeControlLocationInfo(
                VolumeControlLocation.LobbyAndGame,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.VolumeControlLocationLobbyAndGameLabel))
        };

        public bool SlotOptionsEnabled { get; }

        public bool KenoOptionsEnabled { get; }

        public bool PokerOptionsEnabled { get; }

        public bool BlackjackOptionsEnabled { get; }

        public bool RouletteOptionsEnabled { get; }

        public bool DemoModeEnabled { get; }

        public bool EditableCensorship { get; }

        public bool CensorshipEnforced { get; }

        public bool AutoHoldConfigurable { get; }

        public bool SlotAllowedAutoPlay { get; }

        public bool KenoAllowedAutoPlay { get; }

        public bool ProgressiveOptionsEnabled { get; }

        public bool ProgressiveOptionsVisible { get; }

        public bool KenoPlayerSpeedButtonEnabled { get; }

        public bool PokerPlayerSpeedButtonEnabled { get; }

        public List<int> GameSpeed { get; } = new List<int>();

        public List<int> DefaultPlaySpeed { get; } = new List<int>();

        public List<string> PokerBackgroundColors { get; } = new List<string>();

        public bool IsGameStartMethodConfigurable { get; }

        public bool IsGameStartMethodSettingVisible { get; }

        public bool IsShowProgramPinConfigurable { get; }

        public IList<(string Color, string BackgroundFilePath)> BackgroundPreviewFiles { get; } = new List<(string, string)>();

        public bool BackgroundPreviewAvailable => PokerBackgroundColor != null && BackgroundPreviewFiles.Any(x => string.Equals(x.Color, PokerBackgroundColor, StringComparison.CurrentCultureIgnoreCase));

        public bool EnableVideo
        {
            get => _enableVideo;
            set
            {
                if (_enableVideo == value)
                {
                    return;
                }

                _enableVideo = value;
                RaisePropertyChanged(nameof(EnableVideo));
            }
        }

        public bool EnableAudio
        {
            get => _enableAudio;
            set
            {
                if (_enableAudio == value)
                {
                    return;
                }

                _enableAudio = value;
                RaisePropertyChanged(nameof(EnableAudio));
            }
        }

        public bool EnableLighting
        {
            get => _enableLighting;
            set
            {
                if (_enableLighting == value)
                {
                    return;
                }

                _enableLighting = value;
                RaisePropertyChanged(nameof(EnableLighting));
            }
        }

        public bool AttractOptionsEnabled
        {
            get => _attractOptionsEnabled;
            set
            {
                if (_attractOptionsEnabled == value)
                {
                    return;
                }

                _attractOptionsEnabled = value;
                RaisePropertyChanged(nameof(AttractOptionsEnabled));
            }
        }

        public bool AttractEnabled
        {
            get => _attractEnabled;
            set
            {
                if (_attractEnabled == value)
                {
                    return;
                }

                _attractEnabled = value;
                RaisePropertyChanged(nameof(AttractEnabled));
                Save(GamingConstants.AttractModeEnabled, _attractEnabled);
                EventBus.Publish(new AttractConfigurationChangedEvent());
            }
        }

        public VolumeControlLocation VolumeControlLocation
        {
            get => _volumeControlLocation;
            set
            {
                if (_volumeControlLocation == value)
                {
                    return;
                }

                _volumeControlLocation = value;
                RaisePropertyChanged(nameof(VolumeControlLocation));

                Save(ApplicationConstants.VolumeControlLocationKey, (int)_volumeControlLocation);
                EventBus.Publish(new LobbySettingsChangedEvent(LobbySettingType.VolumeButtonVisible));//TODO
            }
        }

        public bool ShowProgramPinRequired
        {
            get => _showProgramPinRequired;
            set
            {
                if (_showProgramPinRequired == value)
                {
                    return;
                }

                _showProgramPinRequired = value;
                RaisePropertyChanged(nameof(ShowProgramPinRequired));
                Save(GamingConstants.ShowProgramPinRequired, _showProgramPinRequired);
            }
        }

        public string ShowProgramPin
        {
            get => _showProgramPin;
            set
            {
                if (_showProgramPin == value)
                {
                    return;
                }

                _showProgramPin = value;
                RaisePropertyChanged(nameof(ShowProgramPin));
                ValidateShowProgramPin();
            }
        }

        public bool ShowProgramEnableResetCredits
        {
            get => _showProgramEnableResetCredits;
            set
            {
                if (_showProgramEnableResetCredits == value)
                {
                    return;
                }

                _showProgramEnableResetCredits = value;
                RaisePropertyChanged(nameof(ShowProgramEnableResetCredits));
                Save(GamingConstants.ShowProgramEnableResetCredits, _showProgramEnableResetCredits);
            }
        }

        public bool ShowServiceButton
        {
            get => _showServiceButton;
            set
            {
                if (_showServiceButton == value)
                {
                    return;
                }

                _showServiceButton = value;
                RaisePropertyChanged(nameof(ShowServiceButton));

                Save(GamingConstants.ShowServiceButton, _showServiceButton);
                EventBus.Publish(new LobbySettingsChangedEvent(LobbySettingType.ServiceButtonVisible));
            }
        }

        public bool ShowTopPickBanners
        {
            get => _showTopPickBanners;
            set
            {
                if (_showTopPickBanners == value)
                {
                    return;
                }

                _showTopPickBanners = value;
                RaisePropertyChanged(nameof(ShowTopPickBanners));

                Save(GamingConstants.ShowTopPickBanners, _showTopPickBanners);
                EventBus.Publish(new LobbySettingsChangedEvent(LobbySettingType.ShowTopPickBanners));
            }
        }

        public bool ShowTopPickBannersVisible
        {
            get => _showTopPickBannersVisible;
            set
            {
                if (_showTopPickBannersVisible == value)
                {
                    return;
                }

                _showTopPickBannersVisible = value;
                RaisePropertyChanged(nameof(ShowTopPickBannersVisible));
            }
        }

        public VolumeScalar LobbyVolumeScalar
        {
            get => _lobbyVolumeScalar;
            set
            {
                if (_lobbyVolumeScalar == value)
                {
                    return;
                }

                _lobbyVolumeScalar = value;
                RaisePropertyChanged(nameof(LobbyVolumeScalar));

                Save(ApplicationConstants.LobbyVolumeScalarKey, _lobbyVolumeScalar);
                PlayVolumeChangeSound(_dingSoundFilePath, _audio.GetVolumeScalar(value));
            }
        }

        public bool SlotEnableAutoPlay
        {
            get => _slotEnableAutoPlay;
            set
            {
                if (_slotEnableAutoPlay == value)
                {
                    return;
                }

                _slotEnableAutoPlay = value;
                RaisePropertyChanged(nameof(SlotEnableAutoPlay));

                Save(
                    GameType.Slot,
                    a =>
                    {
                        a.AutoPlay = _slotEnableAutoPlay;
                        return a;
                    });
            }
        }

        public VolumeScalar SlotVolumeScalar
        {
            get => _slotVolumeScalar;
            set
            {
                if (_slotVolumeScalar == value)
                {
                    return;
                }

                _slotVolumeScalar = value;
                RaisePropertyChanged(nameof(SlotVolumeScalar));

                Save(
                    GameType.Slot,
                    a =>
                    {
                        a.VolumeScalar = _slotVolumeScalar;
                        return a;
                    });
                PlayVolumeChangeSound(_slotSoundFilePath, _audio.GetVolumeScalar(value));
            }
        }

        public bool IsButtonContinuousPlayChecked
        {
            get => _isButtonContinuousPlayChecked;
            set
            {
                if (_isButtonContinuousPlayChecked == value || !IsButtonContinuousPlayConfigurable)
                {
                    return;
                }

                _isButtonContinuousPlayChecked = value;
                RaisePropertyChanged(nameof(IsButtonContinuousPlayChecked));

                var mode = _isButtonContinuousPlayChecked
                    ? PlayMode.Continuous
                    : PlayMode.Toggle;

                Save(GamingConstants.ContinuousPlayMode, mode);
            }
        }

        public bool IsButtonContinuousPlayConfigurable { get; }

        public bool IsDoubleTapForceReelStopChecked
        {
            get => _isDoubleTapForceReelStopChecked;
            set
            {
                if (_isDoubleTapForceReelStopChecked == value)
                {
                    return;
                }

                _isDoubleTapForceReelStopChecked = value;
                RaisePropertyChanged(nameof(IsDoubleTapForceReelStopChecked));

                Save(GamingConstants.ReelStopEnabled, _isDoubleTapForceReelStopChecked);
            }
        }

        public bool KenoEnableAutoPlay
        {
            get => _kenoEnableAutoPlay;
            set
            {
                if (_kenoEnableAutoPlay == value)
                {
                    return;
                }

                _kenoEnableAutoPlay = value;
                RaisePropertyChanged(nameof(KenoEnableAutoPlay));

                Save(
                    GameType.Keno,
                    a =>
                    {
                        a.AutoPlay = _kenoEnableAutoPlay;
                        return a;
                    });
            }
        }

        public bool KenoShowPlayerSpeedButton
        {
            get => _kenoShowPlayerSpeedButton;
            set
            {
                if (_kenoShowPlayerSpeedButton == value && _gameCategory[GameType.Keno].ShowPlayerSpeedButton == value)
                {
                    return;
                }

                _kenoShowPlayerSpeedButton = value;
                RaisePropertyChanged(nameof(KenoShowPlayerSpeedButton));

                Save(
                    GameType.Keno,
                    a =>
                    {
                        a.ShowPlayerSpeedButton = _kenoShowPlayerSpeedButton;
                        return a;
                    });
            }
        }

        public int KenoSpeedLevel
        {
            get => _kenoSpeedLevel;
            set
            {
                if (_kenoSpeedLevel == value)
                {
                    return;
                }

                _kenoSpeedLevel = value;
                RaisePropertyChanged(nameof(KenoSpeedLevel));

                Save(
                    GameType.Keno,
                    a =>
                    {
                        a.DealSpeed = _kenoSpeedLevel;
                        return a;
                    });
            }
        }

        public int KenoDefaultSpeedLevel
        {
            get => _kenoDefaultSpeedLevel;
            set
            {
                if (_kenoDefaultSpeedLevel == value)
                {
                    return;
                }

                _kenoDefaultSpeedLevel = value;
                RaisePropertyChanged(nameof(KenoDefaultSpeedLevel));

                Save(
                    GameType.Keno,
                    a =>
                    {
                        a.PlayerSpeed = _kenoDefaultSpeedLevel;
                        return a;
                    });
            }
        }

        public VolumeScalar KenoVolumeScalar
        {
            get => _kenoVolumeScalar;
            set
            {
                if (_kenoVolumeScalar == value)
                {
                    return;
                }

                _kenoVolumeScalar = value;
                RaisePropertyChanged(nameof(KenoVolumeScalar));

                Save(
                    GameType.Keno,
                    a =>
                    {
                        a.VolumeScalar = _kenoVolumeScalar;
                        return a;
                    });
                PlayVolumeChangeSound(_kenoSoundFilePath, _audio.GetVolumeScalar(value));
            }
        }

        public bool PokerEnableAutoHold
        {
            get => _pokerEnableAutoHold;
            set
            {
                if (_pokerEnableAutoHold == value)
                {
                    return;
                }

                _pokerEnableAutoHold = value;
                RaisePropertyChanged(nameof(PokerEnableAutoHold));

                Save(
                    GameType.Poker,
                    a =>
                    {
                        a.AutoHold = _pokerEnableAutoHold;
                        return a;
                    });
            }
        }

        public bool PokerShowPlayerSpeedButton
        {
            get => _pokerShowPlayerSpeedButton;
            set
            {
                if (_pokerShowPlayerSpeedButton == value && _gameCategory[GameType.Poker].ShowPlayerSpeedButton == value)
                {
                    return;
                }

                _pokerShowPlayerSpeedButton = value;
                RaisePropertyChanged(nameof(PokerShowPlayerSpeedButton));

                Save(
                    GameType.Poker,
                    a =>
                    {
                        a.ShowPlayerSpeedButton = _pokerShowPlayerSpeedButton;
                        return a;
                    });
            }
        }

        public int PokerSpeedLevel
        {
            get => _pokerSpeedLevel;
            set
            {
                if (_pokerSpeedLevel == value)
                {
                    return;
                }

                _pokerSpeedLevel = value;
                RaisePropertyChanged(nameof(PokerSpeedLevel));

                Save(
                    GameType.Poker,
                    a =>
                    {
                        a.DealSpeed = _pokerSpeedLevel;
                        return a;
                    });
            }
        }

        public int PokerDefaultSpeedLevel
        {
            get => _pokerDefaultSpeedLevel;
            set
            {
                if (_pokerDefaultSpeedLevel == value)
                {
                    return;
                }

                _pokerDefaultSpeedLevel = value;
                RaisePropertyChanged(nameof(PokerDefaultSpeedLevel));

                Save(
                    GameType.Poker,
                    a =>
                    {
                        a.PlayerSpeed = _pokerDefaultSpeedLevel;
                        return a;
                    });
            }
        }

        public VolumeScalar PokerVolumeScalar
        {
            get => _pokerVolumeScalar;
            set
            {
                if (_pokerVolumeScalar == value)
                {
                    return;
                }

                _pokerVolumeScalar = value;
                RaisePropertyChanged(nameof(PokerVolumeScalar));

                Save(
                    GameType.Poker,
                    a =>
                    {
                        a.VolumeScalar = _pokerVolumeScalar;
                        return a;
                    });
                PlayVolumeChangeSound(_cardSoundFilePath, _audio.GetVolumeScalar(value));
            }
        }

        public string PokerBackgroundColor
        {
            get => _pokerBackgroundColor;
            set
            {
                if (_pokerBackgroundColor == value)
                {
                    return;
                }

                _pokerBackgroundColor = value;
                RaisePropertyChanged(nameof(PokerBackgroundColor), nameof(BackgroundPreviewAvailable));

                Save(GameType.Poker, UpdateBackgroundColor);
                Save(GameType.Slot, UpdateBackgroundColor);

                GameCategorySetting UpdateBackgroundColor(GameCategorySetting setting)
                {
                    setting.BackgroundColor = _pokerBackgroundColor;
                    return setting;
                }
            }
        }

        public VolumeScalar BlackjackVolumeScalar
        {
            get => _blackjackVolumeScalar;
            set
            {
                if (_blackjackVolumeScalar == value)
                {
                    return;
                }

                _blackjackVolumeScalar = value;
                RaisePropertyChanged(nameof(BlackjackVolumeScalar));

                Save(
                    GameType.Blackjack,
                    a =>
                    {
                        a.VolumeScalar = _blackjackVolumeScalar;
                        return a;
                    });
                PlayVolumeChangeSound(_cardSoundFilePath, _audio.GetVolumeScalar(value));
            }
        }

        public VolumeScalar RouletteVolumeScalar
        {
            get => _rouletteVolumeScalar;
            set
            {
                if (_rouletteVolumeScalar == value)
                {
                    return;
                }

                _rouletteVolumeScalar = value;
                RaisePropertyChanged(nameof(RouletteVolumeScalar));

                Save(
                    GameType.Roulette,
                    a =>
                    {
                        a.VolumeScalar = _rouletteVolumeScalar;
                        return a;
                    });
                PlayVolumeChangeSound(_cardSoundFilePath, _audio.GetVolumeScalar(value));
            }
        }

        public bool CensorViolentContent
        {
            get => _censorViolentContent;
            set
            {
                if (_censorViolentContent == value)
                {
                    return;
                }

                _censorViolentContent = value;
                RaisePropertyChanged(nameof(CensorViolentContent));

                // TODO save update this in the PropertiesManager
            }
        }

        public bool CensorDrugUseContent
        {
            get => _censorDrugUseContent;
            set
            {
                if (_censorDrugUseContent == value)
                {
                    return;
                }

                _censorDrugUseContent = value;
                RaisePropertyChanged(nameof(CensorDrugUseContent));

                // TODO save update this in the PropertiesManager
            }
        }

        public bool CensorSexualContent
        {
            get => _censorSexualContent;
            set
            {
                if (_censorSexualContent == value)
                {
                    return;
                }

                _censorSexualContent = value;
                RaisePropertyChanged(nameof(CensorSexualContent));

                // TODO save update this in the PropertiesManager
            }
        }

        public bool CensorOffensiveContent
        {
            get => _censorOffensiveContent;
            set
            {
                if (_censorOffensiveContent == value)
                {
                    return;
                }

                _censorOffensiveContent = value;
                RaisePropertyChanged(nameof(CensorOffensiveContent));

                // TODO save update this in the PropertiesManager
            }
        }

        public ObservableCollection<LanguageOptionViewModel> LanguageOptions { get; } = new ObservableCollection<LanguageOptionViewModel>();

        public GameStartMethodOption GameStartMethod
        {
            get => _gameStartMethod;
            set
            {
                if (_gameStartMethod == value)
                {
                    return;
                }

                _gameStartMethod = value;
                RaisePropertyChanged(nameof(GameStartMethod));
                Save(GamingConstants.GameStartMethod, value);
            }
        }

        public int MinimumGameRoundDuration { get; }

        public int MaximumGameRoundDuration { get; }

        public int GameRoundDurationMs
        {
            get => PropertiesManager.GetValue(GamingConstants.GameRoundDurationMs, GamingConstants.DefaultMinimumGameRoundDurationMs);
            set
            {
                if (value < MinimumGameRoundDuration || value > MaximumGameRoundDuration) return;
                Save(GamingConstants.GameRoundDurationMs, value);
            }
        }

        public ICommand ChangeGameLobbyLayoutCommand { get; set; }

        public ICommand AttractModeCustomizeCommand { get; set; }

        public ICommand AttractModePreviewCommand { get; set; }
        public ICommand PreviewBackgroundCommand { get; set; }
        public bool LogicDoorEnabled
        {
            get => _logicDoorEnabled;
            set
            {
                if (_logicDoorEnabled != value)
                {
                    _logicDoorEnabled = value;
                    RaisePropertyChanged(nameof(LogicDoorEnabled));
                }
            }
        }

        public OperatorMenuAccessRestriction LogicDoorAccessRestriction
        {
            get => _logicDoorAccessRestriction;
            set
            {
                if (_logicDoorAccessRestriction != value)
                {
                    _logicDoorAccessRestriction = value;
                    RaisePropertyChanged(nameof(LogicDoorAccessRestriction));
                    OnFieldAccessRestrictionChange();
                }
            }
        }

        public ProgressiveLobbyIndicator ProgressiveIndicator
        {
            get => _progressiveIndicator;
            set
            {
                if (_progressiveIndicator == value)
                {
                    return;
                }

                if (SetProperty(ref _progressiveIndicator, value, nameof(ProgressiveIndicator)))
                {
                    Save(GamingConstants.ProgressiveLobbyIndicatorType, value);
                }
            }
        }

        protected override void OnFieldAccessRestrictionChange()
        {
            switch (FieldAccessRestriction)
            {
                case OperatorMenuAccessRestriction.LogicDoor:
                    FieldAccessStatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OpenLogicDoorOptionsText);
                    break;
                case OperatorMenuAccessRestriction.MainDoor:
                case OperatorMenuAccessRestriction.MainOpticDoor:
                    FieldAccessStatusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OpenMainDoorOptionsText);
                    break;
            }
        }

        protected override void OnLoaded()
        {
            LoadGameSpeedOptions();
            LoadDefaultPlaySpeedOptions();

            ShowServiceButton = PropertiesManager.GetValue(GamingConstants.ShowServiceButton, false);
            LobbyVolumeScalar = (VolumeScalar)PropertiesManager.GetValue(
                ApplicationConstants.LobbyVolumeScalarKey,
                ApplicationConstants.LobbyVolumeScalar);

            VolumeControlLocation = (VolumeControlLocation)PropertiesManager.GetValue(
                ApplicationConstants.VolumeControlLocationKey,
                ApplicationConstants.VolumeControlLocationDefault);

            var gameProvider = ServiceManager.GetInstance().GetService<IGameProvider>();
            var localeCode = PropertiesManager.GetValue(GamingConstants.SelectedLocaleCode, "EN-US");
            var enabledGames = gameProvider.GetEnabledGames();
            ShowTopPickBanners = PropertiesManager.GetValue(GamingConstants.ShowTopPickBanners, false);

            ShowTopPickBannersVisible = enabledGames.Any(x => x.LocaleGraphics[localeCode].LargeTopPickIcon != null);

            if (CensorshipEnforced)
            {
                //TODO read these values from the property manager
                CensorViolentContent = true;
                CensorDrugUseContent = true;
                CensorSexualContent = true;
                CensorOffensiveContent = true;
            }

            if (SlotOptionsEnabled)
            {
                SlotEnableAutoPlay = _gameCategory[GameType.Slot].AutoPlay;
                SlotVolumeScalar = _gameCategory[GameType.Slot].VolumeScalar;

                var playMode = PropertiesManager.GetValue(GamingConstants.ContinuousPlayMode, PlayMode.Toggle);

                IsButtonContinuousPlayChecked = playMode == PlayMode.Continuous;
                IsDoubleTapForceReelStopChecked = PropertiesManager.GetValue(GamingConstants.ReelStopEnabled, false);
            }

            if (KenoOptionsEnabled)
            {
                KenoEnableAutoPlay = _gameCategory[GameType.Keno].AutoPlay;
                KenoShowPlayerSpeedButton = KenoPlayerSpeedButtonEnabled && _gameCategory[GameType.Keno].ShowPlayerSpeedButton;
                KenoSpeedLevel = SetSpeed(_gameCategory[GameType.Keno].DealSpeed, 1, 9);
                KenoDefaultSpeedLevel = SetSpeed(_gameCategory[GameType.Keno].PlayerSpeed, 1, 3);
                KenoVolumeScalar = _gameCategory[GameType.Keno].VolumeScalar;
            }

            if (PokerOptionsEnabled)
            {
                PokerEnableAutoHold = _gameCategory[GameType.Poker].AutoHold;
                PokerShowPlayerSpeedButton = PokerPlayerSpeedButtonEnabled && _gameCategory[GameType.Poker].ShowPlayerSpeedButton;
                PokerSpeedLevel = SetSpeed(_gameCategory[GameType.Poker].DealSpeed, 1, 9);
                PokerDefaultSpeedLevel = SetSpeed(_gameCategory[GameType.Poker].PlayerSpeed, 1, 3);
                PokerVolumeScalar = _gameCategory[GameType.Poker].VolumeScalar;
                PokerBackgroundColor = _gameCategory[GameType.Poker].BackgroundColor ?? PokerBackgroundColors.First();
            }

            if (BlackjackOptionsEnabled)
            {
                BlackjackVolumeScalar = _gameCategory[GameType.Blackjack].VolumeScalar;
            }

            if (RouletteOptionsEnabled)
            {
                RouletteVolumeScalar = _gameCategory[GameType.Roulette].VolumeScalar;
            }

            AttractEnabled = PropertiesManager.GetValue(GamingConstants.AttractModeEnabled, true);
            ProgressiveIndicator = PropertiesManager.GetValue(
                GamingConstants.ProgressiveLobbyIndicatorType,
                ProgressiveLobbyIndicator.Disabled);

            AttractOptionsEnabled = enabledGames.Any();

            RegisterAccessRule(AutoHoldField, nameof(LogicDoorEnabled), nameof(LogicDoorAccessRestriction));

            LoadSoundFile();
        }

        protected override void OnUnloaded()
        {
            foreach (var language in LanguageOptions)
            {
                language.PropertyChanged -= Language_OnPropertyChanged;
            }
        }

        protected override void DisposeInternal()
        {
            foreach (var language in LanguageOptions)
            {
                language.PropertyChanged -= Language_OnPropertyChanged;
            }

            LanguageOptions.Clear();

            base.DisposeInternal();
        }

        private void ValidateShowProgramPin()
        {
            if (ShowProgramPin.Length < 4)
            {
                SetError(
                    nameof(ShowProgramPin),
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PinLengthLessThanFourErrorMessage));
            }
            else
            {
                ClearErrors(nameof(ShowProgramPin));
                Save(GamingConstants.ShowProgramPin, _showProgramPin);
            }
        }

        private void LoadSoundFile()
        {
            var nodes =
                MonoAddinsHelper.GetSelectedNodes<FilePathExtensionNode>("/OperatorMenu/GamePreferences/Sounds");

            foreach (var node in nodes)
            {
                var path = node.FilePath;
                var name = !string.IsNullOrWhiteSpace(node.Name)
                    ? node.Name
                    : Path.GetFileNameWithoutExtension(path);

                if (name.Equals("Ding"))
                {
                    _dingSoundFilePath = Path.GetFullPath(path);
                    _audio.Load(_dingSoundFilePath);
                }
                else if (name.Equals("ReelClick"))
                {
                    _slotSoundFilePath = Path.GetFullPath(path);
                    _audio.Load(_slotSoundFilePath);
                }
                else if (name.Equals("BallDrop"))
                {
                    _kenoSoundFilePath = Path.GetFullPath(path);
                    _audio.Load(_kenoSoundFilePath);
                }
                else if (name.Equals("CardFlip"))
                {
                    _cardSoundFilePath = Path.GetFullPath(path);
                    _audio.Load(_cardSoundFilePath);
                }
            }
        }

        private void InitializeLanguageOptions()
        {
            var languageOptions = _playerCultureProvider.LanguageOptions.ToList();
            var mandatoryLanguages = (from l in languageOptions where l.IsMandatory select l.Locale).ToArray();

            // get all the languages configured in the jurisdiction and installed games
            var gameLocalesCollection = (from g in _gameProvider.GetAllGames() where DoesGameSupportAllMandatoryLanguages(mandatoryLanguages, g.LocaleGraphics?.Keys.AsEnumerable()) select g.LocaleGraphics.Keys.AsEnumerable()).ToList();
            if (gameLocalesCollection.Any())
            {
                var config = PropertiesManager.GetValue<LobbyConfiguration>(GamingConstants.LobbyConfig, null);
                gameLocalesCollection.Add(config.LocaleCodes);
            }

            var supportedLanguages = LocaleHelper.GetAllSupportedLocales(gameLocalesCollection, _localization, Logger);
            Logger.Debug($"Supported languages by platform: {string.Join(",", supportedLanguages)}");

            var staleLanguages = (from l in languageOptions where !l.IsMandatory && !LocaleHelper.Includes(supportedLanguages, l.Locale) select l).ToList();
            staleLanguages.ForEach(l => languageOptions.Remove(l));

            var gameLanguageOptions = (from l in supportedLanguages
                                       where !languageOptions.Any(s => string.Equals(s.Locale, l, StringComparison.CurrentCultureIgnoreCase))
                                       select new LanguageOption { Locale = l, Enabled = false, IsMandatory = false }).ToArray();

            languageOptions.AddRange(gameLanguageOptions);

            var languageOptionViewModels = new List<LanguageOptionViewModel>();
            languageOptions.ForEach(l => languageOptionViewModels.Add(new LanguageOptionViewModel(new CultureInfo(l.Locale), l.IsMandatory || l.Enabled, l.IsMandatory)));

            var defaultLanguageOption =
                languageOptionViewModels.FirstOrDefault(l => l.CultureInfo.Equals(_playerCultureProvider.DefaultCulture)) ??
                languageOptionViewModels.FirstOrDefault() ?? throw new Exception("Empty language list.");
            defaultLanguageOption.IsDefault = true;

            languageOptionViewModels.ForEach(AddLanguage);

            bool DoesGameSupportAllMandatoryLanguages(IEnumerable<string> jurisdictionMandatoryLanguages, IEnumerable<string> gameSupportedLanguages) => gameSupportedLanguages != null && jurisdictionMandatoryLanguages.All(l => gameSupportedLanguages.Any(k => string.Equals(k, l, StringComparison.InvariantCultureIgnoreCase)));
        }

        private void PlayVolumeChangeSound(string soundFilePath, float fVolumeScale)
        {
            if (!string.IsNullOrEmpty(soundFilePath))
            {
                _audio.Play(soundFilePath, fVolumeScale * _audio.GetDefaultVolume());
            }
        }

        // verifies the speed setting is valid or not 0 and return number or mid point value
        private int SetSpeed(int speed, int min, int max)
        {
            if (speed < min || speed > max)
            {
                return (min + max) / 2;
            }

            return speed;
        }

        private void Save(GameType gameType, Func<GameCategorySetting, GameCategorySetting> setValue)
        {
            if (IsLoaded)
            {
                var setting = _gameCategory[gameType];
                _gameCategory.UpdateGameCategory(gameType, setValue(setting));
            }
        }

        private void Save(string property, object value)
        {
            if (IsLoaded)
            {
                PropertiesManager.SetProperty(property, value);
                EventBus.Publish(new GameCategoryChangedEvent(_currentSelectedGameType));
            }
        }

        private void AddLanguage(LanguageOptionViewModel language)
        {
            language.PropertyChanged += Language_OnPropertyChanged;
            LanguageOptions.Add(language);
        }

        private void Language_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is LanguageOptionViewModel option)
            {
                if (!LanguageOptions.Any(l => l.IsDefault))
                {
                    var defaultLanguage = this.LanguageOptions.FirstOrDefault(l => l.IsEnabled && l.IsMandatoryLanguage) ?? throw new Exception("No enabled mandatory language is found.");
                    defaultLanguage.IsDefault = true;
                }

                if (e.PropertyName == nameof(option.IsDefault) && option.IsDefault)
                {
                    _playerCultureProvider.DefaultCulture = option.CultureInfo;
                    PropertiesManager.SetProperty(ApplicationConstants.LocalizationPlayerDefault, option.CultureInfo.Name);
                }
                else if (e.PropertyName == nameof(option.IsEnabled))
                {
                    if (option.IsEnabled)
                    {
                        _playerCultureProvider.AddCultures(option.CultureInfo);
                    }
                    else
                    {
                        _playerCultureProvider.RemoveCultures(option.CultureInfo);
                    }

                }
            }
        }

        private void ChangeGameLobbyLayout_Clicked(object o)
        {
            // TODO
        }

        private void AttractModeCustomizeButton_Clicked(object o)
        {
            var viewModel = new AttractCustomizationViewModel();

            _dialogService.ShowDialog<AttractCustomizationView>(
                this,
                viewModel,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AttractModeCustomizationViewCaption));
        }

        private void AttractModePreviewButton_Clicked(object o)
        {
            var viewModel = new AttractPreviewViewModel();

            _dialogService.ShowDialog<AttractPreviewView>(
                this,
                viewModel,
                "",
                DialogButton.Cancel,
                new List<DialogButtonCustomTextItem>
                {
                    new DialogButtonCustomTextItem(
                        DialogButton.Cancel,
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExitText))
                });
        }

        private void BackgroundPreviewButton_Clicked(object o)
        {
            var backgroundImagePath = BackgroundPreviewFiles.FirstOrDefault(x => string.Equals(x.Color, PokerBackgroundColor, StringComparison.CurrentCultureIgnoreCase)).BackgroundFilePath;
            if (backgroundImagePath.IsEmpty())
            {
                return;
            }
            var viewModel = new BackgroundPreviewViewModel { BackgroundImagePath = backgroundImagePath };

            _dialogService.ShowDialog<BackgroundPreviewView>(
                this,
                viewModel,
                "",
                DialogButton.Cancel,
                new List<DialogButtonCustomTextItem>
                {
                    new DialogButtonCustomTextItem(
                        DialogButton.Cancel,
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExitText))
                });
        }

        private void LoadGameSpeedOptions()
        {
            GameSpeed.Clear();
            for (var speed = 1; speed < 10; speed++) // 1-9
            {
                GameSpeed.Add(speed);
            }
        }

        private void LoadDefaultPlaySpeedOptions()
        {
            DefaultPlaySpeed.Clear();
            for (var speed = 1; speed < 4; speed++) // 1-3
            {
                DefaultPlaySpeed.Add(speed);
            }
        }

        private void LoadBackgroundPreviewFiles()
        {
            var imagesPath = Path.Combine(
                Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                BackgroundPreviewFilesRelativePath
            );

            if (!Directory.Exists(imagesPath))
            {
                return;
            }

            var images = Directory.EnumerateFiles(imagesPath).OrderBy(s => s).ToList();

            foreach (var color in PokerBackgroundColors.ToList())
            {
                var matchingFilePath = images.Where(path => Path.GetFileName(path).StartsWith(color)).ToList();
                if (matchingFilePath.Any())
                {
                    BackgroundPreviewFiles.Add((color, matchingFilePath.First()));
                }
                else
                {
                    PokerBackgroundColors.Remove(color);
                }
            }
        }

        private void LoadPokerBackgroundColors()
        {
            PokerBackgroundColors.Clear();
            PokerBackgroundColors.Add(KnownColor.Blue.ToString());
            PokerBackgroundColors.Add(KnownColor.Red.ToString());
            PokerBackgroundColors.Add(KnownColor.Green.ToString());
            PokerBackgroundColors.Add(KnownColor.Purple.ToString());
            PokerBackgroundColors.Add(KnownColor.Black.ToString());
        }
    }
}