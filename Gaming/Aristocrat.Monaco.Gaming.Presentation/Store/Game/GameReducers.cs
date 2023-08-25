namespace Aristocrat.Monaco.Gaming.Presentation.Store.Game;

using Fluxor;

public static class GameReducers
{
    [ReducerMethod]
    public static GameState BottomWindowLoaded(GameState state, GameMainWindowLoadedAction action) =>
        state with { MainWindowHandle = action.WindowHandle };

    [ReducerMethod]
    public static GameState ButtonDeckWindowLoaded(GameState state, GameButtonDeckWindowLoadedAction action) =>
        state with { ButtonDeckWindowHandle = action.WindowHandle };

    [ReducerMethod]
    public static GameState TopWindowLoaded(GameState state, GameTopWindowLoadedAction action) =>
        state with { TopWindowHandle = action.WindowHandle };

    [ReducerMethod]
    public static GameState TopperWindowLoaded(GameState state, GameTopperWindowLoadedAction action) =>
        state with { TopperWindowHandle = action.WindowHandle };

    [ReducerMethod]
    public static GameState TopperWindowLoaded(GameState state, GameLoadingAction action) =>
        state with
        {
            SelectedGame = action.Game,
            IsLoading = true
        };

    [ReducerMethod]
    public static GameState TopperWindowLoaded(GameState state, GameLoadedAction action) =>
        state with
        {
            IsLoading = false,
            IsLoaded = true
        };
}
