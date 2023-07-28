namespace Aristocrat.Monaco.Gaming.Lobby.Store.Translate;

using System.Collections.Immutable;
using System.Linq;
using Fluxor;

public static class TranslateReducers
{
    [ReducerMethod]
    public static TranslateState Reduce(TranslateState state, StartupAction action) =>
        state with
        {
            LocalCodes = ImmutableList.CreateRange(action.Configuration.LocaleCodes),
            IsMultiLangaugeEnabled = action.Configuration.MultiLanguageEnabled
        };

    [ReducerMethod]
    public static TranslateState Reduce(TranslateState state, UpdatePrimaryLanguageSelectedAction action) =>
        state with
        {
            IsPrimaryLanguageSelected = action.IsSelected
        };
}
