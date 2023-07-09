namespace Aristocrat.Monaco.Gaming.Lobby.Store.Chooser;

using Fluxor;
using System.Threading.Tasks;

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
        await dispatcher.DispatchAsync(new RehydrateGamesAction());
    }

    [EffectMethod]
    public async Task Effect(GameAddedAction _, IDispatcher dispatcher)
    {
        // OnUserInteraction

        await dispatcher.DispatchAsync(new RehydrateGamesAction());
    }

    [EffectMethod]
    public async Task Effect(GameUpgradedAction _, IDispatcher dispatcher)
    {
        // OnUserInteraction

        await dispatcher.DispatchAsync(new RehydrateGamesAction());
    }

    [EffectMethod]
    public async Task Effect(GameRemovedAction _, IDispatcher dispatcher)
    {
        // OnUserInteraction

        await dispatcher.DispatchAsync(new RehydrateGamesAction());
    }

    [EffectMethod]
    public async Task Effect(GameIconOrderChangedAction _, IDispatcher dispatcher)
    {
        // OnUserInteraction

        await dispatcher.DispatchAsync(new RehydrateGamesAction());
    }

    [EffectMethod]
    public async Task Effect(GameEnabledAction _, IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new RehydrateGamesAction());

        // SetTabViewToDefault
    }

    [EffectMethod]
    public async Task Effect(GameDisabledAction _, IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new RehydrateGamesAction());

        // SetTabViewToDefault
    }
}
