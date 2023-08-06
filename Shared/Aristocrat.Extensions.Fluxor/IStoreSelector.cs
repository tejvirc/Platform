namespace Aristocrat.Extensions.Fluxor;

using System;

public interface IStoreSelector
{
    IObservable<TResult> Select<TState, TResult>(ISelector<TState, TResult> selector);
}
