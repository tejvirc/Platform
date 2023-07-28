namespace Aristocrat.Monaco.Gaming.Lobby.Store.Translate;

using System.Collections.Immutable;
using Extensions.Fluxor;
using static Extensions.Fluxor.Selectors;

public static class TranslateSelectors
{
    public static readonly ISelector<TranslateState, IImmutableList<string>> SelectLocalCodes = CreateSelector(
        (TranslateState state) => state.LocalCodes);

    public static readonly ISelector<TranslateState, bool> SelectIsPrimaryLanguageSelected = CreateSelector(
        (TranslateState state) => state.IsPrimaryLanguageSelected);

    public static readonly ISelector<TranslateState, string> SelectActiveLocale = CreateSelector(
        SelectLocalCodes, SelectIsPrimaryLanguageSelected, (localeCodes, isPrimaryLanguageSelected) =>
            isPrimaryLanguageSelected ? localeCodes[0] : localeCodes[1]);
}
