namespace Aristocrat.Fluxor.Extensions;

using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

public sealed class StateSelectors<TState> : IStateSelectors<TState>, IDisposable
{
    private readonly global::Fluxor.IState<TState> _state;
    private readonly BehaviorSubject<TState> _subject;

    private bool _disposed;

    public StateSelectors(global::Fluxor.IState<TState> state)
    {
        _state = state;

        _subject = new BehaviorSubject<TState>(_state.Value);

        _state.StateChanged += OnStateChanged;
    }

    public TState State => _state.Value;

    public IObservable<TResult> Select<TResult>(ISelector<TState, TResult> selector) =>
        _subject.Select(_ => selector.Apply(_state.Value)).AsQbservable();

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
