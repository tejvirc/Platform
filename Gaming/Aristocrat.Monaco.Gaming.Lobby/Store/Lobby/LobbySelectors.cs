namespace Aristocrat.Monaco.Gaming.Lobby.Store.Lobby;

using Aristocrat.Monaco.Gaming.Lobby.Redux;
using Aristocrat.Monaco.Gaming.Lobby.Store.Chooser;
using System.Collections.Immutable;
using Models;
using static Redux.Selectors;

public class LobbySelectors
{
    public static readonly ISelector<LobbyState, IImmutableList<GameInfo>> GamesSelector = CreateSelector(
        (LobbyState s) => s.Games);
}
