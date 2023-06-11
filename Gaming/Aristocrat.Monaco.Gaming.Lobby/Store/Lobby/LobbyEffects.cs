namespace Aristocrat.Monaco.Gaming.Lobby.Store.Lobby;

using System.Linq;
using System.Threading.Tasks;
using Commands;
using Fluxor;
using Services;

public class LobbyEffects
{
    private readonly ILayoutManager _layoutManager;
    private readonly IOperatorMenuController _operatorMenu;
    private readonly IGameLoader _gameLoader;
    private readonly IApplicationCommands _commands;

    public LobbyEffects(
        ILayoutManager layoutManager,
        IOperatorMenuController operatorMenu,
        IGameLoader gameLoader,
        IApplicationCommands commands)
    {
        _layoutManager = layoutManager;
        _operatorMenu = operatorMenu;
        _gameLoader = gameLoader;
        _commands = commands;
    }

    [EffectMethod]
    public async Task Effect(LoadGamesAction _, IDispatcher dispatcher)
    {
        var games = (await _gameLoader.LoadGames()).ToList();
        dispatcher.Dispatch(new GamesLoadedAction(games));
    }

    [EffectMethod]
    public Task Effect(StartupAction _, IDispatcher dispatcher)
    {
        _layoutManager.CreateWindows();

        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task Effect(SystemInitializedAction action, IDispatcher dispatcher)
    {
        _operatorMenu.Enable();

        dispatcher.Dispatch(new LoadGamesAction());

        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task Effect(ShutdownAction _, IDispatcher dispatcher)
    {
        _layoutManager.DestroyWindows();

        _commands.ShutdownCommand.Execute(null);

        return Task.CompletedTask;
    }
}
