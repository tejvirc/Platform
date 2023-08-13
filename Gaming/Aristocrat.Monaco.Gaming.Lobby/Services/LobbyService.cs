namespace Aristocrat.Monaco.Gaming.Lobby.Services;

using System;
using System.Threading.Tasks;
using Application.Contracts;
using Commands;
using Contracts.Lobby;
using Fluxor;
using Kernel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Services;
using Store;
using Vgt.Client12.Application.OperatorMenu;

public class LobbyService : ILobby
{
    private readonly ILogger<LobbyService> _logger;
    private readonly IStore _store;
    private readonly IDispatcher _dispatcher;
    private readonly LobbyConfiguration _configuration;
    private readonly IEventBus _eventBus;
    private readonly IOperatorMenuLauncher _operatorMenuLauncher;
    private readonly ILayoutManager _layoutManager;
    private readonly IApplicationCommands _commands;

    public LobbyService(
        ILogger<LobbyService> logger,
        IStore store,
        IDispatcher dispatcher,
        LobbyConfiguration configuration,
        IEventBus eventBus,
        IOperatorMenuLauncher operatorMenuLauncher,
        ILayoutManager layoutManager,
        IApplicationCommands commands)
    {
        _logger = logger;
        _store = store;
        _dispatcher = dispatcher;
        _configuration = configuration;
        _eventBus = eventBus;
        _operatorMenuLauncher = operatorMenuLauncher;
        _layoutManager = layoutManager;
        _commands = commands;
    }

    public void CreateWindow()
    {
        _store.InitializeAsync().Wait();

        _dispatcher.Dispatch(new StartupAction(_configuration));

        _layoutManager.InitializeAsync().Wait();

        _dispatcher.Dispatch(new LobbyInitializedAction());

        _operatorMenuLauncher.EnableKey(ApplicationConstants.OperatorMenuInitializationKey);

        _eventBus.Publish(new LobbyInitializedEvent());
    }

    public void Show() => throw new NotSupportedException();

    public void Hide() => throw new NotSupportedException();

    public void Close()
    {
        _dispatcher.Dispatch(new ShutdownAction());

        _commands.ShutdownCommand.Execute(null);

        _layoutManager.DestroyWindows();
    }

    public async Task StartAsync()
    {
        await _store.InitializeAsync();

        await _dispatcher.DispatchAsync(new StartupAction(_configuration));
    }

    public async Task StopAsync()
    {
        await _dispatcher.DispatchAsync(new ShutdownAction());

        _commands.ShutdownCommand.Execute(null);
    }
}
