namespace Aristocrat.G2S.Emdi
{
    using Consumers;
    using Events;
    using Extensions;
    using Handlers;
    using Host;
    using Meters;
    using Serialization;
    using SimpleInjector;

    /// <summary>
    /// Configures media display host
    /// </summary>
    public static class EmdiBuilder
    {
        /// <summary>
        /// Extension method to configure media display host
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static Container ConfigureEmdi(this Container container)
        {
            container.Register<IMessageSerializer, MessageSerializer>();
            container.RegisterSingleton<IEmdi, EmdiService>();
            container.RegisterSingleton<IMediaAdapter, MediaAdapter>();
            container.RegisterSingleton<IEventSubscriptions, EventSubscriptions>();
            container.RegisterSingleton<IMeterSubscriptions, MeterSubscriptions>();
            container.RegisterSingleton<IReporter, ReporterService>();
            container.RegisterInstance<IHostQueue>(new HostQueue(container));

            container.ConfigureConsumers();
            container.ConfigureHandlers();

            return container;
        }

        private static void ConfigureHandlers(this Container container)
        {
            container.Register(typeof(ICommandHandler<>), typeof(ICommandHandler<>).Assembly);
            container.RegisterInstance<ICommandHandlerFactory>(new CommandHandlerFactory(container));
        }

        private static void ConfigureConsumers(this Container container)
        {
            container.RegisterManyForOpenGeneric(
                typeof(IEmdiConsumer<>),
                true,
                typeof(IEmdiConsumer<>).Assembly);
        }
    }
}
