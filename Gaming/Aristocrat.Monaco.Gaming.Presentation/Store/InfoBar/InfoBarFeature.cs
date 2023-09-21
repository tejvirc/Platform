namespace Aristocrat.Monaco.Gaming.Presentation.Store.InfoBar;

using System.Collections.Immutable;
using Contracts.InfoBar;
using Fluxor;

public class InfoBarFeature : Feature<InfoBarState>
{
    public override string GetName() => "InfoBar";

    protected override InfoBarState GetInitialState()
    {
        return new InfoBarState
        {
            MessageDataSet = ImmutableList<InfoBarMessageData>.Empty
        };
    }
}
