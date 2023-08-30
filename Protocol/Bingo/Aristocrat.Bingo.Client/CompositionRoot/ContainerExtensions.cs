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
        public static Container RegisterClient(this Container container, bool isBingoProgressiveEnabled, params Assembly[] assemblies)
        {
            return container.RegisterClient(isBingoProgressiveEnabled)
                .AddCommandProcessors(isBingoProgressiveEnabled)
                .RegisterMessageHandlers(isBingoProgressiveEnabled, assemblies.Append(Assembly.GetExecutingAssembly()).ToArray());
        }

        public static Container WithGrpcLogging(this Container container, bool enabled)
        {
            if (!enabled)
            {
                return container;
            }

            return container;
        }

        private static Container RegisterClient(this Container container, bool isBingoProgressiveEnabled)
        {
            var clientRegistration = Lifestyle.Singleton.CreateRegistration<BingoClient>(container);

            if (isBingoProgressiveEnabled)
            {
                var progressiveClientRegistration = Lifestyle.Singleton.CreateRegistration<ProgressiveClient>(container);
                container.Collection.Register<IClient>(
                    new Registration[] { clientRegistration, progressiveClientRegistration });

                container.AddRegistration<IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient>>(progressiveClientRegistration);
                container.RegisterSingleton<IProgressiveAuthorizationProvider, ProgressiveAuthorizationProvider>();
                container.RegisterSingleton<ProgressiveClientAuthorizationInterceptor>();
                container.RegisterSingleton<IProgressiveRegistrationService, ProgressiveRegistrationService>();
                container.RegisterSingleton<IProgressiveContributionService, ProgressiveContributionService>();
                container.RegisterSingleton<IProgressiveClaimService, ProgressiveClaimService>();
                container.RegisterSingleton<IProgressiveAwardService, ProgressiveAwardService>();
                var progressiveCommand = Lifestyle.Singleton.CreateRegistration<ProgressiveCommandService>(container);
                container.AddRegistration<IProgressiveCommandService>(progressiveCommand);
            }
            else
            {
                container.Collection.Register<IClient>(
                    new Registration[] { clientRegistration });
            }

            container.AddRegistration<IClientEndpointProvider<ClientApi.ClientApiClient>>(clientRegistration);

            container.RegisterSingleton<IBingoAuthorizationProvider, BingoAuthorizationProvider>();
            container.RegisterSingleton<BingoClientAuthorizationInterceptor>();
            container.RegisterSingleton<LoggingInterceptor>();
            container.RegisterSingleton<IRegistrationService, RegistrationService>();
            container.RegisterSingleton<IReportTransactionService, ReportTransactionService>();
            container.RegisterSingleton<IReportEventService, ReportEventService>();
            container.RegisterSingleton<IActivityReportService, ActivityReportService>();
            container.RegisterSingleton<IStatusReportingService, StatusReportingService>();

            var command = Lifestyle.Singleton.CreateRegistration<CommandService>(container);
            container.AddRegistration<ICommandService>(command);

            var gamePlay = Lifestyle.Singleton.CreateRegistration<GameOutcomeService>(container);
            container.AddRegistration<IGameOutcomeService>(gamePlay);

            var configuration = Lifestyle.Singleton.CreateRegistration<ConfigurationService>(container);
            container.AddRegistration<IConfigurationService>(configuration);

            return container;
        }

        private static Container RegisterCommunicationServices(this Container container)
        {
            var communicationServices = Assembly.GetExecutingAssembly().GetExportedTypes()
                .Where(x => x.IsSubclassOf(typeof(BaseClientCommunicationService<ClientApi.ClientApiClient>)) ||
                                 x.IsSubclassOf(typeof(BaseClientCommunicationService<ProgressiveApi.ProgressiveApiClient>)) && !x.IsAbstract);
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

        private static Container RegisterMessageHandlers(this Container container, bool isBingoProgressiveEnabled, params Assembly[] assemblies)
        {
            container.RegisterSingleton<IMessageHandlerFactory, MessageHandlerFactory>();
            container.Register(typeof(IMessageHandler<,>), assemblies, Lifestyle.Transient);

            if (isBingoProgressiveEnabled)
            {
                container.RegisterSingleton<IProgressiveMessageHandlerFactory, ProgressiveMessageHandlerFactory>();
                container.Register(typeof(IProgressiveMessageHandler<,>), assemblies, Lifestyle.Transient);
            }

            return container;
        }

        private static Container AddCommandProcessors(this Container container, bool isBingoProgressiveEnabled)
        {
            var factory = new CommandProcessorFactory(container);
            factory.Register<EnableCommandProcessor>(EnableCommand.Descriptor, Lifestyle.Transient);
            factory.Register<DisableCommandProcessor>(DisableCommand.Descriptor, Lifestyle.Transient);
            container.RegisterInstance<ICommandProcessorFactory>(factory);

            if (isBingoProgressiveEnabled)
            {
                var progressiveFactory = new ProgressiveCommandProcessorFactory(container);
                progressiveFactory.Register<ProgressiveUpdateCommandProcessor>(
                    ProgressiveLevelUpdate.Descriptor,
                    Lifestyle.Transient);
                progressiveFactory.Register<EnableByProgressiveCommandProcessor>(
                    EnableByProgressive.Descriptor,
                    Lifestyle.Transient);
                progressiveFactory.Register<DisableByProgressiveCommandProcessor>(
                    DisableByProgressive.Descriptor,
                    Lifestyle.Transient);
                container.RegisterInstance<IProgressiveCommandProcessorFactory>(progressiveFactory);
            }

            return container;
        }
    }
}
