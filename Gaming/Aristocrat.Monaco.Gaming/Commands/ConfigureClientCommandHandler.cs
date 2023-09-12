namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Accounting.Contracts.HandCount;
    using Cabinet.Contracts;
    using Common;
    using Consumers;
    using Contracts;
    using Contracts.Configuration;
    using Contracts.GameSpecificOptions;
    using Contracts.Lobby;
    using Contracts.Models;
    using Hardware.Contracts;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;
    using log4net;
    using Runtime;
    using Runtime.Client;
    using PlayMode = Contracts.PlayMode;

    /// <summary>
    ///     Command handler for the <see cref="ConfigureClient" /> command.
    /// </summary>
    public class ConfigureClientCommandHandler : ICommandHandler<ConfigureClient>
    {
        private static readonly IReadOnlyCollection<ContinuousPlayButton> DefaultContinuousPlayButtons = new[] { ContinuousPlayButton.Play };
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IAudio _audio;
        private readonly IGameHistory _gameHistory;
        private readonly IGameRecovery _gameRecovery;
        private readonly IGameDiagnostics _gameDiagnostics;
        private readonly ILobbyStateManager _lobbyStateManager;
        private readonly IPlayerBank _playerBank;
        private readonly IPropertiesManager _properties;
        private readonly IRuntime _runtime;
        private readonly IHandCountService _handCountProvider;
        private readonly IGameCategoryService _gameCategoryService;
        private readonly IGameProvider _gameProvider;
        private readonly ICabinetDetectionService _cabinetDetectionService;
        private readonly IHardwareHelper _hardwareHelper;
        private readonly IAttendantService _attendantService;
        private readonly IGameConfigurationProvider _gameConfiguration;
        private readonly IGameSpecificOptionProvider _gameSpecificOptionProvider;
        /// <summary>
        ///     Initializes a new instance of the <see cref="ConfigureClientCommandHandler" /> class.
        /// </summary>
        public ConfigureClientCommandHandler(
            IRuntime runtime,
            IHandCountService handCountProvider,
            IGameHistory gameHistory,
            IGameRecovery gameRecovery,
            IGameDiagnostics gameDiagnostics,
            ILobbyStateManager lobbyStateManager,
            IPropertiesManager properties,
            IPlayerBank bank,
            IAudio audio,
            IGameProvider gameProvider,
            IGameCategoryService gameCategoryService,
            ICabinetDetectionService cabinetDetectionService,
            IHardwareHelper hardwareHelper,
            IAttendantService attendantService,
            IGameConfigurationProvider gameConfiguration,
            IGameSpecificOptionProvider gameSpecificOptionProvider)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _handCountProvider = handCountProvider ?? throw new ArgumentNullException(nameof(handCountProvider));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _gameRecovery = gameRecovery ?? throw new ArgumentNullException(nameof(gameRecovery));
            _gameDiagnostics = gameDiagnostics ?? throw new ArgumentNullException(nameof(gameDiagnostics));
            _lobbyStateManager = lobbyStateManager ?? throw new ArgumentNullException(nameof(lobbyStateManager));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _playerBank = bank ?? throw new ArgumentNullException(nameof(bank));
            _audio = audio ?? throw new ArgumentNullException(nameof(audio));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _gameCategoryService = gameCategoryService ?? throw new ArgumentNullException(nameof(gameCategoryService));
            _cabinetDetectionService = cabinetDetectionService ?? throw new ArgumentNullException(nameof(cabinetDetectionService));
            _hardwareHelper = hardwareHelper ?? throw new ArgumentNullException(nameof(hardwareHelper));
            _attendantService = attendantService ?? throw new ArgumentNullException(nameof(attendantService));
            _gameConfiguration = gameConfiguration ?? throw new ArgumentNullException(nameof(gameConfiguration)); 
            _gameSpecificOptionProvider = gameSpecificOptionProvider ?? throw new ArgumentNullException(nameof(gameSpecificOptionProvider));
        }

        /// <inheritdoc />
        public void Handle(ConfigureClient command)
        {
            _runtime.UpdateLocalTimeTranslationBias(_properties.GetValue(ApplicationConstants.TimeZoneBias, TimeSpan.Zero));

            var currentGame = _gameProvider.GetGame(_properties.GetValue(GamingConstants.SelectedGameId, 0));

            var activeDenominations = _gameProvider.GetEnabledGames().Where(g => g.ThemeId == currentGame.ThemeId)
                .SelectMany(g => g.Denominations.Where(d => d.Active)).OrderBy(d => d.Value).ToList();

            var denomination = currentGame.Denominations.Single(d => d.Value == _properties.GetValue(GamingConstants.SelectedDenom, 0L));

            var volumeControlLocation = (VolumeControlLocation)_properties.GetValue(
                ApplicationConstants.VolumeControlLocationKey,
                ApplicationConstants.VolumeControlLocationDefault);

            var showVolumeControlInLobbyOnly = volumeControlLocation == VolumeControlLocation.Lobby;

            var maxVolumeLevel = _audio.GetMaxVolume(_properties, _gameCategoryService, showVolumeControlInLobbyOnly);

            var useWinLimit = _properties.GetValue(GamingConstants.UseGambleWinLimit, false);
            var singleGameAutoLaunch = _lobbyStateManager.AllowSingleGameAutoLaunch;

            var parameters = new Dictionary<string, string>
            {
                { "/Runtime/Variation/SelectedID", currentGame.VariationId },
                { "/Runtime/Denomination", denomination.Value.MillicentsToCents().ToString() },
                { "/Runtime/ActiveDenominations", string.Join(",", activeDenominations.Select(d => d.Value.MillicentsToCents())) },
                { "/Runtime/Flags&RequireGameStartPermission", "true" },
                { "/Runtime/Localization/Language", _properties.GetValue(GamingConstants.SelectedLocaleCode, "en-us") },
                { "/Runtime/Localization/Currency&symbol", CurrencyExtensions.Currency.CurrencySymbol },
                { "/Runtime/Localization/Currency&minorSymbol", CurrencyExtensions.Currency.MinorUnitSymbol },
                { "/Runtime/Localization/Currency&positivePattern", CurrencyExtensions.CurrencyCultureInfo.NumberFormat.CurrencyPositivePattern.ToString() },
                { "/Runtime/Localization/Currency&negativePattern", CurrencyExtensions.CurrencyCultureInfo.NumberFormat.CurrencyNegativePattern.ToString() },
                { "/Runtime/Localization/Currency&decimalDigits", CurrencyExtensions.CurrencyCultureInfo.NumberFormat.CurrencyDecimalDigits.ToString() },
                { "/Runtime/Localization/Currency&decimalSeparator", CurrencyExtensions.CurrencyCultureInfo.NumberFormat.CurrencyDecimalSeparator },
                { "/Runtime/Localization/Currency&groupSeparator", CurrencyExtensions.CurrencyCultureInfo.NumberFormat.CurrencyGroupSeparator },
                { "/Runtime/Localization/Currency&DenominationDisplayUnit", CurrencyExtensions.Currency.DenomDisplayUnit.ToString() },
                { "/Runtime/Audio&activeLevel", maxVolumeLevel.ToString(CultureInfo.InvariantCulture) },
                { "/Runtime/Account&balance", _playerBank.Credits.ToString() },
                { "/Runtime/Hardware/HasUsbButtonDeck", _hardwareHelper.CheckForUsbButtonDeckHardware().ToString() },
                { "/Runtime/Hardware/HasVirtualButtonDeck", _hardwareHelper.CheckForVirtualButtonDeckHardware().ToString() },
                { "/Runtime/Flags&ShowMode", _properties.GetValue(ApplicationConstants.ShowMode, false).ToString() },
                { "/Runtime/GameRules", _properties.GetValue(ApplicationConstants.GameRules, true).ToString() },
                { "/Runtime/Flags&ShowPlayerVolume", (!showVolumeControlInLobbyOnly).ToString() },
                { "/Runtime/Flags&ShowServiceButton", _properties.GetValue(GamingConstants.ShowServiceButton, true).ToString() },
                { "/Runtime/Flags&InServiceRequest", _attendantService.IsServiceRequested.ToString() },
                { "/Runtime/ReelStop", _properties.GetValue(GamingConstants.ReelStopEnabled, true).ToString() },
                { "/Runtime/ReelStopInBaseGame", _properties.GetValue(GamingConstants.ReelStopInBaseGameEnabled, true).ToString() },
                { "/Runtime/JackpotCeiling", _properties.GetValue(GamingConstants.JackpotCeilingHelpScreen, false).ToString() },
                { "/Runtime/Flags&NewReelSetSelectedNotification", _properties.GetValue(GamingConstants.PlayerNotificationNewReelSetSelected, false).ToString() },
                { "/Runtime/NewReelSetSelectedNotification", _properties.GetValue(GamingConstants.PlayerNotificationNewReelSetSelected, false).ToString() },
                { "/Runtime/Flags&GameContentCensorship", (bool)_properties.GetProperty(GamingConstants.CensorshipEnforced, false) ? "censored" : "uncensored" },
                { "/Runtime/GameContentCensorship&scheme", (bool)_properties.GetProperty(GamingConstants.CensorshipEnforced, false) ? "censored" : "uncensored" },
                { "/Runtime/GambleDynamicHelp", _properties.GetProperty(GamingConstants.ShowGambleDynamicHelp, false).ToString() },
                // Gamble Wager Limit property is always set to a default value: see DefaultGambleWagerLimit
                { "/Runtime/Flags&MaximumGambleHandWager", _properties.GetValue(GamingConstants.GambleWagerLimit, long.MaxValue).MillicentsToCents().ToString() },
                { "/Runtime/GambleWinLimit", useWinLimit ? "true" : "false" },
                { "/Runtime/GambleWinLimit&valueCents", _properties.GetValue(GamingConstants.GambleWinLimit, GamingConstants.DefaultGambleWinLimit).MillicentsToCents().ToString() },
                { "/Runtime/MaximumGambleHandWager", !useWinLimit ? "true" : "false" },
                { "/Runtime/MaximumGambleHandWager&valueCents", _properties.GetValue(GamingConstants.GambleWagerLimit, long.MaxValue).MillicentsToCents().ToString() },
                { "/Runtime/Hardware/EdgeLightSharedMemoryName", EdgeLightRuntimeParameters.EdgeLightSharedMemoryName },
                { "/Runtime/Hardware/EdgeLightSharedMutexName", EdgeLightRuntimeParameters.EdgeLightSharedMutexName },
                { "/Runtime/Hardware/VBD&type", _cabinetDetectionService.ButtonDeckType },
                { "/Runtime/MinimumWagerCredits", denomination.MinimumWagerCredits.ToString() },
                { "/Runtime/MaximumWagerCredits", denomination.MaximumWagerCredits.ToString() },
                { "/Runtime/MaximumWagerInsideCredits", denomination.MaximumWagerCredits.ToString() },
                { "/Runtime/MaximumWagerOutsideCredits", denomination.MaximumWagerOutsideCredits.ToString() },
                { "/Runtime/WagerLimit", _properties.GetValue(AccountingConstants.MaxBetLimit, AccountingConstants.DefaultMaxBetLimit).MillicentsToCents().ToString() },
                { "/Runtime/AttractMode", _properties.GetValue(GamingConstants.AttractModeEnabled, ApplicationConstants.DefaultAttractMode).ToString() },
                { "/Runtime/AttractMode&optional", "true" },
                { "/Runtime/WagerLimits&maxTotalWager", _properties.GetValue(GamingConstants.WagerLimitsMaxTotalWagerKey, GamingConstants.WagerLimitsMaxTotalWager).ToString() },
                { "/Runtime/WagerLimits&use", _properties.GetValue(GamingConstants.WagerLimitsUseKey, GamingConstants.WagerLimitsUse) ? "required" : "disallowed"  },
                { "/Runtime/MaximumGameRoundWin&resetWinAmount", _properties.GetValue(GamingConstants.MaximumGameRoundWinResetWinAmountKey, GamingConstants.MaximumGameRoundWinResetWinAmount).ToLower() },
                { "/Runtime/VolumeLevel&showInHelpScreen", _properties.GetValue(GamingConstants.VolumeLevelShowInHelpScreenKey, GamingConstants.VolumeLevelShowInHelpScreen) ? "allowed" : "disallowed"  },
                { "/Runtime/Service&use", _properties.GetValue(GamingConstants.ServiceUseKey, GamingConstants.ServiceUse).ToString()  },
                { "/Runtime/Clock&useHInDisplay", _properties.GetValue(GamingConstants.ClockUseHInDisplayKey, GamingConstants.ClockUseHInDisplay).ToString()  },
                { "/Runtime/KenoFreeGames&selectionChange", _properties.GetValue(GamingConstants.KenoFreeGamesSelectionChangeKey, GamingConstants.KenoFreeGamesSelectionChange) ? "allowed" : "disallowed"  },
                { "/Runtime/KenoFreeGames&autoPlay", _properties.GetValue(GamingConstants.KenoFreeGamesAutoPlayKey, GamingConstants.KenoFreeGamesAutoPlay) ? "allowed" : "disallowed"  },
                { "/Runtime/InitialZeroWager&use", _properties.GetValue(GamingConstants.InitialZeroWagerUseKey, GamingConstants.InitialZeroWagerUse) ? "allowed" : "disallowed"  },
                { "/Runtime/ChangeLineSelectionAtZeroCredit&use", _properties.GetValue(GamingConstants.ChangeLineSelectionAtZeroCreditUseKey, GamingConstants.ChangeLineSelectionAtZeroCreditUse) ? "allowed" : "disallowed" },
                { "/Runtime/GameDuration&useMarketGameTime", _properties.GetValue(GamingConstants.GameDurationUseMarketGameTimeKey, GamingConstants.GameDurationUseMarketGameTime).ToString()  },
                { "/Runtime/GameLog&enabled", _properties.GetValue(GamingConstants.GameLogEnabledKey, GamingConstants.GameLogEnabled).ToString()  },
                { "/Runtime/GameLog&outcomeDetails", _properties.GetValue(GamingConstants.GameLogOutcomeDetailsKey, GamingConstants.GameLogOutcomeDetails).ToString()  },
                { "/Runtime/Audio&audioChannels", _properties.GetValue(GamingConstants.AudioAudioChannelsKey, GamingConstants.AudioAudioChannels).ToString()  },
                { "/Runtime/ButtonAnimation&goodluck", _properties.GetValue(GamingConstants.ButtonAnimationGoodLuckKey, GamingConstants.ButtonAnimationGoodLuck) ? "allowed" : "disallowed"  },
                { "/Runtime/CardRevealDelayValue", GamingConstants.CardRevealDelayValue.ToString() },
                { "/Runtime/Cashout&clearWins", _properties.GetValue(ApplicationConstants.CashoutClearWins, true).ToString() },
                { "/Runtime/Cashout&commitStorageAfterCashout", _properties.GetValue(ApplicationConstants.CommitStorageAfterCashout, false).ToString() },
                { "/Runtime/ChangeBetSelectionAtZeroCredit", GamingConstants.ChangeBetSelectionAtZeroCredit.ToString() },
                { "/Runtime/Clock", _properties.GetValue(ApplicationConstants.ClockEnabled, false).ToString() },
                { "/Runtime/Clock&format", _properties.GetValue(ApplicationConstants.ClockFormat, 12).ToString() },
                { "/Runtime/DefaultBetInAttract", ApplicationConstants.DefaultBetInAttract.ToString() },
                { "/Runtime/AllowZeroCreditCashout", _properties.GetValue(GamingConstants.AllowZeroCreditCashout, false).ToString()  },
                { "/Runtime/DenomPatch", ApplicationConstants.DefaultAllowDenomPatch.ToString() },
                { "/Runtime/Gamble&maxRounds", GamingConstants.MaxRounds.ToString() },
                { "/Runtime/Gamble&skipByJackpotHit", _properties.GetValue(GamingConstants.GambleSkipByJackpotHit, false).ToString() },
                { "/Runtime/GameDuration&kenoSpeed", _gameCategoryService.SelectedGameCategorySetting.PlayerSpeed.ToString() },
                { "/Runtime/GameDuration&reelSpeed", _properties.GetValue(GamingConstants.ReelSpeedKey, GamingConstants.ReelSpeed).ToString() },
                { "/Runtime/GameRules&gameDisabled", ApplicationConstants.DefaultGameDisabledUse },
                { "/Runtime/DisplayGamePayMessage&use", _properties.GetValue(GamingConstants.DisplayGamePayMessageUseKey, GamingConstants.DisplayGamePayMessageUse) ? "allowed" : "disallowed"  },
                { "/Runtime/DisplayGamePayMessage&format", _properties.GetValue(GamingConstants.DisplayGamePayMessageFormatKey, GamingConstants.DisplayGamePayMessageFormat).ToLower() },
                { "/Runtime/Meters&defaultDisplay", _properties.GetValue(GamingConstants.DefaultCreditDisplayFormat, DisplayFormat.Credit).ToString().ToLower() },
                { "/Runtime/Meters&idleDisplay", GamingConstants.IdleCreditDisplayFormat },
                { "/Runtime/Meters/Win&destination", _properties.GetValue(GamingConstants.WinDestinationKey, GamingConstants.WinDestination) },
                { "/Runtime/FreeSpin&clearWinMeter", _properties.GetValue(GamingConstants.FreeSpinClearWinMeterKey, GamingConstants.FreeSpinClearWinMeter) ? "allowed" : "disallowed" },
                { "/Runtime/ClearWinMeter&onBetChange", _properties.GetValue(GamingConstants.ClearWinMeterOnBetChangeKey, GamingConstants.ClearWinMeterOnBetChange).ToString() },
                { "/Runtime/WinMeterResetOnBetLineDenomChanged", _properties.GetValue(GamingConstants.WinMeterResetOnBetLineDenomChanged, ApplicationConstants.DefaultWinMeterResetOnBetLineDenomChanged).ToString() },
                { "/Runtime/WinMeterResetOnBetLineChanged", _properties.GetValue(GamingConstants.WinMeterResetOnBetLineChanged, ApplicationConstants.DefaultWinMeterResetOnBetLineDenomChanged).ToString() },
                { "/Runtime/WinMeterResetOnDenomChanged", _properties.GetValue(GamingConstants.WinMeterResetOnDenomChanged, ApplicationConstants.DefaultWinMeterResetOnBetLineDenomChanged).ToString() },
                { "/Runtime/MinimumBetMessage&format", _properties.GetProperty(GamingConstants.MinBetMessageFormat, DisplayFormat.Credit.ToString()).ToString().ToLower() },
                { "/Runtime/MinimumBetMessage", _properties.GetValue(GamingConstants.MinBetMessageMustDisplay, false).ToString() },
                { "/Runtime/Multigame&confirmDenomChange", ApplicationConstants.DefaultConfirmDenomChange },
                { "/Runtime/Multigame&defaultBetAfterSwitch", _properties.GetValue(ApplicationConstants.DefaultBetAfterSwitch, true) ? "allowed" : "required" },
                { "/Runtime/Multigame&restoreRebootStateAfterSwitch", _properties.GetValue(ApplicationConstants.RestoreRebootStateAfterSwitch, true).ToString() },
                { "/Runtime/Multigame&stateStorageLocation", _properties.GetValue(ApplicationConstants.StateStorageLocation, "gamePlayerSession") },
                { "/Runtime/InitialBetOption", GamingConstants.DefaultInitialBetOption },
                { "/Runtime/InitialLineOption", GamingConstants.DefaultInitialLineOption },
                { "/Runtime/SelectedWager", (denomination.Value.MillicentsToCents()*_properties.GetValue(GamingConstants.SelectedBetCredits, 0L)).ToString()},
                { "/Runtime/SelectedBetMultiplier", _properties.GetValue(GamingConstants.SelectedBetMultiplier, 0).ToString()},
                { "/Runtime/SelectedLineCost", _properties.GetValue(GamingConstants.SelectedLineCost, 0).ToString()},
                { "/Runtime/Flags&Reserve", _properties.GetValue(ApplicationConstants.ReserveServiceAllowed, true).ToString().ToLower() },
                { "/Runtime/Flags&ReserveType", _properties.GetValue(ApplicationConstants.ReserveServiceEnabled, true) ? "popupReserve" : string.Empty },
                { "/Runtime/CDS&ImmediateReelSpin", _properties.GetValue(GamingConstants.ImmediateReelSpin, false).ToString().ToLower() },
                { "/Runtime/CDS&FudgePay", _properties.GetValue(GamingConstants.FudgePay, false).ToString().ToLower() },
                { "/Runtime/CDS&AdditionalInfoButton", _properties.GetValue(GamingConstants.AdditionalInfoButton, false).ToString().ToLower() },
                { "/Runtime/CDS&CycleMaxBet", _properties.GetValue(GamingConstants.CycleMaxBet, false).ToString().ToLower() },
                { "/Runtime/CDS/AlwaysCombineOutcomesByType", _properties.GetValue(GamingConstants.AlwaysCombineOutcomesByType , true).ToString().ToLower() },
                { "/Runtime/Bell&InitialWinAmount", _properties.GetValue(ApplicationConstants.InitialBellRing, 0L).MillicentsToCents().ToString() },
                { "/Runtime/Bell&IntervalWinAmount", _properties.GetValue(ApplicationConstants.IntervalBellRing, 0L).MillicentsToCents().ToString()},
                { "/Runtime/Multigame", (!singleGameAutoLaunch).ToString() },
                { "/Runtime/Multigame&gameMenuButton", singleGameAutoLaunch ? GamingConstants.SetSubGame : GamingConstants.RequestExitGame },
                { "/Runtime/IKey", _properties.GetValue(GamingConstants.PlayerInformationDisplay.Enabled, false) ? "true" : "false" },
                { "/Runtime/IKey&restrictedModeUse", _properties.GetValue(GamingConstants.PlayerInformationDisplay.RestrictedModeUse, false) ? "allowed" : "disallowed" },
                { "/Runtime/Bell&Use", _properties.GetValue(HardwareConstants.BellEnabledKey, false) ? "allowed" : "disallowed" },
                { "/Runtime/WinTuneCapping", _properties.GetValue(GamingConstants.WinTuneCapping, false).ToString().ToLower() },
                { "/Runtime/WinIncrementSpeed&scheme", _properties.GetValue(GamingConstants.WinIncrementSpeed, WinIncrementSpeed.WinAmountOnly).ToString() },
                { "/Runtime/GameSpecificOptions", _properties.GetValue(GamingConstants.GameSpecificOptions, _gameSpecificOptionProvider.GetCurrentOptionsForGDK(currentGame.ThemeId)) }
            };

            if (currentGame?.Features?.Any(x => x?.FeatureName?.Equals(GamingConstants.BetKeeper, StringComparison.Ordinal) ?? false) ?? false)
            {
                parameters.Add("/Runtime/BetKeeper&enabled", denomination.BetKeeperAllowed ? "true" : "false");
            }

            SetGambleParameters(parameters, currentGame.GameType, denomination);

            if (_properties.GetValue(GamingConstants.RetainLastRoundResult, false))
            {
                parameters.Add("/Runtime/RetainLastRoundResult&optional", "false");
                parameters.Add("/Runtime/RetainLastRoundResult", "true");
            }

            if (_properties.GetValue(GamingConstants.ShowProgramPinRequired, true))
            {
                parameters.Add("/Runtime/ShowProgram&Pin", _properties.GetValue(GamingConstants.ShowProgramPin, GamingConstants.DefaultShowProgramPin));
            }

            var lobbyConfiguration = (LobbyConfiguration)_properties.GetProperty(GamingConstants.LobbyConfig, null);
            if (lobbyConfiguration != null)
            {
                parameters.Add("/Runtime/ResponsibleGambling", lobbyConfiguration.ResponsibleGamingTimeLimitEnabled.ToString());
            }

            SetBeOptionData(parameters, denomination, currentGame);

            var maxGameRoundWin = _properties.GetValue(GamingConstants.MaximumGameRoundWinAmount, 0L);
            if (maxGameRoundWin is not 0L)
            {
                parameters["/Runtime/MaximumGameRoundWin&use"] = "allowed";
                parameters["/Runtime/MaximumGameRoundWin&valueCents"] = maxGameRoundWin.MillicentsToCents().ToString(CultureInfo.InvariantCulture);
                parameters["/Runtime/MaximumGameRoundWin&onMaxWinReach"] = _properties.GetValue(GamingConstants.MaximumGameRoundWinOnMaxWinReachKey, GamingConstants.MaximumGameRoundWinOnMaxWinReachDefault);
            }

            if (denomination.LineOption != null)
            {
                parameters.Add("/Runtime/LineOption", denomination.LineOption);
            }

            if (denomination.BonusBet != 0)
            {
                parameters.Add("/Runtime/BonusAwardMultiplier", denomination.BonusBet.ToString());
            }

            if (showVolumeControlInLobbyOnly)
            {
                var playerVolumeScalar = _audio.GetVolumeScalar((VolumeScalar)_properties.GetValue(ApplicationConstants.PlayerVolumeScalarKey, ApplicationConstants.PlayerVolumeScalar));
                parameters["/Runtime/Audio&playerVolumeScalar"] = playerVolumeScalar.ToString(CultureInfo.InvariantCulture);
            }

            ApplyGameCategorySettings(parameters);
            SetButtonLayout(parameters);
            SetButtonBehavior(parameters);

            if (_properties.GetValue(GamingConstants.PlayLinesAllowed, false))
            {
                parameters["/Runtime/PlayLines&showLinesOnFeatureStart"] =
                    _properties.GetValue(GamingConstants.PlayLinesShowLinesOnFeatureStart, false) ? "allowed" : "disallowed";
                parameters["/Runtime/PlayLines&type"] =
                    _properties.GetValue(GamingConstants.PlayLinesType, string.Empty);
            }


            var gameRoundDuration = _properties.GetValue(GamingConstants.GameRoundDurationMs, GamingConstants.DefaultMinimumGameRoundDurationMs);
            if (gameRoundDuration > GamingConstants.DefaultMinimumGameRoundDurationMs)
            {
                parameters.Add("/Runtime/GameRoundDurationMs", gameRoundDuration.ToString(CultureInfo.InvariantCulture));
            }

            var restrictions = _gameConfiguration.GetActive(currentGame.ThemeId);
            if (restrictions?.RestrictionDetails?.Mapping?.Any() ?? false)
            {
                parameters.Add("/Runtime/Multigame&ActivePack", _properties.GetValue(GamingConstants.GameConfiguration, string.Empty));
            }

            var denomSelectionLobby = _properties.GetValue(GamingConstants.DenomSelectionLobby, DenomSelectionLobby.Allowed);
            if (denomSelectionLobby == DenomSelectionLobby.Required)
            {
                parameters.Add("/Runtime/DenomSelectionLobbyRequired", "true");
            }
            else
            {
                parameters.Add("/Runtime/DenomSelectionLobbyRequired", "false");
                if (denomSelectionLobby == DenomSelectionLobby.Allowed)
                {
                    parameters.Add("/Runtime/DenomSelectionLobby&optional", "true");
                }
            }

            var subGames = _gameProvider.GetEnabledSubGames(currentGame);
            if (!subGames.IsNullOrEmpty())
            {
                var subGameConfiguration = subGames.Serialize();
                Logger.Debug(subGameConfiguration);
                parameters.Add("/Runtime/SimultaneousPlayGames", subGameConfiguration);
            }
            
            var gameRulesInstructions = _properties.GetValue(GamingConstants.GameRulesInstructions, string.Empty);
            if (!string.IsNullOrEmpty(gameRulesInstructions))
            {
                parameters["/Runtime/Instructions/GameRulesInstructions1"] = gameRulesInstructions;
            }

            var pressStartInstructions = _properties.GetValue(GamingConstants.PressStartInstructions, string.Empty);
            if (!string.IsNullOrEmpty(pressStartInstructions))
            {
                parameters["/Runtime/Instructions/PressStart"] = pressStartInstructions;
            }

            AddHandCountSettings(parameters);
            HandleGameHistoryData(parameters);

            foreach (var displayDevice in _cabinetDetectionService.ExpectedDisplayDevices.Where(d => d.Role != DisplayRole.Unknown))
            {
                var diagonalDisplayFlag = ConfigureClientConstants.DiagonalDisplayFlag(displayDevice.Role);
                if (diagonalDisplayFlag != null)
                {
                    parameters[diagonalDisplayFlag] = Math.Round(displayDevice.PhysicalSize.Diagonal, 2).ToString(CultureInfo.InvariantCulture);
                }
            }

            _runtime.UpdateParameters(parameters, ConfigurationTarget.GameConfiguration);
            SetMarketParameters(parameters, singleGameAutoLaunch);
        }

        private void SetGambleParameters(Dictionary<string, string> parameters, GameType gameType, IDenomination denomination)
        {
            if (gameType == GameType.Blackjack)
            {
                parameters.Add("/Runtime/LetItRide", denomination.LetItRideAllowed ? "true" : "false");
            }
            else
            {
                var gambleAllowed = _properties.GetValue(GamingConstants.GambleAllowed, true) &&
                                    denomination.SecondaryAllowed;
                parameters.Add("/Runtime/Flags&Gamble", gambleAllowed ? "true" : "false");
                parameters.Add("/Runtime/Gamble", gambleAllowed ? "true" : "false");
                parameters.Add(
                    "/Runtime/PlayOnFromGambleAvailable",
                    _properties.GetValue(GamingConstants.PlayOnFromGambleAvailable, true) ? "true" : "false");
                parameters.Add(
                    "/Runtime/PlayOnFromPresentWins",
                    _properties.GetValue(GamingConstants.PlayOnFromPresentWins, false) ? "true" : "false");
            }
        }

        private void SetMarketParameters(IDictionary<string, string> parameters, bool singleGameAutoLaunch)
        {
            var marketParameters = new Dictionary<string, string>
            {
                { "/Market/Multigame&use", singleGameAutoLaunch ? "disallowed" : "allowed" },
                { "/Market/MinimumBetMessage&use", _properties.GetValue(GamingConstants.MinBetMessageMustDisplay, false) ? "required" : "allowed" },
                { "/Market/MinimumBetMessage&format", _properties.GetProperty(GamingConstants.MinBetMessageFormat, DisplayFormat.Credit.ToString()).ToString().ToLower() },
                { "/Market/DisplayJackpotOdds&use", _properties.GetValue(GamingConstants.JackpotOddsMustDisplay, false) ? "required" : "allowed" },
                { "/Runtime/DisplayJackpotOdds", _properties.GetValue(GamingConstants.JackpotOddsMustDisplay, false) ? "required" : "allowed" },
                { "/Market/Meters&defaultDisplay", _properties.GetValue(GamingConstants.DefaultCreditDisplayFormat, DisplayFormat.Credit).ToString().ToLower() },
            };

            var inGameDisplayVal = _properties.GetValue(GamingConstants.InGameDisplayFormat, DisplayFormat.Any);
            if (inGameDisplayVal != DisplayFormat.Any)
            {
                marketParameters["/Market/Meters&inGameDisplay"] = inGameDisplayVal.ToString().ToCamelCase();
                parameters["/Runtime/Meters&inGameDisplay"] = inGameDisplayVal.ToString().ToCamelCase();
            }

            _runtime.UpdateParameters(marketParameters, ConfigurationTarget.MarketConfiguration);
        }

        private void SetBeOptionData(Dictionary<string, string> parameters, IDenomination denomination, IGameDetail currentGame)
        {
            if (denomination.BetOption != null) // check that the bet option has been set by the operator
            {
                parameters.Add("/Runtime/BetOption", denomination.BetOption);
                var selectedBetOption = currentGame.BetOptionList?.FirstOrDefault(x => x.Name == denomination.BetOption);
                if (selectedBetOption?.MaxWin != null)
                {
                    parameters["/Runtime/MaximumGameRoundWin&use"] = "allowed";
                    parameters["/Runtime/MaximumGameRoundWin&valueCents"] =
                        (selectedBetOption.MaxWin.Value * denomination.Value).MillicentsToCents()
                        .ToString(CultureInfo.InvariantCulture);
                    parameters["/Runtime/MaximumGameRoundWin&onMaxWinReach"] = _properties.GetValue(
                        GamingConstants.ActionOnMaxWinReached,
                        "endgame");
                }
                else
                {
                    parameters["/Runtime/MaximumGameRoundWin&use"] = "disallowed";
                }
            }
        }

        private void AddHandCountSettings(Dictionary<string, string> parameters)
        {
            if (_handCountProvider.HandCountServiceEnabled)
            {
                parameters.Add("/Runtime/DisplayHandCount", "true");
                parameters.Add("/Runtime/HandCountValue", _handCountProvider.HandCount.ToString());
                parameters.Add(
                    "/Runtime/MinResidualCreditInCents",
                    _properties.GetValue(
                        AccountingConstants.HandCountMinimumRequiredCredits,
                        AccountingConstants.HandCountDefaultRequiredCredits).MillicentsToCents().ToString());
            }
        }

        private static string GetGameStartMethodString(GameStartMethodOption startMethod) =>
            startMethod switch
            {
                GameStartMethodOption.None => "",
                GameStartMethodOption.LineOrReel => "Line",
                GameStartMethodOption.LineReelOrMaxBet => "Line, MaxBet",
                GameStartMethodOption.Bet => "Bet",
                _ => "Bet, MaxBet"
            };

        private static string GetConfigurableGameStartMethods(IEnumerable<GameStartConfigurableMethod> startMethod)
        {
            return string.Join(", ", startMethod.Select(GetButtonText).Where(x => !string.IsNullOrEmpty(x)));

            static string GetButtonText(GameStartConfigurableMethod button)
            {
                return button switch
                {
                    GameStartConfigurableMethod.Bet => "Bet",
                    GameStartConfigurableMethod.MaxBet => "MaxBet",
                    GameStartConfigurableMethod.LineOrReel => "Line",
                    _ => string.Empty
                };
            }
        }

        private static string GetContinuePlayModeString(PlayMode playMode) =>
            playMode switch
            {
                PlayMode.Toggle => "toggle",
                _ => "continuous"
            };

        private static string GetContinuePlayButtonString(ContinuousPlayButton button)
        {
            return button switch
            {
                ContinuousPlayButton.MaxBet => "maxbet",
                _ => "play"
            };
        }

        private static string GetContinuePlayButtonsString(IEnumerable<ContinuousPlayButton> buttons)
        {
            return string.Join(",", buttons.Select(GetContinuePlayButtonString));
        }

        private void HandleGameHistoryData(IDictionary<string, string> parameters)
        {
            if (_gameRecovery.IsRecovering && _gameHistory.LoadRecoveryPoint(out var data))
            {
                AddRecoveryData(parameters, data);
                return;
            }

            if (!_gameDiagnostics.IsActive)
            {
                return;
            }

            foreach (var parameter in _gameDiagnostics.Context.GetParameters())
            {
                parameters[parameter.Key] = parameter.Value;
            }
        }

        private void AddRecoveryData(IDictionary<string, string> parameters, byte[] data)
        {
            _gameHistory.LogRecoveryData(data, "[RECOVERY POINT] <-");
            if (_properties.GetValue(GamingConstants.UseSlowRecovery, false))
            {
                parameters.Add("/Runtime/Recovery", "true");
                parameters.Add("/Runtime/Recovery&realtime", "true");
            }

            if (data is { Length: > 0 })
            {
                parameters.Add("/Runtime/Recovery/BinaryData", Encoding.ASCII.GetString(data));
            }
        }

        private void ApplyGameCategorySettings(IDictionary<string, string> parameters)
        {
            if (!_properties.GetValue(GamingConstants.ApplyGameCategorySettings, false))
            {
                if (_properties.GetValue(GamingConstants.AutoPlayAllowed, false))
                {
                    parameters["/Runtime/Flags&AutoPlay"] = "true";
                }

                return;
            }

            var gameCategorySetting = _gameCategoryService.SelectedGameCategorySetting;
            parameters["/Runtime/Flags&AutoPlay"] = gameCategorySetting.AutoPlay.ToString();
            parameters["/Runtime/Flags&AutoHold"] = gameCategorySetting.AutoHold.ToString();
            parameters["/Runtime/AutoHold"] = gameCategorySetting.AutoHold.ToString();
            parameters["/Runtime/Flags&ShowPlayerSpeedButton"] = gameCategorySetting.ShowPlayerSpeedButton.ToString();
            parameters["/Runtime/ShowPlayerSpeedButton"] = gameCategorySetting.ShowPlayerSpeedButton.ToString();
            parameters["/Runtime/PlaySpeed"] = gameCategorySetting.PlayerSpeed.ToString(CultureInfo.InvariantCulture);
            parameters["/Runtime/GameDuration&playSpeed"] =
                gameCategorySetting.PlayerSpeed.ToString(CultureInfo.InvariantCulture);
            parameters["/Runtime/DealSpeed"] = gameCategorySetting.DealSpeed.ToString(CultureInfo.InvariantCulture);
            parameters["/Runtime/GameDuration&dealSpeed"] =
                gameCategorySetting.DealSpeed.ToString(CultureInfo.InvariantCulture);
            parameters["/Runtime/GamePreferences/BackgroundColor"] =
                gameCategorySetting.BackgroundColor ?? string.Empty;
        }

        private void SetButtonLayout(IDictionary<string, string> parameters)
        {
            parameters["/Runtime/PhysicalButtons&betOnBottom"] = _properties.GetValue(
                GamingConstants.ButtonLayoutBetButtonsOnBottom,
                true).ToString();
            parameters["/Runtime/PhysicalButtons/BetDown"] = _properties.GetValue(
                GamingConstants.ButtonLayoutBetButtonsBetDown,
                "false");
            parameters["/Runtime/PhysicalButtons/BetUp"] = _properties.GetValue(
                GamingConstants.ButtonLayoutBetButtonsBetUp,
                "false");
            parameters["/Runtime/PhysicalButtons/MaxBet"] = _properties.GetValue(
                GamingConstants.ButtonLayoutBetButtonsMaxBet,
                "false");
            parameters["/Runtime/PhysicalButtons/LeftPlay"] = _properties.GetValue(
                GamingConstants.ButtonLayoutPhysicalButtonLeftPlay,
                "false");
            parameters["/Runtime/PhysicalButtons/LeftPlay&optional"] = _properties.GetValue(
                GamingConstants.ButtonLayoutPhysicalButtonLeftPlayOptional,
                false).ToString();
            parameters["/Runtime/PhysicalButtons/Collect"] = _properties.GetValue(
                GamingConstants.ButtonLayoutPhysicalButtonCollect,
                "true");
            parameters["/Runtime/PhysicalButtons/Collect&optional"] = _properties.GetValue(
                GamingConstants.ButtonLayoutPhysicalButtonCollectOptional,
                false).ToString();
            parameters["/Runtime/PhysicalButtons/Gamble"] = _properties.GetValue(
                GamingConstants.ButtonLayoutPhysicalButtonGamble,
                "false");
            parameters["/Runtime/PhysicalButtons/Gamble&optional"] = _properties.GetValue(
                GamingConstants.ButtonLayoutPhysicalButtonGambleOptional,
                false).ToString();
            parameters["/Runtime/PhysicalButtons/Service"] = _properties.GetValue(
                GamingConstants.ButtonLayoutPhysicalButtonService,
                "true");
            parameters["/Runtime/PhysicalButtons/Service&optional"] = _properties.GetValue(
                GamingConstants.ButtonLayoutPhysicalButtonServiceOptional,
                false).ToString();
            parameters["/Runtime/PhysicalButtons/TakeWin"] = _properties.GetValue(
                GamingConstants.ButtonLayoutPhysicalButtonTakeWin,
                "false");
            parameters["/Runtime/PhysicalButtons/TakeWin&optional"] = _properties.GetValue(
                GamingConstants.ButtonLayoutPhysicalButtonTakeWinOptional,
                false).ToString();
        }

        private void SetButtonBehavior(IDictionary<string, string> parameters)
        {
            var defaultStartMethods = _properties.GetValue(GamingConstants.GameStartMethod, GameStartMethodOption.Bet);
            var configurablePlayButtons = _properties.GetValue(GamingConstants.GameConfigurableStartMethods, Array.Empty<GameStartConfigurableMethod>());
            var playMode = _properties.GetValue(GamingConstants.ContinuousPlayMode, PlayMode.Toggle);
            var continuousPlayButtons = _properties.GetValue(
                GamingConstants.ContinuousPlayModeButtonsToUse,
                DefaultContinuousPlayButtons);
            parameters["/Runtime/StartGame&configurableButtons"] = GetConfigurableGameStartMethods(configurablePlayButtons);
            parameters["/Runtime/StartGame&buttons"] = GetGameStartMethodString(defaultStartMethods);
            parameters["/Runtime/Flag&ContinuousPlayMode"] = GetContinuePlayModeString(playMode);
            parameters["/Runtime/ContinuousPlay&buttons"] = GetContinuePlayButtonsString(continuousPlayButtons);
        }
    }
}
