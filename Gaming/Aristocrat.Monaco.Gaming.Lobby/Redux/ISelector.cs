namespace Aristocrat.Monaco.Gaming.Lobby.Redux;

using System;

public interface ISelector
{
    IObservable<TResult> Select<TState, TResult>(ISelector<TState, TResult> selector);
}

public interface ISelector<in TState, out TResult>
{
    TResult Apply(TState stateObserver);

    IObservable<TResult> Apply(IObservable<TState> stateObserver);
}
