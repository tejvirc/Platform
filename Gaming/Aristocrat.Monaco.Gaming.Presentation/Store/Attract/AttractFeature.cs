namespace Aristocrat.Monaco.Gaming.Presentation.Store.Attract;

using System.Collections.Immutable;
using Fluxor;
using UI.Models;

public class AttractFeature : Feature<AttractState>
{
    public override string GetName() => "Attract";

    protected override AttractState GetInitialState()
    {
        return new AttractState
        {
            Videos = ImmutableList<IAttractDetails>.Empty,
            ModeTopperImageIndex = -1,
            ModeTopImageIndex = -1

        };
    }
}
