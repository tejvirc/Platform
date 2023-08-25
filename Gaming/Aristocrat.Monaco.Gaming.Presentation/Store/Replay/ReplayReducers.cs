namespace Aristocrat.Monaco.Gaming.Presentation.Store.Replay;

using Fluxor;

public static class ReplayReducers
{
    [ReducerMethod]
    public static ReplayState Started(ReplayState state, ReplayStartedAction action) =>
        state with
        {
            IsStarted = true,
            IsPaused = false,
            IsCompleted = false,
            GameName = action.GameName,
            Label = action.Label,
            Sequence = action.Sequence,
            StartTime = action.StartTime,
            EndCredits = action.EndCredits,
            GameWinBonusAwarded = action.GameWinBonusAwarded,
            HardMeterOut = action.HardMeterOut,
            VoucherOut = action.VoucherOut,
            BonusOrGameWinToHandpay = action.BonusOrGameWinToHandpay,
            BonusOrGameWinToCredits = action.BonusOrGameWinToCredits,
            CancelledCredits = action.CancelledCredits
        };

    [ReducerMethod(typeof(ReplayPausedAction))]
    public static ReplayState Paused(ReplayState state) =>
        state with
        {
            IsPaused = true
        };

    [ReducerMethod(typeof(ReplayContinueAction))]
    public static ReplayState Continue(ReplayState state) =>
        state with
        {
            IsPaused = false
        };

    [ReducerMethod(typeof(ReplayCompletedAction))]
    public static ReplayState Completed(ReplayState state) =>
        state with
        {
            IsStarted = false,
            IsPaused = false,
            IsCompleted = true
        };
}
