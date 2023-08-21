namespace Aristocrat.Monaco.Gaming.Presentation.Services.EdgeLighting;

using Application.Contracts.EdgeLight;

public interface IEdgeLightingService
{
    void SetEdgeLighting(EdgeLightState? newState);
}
