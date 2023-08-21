namespace Aristocrat.Monaco.Gaming.Presentation.Store.Replay;

using System.Threading.Tasks;
using Extensions.Fluxor;
using Fluxor;
using Services.Replay;

public class ReplayEffects
{
    private readonly IState<ReplayState> _replayState;
    private readonly IReplayService _replayAgent;

    public ReplayEffects(IState<ReplayState> replayState, IReplayService replayAgent)
    {
        _replayState = replayState;
        _replayAgent = replayAgent;
    }

    [EffectMethod]
    public async Task ReplayPauseInput(ReplayPauseInputAction action, IDispatcher dispatcher)
    {
        if (await _replayAgent.GetReplayPauseActiveAsync())
        {
            if (!_replayState.Value.IsCompleted)
            {
                await dispatcher.DispatchAsync(new ReplayPausedAction());
            }

            return;
        }

        if (action.IsPaused)
        {
            await _replayAgent.ContinueAsync();
        }
    }

    [EffectMethod(typeof(ReplayContinueAction))]
    public async Task ReplayContinue(IDispatcher _)
    {
        await _replayAgent.ContinueAsync();
    }

    [EffectMethod(typeof(ReplayExitAction))]
    public async Task ReplayExit(IDispatcher _)
    {
        await _replayAgent.EndReplayAsync();
    }

    [EffectMethod(typeof(ReplayCompletedAction))]
    public async Task ReplayCompleted(IDispatcher _)
    {
        await _replayAgent.NotifyCompletedAsync(_replayState.Value.EndCredits);
    }
}
