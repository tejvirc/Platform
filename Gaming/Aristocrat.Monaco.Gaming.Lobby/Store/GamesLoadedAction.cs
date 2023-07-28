namespace Aristocrat.Monaco.Gaming.Lobby.Store;

using System.Collections.Generic;
using UI.Models;

public record GamesLoadedAction
{
    public IList<GameInfo> Games { get; init; } = new List<GameInfo>();
}
