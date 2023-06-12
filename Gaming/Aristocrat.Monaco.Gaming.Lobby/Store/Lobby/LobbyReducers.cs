namespace Aristocrat.Monaco.Gaming.Lobby.Store.Lobby;

using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using global::Fluxor;

public static class LobbyReducers
{
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
    public static LobbyState Reduce(LobbyState state, StartupAction payload) =>
        state with
        {
            IsMultiLanguage = payload.Configuration.MultiLanguageEnabled,
            IsAgeWarningNeeded = payload.Configuration.DisplayAgeWarning
        };

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
}
