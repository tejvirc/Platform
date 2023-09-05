namespace Aristocrat.Monaco.Gaming.Presentation.Store.EdgeLighting;

using System.Threading.Tasks;
using Application.Contracts.EdgeLight;
using Extensions.Fluxor;
using Fluxor;
using Microsoft.Extensions.Options;
using Options;
using Services.EdgeLighting;
using Store.Platform;

public class EdgeLightingEffects
{
    private readonly IState<PlatformState> _platformState;
    private readonly IState<EdgeLightingState> _edgeLightingState;
    private readonly EdgeLightingOptions _edgeLightingOptions;
    private readonly IEdgeLightingService _edgeLightingService;

    public EdgeLightingEffects(
        IState<PlatformState> platformState,
        IState<EdgeLightingState> edgeLightingState,
        IOptions<EdgeLightingOptions> edgeLightingOptions,
        IEdgeLightingService edgeLightingService)
    {
        _platformState = platformState;
        _edgeLightingState = edgeLightingState;
        _edgeLightingOptions = edgeLightingOptions.Value;
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
        if (!_platformState.Value.IsDisabled && !_edgeLightingOptions.Gen8IdleModeOverride ||
            _edgeLightingState.Value.CanOverrideEdgeLight && _edgeLightingOptions.Gen8IdleModeOverride)
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
