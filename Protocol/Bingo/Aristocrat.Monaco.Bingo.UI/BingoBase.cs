namespace Aristocrat.Monaco.Bingo.UI
{
    using System;
    using System.Linq;
    using System.Threading;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Application.Contracts.Protocol;
    using Bingo.CompositionRoot;
    using Bingo.Services.Security;
    using Common.Events;
    using CompositionRoot;
    using Gaming.Contracts;
    using Gaming.Contracts.Bonus;
    using Gaming.Contracts.Central;
    using Gaming.Contracts.Meters;
    using Gaming.Contracts.Payment;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Tickets;
    using Hardware.Contracts.Cabinet;
    using Kernel;
    using Localization.Properties;
    using Monaco.Common;
    using SimpleInjector;
    using SimpleInjector.Lifestyles;

    [ProtocolCapability(
        protocol: CommsProtocol.Bingo,
        isValidationSupported: false,
        isFundTransferSupported: false,
        isProgressivesSupported: true,
        isCentralDeterminationSystemSupported: true)]
    public class BingoBase : BaseRunnable
    {
        private static readonly Guid Initializing = new("{9594364A-7446-4313-B375-92E3EF62E3D9}");
        private Container _container = new();
        private ManualResetEvent _shutdownEvent = new(false);
        private ServiceWaiter _serviceWaiter;

        protected override void OnInitialize()
        {
            var disableManager = ServiceManager.GetInstance().GetService<ISystemDisableManager>();
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            disableManager.Disable(
                Initializing,
                SystemDisablePriority.Immediate,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DisabledDuringInitialization),
                false);
            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            var isBingoProgressiveEnabled = ServiceManager.GetInstance().GetService<IMultiProtocolConfigurationProvider>().MultiProtocolConfiguration
                .Any(x => x.IsProgressiveHandled && x.Protocol == CommsProtocol.Bingo);

            eventBus.Subscribe<BingoDisplayConfigurationStartedEvent>(this, async (_, _) =>
            {
                eventBus.Unsubscribe<BingoDisplayConfigurationStartedEvent>(this);
                await _container.GetInstance<IBingoClientConnectionState>().Start();
                if (isBingoProgressiveEnabled)
                {
                    await _container.GetInstance<IProgressiveClientConnectionState>().Start();
                }
                disableManager.Enable(Initializing);
            });

            _serviceWaiter?.Dispose();
            _serviceWaiter = new ServiceWaiter(ServiceManager.GetInstance().GetService<IEventBus>());
            _serviceWaiter.AddServiceToWaitFor<IEventBus>();
            _serviceWaiter.AddServiceToWaitFor<IGameProvider>();
            _serviceWaiter.AddServiceToWaitFor<IMultiProtocolConfigurationProvider>();
            _serviceWaiter.AddServiceToWaitFor<ICabinetDetectionService>();
            _serviceWaiter.AddServiceToWaitFor<ICentralProvider>();
            _serviceWaiter.AddServiceToWaitFor<IGameHistory>();
            _serviceWaiter.AddServiceToWaitFor<IPaymentDeterminationProvider>();
            _serviceWaiter.AddServiceToWaitFor<IGameMeterManager>();
            _serviceWaiter.AddServiceToWaitFor<IBonusHandler>();
            _serviceWaiter.AddServiceToWaitFor<IProtocolLinkedProgressiveAdapter>();

            if (!_serviceWaiter.WaitForServices())
            {
                return;
            }

            _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            _container.InitializeContainer(propertiesManager, isBingoProgressiveEnabled)
                .AddBingoOverlay()
                .AddGameRoundHistoryDetails()
                .AddEventConsumers();

            ServiceManager.GetInstance()
                .AddServiceAndInitialize(_container.GetInstance<ICertificateService>());

            _container.Verify();
        }

        protected override void OnRun()
        {
            var eventBus = _container.GetInstance<IEventBus>();
            if (RunState == RunnableState.Running)
            {
                var eventListener = ServiceManager.GetInstance().GetService<StartupEventListener>();
                eventListener.Unsubscribe();
                eventListener.HandleStartupEvents(evtType => _container.GetAllInstances(evtType).FirstOrDefault());
                eventBus.Publish(new ProtocolInitialized());
                ServiceManager.GetInstance().AddService(_container.GetInstance<IGameRoundDetailsDisplayProvider>());
                ServiceManager.GetInstance().AddService(_container.GetInstance<IGameRoundPrintFormatter>());
            }

            _shutdownEvent.WaitOne();

            _container.GetInstance<IBingoClientConnectionState>().Stop().WaitForCompletion();
            var isBingoProgressiveEnabled = ServiceManager.GetInstance().GetService<IMultiProtocolConfigurationProvider>().MultiProtocolConfiguration
                .Any(x => x.IsProgressiveHandled && x.Protocol == CommsProtocol.Bingo);
            if (isBingoProgressiveEnabled)
            {
                _container.GetInstance<IProgressiveClientConnectionState>().Stop().WaitForCompletion();
            }

            ServiceManager.GetInstance().RemoveService(_container.GetInstance<IGameRoundDetailsDisplayProvider>());
            ServiceManager.GetInstance().RemoveService(_container.GetInstance<IGameRoundPrintFormatter>());
            eventBus.UnsubscribeAll(this);
        }

        protected override void OnStop()
        {
            _serviceWaiter?.Dispose();
            _shutdownEvent.Set();
        }

        protected override void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);
                if (_container != null)
                {
                    _container.Dispose();
                }

                if (_serviceWaiter != null)
                {
                    _serviceWaiter.Dispose();
                }

                if (_shutdownEvent != null)
                {
                    _shutdownEvent.Close();
                }
            }

            _container = null;
            _shutdownEvent = null;
            _serviceWaiter = null;

            base.Dispose(disposing);
        }
    }
}
