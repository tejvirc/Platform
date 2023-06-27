namespace Aristocrat.Monaco.Gaming.Lobby.Store.Attract;

using Fluxor;

public class AttractReducers
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
}
