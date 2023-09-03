namespace Aristocrat.Monaco.Gaming.Presentation.Store.Lobby;

using Fluxor;

public static class LobbyReducers
{

    //[ReducerMethod]
    //public static LobbyState Reduce(LobbyState state, GamePlayEnabledAction action) =>
    //    state with { AllowGameAutoLaunch = true };

    [ReducerMethod]
    public static LobbyState Reduce(LobbyState state, StartupAction action) =>
        state with
        {
            IsStartingUp = true
        };
}
