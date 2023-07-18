namespace Aristocrat.Monaco.Gaming.Lobby.Store.Translate;

using Aristocrat.Extensions.Fluxor;
using System.Collections.Immutable;
using static Extensions.Fluxor.Selectors;

public static class TranslateSelectors
{
    public static readonly ISelector<TranslateState, string> SelectActiveLocale = CreateSelector(
        (TranslateState state) => state.ActiveLocaleCode!);
}
