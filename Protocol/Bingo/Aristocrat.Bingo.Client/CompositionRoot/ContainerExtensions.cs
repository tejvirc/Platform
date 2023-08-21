namespace Aristocrat.Bingo.Client.CompositionRoot
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Messages;
    using Messages.Commands;
    using Messages.Interceptor;
    using ServerApiGateway;
    using SimpleInjector;

    public static class ContainerExtensions
    {
        public static Container RegisterClient(this Container container, params Assembly[] assemblies)
        {
            return container.RegisterClient()
                .AddCommandProcessors()
                .RegisterMessageHandlers(assemblies.Append(Assembly.GetExecutingAssembly()).ToArray());
        }

        public static Container WithGrpcLogging(this Container container, bool enabled)
        {
            if (!enabled)
            {
                return container;
            }

            return container;
        }

        private static Container RegisterClient(this Container container)
        {
            container.RegisterSingleton<BingoClientInterceptor>();
            container.RegisterSingleton<IAuthorizationProvider, BingoAuthorizationProvider>();
            return container.RegisterAllInterfaces<BingoClient>()
                .RegisterCommunicationServices();
        }

        private static Container RegisterCommunicationServices(this Container container)
        {
            var communicationServices = Assembly.GetExecutingAssembly().GetExportedTypes()
                .Where(x => x.IsSubclassOf(typeof(BaseClientCommunicationService)) && !x.IsAbstract);
            foreach (var service in communicationServices)
            {
                container.RegisterAllInterfaces(service);
            }

            return container;
        }

        private static Container RegisterAllInterfaces<TType>(this Container container, Func<Type, bool> filter = null)
        {
            return container.RegisterAllInterfaces(typeof(TType), filter);
        }

        private static Container RegisterAllInterfaces(
            this Container container,
            Type type,
            Func<Type, bool> filter = null)
        {
            filter ??= t => t != typeof(IDisposable) && t != typeof(IEnumerable<>) && t != typeof(IEnumerable);
            var registration = Lifestyle.Singleton.CreateRegistration(type, container);
            foreach (var @interface in type.GetInterfaces())
            {
                if (!filter(@interface))
                {
                    continue;
                }

                container.AddRegistration(@interface, registration);
            }

            return container;
        }

        private static Container RegisterMessageHandlers(this Container container, params Assembly[] assemblies)
        {
            container.RegisterSingleton<IMessageHandlerFactory, MessageHandlerFactory>();
            container.Register(typeof(IMessageHandler<,>), assemblies, Lifestyle.Transient);
            return container;
        }

        private static Container AddCommandProcessors(this Container container)
        {
            var factory = new CommandProcessorFactory(container);
            factory.Register<EnableCommandProcessor>(EnableCommand.Descriptor, Lifestyle.Transient);
            factory.Register<DisableCommandProcessor>(DisableCommand.Descriptor, Lifestyle.Transient);
            container.RegisterInstance<ICommandProcessorFactory>(factory);
            return container;
        }
    }
}
