namespace Aristocrat.Monaco.Gaming.Lobby.Store.Replay;

using System;
using Extensions.Fluxor;
using static Extensions.Fluxor.Selectors;

public static class ReplaySelectors
{
    public static readonly ISelector<ReplayState, bool> SelectStarted = CreateSelector(
        (ReplayState state) => state.IsStarted);

    public static readonly ISelector<ReplayState, bool> SelectPaused = CreateSelector(
        (ReplayState state) => state.IsPaused);

    public static readonly ISelector<ReplayState, bool> SelectCompleted = CreateSelector(
        (ReplayState state) => state.IsCompleted);

    public static readonly ISelector<ReplayState, string> SelectReplayGameName = CreateSelector(
        (ReplayState state) => state.GameName);

    public static readonly ISelector<ReplayState, string> SelectReplayLabel = CreateSelector(
        (ReplayState state) => state.Label);

    public static readonly ISelector<ReplayState, long> SelectReplaySequence = CreateSelector(
        (ReplayState state) => state.Sequence);

    public static readonly ISelector<ReplayState, DateTime> SelectReplayStartTime = CreateSelector(
        (ReplayState state) => state.StartTime);

    public static readonly ISelector<ReplayState, long> SelectReplayEndCredits = CreateSelector(
        (ReplayState state) => state.EndCredits);

    public static readonly ISelector<ReplayState, decimal> SelectReplayGameWinBonusAwarded = CreateSelector(
        (ReplayState state) => state.GameWinBonusAwarded);

    public static readonly ISelector<ReplayState, long> SelectReplayVoucherOut = CreateSelector(
        (ReplayState state) => state.VoucherOut);

    public static readonly ISelector<ReplayState, long> SelectReplayHardMeterOut = CreateSelector(
        (ReplayState state) => state.HardMeterOut);

    public static readonly ISelector<ReplayState, long> SelectReplayBonusOrGameWinToCredits = CreateSelector(
        (ReplayState state) => state.BonusOrGameWinToCredits);

    public static readonly ISelector<ReplayState, long> SelectReplayBonusOrGameWinToHandpay = CreateSelector(
        (ReplayState state) => state.BonusOrGameWinToHandpay);

    public static readonly ISelector<ReplayState, long> SelectReplayCancelledCredits = CreateSelector(
        (ReplayState state) => state.CancelledCredits);
}
