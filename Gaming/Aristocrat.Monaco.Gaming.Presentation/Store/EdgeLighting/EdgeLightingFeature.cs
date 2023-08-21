namespace Aristocrat.Monaco.Gaming.Presentation.Store.EdgeLighting;

using Fluxor;

public class EdgeLightingFeature : Feature<EdgeLightingState>
{
    public override string GetName() => "EdgeLighting";

    protected override EdgeLightingState GetInitialState()
    {
        return new EdgeLightingState();
    }
}
