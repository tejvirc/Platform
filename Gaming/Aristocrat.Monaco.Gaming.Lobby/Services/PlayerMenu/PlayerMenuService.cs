namespace Aristocrat.Monaco.Gaming.Lobby.Services.PlayerMenu;

using System.Threading;
using System.Threading.Tasks;
using Fluxor;
using Hardware.Contracts.Button;
using Kernel;
using Microsoft.Extensions.Logging;

public class PlayerMenuService : IPlayerMenu
{
    private readonly ILogger<PlayerMenuService> _logger;
    private readonly IEventBus _eventBus;
    private readonly IDispatcher _dispatcher;

    public PlayerMenuService(ILogger<PlayerMenuService> logger, IEventBus eventBus, IDispatcher dispatcher)
    {
        _logger = logger;
        _eventBus = eventBus;
        _dispatcher = dispatcher;

        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        _eventBus.Subscribe<SystemDownEvent>(this, Handle);
    }

    private Task Handle(SystemDownEvent evt, CancellationToken cancellationToken)
    {
        _dispatcher.Dispatch(new PlayerMenuEnabled(false));

        return Task.CompletedTask;
    }
}
