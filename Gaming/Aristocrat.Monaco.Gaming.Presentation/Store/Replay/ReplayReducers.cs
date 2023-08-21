namespace Aristocrat.Monaco.Gaming.Presentation.Store.Replay;

using Fluxor;

public static class ReplayReducers
{
    [ReducerMethod]
    public static ReplayState Reduce(ReplayState state, ReplayStartedAction action) =>
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

    [ReducerMethod]
    public static ReplayState Reduce(ReplayState state, ReplayPausedAction action) =>
        state with
        {
            IsPaused = true
        };

    [ReducerMethod]
    public static ReplayState Reduce(ReplayState state, ReplayContinueAction action) =>
        state with
        {
            IsPaused = false
        };

    [ReducerMethod]
    public static ReplayState Reduce(ReplayState state, ReplayCompletedAction action) =>
        state with
        {
            IsStarted = false,
            IsPaused = false,
            IsCompleted = true
        };
}
