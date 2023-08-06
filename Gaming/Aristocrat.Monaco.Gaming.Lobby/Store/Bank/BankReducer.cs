namespace Aristocrat.Monaco.Gaming.Lobby.Store.Bank;

using System;
using Aristocrat.Monaco.Gaming.Lobby.Store.Chooser;
using Fluxor;

public static class BankReducer
{
    [ReducerMethod]
    public static BankState Reduce(BankState state, UpdateCreditsAction action) =>
        state with
        {
            Credits = action.Credits
        };
}
