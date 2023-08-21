namespace Aristocrat.Monaco.Gaming.Presentation.Store.Replay;

using Extensions.Fluxor;
using static Extensions.Fluxor.Selectors;

public static class ReplaySelectors
{
    public static readonly ISelector<ReplayState, bool> SelectReplayStarted = CreateSelector(
        (ReplayState state) => state.IsStarted);

    public static readonly ISelector<ReplayState, bool> SelectReplayPaused = CreateSelector(
        (ReplayState state) => state.IsPaused);

    public static readonly ISelector<ReplayState, bool> SelectReplayCompleted = CreateSelector(
        (ReplayState state) => state.IsCompleted);
}
