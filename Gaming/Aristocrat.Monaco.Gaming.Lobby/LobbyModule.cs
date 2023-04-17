namespace Aristocrat.Monaco.Gaming.Lobby;

using System.Threading;
using System.Threading.Tasks;
using Aristocrat.Monaco.Gaming.Lobby.Services.Layout;
using Aristocrat.Monaco.Kernel.Contracts.Events;
using Fluxor;
using Kernel;
using Microsoft.Extensions.Logging;
using Services;

public class LobbyModule
{
    private readonly ILogger<LobbyModule> _logger;
    private readonly IEventBus _eventBus;
    private readonly IDispatcher _dispatcher;
    private readonly IViewComposer _composer;

    public LobbyModule(ILogger<LobbyModule> logger, IEventBus eventBus, IDispatcher dispatcher, IViewComposer composer)
    {
        _logger = logger;
        _eventBus = eventBus;
        _dispatcher = dispatcher;
        _composer = composer;

        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        _eventBus.Subscribe<InitializationCompletedEvent>(this, Handle);
    }

    private Task Handle(InitializationCompletedEvent evt, CancellationToken cancellationToken)
    {
        _composer.ComposeAsync();

        return Task.CompletedTask;
    }
}
