namespace Aristocrat.Monaco.Common.Container;

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     ServiceCollection extension methods
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers containers
    /// </summary>
    /// <param name="services">This container</param>
    /// <param name="serviceType">The service to register for</param>
    /// <param name="assemblies">The assemblies to look for the service</param>
    /// <returns>A container</returns>
    public static IServiceCollection AddManyForOpenGeneric(
        this IServiceCollection services,
        Type serviceType,
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

        var registrations =
            from assembly in assemblies
            where !assembly.IsDynamic
            from type in assembly.GetExportedTypes()
            where type.IsImplementationOf(serviceType)
            where !type.IsAbstract
            select new
            {
                service = type.GetInterfaces()
                    .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == serviceType),
                implementation = type
            };

        foreach (var registration in registrations)
        {
            services.AddSingleton(registration.service, registration.implementation);
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

    /// <summary>
    ///     Registers many for use with a collection.
    ///     These items can be injected by adding <code><![CDATA[IEnumerable<Type> collection]]></code> to your constructor.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/>.</param>
    /// <param name="serviceType">Type of the service to register.</param>
    /// <param name="assemblies">The assemblies to look at when registering.</param>
    /// <returns>The container.</returns>
    public static IServiceCollection AddManyAsCollection(
        this IServiceCollection services,
        Type serviceType,
        params Assembly[] assemblies)
    {
        if (serviceType == null)
        {
            throw new ArgumentNullException(nameof(serviceType));
        }

        if (assemblies == null || assemblies.Length == 0)
        {
            throw new ArgumentNullException(nameof(assemblies));
        }

        if (serviceType.ContainsGenericParameters)
        {
            throw new ArgumentException(@"ServiceType is not as expected", nameof(serviceType));
        }

        services.Scan(
            scan =>
                scan.FromAssemblies(assemblies)
                    .AddClasses(configClass => configClass.AssignableTo(serviceType))
                    .AsImplementedInterfaces()
                    .WithSingletonLifetime()
        );

        return services;
    }
}
