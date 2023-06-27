namespace Aristocrat.Monaco.Gaming.Lobby.Store.Lobby;

using System.Linq;
using System.Threading.Tasks;
using Aristocrat.Monaco.Gaming.Lobby.Services.Attract;
using Aristocrat.Monaco.Gaming.Lobby.Services.EdgeLighting;
using Commands;
using Controllers;
using Controllers.Attract;
using Controllers.EdgeLighting;
using Fluxor;
using Microsoft.Extensions.Logging;
using Services;

public partial class LobbyEffects
{
    private readonly ILogger<LobbyEffects> _logger;
    private readonly IState<LobbyState> _state;
    private readonly LobbyConfiguration _configuration;
    private readonly IAttractService _attractService;
    private readonly IEdgeLightingService _edgeLightingService;
    private readonly IOperatorMenuController _operatorMenu;
    private readonly IGameLoader _gameLoader;
    private readonly IApplicationCommands _commands;

    public LobbyEffects(
        ILogger<LobbyEffects> logger,
        IState<LobbyState> state,
        LobbyConfiguration configuration,
        IAttractService attractService,
        IEdgeLightingService edgeLightingService,
        IOperatorMenuController operatorMenu,
        IGameLoader gameLoader,
        IApplicationCommands commands)
    {
        _logger = logger;
        _state = state;
        _configuration = configuration;
        _attractService = attractService;
        _edgeLightingService = edgeLightingService;
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
    public Task Initialize(SystemInitializedAction _, IDispatcher dispatcher)
    {
        _operatorMenu.Enable();

        dispatcher.Dispatch(new LoadGamesAction());

        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task Shutdown(ShutdownAction _)
    {
        _commands.ShutdownCommand.Execute(null);

        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task RotateTopImage(AttractExitAction _, IDispatcher dispatcher)
    {
        if (!_state.Value.IsRotateTopImageAfterAttractVideo)
        {
            return Task.CompletedTask;
        }

        var newIndex = _state.Value.AttractModeTopImageIndex + 1;

        if (newIndex < 0 || newIndex >= _state.Value.RotateTopImageAfterAttractVideoCount)
        {
            newIndex = 0;
        }

        dispatcher.Dispatch(new UpdateAttractModeTopImageIndex { Index = newIndex });

        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task RotateTopperImage(AttractExitAction _, IDispatcher dispatcher)
    {
        if (!_state.Value.IsRotateTopperImageAfterAttractVideo)
        {
            return Task.CompletedTask;
        }

        var newIndex = _state.Value.AttractModeTopperImageIndex + 1;

        if (newIndex < 0 || newIndex >= _configuration.RotateTopperImageAfterAttractVideo.Length)
        {
            newIndex = 0;
        }

        dispatcher.Dispatch(new UpdateAttractModeTopperImageIndex { Index = newIndex });

        return Task.CompletedTask;
    }
}
