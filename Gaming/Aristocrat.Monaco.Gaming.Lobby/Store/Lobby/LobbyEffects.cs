namespace Aristocrat.Monaco.Gaming.Lobby.Store.Lobby;

using System.Linq;
using System.Threading.Tasks;
using Aristocrat.Monaco.Application.Contracts;
using Aristocrat.Monaco.Gaming.Lobby.Services.OperatorMenu;
using Fluxor;
using Services;
using Services.Layout;
using Vgt.Client12.Application.OperatorMenu;

public class LobbyEffects
{
    private readonly ILayoutManager _layoutManager;
    private readonly IOperatorMenuController _operatorMenu;
    private readonly IGameLoader _gameLoader;

    public LobbyEffects(
        ILayoutManager layoutManager,
        IOperatorMenuController operatorMenu,
        IGameLoader gameLoader)
    {
        _layoutManager = layoutManager;
        _operatorMenu = operatorMenu;
        _gameLoader = gameLoader;
    }

    [EffectMethod]
    public async Task Effect(LoadGamesAction action, IDispatcher dispatcher)
    {
        var games = (await _gameLoader.LoadGames()).ToList();
        dispatcher.Dispatch(new GamesLoadedAction(action.Trigger, games));
    }

    [EffectMethod]
    public Task Effect(StartupAction _, IDispatcher dispatcher)
    {
        _layoutManager.CreateWindows();

        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task Effect(InitializeAction action, IDispatcher dispatcher)
    {
        _operatorMenu.Enable();

        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task Effect(ShutdownAction _, IDispatcher dispatcher)
    {
        _layoutManager.DestroyWindows();

        return Task.CompletedTask;
    }
}
