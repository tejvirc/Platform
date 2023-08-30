namespace Aristocrat.Monaco.Gaming.Presentation.Store.Attract;

using Aristocrat.Monaco.Application.Contracts;
using Fluxor;

public static class AttractReducers
{
    [ReducerMethod(typeof(AttractEnterAction))]
    public static AttractState Enter(AttractState state) =>
        state with
        {
            IsActive = true,
            IsPlaying = true,
            IsPrimaryLanguageSelected = true
        };

    [ReducerMethod(typeof(AttractExitAction))]
    public static AttractState Exit(AttractState state) =>
        state with
        {
            TopVideoPath = null,
            BottomVideoPath = null,
            IsPlaying = false,
            IsTopperPlaying = false,
            IsTopPlaying = false,
            IsBottomPlaying = false
        };

    [ReducerMethod]
    public static AttractState UpdateIndex(AttractState state, AttractUpdateIndexAction action) =>
        state with { CurrentAttractIndex = action.AttractIndex };

    [ReducerMethod]
    public static AttractState UpdateTopperImageIndex(AttractState state, AttractUpdateTopperImageIndexAction action) =>
        state with
        {
            ModeTopperImageIndex = action.Index,
        };

    [ReducerMethod]
    public static AttractState UpdateTopImageIndex(AttractState state, AttractUpdateTopImageIndexAction action) =>
        state with
        {
            ModeTopImageIndex = action.Index,
        };

    [ReducerMethod(typeof(AttractRotateTopImageAction))]
    public static AttractState RotateTopImage(AttractState state) =>
        state with
        {
            IsAlternateTopImageActive = !state.IsAlternateTopImageActive,
        };

    [ReducerMethod(typeof(AttractRotateTopperImageAction))]
    public static AttractState RotateTopperImage(AttractState state) =>
        state with
        {
            IsAlternateTopperImageActive = !state.IsAlternateTopperImageActive,
        };


    [ReducerMethod]
    public static AttractState Reduce(AttractState state, AttractAddVideosAction action) =>
        state with
        {
            Videos = action.AttractList ?? state.Videos
        };

    [ReducerMethod]
    public static AttractState Reduce(AttractState state, AttractSetCanModeStartAction action) =>
        state with
        {
            CanAttractModeStart = action.Bank.QueryBalance() == 0 ||
                                  (bool)action.Properties.GetProperty(ApplicationConstants.ShowMode, false)

        };

    [ReducerMethod]
    public static AttractState Reduce(AttractState state, AttractSetResetOnInterruptionAction action) =>
        state with
        {
            ResetAttractOnInterruption = action.ResetOnInteraction
        };
}
