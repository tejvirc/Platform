namespace Aristocrat.Monaco.Gaming.Presentation.Store.Platform;

using Fluxor;

public class PlatfromFeature : Feature<PlatformState>
{
    public override string GetName() => "Platform";

    protected override PlatformState GetInitialState()
    {
        return new PlatformState
        {
            IsDisplayConnected = true
        };
    }
}
