namespace Aristocrat.Monaco.Gaming.Presentation.Store.Translate;

using System.Collections.Immutable;
using System.Linq;
using Fluxor;

public static class TranslateReducers
{
    [ReducerMethod]
    public static TranslateState Reduce(TranslateState state, TranslateUpdateLocaleCodesAction action) =>
        state with
        {
            LocaleCodes = ImmutableList.CreateRange(action.LocaleCodes),
        };

    [ReducerMethod]
    public static TranslateState Reduce(TranslateState state, TranslateUpdateMultiLanguageAction action) =>
        state with
        {
            IsMultiLangauge = action.IsEnabled
        };

    [ReducerMethod]
    public static TranslateState Reduce(TranslateState state, TranslateUpdateActiveLanguageAction action) =>
        state with
        {
            IsPrimaryLanguageActive = action.IsPrimaryLanguageActive
        };
}
