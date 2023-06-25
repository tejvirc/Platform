namespace Aristocrat.Monaco.Gaming.Lobby.Store.Lobby;

using System.Linq;
using System.Threading.Tasks;
using Commands;
using Controllers;
using global::Fluxor;
using Microsoft.Extensions.Logging;
using Services;

public partial class LobbyEffects
{
    private readonly ILogger<LobbyEffects> _logger;
    private readonly IState<LobbyState> _state;
    private readonly LobbyConfiguration _configuration;
    private readonly IControllerFactory _controllers;
    private readonly ILayoutManager _layoutManager;
    private readonly IOperatorMenuController _operatorMenu;
    private readonly IGameLoader _gameLoader;
    private readonly IApplicationCommands _commands;

    public LobbyEffects(
        ILogger<LobbyEffects> logger,
        IState<LobbyState> state,
        LobbyConfiguration configuration,
        IControllerFactory controllers,
        ILayoutManager layoutManager,
        IOperatorMenuController operatorMenu,
        IGameLoader gameLoader,
        IApplicationCommands commands)
    {
        _logger = logger;
        _state = state;
        _configuration = configuration;
        _controllers = controllers;
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
    public Task Startup(StartupAction _, IDispatcher dispatcher)
    {
        _layoutManager.CreateWindows();

        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task Initialize(SystemInitializedAction action, IDispatcher dispatcher)
    {
        _operatorMenu.Enable();

        dispatcher.Dispatch(new LoadGamesAction());

        return Task.CompletedTask;
    }

    [EffectMethod]
    public Task Shutdown(ShutdownAction _, IDispatcher dispatcher)
    {
        _layoutManager.DestroyWindows();

        _commands.ShutdownCommand.Execute(null);

        return Task.CompletedTask;
    }

    [EffectMethod]
    public async Task AttractVideoCompleted(AttractVideoCompletedAction payload, IDispatcher dispatcher)
    {
        var consecutiveAttractCount = _state.Value.ConsecutiveAttractCount;

        if (!_state.Value.HasAttractIntroVideo || _state.Value.CurrentAttractIndex != 0 || AttractList.Count <= 1)
        {
            consecutiveAttractCount++;

            _logger.LogDebug("Consecutive Attract Video count: {ConsecutiveAttractCount}", consecutiveAttractCount);

            if (consecutiveAttractCount >= _state.Value.ConsecutiveAttractVideos ||
                consecutiveAttractCount >= _state.Value.Games.Count)
            {
                _logger.LogDebug("Stopping attract video sequence");

                await dispatcher.DispatchAsync(new AttractExitAction { ConsecutiveAttractCount = consecutiveAttractCount});

                return;
            }

            _logger.LogDebug("Starting another attract video");
        }

        await dispatcher.DispatchAsync(new AttractNextVideoAction { ConsecutiveAttractCount = consecutiveAttractCount });

        Task.Run(
            () =>
            {
                if (AttractList.Count <= 1)
                {
                    StopAndUnloadAttractVideo();
                }

                AdvanceAttractIndex();
                SetAttractVideos();
            });

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
