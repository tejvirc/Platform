namespace Aristocrat.Monaco.Gaming.Lobby.Store.Runtime;

using Fluxor;

public static class RuntimeReducers
{
    [ReducerMethod]
    public static RuntimeState Reduce(RuntimeState state, GameMainWindowLoadedAction payload) =>
        state with { GameMainHandle = payload.Handle };

    [ReducerMethod]
    public static RuntimeState Reduce(RuntimeState state, GameTopWindowLoadedAction payload) =>
        state with { GameTopHandle = payload.Handle };

    [ReducerMethod]
    public static RuntimeState Reduce(RuntimeState state, GameTopperWindowLoadedAction payload) =>
        state with { GameTopperHandle = payload.Handle };

    [ReducerMethod]
    public static RuntimeState Reduce(RuntimeState state, GameButtonDeckWindowLoadedAction payload) =>
        state with { GameButtonDeckHandle = payload.Handle };
}
