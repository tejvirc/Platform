namespace Aristocrat.Monaco.Gaming.Presentation.Store.Bank;

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
