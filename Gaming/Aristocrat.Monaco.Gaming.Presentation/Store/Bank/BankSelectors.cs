namespace Aristocrat.Monaco.Gaming.Presentation.Store.Bank;

using Extensions.Fluxor;
using static Extensions.Fluxor.Selectors;

public static class BankSelectors
{
    public static readonly ISelector<BankState, double> SelectCredits = CreateSelector(
        (BankState state) => state.Credits);
}
