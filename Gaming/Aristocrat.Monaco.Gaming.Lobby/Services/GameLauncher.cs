namespace Aristocrat.Monaco.Gaming.Lobby.Services;

using System;
using Contracts;
using Kernel;
using Kernel.Contracts.Events;

public sealed class GameLauncher : IGameLauncher, IDisposable
{
    private readonly IPropertiesManager _properties;
    private readonly IEventBus _eventBus;

    public GameLauncher(IPropertiesManager properties, IEventBus eventBus)
    {
        _properties = properties;
        _eventBus = eventBus;

        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        _eventBus.Subscribe<InitializationCompletedEvent>(this, Handle);
    }

    public void Dispose()
    {
    }

    private void Handle(InitializationCompletedEvent evt)
    {
        var marketType = _properties.GetValue(GamingConstants.MarketType, MarketType.Unknown);


    }
}
