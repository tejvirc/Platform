namespace Aristocrat.Monaco.Gaming.Presentation.Store.Chooser;

using System.Threading.Tasks;
using Extensions.Fluxor;
using Fluxor;

public class ChooserEffects
{
    private readonly IState<ChooserState> _chooserState;

    public ChooserEffects(IState<ChooserState> chooserState)
    {
        _chooserState = chooserState;
    }

    [EffectMethod]
    public async Task Effect(StartupAction _, IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new GameListRehydrateAction());
    }

    [EffectMethod]
    public async Task Effect(GameListAddedAction _, IDispatcher dispatcher)
    {
        // OnUserInteraction

        await dispatcher.DispatchAsync(new GameListRehydrateAction());
    }

    [EffectMethod]
    public async Task Effect(GameUpgradedAction _, IDispatcher dispatcher)
    {
        // OnUserInteraction

        await dispatcher.DispatchAsync(new GameListRehydrateAction());
    }

    [EffectMethod]
    public async Task Effect(GameListRemovedAction _, IDispatcher dispatcher)
    {
        // OnUserInteraction

        await dispatcher.DispatchAsync(new GameListRehydrateAction());
    }

    [EffectMethod]
    public async Task Effect(GameListIconOrderChangedAction _, IDispatcher dispatcher)
    {
        // OnUserInteraction

        await dispatcher.DispatchAsync(new GameListRehydrateAction());
    }

    [EffectMethod]
    public async Task Effect(GameEnabledAction _, IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new GameListRehydrateAction());

        // SetTabViewToDefault
    }

    [EffectMethod]
    public async Task Effect(GameDisabledAction _, IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new GameListRehydrateAction());

        // SetTabViewToDefault
    }
}
