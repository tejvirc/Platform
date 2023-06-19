namespace Aristocrat.Monaco.Gaming.Lobby.Services;

using System;
using Application.Contracts.Localization;
using Aristocrat.Monaco.Gaming.Contracts;
using Aristocrat.Monaco.Gaming.Contracts.Models;
using global::Fluxor;
using Kernel;
using Store;
using UI.Common;

public class IdleTextController
{
    private const double IdleTextTimerIntervalSeconds = 30.0;

    private readonly IDispatcher _dispatcher;
    private readonly IEventBus _eventBus;
    private readonly IPropertiesManager _properties;

    private ITimer _idleTextTimer;

    public IdleTextController(IDispatcher dispatcher, IEventBus eventBus, IPropertiesManager properties)
    {
        _dispatcher = dispatcher;
        _eventBus = eventBus;
        _properties = properties;

        _idleTextTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(IdleTextTimerIntervalSeconds) };
        _idleTextTimer.Tick += IdleTextTimerTick;

        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        _eventBus.Subscribe<PropertyChangedEvent>(this, Handle);
    }

    private void Handle(PropertyChangedEvent evt)
    {
        if (evt.PropertyName == GamingConstants.IdleText)
        {
            UpdateIdelText();
        }
    }

    private void UpdateIdelText()
    {
        var text = _properties.GetValue<string?>(GamingConstants.IdleText, null);
        _dispatcher.Dispatch(new UpdateIdleTextAction { Text = text });
    }

    private void IdleTextTimerTick(object? sender, EventArgs e)
    {
        _idleTextTimer?.Stop();
    }
}
