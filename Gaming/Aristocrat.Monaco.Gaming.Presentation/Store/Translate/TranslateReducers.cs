namespace Aristocrat.Monaco.Gaming.Presentation.Store.Translate;

using System.Collections.Immutable;
using Fluxor;

public static class TranslateReducers
{
    [ReducerMethod]
    public static TranslateState Reduce(TranslateState state, TranslateUpdateLocaleCodesAction action)
    {
        return state with { LocaleCodes = ImmutableList.CreateRange(action.LocaleCodes) };
    }

    [ReducerMethod]
    public static TranslateState Reduce(TranslateState state, TranslateUpdateMultiLanguageAction action)
    {
        return state with { IsMultiLanguage = action.IsEnabled };
    }

    [ReducerMethod]
    public static TranslateState Reduce(TranslateState state, TranslateUpdatePrimaryLanguageAction action)
    {
        return state with { IsPrimaryLanguageActive = action.IsPrimaryLanguageActive };
    }

    [ReducerMethod]
    public static TranslateState Reduce(TranslateState state, TranslateInitialLanguageEventAction action)
    {
        return state with { IsInitialLanguageEventSent = action.IsInitialLanguageEventSent };
    }
}