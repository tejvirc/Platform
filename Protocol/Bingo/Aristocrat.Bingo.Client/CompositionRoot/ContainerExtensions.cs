namespace Aristocrat.Bingo.Client.CompositionRoot
{
    using System;
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
            /*
             * Set the DNS Resolution to native the default does not always resolve host names correctly.
             * Unfortunately GRPC does not offer a way to set these outside of environment variables.
             * So set them here before anything is loaded
             */
            Environment.SetEnvironmentVariable(GrpcConstants.GrpcDnsResolver, GrpcConstants.GrpcDefaultDnsResolver);

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

            Environment.SetEnvironmentVariable(GrpcConstants.GrpcTrace, GrpcConstants.GrpcTraceLevel);
            Environment.SetEnvironmentVariable(GrpcConstants.GrpcVerbosity, GrpcConstants.GrpcLogLevel);
            container.Register<GrpcLogger>();
            return container;
        }

        private static Container RegisterClient(this Container container)
        {
            var clientRegistration = Lifestyle.Singleton.CreateRegistration<BingoClient>(container);
            var progressiveClientRegistration = Lifestyle.Singleton.CreateRegistration<ProgressiveClient>(container);
            container.Collection.Register<IClient>(new Registration[] { clientRegistration, progressiveClientRegistration });

            container.AddRegistration<IClientEndpointProvider<ClientApi.ClientApiClient>>(clientRegistration);
            container.AddRegistration<IClientEndpointProvider<ProgressiveApi.ProgressiveApiClient>>(progressiveClientRegistration);

            container.RegisterSingleton<BingoClientInterceptor>();
            container.RegisterSingleton<ProgressiveClientInterceptor>();
            container.RegisterSingleton<IRegistrationService, RegistrationService>();
            container.RegisterSingleton<IProgressiveRegistrationService, ProgressiveRegistrationService>();
            container.RegisterSingleton<IAuthorizationProvider, BingoAuthorizationProvider>();
            container.RegisterSingleton<IProgressiveAuthorizationProvider, ProgressiveAuthorizationProvider>();

            var command = Lifestyle.Singleton.CreateRegistration<CommandService>(container);
            container.AddRegistration<ICommandService>(command);

            var progressiveCommand = Lifestyle.Singleton.CreateRegistration<ProgressiveCommandService>(container);
            container.AddRegistration<IProgressiveCommandService>(progressiveCommand);

            var gamePlay = Lifestyle.Singleton.CreateRegistration<GameOutcomeService>(container);
            container.AddRegistration<IGameOutcomeService>(gamePlay);

            var configuration = Lifestyle.Singleton.CreateRegistration<ConfigurationService>(container);
            container.AddRegistration<IConfigurationService>(configuration);

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
            factory.Register<PingCommandProcessor>(PingCommand.Descriptor, Lifestyle.Transient);
            container.RegisterInstance<ICommandProcessorFactory>(factory);

            var progressiveFactory = new ProgressiveCommandProcessorFactory(container);
            progressiveFactory.Register<ProgressiveUpdateCommandProcessor>(ProgressiveLevelUpdate.Descriptor, Lifestyle.Transient);
            progressiveFactory.Register<EnableByProgressiveCommandProcessor>(EnableByProgressive.Descriptor, Lifestyle.Transient);
            progressiveFactory.Register<DisableByProgressiveCommandProcessor>(DisableByProgressive.Descriptor, Lifestyle.Transient);
            container.RegisterInstance<IProgressiveCommandProcessorFactory>(progressiveFactory);

            return container;
        }
    }
}
