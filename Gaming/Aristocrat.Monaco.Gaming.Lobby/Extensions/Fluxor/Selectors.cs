namespace Aristocrat.Extensions.Fluxor;

using System;

public static class Selectors
{
    public static ISelector<TState, TResult> CreateSelector<TState, TResult>(Func<TState, TResult> projector) =>
        new Selector<TState, TResult>(projector);

    public static ISelector<TState, TResult> CreateSelector<TState, TSelector1, TResult>(
        ISelector<TState, TSelector1> selector1,
        Func<TSelector1, TResult> projector) =>
        new MemorizedSelector<TState, TSelector1, TResult>(selector1, projector);

    public static ISelector<TState, TResult> CreateSelector<TState, TSelector1, TSelector2, TResult>(
        ISelector<TState, TSelector1> selector1,
        ISelector<TState, TSelector2> selector2,
        Func<TSelector1, TSelector2, TResult> projector) =>
        new MemorizedSelector<TState, TSelector1, TSelector2, TResult>(selector1, selector2, projector);

    public static ISelector<TState, TResult> CreateSelector<TState, TSelector1, TSelector2, TSelector3, TResult>(
        ISelector<TState, TSelector1> selector1,
        ISelector<TState, TSelector2> selector2,
        ISelector<TState, TSelector3> selector3,
        Func<TSelector1, TSelector2, TSelector3, TResult> projector) =>
        new MemorizedSelector<TState, TSelector1, TSelector2, TSelector3, TResult>(
            selector1,
            selector2,
            selector3,
            projector);
}
