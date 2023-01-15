namespace Aristocrat.Monaco.Mgam
{
    using System;
    using System.Threading;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts;
    using Application.Contracts.Identification;
    using Application.Contracts.Localization;
    using Aristocrat.Mgam.Client.Logging;
    using Common;
    using CompositionRoot;
    using Consumers;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Gaming.Contracts.Session;
    using Kernel;
    using Kernel.Contracts.MessageDisplay;
    using Localization.Properties;
    using Services.Attributes;
    using Services.Communications;
    using Services.Event;
    using Services.GameConfiguration;
    using Services.Security;
    using Services.SoftwareValidation;
    using SimpleInjector;

    /// <summary>
    ///     Launcher for the MGAM protocol.
    /// </summary>
    [ProtocolCapability(
        protocol: CommsProtocol.MGAM,
        isValidationSupported: true,
        isFundTransferSupported: false,
        isProgressivesSupported: true,
        isCentralDeterminationSystemSupported: true)]
    public class MgamBase : BaseRunnable
    {
        private static readonly ILogger Logger = Log4NetLoggerFactory.CreateLogger<MgamBase>();

        private static bool _isFirstLoad = true;

        private ServiceWaiter _serviceWaiter;

        private ManualResetEvent _shutdownEvent = new ManualResetEvent(false);

        //private SharedConsumerContext _sharedConsumerContext;

        private Container _container;

        /// <summary>
        ///     Initialized a new instance of the <see cref="MgamBase"/> class.
        /// </summary>
        public MgamBase()
            : this(ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        /// <summary>
        ///     Initialized a new instance of the <see cref="MgamBase"/> class.
        /// </summary>
        /// <param name="eventBus"><see cref="IEventBus"/>.</param>
        public MgamBase(IEventBus eventBus)
        {
            _serviceWaiter = new ServiceWaiter(eventBus);
        }

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            Logger.LogDebug($"{nameof(MgamBase)} initializing");

            _serviceWaiter.AddServiceToWaitFor<ITransactionCoordinator>();
            _serviceWaiter.AddServiceToWaitFor<ICabinetService>();
            _serviceWaiter.AddServiceToWaitFor<IGameHistory>();
            _serviceWaiter.AddServiceToWaitFor<IPlayerBank>();
            _serviceWaiter.AddServiceToWaitFor<IPlayerSessionHistory>();
            _serviceWaiter.AddServiceToWaitFor<IAttendantService>();
            _serviceWaiter.AddServiceToWaitFor<IProtocolLinkedProgressiveAdapter>();
            _serviceWaiter.AddServiceToWaitFor<IIdentificationProvider>();

            if (_serviceWaiter.WaitForServices())
            {
                _container = Bootstrapper.InitializeContainer();

                _container.Verify();

                _container.GetInstance<StartupEventListener>().Unsubscribe();

                ServiceManager.GetInstance()
                    .AddService(_container.GetInstance<ISharedConsumer>());

                ServiceManager.GetInstance()
                    .AddServiceAndInitialize(_container.GetInstance<IIdentificationValidator>() as IService);

                ServiceManager.GetInstance()
                    .AddServiceAndInitialize(_container.GetInstance<ICertificateService>() as IService);

                ServiceManager.GetInstance()
                    .AddServiceAndInitialize(_container.GetInstance<IAttributeManager>() as IService);

                ServiceManager.GetInstance()
                    .AddServiceAndInitialize(_container.GetInstance<IGameConfigurator>() as IService);

                ServiceManager.GetInstance()
                    .AddServiceAndInitialize(_container.GetInstance<IHostTranscripts>() as IService);

                ServiceManager.GetInstance()
                    .AddServiceAndInitialize(_container.GetInstance<IChecksumCalculator>() as IService);

                if (ServiceManager.GetInstance().GetService<IValidationProvider>().Register(
                    ProtocolNames.MGAM,
                    _container.GetInstance<IVoucherValidator>()))
                {
                    Logger.LogDebug("IVoucherValidator has been registered");
                }
                _container.GetInstance<IVoucherValidator>().Initialize();

                if (ServiceManager.GetInstance().GetService<IValidationProvider>().Register(
                    ProtocolNames.MGAM,
                    _container.GetInstance<IHandpayValidator>()))
                {
                    Logger.LogDebug("IHandpayValidator has been registered");
                }
                _container.GetInstance<IHandpayValidator>().Initialize();

                if (ServiceManager.GetInstance().GetService<IValidationProvider>().Register(
                    ProtocolNames.MGAM,
                    _container.GetInstance<ICurrencyValidator>()))
                {
                    Logger.LogDebug("ICurrencyValidator has been registered");
                }
                _container.GetInstance<ICurrencyValidator>().Initialize();

                ServiceManager.GetInstance().GetService<IProtocolProgressiveEventsRegistry>()
                    .SubscribeProgressiveEvent<LinkedProgressiveHitEvent>(
                        ProtocolNames.MGAM,
                        _container.GetInstance<LinkedProgressiveHitConsumer>());
            }
            else
            {
                throw new InvalidOperationException("Required services not available");
            }

            Logger.LogDebug($"{nameof(MgamBase)} initialized");
        }

        /// <inheritdoc />
        protected override void OnRun()
        {
            Logger.LogDebug($"{nameof(MgamBase)} running");

            var disableManager = ServiceManager.GetInstance().GetService<ISystemDisableManager>();

            // EGM should start in a Host Disconnected disabled state until host is connected
            disableManager.Disable(
                MgamConstants.HostOfflineGuid,
                SystemDisablePriority.Normal,
                ResourceKeys.HostDisconnected,
                CultureProviderType.Player);

            disableManager.Disable(
                MgamConstants.GamePlayDisabledKey,
                SystemDisablePriority.Normal,
                ResourceKeys.DisabledByHost,
                CultureProviderType.Player);

            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            propertiesManager.SetProperty(AccountingConstants.ValidateHandpays, false); // MGAM does not require handpay validation

            var engine = _container.GetInstance<IEngine>();

            var startUpContext = new StartupContext();

            // don't start the engine if we've already signaled a shutdown or Disposed
            var start = false;

            if (_shutdownEvent != null)
            {
                start = !_shutdownEvent.WaitOne(0);
            }

            if (start)
            {
                engine.Start(
                    startUpContext,
                    () =>
                    {
                        if (_isFirstLoad)
                        {
                            _container.GetInstance<ISystemDisableManager>()
                                .Enable(MgamConstants.ProtocolDisabledKey);
                            _isFirstLoad = false;
                        }
                    }).Wait();

                _container.GetInstance<ISoftwareValidator>().Validate();
                // Uncomment below and comment above to test in the lab
                // _container.GetInstance<IEventBus>().Publish(new ProtocolsInitializedEvent());

                _container.GetInstance<StartupEventListener>().Publish();

                // Keep it running...
                _shutdownEvent.WaitOne();
            }

            _container.GetInstance<IChecksumCalculator>().Stop();

            _container.GetInstance<IEventDispatcher>().Unsubscribe();

            Logger.LogDebug($"Stopping {nameof(MgamBase)}");

            _container.GetInstance<IEngine>().Stop().Wait();

            Unloaded();

            Logger.LogDebug($"{nameof(MgamBase)} exiting");
        }

        /// <inheritdoc />
        protected override void OnStop()
        {
            Logger.LogDebug($"{nameof(MgamBase)} stopping");

            _shutdownEvent.Set();

            Logger.LogDebug($"{nameof(MgamBase)} stopped");
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);

                var eventSubscriber = ServiceManager.GetInstance().GetService<IProtocolProgressiveEventsRegistry>();
                eventSubscriber.UnSubscribeProgressiveEvent<LinkedProgressiveHitEvent>(
                    ProtocolNames.MGAM,
                    _container.GetInstance<LinkedProgressiveHitConsumer>());

                if (_serviceWaiter != null)
                {
                    _serviceWaiter.Dispose();
                }

                if (_shutdownEvent != null)
                {
                    _shutdownEvent.Dispose();
                }

                if (_container != null)
                {
                    _container.Dispose();
                }
            }

            _serviceWaiter = null;
            _shutdownEvent = null;
            _container = null;
        }

        private void Unloaded()
        {
            if (ServiceManager.GetInstance().GetService<IValidationProvider>().UnRegister(
                ProtocolNames.MGAM,
                _container.GetInstance<IVoucherValidator>()))
            {
                Logger.LogDebug("Unregistered IVoucherValidator");
            }

            if (ServiceManager.GetInstance().GetService<IValidationProvider>().UnRegister(
                ProtocolNames.MGAM,
                _container.GetInstance<IHandpayValidator>()))
            {
                Logger.LogDebug("Unregistered IHandpayValidator");
            }

            if (ServiceManager.GetInstance().GetService<IValidationProvider>().UnRegister(
                ProtocolNames.MGAM,
                _container.GetInstance<ICurrencyValidator>()))
            {
                Logger.LogDebug("Unregistered ICurrencyValidator");
            }

            ServiceManager.GetInstance().RemoveService(_container.GetInstance<IIdentificationValidator>() as IService);
            ServiceManager.GetInstance().RemoveService(_container.GetInstance<ICertificateService>() as IService);
            ServiceManager.GetInstance().RemoveService(_container.GetInstance<IGameConfigurator>() as IService);
            ServiceManager.GetInstance().RemoveService(_container.GetInstance<IHostTranscripts>() as IService);
            ServiceManager.GetInstance().RemoveService(_container.GetInstance<IAttributeManager>() as IService);
            ServiceManager.GetInstance().RemoveService(_container.GetInstance<IChecksumCalculator>() as IService);
            ServiceManager.GetInstance().RemoveService(_container.GetInstance<ISharedConsumer>());
        }
    }
}
