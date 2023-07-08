namespace Aristocrat.Monaco.Gaming.Lobby.Services.EdgeLighting;

using Application.Contracts.EdgeLight;

public interface IEdgeLightingService
{
    void SetEdgeLighting(EdgeLightState? newState);
}
