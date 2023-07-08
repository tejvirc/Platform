namespace Aristocrat.Monaco.Gaming.Lobby;

using System;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

public static class FluxorExtensions
{
    public static Container RegisterFluxor(this Container container)
    {
        var services = new ServiceCollection();

        services.AddFluxor(
            options =>
            {
                options
                    .ScanAssemblies(typeof(FluxorExtensions).Assembly);
                //.UseRemoteReduxDevTools(
                //    devToolsOptions =>
                //    {
                //        devToolsOptions.RemoteReduxDevToolsUri = new Uri("https://localhost:7232/clientapphub");
                //        devToolsOptions.RemoteReduxDevToolsSessionId = "71637a4c-43b7-4ab0-a658-15b85e3c037f";
                //        devToolsOptions.Name = "Monaco Lobby";
                //        //devToolsOptions.EnableStackTrace();
                //    });
            }
        );

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

    public static Task DispatchAsync(this IDispatcher dispatcher, object action)
    {
        dispatcher.Dispatch(action);

        return Task.CompletedTask;
    }

    public static IState<TState> State<TState>(this IStore store)
    {
        return new State<TState>(
            (IFeature<TState>)store.Features.Values.First(x => x.GetStateType() == typeof(TState)));
    }
}
