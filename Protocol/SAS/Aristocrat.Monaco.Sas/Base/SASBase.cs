namespace Aristocrat.Monaco.Sas.Base
{
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Sas.Client;
    using Common.Container;
    using CompositionRoot;
    using Consumers;
    using Contracts.Client;
    using Contracts.Events;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Hardware.Contracts.SerialPorts;
    using Kernel;
    using Kernel.Contracts.Events;
    using Localization.Properties;
    using log4net;
    using Protocol.Common.Storage.Entity;
    using SimpleInjector;
    using Storage.Models;
    using Storage.Repository;
    using VoucherValidation;

    /// <summary>
    ///     Handles the base level Sas communications including meter management and system events.
    /// </summary>
    [ProtocolCapability(
        protocol: CommsProtocol.SAS,
        isValidationSupported: true,
        isFundTransferSupported: true,
        isProgressivesSupported: true,
        isCentralDeterminationSystemSupported: false)]
    public sealed class SasBase : BaseRunnable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private bool _disposed;
        private ManualResetEvent _shutdownEvent = new(false);
        private ManualResetEvent _startupWaiter = new(false);
        private ISasHost _sasHost;
        private IProtocolProgressiveEventHandler _linkedProgressiveExpiredConsumer;
        private IProtocolProgressiveEventHandler _progressiveHitConsumer;

        /// <summary>
        ///     Get the container
        /// </summary>
        public static Container Container { get; private set; }

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            ServiceManager.GetInstance().GetService<IEventBus>()
                .Subscribe<InitializationCompletedEvent>(this, _ => _startupWaiter.Set());
            ServiceManager.GetInstance().GetService<IEventBus>().Subscribe<RestartProtocolEvent>(this, _ => OnRestart());
            var disableManager = ServiceManager.GetInstance().GetService<ISystemDisableManager>();
            disableManager.Disable(BaseConstants.ProtocolDisabledKey, SystemDisablePriority.Immediate, () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SasProtocolInitializing));

            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            // wait for the InitializationCompletedEvent which indicated
            // all the components we will use have been loaded
            if (!propertiesManager.GetValue(SasProperties.SasShutdownCommandReceivedKey, false))
            {
                _startupWaiter.WaitOne();
            }

            propertiesManager.SetProperty(SasProperties.SasShutdownCommandReceivedKey, false);
            Logger.Debug("Runnable initialized!");
        }

        /// <inheritdoc />
        /// <exception cref="RunnableException">Thrown when Run() is called a second time without calling Stop().</exception>
        protected override void OnRun()
        {
            Logger.Debug("OnRun started");
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var disableManager = ServiceManager.GetInstance().GetService<ISystemDisableManager>();

            Logger.Debug("OnRun got InitializationCompletedEvent");
            if (RunState == RunnableState.Running)
            {
                Bootstrapper.OnAddingService(new SharedConsumerContext());

                Container = Bootstrapper.ConfigureContainer();
                Container.Verify();

                _linkedProgressiveExpiredConsumer = Container.GetInstance<LinkedProgressiveExpiredConsumer>();
                _progressiveHitConsumer = Container.GetInstance<ProgressiveHitConsumer>();

                SubscribeProgressiveEvents();

                // Container.Verify will create all the SAS event consumers, so stop the
                // SAS StartupEventListener from queueing up more events
                var eventListener = ServiceManager.GetInstance().GetService<StartupEventListener>();
                eventListener.Unsubscribe();

                _sasHost = Container.GetInstance<ISasHost>();

                var validationHandlerFactory = Container.GetInstance<SasValidationHandlerFactory>();
                var validationHandler = validationHandlerFactory.GetValidationHandler();

                // inject dependencies into SasHost
                _sasHost.InjectDependencies(
                    Container.GetInstance<IPropertiesManager>(),
                    Container.GetInstance<IEventBus>(),
                    Container.GetInstance<ISerialPortsService>());

                // Initialize the Sas connections.
                InitializeConnections(validationHandler);

                // Get the system configuration.
                var configuration = Container.GetInstance<IUnitOfWorkFactory>()
                    .Invoke(x => x.Repository<Host>().GetConfiguration());
                _sasHost.SetConfiguration(configuration);

                Bootstrapper.EnableServices(Container);

                // TODO: Moving after SAS progressive creation, otherwise providerId for it will NOT be set.
                // TODO : Move this to a central location after all protocols are initialized when multiple protocols are supported.
                // NOTE: Placed in OnRun because no other necessary instances are created before here.
                ServiceManager.GetInstance()
                    .GetService<IEventBus>()
                    .Publish(new ProtocolsInitializedEvent());

                propertiesManager.SetProperty(AccountingConstants.RequestNonCash, false); // SAS Requires handpays to exclude non cash
                propertiesManager.SetProperty(AccountingConstants.IgnoreVoucherStackedDuringReboot, false);

                var sasValidationType = propertiesManager.GetValue(
                    SasProperties.SasFeatureSettings,
                    new SasFeatures()).ValidationType;

                var ticketBarcodeLength = sasValidationType == SasValidationType.None
                    ? SasConstants.SasNoneValidationTicketBarcodeLength
                    : AccountingConstants.DefaultTicketBarcodeLength;

                propertiesManager.SetProperty(AccountingConstants.TicketBarcodeLength, ticketBarcodeLength);

                _sasHost.StartEventSystem();

                ServiceManager.GetInstance().GetService<IMeterManager>().CreateSnapshot();

                if (RunState == RunnableState.Running)
                {
                    // Handle all saved startup events
                    eventListener.HandleStartupEvents(consumerType => Container.GetAllInstances(consumerType).FirstOrDefault());
                    Container.GetInstance<ISystemEventHandler>().OnSasStarted();

                    Container.GetInstance<ISasTicketPrintedHandler>().ProcessPendingTickets();
                    Container.GetInstance<ISasHandPayCommittedHandler>().Recover();
                    Container.GetInstance<IAftTransferProvider>().OnSasInitialized();

                    validationHandler?.Initialize();

                    // check if components changed since the last boot
                    if (Container.GetInstance<IComponentMonitor>().HaveComponentsChangedWhilePoweredOff("SAS"))
                    {
                        Logger.Debug("Sending Component changed exception");
                        Container.GetInstance<ISasExceptionHandler>()
                            .ReportException(new GenericExceptionBuilder(GeneralExceptionCode.ComponentListChanged));
                    }

                    var gameProvider = Container.GetInstance<IGameProvider>();
                    var activeGames = gameProvider.GetGames();
                    foreach (var game in activeGames.Where(g => (g.Status & GameStatus.DisabledByBackend) == GameStatus.DisabledByBackend))
                    {
                        gameProvider.EnableGame(game.Id, GameStatus.DisabledByBackend);
                    }

                    disableManager.Enable(BaseConstants.ProtocolDisabledKey);
                    _shutdownEvent.WaitOne();
                    UnSubscribeProgressiveEvents();
                    ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);
                    _sasHost.StopEventSystem();
                }
                else
                {
                    UnSubscribeProgressiveEvents();
                    ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);
                }

                Bootstrapper.OnExiting();
            }

            // Set Sas property for Sas communications offline
            Container?.GetInstance<IPropertiesManager>().SetProperty(SasProperties.SasCommunicationsOfflineKey, true);
            _startupWaiter?.Set();

            Logger.Debug("End of OnRun().");
        }

        /// <summary>
        ///     Terminates execution of Run().
        /// </summary>
        protected override void OnStop()
        {
            // Set the Sas property for the Sas shutdown command received
            Container?.GetInstance<IPropertiesManager>().SetProperty(SasProperties.SasShutdownCommandReceivedKey, true);
            Container?.GetInstance<ISasDisableProvider>().OnSasReconfigured().Wait();

            _shutdownEvent?.Set();
            Logger.Debug("End of OnStop()!");
        }

        /// <summary>
        ///     Disposes of stuff.  (SasHost, _shutdownEvent, _serviceWaiter, etc...)
        /// </summary>
        /// <param name="disposing">true if the object is being disposed of.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                UnSubscribeProgressiveEvents();
                if (_sasHost != null)
                {
                    _sasHost.Dispose();
                    _sasHost = null;
                }

                if (_shutdownEvent != null)
                {
                    _shutdownEvent.Close();
                    _shutdownEvent = null;
                }

                if (_startupWaiter != null)
                {
                    _startupWaiter.Close();
                    _startupWaiter = null;
                }

                if (Container != null)
                {
                    Container.Dispose();
                    Container = null;
                }
            }

            _disposed = true;
        }

        private void OnRestart()
        {
            _sasHost.HandlePendingExceptions();
            OnStop();
        }

        private void SubscribeProgressiveEvents()
        {
            var eventSubscriber = ServiceManager.GetInstance().GetService<IProtocolProgressiveEventsRegistry>();
            eventSubscriber.SubscribeProgressiveEvent<LinkedProgressiveExpiredEvent>(
                ProtocolNames.SAS,
                _linkedProgressiveExpiredConsumer);
            eventSubscriber.SubscribeProgressiveEvent<ProgressiveHitEvent>(
                ProtocolNames.SAS,
                _progressiveHitConsumer);
        }

        private void UnSubscribeProgressiveEvents()
        {
            if (_progressiveHitConsumer == null || _linkedProgressiveExpiredConsumer == null)
            {
                return;
            }

            var eventSubscriber = ServiceManager.GetInstance().TryGetService<IProtocolProgressiveEventsRegistry>();
            eventSubscriber?.UnSubscribeProgressiveEvent<LinkedProgressiveExpiredEvent>(
                ProtocolNames.SAS,
                _linkedProgressiveExpiredConsumer);
            eventSubscriber?.UnSubscribeProgressiveEvent<ProgressiveHitEvent>(
                ProtocolNames.SAS,
                _progressiveHitConsumer);

            _progressiveHitConsumer = null;
            _linkedProgressiveExpiredConsumer = null;
        }

        private void InitializeConnections(IValidationHandler validationHandler)
        {
            var propertyManager = Container.GetInstance<IPropertiesManager>();
            _sasHost.Initialize(
                Container.GetInstance<ISasDisableProvider>(),
                Container.GetInstance<IUnitOfWorkFactory>());

            _sasHost.RegisterHandlers(
                Container.GetInstance<ISasExceptionHandler>(),
                Container.GetOpenGenericList<ISasLongPollHandler>(
                    typeof(ISasLongPollHandler<,>),
                    Assembly.Load("Aristocrat.Monaco.Sas")).ToList(),
                validationHandler,
                Container.GetInstance<ISasTicketPrintedHandler>(),
                Container.GetInstance<IAftRegistrationProvider>(),
                Container.GetInstance<ISasHandPayCommittedHandler>(),
                Container.GetInstance<IAftTransferProvider>(),
                Container.GetInstance<ISasVoucherInProvider>());

            // Set Sas property for Sas communications online
            propertyManager.SetProperty(SasProperties.SasCommunicationsOfflineKey, false);
        }
    }
}
