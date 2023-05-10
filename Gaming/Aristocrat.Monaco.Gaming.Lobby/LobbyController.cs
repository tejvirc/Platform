namespace Aristocrat.Monaco.Gaming.Lobby;

using System;
using System.Threading;
using System.Threading.Tasks;
using Aristocrat.Monaco.Kernel.Contracts.Events;
using Contracts;
using Contracts.Lobby;
using Fluxor;
using Kernel;
using Store;

internal class LobbyController : ILobby
{
    private readonly IStore _store;
    private readonly IDispatcher _dispatcher;
    private readonly IEventBus _eventBus;
    private readonly IPropertiesManager _properties;

    public LobbyController(
        IEventBus eventBus,
        IPropertiesManager properties,
        IStore store,
        IDispatcher dispatcher)
    {
        _eventBus = eventBus;
        _properties = properties;
        _store = store;
        _dispatcher = dispatcher;

        SubscribeToEvents();
    }

    public void CreateWindow()
    {
        _store.InitializeAsync().Wait();

        var config = _properties.GetValue<LobbyConfiguration?>(GamingConstants.LobbyConfig, null);

        if (config == null)
        {
            throw new InvalidOperationException("Lobby configuration not set");
        }

        _dispatcher.Dispatch(new StartupAction(config));
    }

    public void Show() => throw new NotSupportedException();

    public void Hide() => throw new NotSupportedException();

    public void Close()
    {
        _dispatcher.Dispatch(new ShutdownAction());
    }

    private void SubscribeToEvents()
    {
        _eventBus.Subscribe<InitializationCompletedEvent>(this, Handle);
    }

    private Task Handle(InitializationCompletedEvent evt, CancellationToken cancellationToken)
    {
        _dispatcher.Dispatch(new InitializeAction());

        return Task.CompletedTask;
    }
}
