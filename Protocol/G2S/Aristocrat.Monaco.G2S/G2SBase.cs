namespace Aristocrat.Monaco.G2S
{
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Emdi;
    using Common.Events;
    using Common.PackageManager;
    using CompositionRoot;
    using Consumers;
    using Data.Profile;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Session;
    using Hardware.Contracts.Communicator;
    using Hardware.Contracts.IdReader;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using log4net;
    using Localization.Properties;
    using Logging;
    using Meters;
    using Monaco.Common;
    using Services;
    using SimpleInjector;
    using Aristocrat.Monaco.Application.Contracts.Protocol;
    using System.Collections.Generic;
    using Aristocrat.Monaco.G2S.Services.Progressive;

    /// <summary>
    ///     Handle the base level G2S communications including meter managements and system events.
    /// </summary>
    [ProtocolCapability(
        protocol:CommsProtocol.G2S,
        isValidationSupported:true,
        isFundTransferSupported:false,
        isProgressivesSupported:true,
        isCentralDeterminationSystemSupported:true)]
    public class G2SBase : BaseRunnable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static bool _isFirstLoad = true;

        private Container _container;
        private ContainerService _containerService;
        private ServiceWaiter _serviceWaiter = new ServiceWaiter(ServiceManager.GetInstance().GetService<IEventBus>());
        private SharedConsumerContext _sharedConsumerContext;
        private ManualResetEvent _shutdownEvent = new ManualResetEvent(false);

        private bool _g2sProgressivesEnabled = false;

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            Logger.Debug("G2SBase initializing");

            if (_isFirstLoad)
            {
                // This only happens during first load.  We will reload the protocol for a number of reasons, but there's no reason to display this again
                ServiceManager.GetInstance().GetService<ISystemDisableManager>()
                    .Disable(
                        Constants.ProtocolDisabledKey,
                        SystemDisablePriority.Immediate,
                        () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DisabledDuringInitialization),
                        false);

                CreateVirtualIdReaders();
            }

            _g2sProgressivesEnabled = (bool)ServiceManager.GetInstance().
                TryGetService<IMultiProtocolConfigurationProvider>().MultiProtocolConfiguration.FirstOrDefault(c => c.Protocol == CommsProtocol.G2S)?.IsProgressiveHandled;
            var propertyProvider = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            propertyProvider.SetProperty(Constants.G2SProgressivesEnabled, _g2sProgressivesEnabled);

            // The G2S Lib uses TraceSource...
            Logger.AddAsTraceSource();

            _serviceWaiter.AddServiceToWaitFor<ITransactionCoordinator>();
            _serviceWaiter.AddServiceToWaitFor<ICabinetService>();
            _serviceWaiter.AddServiceToWaitFor<IGameHistory>();
            _serviceWaiter.AddServiceToWaitFor<IPlayerBank>();
            _serviceWaiter.AddServiceToWaitFor<IPlayerSessionHistory>();
            _serviceWaiter.AddServiceToWaitFor<IAttendantService>();
            _serviceWaiter.AddServiceToWaitFor<IProtocolLinkedProgressiveAdapter>();

            if (_serviceWaiter.WaitForServices())
            {
                _sharedConsumerContext = new SharedConsumerContext();
                ServiceManager.GetInstance().AddService(_sharedConsumerContext);
                _container = Bootstrapper.InitializeContainer();
                _container.Verify();

                ServiceManager.GetInstance().GetService<IValidationProvider>().Register(
                    ProtocolNames.G2S,
                    _container.GetInstance<IVoucherValidator>());

                ServiceManager.GetInstance().AddService(_container.GetInstance<IVoucherDataService>() as IService);
                if (_g2sProgressivesEnabled)
                {
                    ServiceManager.GetInstance().AddService(_container.GetInstance<IProgressiveService>() as IService);
                }
                ServiceManager.GetInstance().AddService(_container.GetInstance<IMasterResetService>() as IService);
                ServiceManager.GetInstance().AddServiceAndInitialize(_container.GetInstance<IInformedPlayerService>() as IService);
                ServiceManager.GetInstance().AddServiceAndInitialize(_container.GetInstance<IIdReaderValidator>());
                var handPayValidator = _container.GetInstance<IHandpayService>();
                ServiceManager.GetInstance().GetService<IValidationProvider>().Register(
                    ProtocolNames.G2S,
                    _container.GetInstance<IHandpayService>() as IHandpayValidator);
                ((IService)_container.GetInstance<IHandpayService>()).Initialize();

                ServiceManager.GetInstance().AddServiceAndInitialize(_container.GetInstance<IEmdi>());
            }

            _container.GetInstance<IEventBus>().Subscribe<RestartProtocolEvent>(this, _ => OnStop());

            _containerService = new ContainerService(_container);
            ServiceManager.GetInstance().AddService(_containerService);

            ModelMappingRules.Initialize();

            _container.GetInstance<IPackageManager>().VerifyPackages();

            Logger.Debug("G2S Runnable initialized!");
        }

        /// <inheritdoc />
        protected override void OnRun()
        {
            Logger.Debug("Start of OnRun() for G2SBase");

            LogToMessageDisplay(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LoadingProtocol));

            var engine = _container.GetInstance<IEngine>();
            ResolveHandlers(_container); // We'll want to move this when the registration is cleaned-up

            var startUpContext = new StartupContext();

            var eventListener = ServiceManager.GetInstance().GetService<StartupEventListener>();

            var profiles = _container.GetInstance<IProfileService>().GetAll();
            if (!profiles.Any() || eventListener.HasEvent<PlatformBootedEvent>(e => e.CriticalMemoryCleared))
            {
                // If this is the first time we're adding devices or the persistent storage has been reset we're going to set forcibly set all of these values
                startUpContext.DeviceReset = true;
                startUpContext.DeviceChanged = true;
                startUpContext.SubscriptionLost = true;
                startUpContext.MetersReset = true;
                startUpContext.DeviceStateChanged = true;
                startUpContext.DeviceAccessChanged = true;
            }
            else
            {
                var context = (StartupContext)ServiceManager.GetInstance().GetService<IPropertiesManager>().GetProperty(Constants.StartupContext, null);
                if (context != null)
                {
                    startUpContext = context;
                }
            }

            // don't start the engine if we've already signaled a shutdown or Disposed
            var start = false;

            if (_shutdownEvent != null)
            {
                start = !_shutdownEvent.WaitOne(0);
            }

            LogToMessageDisplay(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.StartingProtocol));

            if (start)
            {
                engine.Start(
                    startUpContext,
                    () =>
                    {
                        if (_isFirstLoad)
                        {
                            ServiceManager.GetInstance().GetService<ISystemDisableManager>().Enable(Constants.ProtocolDisabledKey);
                            _isFirstLoad = false;
                        }
                    });

                var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
                propertiesManager.SetProperty(Constants.StartupContext, null);
                propertiesManager.SetProperty(AccountingConstants.TicketBarcodeLength, AccountingConstants.DefaultTicketBarcodeLength);
                propertiesManager.SetProperty(GamingConstants.ProgressiveConfigurableId, ServiceManager.GetInstance().
                    TryGetService<IMultiProtocolConfigurationProvider>().MultiProtocolConfiguration.FirstOrDefault(c => c.Protocol == CommsProtocol.G2S)?.IsProgressiveHandled);

                propertiesManager.SetProperty(GamingConstants.ProgressiveConfigurableId, _g2sProgressivesEnabled);
                if (_g2sProgressivesEnabled)
                {
                    //Populate the levelID fields in the ProgressiveService
                    var progService = ServiceManager.GetInstance().TryGetService<IProgressiveService>();

                    if (progService != null)
                    {
                        var vertexLevelIds = (Dictionary<string, int>)propertiesManager.GetProperty(Constants.VertexProgressiveLevelIds, new Dictionary<string, int>());
                        var vertexProgIds = (List<int>)propertiesManager.GetProperty(Constants.VertexProgressiveIds, new List<int>());
    
                        progService.LevelIds.SetProgressiveLevelIds(vertexLevelIds);
                        progService.VertexProgressiveIds = vertexProgIds;
                        propertiesManager.SetProperty(GamingConstants.ProgressiveConfiguredLevelIds, vertexLevelIds);
                        propertiesManager.SetProperty(GamingConstants.ProgressiveConfiguredIds, vertexProgIds);
                        progService.engine = engine;
                        if (vertexProgIds != null && vertexProgIds.Count > 0)
                        {
                            progService?.UpdateVertexProgressives(false, true);
                        }
                        else
                        {
                            progService?.UpdateVertexProgressives();
                        }
                    }
                }


                // Handle all saved startup events
                eventListener.HandleStartupEvents(
                    (consumerType) => _container.GetAllInstances(consumerType).FirstOrDefault() as dynamic);
                _container.GetInstance<IMeterManager>().AddProvider(_container.GetInstance<IG2SMeterProvider>());

                // TODO : Move this to a central location after all protocols are initialized when multiple protocols are supported.
                ServiceManager.GetInstance().GetService<IEventBus>().Publish(new ProtocolsInitializedEvent());

                // Keep it running...
                _shutdownEvent.WaitOne();
            }

            ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);

            Logger.Debug("Stopping G2S engine");

            engine.Stop();

            ModelMappingRules.Reset();

            Unload();

            Logger.Debug("End of OnRun() for G2SBase");
        }

        /// <inheritdoc />
        protected override void OnStop()
        {
            LogToMessageDisplay(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClosingProtocol));

            // Allow OnRun to exit
            _shutdownEvent?.Set();

            Logger.Debug("End of OnStop() for G2SBase");
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);

                _container?.Dispose();

                if (_sharedConsumerContext != null)
                {
                    _sharedConsumerContext.Dispose();
                }

                if (_serviceWaiter != null)
                {
                    _serviceWaiter.Dispose();
                }

                if (_shutdownEvent != null)
                {
                    _shutdownEvent.Close();
                }

                ModelMappingRules.Reset();
            }

            _container = null;
            _sharedConsumerContext = null;
            _serviceWaiter = null;
            _shutdownEvent = null;

            base.Dispose(disposing);
        }

        private static void LogToMessageDisplay(string message)
        {
            var display = ServiceManager.GetInstance().GetService<IMessageDisplay>();

            display.DisplayStatus(message);
        }

        private static void ResolveHandlers(Container container)
        {
            var egm = container.GetInstance<IG2SEgm>();
            var serviceType = typeof(ICommandHandler<,>);
            var assemblies = new[] { Assembly.GetExecutingAssembly() };

            var registrations =
                from assembly in assemblies
                where !assembly.IsDynamic
                from type in assembly.GetExportedTypes()
                where type.IsImplementationOf(serviceType)
                where !type.IsAbstract
                select new
                {
                    implementation = type.GetInterfaces()
                        .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == serviceType),
                    service = type
                };

            foreach (var registration in registrations)
            {
                var handler = container.GetInstance(registration.implementation);

                egm.RegisterHandler(handler as ICommandHandler);
            }
        }

        private static void CreateVirtualIdReaders()
        {
            // TODO: eventually read this from a configuration

            var provider = ServiceManager.GetInstance().TryGetService<IIdReaderProvider>();

            if (provider == null)
            {
                return;
            }

            if (!provider.Adapters.Any())
            {
                Logger.Debug("Adding a second virtual reader as the first reader");
                AddVirtualAdapter(provider, @"Virtual2");
            }

            AddVirtualAdapter(provider, @"Virtual");

            Logger.Debug($"Reader count: {provider.Adapters.Count()}");
        }

        private static void AddVirtualAdapter(IIdReaderProvider provider, string idReaderType)
        {
            if (provider.Adapters.All(a => a.ServiceProtocol != idReaderType))
            {
                var adapter = provider.CreateAdapter(idReaderType);
                adapter.ServiceProtocol = idReaderType;
                adapter.Initialize();
                adapter.Enable(EnabledReasons.Device);

                var comConfiguration = new ComConfiguration
                {
                    Mode = idReaderType
                };

                provider.Inspect(adapter.IdReaderId, comConfiguration, 0);
            }
        }

        private void Unload()
        {
            if (_container != null)
            {
                if (ServiceManager.GetInstance().GetService<IValidationProvider>().UnRegister(
                    ProtocolNames.G2S,
                    _container.GetInstance<IVoucherValidator>()))
                {
                    Logger.Debug("Unregistered G2S IVoucherValidator ");
                }
                ServiceManager.GetInstance().RemoveService(_container.GetInstance<IVoucherDataService>() as IService);
                if (_g2sProgressivesEnabled)
                {
                    ServiceManager.GetInstance().RemoveService(_container.GetInstance<IProgressiveService>() as IService);
                }
                ServiceManager.GetInstance().RemoveService(_container.GetInstance<IMasterResetService>() as IService);
                ServiceManager.GetInstance().RemoveService(_container.GetInstance<IInformedPlayerService>() as IService);
                ServiceManager.GetInstance().RemoveService(_container.GetInstance<IIdReaderValidator>());
                if (ServiceManager.GetInstance().GetService<IValidationProvider>().UnRegister(
                    ProtocolNames.G2S,
                    _container.GetInstance<IHandpayValidator>()))
                {
                    Logger.Debug("Unregistered G2S IVoucherValidator ");
                }
                _container.GetInstance<IEmdi>().Unload();
                ServiceManager.GetInstance().RemoveService(_container.GetInstance<IEmdi>());
            }

            if (_containerService != null)
            {
                ServiceManager.GetInstance().RemoveService(_containerService);
            }

            if (_sharedConsumerContext != null)
            {
                ServiceManager.GetInstance().RemoveService(_sharedConsumerContext);
            }
        }
    }
}