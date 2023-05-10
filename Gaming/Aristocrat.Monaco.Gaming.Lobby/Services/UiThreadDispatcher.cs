namespace Aristocrat.Monaco.Gaming.Lobby.Services;

using System;
using System.Windows;
using Fluxor;

public class UiThreadDispatcher : IDispatcher
{
    private readonly IDispatcher _next;

    public UiThreadDispatcher(IDispatcher next)
    {
        _next = next;

        _next.ActionDispatched += OnActionDispatched;
    }

    public void Dispatch(object action)
    {
        Application.Current.Dispatcher.Invoke(() => _next.Dispatch(action));
    }

    public event EventHandler<ActionDispatchedEventArgs>? ActionDispatched;

    private void OnActionDispatched(object? sender, ActionDispatchedEventArgs e)
    {
        ActionDispatched?.Invoke(sender, e);
    }
}
