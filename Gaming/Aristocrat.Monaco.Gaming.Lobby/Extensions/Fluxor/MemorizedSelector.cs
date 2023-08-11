namespace Aristocrat.Extensions.Fluxor;

using System;
using System.Reactive.Linq;

public class MemorizedSelector<TState, TSelector1, TResult> : ISelector<TState, TResult>
{
    private readonly ISelector<TState, TSelector1> _selector1;
    private readonly Func<TSelector1, TResult> _projector;

    private IObservable<TSelector1>? _selectorObservable1;

    public MemorizedSelector(
        ISelector<TState, TSelector1> selector1,
        Func<TSelector1, TResult> projector)
    {
        _selector1 = selector1;
        _projector = projector;
    }

    public TResult Apply(TState state)
    {
        return _projector(
            _selector1.Apply(state));
    }

    public IObservable<TResult> Apply(IObservable<TState> stateObservable)
    {
        if (_selectorObservable1 == null)
        {
            _selectorObservable1 = _selector1.Apply(stateObservable);
        }

        return stateObservable
            .CombineLatest(
                _selectorObservable1,
                (_, s1) => _projector(s1))
            .DistinctUntilChanged();
    }
}

public class MemorizedSelector<TState, TSelector1, TSelector2, TResult> : ISelector<TState, TResult>
{
    private readonly ISelector<TState, TSelector1> _selector1;
    private readonly ISelector<TState, TSelector2> _selector2;
    private readonly Func<TSelector1, TSelector2, TResult> _projector;

    private IObservable<TSelector1>? _selectorObservable1;
    private IObservable<TSelector2>? _selectorObservable2;

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

    public IObservable<TResult> Apply(IObservable<TState> stateObservable)
    {
        if (_selectorObservable1 == null)
        {
            _selectorObservable1 = _selector1.Apply(stateObservable);
        }

        if (_selectorObservable2 == null)
        {
            _selectorObservable2 = _selector2.Apply(stateObservable);
        }

        return stateObservable
            .CombineLatest(
                _selectorObservable1,
                _selectorObservable2,
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

    private IObservable<TSelector1>? _selectorObservable1;
    private IObservable<TSelector2>? _selectorObservable2;
    private IObservable<TSelector3>? _selectorObservable3;

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

    public IObservable<TResult> Apply(IObservable<TState> stateObservable)
    {
        if (_selectorObservable1 == null)
        {
            _selectorObservable1 = _selector1.Apply(stateObservable);
        }

        if (_selectorObservable2 == null)
        {
            _selectorObservable2 = _selector2.Apply(stateObservable);
        }

        if (_selectorObservable3 == null)
        {
            _selectorObservable3 = _selector3.Apply(stateObservable);
        }

        return stateObservable
            .CombineLatest(
                _selectorObservable1,
                _selectorObservable2,
                _selectorObservable3,
                (_, s1, s2, s3) => _projector(s1, s2, s3))
            .DistinctUntilChanged();
    }
}

public class MemorizedSelector<TState, TSelector1, TSelector2, TSelector3, TSelector4, TResult> : ISelector<TState, TResult>
{
    private readonly ISelector<TState, TSelector1> _selector1;
    private readonly ISelector<TState, TSelector2> _selector2;
    private readonly ISelector<TState, TSelector3> _selector3;
    private readonly ISelector<TState, TSelector4> _selector4;
    private readonly Func<TSelector1, TSelector2, TSelector3, TSelector4, TResult> _projector;

    private IObservable<TSelector1>? _selectorObservable1;
    private IObservable<TSelector2>? _selectorObservable2;
    private IObservable<TSelector3>? _selectorObservable3;
    private IObservable<TSelector4>? _selectorObservable4;

    public MemorizedSelector(
        ISelector<TState, TSelector1> selector1,
        ISelector<TState, TSelector2> selector2,
        ISelector<TState, TSelector3> selector3,
        ISelector<TState, TSelector4> selector4,
        Func<TSelector1, TSelector2, TSelector3, TSelector4, TResult> projector)
    {
        _selector1 = selector1;
        _selector2 = selector2;
        _selector3 = selector3;
        _selector4 = selector4;
        _projector = projector;
    }

    public TResult Apply(TState state)
    {
        return _projector(
            _selector1.Apply(state),
            _selector2.Apply(state),
            _selector3.Apply(state),
            _selector4.Apply(state));
    }

    public IObservable<TResult> Apply(IObservable<TState> stateObservable)
    {
        if (_selectorObservable1 == null)
        {
            _selectorObservable1 = _selector1.Apply(stateObservable);
        }

        if (_selectorObservable2 == null)
        {
            _selectorObservable2 = _selector2.Apply(stateObservable);
        }

        if (_selectorObservable3 == null)
        {
            _selectorObservable3 = _selector3.Apply(stateObservable);
        }

        if (_selectorObservable4 == null)
        {
            _selectorObservable4 = _selector4.Apply(stateObservable);
        }

        return stateObservable
            .CombineLatest(
                _selectorObservable1,
                _selectorObservable2,
                _selectorObservable3,
                _selectorObservable4,
                (_, s1, s2, s3, s4) => _projector(s1, s2, s3, s4))
            .DistinctUntilChanged();
    }
}
