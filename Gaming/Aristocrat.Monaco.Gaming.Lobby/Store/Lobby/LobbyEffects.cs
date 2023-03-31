namespace Aristocrat.Monaco.Gaming.Lobby.Store.Lobby;

using System.Linq;
using System.Threading.Tasks;
using Aristocrat.Monaco.Application.Contracts;
using Fluxor;
using Services;
using Vgt.Client12.Application.OperatorMenu;

public class LobbyEffects
{
    private readonly IOperatorMenuLauncher _operatorMenu;
    private readonly IGameLoader _gameLoader;

    public LobbyEffects(IOperatorMenuLauncher operatorMenu, IGameLoader gameLoader)
    {
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
    public Task Effect(InitializeAction action, IDispatcher dispatcher)
    {
        _operatorMenu.EnableKey(ApplicationConstants.OperatorMenuInitializationKey);
        return Task.CompletedTask;
    }
}
