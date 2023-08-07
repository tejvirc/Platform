namespace Aristocrat.Monaco.Demonstration
{
    using System;
    using System.Reflection;
    using System.Threading;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Gaming.Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Localization.Properties;
    using log4net;

    /// <summary>
    ///     Definition of the DemonstrationRunnable class.
    /// </summary>
    [ProtocolCapability(
    protocol:CommsProtocol.DemonstrationMode,
    isValidationSupported:true,
    isFundTransferSupported:false,
    isProgressivesSupported:false,
    isCentralDeterminationSystemSupported:false)]
    public class DemonstrationRunnable : BaseRunnable
    {
        private const int TimeoutInMilliseconds = 1000;
        private const string DenominationMeterNamePrefix = "BillCount";
        private const string DenominationMeterNamePostfix = "s";

        private static readonly Guid RequestorId = new Guid("{D45304D5-ADEF-4770-89A7-07D0F6203985}");
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private ServiceWaiter _serviceWaiter = new ServiceWaiter(ServiceManager.GetInstance().GetService<IEventBus>());
        private ManualResetEvent _shutdownEvent = new ManualResetEvent(false);

        private HandpayValidator _handpayValidator;
        private VoucherValidator _voucherValidator;

        protected override void OnInitialize()
        {
            var serviceManager = ServiceManager.GetInstance();

            _serviceWaiter.AddServiceToWaitFor<ITransactionCoordinator>();
            _serviceWaiter.AddServiceToWaitFor<ICabinetService>();
            _serviceWaiter.AddServiceToWaitFor<IGameHistory>();
            _serviceWaiter.AddServiceToWaitFor<IPlayerBank>();
            _serviceWaiter.AddServiceToWaitFor<IAttendantService>();
            _serviceWaiter.AddServiceToWaitFor<IGameProvider>();
            _serviceWaiter.AddServiceToWaitFor<INoteAcceptor>();
            _serviceWaiter.AddServiceToWaitFor<ITransactionHistory>();
            _serviceWaiter.WaitForServices();

            // Demonstration Overlay
            if (!serviceManager.GetService<IPropertiesManager>().GetValue(ApplicationConstants.ShowMode, false))
            {
                serviceManager.GetService<IPropertiesManager>().SetProperty(
                    ApplicationConstants.ActiveProtocol,
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DemonstrationPage));
            }

            // Handpay Validator
            _handpayValidator = new HandpayValidator();
            _handpayValidator.Initialize();
            ServiceManager.GetInstance().GetService<IValidationProvider>().Register(
                ApplicationConstants.DemonstrationMode,
                _handpayValidator);
           

            // Voucher Validator
            _voucherValidator = new VoucherValidator();
            _voucherValidator.Initialize();
            ServiceManager.GetInstance().GetService<IValidationProvider>().Register(
                ApplicationConstants.DemonstrationMode,
                _voucherValidator);

            var gameProvider = serviceManager.GetService<IGameProvider>();
            foreach (var game in gameProvider.GetGames())
            {
                gameProvider.EnableGame(game.Id, GameStatus.DisabledByBackend);
            }

            // Disable NoteAcceptor
            serviceManager.TryGetService<INoteAcceptor>()?.Disable(DisabledReasons.Backend);
        }

        protected override void OnRun()
        {
            var serviceManager = ServiceManager.GetInstance();
            var eventBus = serviceManager.GetService<IEventBus>();
            eventBus.Subscribe<DemonstrationCurrencyEvent>(this, HandleEvent);

            // Subscribe for Demonstration Exit Handling Events
            eventBus.Subscribe<DemonstrationExitingEvent>(this, HandleEvent);
            eventBus.Subscribe<PersistentStorageClearStartedEvent>(
                this,
                _ =>
                {
                    serviceManager.GetService<IPersistentStorageManager>().StorageClearingEventHandler
                        += OnStorageClearing;
                });

            // Keep it running...
            _shutdownEvent.WaitOne();
        }

        protected override void OnStop()
        {
            var serviceManager = ServiceManager.GetInstance();
            serviceManager.RemoveService(_handpayValidator);
            serviceManager.RemoveService(_voucherValidator);
            _handpayValidator = null;
            _voucherValidator = null;

            // Allow exit
            _shutdownEvent?.Set();
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
                if (_serviceWaiter != null)
                {
                    _serviceWaiter.Dispose();
                    _serviceWaiter = null;
                }

                if (_shutdownEvent != null)
                {
                    _shutdownEvent.Close();
                }

                ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);
            }

            _serviceWaiter = null;
            _shutdownEvent = null;

            base.Dispose(disposing);
        }

        private static void HandleEvent(DemonstrationCurrencyEvent currencyEvent)
        {
            var serviceMgr = ServiceManager.GetInstance();
            var eventBus = serviceMgr.GetService<IEventBus>();
            var propertiesManager = serviceMgr.GetService<IPropertiesManager>();
            var bank = serviceMgr.GetService<IBank>();

            var maxCreditMeter = propertiesManager.GetValue(
                AccountingConstants.MaxCreditMeter,
                0L);

            var allowCreditsInAboveMaxCredit = propertiesManager.GetValue(
                AccountingConstants.AllowCreditsInAboveMaxCredit,
                false);

            // Cash out as soon as bank goes beyond maxCreditMeter and credits in is allowed above max credit limit.
            if (allowCreditsInAboveMaxCredit && bank.QueryBalance() >= maxCreditMeter)
            {
                return;
            }

            var denominationToCurrencyMultiplier =
                propertiesManager.GetValue(
                    ApplicationConstants.CurrencyMultiplierKey,
                    ApplicationConstants.DefaultCurrencyMultiplier);
            var amount = (long)(currencyEvent.Amount * denominationToCurrencyMultiplier);

            var coordinator = serviceMgr.GetService<ITransactionCoordinator>();

            var guid = coordinator.RequestTransaction(RequestorId, TimeoutInMilliseconds, TransactionType.Write);

            if (guid != Guid.Empty)
            {
                if (bank.CheckDeposit(AccountType.Cashable, amount, guid))
                {
                    bank.Deposit(AccountType.Cashable, amount, guid);
                }
            }

            coordinator.ReleaseTransaction(guid);
            var transaction = RecordTransaction(amount);
            eventBus?.Publish(new CurrencyInCompletedEvent(amount, null, transaction));
        }

        private static BillTransaction RecordTransaction(long amount)
        {
            var serviceManager = ServiceManager.GetInstance();
            var history = serviceManager.GetService<ITransactionHistory>();

            var propertiesManager = serviceManager.GetService<IPropertiesManager>();
            var currencyId = propertiesManager
                .GetValue(ApplicationConstants.CurrencyId, string.Empty).ToCharArray(0, 3);
            var denominationToCurrencyMultiplier =
                propertiesManager.GetValue(
                    ApplicationConstants.CurrencyMultiplierKey,
                    ApplicationConstants.DefaultCurrencyMultiplier);
            var denomination = (long)(amount / denominationToCurrencyMultiplier);

            var meterName = DenominationMeterNamePrefix + denomination + DenominationMeterNamePostfix;
            var meterManager = serviceManager.GetService<IMeterManager>();
            var meter = meterManager.GetMeter(meterName);
            meter.Increment(1);

            var billTransaction = new BillTransaction(
                currencyId,
                1,
                DateTime.UtcNow,
                amount) { Accepted = DateTime.UtcNow, Denomination = denomination };

            history.AddTransaction(billTransaction);
            return billTransaction;
        }

        private static void HandleEvent(DemonstrationExitingEvent exitingEvent)
        {
            Logger.Debug("Demonstration: Starting cleaning PersistenceLevel.Critical");
            var storage = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
            storage.Clear(PersistenceLevel.Critical);
        }

        private static void OnStorageClearing(object sender, StorageEventArgs e)
        {
            // Reset only when Exiting from Demonstration Page
            ServiceManager.GetInstance().GetService<IMessageDisplay>().DisplayMessage(
                new DisplayableMessage(
                    () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DemonstrationExitingMessage),
                    DisplayableMessageClassification.Diagnostic,
                    DisplayableMessagePriority.Immediate));

            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            properties.SetProperty(ApplicationConstants.IsInitialConfigurationComplete, false);
            properties.SetProperty(ApplicationConstants.DemonstrationMode, false);

            ServiceManager.GetInstance().GetService<IPersistentStorageManager>().StorageClearingEventHandler -=
                OnStorageClearing;
            Logger.Debug("Demonstration: Exit successful");
        }
    }
}