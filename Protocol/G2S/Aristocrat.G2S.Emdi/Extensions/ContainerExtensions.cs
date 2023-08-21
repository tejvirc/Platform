namespace Aristocrat.G2S.Emdi.Extensions
{
    using System;
    using System.Linq;
    using System.Reflection;
    using SimpleInjector;

    /// <summary>
    /// Extension methods for configuring container
    /// </summary>
    public static class ContainerExtensions
    {
        /// <summary>
        ///     Registers many for open generic.
        /// </summary>
        /// <param name="this">The this.</param>
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
        public static Container RegisterManyForOpenGeneric(
            this Container @this,
            Type serviceType,
            bool multipleImplementations,
            params Assembly[] assemblies)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
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
                    implementation = type
                }).GroupBy(x => x.service, y => y.implementation);

            if (multipleImplementations)
            {
                foreach (var implementations in implementationsGroups)
                {
                    var registrationsOfService =
                        implementations.Select(x => Lifestyle.Singleton.CreateRegistration(x, @this));
                    @this.Collection.Register(implementations.Key, registrationsOfService);
                }
            }
            else
            {
                foreach (var implementations in implementationsGroups)
                {
                    @this.Register(implementations.Key, implementations.Single(), Lifestyle.Singleton);
                }
            }

            return @this;
        }
    }
}
