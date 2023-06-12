namespace Aristocrat.Monaco.Gaming.Lobby;

using System;
using global::Fluxor;
using global::Fluxor.Selectors;
using Microsoft.Extensions.DependencyInjection;
using Services;
using SimpleInjector;

public static class FluxorExtensions
{
    public static Container RegisterFluxor(this Container container)
    {
        var services = new ServiceCollection();

        services.AddFluxor(options => options.ScanAssemblies(typeof(FluxorExtensions).Assembly));

        foreach (var d in services)
        {
            var lifetime = d.Lifetime switch
            {
                ServiceLifetime.Singleton => Lifestyle.Singleton,
                ServiceLifetime.Scoped => Lifestyle.Singleton,
                _ => Lifestyle.Transient
            };

            if (d.ImplementationType != null)
            {
                container.Register(d.ServiceType, d.ImplementationType, lifetime);
            }
            else if (d.ImplementationInstance != null)
            {
                container.Register(d.ServiceType, () => d.ImplementationInstance, lifetime);
            }
            else if (d.ImplementationFactory != null)
            {
                container.Register(d.ServiceType, () => d.ImplementationFactory.Invoke(container), lifetime);
            }
            else
            {
                throw new InvalidOperationException($"Unable to register {d.ServiceType}");
            }

            // TODO Handle IEnumerable registration conversion between MS DI and SimpleInjector
        }

        // container.RegisterDecorator<IDispatcher, UiThreadDispatcher>(Lifestyle.Singleton);

        return container;
    }

    public static ISelectorSubscription<TResult> SubscribeSelector<TFeatureState, TResult>(
        this IStore store,
        Func<TFeatureState, TResult> projector)
    {
        var state = SelectorFactory.CreateFeatureSelector<TFeatureState>();
        var selectItems = SelectorFactory.CreateSelector(state, projector);
        return store.SubscribeSelector(selectItems);
    }
}
