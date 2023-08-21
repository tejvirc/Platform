namespace Aristocrat.Monaco.Gaming.Runtime.Client
{
    using System;

    public enum RuntimeCondition
    {
        AllowGameRound = 0,
        ResponsibleGamingActive = 1,
        ValidatingCurrency = 2,
        CashingOut = 3,
        PlayTimeExpiredForceCashOut = 4,
        TimeoutImminent = 5,
        InLockup = 6,
        ReplayPause = 7,
        AutoCompleteGameRound = 8,
        AllowSubGameRound = 9,
        DisplayingTimeRemaining = 10,
        AllowReplayResume = 11,
        DisplayingVbdOverlay = 12,
        PendingHandpay = 13,
        StartSystemDrivenAutoPlay = 14,
        FundsTransferring = 15,
        ProgressiveError = 16,
        ServiceRequested = 17,
        PlatformDisableAutoPlay = 18,
        AwaitingPlayerSelection = 19,
        Class2MultipleOutcomeSpins = 20,
        AllowCombinedOutcomes = 21,
        InPlayerMenu = 22,
        InPlayerInfoDisplayMenu = 23,
        InOverlayLockup = 24,
        RequestExitGame = 25,
        GambleFeatureActive = 26,
        InPlatformHelp = 27
    }

    public enum RuntimeRequestState
    {
        BeginGameRound = 0,
        BeginAttract = 1,
        BeginLobby = 2,
        BeginPlatformHelp = 3,
        EndPlatformHelp = 4,
        BeginCelebratoryNoise = 5,
        EndCelebratoryNoise = 6,
        BeginGameAttract = 7,
        EndGameAttract = 8
    }

    public enum RuntimeState
    {
        Initialization = 0,
        Configuration = 1,
        Configured = 2,
        Loading = 3,
        Recovery = 4,
        Replay = 5,
        Pause = 10,
        RenderOnly = 11,
        Running = 12,
        Normal = 12,
        Reconfigure = 13,
        Shutdown = 100,
        Restart = 101,
        Abort = 1000,
        Error = 1001
    }

    [Flags]
    public enum ButtonMask : uint
    {
        None = 0,
        Enabled = 1,
        Lamps = 14,
        Override = 16
    }

    [Flags]
    public enum ButtonState : uint
    {
        NotSet = 0,
        Enabled = 1,
        LightOn = 2,
        BlinkFast = 4,
        BlinkSlow = 8,
        OverridePlatform = 16,
        LockedByPlatform = 128
    }

    public enum ConfigurationTarget
    {
        GameConfiguration = 0,
        MarketConfiguration = 1
    }

    public enum GameRoundEventState
    {
        Primary = 0,
        Selection = 1,
        FreeGame = 2,
        Secondary = 3,
        NonDeterministic = 4,
        Bonus = 5,
        Jackpot = 6,
        Betting = 7,
        GameResults = 8,
        Celebration = 9,
        Present = 10,
        WaitingForPlayerInput = 11,
        AllowCashInDuringPlay = 12,
        Idle = 65535
    }

    public enum GameRoundEventAction
    {
        Invoked = 0,
        Begin = 1,
        Completed = 2,
        Triggered = 4,
        Pending = 8
    }

    public enum PlayMode
    {
        Normal = 0,
        Recovery = 1,
        Replay = 2,
        Demo = 3
    }

    public enum StorageType
    {
        GameLocalSession = 0,
        LocalSession = 1,
        PlayerSession = 2,
        GamePlayerSession = 3
    }

    public enum BeginGameRoundResult
    {
        Success,
        Failed,
        TimedOut
    }
}