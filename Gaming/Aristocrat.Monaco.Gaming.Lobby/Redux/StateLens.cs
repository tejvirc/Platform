namespace Aristocrat.Monaco.Gaming.Lobby.Redux;

using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Fluxor;

public sealed class StateLens<TState> : IStateLens<TState>, IDisposable
{
    private readonly IStore _store;
    private readonly IState<TState> _state;
    private readonly BehaviorSubject<TState> _subject;

    private bool _disposed;

    public StateLens(IStore store, IState<TState> state)
    {
        _store = store;
        _state = state;

        _subject = new(_state.Value);

        _state.StateChanged += OnStateChanged;
    }

    public IStore Store => _store;

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
