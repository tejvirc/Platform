namespace Aristocrat.Monaco.Gaming.Presentation.Store.EdgeLighting;

using System.Threading.Tasks;
using Application.Contracts.EdgeLight;
using Extensions.Fluxor;
using Fluxor;
using Services.EdgeLighting;
using Store.Platform;

public class EdgeLightingEffects
{
    private readonly IState<PlatformState> _platformState;
    private readonly IState<EdgeLightingState> _edgeLightingState;
    private readonly LobbyConfiguration _configuration;
    private readonly IEdgeLightingService _edgeLightingService;

    public EdgeLightingEffects(
        IState<PlatformState> platformState,
        IState<EdgeLightingState> edgeLightingState,
        LobbyConfiguration configuration,
        IEdgeLightingService edgeLightingService)
    {
        _platformState = platformState;
        _edgeLightingState = edgeLightingState;
        _configuration = configuration;
        _edgeLightingService = edgeLightingService;
    }

    [EffectMethod(typeof(LobbyEnterAction))]
    public async Task LobbyEnter(IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new EdgeLightUpdateStateAction { EdgeLightState = EdgeLightState.Lobby });
    }

    [EffectMethod(typeof(AttractEnterAction))]
    public async Task AttractEnter(IDispatcher dispatcher)
    {
        if (!_platformState.Value.IsDisabled && !_configuration.EdgeLightingOverrideUseGen8IdleMode ||
            _edgeLightingState.Value.CanOverrideEdgeLight && _configuration.EdgeLightingOverrideUseGen8IdleMode)
        {
            await dispatcher.DispatchAsync(new EdgeLightUpdateStateAction { EdgeLightState = EdgeLightState.AttractMode });
        }
    }

    [EffectMethod(typeof(GameLoadedAction))]
    public async Task GameLoaded(IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new EdgeLightUpdateStateAction { EdgeLightState = null });
    }

    [EffectMethod(typeof(BankCashOutStartAction))]
    public async Task BankCashOutStart(IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new EdgeLightUpdateStateAction { EdgeLightState = EdgeLightState.Cashout });
    }

    [EffectMethod(typeof(BankCashOutCompleteAction))]
    public async Task BankCashOutComplete(IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new EdgeLightUpdateStateAction { EdgeLightState = EdgeLightState.Lobby });
    }

    [EffectMethod]
    public Task UpdateState(EdgeLightUpdateStateAction action, IDispatcher _)
    {
        _edgeLightingService.SetEdgeLighting(action.EdgeLightState);

        return Task.CompletedTask;
    }
}
