namespace Aristocrat.Monaco.Gaming.Lobby.Store.Upi;

using System.Collections.Immutable;
using Fluxor;

public class UpiFeature : Feature<UpiState>
{
    public override string GetName() => "Upi";

    protected override UpiState GetInitialState()
    {
        return new UpiState();
    }
}
