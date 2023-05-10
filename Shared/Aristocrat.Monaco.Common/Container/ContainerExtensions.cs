namespace Aristocrat.Monaco.Common.Container
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using log4net;
    using SimpleInjector;

    /// <summary>
    ///     Defines the ContainerExtensions class
    /// </summary>
    public static class ContainerExtensions
    {
        /// <summary>
        ///     Registers service types and implementation types with this container.
        /// </summary>
        /// <param name="this">The this.</param>
        /// <param name="namespace">The namespace.</param>
        /// <param name="assembly">The assembly.</param>
        /// <param name="lifestyle">The lifestyle.</param>
        /// <returns>the container</returns>
        /// <exception cref="ArgumentNullException">
        ///     if @this, @namespace, or assembly are null
        /// </exception>
        public static Container Register(
            this Container @this,
            string @namespace,
            Assembly assembly,
            Lifestyle lifestyle)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (@namespace == null)
            {
                throw new ArgumentNullException(nameof(@namespace));
            }

            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            // We're specifically disallowing generics here, since there are other ways to register those
            var registrations =
                from type in assembly.GetExportedTypes()
                where type.Namespace == @namespace
                where type.GetInterfaces().Any()
                where type.IsClass
                select new { Service = type.GetInterfaces().Single(i => !i.IsGenericType), Implementation = type };

            foreach (var reg in registrations)
            {
                @this.Register(reg.Service, reg.Implementation, lifestyle);
            }

            return @this;
        }

        /// <summary>
        ///     Registers containers
        /// </summary>
        /// <param name="this">This container</param>
        /// <param name="serviceType">The service to register for</param>
        /// <param name="assemblies">The assemblies to look for the service</param>
        /// <returns>A container</returns>
        public static Container RegisterManyForOpenGeneric(
            this Container @this,
            Type serviceType,
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
                @this.Register(registration.service, registration.implementation, Lifestyle.Singleton);
            }

            return @this;
        }

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
                    implemetation = type
                }).GroupBy(x => x.service, y => y.implemetation);

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

        /// <summary>
        ///     This will generate a list of open generic registered components that have a base assignable type of T
        /// </summary>
        /// <typeparam name="T">The type to obtain a component list</typeparam>
        /// <param name="this">The container</param>
        /// <param name="serviceType">The open generic type you want to get</param>
        /// <param name="assemblies">The list of assemblies to search</param>
        /// <returns>A generic list of applicable components</returns>
        /// <exception cref="ArgumentNullException">
        ///     if @this, serviceType, or assemblies is null
        /// </exception>
        /// <exception cref="ArgumentException">ServiceType is not as expected</exception>
        public static IEnumerable<T> GetOpenGenericList<T>(this Container @this, Type serviceType, params Assembly[] assemblies)
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

            var registrations = assemblies.SelectMany(assembly => assembly.GetExportedTypes())
                .Where(
                    type => type.GetInterfaces()
                                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == serviceType) &&
                            !type.IsAbstract && typeof(T).IsAssignableFrom(type))
                .Select(
                    type => new
                    {
                        service = type.GetInterfaces()
                            .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == serviceType),
                        implemetation = type
                    }).GroupBy(x => x.service, y => y.implemetation);

            return registrations.SelectMany(registration => (IEnumerable<T>)@this.GetAllInstances(registration.Key));
        }

        /// <summary>
        ///     This will generate a list of open non-generic registered components that have a base assignable type of T
        /// </summary>
        /// <typeparam name="T">The type to obtain a component list</typeparam>
        /// <param name="this">The container</param>
        /// <param name="serviceType">The open non-generic type you want to get</param>
        /// <param name="assemblies">The list of assemblies to search</param>
        /// <returns>A list of applicable components</returns>
        /// <exception cref="ArgumentNullException">
        ///     if @this, serviceType, or assemblies is null
        /// </exception>
        /// <exception cref="ArgumentException">ServiceType is not as expected</exception>
        public static IEnumerable<T> GetOpenNonGenericList<T>(this Container @this, Type serviceType, params Assembly[] assemblies)
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

            if (serviceType.ContainsGenericParameters)
            {
                throw new ArgumentException(@"ServiceType is not as expected", nameof(serviceType));
            }

            var registrations = assemblies.SelectMany(assembly => assembly.GetExportedTypes())
                .Where(
                    type => !type.IsAbstract && serviceType.IsAssignableFrom(type))
                .Select(
                    type => new
                    {
                        service = type.GetInterfaces()
                            .Single(i => i.IsAbstract && i == serviceType),
                        implemetation = type
                    }).GroupBy(x => x.service, y => y.implemetation);
            return registrations.SelectMany(registration => (IEnumerable<T>)@this.GetAllInstances(registration.Key));
        }

        /// <summary>
        ///     Registers many for use with a collection.
        ///     These items can be injected by adding <code><![CDATA[IEnumerable<Type> collection]]></code> to your constructor.
        /// </summary>
        /// <param name="this">The this.</param>
        /// <param name="serviceType">Type of the service to register.</param>
        /// <param name="assemblies">The assemblies to look at when registering.</param>
        /// <returns>The container.</returns>
        public static Container RegisterManyAsCollection(
            this Container @this,
            Type serviceType,
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

            if (serviceType.ContainsGenericParameters)
            {
                throw new ArgumentException(@"ServiceType is not as expected", nameof(serviceType));
            }

            var implementations =
                from assembly in assemblies
                where !assembly.IsDynamic
                from type in assembly.GetExportedTypes()
                where serviceType.IsAssignableFrom(type)
                where !type.IsAbstract
                select type;

            var registrations = implementations
                .Select(implementation => Lifestyle.Singleton.CreateRegistration(implementation, @this));

            @this.Collection.Register(serviceType, registrations);

            return @this;
        }

        /// <summary>
        /// extension method for resolve unregistered type
        /// </summary>
        /// <param name="container"></param>
        /// <param name="caller"></param>
        /// <param name="logger"></param>
        public static void AddResolveUnregisteredType(this Container container, string caller = null, ILog logger = null)
        {
            container.Options.ResolveUnregisteredConcreteTypes = true;
            container.ResolveUnregisteredType += (s, e) =>
            {
                if (!e.Handled && !e.UnregisteredServiceType.IsAbstract && logger != null)
                {
                    logger.Error($"UnregisteredServiceType [{caller}]: {e.UnregisteredServiceType}");
                }
            };
        }
    }
}