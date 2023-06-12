namespace Aristocrat.Fluxor.Extensions;

using System;

public interface IStateSelectors<TState>
{
    TState State { get; }

    IObservable<TResult> Select<TResult>(ISelector<TState, TResult> selector);
}
