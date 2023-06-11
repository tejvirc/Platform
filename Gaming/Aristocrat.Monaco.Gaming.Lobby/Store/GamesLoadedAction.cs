namespace Aristocrat.Monaco.Gaming.Lobby.Store;

using System.Collections.Generic;
using Models;

public class GamesLoadedAction
{
    public GamesLoadedAction(IEnumerable<GameInfo> games)
    {
        Games = new List<GameInfo>(games);
    }

    public IList<GameInfo> Games { get; }
}
