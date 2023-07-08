namespace Aristocrat.Extensions.Fluxor;

using System;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

public class StoreSelector : ISelector
{
    private readonly IServiceProvider _services;

    public StoreSelector(IServiceProvider services)
    {
        _services = services;
    }

    public IObservable<TResult> Select<TState, TResult>(ISelector<TState, TResult> selector) =>
        _services.GetRequiredService<IStateSelector<TState>>()
            .Select(selector);
}
