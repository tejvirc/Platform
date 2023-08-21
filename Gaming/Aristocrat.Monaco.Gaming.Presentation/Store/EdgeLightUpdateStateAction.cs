namespace Aristocrat.Monaco.Gaming.Presentation.Store;

using Monaco.Application.Contracts.EdgeLight;

public record EdgeLightUpdateStateAction
{
    public EdgeLightState? EdgeLightState { get; init; }
}
