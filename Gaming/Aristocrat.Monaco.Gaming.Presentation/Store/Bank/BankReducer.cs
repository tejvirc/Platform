namespace Aristocrat.Monaco.Gaming.Presentation.Store.Bank;

using System;
using Aristocrat.Monaco.Gaming.Presentation.Store.Chooser;
using Fluxor;

public static class BankReducer
{
    [ReducerMethod]
    public static BankState Reduce(BankState state, BankUpdateCreditsAction action) =>
        state with
        {
            Credits = action.Credits
        };
}
