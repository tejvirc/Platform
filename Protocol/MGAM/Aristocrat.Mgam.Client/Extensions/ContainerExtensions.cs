// ReSharper disable once CheckNamespace
namespace Aristocrat.Mgam.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Attribute;
    using Logging;
    using Messaging.Translators;
    using Middleware;
    using Options;
    using Protocol;
    using Routing;
    using Services;
    using Services.CreditServices;
    using Services.Directory;
    using Services.DropMode;
    using Services.Identification;
    using Services.Notification;
    using Services.Registration;
    using Services.Session;
    using SimpleInjector;

    /// <summary>
    ///     Container method extensions.
    /// </summary>
    public static class ContainerExtensions
    {
        /// <summary>
        ///     Configures and builds <see cref="IEgm"/>.
        /// </summary>
        /// <param name="container">Dependency injection container.</param>
        /// <param name="optionsConfig">Configure protocol options.</param>
        /// <param name="loggerConfig">Configure logging.</param>
        /// <param name="assemblies">List of assemblies to be searched.</param>
        /// <returns><see cref="Container"/>.</returns>
        public static Container RegisterClient(this Container container, Action<ProtocolOptionsBuilder> optionsConfig, Action<LoggerConfiguration> loggerConfig, params Assembly[] assemblies)
        {
            var searchAssemblies = new List<Assembly>(assemblies) { Assembly.GetExecutingAssembly() }.ToArray();

            container.RegisterSingleton<IEgm, MgamEgm>();

            return container
                .AddOptions(optionsConfig)
                .AddRouting()
                .AddTranslators(searchAssemblies)
                .AddHandlers(searchAssemblies)
                .AddStartables(searchAssemblies)
                .AddLogging(loggerConfig)
                .ConfigureServices();
        }

        /// <summary>
        ///     Adds a request message router to the request pipeline.
        /// </summary>
        /// <typeparam name="TMiddleware">The type of <see cref="Messaging.IRequest"/>.</typeparam>
        /// <param name="container">Dependency injection container.</param>
        /// <param name="lifestyle">The decorator lifestyle.</param>
        /// <returns><see cref="Container"/>.</returns>
        public static Container UseMiddleware<TMiddleware>(this Container container, Lifestyle lifestyle = null)
            where TMiddleware : class, IRequestRouter
        {
            container.RegisterDecorator<IRequestRouter, TMiddleware>(lifestyle ?? Lifestyle.Singleton);

            return container;
        }

        private static Container ConfigureServices(this Container container)
        {
            container.RegisterSingleton<IDirectory, DirectoryService>();

            container.RegisterSingleton<IRegistration, RegistrationService>();

            container.RegisterSingleton<IVoucher, VoucherService>();

            container.RegisterSingleton<ICurrency, CurrencyService>();

            container.RegisterSingleton<IIdentification, IdentificationService>();
            container.RegisterSingleton<ISession, SessionService>();

            container.RegisterSingleton<IHostServiceCollection, HostServiceCollection>();

            container.RegisterSingleton<IBillAcceptorMeter, BillAcceptorMeterService>();

            container.RegisterSingleton<IAttributeCache, AttributeCache>();

            container.RegisterSingleton<INotification, NotificationService>();

            return container;
        }

        private static Container AddStartables(this Container container, params Assembly[] assemblies)
        {
            var types = container.GetTypesToRegister(
                serviceType: typeof(IStartable),
                assemblies: assemblies,
                options: new TypesToRegisterOptions
                {
                    IncludeGenericTypeDefinitions = true,
                    IncludeComposites = false,
                });

            var registrations = types.Select(t => Lifestyle.Singleton.CreateRegistration(t, container));

            container.Collection.Register<IStartable>(registrations);

            return container;
        }

        private static Container AddLogging(this Container container, Action<LoggerConfiguration> configure)
        {
            var config = new LoggerConfiguration();
            configure?.Invoke(config);

            container.RegisterInstance(config);
            container.RegisterSingleton<ILoggerFactory, Log4NetLoggerFactory>();
            container.RegisterConditional(typeof(ILogger<>), typeof(Logger<>), Lifestyle.Singleton, _ => true);

            return container;
        }

        private static Container AddRouting(this Container container)
        {
            container.RegisterSingleton<IHostQueue, HostQueue>();
            container.RegisterSingleton<IRequestRouter, HostQueue>();
            container.RegisterSingleton<IBroadcastRouter, HostQueue>();

            container.UseMiddleware<InstanceMiddleware>();

            container.RegisterSingleton<IRequestHandler, RequestHandler>();

            container.RegisterSingleton<IBroadcastTransporter, BroadcastTransporter>();
            container.RegisterSingleton<ISecureTransporter, SecureTransporter>();

            container.RegisterSingleton<ITransportPublisher, TransportPublisher>();
            container.RegisterSingleton<ITransportStatusSubscription, TransportPublisher>();
            container.RegisterSingleton<IRoutedMessagesSubscription, TransportPublisher>();

            container.RegisterSingleton<IXmlMessageSerializer, XmlMessageSerializer>();

            container.RegisterSingleton<ICompressor, BZip2Compressor>();

            container.RegisterSingleton<IClientFactory, ClientFactory>();

            return container;
        }

        private static Container AddTranslators(this Container container, params Assembly[] assemblies)
        {
            container.Register(typeof(IMessageTranslator<>), assemblies, Lifestyle.Transient);
            container.RegisterSingleton<IMessageTranslatorFactory>(() => new MessageTranslatorFactory(container));

            return container;
        }

        private static Container AddHandlers(this Container container, params Assembly[] assemblies)
        {
            container.Register(typeof(Messaging.IMessageHandler<>), assemblies, Lifestyle.Transient);

            return container;
        }
    }
}
