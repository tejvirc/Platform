namespace Aristocrat.Monaco.Gaming.Lobby.Store.Bank;

using Fluxor;

public class BankFeature : Feature<BankState>
{
    public override string GetName() => "Bank";

    protected override BankState GetInitialState() =>
        new BankState
        {
            CashOutEnabled = true
        };
}
