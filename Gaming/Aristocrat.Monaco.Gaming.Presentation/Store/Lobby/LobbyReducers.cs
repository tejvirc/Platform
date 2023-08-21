namespace Aristocrat.Monaco.Gaming.Presentation.Store.Lobby;

using Fluxor;

public static class LobbyReducers
{
    //[ReducerMethod]
    //public static LobbyState Reduce(LobbyState state, StartupAction action) =>
    //    state with
    //    {
    //        IsMultiLanguage = action.Configuration.MultiLanguageEnabled,
    //        IsAgeWarningNeeded = action.Configuration.DisplayAgeWarning,
    //        UseGen8IdleModeEdgeLightingOverride = action.Configuration.EdgeLightingOverrideUseGen8IdleMode,
    //        HideIdleTextOnCashIn = action.Configuration.HideIdleTextOnCashIn,
    //        HasAttractIntroVideo = action.Configuration.HasAttractIntroVideo,
    //        ConsecutiveAttractVideos = action.Configuration.ConsecutiveAttractVideos,
    //        IsRotateTopImageAfterAttractVideo = action.Configuration.RotateTopImageAfterAttractVideo is { Length: > 0 },
    //        RotateTopImageAfterAttractVideo = ImmutableList.CreateRange(action.Configuration.RotateTopImageAfterAttractVideo ?? Array.Empty<string>()),
    //        RotateTopImageAfterAttractVideoCount = action.Configuration.RotateTopImageAfterAttractVideo?.Length ?? 0,
    //        IsRotateTopperImageAfterAttractVideo = action.Configuration.RotateTopperImageAfterAttractVideo is { Length: > 0 },
    //        RotateTopperImageAfterAttractVideo = ImmutableList.CreateRange(action.Configuration.RotateTopperImageAfterAttractVideo ?? Array.Empty<string>()),
    //        RotateTopperImageAfterAttractVideoCount = action.Configuration.RotateTopperImageAfterAttractVideo?.Length ?? 0,
    //    };


    //[ReducerMethod]
    //public static LobbyState Reduce(LobbyState state, GamePlayEnabledAction action) =>
    //    state with { AllowGameAutoLaunch = true };

    [ReducerMethod]
    public static LobbyState Reduce(LobbyState state, StartupAction action) =>
        state with
        {
            IsStartingUp = true
        };
}
