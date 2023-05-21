namespace Aristocrat.Monaco.Gaming.Lobby.Store.EdgeLighting;

using Application.Contracts.EdgeLight;

public record EdgeLightingState
{
    public EdgeLightState CurrentState { get; set; }

    public bool UseGen8IdleMode { get; set; }
}
