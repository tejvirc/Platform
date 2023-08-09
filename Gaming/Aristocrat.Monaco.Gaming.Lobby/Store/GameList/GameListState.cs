namespace Aristocrat.Monaco.Gaming.Lobby.Store.GameList;

using System.Collections.Immutable;
using UI.Models;

public record GameListState
{
    public IImmutableList<GameInfo> Games { get; set; }
}
