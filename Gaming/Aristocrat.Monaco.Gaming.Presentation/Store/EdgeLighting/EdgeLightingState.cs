namespace Aristocrat.Monaco.Gaming.Presentation.Store.EdgeLighting;

using Application.Contracts.EdgeLight;

public record EdgeLightingState
{
    public EdgeLightState? EdgeLightState { get; init; }

    public bool CanOverrideEdgeLight { get; init; }
}
