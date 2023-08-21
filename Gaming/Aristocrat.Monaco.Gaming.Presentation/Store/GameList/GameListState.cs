namespace Aristocrat.Monaco.Gaming.Presentation.Store.GameList;

using System.Collections.Immutable;
using UI.Models;

public record GameListState
{
    public IImmutableList<GameInfo> Games { get; init; }
}
