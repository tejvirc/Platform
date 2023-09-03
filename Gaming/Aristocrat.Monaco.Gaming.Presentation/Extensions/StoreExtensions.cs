namespace Aristocrat.Monaco.Gaming.Presentation;

using System;
using System.Linq;
using Extensions.Fluxor;
using Fluxor;
using Prism.Ioc;
using Store.Bank;

public static class StoreExtensions
{
    public static IObservable<TResult> Select<TState, TResult>(this IStore _, ISelector<TState, TResult> selector)
    {
        var storeSelector = ContainerLocator.Current.Resolve<IStoreSelector>();
        return storeSelector.Select(selector);
    }

    public static bool HasZeroCredits(this IStore store)
    {
        return store.Features.Values.OfType<IFeature<BankState>>().Single().State.Credits.Equals(0.0);
    }
}
