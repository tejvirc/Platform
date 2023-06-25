namespace Aristocrat.Fluxor.Extensions;

using System;
using System.Reactive.Linq;

public class MemorizedSelector<TState, TSelector1, TResult> : ISelector<TState, TResult>
{
    private readonly ISelector<TState, TSelector1> _selector1;
    private readonly Func<TSelector1, TResult> _projector;

    public MemorizedSelector(ISelector<TState, TSelector1> selector1, Func<TSelector1, TResult> projector)
    {
        _selector1 = selector1;
        _projector = projector;
    }

    public TResult Apply(TState state)
    {
        return _projector(
            _selector1.Apply(state));
    }

    public IObservable<TResult> Apply(IObservable<TState> stateObserver)
    {
        return stateObserver
            .CombineLatest(
                _selector1.Apply(stateObserver),
                (_, s1) => _projector(s1))
            .DistinctUntilChanged();
    }
}

public class MemorizedSelector<TState, TSelector1, TSelector2, TResult> : ISelector<TState, TResult>
{
    private readonly ISelector<TState, TSelector1> _selector1;
    private readonly ISelector<TState, TSelector2> _selector2;
    private readonly Func<TSelector1, TSelector2, TResult> _projector;

    public MemorizedSelector(
        ISelector<TState, TSelector1> selector1,
        ISelector<TState, TSelector2> selector2,
        Func<TSelector1, TSelector2, TResult> projector)
    {
        _selector1 = selector1;
        _selector2 = selector2;
        _projector = projector;
    }

    public TResult Apply(TState state)
    {
        return _projector(
            _selector1.Apply(state),
            _selector2.Apply(state));
    }

    public IObservable<TResult> Apply(IObservable<TState> stateObserver)
    {
        return stateObserver
            .CombineLatest(
                _selector1.Apply(stateObserver),
                _selector2.Apply(stateObserver),
                (_, s1, s2) => _projector(s1, s2))
            .DistinctUntilChanged();
    }
}

public class MemorizedSelector<TState, TSelector1, TSelector2, TSelector3, TResult> : ISelector<TState, TResult>
{
    private readonly ISelector<TState, TSelector1> _selector1;
    private readonly ISelector<TState, TSelector2> _selector2;
    private readonly ISelector<TState, TSelector3> _selector3;
    private readonly Func<TSelector1, TSelector2, TSelector3, TResult> _projector;

    public MemorizedSelector(
        ISelector<TState, TSelector1> selector1,
        ISelector<TState, TSelector2> selector2,
        ISelector<TState, TSelector3> selector3,
        Func<TSelector1, TSelector2, TSelector3, TResult> projector)
    {
        _selector1 = selector1;
        _selector2 = selector2;
        _selector3 = selector3;
        _projector = projector;
    }

    public TResult Apply(TState state)
    {
        return _projector(
            _selector1.Apply(state),
            _selector2.Apply(state),
            _selector3.Apply(state));
    }

    public IObservable<TResult> Apply(IObservable<TState> stateObserver)
    {
        return stateObserver
            .CombineLatest(
                _selector1.Apply(stateObserver),
                _selector2.Apply(stateObserver),
                _selector3.Apply(stateObserver),
                (_, s1, s2, s3) => _projector(s1, s2, s3))
            .DistinctUntilChanged();
    }
}
