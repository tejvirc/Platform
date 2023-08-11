namespace Aristocrat.Extensions.Fluxor;

using System;

public interface IStateSelector<TState>
{
    TState State { get; }

    IObservable<TResult> Select<TResult>(ISelector<TState, TResult> selector);
}
