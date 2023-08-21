namespace Aristocrat.Monaco.Gaming.Presentation.Store;

using System.Collections.Generic;
using UI.Models;

public record GameListLoadedAction
{
    public IList<GameInfo> Games { get; init; } = new List<GameInfo>();
}
