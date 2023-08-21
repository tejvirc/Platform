namespace Aristocrat.Extensions.Fluxor;

using System;

public interface ISelector<in TState, out TResult>
{
    TResult Apply(TState state);

    IObservable<TResult> Apply(IObservable<TState> stateObservable);
}
