namespace Aristocrat.Fluxor.Extensions;

using System;
using SimpleInjector;

public class StoreSelector : ISelector
{
    private readonly Container _container;

    public StoreSelector(Container container)
    {
        _container = container;
    }

    public IObservable<TResult> Select<TState, TResult>(ISelector<TState, TResult> selector) =>
        _container.GetInstance<IStateSelectors<TState>>()
            .Select(selector);
}
