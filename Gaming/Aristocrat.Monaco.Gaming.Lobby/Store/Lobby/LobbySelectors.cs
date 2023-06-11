namespace Aristocrat.Monaco.Gaming.Lobby.Store.Lobby;

using System.Collections.Immutable;
using Models;
using Redux;
using static Redux.Selectors;

public class LobbySelectors
{
    public static readonly ISelector<LobbyState, IImmutableList<GameInfo>> GamesSelector = CreateSelector(
        (LobbyState s) => s.Games);
}
