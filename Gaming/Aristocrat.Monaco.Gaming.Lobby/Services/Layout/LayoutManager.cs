namespace Aristocrat.Monaco.Gaming.Lobby.Services.Layout;

using System.Threading;
using System.Threading.Tasks;
using Aristocrat.Monaco.Kernel.Contracts.Events;
using Kernel;
using Microsoft.Extensions.Logging;

public class LayoutManager : ILayoutManager
{
    private readonly ILogger<LayoutManager> _logger;
    private readonly IEventBus _eventBus;

    public LayoutManager(ILogger<LayoutManager> logger, IEventBus eventBus)
    {
        _logger = logger;
        _eventBus = eventBus;

        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        _eventBus.Subscribe<InitializationCompletedEvent>(this, Handle);
    }

    private Task Handle(InitializationCompletedEvent evt, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
