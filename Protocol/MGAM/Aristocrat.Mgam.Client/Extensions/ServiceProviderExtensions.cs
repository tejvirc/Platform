// ReSharper disable once CheckNamespace
namespace Aristocrat.Mgam.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SimpleInjector;

    /// <summary>
    ///     Extension method for <see cref="IServiceProvider"/>.
    /// </summary>
    internal static class ServiceProviderExtensions
    {
        /// <summary>
        ///     Gets the service object of the specified type.
        /// </summary>
        /// <typeparam name="TService">The type of service object to get.</typeparam>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/>.</param>
        /// <returns>A service object of type <typeparamref name="TService" />.</returns>
        public static TService GetService<TService>(this IServiceProvider serviceProvider)
        {
            return (TService)serviceProvider.GetService(typeof(TService));
        }

        /// <summary>
        ///     Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/>.</param>
        /// <param name="serviceType">An object that specifies the type of service object to get. </param>
        /// <exception cref="ActivationException">Thrown when a container has an error in resolving an object.</exception>
        public static object GetRequiredService(this IServiceProvider serviceProvider, Type serviceType)
        {
            var service = serviceProvider.GetService(serviceType);

            if (service == null)
            {
                throw new ActivationException($"Unable to resolve service for {serviceType.FullName}");
            }

            return service;
        }

        /// <summary>
        ///     Gets the service object of the specified type.
        /// </summary>
        /// <typeparam name="TService">The type of service object to get.</typeparam>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/>.</param>
        /// <returns>A service object of type <typeparamref name="TService" />.</returns>
        /// <exception cref="ActivationException">Thrown when a container has an error in resolving an object.</exception>
        public static TService GetRequiredService<TService>(this IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetRequiredService(typeof(TService));

            if (service == null)
            {
                throw new ActivationException($"Unable to resolve service for {typeof(TService).FullName}");
            }

            return (TService)service;
        }

        /// <summary>
        ///     Gets the service object of the specified type.
        /// </summary>
        /// <typeparam name="TService">The type of service object to get.</typeparam>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/>.</param>
        /// <returns></returns>
        public static IEnumerable<TService> GetServices<TService>(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<IEnumerable<TService>>() ?? Enumerable.Empty<TService>();
        }
    }
}
