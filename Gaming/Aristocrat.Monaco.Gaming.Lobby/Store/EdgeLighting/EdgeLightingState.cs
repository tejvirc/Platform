namespace Aristocrat.Monaco.Gaming.Lobby.Store.EdgeLighting;

using Application.Contracts.EdgeLight;
using Fluxor;

[FeatureState]
public record EdgeLightingState
{
    public EdgeLightState? EdgeLightState { get; set; }

    public bool CanOverrideEdgeLight { get; set; }
}
