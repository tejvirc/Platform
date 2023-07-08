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
    public static AttractState Reduce(AttractState state, UpdateAttractIndexAction payload) =>
        state with { CurrentAttractIndex = payload.AttractIndex };

    [ReducerMethod]
    public static AttractState Reduce(AttractState state, UpdateAttractModeTopperImageIndex payload) =>
        state with
        {
            AttractModeTopperImageIndex = payload.Index,
        };

    [ReducerMethod]
    public static AttractState Reduce(AttractState state, UpdateAttractModeTopImageIndex payload) =>
        state with
        {
            AttractModeTopImageIndex = payload.Index,
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
