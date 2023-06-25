namespace Aristocrat.Monaco.Gaming.Lobby.Store.Lobby;

using System;
using System.Collections.Immutable;
using System.Linq;
using global::Fluxor;

public static class LobbyReducers
{
    [ReducerMethod]
    public static LobbyState Reduce(LobbyState state, StartupAction payload) =>
        state with
        {
            IsMultiLanguage = payload.Configuration.MultiLanguageEnabled,
            IsAgeWarningNeeded = payload.Configuration.DisplayAgeWarning,
            UseGen8IdleModeEdgeLightingOverride = payload.Configuration.EdgeLightingOverrideUseGen8IdleMode,
            HideIdleTextOnCashIn = payload.Configuration.HideIdleTextOnCashIn,
            HasAttractIntroVideo = payload.Configuration.HasAttractIntroVideo,
            ConsecutiveAttractVideos = payload.Configuration.ConsecutiveAttractVideos,
            IsRotateTopImageAfterAttractVideo = payload.Configuration.RotateTopImageAfterAttractVideo is { Length: > 0 },
            RotateTopImageAfterAttractVideo = ImmutableList.CreateRange(payload.Configuration.RotateTopImageAfterAttractVideo ?? Array.Empty<string>()),
            RotateTopImageAfterAttractVideoCount = payload.Configuration.RotateTopImageAfterAttractVideo?.Length ?? 0,
            IsRotateTopperImageAfterAttractVideo = payload.Configuration.RotateTopperImageAfterAttractVideo is { Length: > 0 },
            RotateTopperImageAfterAttractVideo = ImmutableList.CreateRange(payload.Configuration.RotateTopperImageAfterAttractVideo ?? Array.Empty<string>()),
            RotateTopperImageAfterAttractVideoCount = payload.Configuration.RotateTopperImageAfterAttractVideo?.Length ?? 0,
        };

    [ReducerMethod]
    public static LobbyState Reduce(LobbyState state, GamesLoadedAction payload)
    {
        var themesCount = payload.Games.Where(g => g.Enabled).Select(o => o.ThemeId).Distinct().Count();

        return state with
        {
            IsGamesLoaded = true,
            Games = ImmutableList.CreateRange(payload.Games),
            UniqueThemesCount = themesCount,
            IsSingleGame = themesCount <= 1 && state.AllowGameInCharge
        };
    }

    [ReducerMethod]
    public static LobbyState Reduce(LobbyState state, GameMainWindowLoadedAction payload) =>
        state with { GameMainHandle = payload.Handle };

    [ReducerMethod]
    public static LobbyState Reduce(LobbyState state, GameTopWindowLoadedAction payload) =>
        state with { GameTopHandle = payload.Handle };

    [ReducerMethod]
    public static LobbyState Reduce(LobbyState state, GameTopperWindowLoadedAction payload) =>
        state with { GameTopperHandle = payload.Handle };

    [ReducerMethod]
    public static LobbyState Reduce(LobbyState state, GameButtonDeckWindowLoadedAction payload) =>
        state with { GameButtonDeckHandle = payload.Handle };

    [ReducerMethod]
    public static LobbyState Reduce(LobbyState state, LoadGamesAction _) =>
        state with { IsGamesLoaded = false };

    [ReducerMethod]
    public static LobbyState Reduce(LobbyState state, GamePlayEnabledAction payload) =>
        state with { AllowGameAutoLaunch = true };

    [ReducerMethod]
    public static LobbyState Reduce(LobbyState state, SystemEnabledAction payload) =>
        state with
        {
            IsSystemDisabled = payload.IsDisabled,
            IsSystemDisableImmediately = payload.IsDisableImmediately
        };

    [ReducerMethod]
    public static LobbyState Reduce(LobbyState state, SystemDisabledAction payload) =>
        state with
        {
            IsSystemDisabled = payload.IsDisabled,
            IsSystemDisableImmediately = payload.IsDisableImmediately
        };

    [ReducerMethod]
    public static LobbyState Reduce(LobbyState state, UpdateIdleTextAction payload) =>
        state with
        {
            IdleText = payload.Text,
        };

    [ReducerMethod]
    public static LobbyState Reduce(LobbyState state, UpdateBannerDisplayModeAction payload) =>
        state with
        {
            BannerDisplayMode = payload.Mode,
        };

    [ReducerMethod]
    public static LobbyState Reduce(LobbyState state, ToggleTopImageAction payload) =>
        state with
        {
            IsAlternateTopImageActive = !state.IsAlternateTopImageActive,
        };

    [ReducerMethod]
    public static LobbyState Reduce(LobbyState state, ToggleTopperImageAction payload) =>
        state with
        {
            IsAlternateTopperImageActive = !state.IsAlternateTopperImageActive,
        };

    [ReducerMethod]
    public static LobbyState Reduce(LobbyState state, UpdateAttractModeTopperImageIndex payload) =>
        state with
        {
            AttractModeTopperImageIndex = payload.Index,
        };

    [ReducerMethod]
    public static LobbyState Reduce(LobbyState state, UpdateAttractModeTopImageIndex payload) =>
        state with
        {
            AttractModeTopImageIndex = payload.Index,
        };
}
