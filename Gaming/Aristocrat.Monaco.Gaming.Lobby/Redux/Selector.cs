namespace Aristocrat.Monaco.Gaming.Lobby.Redux;

using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

public abstract class MemorizedSelector<TState, TResult> : ISelector<TState, TResult>, IDisposable
    where TState : class
{
    private readonly Subject<TResult> _subject = new();

    private TResult? _lastValue;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public IDisposable Subscribe(IObserver<TResult> observer) => _subject.Subscribe(observer);

    protected void OnNext(TResult value)
    {
        if (DefaultValueEquals(value, _lastValue))
        {
            return;
        }

        _lastValue = value;

        _subject.OnNext(value);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _subject.Dispose();
        }
    }

    private static bool DefaultValueEquals<T>(T x, T y) => (object?)x == (object?)y || (x is IEquatable<T> equatable ? equatable.Equals(y) ? 1 : 0 : 0) != 0 || Equals(x, y);
}

public class MemorizedSelector<TState, TSelector1, TResult> : MemorizedSelector<TState, TResult>
    where TState : class
{
    private readonly IDisposable _subscription;

    public MemorizedSelector(ISelector<TState, TSelector1> selector1, Func<TSelector1, TResult> projector)
    {
        _subscription = selector1
            .Select(projector.Invoke)
            .Subscribe(OnNext);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _subscription.Dispose();
        }
    }
}

public class Selector<TState, TSelector1, TSelector2, TResult> : MemorizedSelector<TState, TResult>
    where TState : class
{
    private readonly IDisposable _subscription;

    public Selector(
        ISelector<TState, TSelector1> selector1,
        ISelector<TState, TSelector2> selector2,
        Func<TSelector1, TSelector2, TResult> projector)
    {
        _subscription = selector1.CombineLatest(selector2, projector.Invoke)
            .Subscribe(OnNext);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _subscription.Dispose();
        }
    }
}

public sealed class
    MemorizedSelector<TState, TSelector1, TSelector2, TSelector3, TResult> : MemorizedSelector<TState, TResult>
    where TState : class
{
    private readonly IDisposable _subscription;

    public MemorizedSelector(
        ISelector<TState, TSelector1> selector1,
        ISelector<TState, TSelector2> selector2,
        ISelector<TState, TSelector3> selector3,
        Func<TSelector1, TSelector2, TSelector3, TResult> projector)
    {
        _subscription = selector1.CombineLatest(selector2, selector3, projector.Invoke)
            .Subscribe(OnNext);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _subscription.Dispose();
        }
    }
}

public class
    MemorizedSelector<TState, TSelector1, TSelector2, TSelector3, TSelector4,
        TResult> : MemorizedSelector<TState, TResult>
    where TState : class
{
    private readonly IDisposable _subscription;

    public MemorizedSelector(
        ISelector<TState, TSelector1> selector1,
        ISelector<TState, TSelector2> selector2,
        ISelector<TState, TSelector3> selector3,
        ISelector<TState, TSelector4> selector4,
        Func<TSelector1, TSelector2, TSelector3, TSelector4, TResult> projector)
    {
        _subscription = selector1.CombineLatest(
                selector2,
                selector3,
                selector4,
                projector.Invoke)
            .Subscribe(OnNext);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _subscription.Dispose();
        }
    }
}

public sealed class
    MemorizedSelector<TState, TSelector1, TSelector2, TSelector3, TSelector4, TSelector5,
        TResult> : MemorizedSelector<TState, TResult>
    where TState : class
{
    private readonly IDisposable _subscription;

    public MemorizedSelector(
        ISelector<TState, TSelector1> selector1,
        ISelector<TState, TSelector2> selector2,
        ISelector<TState, TSelector3> selector3,
        ISelector<TState, TSelector4> selector4,
        ISelector<TState, TSelector5> selector5,
        Func<TSelector1, TSelector2, TSelector3, TSelector4, TSelector5, TResult> projector)
    {
        _subscription = selector1.CombineLatest(
                selector2,
                selector3,
                selector4,
                selector5,
                projector.Invoke)
            .Subscribe(OnNext);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _subscription.Dispose();
        }
    }
}
