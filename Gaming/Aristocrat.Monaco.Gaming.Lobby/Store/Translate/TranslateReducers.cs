namespace Aristocrat.Monaco.Gaming.Lobby.Store.Translate;

using Fluxor;

public static class TranslateReducers
{
    [ReducerMethod]
    public static TranslateState Reduce(TranslateState state, UpdateActiveLocaleAction payload) =>
        state with
        {
            ActiveLocaleCode = payload.ActiveLocaleCode,
        };
}
