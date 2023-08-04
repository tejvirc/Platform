namespace Aristocrat.Monaco.Gaming.UI.Settings
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using Application.Contracts;
    using Application.Contracts.Settings;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using Contracts;
    using Hardware.Contracts.Audio;
    using Kernel;

    /// <summary>
    ///     Gaming configuration settings provider.
    /// </summary>
    public class GamingConfigurationSettings : IConfigurationSettings
    {
        private readonly IPropertiesManager _properties;

        private IGameProvider _gameProvider;
        private IGameCategoryService _gameCategory;
        private IAttractConfigurationProvider _attractConfigurationProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GamingConfigurationSettings" /> class.
        /// </summary>
        public GamingConfigurationSettings()
            : this(ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GamingConfigurationSettings" /> class.
        /// </summary>
        /// <param name="properties">A <see cref="IPropertiesManager" /> instance.</param>
        public GamingConfigurationSettings(IPropertiesManager properties)
        {
            _properties = properties;
        }

        /// <inheritdoc />
        public string Name => "Gaming";

        /// <inheritdoc />
        public ConfigurationGroup Groups => ConfigurationGroup.Machine | ConfigurationGroup.Game;

        /// <inheritdoc />
        public async Task Initialize()
        {
            Execute.OnUIThread(
                () =>
                {
                    var resourceDictionary = new ResourceDictionary
                    {
                        Source = new Uri(
                            "/Aristocrat.Monaco.Gaming.UI;component/Settings/MachineSettings.xaml",
                            UriKind.RelativeOrAbsolute)
                    };

                    Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
                });

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task Apply(ConfigurationGroup configGroup, object settings)
        {
            if (!Groups.HasFlag(configGroup))
            {
                throw new ArgumentOutOfRangeException(nameof(configGroup));
            }

            switch (settings)
            {
                case MachineSettings machineSettings:
                    await ApplySettings(machineSettings);
                    break;

                case GamingSettings gamingSettings:
                {
                    var jurisdiction = _properties.GetValue(ApplicationConstants.JurisdictionKey, string.Empty);

                    if (jurisdiction.Equals(gamingSettings.Jurisdiction))
                    {
                        await ApplySettings(gamingSettings);
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid jurisdiction : {gamingSettings.Jurisdiction}");
                    }

                    break;
                }

                default:
                    throw new ArgumentException($@"Invalid settings type, {settings?.GetType()}", nameof(settings));
            }
        }

        /// <inheritdoc />
        public async Task<object> Get(ConfigurationGroup configGroup)
        {
            object settings;

            switch (configGroup)
            {
                case ConfigurationGroup.Machine:
                    settings = await GetMachineSettings();
                    break;

                case ConfigurationGroup.Game:
                    settings = await GetGamingSettings();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(configGroup), configGroup, null);
            }

            return settings;
        }

        private async Task<MachineSettings> GetMachineSettings()
        {
            return await Task.FromResult(
                new MachineSettings
                {
                    ApplyGameCategorySettings =
                        _properties.GetValue(GamingConstants.ApplyGameCategorySettings, false),
                    IdleText =
                        _properties.GetValue(GamingConstants.IdleText, string.Empty),
                    IdleTimePeriod =
                        _properties.GetValue(GamingConstants.IdleTimePeriod, 0),
                    GameRoundDurationMs = 
                        _properties.GetValue(GamingConstants.GameRoundDurationMs, 0),
                });
        }

        private async Task<GamingSettings> GetGamingSettings()
        {
            _gameProvider = _gameProvider ?? ServiceManager.GetInstance().GetService<IGameProvider>();
            _gameCategory = _gameCategory ?? ServiceManager.GetInstance().GetService<IGameCategoryService>();

            var settings = new GamingSettings
            {
                Jurisdiction = _properties.GetValue(ApplicationConstants.JurisdictionKey, string.Empty),
                AutoPlayAllowed = _properties.GetValue(GamingConstants.AutoPlayAllowed, true),
                AllowZeroCreditCashout = _properties.GetValue(GamingConstants.AllowZeroCreditCashout, false),
                VolumeControlLocation = (VolumeControlLocation)_properties.GetValue(
                    ApplicationConstants.VolumeControlLocationKey,
                    ApplicationConstants.VolumeControlLocationDefault),
                ContinuousPlayModeConfigurable =
                    _properties.GetValue(GamingConstants.ContinuousPlayModeConfigurable, true),
                AutoHoldConfigurable = _properties.GetValue(GamingConstants.AutoHoldConfigurable, true),
                ProgressiveIndicator = _properties.GetValue(
                    GamingConstants.ProgressiveLobbyIndicatorType,
                    ProgressiveLobbyIndicator.Disabled),
                ShowServiceButton =
                    _properties.GetValue(GamingConstants.ShowServiceButton, false),
                RelativeVolume =
                    (VolumeScalar)_properties.GetValue(
                        ApplicationConstants.LobbyVolumeScalarKey,
                        ApplicationConstants.LobbyVolumeScalar),
                ReelStopEnabled =
                    _properties.GetValue(GamingConstants.ReelStopEnabled, false),
                ReelSpeed = _properties.GetValue(GamingConstants.ReelSpeedKey, GamingConstants.ReelSpeed),
                WagerLimitsMaxTotalWager = _properties.GetValue(GamingConstants.WagerLimitsMaxTotalWagerKey, GamingConstants.WagerLimitsMaxTotalWager),
                WagerLimitsUse = _properties.GetValue(GamingConstants.WagerLimitsUseKey, GamingConstants.WagerLimitsUse),
                MaximumGameRoundWinResetWinAmount = _properties.GetValue(GamingConstants.MaximumGameRoundWinResetWinAmountKey, GamingConstants.MaximumGameRoundWinResetWinAmount),
                VolumeLevelShowInHelpScreen = _properties.GetValue(GamingConstants.VolumeLevelShowInHelpScreenKey, GamingConstants.VolumeLevelShowInHelpScreen),
                ServiceUse = _properties.GetValue(GamingConstants.ServiceUseKey, GamingConstants.ServiceUse),
                ClockUseHInDisplay = _properties.GetValue(GamingConstants.ClockUseHInDisplayKey, GamingConstants.ClockUseHInDisplay),
                KenoFreeGamesSelectionChange = _properties.GetValue(GamingConstants.KenoFreeGamesSelectionChangeKey, GamingConstants.KenoFreeGamesSelectionChange),
                KenoFreeGamesAutoPlay = _properties.GetValue(GamingConstants.KenoFreeGamesAutoPlayKey, GamingConstants.KenoFreeGamesAutoPlay),
                InitialZeroWagerUse = _properties.GetValue(GamingConstants.InitialZeroWagerUseKey, GamingConstants.InitialZeroWagerUse),
                ChangeLineSelectionAtZeroCreditUse = _properties.GetValue(GamingConstants.ChangeLineSelectionAtZeroCreditUseKey, GamingConstants.ChangeLineSelectionAtZeroCreditUse),
                GameDurationUseMarketGameTime = _properties.GetValue(GamingConstants.GameDurationUseMarketGameTimeKey, GamingConstants.GameDurationUseMarketGameTime),
                GameLogOutcomeDetails = _properties.GetValue(GamingConstants.GameLogOutcomeDetailsKey, GamingConstants.GameLogOutcomeDetails),
                GameLogEnabled = _properties.GetValue(GamingConstants.GameLogEnabledKey, GamingConstants.GameLogEnabled),
                AudioAudioChannels = _properties.GetValue(GamingConstants.AudioAudioChannelsKey, GamingConstants.AudioAudioChannels),
                FreeSpinClearWinMeter = _properties.GetValue(GamingConstants.FreeSpinClearWinMeterKey, GamingConstants.FreeSpinClearWinMeter),
                WinDestination = _properties.GetValue(GamingConstants.WinDestinationKey, GamingConstants.WinDestination),
                DisplayGamePayMessageUse = _properties.GetValue(GamingConstants.DisplayGamePayMessageUseKey, GamingConstants.DisplayGamePayMessageUse),
                DisplayGamePayMessageFormat = _properties.GetValue(GamingConstants.DisplayGamePayMessageFormatKey, GamingConstants.DisplayGamePayMessageFormat),
                ButtonAnimationGoodLuck = _properties.GetValue(GamingConstants.ButtonAnimationGoodLuckKey, GamingConstants.ButtonAnimationGoodLuck),
                GameStartMethod =
                    _properties.GetValue(GamingConstants.GameStartMethod, GameStartMethodOption.Bet),
                Games = new ObservableCollection<GameSettings>(GetGameSettings()),
                Censorship = GetCensorshipSettings(),
                Slot = GetGameCategorySettings<SlotSettings>(
                    x =>
                    {
                        x.ContinuousPlayMode = _properties.GetValue(
                            GamingConstants.ContinuousPlayMode,
                            PlayMode.Toggle);
                        x.ReelStopEnabled = _properties.GetValue(
                            GamingConstants.ReelStopEnabled,
                            false);
                    }),
                Keno = GetGameCategorySettings<KenoSettings>(),
                Poker = GetGameCategorySettings<PokerSettings>(),
                Blackjack = GetGameCategorySettings<BlackjackSettings>(),
                Roulette = GetGameCategorySettings<RouletteSettings>(),
                AttractSettings = GetGameAttractSettings()
            };

            return await Task.FromResult(settings);
        }

        private GameSettings[] GetGameSettings()
        {
            return _gameProvider.GetGames()
                .Select(
                    game => new GameSettings
                    {
                        ThemeId = game.ThemeId,
                        PaytableId = game.PaytableId,
                        Denominations = new ObservableCollection<DenominationSettings>(
                            game.Denominations
                                .Cast<Denomination>()
                                .Select(x => (DenominationSettings)x))
                    }).ToArray();
        }

        private CensorshipSettings GetCensorshipSettings()
        {
            return new CensorshipSettings
            {
                CensorshipEnforced = _properties.GetValue(GamingConstants.CensorshipEnforced, false),
                CensorViolentContent = true,
                CensorDrugUseContent = true,
                CensorSexualContent = true,
                CensorOffensiveContent = true
            };
        }

        private GameAttractSettings GetGameAttractSettings()
        {
            _attractConfigurationProvider = _attractConfigurationProvider ??
                                            ServiceManager.GetInstance().GetService<IAttractConfigurationProvider>();

            return new GameAttractSettings
            {
                OverallAttractEnabled = _properties.GetValue(GamingConstants.AttractModeEnabled, true),
                SlotAttractSelected = _properties.GetValue(GamingConstants.SlotAttractSelected, true),
                KenoAttractSelected = _properties.GetValue(GamingConstants.KenoAttractSelected, false),
                PokerAttractSelected = _properties.GetValue(GamingConstants.PokerAttractSelected, false),
                BlackjackAttractSelected = _properties.GetValue(GamingConstants.BlackjackAttractSelected, false),
                RouletteAttractSelected = _properties.GetValue(GamingConstants.RouletteAttractSelected, false),
                AttractSequence = _attractConfigurationProvider.GetAttractSequence().ToList(),
                DefaultSequenceOverriden = _properties.GetValue(GamingConstants.DefaultAttractSequenceOverridden, false),
            };
        }

        private TSettings GetGameCategorySettings<TSettings>(Action<TSettings> created = null)
            where TSettings : GameCategorySettings, new()
        {
            var settings = new TSettings();

            settings.AutoPlay = _gameCategory[settings.GameType].AutoPlay;
            settings.AutoHold = _gameCategory[settings.GameType].AutoHold;
            settings.ShowPlayerSpeedButton = _gameCategory[settings.GameType].ShowPlayerSpeedButton;
            settings.DealSpeed = _gameCategory[settings.GameType].DealSpeed;
            settings.PlayerSpeed = _gameCategory[settings.GameType].PlayerSpeed;
            settings.VolumeScalar = _gameCategory[settings.GameType].VolumeScalar;
            settings.BackgroundColor = _gameCategory[settings.GameType].BackgroundColor;

            created?.Invoke(settings);

            return settings;
        }

        private async Task ApplySettings(MachineSettings settings)
        {
            _properties.SetProperty(GamingConstants.ApplyGameCategorySettings, settings.ApplyGameCategorySettings);
            _properties.SetProperty(GamingConstants.IdleText, settings.IdleText);
            _properties.SetProperty(GamingConstants.IdleTimePeriod, settings.IdleTimePeriod);
            _properties.SetProperty(GamingConstants.GameRoundDurationMs, settings.GameRoundDurationMs);

            await Task.CompletedTask;
        }

        private async Task ApplySettings(GamingSettings settings)
        {
            _properties.SetProperty(GamingConstants.AutoPlayAllowed, settings.AutoPlayAllowed);
            _properties.SetProperty(
                GamingConstants.ContinuousPlayModeConfigurable,
                settings.ContinuousPlayModeConfigurable);
            _properties.SetProperty(GamingConstants.AutoHoldConfigurable, settings.AutoHoldConfigurable);
            _properties.SetProperty(GamingConstants.ProgressiveLobbyIndicatorType, settings.ProgressiveIndicator);
            _properties.SetProperty(ApplicationConstants.VolumeControlLocationKey, settings.VolumeControlLocation);
            _properties.SetProperty(GamingConstants.ShowServiceButton, settings.ShowServiceButton);
            _properties.SetProperty(ApplicationConstants.LobbyVolumeScalarKey, settings.RelativeVolume);
            _properties.SetProperty(GamingConstants.ReelStopEnabled, settings.ReelStopEnabled);
            _properties.SetProperty(GamingConstants.AllowZeroCreditCashout, settings.AllowZeroCreditCashout);
            _properties.SetProperty(GamingConstants.ReelSpeedKey, settings.ReelSpeed);
            _properties.SetProperty(GamingConstants.WagerLimitsMaxTotalWagerKey, settings.WagerLimitsMaxTotalWager);
            _properties.SetProperty(GamingConstants.WagerLimitsUseKey, settings.WagerLimitsUse);
            _properties.SetProperty(GamingConstants.MaximumGameRoundWinResetWinAmountKey, settings.MaximumGameRoundWinResetWinAmount);
            _properties.SetProperty(GamingConstants.VolumeLevelShowInHelpScreenKey, settings.VolumeLevelShowInHelpScreen);
            _properties.SetProperty(GamingConstants.ServiceUseKey, settings.ServiceUse);
            _properties.SetProperty(GamingConstants.ClockUseHInDisplayKey, settings.ClockUseHInDisplay);
            _properties.SetProperty(GamingConstants.KenoFreeGamesSelectionChangeKey, settings.KenoFreeGamesSelectionChange);
            _properties.SetProperty(GamingConstants.KenoFreeGamesAutoPlayKey, settings.KenoFreeGamesAutoPlay);
            _properties.SetProperty(GamingConstants.InitialZeroWagerUseKey, settings.InitialZeroWagerUse);
            _properties.SetProperty(GamingConstants.ChangeLineSelectionAtZeroCreditUseKey, settings.ChangeLineSelectionAtZeroCreditUse);
            _properties.SetProperty(GamingConstants.GameDurationUseMarketGameTimeKey, settings.GameDurationUseMarketGameTime);
            _properties.SetProperty(GamingConstants.GameLogEnabledKey, settings.GameLogEnabled);
            _properties.SetProperty(GamingConstants.GameLogOutcomeDetailsKey, settings.GameLogOutcomeDetails);
            _properties.SetProperty(GamingConstants.AudioAudioChannelsKey, settings.AudioAudioChannels);
            _properties.SetProperty(GamingConstants.FreeSpinClearWinMeterKey, settings.FreeSpinClearWinMeter);
            _properties.SetProperty(GamingConstants.ButtonAnimationGoodLuckKey, settings.ButtonAnimationGoodLuck);
            _properties.SetProperty(GamingConstants.WinDestinationKey, settings.WinDestination);

            if (_properties.GetValue(GamingConstants.GameStartMethodConfigurable, false))
            {
                _properties.SetProperty(GamingConstants.GameStartMethod, settings.GameStartMethod);
            }

            ApplySettings(settings.Blackjack);
            ApplySettings(settings.Keno);
            ApplySettings(settings.Poker);
            ApplySettings(settings.Roulette);
            ApplySettings(
                settings.Slot,
                x =>
                {
                    _properties.SetProperty(GamingConstants.ContinuousPlayMode, x.ContinuousPlayMode);
                    _properties.SetProperty(GamingConstants.AutoHoldConfigurable, x.ReelStopEnabled);
                });

            ApplySetting(settings.AttractSettings);

            await Task.CompletedTask;
        }

        private void ApplySetting(GameAttractSettings attractSettings)
        {
            _properties.SetProperty(GamingConstants.AttractModeEnabled, attractSettings.OverallAttractEnabled);
            _properties.SetProperty(GamingConstants.SlotAttractSelected, attractSettings.SlotAttractSelected);
            _properties.SetProperty(GamingConstants.KenoAttractSelected, attractSettings.KenoAttractSelected);
            _properties.SetProperty(GamingConstants.PokerAttractSelected, attractSettings.PokerAttractSelected);
            _properties.SetProperty(GamingConstants.BlackjackAttractSelected, attractSettings.BlackjackAttractSelected);
            _properties.SetProperty(GamingConstants.RouletteAttractSelected, attractSettings.RouletteAttractSelected);
            _properties.SetProperty(GamingConstants.DefaultAttractSequenceOverridden, attractSettings.DefaultSequenceOverriden);

            if (attractSettings.DefaultSequenceOverriden)
            {
                _attractConfigurationProvider = _attractConfigurationProvider ??
                                                ServiceManager.GetInstance().GetService<IAttractConfigurationProvider>();
                var importedAttractSequence = attractSettings.AttractSequence.OrderBy(ai => ai.SequenceNumber).ToList();

                _attractConfigurationProvider.SaveAttractSequence(importedAttractSequence);
            }
        }

        private void ApplySettings<TSettings>(TSettings settings, Action<TSettings> applied = null)
            where TSettings : GameCategorySettings
        {
            if (settings is null)
            {
                return;
            }

            _gameCategory = _gameCategory ?? ServiceManager.GetInstance().GetService<IGameCategoryService>();
            _gameCategory.UpdateGameCategory(settings.GameType, (GameCategorySetting)settings);

            applied?.Invoke(settings);
        }
    }
}
