namespace Aristocrat.Extensions.Fluxor;

using System;

public interface IStateSelectors<TState>
{
    TState State { get; }

    IObservable<TResult> Select<TResult>(ISelector<TState, TResult> selector);
}
