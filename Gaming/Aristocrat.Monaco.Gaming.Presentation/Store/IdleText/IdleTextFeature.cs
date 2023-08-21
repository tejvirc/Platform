namespace Aristocrat.Monaco.Gaming.Presentation.Store.IdleText;

using Fluxor;

public class IdleTextFeature : Feature<IdleTextState>
{
    public override string GetName() => "IdleText";

    protected override IdleTextState GetInitialState()
    {
        return new IdleTextState();
    }
}
