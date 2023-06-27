namespace Aristocrat.Monaco.Gaming.Lobby.Services.EdgeLighting;

using System.Threading.Tasks;
using Application.Contracts.EdgeLight;
using Aristocrat.Monaco.Application.Contracts.Localization;
using Aristocrat.Monaco.Application.Contracts;
using Fluxor;
using Hardware.Contracts.EdgeLighting;
using Kernel;
using Store;
using Store.Application;
using Store.Attract;
using Store.Lobby;
using LobbyCashOutState = Contracts.Models.LobbyCashOutState;

public class EdgeLightingService : IEdgeLightingService
{
    private readonly IDispatcher _dispatcher;
    private readonly IState<CommonState> _applicationState;
    private readonly IState<AttractState> _attractState;
    private readonly LobbyConfiguration _configuration;
    private readonly IPropertiesManager _properties;
    private readonly IEdgeLightingStateManager _edgeLightingStateManager;

    private IEdgeLightToken? _edgeLightStateToken;
    private bool _canOverrideEdgeLight;

    public EdgeLightingService(
        IDispatcher dispatcher,
        IState<CommonState> applicationState,
        IState<AttractState> attractState,
        LobbyConfiguration configuration,
        IPropertiesManager properties,
        IEdgeLightingStateManager edgeLightingStateManager)
    {
        _dispatcher = dispatcher;
        _applicationState = applicationState;
        _attractState = attractState;
        _configuration = configuration;
        _properties = properties;
        _edgeLightingStateManager = edgeLightingStateManager;
    }

    public void SetEdgeLighting()
    {
        EdgeLightState? newState;

        if (_state.Value.IsCashingOut && _state.Value.CurrentCashOutState != LobbyCashOutState.Undefined)
        {
            newState = EdgeLightState.Cashout;
        }
        else if (!_applicationState.Value.IsSystemDisabled &&
                 (_attractState.Value.IsAttractMode && !_configuration.EdgeLightingOverrideUseGen8IdleMode ||
                  _canOverrideEdgeLight && _configuration.EdgeLightingOverrideUseGen8IdleMode))
        {
            newState = EdgeLightState.AttractMode;
        }
        else if (_state.Value.IsGameLoaded)
        {
            newState = null;
        }
        else
        {
            newState = EdgeLightState.Lobby;
        }

        if (newState != _state.Value.CurrentEdgeLightState)
        {
            _edgeLightingStateManager.ClearState(_edgeLightStateToken);

            if (newState.HasValue)
            {
                _edgeLightStateToken = _edgeLightingStateManager.SetState(newState.Value);
            }

            _dispatcher.Dispatch(new UpdateEdgeLightStateAction { EdgeLightState = newState });
        }
    }

    public void SetEdgeLightOverride()
    {
        var edgeLightingAttractModeOverrideSelection = _properties.GetValue(
            ApplicationConstants.EdgeLightingAttractModeColorOverrideSelectionKey,
            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LightingOverrideTransparent));

        if (edgeLightingAttractModeOverrideSelection != Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LightingOverrideTransparent))
        {
            _canOverrideEdgeLight = true;
        }
    }

    public void ResetEdgeLightOverride()
    {
        _canOverrideEdgeLight = false;
    }
}
