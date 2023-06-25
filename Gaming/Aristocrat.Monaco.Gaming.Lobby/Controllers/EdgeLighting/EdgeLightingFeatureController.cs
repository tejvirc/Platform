namespace Aristocrat.Monaco.Gaming.Lobby.Controllers.EdgeLighting;

using Aristocrat.Monaco.Application.Contracts.EdgeLight;
using Aristocrat.Monaco.Gaming.Contracts.Models;
using Aristocrat.Monaco.Gaming.Lobby.Store;
using global::Fluxor;

public class EdgeLightingFeatureController
{
    private readonly IStore _store;

    public EdgeLightingFeatureController(IStore store)
    {
        _store = store;

        SubscribeToActions();
    }

    private void SubscribeToActions()
    {
        _store.SubscribeToAction<StartupAction>(this, Handle);
    }

    private void Handle(StartupAction action)
    {
        throw new System.NotImplementedException();
    }

    private void SetEdgeLighting()
    {
        lock (_edgeLightLock)
        {
            Logger.Debug($"Current EdgeLightState:  {_currentEdgeLightState}");
            EdgeLightState? newState;

            if (ContainsAnyState(LobbyState.CashOut) && _lobbyStateManager.CashOutState != LobbyCashOutState.Undefined)
            {
                newState = EdgeLightState.Cashout;
            }
            else if (!ContainsAnyState(LobbyState.Disabled) &&
                     (_attractMode && !Config.EdgeLightingOverrideUseGen8IdleMode ||
                      _canOverrideEdgeLight && Config.EdgeLightingOverrideUseGen8IdleMode))
            {
                newState = EdgeLightState.AttractMode;
            }
            else if (BaseState == LobbyState.Game)
            {
                newState = null;
            }
            else
            {
                newState = EdgeLightState.Lobby;
            }

            if (newState != _currentEdgeLightState)
            {
                _edgeLightingStateManager.ClearState(_edgeLightStateToken);
                if (newState.HasValue)
                {
                    _edgeLightStateToken = _edgeLightingStateManager.SetState(newState.Value);
                }
                _currentEdgeLightState = newState;
                Logger.Debug($"New EdgeLightState:  {_currentEdgeLightState}");
            }
        }
    }
}
