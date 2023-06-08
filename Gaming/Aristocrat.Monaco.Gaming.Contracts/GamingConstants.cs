namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Application.Contracts;

    /// <summary>
    ///     Gaming Constants
    /// </summary>
    public static class GamingConstants
    {
        /// <summary>
        ///     Helper definition for handling conversions to/from millicents
        /// </summary>
        public const long Millicents = 1000;

        /// <summary>
        ///     The Millicent divisor.
        /// </summary>
        public const long MillicentDivisor = 100000L;

        /// <summary>
        ///     Path lookup of the games folder
        /// </summary>
        public const string GamesPath = "/Games";

        /// <summary>
        ///     The default value for the MaxRounds flag, setting the maximum number of gamble
        ///     rounds that can be played. A value of 0 means unlimited rounds.
        /// </summary>
        public const int MaxRounds = 0;

        /// <summary>
        ///     The default value for the ReelSpeed flag
        /// </summary>
        public const double ReelSpeed = 2.5333;

        /// <summary>
        ///     The default value for show program pin
        /// </summary>
        public const string DefaultShowProgramPin = "1795";

        /// <summary>
        ///     Path lookup of the runtime folder
        /// </summary>
        public const string RuntimePath = "/Runtime";

        /// <summary>
        ///     Path lookup of the packages folder
        /// </summary>
        public const string PackagesPath = "/Packages";

        /// <summary>
        ///     Gets the runtime executable name
        /// </summary>
        public const string RuntimeHostName = @"RuntimeHost";

        /// <summary>
        ///     Gets the runtime file name
        /// </summary>
        public static readonly string RuntimeHost = $"{RuntimeHostName}.exe";

        /// <summary>
        ///     Server pipe name for Snapp inter-process communications with the runtime
        /// </summary>
        public static readonly string IpcPlatformPipeName = "MonacoPlatformSnapp";

        /// <summary>
        ///     Client pipe name for Snapp inter-process communications with the runtime
        /// </summary>
        public static readonly string IpcRuntimePipeName = "MonacoRuntimeSnapp";

        /// <summary>
        ///     Is there a game running
        /// </summary>
        public const string IsGameRunning = @"GamePlay.IsGameRunning";

        /// <summary>
        ///     The currently selected game
        /// </summary>
        public const string SelectedGameId = @"GamePlay.SelectedDeviceId";

        /// <summary>
        ///     The currently selected denomination
        /// </summary>
        public const string SelectedDenom = @"GamePlay.SelectedDenom";

        /// <summary>
        ///     The currently selected bet option
        /// </summary>
        public const string SelectedBetOption = @"GamePlay.SelectedBetOption";

        /// <summary>
        ///     The currently selected bet option
        /// </summary>
        public const string SelectedBetMultiplier = @"GamePlay.SelectedBetMultiplier";

        /// <summary>
        ///     The currently selected bet option
        /// </summary>
        public const string SelectedLineCost = @"GamePlay.SelectedLineCost";

        /// <summary>
        ///     The currently selected wager category
        /// </summary>
        public const string SelectedWagerCategory = @"GamePlay.SelectedWagerCategory";

        /// <summary>
        ///     The currently selected bet details
        /// </summary>
        public const string SelectedBetDetails = @"GamePlay.SelectedBetDetails";

        /// <summary>
        ///     The currently Game Configuration
        /// </summary>
        public const string GameConfiguration = @"GamePlay.GameConfiguration";

        /// <summary>
        ///     The default limit for the gamble wager setting
        /// </summary>
        public const long DefaultGambleWagerLimit = 50000000;

        /// <summary>
        ///     The default limit for the gamble win setting
        /// </summary>
        public const long DefaultGambleWinLimit = long.MaxValue;

        /// <summary>
        ///     The default minimum delay in milliseconds between each card deal round in Poker games.
        /// </summary>
        public const int CardRevealDelayValue = 0;

        /// <summary>
        ///     The default value for the ChangeBetSelectionAtZeroCredit flag
        ///     If "allowed", change to a new bet option by pressing a bet button when credit is 0.
        /// </summary>
        public const bool ChangeBetSelectionAtZeroCredit = true;

        /// <summary>
        ///     The default value for the IdleCreditDisplayFormat flag
        /// </summary>
        public const string IdleCreditDisplayFormat = "creditOrCurrency";

        /// <summary>
        ///     The default value for the InitialBetOption flag
        /// </summary>
        public const string DefaultInitialBetOption = "min";

        /// <summary>
        ///     The default value for the InitialLineOption flag
        /// </summary>
        public const string DefaultInitialLineOption = "max";

        /// <summary>
        ///     Property key for ShowServiceButton
        /// </summary>
        public const string ShowServiceButton = "GamePlay.ShowServiceButton";

        /// <summary>
        ///     The active games
        /// </summary>
        public const string Games = @"GamePlay.Profiles";

        /// <summary>
        ///     The active and inactive games
        /// </summary>
        public const string AllGames = @"GamePlay.AllProfiles";

        /// <summary>
        ///     The available game combos
        /// </summary>
        public const string GameCombos = @"GamePlay.Combos";

        /// <summary>
        ///     The current idle time period.
        /// </summary>
        public const string IdleTimePeriod = @"Cabinet.IdleTimePeriod";

        /// <summary>
        ///     The game end cash out strategy to use
        /// </summary>
        public const string GameEndCashOutStrategy = @"GamePlay.GameEndCashOutStrategy";

        /// <summary>
        ///     The flag used for keeping failed game outcomes
        /// </summary>
        public const string KeepFailedGameOutcomes = @"GanePlay.KeepFailedGameOutcomes";

        /// <summary>
        ///     The game win max credit cash out strategy to use
        /// </summary>
        public const string GameWinMaxCreditCashOutStrategy = @"GamePlay.GameWinMaxCreditCashOutStrategy";

        /// <summary>
        ///     The game win large win cash out strategy to use
        /// </summary>
        public const string GameWinLargeWinCashOutStrategy = @"GamePlay.GameWinLargeWinCashOutStrategy";

        /// <summary>
        ///     The state change override setting
        /// </summary>
        public const string StateChangeOverride = @"GamePlay.StateChangeOverride";

        /// <summary>
        ///     The permission to enter menu during game
        /// </summary>
        public const string OperatorMenuDisableDuringGame = @"GamePlay.OperatorMenuDisableDuringGame";

        /// <summary>
        ///     The installed game packages
        /// </summary>
        public const string GamePackages = @"Download.Games";

        /// <summary>
        ///     The idle text to be displayed.
        /// </summary>
        public const string IdleText = @"Cabinet.IdleText";

        /// <summary>
        ///     Determines whether or not free games are metered independently
        /// </summary>
        public const string MeterFreeGamesIndependently = @"FreeGames.MeterIndependently";

        /// <summary>
        ///     Determines whether or not warning messages are shown in the game
        /// </summary>
        public const string ShowMessages = @"Messages.ShowMessages";

        /// <summary>
        ///     Determines how messages are removed that are being displayed
        /// </summary>
        public const string MessageClearStyle = @"Messages.MessageClearStyle";

        /// <summary>
        ///     Determines whether or not the minimum bet message must be displayed
        /// </summary>
        public const string MinBetMessageMustDisplay = @"Messages.MinBetMessage.MustDisplay";

        /// <summary>
        ///     Determines whether or not the minimum bet message should be displayed in currency or credit units
        /// </summary>
        public const string MinBetMessageFormat = @"Messages.MinBetMessage.Format";

        /// <summary>
        ///     Determines whether or not the jackpot odds, when greater than 100M to 1, must be displayed on the Help screen
        /// </summary>
        public const string JackpotOddsMustDisplay = @"Messages.JackpotOdds.MustDisplay";

        /// <summary>
        ///     Determines whether is there need to notify the player if reel set is changed.
        /// </summary>
        public const string PlayerNotificationNewReelSetSelected = @"Messages.PlayerNotification.NewReelSetSelected";

        /// <summary>
        ///     Storage key for in-game meters
        /// </summary>
        public const string InGameMeters = "InGameMeters";

        /// <summary>
        ///     The maximum number of game history records.
        /// </summary>
        public const string MaxGameHistory = @"GamePlay.MaxGameHistory";

        /// <summary>
        ///     The default maximum number of game history records to store
        /// </summary>
        public const int DefaultMaxGameHistory = 500;

        /// <summary>
        ///     Indicates whether to allow money in during game play or not.
        /// </summary>
        public const string AllowCashInDuringPlay = @"AllowCashInDuringPlay";

        /// <summary>
        ///     Indicates whether to allow games to be configured that have not been enabled by the host.
        /// </summary>
        public const string AllowEditHostDisabled = @"AllowEditHostDisabled";

        /// <summary>
        ///     The package extension
        /// </summary>
        public const string PackageExtension = @"iso";

        /// <summary>
        ///     The file prefix for the runtime package
        /// </summary>
        public const string RuntimePackagePrefix = @"ATI_Runtime";

        /// <summary>
        ///     The file prefix for the platform package
        /// </summary>
        public const string PlatformPackagePrefix = @"ATI_Platform";

        /// <summary>
        ///     The file prefix for the jurisdiction package
        /// </summary>
        public const string JurisdictionPackagePrefix = @"ATI_Jurisdictions";

        /// <summary>
        ///     Additional runtime args forwarded the bootstrap command line args
        /// </summary>
        public const string RuntimeArgs = @"runtimeArgs";

        /// <summary>
        ///     Default idle time out period
        /// </summary>
        public static readonly TimeSpan DefaultIdleTimeoutPeriod = TimeSpan.FromMinutes(4);

        /// <summary>
        ///     Key to disable the operator menu.
        /// </summary>
        public static readonly Guid OperatorMenuDisableKey = new Guid("{DEBE3C68-F5CC-4AAA-9175-4586B72017FF}");

        /// <summary>
        ///     Key to disable the system when the reels are tilted
        /// </summary>
        public static readonly Guid ReelsTiltedGuid = new("{AD46A871-616A-4034-9FB5-962F8DE15E79}");

        /// <summary>
        ///     Key to disable the system when the reels need to be homed
        /// </summary>
        public static readonly Guid ReelsNeedHomingGuid = new("{9613086D-052A-4FCE-9AA0-B279F8C23993}");

        /// <summary>
        ///     Key to disable the system when the reels are disabled
        /// </summary>
        public static readonly Guid ReelsDisabledGuid = new("{B9029021-106D-419B-961F-1B2799817916}");

        /// <summary>
        ///     Key to disable the system when the reels fail to home
        /// </summary>
        public static readonly Guid ReelsFailedHomingGuid = new("{3BD10514-10BA-4A48-826F-41ADFECFD01D}");

        /// <summary>
        ///     System disable guid for fatal game error.
        /// </summary>
        public static readonly Guid FatalGameErrorGuid = ApplicationConstants.FatalGameErrorGuid;

        /// <summary>
        ///     Key for the show program enabled property.
        /// </summary>
        public const string ShowProgramPinRequired = "ShowProgram.PinRequired";

        /// <summary>
        ///     Key for the show program pin property.
        /// </summary>
        public const string ShowProgramPin = "ShowProgram.Pin";

        /// <summary>
        ///     Key for the show program wager match enabled property.
        /// </summary>
        public const string ShowProgramEnableResetCredits = "ShowProgram.EnableResetCredits";

        /// <summary>
        ///     Key for the selected language property.
        /// </summary>
        public const string SelectedLocaleCode = "Cabinet.SelectedLocaleCode";

        /// <summary>
        ///     Culture code for French-Canada
        /// </summary>
        public const string FrenchCultureCode = "FR-CA";

        /// <summary>
        ///     Culture code for English-US
        /// </summary>
        public const string EnglishCultureCode = "EN-US";

        /// <summary>
        ///     Determines if autocomplete is current in effect
        /// </summary>
        public const string AutocompleteSet = @"GamePlay.AutocompleteSet";

        /// <summary>
        ///     Determines if autocomplete is current in effect
        /// </summary>
        public const string AutocompleteExpired = @"GamePlay.AutocompleteExpired";

        /// <summary>
        ///     Key for the lobby configuration property.
        /// </summary>
        public const string LobbyConfig = "Lobby.Config";

        /// <summary>
        ///     The (minimum) game round duration. A game may not start a new round more often than this.
        /// </summary>
        public const string GameRoundDurationMs = @"Cabinet.GameRoundDurationMs";

        /// <summary>
        ///     Minimum allowed game round duration. This is the minimum an operator can configure for GameRoundDurationMs. This
        ///     may be a jurisdictional requirement, and will be read from the Gaming.config.xml and enforced by the menu.
        /// </summary>
        public static string MinimumGameRoundDuration = @"MinimumGameRoundDuration";

        /// <summary>
        ///     Default minimum game round duration that can be configured
        /// </summary>
        public static readonly int DefaultMinimumGameRoundDurationMs = 100;

        /// <summary>
        ///     Maximum allowed reel duration. This is the maximum an operator can configure for GameRoundDurationMs.
        /// </summary>
        public static string MaximumGameRoundDuration = @"MaximumGameRoundDuration";

        /// <summary>
        ///     Default maximum game round duration that can be configured
        /// </summary>
        public static readonly int DefaultMaximumGameRoundDurationMs = 10000;

        /// <summary>
        ///     Determines if reel stop is enabled
        /// </summary>
        public const string ReelStopEnabled = @"Cabinet.ReelStop";

        /// <summary>
        ///     Determines if reel stop in base game is enabled
        /// </summary>
        public const string ReelStopInBaseGameEnabled = @"Cabinet.ReelStopInBaseGame";

        /// <summary>
        ///     Determines if apply game category settings enabled
        /// </summary>
        public const string ApplyGameCategorySettings = "ApplyGameCategorySettings";

        /// <summary>
        ///     Property Manager key for whether or not the browser processes are being monitored
        /// </summary>
        public const string MonitorBrowserProcess = "Browser.MonitorBrowserProcess";

        /// <summary>
        ///     Property Manager key for maximum CPU usage per browser process
        /// </summary>
        public const string BrowserMaxCpuPerProcess = "Browser.MaxCpuPerProcess";

        /// <summary>
        ///     Property Manager key for maximum CPU usage total for all browser processes
        /// </summary>
        public const string BrowserMaxCpuTotal = "Browser.MaxCpuTotal";

        /// <summary>
        ///     Property Manager key for maximum memory usage per browser process
        /// </summary>
        public const string BrowserMaxMemoryPerProcess = "Browser.MaxMemoryPerProcess";

        /// <summary>
        ///     Storage key for Bonus Key
        /// </summary>
        public const string BonusKey = "BonusKey";

        /// <summary>
        ///     Property manager key for cashable lockup strategy. Strategy is either Allowed, NotAllowed, or ForceCashout
        /// </summary>
        public const string LockupBehavior = "CashableLockupStrategy";

        /// <summary>
        ///     The percentage conversion.
        /// </summary>
        public const long PercentageConversion = 100000000L;

        /// <summary>
        ///     The maximum value for Shared Level Detail's Increment Rate.
        /// </summary>
        public const decimal MaxSharedLevelDetailIncrementRate = 9999.99M;

        /// <summary>
        ///     The default progressive commit timeout in milliseconds
        /// </summary>
        public const int DefaultProgressiveCommitTimeoutMs = 60000;

        /// <summary>
        ///     Property manager key for the progressive timeout in milliseconds
        /// </summary>
        public const string ProgressiveCommitTimeoutMs = @"Progressive.CommitTimeoutMs";

        /// <summary>
        ///     Property manager key for the progressive lobby indicator type
        /// </summary>
        public const string ProgressiveLobbyIndicatorType = @"Progressive.LobbyIndicator";

        /// <summary>
        ///     Property manager key for CensorshipEnforced.
        /// </summary>
        public const string CensorshipEnforced = @"GamePreferences.Censorship.Enforced";

        /// <summary>
        ///     Property manager key for CensorshipEditable.
        /// </summary>
        public const string CensorshipEditable = @"GamePreferences.Censorship.Editable";

        /// <summary>
        ///     Determines if auto hold is enabled
        /// </summary>
        public const string AutoHoldEnable = @"AutoHold.Enable";

        /// <summary>
        ///     Determines if auto hold is configurable
        /// </summary>
        public const string AutoHoldConfigurable = @"AutoHold.Configurable";

        /// <summary>
        ///     Property manager key for DefaultCreditDisplay
        /// </summary>
        public const string DefaultCreditDisplayFormat = @"DefaultCreditDisplay.Format";

        /// <summary>
        ///     Indicates whether Lockup Behavior is configurable
        /// </summary>
        public static string LockupBehaviorConfigurable = "CashableLockupStrategyConfigurable";

        /// <summary>
        ///     Whether Play On From Gamble Available.
        /// </summary>
        public const string PlayOnFromGambleAvailable = @"GamePlay.PlayOnFromGambleAvailable";

        /// <summary>
        ///     Whether Play On From Present Wins.
        /// </summary>
        public const string PlayOnFromPresentWins = @"GamePlay.PlayOnFromPresentWins";

        /// <summary>
        ///     Whether gamble is allowed or not.
        /// </summary>
        public const string GambleAllowed = @"GamePlay.Gamble.Allowed";

        /// <summary>
        ///     Whether gamble is skipped on jackpot hit or not
        /// </summary>
        public const string GambleSkipByJackpotHit = @"GamePlay.Gamble.SkipByJackpotHit";

        /// <summary>
        ///     Whether gamble Game Rules will show dynamic settings
        /// </summary>
        public const string ShowGambleDynamicHelp = @"GamePlay.Gamble.ShowGambleDynamicHelp";

        /// <summary>
        ///     Whether gamble is enabled by default or not.
        /// </summary>
        public const string GambleEnabled = @"GamePlay.Gamble.Enabled";

        /// <summary>
        ///     The Maximum gamble wager amount
        /// </summary>
        public const string GambleWagerLimit = @"GamePlay.Gamble.WagerLimit";

        /// <summary>
        ///     The Maximum gamble wager amount
        /// </summary>
        public const string GambleWinLimit = @"GamePlay.Gamble.WinLimit";

        /// <summary>
        ///     The flag to determine whether gamble win limit should be used
        /// </summary>
        public const string UseGambleWinLimit = @"GamePlay.Gamble.UseWinLimit";

        /// <summary>
        ///     The Maximum gamble wager amount is configurable or not
        /// </summary>
        public const string GambleWagerLimitConfigurable = @"GamePlay.Gamble.WagerLimitConfigurable";

        /// <summary>
        ///     The Maximum gamble win amount is configurable or not
        /// </summary>
        public const string GambleWinLimitConfigurable = @"GamePlay.Gamble.WinLimitConfigurable";

        /// <summary>
        ///     Whether let it ride is allowed or not.
        /// </summary>
        public const string LetItRideAllowed = @"GamePlay.LetItRide.Allowed";

        /// <summary>
        ///     Whether let it ride is enabled by default or not.
        /// </summary>
        public const string LetItRideEnabled = @"GamePlay.LetItRide.Enabled";

        /// <summary>
        ///     Continuous play mode
        /// </summary>
        public const string ContinuousPlayMode = @"GamePlay.ContinuousPlaySupport.Mode";

        /// <summary>
        ///     Continuous play mode
        /// </summary>
        public const string ContinuousPlayModeConfigurable = @"GamePlay.ContinuousPlaySupport.Configurable";

        /// <summary>
        ///     Buttons that start continuous play mode
        /// </summary>
        public const string ContinuousPlayModeButtonsToUse = @"GamePlay.ContinuousPlaySupport.ButtonsToUse";

        /// <summary>
        ///     When enabled, for each enabled progressive level, their name and ceiling will be shown to the user in the game rules screen.
        /// </summary>
        public const string DisplayProgressiveCeilingMessage = @"GamePlay.DisplayProgressiveCeilingMessage";

        /// <summary>
        ///     When enabled, it will inform the player that they can press the big button to stop the reel.
        /// </summary>
        public const string DisplayStopReelMessage = @"GamePlay.DisplayStopReelMessage";

        /// <summary>
        ///     When enabled, it will retain last round result for roulette games.
        /// </summary>
        public const string RetainLastRoundResult = @"GamePlay.RetainLastRoundResult";

        /// <summary>
        ///     When enabled, it will reset win meter on bet line denom changed.
        /// </summary>
        public const string WinMeterResetOnBetLineDenomChanged = @"GamePlay.WinMeterResetOnBetLineDenomChanged";

        /// <summary>
        ///     When enabled, it will reset win meter on bet line changed.
        /// </summary>
        public const string WinMeterResetOnBetLineChanged = @"GamePlay.WinMeterResetOnBetLineChanged";

        /// <summary>
        ///     When enabled, it will reset win meter on denom changed.
        /// </summary>
        public const string WinMeterResetOnDenomChanged = @"GamePlay.WinMeterResetOnDenomChanged";

        /// <summary>
        ///     When enabled, game round events will be persisted.
        /// </summary>
        public const string KeepGameRoundEvents = @"GamePlay.KeepGameRoundEvents";

        /// <summary>
        ///     When enabled, game round meter snapshots will be persisted.
        /// </summary>
        public const string KeepGameRoundMeterSnapshots = @"GamePlay.KeepGameRoundMeterSnapshots";

        /// <summary>
        ///     Minimum RTP for Any game
        /// </summary>
        public const string AnyGameMinimumReturnToPlayer = @"GameRestrictions.ReturnToPlayerLimits.Minimum.Any";

        /// <summary>
        ///     Maximum RTP for Any game
        /// </summary>
        public const string AnyGameMaximumReturnToPlayer = @"GameRestrictions.ReturnToPlayerLimits.Maximum.Any";

        /// <summary>
        ///     Minimum RTP for the Slot game
        /// </summary>
        public const string SlotMinimumReturnToPlayer = @"GameRestrictions.ReturnToPlayerLimits.Minimum.Slot";

        /// <summary>
        ///     Maximum RTP for the Slot game
        /// </summary>
        public const string SlotMaximumReturnToPlayer = @"GameRestrictions.ReturnToPlayerLimits.Maximum.Slot";

        /// <summary>
        ///     Minimum RTP for the Poker game
        /// </summary>
        public const string PokerMinimumReturnToPlayer = @"GameRestrictions.ReturnToPlayerLimits.Minimum.Poker";

        /// <summary>
        ///     Maximum RTP for the Poker game
        /// </summary>
        public const string PokerMaximumReturnToPlayer = @"GameRestrictions.ReturnToPlayerLimits.Maximum.Poker";

        /// <summary>
        ///     Minimum RTP for the Keno game
        /// </summary>
        public const string KenoMinimumReturnToPlayer = @"GameRestrictions.ReturnToPlayerLimits.Minimum.Keno";

        /// <summary>
        ///     Maximum RTP for the Keno game
        /// </summary>
        public const string KenoMaximumReturnToPlayer = @"GameRestrictions.ReturnToPlayerLimits.Maximum.Keno";

        /// <summary>
        ///     Minimum RTP for the Blackjack game
        /// </summary>
        public const string BlackjackMinimumReturnToPlayer = @"GameRestrictions.ReturnToPlayerLimits.Minimum.Blackjack";

        /// <summary>
        ///     Maximum RTP for the Blackjack game
        /// </summary>
        public const string BlackjackMaximumReturnToPlayer = @"GameRestrictions.ReturnToPlayerLimits.Maximum.Blackjack";

        /// <summary>
        ///     Minimum RTP for the Roulette game
        /// </summary>
        public const string RouletteMinimumReturnToPlayer = @"GameRestrictions.ReturnToPlayerLimits.Minimum.Roulette";

        /// <summary>
        ///     Maximum RTP for the Roulette game
        /// </summary>
        public const string RouletteMaximumReturnToPlayer = @"GameRestrictions.ReturnToPlayerLimits.Maximum.Roulette";

        /// <summary>
        ///     Provides restriction status for the Slot game type
        /// </summary>
        public const string AllowSlotGames = @"GameRestrictions.RestrictedGameTypes.Slots";

        /// <summary>
        ///     Provides restriction status for the Poker game type
        /// </summary>
        public const string AllowPokerGames = @"GameRestrictions.RestrictedGameTypes.Poker";

        /// <summary>
        ///     Provides restriction status for the Keno game type
        /// </summary>
        public const string AllowKenoGames = @"GameRestrictions.RestrictedGameTypes.Keno";

        /// <summary>
        ///     Provides restriction status for the Blackjack game type
        /// </summary>
        public const string AllowBlackjackGames = @"GameRestrictions.RestrictedGameTypes.Blackjack";

        /// <summary>
        ///     Provides restriction status for the Roulette game type
        /// </summary>
        public const string AllowRouletteGames = @"GameRestrictions.RestrictedGameTypes.Roulette";

        /// <summary>
        ///     Provides restrictions for the Progressive types
        /// </summary>
        public const string RestrictedProgressiveTypes = @"GameRestrictions.RestrictedProgressiveTypes";

        /// <summary>
        ///     Flag to enable/disable the inclusion of the link progressive increment RTP for in the RTP check for Slot games
        /// </summary>
        public const string SlotsIncludeLinkProgressiveIncrementRtp = @"GameRestrictions.IncludeLinkProgressiveIncrementRTP.Slots";

        /// <summary>
        ///     Flag to enable/disable the inclusion of the link progressive increment RTP for in the RTP check for Poker games
        /// </summary>
        public const string PokerIncludeLinkProgressiveIncrementRtp = @"GameRestrictions.IncludeLinkProgressiveIncrementRTP.Poker";

        /// <summary>
        ///     Flag to enable/disable the inclusion of the link progressive increment RTP for in the RTP check for Keno games
        /// </summary>
        public const string KenoIncludeLinkProgressiveIncrementRtp = @"GameRestrictions.IncludeLinkProgressiveIncrementRTP.Keno";

        /// <summary>
        ///     Flag to enable/disable the inclusion of the link progressive increment RTP for in the RTP check for Blackjack games
        /// </summary>
        public const string BlackjackIncludeLinkProgressiveIncrementRtp = @"GameRestrictions.IncludeLinkProgressiveIncrementRTP.Blackjack";

        /// <summary>
        ///     Flag to enable/disable the inclusion of the link progressive increment RTP for in the RTP check for Roulette games
        /// </summary>
        public const string RouletteIncludeLinkProgressiveIncrementRtp = @"GameRestrictions.IncludeLinkProgressiveIncrementRTP.Roulette";

        /// <summary>
        ///     Flag to enable/disable the inclusion of the standalone progressive increment RTP for in the RTP check for Slot games
        /// </summary>
        public const string SlotsIncludeStandaloneProgressiveIncrementRtp = @"GameRestrictions.IncludeStandaloneProgressiveIncrementRTP.Slots";

        /// <summary>
        ///     Flag to enable/disable the inclusion of the standalone progressive increment RTP for in the RTP check for Poker games
        /// </summary>
        public const string PokerIncludeStandaloneProgressiveIncrementRtp = @"GameRestrictions.IncludeStandaloneProgressiveIncrementRTP.Poker";

        /// <summary>
        ///     Flag to enable/disable the inclusion of the standalone progressive increment RTP for in the RTP check for Keno games
        /// </summary>
        public const string KenoIncludeStandaloneProgressiveIncrementRtp = @"GameRestrictions.IncludeStandaloneProgressiveIncrementRTP.Keno";

        /// <summary>
        ///     Flag to enable/disable the inclusion of the standalone progressive increment RTP for in the RTP check for Blackjack games
        /// </summary>
        public const string BlackjackIncludeStandaloneProgressiveIncrementRtp = @"GameRestrictions.IncludeStandaloneProgressiveIncrementRTP.Blackjack";

        /// <summary>
        ///     Flag to enable/disable the inclusion of the standalone progressive increment RTP for in the RTP check for Roulette games
        /// </summary>
        public const string RouletteIncludeStandaloneProgressiveIncrementRtp = @"GameRestrictions.IncludeStandaloneProgressiveIncrementRTP.Roulette";

        /// <summary>
        ///     Provides hand information for poker games
        /// </summary>
        public const string PokerHandInformation = @"PokerHandInformation";

        /// <summary>
        ///    Represents the inGameDisplay runtime flag that controls switching back and forth between credits and currency during game play
        /// </summary>
        public const string InGameDisplayFormat = @"InGameDisplay.DisplayFormat";

        /// <summary>
        ///     flag to tell if the auto play is allowed or not.
        /// </summary>
        public const string AutoPlayAllowed = @"AutoPlay.Allowed";

        /// <summary>
        ///     flag to indicate if auto play is currently active
        /// </summary>
        public const string AutoPlayActive = "AutoPlayActive";

        /// <summary>
        ///     Determines whether EGM will send the cashout button press event to host in case credit is zero
        /// </summary>
        public const string ReportCashoutButtonPressWithZeroCredit = @"CashoutButton.ReportToHostWithZeroCredit";

        /// <summary>
        ///     Determines whether or not the Voucher Issued message is displayed in game
        /// </summary>
        public const string DisplayVoucherIssuedMessage = @"Messages.VoucherIssued.Display";

        /// <summary>
        ///     Determines if replay pause is enabled
        /// </summary>
        public const string ReplayPauseEnable = @"ReplayPause.Enable";

        /// <summary>
        ///     Determines if replay pause is active (on)
        /// </summary>
        public const string ReplayPauseActive = @"ReplayPause.Active";

        /// <summary>
        ///     Determines "Way to Start game"  (say bet or line etc)
        /// </summary>
        public const string GameStartMethod = @"GameStartMethod";

        /// <summary>
        ///     Determines "Way to Start game" that can be changed by the game (say bet or line etc)
        /// </summary>
        public const string GameConfigurableStartMethods = @"GameConfigurableStartMethod";

        /// <summary>
        ///     Determines whether "Way to Start game" option is configurable or not
        /// </summary>
        public const string GameStartMethodConfigurable = @"GameStartMethod.Configurable";

        /// <summary>
        ///     Determines whether "Way to Start game" option is visible or not
        ///     Note : In some cases the way to start game is just play button. In those cases, the setting should be hidden
        ///     And a fixed value for the settings is used.
        /// </summary>
        public const string GameStartMethodSettingVisible = @"GameStartMethod.SettingVisible";

        /// <summary>
        ///     Property manager key for OperatorMenuPerformancePageSelectedGameType.
        /// </summary>
        public const string OperatorMenuPerformancePageSelectedGameType = "OperatorMenu.PerformancePage.SelectedGameType";

        /// <summary>
        ///     Property manager key for OperatorMenuPerformancePageSelectedGameThemes.
        /// </summary>
        public const string OperatorMenuPerformancePageDeselectedGameThemes = "OperatorMenu.PerformancePage.DeselectedGameThemes";

        /// <summary>
        ///     Property manager key for OperatorMenuPerformancePageHideNeverActive.
        /// </summary>
        public const string OperatorMenuPerformancePageHideNeverActive = "OperatorMenu.PerformancePage.HideNeverActive";

        /// <summary>
        ///     Property manager key for OperatorMenuPerformancePageHidePreviouslyActive.
        /// </summary>
        public const string OperatorMenuPerformancePageHidePreviouslyActive = "OperatorMenu.PerformancePage.HidePreviouslyActive";

        /// <summary>
        ///     Property manager key for OperatorMenuPerformancePageSortMemberPath.
        /// </summary>
        public const string OperatorMenuPerformancePageSortMemberPath = "OperatorMenu.PerformancePage.SortMemberPath";

        /// <summary>
        ///     Property manager key for OperatorMenuPerformancePageSortDirection.
        /// </summary>
        public const string OperatorMenuPerformancePageSortDirection = "OperatorMenu.PerformancePage.SortDirection";

        /// <summary>
        ///     Property manager key for ResetGamesPlayedSinceDoorClosedBelly.
        /// </summary>
        public const string ResetGamesPlayedSinceDoorClosedBelly = "ResetGamesPlayedSinceDoorClosed.Belly";

        /// <summary>
        ///     Property manager key for ReelStopConfigurable.
        /// </summary>
        public const string ReelStopConfigurable = "ReelStopConfigurable";

        /// <summary>
        ///     Property manager key for Attendant service timeout support.
        /// </summary>
        public const string AttendantServiceTimeoutSupportEnabled = "AttendantServiceTimeoutSupport.Enabled";

        /// <summary>
        ///     Property manager key for Attendant service timeout duration in milliseconds.
        /// </summary>
        public const string AttendantServiceTimeoutInMilliseconds = "AttendantServiceTimeoutSupport.TimeoutInMilliseconds";

        /// <summary>
        ///     Property Key for Game Configuration Initial Configuration Complete.
        /// </summary>
        public const string OperatorMenuGameConfigurationInitialConfigComplete = @"OperatorMenu.GameConfiguration.InitialConfigComplete";

        /// <summary>
        ///     Property Key for Button layout : Bet buttons on bottom
        /// </summary>
        public const string ButtonLayoutBetButtonsOnBottom = @"ButtonLayoutOptions.PhysicalButtons.BetButtonsOnBottom";

        /// <summary>
        ///     Property Key for Physical button layout for Collect button
        /// </summary>
        public const string ButtonLayoutPhysicalButtonCollect = @"ButtonLayoutOptions.PhysicalButtons.Collect";

        /// <summary>
        ///     Property Key for Physical button layout for Collect button optional
        /// </summary>
        public const string ButtonLayoutPhysicalButtonCollectOptional = @"ButtonLayoutOptions.PhysicalButtons.CollectOptional";

        /// <summary>
        ///     Property Key for Physical button layout for Gamble button
        /// </summary>
        public const string ButtonLayoutPhysicalButtonGamble = @"ButtonLayoutOptions.PhysicalButtons.Gamble";

        /// <summary>
        ///     Property Key for Physical button layout for Gamble button optional
        /// </summary>
        public const string ButtonLayoutPhysicalButtonGambleOptional = @"ButtonLayoutOptions.PhysicalButtons.GambleOptional";

        /// <summary>
        ///     Property Key for Physical button layout for Service button
        /// </summary>
        public const string ButtonLayoutPhysicalButtonService = @"ButtonLayoutOptions.PhysicalButtons.Service";

        /// <summary>
        ///     Property Key for Physical button layout for Service button optional
        /// </summary>
        public const string ButtonLayoutPhysicalButtonServiceOptional = @"ButtonLayoutOptions.PhysicalButtons.ServiceOptional";

        /// <summary>
        ///     Property Key for Physical button layout for TakeWin button
        /// </summary>
        public const string ButtonLayoutPhysicalButtonTakeWin = @"ButtonLayoutOptions.PhysicalButtons.TakeWin";

        /// <summary>
        ///     Property Key for Physical button layout for TakeWin button optional
        /// </summary>
        public const string ButtonLayoutPhysicalButtonTakeWinOptional = @"ButtonLayoutOptions.PhysicalButtons.TakeWinOptional";

        /// <summary>
        ///     Property Key for whether attract is enabled
        /// </summary>
        public const string AttractModeEnabled = @"AttractModeOptions.AttractEnabled";

        /// <summary>
        ///     Property Key for whether default attract is overriden
        /// </summary>
        public const string DefaultAttractSequenceOverridden = @"AttractModeOptions.DefaultAttractOverriden";

        /// <summary>
        ///     Property Key for whether Slot (games) are selected for attract
        /// </summary>
        public const string SlotAttractSelected = @"AttractModeOptions.SlotAttractSelected";

        /// <summary>
        ///     Property Key for whether Keno (games) are selected for attract
        /// </summary>
        public const string KenoAttractSelected = @"AttractModeOptions.KenoAttractSelected";

        /// <summary>
        ///     Property Key for whether Poker (games) are selected for attract
        /// </summary>
        public const string PokerAttractSelected = @"AttractModeOptions.PokerAttractSelected";

        /// <summary>
        ///     Property Key for whether Blackjack (games) are selected for attract
        /// </summary>
        public const string BlackjackAttractSelected = @"AttractModeOptions.BlackjackAttractSelected";

        /// <summary>
        ///     Property Key for whether Roulette (games) are selected for attract
        /// </summary>
        public const string RouletteAttractSelected = @"AttractModeOptions.RouletteAttractSelected";

        /// <summary>
        ///     Property Key for overriden game type text
        /// </summary>
        public const string OverridenSlotGameTypeText = @"OverridenGameTypeText.SlotGameTypeText";

        /// <summary>
        ///     Barkeeper reward levels
        /// </summary>
        public const string BarkeeperRewardLevels = @"BarkeeperRewardLevels";

        /// <summary>
        ///     Provides the barkeeper active coin in reward.
        /// </summary>
        public const string BarkeeperActiveCoinInReward = @"ActiveCoinInReward";

        /// <summary>
        ///     Provides barkeeper cash in.
        /// </summary>
        public const string BarkeeperCashIn = @"CashIn";

        /// <summary>
        ///     Provides barkeeper coin in.
        /// </summary>
        public const string BarkeeperCoinIn = @"CoinIn";

        /// <summary>
        ///     Provides the barkeeper cash in idle brightness.
        /// </summary>
        public const int BarkeeperCashInIdleBrightness = 50;

        /// <summary>
        ///     Provides the barkeeper default brightness.
        /// </summary>
        public const int BarkeeperDefaultBrightness = 100;

        /// <summary>
        ///     Provides BarKeeper Rate Of Play session Elapsed Milliseconds.
        /// </summary>
        public const string BarkeeperRateOfPlayElapsedMilliseconds = @"RateOfPlayElapsedMilliseconds";

        /// <summary>
        ///     Allows for the lobby to be bypassed and directly load a single game
        /// </summary>
        public const string AllowGameInCharge = @"AllowGameInCharge";

        /// <summary>
        ///     Indicates if reels start spinning immediately after play button press.
        ///     This is needed for games where outcome comes from server.
        /// </summary>
        public const string ImmediateReelSpin = @"ImmediateReelSpin";

        /// <summary>
        ///     Determines how the progressive pool should be created.
        /// </summary>
        public const string ProgressivePoolCreationType = @"Progressive.PoolCreationType";

        /// <summary>
        ///     Bet credits for currently selected game
        /// </summary>
        public const string SelectedBetCredits = @"GamePlay.SelectedBetCredits";

        /// <summary>
        ///     "true" / "false" to indicate if we can use fudge pay feature for extra winning.
        /// </summary>
        public const string FudgePay = @"FudgePay";

        /// <summary>
        ///
        /// </summary>
        public const string ServerControlledPaytables = @"ServerControlledPaytables";

        /// <summary>
        ///     "true" / "false" to indicate whether we display the Additional Info button.
        /// </summary>
        public const string AdditionalInfoButton = @"AdditionalInfoButton";

        /// <summary>
        ///     "true" / "false" to indicate whether we need to do meter increment test after each game ends
        /// </summary>
        public const string ExcessiveMeterIncrementTestEnabled = @"ExcessiveMeterIncrementTest.Enabled";

        /// <summary>
        ///    the banknote limit for meter increment
        /// </summary>
        public const string ExcessiveMeterIncrementTestBanknoteLimit = @"ExcessiveMeterIncrementTest.BanknoteLimit";

        /// <summary>
        ///     default value for excessive meter increment test banknote limit
        /// </summary>
        public const long ExcessiveMeterIncrementTestDefaultBanknoteLimit = long.MaxValue;

        /// <summary>
        ///    the banknote limit for meter increment
        /// </summary>
        public const string ExcessiveMeterIncrementTestCoinLimit = @"ExcessiveMeterIncrementTest.CoinLimit";

        /// <summary>
        ///     default value for excessive meter increment test banknote limit
        /// </summary>
        public const long ExcessiveMeterIncrementTestDefaultCoinLimit = long.MaxValue;

        /// <summary> ExcessiveMeterIncrementTest SoundFilePath, if this file is present then Audio is played when ExcessiveMeterIncrementTest is satisfied.</summary>
        public const string ExcessiveMeterIncrementTestSoundFilePath = "ExcessiveMeterIncrementTest.SoundFilePath";

        /// <summary>
        ///     "true" / "false" to indicate whether max bet should cycle to min bet when pressing bet up button.
        /// </summary>
        public const string CycleMaxBet = @"CycleMaxBet";

        /// <summary>
        ///     "true" / "false" to indicate whether game outcomes should be combined (coalesce wins).
        /// </summary>
        public const string AlwaysCombineOutcomesByType = @"AlwaysCombineOutcomesByType";

        /// <summary>
        ///     true if Handpay Presentation banner needs to be override
        /// </summary>
        public const string HandpayPresentationOverride = @"HandpayPresentationOverride";

        /// <summary>
        ///     Topper lobby/game window title, when title bar is visible.
        /// </summary>
        public const string TopperWindowTitle = @"Topper Lobby";

        /// <summary>
        ///     Top lobby/game window title, when title bar is visible.
        /// </summary>
        public const string TopWindowTitle = @"Top Lobby";

        /// <summary>
        ///     Main lobby/game window title, when title bar is visible.
        /// </summary>
        public const string MainWindowTitle = @"Shell";

        /// <summary>
        ///     VBD window title, when title bar is visible.
        /// </summary>
        public const string VbdWindowTitle = @"VirtualButtonDeckView";

        /// <summary>
        ///     True value of this flag will indicate the game can be initiated even if lockup with priority:Normal is there
        /// </summary>
        public const string AdditionalInfoGameInProgress = @"AdditionalInfoGameInProgress";


        /// <summary>
        ///     Flag will indicate the value of AwaitingPlayerSelection sent to runtime
        /// </summary>
        public const string AwaitingPlayerSelection = @"AwaitingPlayerSelection";

        /// <summary>
        ///     The maximum length a pin number can be when reserving a machine
        /// </summary>
        public const int ReserveMachinePinLength = 4;

        /// <summary>
        ///     The inactivity timeout for the player menu popup
        /// </summary>
        public const int PlayerMenuPopupTimeoutMilliseconds = 30000;

        /// <summary>
        ///     The minimum open/close time before again opening/closing the player menu popup
        /// </summary>
        public const int PlayerMenuPopupOpenCloseDelayMilliseconds = 1000;

        /// <summary>
        ///     Property key for ShowTopPickBanners
        /// </summary>
        public const string ShowTopPickBanners = "GamePlay.ShowTopPickBanners";

        /// <summary>
        ///     The inactivity timeout for the PIN confirm or exit reserve machine display
        /// </summary>
        public const int ReserveMachinePinDisplayTimeoutMilliseconds = 30000;

        /// <summary>
        ///     The number of times a wrong PIN can be entered before forcing a wait
        /// </summary>
        public const int ReserveMachineMaxPinEntryAttempts = 5;

        /// <summary>
        ///     The amount of time that must be waited after X failed PIN entry attempts
        /// </summary>
        public const int ReserveMachineIncorrectPinWaitTimeSeconds = 60;

        /// <summary>
        ///     Flag will control the showing of the Player Menu Popup when pressed on the game UPI
        /// </summary>
        public const string ShowPlayerMenuPopup = @"GamePlay.ShowPlayerMenuPopup";

        /// <summary>
        ///     Flag will control the background cycling of the RNG by the platform
        /// </summary>
        public const string UseRngCycling = @"RngCycling.Enabled";

        /// <summary>
        ///     Flag will control whether the player speed button will be enabled or disabled
        /// </summary>
        public const string ShowPlayerSpeedButtonEnabled = @"ShowPlayerSpeedButton.Enabled";

        /// <summary>
        ///     Determines if platform need to play the sound on bonus transfer.
        /// </summary>
        public const string BonusTransferPlaySound = @"BonusTransfer.PlaySound";

        /// <summary>
        ///     Requesting game exit for the game menu button when using a multi-game setup
        /// </summary>
        public const string RequestExitGame = "RequestExitGame";

        /// <summary>
        ///     Set sub game when using a auto launch single game setup
        /// </summary>
        public const string SetSubGame = "SetSubGame";

        /// <summary>
        ///     Is there a need to launch a game after reboot
        /// </summary>
        public const string LaunchGameAfterReboot = "GamePlay.LaunchGameAfterReboot";

        /// <summary>
        ///     Whether denomination selection lobby is required/allowed
        /// </summary>
        public const string DenomSelectionLobby = "GamingConfiguration.DenomSelectionLobby.Mode";

        /// <summary>
        ///     Command Line argument for enabling slow recovery
        /// </summary>
        public const string UseSlowRecovery = "UseSlowRecovery";

        /// <summary>
        ///     encapsulate Player Information Display options
        /// </summary>
        public static class PlayerInformationDisplay
        {
            /// <summary>
            ///     The inactivity timeout for the player information display
            /// </summary>
            public const int DefaultTimeoutMilliseconds = 30000;

            /// <summary>
            ///     Inactivity timeout for the player information display
            /// </summary>
            public const string TimeoutMilliseconds = @"PlayerInformationDisplay.TimeoutMilliseconds";

            /// <summary>
            ///     Whether Player Information Display is enabled
            /// </summary>
            public const string Enabled = @"PlayerInformationDisplay.Enabled";

            /// <summary>
            ///     Whether Player Information Display supports restricted user mode
            /// </summary>
            public const string RestrictedModeUse = @"PlayerInformationDisplay.RestrictedModeUse";

            /// <summary>
            ///     Whether Game Rules screen enabled
            /// </summary>
            public const string GameRulesScreenEnabled = @"PlayerInformationDisplay.GameRulesScreen.Enabled";

            /// <summary>
            ///     Whether Player Information screen enabled
            /// </summary>
            public const string PlayerInformationScreenEnabled = @"PlayerInformationDisplay.PlayerInformationScreen.Enabled";
        }
    }
}
