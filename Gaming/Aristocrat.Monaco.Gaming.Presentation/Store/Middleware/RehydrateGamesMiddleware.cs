namespace Aristocrat.Monaco.Gaming.Presentation.Store.Middleware;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using UI.Models;
using Services;

public class RehydrateGamesMiddleware : Middleware
{
    private readonly IGameLoader _gameLoader;

    private IDispatcher? _dispatcher;
    private IStore? _store;

    public RehydrateGamesMiddleware(IGameLoader gameLoader)
    {
        _gameLoader = gameLoader;
    }

    public override Task InitializeAsync(IDispatcher dispatcher, IStore store)
    {
        _store = store;
        _dispatcher = dispatcher;

        return Task.CompletedTask;
    }

    public override bool MayDispatchAction(object action)
    {
        if (action is GameListRehydrateAction)
        {
                Rehydrate();
                return false;
        }

        return true;
    }

    private void Rehydrate()
    {
        var games = _gameLoader.LoadGames().Result.ToList();
        _dispatcher?.Dispatch(new GameListLoadedAction { Games = games ?? new List<GameInfo>() });
    }
}
