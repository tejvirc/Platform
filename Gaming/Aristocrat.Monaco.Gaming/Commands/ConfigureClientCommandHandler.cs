namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Cabinet.Contracts;
    using Common;
    using Consumers;
    using Contracts;
    using Contracts.Configuration;
    using Contracts.Lobby;
    using Contracts.Models;
    using Hardware.Contracts;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;
    using Runtime;
    using Runtime.Client;
    using PlayMode = Contracts.PlayMode;

    /// <summary>
    ///     Command handler for the <see cref="ConfigureClient" /> command.
    /// </summary>
    public class ConfigureClientCommandHandler : ICommandHandler<ConfigureClient>
    {
        private readonly IAudio _audio;
        private readonly IGameHistory _gameHistory;
        private readonly IGameRecovery _gameRecovery;
        private readonly IGameDiagnostics _gameDiagnostics;
        private readonly ILobbyStateManager _lobbyStateManager;
        private readonly IPlayerBank _playerBank;
        private readonly IPropertiesManager _properties;
        private readonly IRuntime _runtime;
        private readonly IGameCategoryService _gameCategoryService;
        private readonly IGameProvider _gameProvider;
        private readonly ICabinetDetectionService _cabinetDetectionService;
        private readonly IGameHelpTextProvider _gameHelpTextProvider;
        private readonly IHardwareHelper _hardwareHelper;
        private readonly IAttendantService _attendantService;
        private readonly IGameConfigurationProvider _gameConfiguration;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConfigureClientCommandHandler" /> class.
        /// </summary>
        public ConfigureClientCommandHandler(
            IRuntime runtime,
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
            IGameHelpTextProvider gameHelpTextProvider,
            IHardwareHelper hardwareHelper,
            IAttendantService attendantService,
            IGameConfigurationProvider gameConfiguration)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
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
            _gameHelpTextProvider = gameHelpTextProvider ?? throw new ArgumentNullException(nameof(gameHelpTextProvider));
            _hardwareHelper = hardwareHelper ?? throw new ArgumentNullException(nameof(hardwareHelper));
            _attendantService = attendantService ?? throw new ArgumentNullException(nameof(attendantService));
            _gameConfiguration = gameConfiguration ?? throw new ArgumentNullException(nameof(gameConfiguration));
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

            var continuousPlayButtons = _properties.GetValue(GamingConstants.ContinuousPlayModeButtonsToUse, new [] { ContinuousPlayButton.Play }).ToList()
                .Select(
                    x => x switch
                    {
                        ContinuousPlayButton.MaxBet => "maxbet",
                        _ => "play"
                    });

            var parameters = new Dictionary<string, string>
            {
                { "/Runtime/Variation/SelectedID", currentGame.VariationId },
                { "/Runtime/Denomination", denomination.Value.MillicentsToCents().ToString() },
                { "/Runtime/ActiveDenominations", string.Join(",", activeDenominations.Select(d => d.Value.MillicentsToCents())) },
                { "/Runtime/Flags&RequireGameStartPermission", "true" },
                { "/Runtime/Localization/Language", _properties.GetValue(GamingConstants.SelectedLocaleCode, "en-us") },
                { "/Runtime/Localization/Currency&symbol", CurrencyExtensions.CurrencyCultureInfo.NumberFormat.CurrencySymbol },
                { "/Runtime/Localization/Currency&minorSymbol", CurrencyExtensions.MinorUnitSymbol },
                { "/Runtime/Localization/Currency&positivePattern", CurrencyExtensions.CurrencyCultureInfo.NumberFormat.CurrencyPositivePattern.ToString() },
                { "/Runtime/Localization/Currency&negativePattern", CurrencyExtensions.CurrencyCultureInfo.NumberFormat.CurrencyNegativePattern.ToString() },
                { "/Runtime/Localization/Currency&decimalDigits", CurrencyExtensions.CurrencyCultureInfo.NumberFormat.CurrencyDecimalDigits.ToString() },
                { "/Runtime/Localization/Currency&decimalSeparator", CurrencyExtensions.CurrencyCultureInfo.NumberFormat.CurrencyDecimalSeparator },
                { "/Runtime/Localization/Currency&groupSeparator", CurrencyExtensions.CurrencyCultureInfo.NumberFormat.CurrencyGroupSeparator },
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
                { "/Runtime/Flag&ContinuousPlayMode", _properties.GetValue(GamingConstants.ContinuousPlayMode, PlayMode.Toggle) == PlayMode.Toggle ? "toggle" : "continuous" },
                { "/Runtime/ContinuousPlay&buttons", string.Join(",", continuousPlayButtons) },
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
                { "/Runtime/CardRevealDelayValue", GamingConstants.CardRevealDelayValue.ToString() },
                { "/Runtime/Cashout&clearWins", _properties.GetValue(ApplicationConstants.CashoutClearWins, true).ToString() },
                { "/Runtime/Cashout&commitStorageAfterCashout", _properties.GetValue(ApplicationConstants.CommitStorageAfterCashout, false).ToString() },
                { "/Runtime/ChangeBetSelectionAtZeroCredit", GamingConstants.ChangeBetSelectionAtZeroCredit.ToString() },
                { "/Runtime/Clock", _properties.GetValue(ApplicationConstants.ClockEnabled, false).ToString() },
                { "/Runtime/Clock&format", _properties.GetValue(ApplicationConstants.ClockFormat, 12).ToString() },
                { "/Runtime/DefaultBetInAttract", ApplicationConstants.DefaultBetInAttract.ToString() },
                { "/Runtime/DenomPatch", ApplicationConstants.DefaultAllowDenomPatch.ToString() },
                { "/Runtime/Gamble&maxRounds", GamingConstants.MaxRounds.ToString() },
                { "/Runtime/Gamble&skipByJackpotHit", _properties.GetValue(GamingConstants.GambleSkipByJackpotHit, false).ToString() },
                { "/Runtime/GameDuration&kenoSpeed", _gameCategoryService.SelectedGameCategorySetting.PlayerSpeed.ToString() },
                { "/Runtime/GameDuration&reelSpeed", GamingConstants.ReelSpeed.ToString(CultureInfo.InvariantCulture) },
                { "/Runtime/GameRules&gameDisabled", ApplicationConstants.DefaultGameDisabledUse },
                { "/Runtime/Meters&defaultDisplay", _properties.GetValue(GamingConstants.DefaultCreditDisplayFormat, DisplayFormat.Credit).ToString().ToLower() },
                { "/Runtime/Meters&idleDisplay", GamingConstants.IdleCreditDisplayFormat },
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
                { "/Runtime/Bell&Use", _properties.GetValue(HardwareConstants.BellEnabledKey, false) ? "allowed" : "disallowed" }
            };

            if (currentGame.GameType == GameType.Blackjack)
            {
                parameters.Add("/Runtime/LetItRide", denomination.LetItRideAllowed ? "true" : "false");
            }
            else
            {
                parameters.Add("/Runtime/Flags&Gamble", denomination.SecondaryAllowed ? "true" : "false");
                parameters.Add("/Runtime/Gamble", denomination.SecondaryAllowed ? "true" : "false");
                parameters.Add("/Runtime/PlayOnFromGambleAvailable", _properties.GetValue(GamingConstants.PlayOnFromGambleAvailable, true) ? "true" : "false");
                parameters.Add("/Runtime/PlayOnFromPresentWins", _properties.GetValue(GamingConstants.PlayOnFromPresentWins, false) ? "true" : "false");
            }

            if(_properties.GetValue(GamingConstants.RetainLastRoundResult, false))
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

            if (denomination.BetOption != null) // check that the bet option has been set by the operator
            {
                parameters.Add("/Runtime/BetOption", denomination.BetOption);
            }

            if (denomination.LineOption != null)
            {
                parameters.Add("/Runtime/LineOption", denomination.LineOption);
            }

            if (denomination.BonusBet != 0)
            {
                parameters.Add("/Runtime/BonusAwardMultiplier", denomination.BonusBet.ToString());
            }

            foreach (var helpText in _gameHelpTextProvider.AllHelpTexts)
            {
                parameters[helpText.Key] = helpText.Value();
            }

            if (showVolumeControlInLobbyOnly)
            {
                var playerVolumeScalar = _audio.GetVolumeScalar((VolumeScalar)_properties.GetValue(ApplicationConstants.PlayerVolumeScalarKey, ApplicationConstants.PlayerVolumeScalar));
                parameters["/Runtime/Audio&playerVolumeScalar"] = playerVolumeScalar.ToString(CultureInfo.InvariantCulture);
            }

            if ((bool)_properties.GetProperty(GamingConstants.ApplyGameCategorySettings, false))
            {
                parameters["/Runtime/Flags&AutoPlay"] = _gameCategoryService.SelectedGameCategorySetting.AutoPlay.ToString();
                parameters["/Runtime/Flags&AutoHold"] = _gameCategoryService.SelectedGameCategorySetting.AutoHold.ToString();
                parameters["/Runtime/AutoHold"] = _gameCategoryService.SelectedGameCategorySetting.AutoHold.ToString();
                parameters["/Runtime/Flags&ShowPlayerSpeedButton"] = _gameCategoryService.SelectedGameCategorySetting.ShowPlayerSpeedButton.ToString();
                parameters["/Runtime/ShowPlayerSpeedButton"] = _gameCategoryService.SelectedGameCategorySetting.ShowPlayerSpeedButton.ToString();
                parameters["/Runtime/PlaySpeed"] = _gameCategoryService.SelectedGameCategorySetting.PlayerSpeed.ToString();
                parameters["/Runtime/GameDuration&playSpeed"] = _gameCategoryService.SelectedGameCategorySetting.PlayerSpeed.ToString();
                parameters["/Runtime/DealSpeed"] = _gameCategoryService.SelectedGameCategorySetting.DealSpeed.ToString();
                parameters["/Runtime/GameDuration&dealSpeed"] = _gameCategoryService.SelectedGameCategorySetting.DealSpeed.ToString();
                parameters["/Runtime/GamePreferences/BackgroundColor"] = (_gameCategoryService.SelectedGameCategorySetting.BackgroundColor ?? string.Empty);
            }

            // setup button layout options
            parameters["/Runtime/PhysicalButtons&betOnBottom"] = _properties.GetValue(GamingConstants.ButtonLayoutBetButtonsOnBottom, true).ToString();
            parameters["/Runtime/PhysicalButtons/Collect"] = _properties.GetValue(GamingConstants.ButtonLayoutPhysicalButtonCollect, "true");
            parameters["/Runtime/PhysicalButtons/Collect&optional"] = _properties.GetValue(GamingConstants.ButtonLayoutPhysicalButtonCollectOptional, false).ToString();
            parameters["/Runtime/PhysicalButtons/Gamble"] = _properties.GetValue(GamingConstants.ButtonLayoutPhysicalButtonGamble, "false");
            parameters["/Runtime/PhysicalButtons/Gamble&optional"] = _properties.GetValue(GamingConstants.ButtonLayoutPhysicalButtonGambleOptional, false).ToString();
            parameters["/Runtime/PhysicalButtons/Service"] = _properties.GetValue(GamingConstants.ButtonLayoutPhysicalButtonService, "true");
            parameters["/Runtime/PhysicalButtons/Service&optional"] = _properties.GetValue(GamingConstants.ButtonLayoutPhysicalButtonServiceOptional, false).ToString();
            parameters["/Runtime/PhysicalButtons/TakeWin"] = _properties.GetValue(GamingConstants.ButtonLayoutPhysicalButtonTakeWin, "false");
            parameters["/Runtime/PhysicalButtons/TakeWin&optional"] = _properties.GetValue(GamingConstants.ButtonLayoutPhysicalButtonTakeWinOptional, false).ToString();
            parameters["/Runtime/StartGame&buttons"] = _properties.GetValue(GamingConstants.GameStartMethod, GameStartMethodOption.Bet)
                switch
                {
                    GameStartMethodOption.None => "",
                    GameStartMethodOption.LineOrReel => "Line, MaxBet",
                    _ => "Bet, MaxBet"
                };

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

            if (_gameRecovery.IsRecovering && _gameHistory.LoadRecoveryPoint(out var data))
            {
                _gameHistory.LogRecoveryData(data, "[RECOVERY POINT] <-");

                // Uncomment for slow recovery (debug only).
                //parameters.Add("/Runtime/Recovery", "true");
                //parameters.Add("/Runtime/Recovery&realtime", "true");

                if (data != null && data.Length > 0)
                {
                    parameters.Add("/Runtime/Recovery/BinaryData", Encoding.ASCII.GetString(data));
                }
            }
            else if (_gameDiagnostics.IsActive)
            {
                // Add/merge diagnostics parameters
                foreach (var parameter in _gameDiagnostics.Context.GetParameters())
                {
                    parameters[parameter.Key] = parameter.Value;
                }
            }

            foreach (var displayDevice in _cabinetDetectionService.ExpectedDisplayDevices.Where(d => d.Role != DisplayRole.Unknown))
            {
                var diagonalDisplayFlag = ConfigureClientConstants.DiagonalDisplayFlag(displayDevice.Role);
                if (diagonalDisplayFlag != null)
                {
                    parameters[diagonalDisplayFlag] = Math.Round(displayDevice.PhysicalSize.Diagonal, 2).ToString(CultureInfo.InvariantCulture);
                }
            }

            _runtime.UpdateParameters(parameters, ConfigurationTarget.GameConfiguration);
            _runtime.UpdateParameters(marketParameters, ConfigurationTarget.MarketConfiguration);
        }
    }
}
