namespace Aristocrat.Monaco.Asp.Hosts.CompositionRoot
{
    using System.Linq;
    using System.Reflection;
    using Accounting.Contracts.Handpay;
    using Client.Consumers;
    using Client.Comms;
    using Client.Contracts;
    using Client.DataSources;
    using Client.Devices;
    using Common.Container;
    using Gaming.Contracts.Progressives;
    using Kernel;
    using Progressive;
    using SimpleInjector;

    /// <summary>
    ///     This class is mainly used to compose the object graph for this layer
    /// </summary>
    internal static class Bootstrapper
    {
        internal static Container ConfigureContainer(ProtocolSettings protocolSettings)
        {
            var container = new Container();

            container.RegisterClasses(protocolSettings);
            container.ConfigureConsumers();

            var serviceWaiter = new ServiceWaiter(ServiceManager.GetInstance().GetService<IEventBus>());
            serviceWaiter.AddServiceToWaitFor<IProtocolLinkedProgressiveAdapter>();

            if (serviceWaiter.WaitForServices())
            {
                container.RegisterExternalServices();

                // Register the handlers services
                container.ConfigureServices();
            }

            return container;
        }

        private static void RegisterClasses(this Container @this, ProtocolSettings protocolSettings)
        {
            @this.Register<ICommPort, CommPort>(Lifestyle.Singleton);
            @this.RegisterInstance(protocolSettings);

            var dataSourceTypes = @this.GetTypesToRegister<IDataSource>(typeof(IDataSource).Assembly).Select(
                type => Lifestyle.Singleton.CreateRegistration(type, @this));

            @this.Collection.Register<IDataSource>(dataSourceTypes);

            @this.Register<ILogicSealDataSource, LogicSealDataSource>(Lifestyle.Singleton);
            @this.Register<IDoorsDataSource, DoorsDataSource>(Lifestyle.Singleton);

            @this.Register<IProgressiveManager, ProgressiveManager>(Lifestyle.Singleton);
            @this.Register<IPerLevelMeterProvider, PerLevelMeterProvider>(Lifestyle.Singleton);
            @this.Register<IDataSourceRegistry, DataSourceRegistry>(Lifestyle.Singleton);
            @this.Register<IParameterFactory, ParameterFactory>(Lifestyle.Singleton);
            @this.Register<IParameterProcessor, ParameterProcessor>(Lifestyle.Singleton);
            @this.Register<IAspClient, ApplicationLayer>(Lifestyle.Singleton);
            @this.Register<IGameStatusProvider, GameStatusProvider>(Lifestyle.Singleton);
            @this.Register<IHandpayValidator, HandpayValidator>(Lifestyle.Singleton);
            @this.Register<IMeterSnapshotProvider, MeterSnapshotProvider>(Lifestyle.Singleton);
            @this.Register<IAspGameProvider, AspGameProvider>(Lifestyle.Singleton);
            @this.Register<LinkProgressiveLevelConfigurationService, LinkProgressiveLevelConfigurationService>(Lifestyle.Singleton);
            @this.Register<IReportableEventsManager, ReportableEventsManager>(Lifestyle.Singleton);
            @this.Register<ICurrentMachineModeStateManager, CurrentMachineModeStateManager>(Lifestyle.Singleton);
        }

        private static void ConfigureServices(this Container @this)
        {
            var serviceManager = ServiceManager.GetInstance();
            serviceManager.AddService(@this.GetInstance<IHandpayValidator>());
        }
        private static void ConfigureConsumers(this Container @this)
        {
            @this.RegisterSingleton<ISharedConsumer, SharedConsumerContext>();

            @this.RegisterManyForOpenGeneric(
                typeof(IConsumer<>),
                true,
                Assembly.GetExecutingAssembly());
        }
    }
}