namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Threading;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Application.Contracts.Media;
    using Application.Contracts.OperatorMenu;
    using Application.Contracts.TiltLogger;
    using Common;
    using CompositionRoot;
    using Consumers;
    using Contracts;
    using Contracts.Barkeeper;
    using Contracts.Bonus;
    using Contracts.Central;
    using Contracts.Configuration;
    using Contracts.Meters;
    using Contracts.Payment;
    using Contracts.Progressives;
    using Contracts.Progressives.SharedSap;
    using Contracts.Rtp;
    using Contracts.Session;
    using Hardware.Contracts;
    using Kernel;
    using Kernel.Contracts;
    using Localization.Properties;
    using log4net;
    using Progressives;
    using Runtime;
    using SimpleInjector;

    /// <summary>
    ///     This the base class for all gaming layers.
    /// </summary>
    public abstract class GamingRunnable : BaseRunnable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private static readonly TimeSpan ShutdownTimeout = TimeSpan.FromSeconds(30);

        private Container _container;
        private ContainerService _containerService;

        private SharedConsumerContext _sharedConsumerContext;

        private ManualResetEvent _shutdownEvent = new ManualResetEvent(false);

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            _container = Bootstrapper.InitializeContainer();

            _container.Options.AllowOverridingRegistrations = true;

            ConfigureContainer(_container);

            _container.Options.AllowOverridingRegistrations = false;

            Logger.Info("Initialized");
        }

        /// <inheritdoc />
        protected override void OnRun()
        {
            Logger.Info("Gaming OnRun started");

            Load();

            // send notification that all Components have been registered
            _container.GetInstance<IInitializationProvider>().SystemInitializationCompleted();

            // Keep it running...
            _shutdownEvent.WaitOne();

            Unload();
            Thread.Sleep(500);
            Logger.Info("Gaming OnRun complete");
        }

        /// <inheritdoc />
        protected override void OnStop()
        {
            // Allow OnRun to exit
            _shutdownEvent.Set();

            Logger.Info("Gaming Stopped");
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_container != null)
                {
                    _container.Dispose();
                }

                if (_sharedConsumerContext != null)
                {
                    _sharedConsumerContext.Dispose();
                }

                if (_shutdownEvent != null)
                {
                    _shutdownEvent.Close();
                }
            }

            _container = null;
            _sharedConsumerContext = null;
            _shutdownEvent = null;
        }

        /// <summary>
        ///     Allows derived classes to add or override values in the container
        /// </summary>
        /// <param name="container"></param>
        protected abstract void ConfigureContainer(Container container);

        protected abstract void LoadUi(Container container);

        protected abstract void UnloadUi(Container container);

        private static void DisplayMessage(string resourceStringName)
        {
            var display = ServiceManager.GetInstance().GetService<IMessageDisplay>();

            var localizer = Localizer.For(CultureFor.Operator);

            var displayMessage = localizer.GetString(resourceStringName, _ => display.DisplayStatus(resourceStringName));

            if (!string.IsNullOrWhiteSpace(displayMessage))
            {
                display.DisplayStatus(displayMessage);
            }

            var logMessage = localizer.GetString(CultureInfo.InvariantCulture, resourceStringName, _ => Logger.Info(resourceStringName));

            if (!string.IsNullOrWhiteSpace(logMessage))
            {
                Logger.Info(logMessage);
            }
        }

        private void Load()
        {
            LoadPropertyProviders();
            LoadRuntime();
            LoadGames();
            LoadMeterProviders();

            AddServices();

            RegisterLogAdapters();

            // This will forcibly resolve all instances, which will create the Consumers
            _container.Verify();

            // NOTE: This is just to ensure we don't have an orphan process running
            _container.GetInstance<IGameService>().TerminateAny(false, true);

            // Start the RNG cycling service necessary for some APAC markets
            _container.GetInstance<RngCyclingService>().StartCycling();

            // Always load the UI last
            DisplayMessage(ResourceKeys.LoadUi);
            LoadUi(_container);

            HandleStartupEvents();
        }

        private void LoadPropertyProviders()
        {
            var manager = _container.GetInstance<IPropertiesManager>();

            foreach (var provider in _container.GetAllInstances<IPropertyProvider>())
            {
                manager.AddPropertyProvider(provider);
            }
        }

        private void LoadRuntime()
        {
            DisplayMessage(ResourceKeys.LoadingRuntime);

            var runtime = _container.GetInstance<IRuntimeProvider>();

            runtime.Load();
        }

        private void LoadGames()
        {
            var manager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            DisplayMessage(ResourceKeys.DiscoveringGames);

            ServiceManager.GetInstance().AddServiceAndInitialize(_container.GetInstance<IGameInstaller>());

            var provider = _container.GetInstance<IGameProvider>() as IService;
            provider?.Initialize();
            ServiceManager.GetInstance().AddService(provider);
            manager.AddPropertyProvider(provider as IPropertyProvider);
        }

        private void LoadMeterProviders()
        {
            var meterManager = ServiceManager.GetInstance().GetService<IMeterManager>();

            foreach (var provider in _container.GetAllInstances<IMeterProvider>())
            {
                meterManager.AddProvider(provider);
            }
        }

        private void AddServices()
        {
            _sharedConsumerContext = new SharedConsumerContext();

            var serviceManager = ServiceManager.GetInstance();

            _containerService = new ContainerService(_container);

            serviceManager.AddService(_containerService);
            serviceManager.AddService(_sharedConsumerContext);
            serviceManager.AddService(_container.GetInstance<IGameDiagnostics>() as IService);
            serviceManager.AddService(_container.GetInstance<IGameMeterManager>());
            serviceManager.AddService(_container.GetInstance<IProgressiveMeterManager>());
            serviceManager.AddService(_container.GetInstance<IGamePlayState>());
            serviceManager.AddServiceAndInitialize(_container.GetInstance<ICentralProvider>() as IService);
            serviceManager.AddService(_container.GetInstance<ICabinetService>());
            serviceManager.AddService(_container.GetInstance<IButtonDeckFilter>());
            serviceManager.AddService(_container.GetInstance<IGameHistory>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IGameService>() as IService);
            serviceManager.AddService(_container.GetInstance<IPlayerBank>() as IService);
            serviceManager.AddService(_container.GetInstance<IOperatorMenuGamePlayMonitor>());
            serviceManager.AddService(_container.GetInstance<IHardwareHelper>());
            serviceManager.AddService(_container.GetInstance<IButtonLamps>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IBonusHandler>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<ISoftwareInstaller>());
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IGameOrderSettings>());
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IPlayerSessionHistory>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IPlayerService>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IGameCategoryService>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IGameHelpTextProvider>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IBrowserProcessManager>());
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IAttendantService>());
            serviceManager.AddService(_container.GetInstance<IBarkeeperPropertyProvider>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IBarkeeperHandler>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<ICashoutController>() as IService);
            serviceManager.AddService(_container.GetInstance<IUserActivityService>());
            serviceManager.AddService(_container.GetInstance<ITowerLightManager>());
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IGamingAccessEvaluation>());
            serviceManager.AddService(_container.GetInstance<IFundsTransferDisable>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IAutoPlayStatusProvider>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IAttractConfigurationProvider>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IProgressiveLevelProvider>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IProgressiveConfigurationProvider>() as IService);
            serviceManager.AddService(_container.GetInstance<ISharedSapProvider>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<ILinkedProgressiveProvider>() as IService);
            serviceManager.AddService(_container.GetInstance<IProgressiveErrorProvider>() as IService);
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IProtocolLinkedProgressiveAdapter>() as IService);
            serviceManager.AddService(_container.GetInstance<IGameConfigurationProvider>());
            serviceManager.AddService(_container.GetInstance<IConfigurationProvider>());
            serviceManager.AddService(_container.GetInstance<IPaymentDeterminationProvider>());
            serviceManager.AddService(_container.GetInstance<IGameStartConditionProvider>());
            serviceManager.AddService(_container.GetInstance<IOutcomeValidatorProvider>());
            serviceManager.AddServiceAndInitialize(_container.GetInstance<IRtpService>() as IService);
        }

        private void RemoveServices()
        {
            var serviceManager = ServiceManager.GetInstance();

            serviceManager.RemoveService(_containerService);
            serviceManager.RemoveService(_sharedConsumerContext);
            serviceManager.RemoveService(_container.GetInstance<IGameMeterManager>());
            serviceManager.RemoveService(_container.GetInstance<IProgressiveMeterManager>());
            serviceManager.RemoveService(_container.GetInstance<IGamePlayState>());
            serviceManager.RemoveService(_container.GetInstance<ICentralProvider>() as IService);
            serviceManager.RemoveService(_container.GetInstance<ICabinetService>());
            serviceManager.RemoveService(_container.GetInstance<IButtonDeckFilter>());
            serviceManager.RemoveService(_container.GetInstance<IGameHistory>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IGameService>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IGameProvider>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IGameCategoryService>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IPlayerBank>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IOperatorMenuGamePlayMonitor>());
            serviceManager.RemoveService(_container.GetInstance<IHardwareHelper>());
            serviceManager.RemoveService(_container.GetInstance<IButtonLamps>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IBonusHandler>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IGameInstaller>());
            serviceManager.RemoveService(_container.GetInstance<ISoftwareInstaller>());
            serviceManager.RemoveService(_container.GetInstance<IGameOrderSettings>());
            serviceManager.RemoveService(_container.GetInstance<IPlayerService>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IPlayerSessionHistory>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IGameHelpTextProvider>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IBrowserProcessManager>());
            serviceManager.RemoveService(_container.GetInstance<IAttendantService>());
            serviceManager.RemoveService(_container.GetInstance<IUserActivityService>());
            serviceManager.RemoveService(_container.GetInstance<ITowerLightManager>());
            serviceManager.RemoveService(_container.GetInstance<IGamingAccessEvaluation>());
            serviceManager.RemoveService(_container.GetInstance<ICashoutController>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IBarkeeperPropertyProvider>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IBarkeeperHandler>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IFundsTransferDisable>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IAutoPlayStatusProvider>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IAttractConfigurationProvider>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IProgressiveLevelProvider>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IProgressiveConfigurationProvider>() as IService);
            serviceManager.RemoveService(_container.GetInstance<ISharedSapProvider>() as IService);
            serviceManager.RemoveService(_container.GetInstance<ILinkedProgressiveProvider>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IProgressiveErrorProvider>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IProtocolLinkedProgressiveAdapter>() as IService);
            serviceManager.RemoveService(_container.GetInstance<IGameConfigurationProvider>());
            serviceManager.RemoveService(_container.GetInstance<IConfigurationProvider>());
            serviceManager.RemoveService(_container.GetInstance<IPaymentDeterminationProvider>());
            serviceManager.RemoveService(_container.GetInstance<IGameStartConditionProvider>());
            serviceManager.RemoveService(_container.GetInstance<IOutcomeValidatorProvider>());
            serviceManager.RemoveService(_container.GetInstance<IRtpService>() as IService);
        }

        private void Unload()
        {
            // Stop the RNG cycling service necessary for some APAC markets
            _container.GetInstance<RngCyclingService>().StopCycling();

            // End the game process if one is running
            DisplayMessage(ResourceKeys.ClosingGame);
            Logger.Info("Ending game process from Unload");
            _container.GetInstance<IGameService>().TerminateAny(false, true);

            DisplayMessage(ResourceKeys.UnloadUi);
            UnloadUi(_container);

            UnRegisterLogAdapters();

            RemoveServices();
            _container?.Dispose();
        }

        private void HandleStartupEvents()
        {
            var eventListener = ServiceManager.GetInstance().GetService<StartupEventListener>();
            eventListener.HandleStartupEvents(consumerType => _container?.GetInstance(consumerType));
        }

        private void RegisterLogAdapters()
        {
            var logAdapterService = ServiceManager.GetInstance().GetService<ILogAdaptersService>();
            logAdapterService.RegisterLogAdapter(_container.GetInstance<BonusEventLogAdapter>());
            logAdapterService.RegisterLogAdapter(new JackpotEventLogAdapter());
        }

        private void UnRegisterLogAdapters()
        {
            var logAdapterService = ServiceManager.GetInstance().GetService<ILogAdaptersService>();
            logAdapterService.UnRegisterLogAdapter(EventLogType.BonusAward.GetDescription(typeof(EventLogType)));
            logAdapterService.UnRegisterLogAdapter(EventLogType.Progressive.GetDescription(typeof(EventLogType)));
        }
    }
}