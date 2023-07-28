namespace Aristocrat.Monaco.Gaming.Lobby.Store.Attract;

using Fluxor;

public static class AttractReducers
{
    [ReducerMethod]
    public static AttractState Reduce(AttractState state, AttractEnterAction _) =>
        state with
        {
            IsAttractMode = true,
            IsAttractPlaying = true,
            IsPrimaryLanguageSelected = true
        };

    [ReducerMethod]
    public static AttractState Reduce(AttractState state, AttractExitAction _) =>
        state with
        {
            TopAttractVideoPath = null,
            BottomAttractVideoPath = null,
            IsAttractPlaying = false,
            IsTopperAttractFeaturePlaying = false,
            IsTopAttractFeaturePlaying = false,
            IsBottomAttractFeaturePlaying = false
        };

    [ReducerMethod]
    public static AttractState Reduce(AttractState state, UpdateAttractIndexAction action) =>
        state with { CurrentAttractIndex = action.AttractIndex };

    [ReducerMethod]
    public static AttractState Reduce(AttractState state, UpdateAttractModeTopperImageIndex action) =>
        state with
        {
            AttractModeTopperImageIndex = action.Index,
        };

    [ReducerMethod]
    public static AttractState Reduce(AttractState state, UpdateAttractModeTopImageIndex action) =>
        state with
        {
            AttractModeTopImageIndex = action.Index,
        };

    [ReducerMethod]
    public static AttractState Reduce(AttractState state, ToggleTopImageAction _) =>
        state with
        {
            IsAlternateTopImageActive = !state.IsAlternateTopImageActive,
        };

    [ReducerMethod]
    public static AttractState Reduce(AttractState state, ToggleTopperImageAction _) =>
        state with
        {
            IsAlternateTopperImageActive = !state.IsAlternateTopperImageActive,
        };
}
