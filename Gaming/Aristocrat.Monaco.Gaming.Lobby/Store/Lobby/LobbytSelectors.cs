namespace Aristocrat.Monaco.Gaming.Lobby.Store.Lobby;

using Extensions.Fluxor;
using static Extensions.Fluxor.Selectors;

public static class LobbytSelectors
{
    public static readonly ISelector<LobbyState, bool> SelectLobbyInitailized = CreateSelector(
        (LobbyState state) => state.IsInitialized);
}
