namespace Aristocrat.Monaco.Gaming.Lobby.Redux;

using System;
using System.Reactive.Subjects;
using Fluxor;

public sealed class FeatureSelector<TState> : ISelector<TState, TState>, IDisposable where TState : class
{
    private readonly IState<TState> _state;

    private readonly Subject<TState> _subject = new();

    public FeatureSelector(IState<TState> state)
    {
        _state = state;

        _state.StateChanged += OnStateChanged;
    }

    public IDisposable Subscribe(IObserver<TState> observer) => _subject.Subscribe(observer);

    public void Dispose()
    {
        _state.StateChanged -= OnStateChanged;

        _subject.Dispose();
    }

    private void OnStateChanged(object? sender, EventArgs e) => _subject.OnNext(_state.Value);
}
