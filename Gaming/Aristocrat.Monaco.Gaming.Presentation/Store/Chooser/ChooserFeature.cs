namespace Aristocrat.Monaco.Gaming.Presentation.Store.Chooser;

using System.Collections.Immutable;
using Fluxor;
using UI.Models;

public class ChooserFeature : Feature<ChooserState>
{
    public override string GetName() => "Chooser";

    protected override ChooserState GetInitialState()
    {
        return new ChooserState
        {
            Games = ImmutableList<GameInfo>.Empty,
            DenomFilter = -1
        };
    }
}
