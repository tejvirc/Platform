namespace Aristocrat.Monaco.Gaming.Lobby;

using System;
using Aristocrat.Monaco.Kernel;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Common;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddView<TView>(this IServiceCollection services)
        where TView : class
    {
        var viewModelType = GetViewModelType(typeof(TView));

        if (viewModelType == null)
        {
            services.AddTransient<TView>();
        }
        else
        {
            services.AddTransient(viewModelType);

            var objectFactory = ActivatorUtilities.CreateFactory(
                typeof(TView),
                new[] { typeof(object) });

            services.Add(
                ServiceDescriptor.Describe(
                    typeof(TView),
                    sp => (TView)objectFactory(sp, new object[] { sp.GetRequiredService(viewModelType) }),
                    ServiceLifetime.Transient));
        }

        return services;
    }

    /// <summary>
    ///     Registers many for open generic.
    /// </summary>
    /// <param name="services">The this.</param>
    /// <param name="serviceType">Type of the service.</param>
    /// <param name="multipleImplementations">
    ///     Set <c>true</c> if there are multiple implementations of one type of service else
    ///     <c>false</c>
    /// </param>
    /// <param name="assemblies">The assemblies.</param>
    /// <returns>the container</returns>
    /// <exception cref="ArgumentNullException">
    ///     if @this, serviceType, or assemblies is null
    /// </exception>
    /// <exception cref="ArgumentException">ServiceType is not as expected</exception>
    public static IServiceCollection AddManyForOpenGeneric(
        this IServiceCollection services,
        Type serviceType,
        bool multipleImplementations,
        params Assembly[] assemblies)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (serviceType == null)
        {
            throw new ArgumentNullException(nameof(serviceType));
        }

        if (assemblies == null || assemblies.Length == 0)
        {
            throw new ArgumentNullException(nameof(assemblies));
        }

        if (!serviceType.ContainsGenericParameters)
        {
            throw new ArgumentException(@"ServiceType is not as expected", nameof(serviceType));
        }

        var implementationsGroups =
            (from assembly in assemblies
             where !assembly.IsDynamic
             from type in assembly.GetExportedTypes()
             where type.IsImplementationOf(serviceType)
             where !type.IsAbstract
             select new
             {
                 service = type.GetInterfaces()
                     .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == serviceType),
                 implemetation = type
             }).GroupBy(x => x.service, y => y.implemetation);
        if (multipleImplementations)
        {
            foreach (var implementations in implementationsGroups)
            {
                foreach (var implementation in implementations)
                {
                    services.AddSingleton(implementations.Key, implementation);
                }
            }
        }
        else
        {
            foreach (var implementations in implementationsGroups)
            {
                services.AddSingleton(implementations.Key, implementations.Single());
            }
        }

        return services;
    }

    public static void AddConsumers(this IServiceCollection services)
    {
        services.AddManyForOpenGeneric(
            typeof(IConsumer<>),
            true,
            Assembly.GetExecutingAssembly());
    }

    private static Type GetViewModelType(Type viewType)
    {
        var viewModelName = viewType.Name switch
        {
            string name when name.EndsWith("View") => $"{name}Model",
            string name => $"{name}ViewModel"
        };

        var viewNamespace = viewType.Namespace;

        var viewModelType = Type.GetType($"{viewNamespace}.{viewModelName}");

        if (viewModelType != null)
        {
            return viewModelType;
        }

        var viewModelNamespace = viewType.Namespace.Replace(".Views", ".ViewModels", StringComparison.CurrentCulture);

        return Type.GetType($"{viewModelNamespace}.{viewModelName}");
    }
}
