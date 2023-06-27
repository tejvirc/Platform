namespace Aristocrat.Extensions.Fluxor;

using System;
using System.Reactive.Linq;

public class Selector<TState, TResult> : ISelector<TState, TResult>
{
    private readonly Func<TState, TResult> _projector;

    public Selector(Func<TState, TResult> projector)
    {
        _projector = projector;
    }

    public TResult Apply(TState state)
    {
        return _projector.Invoke(state);
    }

    public IObservable<TResult> Apply(IObservable<TState> stateObserver)
    {
        return stateObserver
            .Select(Apply)
            .DistinctUntilChanged();
    }
}
