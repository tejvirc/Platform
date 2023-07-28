namespace Aristocrat.Monaco.Gaming.Lobby.Store.Runtime;

using Fluxor;

public static class RuntimeReducers
{
    [ReducerMethod]
    public static RuntimeState Reduce(RuntimeState state, GameMainWindowLoadedAction action) =>
        state with { GameMainHandle = action.Handle };

    [ReducerMethod]
    public static RuntimeState Reduce(RuntimeState state, GameTopWindowLoadedAction action) =>
        state with { GameTopHandle = action.Handle };

    [ReducerMethod]
    public static RuntimeState Reduce(RuntimeState state, GameTopperWindowLoadedAction action) =>
        state with { GameTopperHandle = action.Handle };

    [ReducerMethod]
    public static RuntimeState Reduce(RuntimeState state, GameButtonDeckWindowLoadedAction action) =>
        state with { GameButtonDeckHandle = action.Handle };
}
