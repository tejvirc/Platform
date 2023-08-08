namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts;
    using Contracts;
    using Contracts.Progressives;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    /// <summary>
    ///     A <see cref="IPropertyProvider" /> implementation for the gaming layer
    /// </summary>
    public class PropertyProvider : IPropertyProvider
    {
        private const string ConfigurationExtensionPath = "/Gaming/Configuration";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly IPersistentStorageAccessor _persistentStorageAccessor;

        private readonly Dictionary<string, (object property, bool isPersisted)> _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PropertyProvider" /> class.
        /// </summary>
        /// <param name="storageManager">the storage manager</param>
        public PropertyProvider(IPersistentStorageManager storageManager)
        {
            if (storageManager == null)
            {
                throw new ArgumentNullException(nameof(storageManager));
            }

            var storageName = GetType().ToString();

            var configuration = ConfigurationUtilities.GetConfiguration(
                ConfigurationExtensionPath,
                () => new GamingConfiguration
                {
                    GameHistory = new GamingConfigurationGameHistory(),
                    GameEnd = new GamingConfigurationGameEnd(),
                    GameWin = new GamingConfigurationGameWin(),
                    MaximumGameRoundWin = new GamingConfigurationMaximumGameRoundWin(),
                    FreeGames = new GamingConfigurationFreeGames(),
                    Messages = new GamingConfigurationMessages(),
                    StateChangeOverride = new GamingConfigurationStateChangeOverride(),
                    OperatorMenu = new GamingConfigurationOperatorMenu(),
                    LockupBehavior = new GamingConfigurationLockupBehavior(),
                    ReelStop = new GamingConfigurationReelStop(),
                    ReelSpeed = new GamingConfigurationReelSpeed(),
                    ReelStopInBaseGame = new GamingConfigurationReelStopInBaseGame(),
                    GameCategory = new GamingConfigurationGameCategory(),
                    DefaultCreditDisplay = new GamingConfigurationDefaultCreditDisplay(),
                    GameRestrictions = new GamingConfigurationGameRestrictions(),
                    InGameDisplay = new GamingConfigurationInGameDisplay(),
                    AutoPlay = new GamingConfigurationAutoPlay(),
                    Censorship = new GamingConfigurationCensorship(),
                    PlayOnFromGambleAvailable = new GamingConfigurationPlayOnFromGambleAvailable(),
                    PlayOnFromPresentWins = new GamingConfigurationPlayOnFromPresentWins(),
                    Gamble = new GamingConfigurationGamble(),
                    PlayLines = new GamingConfigurationPlayLines(),
                    ContinuousPlaySupport = new GamingConfigurationContinuousPlaySupport(),
                    DynamicHelpScreen = new GamingConfigurationDynamicHelpScreen(),
                    ResetGamesPlayedSinceDoorClosed = new GamingConfigurationResetGamesPlayedSinceDoorClosed(),
                    GameRoundDurationMs = new GamingConfigurationGameRoundDurationMs(),
                    AttendantServiceTimeoutSupport = new GamingConfigurationAttendantServiceTimeoutSupport(),
                    PhysicalButtons = new GamingConfigurationPhysicalButtons(),
                    AttractModeOptions = new GamingConfigurationAttractModeOptions(),
                    OverridenGameTypeText = new GamingConfigurationOverridenGameTypeText(),
                    ProgressiveLobbyIndicator = new GamingConfigurationProgressiveLobbyIndicator(),
                    GameLoad = new GamingConfigurationGameLoad(),
                    ProgressivePoolCreation = new GamingConfigurationProgressivePoolCreation(),
                    PlayerInformationDisplay = new GamingConfigurationPlayerInformationDisplay(),
                    FreeSpin = new GamingConfigurationFreeSpin(),
                    Win = new GamingConfigurationWin(),
                    DisplayGamePayMessage = new GamingConfigurationDisplayGamePayMessage(),
                    WagerLimits = new GamingConfigurationWagerLimits(),
                    VolumeLevel = new GamingConfigurationVolumeLevel(),
                    Service = new GamingConfigurationService(),
                    Clock = new GamingConfigurationClock(),
                    KenoFreeGames = new GamingConfigurationKenoFreeGames(),
                    InitialZeroWager = new GamingConfigurationInitialZeroWager(),
                    ChangeLineSelectionAtZeroCredit = new GamingConfigurationChangeLineSelectionAtZeroCredit(),
                    GameDuration = new GamingConfigurationGameDuration(),
                    GameLog = new GamingConfigurationGameLog(),
                    Audio = new GamingConfigurationAudio(),
                    ButtonAnimation = new GamingConfigurationButtonAnimation()
                });

            var blockExists = storageManager.BlockExists(storageName);

            _persistentStorageAccessor = blockExists
                ? storageManager.GetBlock(storageName)
                : storageManager.CreateBlock(PersistenceLevel.Transient, storageName, 1);

            var anyRtpLimits = configuration.GameRestrictions?.ReturnToPlayerLimits?.FirstOrDefault(x => x.GameType == GameTypes.Any);
            var blackjackRtpLimits = configuration.GameRestrictions?.ReturnToPlayerLimits?.FirstOrDefault(x => x.GameType == GameTypes.Blackjack);
            var slotRtpLimits = configuration.GameRestrictions?.ReturnToPlayerLimits?.FirstOrDefault(x => x.GameType == GameTypes.Slot);
            var kenoRtpLimits = configuration.GameRestrictions?.ReturnToPlayerLimits?.FirstOrDefault(x => x.GameType == GameTypes.Keno);
            var pokerRtpLimits = configuration.GameRestrictions?.ReturnToPlayerLimits?.FirstOrDefault(x => x.GameType == GameTypes.Poker);
            var rouletteRtpLimits = configuration.GameRestrictions?.ReturnToPlayerLimits?.FirstOrDefault(x => x.GameType == GameTypes.Roulette);

            var anyGameMinRtp = anyRtpLimits?.MinimumSpecified ?? false ? anyRtpLimits.Minimum : int.MinValue;
            var anyGameMaxRtp = anyRtpLimits?.MaximumSpecified ?? false ? anyRtpLimits.Maximum : int.MaxValue;
            var anyGameIncludeLinkProgressiveIncrementRtp = anyRtpLimits?.IncludeLinkProgressiveIncrementRTP ?? false;
            var anyGameIncludeStandaloneProgressiveIncrementRtp = anyRtpLimits?.IncludeStandaloneProgressiveIncrementRTP ?? true;
            var playerInformationDisplayOptions = configuration.PlayerInformationDisplay;

            _properties = new Dictionary<string, (object property, bool isPersisted)>
            {
                { GamingConstants.SelectedGameId, (InitFromStorage(GamingConstants.SelectedGameId), true) },
                { GamingConstants.SelectedDenom, (InitFromStorage(GamingConstants.SelectedDenom), true) },
                { GamingConstants.SelectedBetOption, (InitFromStorage(GamingConstants.SelectedBetOption), true) },
                { GamingConstants.SelectedWagerCategory, (null, false) },
                { GamingConstants.IsGameRunning, (false, false) },
                { GamingConstants.IdleTimePeriod, (InitFromStorage(GamingConstants.IdleTimePeriod), true) },
                { GamingConstants.SelectedLocaleCode, (InitFromStorage(GamingConstants.SelectedLocaleCode), true) },
                { GamingConstants.KeepGameRoundEvents, ((object)configuration.GameHistory?.KeepGameRoundEvents ?? true, false) },
                { GamingConstants.KeepGameRoundMeterSnapshots, ((object)configuration.GameHistory?.KeepGameRoundMeterSnapshots ?? true, false) },
                { GamingConstants.GameEndCashOutStrategy, ((object)configuration.GameEnd?.CashOutStrategy ?? CashOutStrategy.None, false) },
                { GamingConstants.KeepFailedGameOutcomes, ((object)configuration.GameEnd?.KeepFailedGameOutcomes ?? true, false) },
                { GamingConstants.RequestGameExitOnCashout, ((object)configuration.GameEnd?.RequestGameExitOnCashOut ?? false, false) },
                { GamingConstants.GameWinMaxCreditCashOutStrategy, ((object)configuration.GameWin?.MaxCreditCashOutStrategy ?? MaxCreditCashOutStrategy.Win, false) },
                { GamingConstants.GameWinLargeWinCashOutStrategy, ((object)configuration.GameWin?.LargeWinCashOutStrategy ?? LargeWinCashOutStrategy.Handpay, false) },
                { GamingConstants.MeterFreeGamesIndependently, ((object)configuration.FreeGames?.MeterIndependently ?? false, false) },
                { GamingConstants.ShowMessages, ((object)configuration.Messages?.ShowMessages ?? false, false) },
                { GamingConstants.MessageClearStyle, ((object)configuration.Messages?.MessageClearStyle ?? MessageClearStyle.GameStart, false) },
                { GamingConstants.MinBetMessageMustDisplay, ((object)configuration.Messages?.MinBetMessage?.MustDisplay ?? false, false) },
                { GamingConstants.MinBetMessageFormat, ((object)configuration.Messages?.MinBetMessage?.Format ?? DisplayFormat.Credit, false) },
                { GamingConstants.JackpotOddsMustDisplay, ((object)configuration.Messages?.JackpotOdds?.MustDisplay ?? false, false) },
                { GamingConstants.PlayerNotificationNewReelSetSelected, ((object)configuration.Messages?.PlayerNotification?.NewReelSetSelected ?? false, false) },
                { GamingConstants.StateChangeOverride, ((object)configuration.StateChangeOverride?.DisableStrategy ?? DisableStrategy.None, false) },
                { GamingConstants.OperatorMenuDisableDuringGame, ((object)configuration.OperatorMenu?.DisableDuringGame ?? false, false) },
                { GamingConstants.LockupBehavior, (InitFromStorage(GamingConstants.LockupBehavior), true) },
                { GamingConstants.IdleText, (InitFromStorage(GamingConstants.IdleText), true) },
                { GamingConstants.AutocompleteSet, (InitFromStorage(GamingConstants.AutocompleteSet), true) },
                { GamingConstants.AutocompleteExpired, (InitFromStorage(GamingConstants.AutocompleteExpired), true) },
                { GamingConstants.GameRoundDurationMs, (InitFromStorage(GamingConstants.GameRoundDurationMs), true) },
                { GamingConstants.ReelStopEnabled, (InitFromStorage(GamingConstants.ReelStopEnabled), true) },
                { GamingConstants.ReelSpeedKey, (InitFromStorage(GamingConstants.ReelSpeedKey), true) },
                { GamingConstants.WagerLimitsMaxTotalWagerKey, (InitFromStorage(GamingConstants.WagerLimitsMaxTotalWagerKey), true) },
                { GamingConstants.WagerLimitsUseKey, (InitFromStorage(GamingConstants.WagerLimitsUseKey), true) },
                { GamingConstants.MaximumGameRoundWinResetWinAmountKey, (InitFromStorage(GamingConstants.MaximumGameRoundWinResetWinAmountKey), true) },
                { GamingConstants.VolumeLevelShowInHelpScreenKey, (InitFromStorage(GamingConstants.VolumeLevelShowInHelpScreenKey), true) },
                { GamingConstants.ServiceUseKey, (InitFromStorage(GamingConstants.ServiceUseKey), true) },
                { GamingConstants.ClockUseHInDisplayKey, (InitFromStorage(GamingConstants.ClockUseHInDisplayKey), true) },
                { GamingConstants.KenoFreeGamesSelectionChangeKey, (InitFromStorage(GamingConstants.KenoFreeGamesSelectionChangeKey), true) },
                { GamingConstants.KenoFreeGamesAutoPlayKey, (InitFromStorage(GamingConstants.KenoFreeGamesAutoPlayKey), true) },
                { GamingConstants.InitialZeroWagerUseKey, (InitFromStorage(GamingConstants.InitialZeroWagerUseKey), true) },
                { GamingConstants.ChangeLineSelectionAtZeroCreditUseKey, (InitFromStorage(GamingConstants.ChangeLineSelectionAtZeroCreditUseKey), true) },
                { GamingConstants.GameDurationUseMarketGameTimeKey, (InitFromStorage(GamingConstants.GameDurationUseMarketGameTimeKey), true) },
                { GamingConstants.GameLogEnabledKey, (InitFromStorage(GamingConstants.GameLogEnabledKey), true) },
                { GamingConstants.GameLogOutcomeDetailsKey, (InitFromStorage(GamingConstants.GameLogOutcomeDetailsKey), true) },
                { GamingConstants.AudioAudioChannelsKey, (InitFromStorage(GamingConstants.AudioAudioChannelsKey), true) },
                { GamingConstants.FreeSpinClearWinMeterKey, (InitFromStorage(GamingConstants.FreeSpinClearWinMeterKey), true) },
                { GamingConstants.WinDestinationKey, (InitFromStorage(GamingConstants.WinDestinationKey), true) },
                { GamingConstants.ButtonAnimationGoodLuckKey, (InitFromStorage(GamingConstants.ButtonAnimationGoodLuckKey), true) },
                { GamingConstants.ReelStopInBaseGameEnabled, ((object)configuration.ReelStopInBaseGame?.Enabled ?? true, false) },
                { GamingConstants.ApplyGameCategorySettings, ((object)configuration.GameCategory?.ApplyGameCategorySettings ?? false, false) },
                { GamingConstants.JackpotCeilingHelpScreen, ((object)configuration.DynamicHelpScreen?.JackpotCeiling ?? false, false) },
                { GamingConstants.RetainLastRoundResult, ((object)configuration.RetainLastRoundResult?.Enabled ?? false, false) },
                {
                    GamingConstants.WinMeterResetOnBetLineDenomChanged, (
                        configuration.WinMeterResetOnBetLineDenomChanged?.Enabled ??
                        ApplicationConstants.DefaultWinMeterResetOnBetLineDenomChanged,
                        false)
                },
                {
                    GamingConstants.WinMeterResetOnBetLineChanged, (
                        configuration.WinMeterResetOnBetLineChanged?.Enabled ??
                        configuration.WinMeterResetOnBetLineDenomChanged?.Enabled ??
                        ApplicationConstants.DefaultWinMeterResetOnBetLineDenomChanged,
                        false)
                },
                {
                    GamingConstants.WinMeterResetOnDenomChanged, (
                        configuration.WinMeterResetOnDenomChanged?.Enabled ??
                        configuration.WinMeterResetOnBetLineDenomChanged?.Enabled ??
                        ApplicationConstants.DefaultWinMeterResetOnBetLineDenomChanged,
                        false)
                },
                { GamingConstants.ShowServiceButton, (InitFromStorage(GamingConstants.ShowServiceButton), true) },
                { GamingConstants.CensorshipEnforced, (configuration.Censorship?.Enforced ?? false, false) },
                { GamingConstants.CensorshipEditable, ((object)configuration.Censorship?.Editable ?? true, false) },
                { GamingConstants.AutoHoldEnable, (InitFromStorage(GamingConstants.AutoHoldEnable), false) },
                { GamingConstants.AutoHoldConfigurable, ((object)configuration.AutoHold?.Configurable ?? true, false) },
                { GamingConstants.ReplayPauseEnable, ((object)configuration.ReplayPause?.Enable ?? true, false) },
                { GamingConstants.ReplayPauseActive, (InitFromStorage(GamingConstants.ReplayPauseActive), true) },
                { GamingConstants.LockupBehaviorConfigurable, (configuration.LockupBehavior?.Configurable ?? false, false) },
                { GamingConstants.DefaultCreditDisplayFormat, (configuration.DefaultCreditDisplay?.Format ?? DisplayFormat.Credit, false) },
                { GamingConstants.PlayOnFromGambleAvailable, ((object)configuration.PlayOnFromGambleAvailable?.Enabled ?? true, false) },
                { GamingConstants.PlayOnFromPresentWins, ((object)configuration.PlayOnFromPresentWins?.Enabled ?? false, false) },
                { GamingConstants.GambleAllowed, ((object)configuration.Gamble?.Allowed ?? true, false) },
                { GamingConstants.GambleEnabled, ((object)configuration.Gamble?.Enabled ?? false, false) },
                { GamingConstants.GambleSkipByJackpotHit, ((object)configuration.Gamble?.SkipByJackpotHit ?? false, false) },
                { GamingConstants.MaximumGameRoundWinAmount, ((object)configuration.MaximumGameRoundWin?.Amount ?? 0L, false) },
                { GamingConstants.GambleWagerLimit, (InitFromStorage(GamingConstants.GambleWagerLimit), true) },
                { GamingConstants.GambleWagerLimitConfigurable, ((object)configuration.Gamble?.WagerLimitConfigurable ?? true, false) },
                { GamingConstants.LetItRideAllowed, ((object)configuration.LetItRide?.Allowed ?? true, false) },
                { GamingConstants.LetItRideEnabled, ((object)configuration.LetItRide?.Enabled ?? false, false) },
                { GamingConstants.ShowGambleDynamicHelp, ((object)configuration.Gamble?.ShowGambleDynamicHelp ?? false, false) },
                { GamingConstants.GambleWinLimitConfigurable, ((object)configuration.Gamble?.WinLimitConfigurable ?? true, false) },
                { GamingConstants.GambleWinLimit, (InitFromStorage(GamingConstants.GambleWinLimit), true) },
                { GamingConstants.UseGambleWinLimit, ((object)configuration.Gamble?.UseWinLimit ?? false, false) },
                { GamingConstants.PlayLinesAllowed, ((object)configuration.PlayLines?.Allowed ?? false, false) },
                { GamingConstants.PlayLinesShowLinesOnFeatureStart, ((object)configuration.PlayLines?.ShowLinesOnFeatureStart ?? false, false) },
                { GamingConstants.PlayLinesType, ((object)configuration.PlayLines?.Type ?? string.Empty, false) },
                { GamingConstants.ContinuousPlayMode, (InitFromStorage(GamingConstants.ContinuousPlayMode), true) },
                { GamingConstants.ContinuousPlayModeConfigurable, ((object)configuration.ContinuousPlaySupport?.Configurable ?? false, false) },
                { GamingConstants.ContinuousPlayModeButtonsToUse, ((object)configuration.ContinuousPlaySupport?.AllowedButtons ?? new [] { ContinuousPlayButton.Play }, false) },
                { GamingConstants.AnyGameMinimumReturnToPlayer, (anyGameMinRtp, false) },
                { GamingConstants.AnyGameMaximumReturnToPlayer, (anyGameMaxRtp, false) },
                { GamingConstants.SlotMinimumReturnToPlayer, (GetRtpValue((slotRtpLimits?.MinimumSpecified ?? false) ? slotRtpLimits.Minimum : anyGameMinRtp, anyGameMinRtp), false) },
                { GamingConstants.SlotMaximumReturnToPlayer, (GetRtpValue((slotRtpLimits?.MaximumSpecified ?? false) ? slotRtpLimits.Maximum : anyGameMaxRtp, anyGameMaxRtp), false) },
                { GamingConstants.PokerMinimumReturnToPlayer, (GetRtpValue((pokerRtpLimits?.MinimumSpecified ?? false) ? pokerRtpLimits.Minimum : anyGameMinRtp, anyGameMinRtp), false) },
                { GamingConstants.PokerMaximumReturnToPlayer, (GetRtpValue((pokerRtpLimits?.MaximumSpecified ?? false) ? pokerRtpLimits.Maximum : anyGameMaxRtp, anyGameMaxRtp), false) },
                { GamingConstants.RouletteMinimumReturnToPlayer, (GetRtpValue((rouletteRtpLimits?.MinimumSpecified ?? false) ? rouletteRtpLimits.Minimum : anyGameMinRtp, anyGameMinRtp), false) },
                { GamingConstants.RouletteMaximumReturnToPlayer, (GetRtpValue((rouletteRtpLimits?.MaximumSpecified ?? false) ? rouletteRtpLimits.Maximum : anyGameMaxRtp, anyGameMaxRtp), false) },
                { GamingConstants.BlackjackMinimumReturnToPlayer, (GetRtpValue((blackjackRtpLimits?.MinimumSpecified ?? false) ? blackjackRtpLimits.Minimum : anyGameMinRtp, anyGameMinRtp), false) },
                { GamingConstants.BlackjackMaximumReturnToPlayer, (GetRtpValue((blackjackRtpLimits?.MaximumSpecified ?? false) ? blackjackRtpLimits.Maximum : anyGameMaxRtp, anyGameMaxRtp), false) },
                { GamingConstants.KenoMinimumReturnToPlayer, (GetRtpValue((kenoRtpLimits?.MinimumSpecified ?? false) ? kenoRtpLimits.Minimum : anyGameMinRtp, anyGameMinRtp), false) },
                { GamingConstants.KenoMaximumReturnToPlayer, (GetRtpValue((kenoRtpLimits?.MaximumSpecified ?? false) ? kenoRtpLimits.Maximum : anyGameMaxRtp, anyGameMaxRtp), false) },
                { GamingConstants.AllowSlotGames, ((configuration.GameRestrictions?.RestrictedGameTypes?.Count(x => x.GameType == GameTypes.Slot) ?? 0) == 0, false) },
                { GamingConstants.AllowPokerGames, ((configuration.GameRestrictions?.RestrictedGameTypes?.Count(x => x.GameType == GameTypes.Poker) ?? 0) == 0, false) },
                { GamingConstants.AllowKenoGames, ((configuration.GameRestrictions?.RestrictedGameTypes?.Count(x => x.GameType == GameTypes.Keno) ?? 0) == 0, false) },
                { GamingConstants.AllowBlackjackGames, ((configuration.GameRestrictions?.RestrictedGameTypes?.Count(x => x.GameType == GameTypes.Blackjack) ?? 0) == 0, false) },
                { GamingConstants.AllowRouletteGames, ((configuration.GameRestrictions?.RestrictedGameTypes?.Count(x => x.GameType == GameTypes.Roulette) ?? 0) == 0, false) },
                { GamingConstants.RestrictedProgressiveTypes, (configuration.GameRestrictions?.RestrictedProgressivesTypes?.Select(x => x.ProgressiveType.ToProgressiveLevelType()).ToList() ?? new List<ProgressiveLevelType>(), false) },
                { GamingConstants.SlotsIncludeLinkProgressiveIncrementRtp, (slotRtpLimits?.IncludeLinkProgressiveIncrementRTP ?? anyGameIncludeLinkProgressiveIncrementRtp, false) },
                { GamingConstants.PokerIncludeLinkProgressiveIncrementRtp, (pokerRtpLimits?.IncludeLinkProgressiveIncrementRTP ?? anyGameIncludeLinkProgressiveIncrementRtp, false) },
                { GamingConstants.RouletteIncludeLinkProgressiveIncrementRtp, (rouletteRtpLimits?.IncludeLinkProgressiveIncrementRTP ?? anyGameIncludeLinkProgressiveIncrementRtp, false) },
                { GamingConstants.KenoIncludeLinkProgressiveIncrementRtp, (kenoRtpLimits?.IncludeLinkProgressiveIncrementRTP ?? anyGameIncludeLinkProgressiveIncrementRtp, false) },
                { GamingConstants.BlackjackIncludeLinkProgressiveIncrementRtp, (blackjackRtpLimits?.IncludeLinkProgressiveIncrementRTP ?? anyGameIncludeLinkProgressiveIncrementRtp, false) },
                { GamingConstants.SlotsIncludeStandaloneProgressiveIncrementRtp, (slotRtpLimits?.IncludeStandaloneProgressiveIncrementRTP ?? anyGameIncludeStandaloneProgressiveIncrementRtp, false) },
                { GamingConstants.PokerIncludeStandaloneProgressiveIncrementRtp, (pokerRtpLimits?.IncludeStandaloneProgressiveIncrementRTP ?? anyGameIncludeStandaloneProgressiveIncrementRtp, false) },
                { GamingConstants.RouletteIncludeStandaloneProgressiveIncrementRtp, (rouletteRtpLimits?.IncludeStandaloneProgressiveIncrementRTP ?? anyGameIncludeStandaloneProgressiveIncrementRtp, false) },
                { GamingConstants.KenoIncludeStandaloneProgressiveIncrementRtp, (kenoRtpLimits?.IncludeStandaloneProgressiveIncrementRTP ?? anyGameIncludeStandaloneProgressiveIncrementRtp, false) },
                { GamingConstants.BlackjackIncludeStandaloneProgressiveIncrementRtp, (blackjackRtpLimits?.IncludeStandaloneProgressiveIncrementRTP ?? anyGameIncludeStandaloneProgressiveIncrementRtp, false) },
                { GamingConstants.InGameDisplayFormat, (configuration.InGameDisplay?.DisplayFormat ?? DisplayFormat.Any, false) },
                { GamingConstants.AllowCashInDuringPlay, ((object)configuration.InGamePlay?.AllowCashInDuringPlay ?? false, false) },
                { GamingConstants.AllowEditHostDisabled, ((object)configuration.GameEditOptions?.AllowEditHostDisabled ?? false, false) },
                { GamingConstants.AutoPlayAllowed, (configuration.AutoPlay?.Allowed ?? true, false) },
                { GamingConstants.DisplayVoucherIssuedMessage, (configuration.Messages?.VoucherIssued?.Display ?? true, false) },
                { GamingConstants.GameStartMethod, (InitFromStorage(GamingConstants.GameStartMethod), true) },
                { GamingConstants.GameConfigurableStartMethods, (configuration.PhysicalButtons?.GameStartButtons?.GameConfigurableButtons ?? new[] { GameStartConfigurableMethod.MaxBet }, false) },
                { GamingConstants.GameStartMethodConfigurable, (configuration.PhysicalButtons?.GameStartButtons?.Configurable ?? false, false) },
                { GamingConstants.GameStartMethodSettingVisible, (configuration.PhysicalButtons?.GameStartButtons?.SettingsVisible ?? true, false) },
                { GamingConstants.ReportCashoutButtonPressWithZeroCredit, (configuration.PhysicalButtons?.CashOutButton?.ReportToHostWithZeroCredit ?? false, false) },
                { GamingConstants.OperatorMenuPerformancePageDeselectedGameThemes, (InitFromStorage(GamingConstants.OperatorMenuPerformancePageDeselectedGameThemes), true) },
                { GamingConstants.OperatorMenuPerformancePageHideNeverActive, (InitFromStorage(GamingConstants.OperatorMenuPerformancePageHideNeverActive), true) },
                { GamingConstants.OperatorMenuPerformancePageHidePreviouslyActive, (InitFromStorage(GamingConstants.OperatorMenuPerformancePageHidePreviouslyActive), true) },
                { GamingConstants.OperatorMenuPerformancePageSelectedGameType, (InitFromStorage(GamingConstants.OperatorMenuPerformancePageSelectedGameType), true) },
                { GamingConstants.OperatorMenuPerformancePageSortDirection, (InitFromStorage(GamingConstants.OperatorMenuPerformancePageSortDirection), true) },
                { GamingConstants.OperatorMenuPerformancePageSortMemberPath, (InitFromStorage(GamingConstants.OperatorMenuPerformancePageSortMemberPath), true) },
                { GamingConstants.ResetGamesPlayedSinceDoorClosedBelly, (configuration.ResetGamesPlayedSinceDoorClosed?.Belly ?? true, false) },
                { GamingConstants.MinimumGameRoundDuration, (configuration.GameRoundDurationMs?.Minimum ?? GamingConstants.DefaultMinimumGameRoundDurationMs, false) },
                { GamingConstants.MaximumGameRoundDuration, (configuration.GameRoundDurationMs?.Maximum ?? GamingConstants.DefaultMaximumGameRoundDurationMs, false) },
                { GamingConstants.ReelStopConfigurable, (configuration.ReelStop?.Configurable ?? true, false) },
                { GamingConstants.ProgressiveLobbyIndicatorType, (InitFromStorage(GamingConstants.ProgressiveLobbyIndicatorType), true) },
                { GamingConstants.ProgressivePoolCreationType, (configuration.ProgressivePoolCreation?.Type ?? ProgressivePoolCreation.Default, false) },
                { GamingConstants.AttendantServiceTimeoutSupportEnabled, (configuration.AttendantServiceTimeoutSupport?.Enabled ?? false, false) },
                { GamingConstants.AttendantServiceTimeoutInMilliseconds, (configuration.AttendantServiceTimeoutSupport?.TimeoutInMilliseconds ?? 180000, false) },
                { GamingConstants.OperatorMenuGameConfigurationInitialConfigComplete, (InitFromStorage(GamingConstants.OperatorMenuGameConfigurationInitialConfigComplete), true) },
                { GamingConstants.ButtonLayoutBetButtonsOnBottom, (configuration?.PhysicalButtons?.BetButtons?.DisplayOnBottom ?? true, false) },
                { GamingConstants.ButtonLayoutBetButtonsBetDown, (configuration?.PhysicalButtons?.BetButtons?.BetDown ?? "false", false) },
                { GamingConstants.ButtonLayoutBetButtonsBetUp, (configuration?.PhysicalButtons?.BetButtons?.BetUp ?? "false", false) },
                { GamingConstants.ButtonLayoutBetButtonsMaxBet, (configuration?.PhysicalButtons?.BetButtons?.MaxBet ?? "false", false) },
                { GamingConstants.ButtonLayoutPhysicalButtonLeftPlay, (configuration?.PhysicalButtons?.LeftPlayButton?.Required ?? "false", false) },
                { GamingConstants.ButtonLayoutPhysicalButtonLeftPlayOptional, (configuration?.PhysicalButtons?.LeftPlayButton?.Optional ?? false, false) },
                { GamingConstants.ButtonLayoutPhysicalButtonCollect, (configuration?.PhysicalButtons?.CollectButton?.Required ?? "true", false) },
                { GamingConstants.ButtonLayoutPhysicalButtonCollectOptional, (configuration?.PhysicalButtons?.CollectButton?.Optional ?? false, false) },
                { GamingConstants.ButtonLayoutPhysicalButtonGamble, (configuration?.PhysicalButtons?.GambleButton?.Required ?? "false", false) },
                { GamingConstants.ButtonLayoutPhysicalButtonGambleOptional, (configuration?.PhysicalButtons?.GambleButton?.Optional ?? false, false) },
                { GamingConstants.ButtonLayoutPhysicalButtonService, (configuration?.PhysicalButtons?.ServiceButton?.Required ?? "true", false) },
                { GamingConstants.ButtonLayoutPhysicalButtonServiceOptional, (configuration?.PhysicalButtons?.ServiceButton?.Optional ?? false, false) },
                { GamingConstants.ButtonLayoutPhysicalButtonTakeWin, (configuration?.PhysicalButtons?.TakeWinButton?.Required ?? "false", false) },
                { GamingConstants.ButtonLayoutPhysicalButtonTakeWinOptional, (configuration?.PhysicalButtons?.TakeWinButton?.Optional ?? false, false) },
                { GamingConstants.ShowProgramPinRequired, (InitFromStorage(GamingConstants.ShowProgramPinRequired), true) },
                { GamingConstants.ShowProgramPin, (InitFromStorage(GamingConstants.ShowProgramPin), true) },
                { GamingConstants.ShowProgramEnableResetCredits, (InitFromStorage(GamingConstants.ShowProgramEnableResetCredits), true) },
                { GamingConstants.AttractModeEnabled, (InitFromStorage(GamingConstants.AttractModeEnabled), true) },
                { GamingConstants.SlotAttractSelected, ((configuration.AttractModeOptions?.SlotAttractSelected ?? true), false) },
                { GamingConstants.KenoAttractSelected, ((configuration.AttractModeOptions?.KenoAttractSelected ?? true), false) },
                { GamingConstants.PokerAttractSelected, ((configuration.AttractModeOptions?.PokerAttractSelected ?? true), false) },
                { GamingConstants.BlackjackAttractSelected, ((configuration.AttractModeOptions?.BlackjackAttractSelected ?? true), false) },
                { GamingConstants.RouletteAttractSelected, ((configuration.AttractModeOptions?.RouletteAttractSelected ?? true), false) },
                { GamingConstants.OverridenSlotGameTypeText, (configuration?.OverridenGameTypeText?.SlotGameTypeText ?? string.Empty, false) },
                { GamingConstants.ProgressiveCommitTimeoutMs, (InitFromStorage(GamingConstants.ProgressiveCommitTimeoutMs), true) },
                { GamingConstants.DefaultAttractSequenceOverridden, (InitFromStorage(GamingConstants.DefaultAttractSequenceOverridden), true) },
                { GamingConstants.AllowGameInCharge, (configuration?.GameLoad?.AllowGameInCharge ?? false, false) },
                { GamingConstants.ImmediateReelSpin, (configuration.ImmediateReelSpin?.Enabled ?? false, false) },
                { GamingConstants.SelectedBetCredits, (InitFromStorage(GamingConstants.SelectedBetCredits), true) },
                { GamingConstants.SelectedBetMultiplier, (InitFromStorage(GamingConstants.SelectedBetMultiplier), true) },
                { GamingConstants.SelectedLineCost, (InitFromStorage(GamingConstants.SelectedLineCost), true) },
                { GamingConstants.FudgePay, (configuration.FudgePay?.Enabled ?? false, false) },
                { GamingConstants.ServerControlledPaytables, (configuration.GameRestrictions?.ServerControlledPaytables ?? false, false) },
                { GamingConstants.AdditionalInfoButton, (configuration.AdditionalInfoButton?.Enabled ?? false, false) },
                { GamingConstants.ExcessiveMeterIncrementTestEnabled, (configuration.ExcessiveMeterIncrementTest?.Enabled ?? false, false)},
                { GamingConstants.ExcessiveMeterIncrementTestBanknoteLimit,(configuration.ExcessiveMeterIncrementTest?.BanknoteLimit ?? GamingConstants.ExcessiveMeterIncrementTestDefaultBanknoteLimit, false)},
                { GamingConstants.ExcessiveMeterIncrementTestCoinLimit,(configuration.ExcessiveMeterIncrementTest?.CoinLimit ?? GamingConstants.ExcessiveMeterIncrementTestDefaultCoinLimit, false)},
                { GamingConstants.ExcessiveMeterIncrementTestSoundFilePath,(configuration.ExcessiveMeterIncrementTest?.SoundFilePath ?? string.Empty ,false) },
                { GamingConstants.CycleMaxBet, (configuration.CycleMaxBet?.Enabled ?? false, false) },
                { GamingConstants.AlwaysCombineOutcomesByType, (configuration.AlwaysCombineOutcomesByType?.Enabled ?? true, false) },
                { GamingConstants.HandpayPresentationOverride, (false, false) },
                { GamingConstants.AdditionalInfoGameInProgress, (false, false) },
                { GamingConstants.AwaitingPlayerSelection, (InitFromStorage(GamingConstants.AwaitingPlayerSelection), true) },
                { GamingConstants.ShowTopPickBanners, (InitFromStorage(GamingConstants.ShowTopPickBanners), true) },
                { GamingConstants.ShowPlayerMenuPopup, (InitFromStorage(GamingConstants.ShowPlayerMenuPopup), true) },
                { GamingConstants.PlayerInformationDisplay.Enabled, (playerInformationDisplayOptions?.Enabled ?? false, false) },
                { GamingConstants.PlayerInformationDisplay.RestrictedModeUse, (playerInformationDisplayOptions?.RestrictedModeUse ?? false, false) },
                { GamingConstants.PlayerInformationDisplay.GameRulesScreenEnabled, (playerInformationDisplayOptions?.GameRulesScreen?.Enabled ?? false, false) },
                { GamingConstants.PlayerInformationDisplay.PlayerInformationScreenEnabled, (playerInformationDisplayOptions?.PlayerInformationScreen?.Enabled ?? false, false) },
                { GamingConstants.GameRulesInstructions, (configuration.Instructions?.GameRulesInstructions ?? string.Empty, false) },
                { GamingConstants.UseRngCycling, (configuration.RngCycling?.Enabled ?? false, false) },
                { GamingConstants.ShowPlayerSpeedButtonEnabled, (configuration.ShowPlayerSpeedButton?.Enabled ?? true, false) },
                { GamingConstants.BonusTransferPlaySound, ((object)configuration.BonusTransfer?.PlaySound ?? true, false) },
                { GamingConstants.LaunchGameAfterReboot, (InitFromStorage(GamingConstants.LaunchGameAfterReboot), true) },
                { GamingConstants.DenomSelectionLobby, (configuration.DenomSelectionLobby?.Mode ?? DenomSelectionLobby.Allowed, false) },
                { GamingConstants.DisplayGamePayMessageUseKey, (InitFromStorage(GamingConstants.DisplayGamePayMessageUseKey), true)},
                { GamingConstants.DisplayGamePayMessageFormatKey, (InitFromStorage(GamingConstants.DisplayGamePayMessageFormatKey), true)},
                { GamingConstants.WinTuneCapping, (configuration.WinIncrement?.WinTuneCapping ?? false, false) },
                { GamingConstants.WinIncrementSpeed, (configuration.WinIncrement?.WinIncrementSpeed ?? WinIncrementSpeed.WinAmountOnly, false) },
                { GamingConstants.AutocompleteGameRoundEnabled, (configuration.AutoCompleteGameRound?.Enabled ?? true, false) },
                { GamingConstants.ProgressiveSetupReadonly, (configuration.ProgressiveView?.InitialSetupView?.Readonly ?? false, false) },
                { GamingConstants.ActionOnMaxWinReached, (configuration.MaxWin?.OnMaxWinReached ?? "endgame", false) },
                { GamingConstants.AutoEnableSimpleGames, (configuration.AutoEnableSimpleGames?.Enabled?? true, false) }
            };

            if (!blockExists)
            {
                SetPropertyBlockNotExist(configuration);
            }
        }

        private void SetPropertyBlockNotExist(GamingConfiguration configuration)
        {
                // This is just weird, but because the storage block accessor is typed it will return the default value vs. a null
                // It renders the default passed in to GetProperty useless, since it returns the default type.
                SetProperty(GamingConstants.ShowServiceButton, true);
                SetProperty(GamingConstants.ProgressiveCommitTimeoutMs, GamingConstants.DefaultProgressiveCommitTimeoutMs);
                SetProperty(GamingConstants.ReelStopEnabled, configuration.ReelStop?.Enabled ?? true);
                SetProperty(GamingConstants.ReelSpeedKey, Convert.ToDouble(configuration.ReelSpeed?.Value ?? Convert.ToString(GamingConstants.ReelSpeed)));
                SetProperty(GamingConstants.WagerLimitsMaxTotalWagerKey, configuration.WagerLimits?.MaxTotalWager ?? GamingConstants.WagerLimitsMaxTotalWager);
                SetProperty(GamingConstants.WagerLimitsUseKey, configuration.WagerLimits?.Use ?? GamingConstants.WagerLimitsUse);
                SetProperty(GamingConstants.MaximumGameRoundWinResetWinAmountKey, configuration.MaximumGameRoundWin?.ResetWinAmount ?? GamingConstants.MaximumGameRoundWinResetWinAmount);
                SetProperty(GamingConstants.VolumeLevelShowInHelpScreenKey, configuration.VolumeLevel?.ShowInHelpScreen ?? GamingConstants.VolumeLevelShowInHelpScreen);
                SetProperty(GamingConstants.ServiceUseKey, configuration.Service?.Use ?? GamingConstants.ServiceUse);
                SetProperty(GamingConstants.ClockUseHInDisplayKey, configuration.Clock?.UseHInDisplay ?? GamingConstants.ClockUseHInDisplay);
                SetProperty(GamingConstants.KenoFreeGamesSelectionChangeKey, configuration.KenoFreeGames?.SelectionChange ?? GamingConstants.KenoFreeGamesSelectionChange);
                SetProperty(GamingConstants.KenoFreeGamesAutoPlayKey, configuration.KenoFreeGames?.AutoPlay ?? GamingConstants.KenoFreeGamesAutoPlay);
                SetProperty(GamingConstants.InitialZeroWagerUseKey, configuration.InitialZeroWager?.Use ?? GamingConstants.InitialZeroWagerUse);
                SetProperty(GamingConstants.ChangeLineSelectionAtZeroCreditUseKey, configuration.ChangeLineSelectionAtZeroCredit?.Use ?? GamingConstants.ChangeLineSelectionAtZeroCreditUse);
                SetProperty(GamingConstants.GameDurationUseMarketGameTimeKey, configuration.GameDuration?.UseMarketGameTime ?? GamingConstants.GameDurationUseMarketGameTime);
                SetProperty(GamingConstants.GameLogEnabledKey, configuration.GameLog?.Enabled ?? GamingConstants.GameLogEnabled);
                SetProperty(GamingConstants.GameLogOutcomeDetailsKey, configuration.GameLog?.OutcomeDetails ?? GamingConstants.GameLogOutcomeDetails);
                SetProperty(GamingConstants.AudioAudioChannelsKey, configuration.Audio?.AudioChannels ?? GamingConstants.AudioAudioChannels);
                SetProperty(GamingConstants.FreeSpinClearWinMeterKey, configuration.FreeSpin?.ClearWinMeter ?? GamingConstants.FreeSpinClearWinMeter);
                SetProperty(GamingConstants.WinDestinationKey, configuration.Win?.Destination ?? GamingConstants.WinDestination);
                SetProperty(GamingConstants.ButtonAnimationGoodLuckKey, configuration.ButtonAnimation?.GoodLuck ?? GamingConstants.ButtonAnimationGoodLuck);
                SetProperty(GamingConstants.DisplayGamePayMessageUseKey, configuration.DisplayGamePayMessage?.Use ?? GamingConstants.DisplayGamePayMessageUse);
                SetProperty(GamingConstants.DisplayGamePayMessageFormatKey, configuration.DisplayGamePayMessage?.Format ?? GamingConstants.DisplayGamePayMessageFormat);
                SetProperty(GamingConstants.AutoHoldEnable, configuration.AutoHold?.Enable ?? false);
                SetProperty(GamingConstants.ReplayPauseActive, configuration.ReplayPause?.Active ?? true);
                SetProperty(GamingConstants.GambleWagerLimit, configuration.Gamble?.WagerLimit ?? GamingConstants.DefaultGambleWagerLimit);
                SetProperty(GamingConstants.GambleWinLimit, configuration.Gamble?.WinLimit ?? GamingConstants.DefaultGambleWinLimit);
                SetProperty(GamingConstants.ContinuousPlayMode, (int)(configuration.ContinuousPlaySupport?.Mode ?? PlayMode.Toggle));
                SetProperty(GamingConstants.GameStartMethod, (int)(configuration.PhysicalButtons?.GameStartButtons?.Method ?? GameStartMethodOption.Bet));
                SetProperty(GamingConstants.ShowProgramPinRequired, true);
                SetProperty(GamingConstants.ShowProgramPin, GamingConstants.DefaultShowProgramPin);
                SetProperty(GamingConstants.ShowProgramEnableResetCredits, true);
                SetProperty(GamingConstants.AttractModeEnabled, (configuration.AttractModeOptions?.AttractEnabled ?? true));
                SetProperty(GamingConstants.ProgressiveLobbyIndicatorType,
                    configuration.ProgressiveLobbyIndicator?.Indicator ?? ProgressiveLobbyIndicator.ProgressiveValue);
                SetProperty(GamingConstants.ShowTopPickBanners,true);
                SetProperty(GamingConstants.ShowPlayerMenuPopup, true);
                SetProperty(GamingConstants.LaunchGameAfterReboot, false);
                var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
                var machineSettingsImported = propertiesManager.GetValue(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None);
                if (machineSettingsImported == ImportMachineSettings.None)
                {
                    SetProperty(GamingConstants.IdleTimePeriod, (int)GamingConstants.DefaultIdleTimeoutPeriod.TotalMilliseconds);
                    SetProperty(GamingConstants.GameRoundDurationMs, (configuration.GameRoundDurationMs?.Minimum ?? 0) > GamingConstants.DefaultMinimumGameRoundDurationMs ? configuration.GameRoundDurationMs.Minimum : GamingConstants.DefaultMinimumGameRoundDurationMs);
                    SetProperty(GamingConstants.LockupBehavior, configuration.LockupBehavior?.CashableLockupStrategy ?? CashableLockupStrategy.Allowed);
                    SetProperty(GamingConstants.DefaultAttractSequenceOverridden, false);
                }
                else
                {
                    // The following settings were imported and set to the default property provider since the gaming property provider was not yet loaded
                    // at the time of import, so we set each to their imported values here.
                    SetProperty(GamingConstants.ApplyGameCategorySettings, propertiesManager.GetValue(GamingConstants.ApplyGameCategorySettings, false));
                    SetProperty(GamingConstants.IdleText, propertiesManager.GetValue(GamingConstants.IdleText, string.Empty));
                    SetProperty(GamingConstants.IdleTimePeriod, propertiesManager.GetValue(GamingConstants.IdleTimePeriod, 0));
                    SetProperty(GamingConstants.GameRoundDurationMs, propertiesManager.GetValue(GamingConstants.GameRoundDurationMs, GamingConstants.DefaultMinimumGameRoundDurationMs));
                    SetProperty(GamingConstants.LockupBehavior, propertiesManager.GetValue(GamingConstants.LockupBehavior, CashableLockupStrategy.NotAllowed));
                    SetProperty(GamingConstants.DefaultAttractSequenceOverridden, propertiesManager.GetProperty(GamingConstants.DefaultAttractSequenceOverridden, false));

                    machineSettingsImported |= ImportMachineSettings.GamingPropertiesLoaded;
                    propertiesManager.SetProperty(ApplicationConstants.MachineSettingsImported, machineSettingsImported);
                }
            }

        /// <inheritdoc />
        public ICollection<KeyValuePair<string, object>> GetCollection
            => new List<KeyValuePair<string, object>>(
                _properties.Select(p => new KeyValuePair<string, object>(p.Key, p.Value.Item1)));

        /// <inheritdoc />
        public object GetProperty(string propertyName)
        {
            if (_properties.TryGetValue(propertyName, out var value))
            {
                return value.property;
            }

            var errorMessage = "Unknown game property: " + propertyName;
            Logger.Error(errorMessage);
            throw new UnknownPropertyException(errorMessage);
        }

        /// <inheritdoc />
        public void SetProperty(string propertyName, object propertyValue)
        {
            if (!_properties.TryGetValue(propertyName, out var value))
            {
                var errorMessage = $"Cannot set unknown property: {propertyName}";
                Logger.Error(errorMessage);
                throw new UnknownPropertyException(errorMessage);
            }

            // NOTE:  Not all properties are persisted
            if (value.isPersisted)
            {
                Logger.Debug(
                    $"setting property {propertyName} to {propertyValue}. Type is {propertyValue.GetType()}");
                _persistentStorageAccessor[propertyName] = propertyValue;
            }

            _properties[propertyName] = (propertyValue, value.isPersisted);
        }

        private object InitFromStorage(string propertyName)
        {
            return _persistentStorageAccessor[propertyName];
        }

        private int GetRtpValue(int configRtp, int defaultRtp)
        {
            return configRtp == 0 ? defaultRtp : configRtp;
        }
    }
}
