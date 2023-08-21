namespace Aristocrat.Monaco.Gaming.Presentation.Store.Replay;

using Fluxor;

public class ReplayFeature : Feature<ReplayState>
{
    public override string GetName() => "Replay";

    protected override ReplayState GetInitialState()
    {
        return new ReplayState();
    }
}
