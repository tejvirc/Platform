namespace Aristocrat.Monaco.Gaming.Presentation.Services;

using System.Threading.Tasks;
using Commands;
using Extensions.Fluxor;
using Fluxor;
using Gaming.Contracts.Lobby;
using Kernel;
using Microsoft.Extensions.Logging;
using Store;

public class PresentationService : IPresentationService
{
    private readonly ILogger<PresentationService> _logger;
    private readonly IStore _store;
    private readonly IDispatcher _dispatcher;
    private readonly IEventBus _eventBus;
    private readonly IOperatorMenuService _operatorMenu;
    private readonly ILayoutManager _layoutManager;
    private readonly IApplicationCommands _commands;

    public PresentationService(
        ILogger<PresentationService> logger,
        IStore store,
        IDispatcher dispatcher,
        IEventBus eventBus,
        IOperatorMenuService operatorMenu,
        ILayoutManager layoutManager,
        IApplicationCommands commands)
    {
        _logger = logger;
        _store = store;
        _dispatcher = dispatcher;
        _eventBus = eventBus;
        _operatorMenu = operatorMenu;
        _layoutManager = layoutManager;
        _commands = commands;
    }

    public async Task StartAsync()
    {
        await _store.InitializeAsync();

        await _dispatcher.DispatchAsync(new StartupAction());

        await _layoutManager.InitializeAsync();

        _operatorMenu.Initialize();

        _eventBus.Publish(new LobbyInitializedEvent());
    }

    public async Task StopAsync()
    {
        await _dispatcher.DispatchAsync(new ShutdownAction());

        _commands.ShutdownCommand.Execute(null);

        await _layoutManager.ShutdownAsync();
    }
}
