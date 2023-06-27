namespace Aristocrat.Monaco.Gaming.Lobby;

using System;
using Contracts.Lobby;
using global::Fluxor;
using Kernel;
using Microsoft.Extensions.Logging;
using Services;
using Store;

internal class LobbyEngine : ILobby
{
    private readonly ILogger<LobbyEngine> _logger;
    private readonly IStore _store;
    private readonly IDispatcher _dispatcher;
    private readonly IEventBus _eventBus;
    private readonly IPropertiesManager _properties;
    private readonly ILayoutManager _layoutManager;
    private readonly LobbyConfiguration _configuration;

    public LobbyEngine(
        ILogger<LobbyEngine> logger,
        IStore store,
        IDispatcher dispatcher,
        IEventBus eventBus,
        IPropertiesManager properties,
        ILayoutManager layoutManager,
        LobbyConfiguration configuration)
    {
        _logger = logger;
        _store = store;
        _dispatcher = dispatcher;
        _eventBus = eventBus;
        _properties = properties;
        _layoutManager = layoutManager;
        _configuration = configuration;
    }

    public void CreateWindow()
    {
        _store.InitializeAsync().Wait();

        _dispatcher.Dispatch(new StartupAction(_configuration));

        _layoutManager.CreateWindows();
    }

    public void Show() => throw new NotSupportedException();

    public void Hide() => throw new NotSupportedException();

    public void Close()
    {
        _layoutManager.DestroyWindows();

        _dispatcher.Dispatch(new ShutdownAction());
    }
}
