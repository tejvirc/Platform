namespace Aristocrat.Monaco.Gaming.Lobby.Store;

using Monaco.Application.Contracts.EdgeLight;

public record UpdateEdgeLightStateAction
{
    public EdgeLightState? EdgeLightState { get; init; }
}
