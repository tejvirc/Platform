namespace Aristocrat.Monaco.Gaming.Presentation;

using System;
using Extensions.Fluxor;
using Fluxor;
using Prism.Ioc;

public static class StoreExtensions
{
    public static IObservable<TResult> Select<TState, TResult>(this IStore _, ISelector<TState, TResult> selector)
    {
        var storeSelector = ContainerLocator.Current.Resolve<IStoreSelector>();
        return storeSelector.Select(selector);
    }
}
