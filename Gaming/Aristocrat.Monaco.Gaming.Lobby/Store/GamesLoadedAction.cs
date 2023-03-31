namespace Aristocrat.Monaco.Gaming.Lobby.Store;

using System.Collections.Generic;
using Models;

public class GamesLoadedAction
{
    public GamesLoadedAction(LoadGameTrigger trigger, IEnumerable<GameInfo> games)
    {
        Trigger = trigger;
        Games = new List<GameInfo>(games);
    }

    public LoadGameTrigger Trigger { get; }

    public IList<GameInfo> Games { get; }
}
