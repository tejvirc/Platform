namespace Aristocrat.Monaco.Gaming.Presentation.Store.Translate;

using System.Collections.Immutable;
using Extensions.Fluxor;
using static Extensions.Fluxor.Selectors;

public static class TranslateSelectors
{
    public static readonly ISelector<TranslateState, IImmutableList<string>> SelectLocaleCodes = CreateSelector(
        (TranslateState state) => state.LocaleCodes);

    public static readonly ISelector<TranslateState, bool> SelectPrimaryLanguageActive = CreateSelector(
        (TranslateState state) => state.IsPrimaryLanguageActive);

    public static readonly ISelector<TranslateState, string> SelectActiveLocale = CreateSelector(
        SelectLocaleCodes,
        SelectPrimaryLanguageActive,
        (localeCodes, isPrimaryActive) =>
            localeCodes switch
            {
                { Count: >= 2 } => isPrimaryActive ? localeCodes[0] : localeCodes[1],
                { Count: 1 } => localeCodes[0],
                _ => "US-EN"
            });

}