namespace Aristocrat.Monaco.Gaming.Presentation.Store.Reserve;

using Fluxor;

public class ReserveFeature : Feature<ReserveState>
{
    public override string GetName() => "Reserve";

    protected override ReserveState GetInitialState()
    {
        return new ReserveState();
    }
}
