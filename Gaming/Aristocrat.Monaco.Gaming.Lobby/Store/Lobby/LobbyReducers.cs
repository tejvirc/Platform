namespace Aristocrat.Monaco.Gaming.Lobby.Store.Lobby;

using System.Collections.Immutable;
using Fluxor;

public static class LobbyReducers
{
    [ReducerMethod]
    public static LobbyState Reduce(LobbyState state, GamesLoadedAction payload) =>
        state with
        {
            IsGamesLoaded = true,
            Games = ImmutableList.CreateRange(payload.Games)
        };

    [ReducerMethod]
    public static LobbyState Reduce(LobbyState state, StartupAction payload) =>
        state with { IsMultiLanguage = payload.Configuration.MultiLanguageEnabled };

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
}
