namespace Aristocrat.Monaco.Gaming.Lobby.Store.EdgeLighting;

using System.Threading.Tasks;
using Application.Contracts.EdgeLight;
using Common;
using Fluxor;
using Services.EdgeLighting;

public class EdgeLightingEffects
{
    private readonly IState<CommonState> _commonState;
    private readonly IState<EdgeLightingState> _edgeLightingState;
    private readonly LobbyConfiguration _configuration;
    private readonly IEdgeLightingService _edgeLightingService;

    public EdgeLightingEffects(
        IState<CommonState> commonState,
        IState<EdgeLightingState> edgeLightingState,
        LobbyConfiguration configuration,
        IEdgeLightingService edgeLightingService)
    {
        _commonState = commonState;
        _edgeLightingState = edgeLightingState;
        _configuration = configuration;
        _edgeLightingService = edgeLightingService;
    }

    [EffectMethod]
    public async Task Effect(LobbyEnterAction _, IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new UpdateEdgeLightStateAction { EdgeLightState = EdgeLightState.Lobby });
    }

    [EffectMethod]
    public async Task Effect(AttractEnterAction _, IDispatcher dispatcher)
    {
        if (!_commonState.Value.IsSystemDisabled && !_configuration.EdgeLightingOverrideUseGen8IdleMode ||
            _edgeLightingState.Value.CanOverrideEdgeLight && _configuration.EdgeLightingOverrideUseGen8IdleMode)
        {
            await dispatcher.DispatchAsync(new UpdateEdgeLightStateAction { EdgeLightState = EdgeLightState.AttractMode });
        }
    }

    [EffectMethod]
    public async Task Effect(GameLoadedAction _, IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new UpdateEdgeLightStateAction { EdgeLightState = null });
    }

    [EffectMethod]
    public async Task Effect(CashOutStartAction _, IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new UpdateEdgeLightStateAction { EdgeLightState = EdgeLightState.Cashout });
    }

    [EffectMethod]
    public async Task Effect(CashOutCompleteAction _, IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new UpdateEdgeLightStateAction { EdgeLightState = EdgeLightState.Lobby });
    }

    [EffectMethod]
    public Task Effect(UpdateEdgeLightStateAction payload, IDispatcher _)
    {
        _edgeLightingService.SetEdgeLighting(payload.EdgeLightState);

        return Task.CompletedTask;
    }
}
