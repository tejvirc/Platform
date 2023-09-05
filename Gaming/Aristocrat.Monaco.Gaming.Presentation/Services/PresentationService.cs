namespace Aristocrat.Monaco.Gaming.Presentation.Services;

using System;
using System.Threading.Tasks;
using Application.Contracts;
using Commands;
using Extensions.Fluxor;
using Fluxor;
using Gaming.Contracts.Lobby;
using Kernel;
using Microsoft.Extensions.Logging;
using Store;
using Vgt.Client12.Application.OperatorMenu;

public class PresentationService : IPresentation, ILobby
{
    private readonly ILogger<PresentationService> _logger;
    private readonly IStore _store;
    private readonly IDispatcher _dispatcher;
    private readonly IEventBus _eventBus;
    private readonly IOperatorMenuLauncher _operatorMenuLauncher;
    private readonly ILayoutManager _layoutManager;
    private readonly IApplicationCommands _commands;

    public PresentationService(
        ILogger<PresentationService> logger,
        IStore store,
        IDispatcher dispatcher,
        IEventBus eventBus,
        IOperatorMenuLauncher operatorMenuLauncher,
        ILayoutManager layoutManager,
        IApplicationCommands commands)
    {
        _logger = logger;
        _store = store;
        _dispatcher = dispatcher;
        _eventBus = eventBus;
        _operatorMenuLauncher = operatorMenuLauncher;
        _layoutManager = layoutManager;
        _commands = commands;
    }

    // The ILobby service will eventually be removed for another service to boot up the
    // presentation services for the platform. The lobby may not be owned by the platform
    // in the future. However, the platform will continue to have overlays that need to
    // display over the game and lobby windows.
    public void CreateWindow()
    {
        StartAsync().Wait();
    }

    public void Show() => throw new NotSupportedException();

    public void Hide() => throw new NotSupportedException();

    public void Close()
    {
        StopAsync().Wait();
    }

    public async Task StartAsync()
    {
        await _store.InitializeAsync();

        await _dispatcher.DispatchAsync(new StartupAction());

        await _layoutManager.InitializeAsync();

        _operatorMenuLauncher.EnableKey(ApplicationConstants.OperatorMenuInitializationKey);

        _eventBus.Publish(new LobbyInitializedEvent());
    }

    public async Task StopAsync()
    {
        await _dispatcher.DispatchAsync(new ShutdownAction());

        _commands.ShutdownCommand.Execute(null);

        await _layoutManager.ShutdownAsync();
    }
}
