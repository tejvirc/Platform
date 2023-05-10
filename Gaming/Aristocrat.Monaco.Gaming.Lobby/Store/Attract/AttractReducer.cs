namespace Aristocrat.Monaco.Gaming.Lobby.Store.Attract;

using Fluxor;

public class AttractReducer
{
    [ReducerMethod]
    public static AttractState Reduce(AttractState state, StartupAction payload) =>
        state with { IsMultiLanguage = payload.Configuration.MultiLanguageEnabled };
}
