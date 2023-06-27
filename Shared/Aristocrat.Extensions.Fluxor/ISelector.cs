namespace Aristocrat.Extensions.Fluxor;

using System;

public interface ISelector
{
    IObservable<TResult> Select<TState, TResult>(ISelector<TState, TResult> selector);
}

public interface ISelector<in TState, out TResult>
{
    TResult Apply(TState state);

    IObservable<TResult> Apply(IObservable<TState> stateObserver);
}
