namespace Aristocrat.Monaco.Gaming.Lobby.Redux;

using Fluxor;
using System;

public interface IStateLens<TState>
{
    IStore Store { get; }

    TState State { get; }

    IObservable<TResult> Select<TResult>(ISelector<TState, TResult> selector);
}
