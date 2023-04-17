namespace Aristocrat.Monaco.Gaming.Lobby.Services.Chooser;

using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Contracts;
using Fluxor;
using Kernel;
using Kernel.Contracts.Events;
using Microsoft.Extensions.Logging;
using Stateless;

public class Chooser : IChooser
{
    private readonly ILogger<Chooser> _logger;
    private readonly IEventBus _eventBus;
    private readonly IDispatcher _dispatcher;

    public Chooser(ILogger<Chooser> logger, IEventBus eventBus, IDispatcher dispatcher)
    {
        _logger = logger;
        _eventBus = eventBus;
        _dispatcher = dispatcher;

        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        _eventBus.Subscribe<InitializationCompletedEvent>(this, Handle);
        _eventBus.Subscribe<GameAddedEvent>(this, Handle);
        _eventBus.Subscribe<GameDenomChangedEvent>(this, Handle);
        _eventBus.Subscribe<GameDisabledEvent>(this, Handle);
        _eventBus.Subscribe<GameEnabledEvent>(this, Handle);
        _eventBus.Subscribe<GameOrderChangedEvent>(this, Handle);
        _eventBus.Subscribe<GameRemovedEvent>(this, Handle);
        _eventBus.Subscribe<GameUpgradedEvent>(this, Handle);
    }

    private Task Handle(InitializationCompletedEvent evt, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private Task Handle(GameAddedEvent evt, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private Task Handle(GameDenomChangedEvent evt, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private Task Handle(GameDisabledEvent evt, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private Task Handle(GameEnabledEvent evt, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private Task Handle(GameOrderChangedEvent evt, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private Task Handle(GameRemovedEvent evt, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private Task Handle(GameUpgradedEvent evt, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
