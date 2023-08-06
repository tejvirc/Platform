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
            LocaleCodes = ImmutableList.CreateRange(action.Configuration.LocaleCodes),
            IsMultiLangauge = action.Configuration.MultiLanguageEnabled
        };

    [ReducerMethod]
    public static TranslateState Reduce(TranslateState state, UpdateActiveLanguageAction action) =>
        state with
        {
            IsPrimaryLanguageActive = action.IsPrimaryLanguageActive
        };
}
