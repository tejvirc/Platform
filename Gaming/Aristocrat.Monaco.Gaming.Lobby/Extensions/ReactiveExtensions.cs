namespace Aristocrat.Monaco.UI.Common;

using System;
using Aristocrat.Extensions.Fluxor;
using ReactiveUI;
using Splat;

public static class ReactiveExtensions
{
    public static IObservable<TResult> WhenSelect<TState, TResult>(this ReactiveObject rxObject, ISelector<TState, TResult> selector)
    {
        var store = Locator.Current.GetService<IStoreSelector>() ?? throw new InvalidOperationException("Selectors were not registered");

        return rxObject.WhenAnyObservable(_ => store.Select(selector));
    }
}
