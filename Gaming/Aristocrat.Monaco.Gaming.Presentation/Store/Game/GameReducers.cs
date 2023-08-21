namespace Aristocrat.Monaco.Gaming.Presentation.Store.Game;

using Fluxor;

public static class GameReducers
{
    [ReducerMethod]
    public static GameState Reduce(GameState state, GameBottomWindowLoadedAction action) =>
        state with { BottomWindowHandle = action.WindowHandle };

    [ReducerMethod]
    public static GameState Reduce(GameState state, GameTopWindowLoadedAction action) =>
        state with { TopWindowHandle = action.WindowHandle };

    [ReducerMethod]
    public static GameState Reduce(GameState state, GameTopperWindowLoadedAction action) =>
        state with { TopperWindowHandle = action.WindowHandle };

    [ReducerMethod]
    public static GameState Reduce(GameState state, GameButtonDeckWindowLoadedAction action) =>
        state with { ButtonDeckWindowHandle = action.WindowHandle };
}
