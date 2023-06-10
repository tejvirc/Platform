namespace Aristocrat.Monaco.Gaming.Lobby.Redux;

using System;

public interface IStateSelectors<TState>
{
    TState State { get; }

    IObservable<TResult> Select<TResult>(ISelector<TState, TResult> selector);
}
