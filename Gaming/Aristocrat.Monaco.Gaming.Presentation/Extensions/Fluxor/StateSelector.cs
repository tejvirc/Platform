namespace Aristocrat.Extensions.Fluxor;

using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

public sealed class StateSelector<TState> : IStateSelector<TState>, IDisposable
{
    private readonly global::Fluxor.IState<TState> _state;
    private readonly BehaviorSubject<TState> _subject;

    private bool _disposed;

    public StateSelector(global::Fluxor.IState<TState> state)
    {
        _state = state;

        _subject = new BehaviorSubject<TState>(_state.Value);

        _state.StateChanged += OnStateChanged;
    }

    public TState State => _state.Value;

    public IObservable<TResult> Select<TResult>(ISelector<TState, TResult> selector) =>
        selector.Apply(_subject);

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _state.StateChanged -= OnStateChanged;

        _disposed = true;
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        _subject.OnNext(_state.Value);
    }
}
