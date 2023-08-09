namespace Aristocrat.Monaco.Gaming.Lobby.Services.Attract;

using System;
using Contracts.Events;
using Kernel;
using Microsoft.Extensions.Logging;

public sealed class AttractAgent : IAttractAgent, IDisposable
{
    private readonly ILogger<AttractAgent> _logger;
    private readonly IEventBus _eventBus;

    public AttractAgent(ILogger<AttractAgent> logger, IEventBus eventBus)
    {
        _logger = logger;
        _eventBus = eventBus;
    }

    public void NotifyEntered()
    {
        _eventBus.Publish(new AttractModeEntered());
    }

    public void Dispose()
    {
        _eventBus.UnsubscribeAll(this);
    }
}
