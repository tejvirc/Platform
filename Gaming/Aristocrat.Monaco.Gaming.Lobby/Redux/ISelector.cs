namespace Aristocrat.Monaco.Gaming.Lobby.Redux;

using System;

public interface ISelector<TState, TResult>
{
    TResult Apply(TState stateObserver);

    IObservable<TResult> Apply(IObservable<TState> stateObserver);
}
