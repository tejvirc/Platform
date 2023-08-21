namespace Aristocrat.Monaco.Gaming.Presentation;

using System;
using System.Reactive.Linq;

public static class ObservableExtensions
{
    public static IObservable<bool> WhenTrue(this IObservable<bool> source)
    {
        return source.Where(x => x);
    }

    public static IObservable<TSource> WhenTrue<TSource>(this IObservable<TSource> source, Predicate<TSource> predicate)
    {
        return source.Where(x => predicate(x));
    }

    public static IObservable<bool> WhenFalse(this IObservable<bool> source)
    {
        return source.Where(x => !x);
    }

    public static IObservable<TSource> WhenFalse<TSource>(this IObservable<TSource> source, Predicate<TSource> predicate)
    {
        return source.Where(x => !predicate(x));
    }
}
