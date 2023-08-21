namespace Aristocrat.Monaco.Gaming.Presentation.Store.GameList;

using System.Collections.Immutable;
using Fluxor;
using UI.Models;

public class GameListStateFeature : Feature<GameListState>
{
    public override string GetName() => "GameList";

    protected override GameListState GetInitialState()
    {
        return new GameListState
        {
            Games = ImmutableList<GameInfo>.Empty
        };
    }
}
