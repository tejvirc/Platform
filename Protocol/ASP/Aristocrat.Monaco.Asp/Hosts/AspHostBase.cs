namespace Aristocrat.Monaco.Asp.Hosts
{
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Client.Consumers;
    using Client.Contracts;
    using CompositionRoot;
    using Events;
    using Gaming.Contracts;
    using Hardware.Contracts.SerialPorts;
    using Kernel;
    using log4net;
    using SimpleInjector;

    /// <summary>
    ///     Definition of the AspHostBase class
    /// </summary>
    public abstract class AspHostBase : BaseRunnable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private bool _disposed;
        private ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
        private IAspClient _aspClient;

        private static string PhysicalCommPort => "COM7";

        /// <summary>
        ///     Get the container
        /// </summary>
        public static Container Container { get; private set; }

        private ISerialPortsService SerialPortsService { get; }

        protected abstract ProtocolSettings Settings { get; }

        protected AspHostBase()
        {
            SerialPortsService = ServiceManager.GetInstance().GetService<ISerialPortsService>();
        }

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            Logger.Debug("Runnable initialized!");
        }

        /// <inheritdoc />
        /// <exception cref="RunnableException">Thrown when Run() is called a second time without calling Stop().</exception>
        protected override void OnRun()
        {
            Logger.Debug("OnRun started");
            if (!WaitForServices()) return;

            EnableGames();

            Container = Bootstrapper.ConfigureContainer(Settings);
            Container.Verify();

            ServiceManager.GetInstance().AddService(Container.GetInstance<ISharedConsumer>());

            var eventListener = ServiceManager.GetInstance().GetService<StartupEventListener>();
            eventListener.Unsubscribe();

            if (!StartAspClient())
            {
                Logger.Error($"Failed to start ASP client on port {PhysicalCommPort}");
                return;
            }

            if (RunState == RunnableState.Running)
            {
                eventListener.HandleStartupEvents(consumerType => Container.GetAllInstances(consumerType).FirstOrDefault());

                Logger.Debug("Waiting for ASP Main loop to stop...");
                _shutdownEvent.WaitOne();
                Logger.Debug("ASP Main loop finished...");

                SerialPortsService.UnRegisterPort(PhysicalCommPort);
                Logger.Info($"Unregistered physical comm port {PhysicalCommPort} for ASP.");
            }

            ServiceManager.GetInstance().RemoveService(Container.GetInstance<ISharedConsumer>());

            Logger.Debug("End of OnRun().");
        }

        /// <summary>
        ///     Terminates execution of Run().
        /// </summary>
        protected override void OnStop()
        {
            _aspClient?.Stop();
            Logger.Debug("Asp client stopped.");
            _shutdownEvent?.Set();
            Logger.Debug("End of OnStop()!");
        }

        /// <summary>
        ///     Disposes of stuff.  (DacomHost, _shutdownEvent, _serviceWaiter, etc...)
        /// </summary>
        /// <param name="disposing">true if the object is being disposed of.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_disposed) return;

            if (disposing)
            {
                OnStop();
                _aspClient = null;
                _shutdownEvent.Close();
                _shutdownEvent = null;
            }

            _disposed = true;
        }

        private static void EnableGames()
        {
            var gameProvider = ServiceManager.GetInstance().GetService<IGameProvider>();
            foreach (var game in gameProvider.GetGames())
            {
                gameProvider.EnableGame(game.Id, GameStatus.DisabledByBackend);
            }
        }

        private bool StartAspClient()
        {
            //Register our comm port so we own it
            SerialPortsService.RegisterPort(PhysicalCommPort);
            Logger.Info($"Registered physical comm port {PhysicalCommPort} for ASP.");

            _aspClient = Container.GetInstance<IAspClient>();
            if (!_aspClient.Start(PhysicalCommPort))
            {
                SerialPortsService.UnRegisterPort(PhysicalCommPort);
                Logger.Info($"Unregistered physical comm port {PhysicalCommPort} for ASP.");

                return false;
            }

            Logger.Debug($"ASP client started on port {PhysicalCommPort}");
            Container.GetInstance<IEventBus>().Publish(new AspClientStartedEvent());
            return true;
        }

        private bool WaitForServices()
        {
            using (var serviceWaiter = new ServiceWaiter(ServiceManager.GetInstance().GetService<IEventBus>()))
            {
                serviceWaiter.AddServiceToWaitFor<IGameProvider>();
                serviceWaiter.AddServiceToWaitFor<IGamePlayState>();
                serviceWaiter.AddServiceToWaitFor<IFundsTransferDisable>();
                if (serviceWaiter.WaitForServices() && RunState == RunnableState.Running)
                {
                    Logger.Warn("Wait for services complete.");
                    return true;
                }
            }

            Logger.Warn("Wait for services failed.");
            return false;
        }
    }
}