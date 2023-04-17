namespace Aristocrat.Monaco.Accounting
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Threading;
    using Application.Contracts.Localization;
    using Application.Contracts.TiltLogger;
    using Common;
    using Kernel;
    using log4net;
    using Mono.Addins;

    /// <summary>
    ///     The BootExtender is responsible for loading components and
    ///     extensions in the Accounting layer.
    /// </summary>
    public class AccountingRunnable : BaseRunnable
    {
        private const string PropertyProvidersPath = "/Accounting/PropertyProviders";
        private const string TransactionCoordinatorExtensionPath = "/Accounting/TransactionCoordinator"; 
        private const string BankExtensionPath = "/Accounting/Bank";
        private const string TransactionHistoryExtensionPath = "/Accounting/TransactionHistory";

        private const string ServicesExtensionPath = "/Accounting/Services";
        private const string RunnablesExtensionPath = "/Accounting/Runnables";
        private const string ExtenderExtensionPath = "/Accounting/BootExtender";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly RunnablesManager _runnablesManager = new RunnablesManager();
        private readonly List<IService> _services = new List<IService>();
        private readonly RunnablesManager _transactionHandlersManager = new RunnablesManager();

        private IService _bank;
        private IRunnable _extender;
        private IService _transactionalMeterReader;
        private IService _transactionCoordinator;
        private IService _transactionHistory;

        /// <inheritdoc />
        protected override void OnInitialize()
        {
        }

        /// <inheritdoc />
        protected override void OnRun()
        {
            Logger.Debug("Start of OnRun().");

            LoadPropertyProvider();

            LoadTransactionCoordinator();

            LoadBank();

            LoadTransactionHistory();

            LoadServices();

            RegisterLogAdapters();

            LoadRunnables();

            RunBootExtender();

            UnLoadLayer();
            Thread.Sleep(500);
            Logger.Debug("End of OnRun().");
        }

        /// <inheritdoc />
        protected override void OnStop()
        {
            Logger.Debug("Stopping");

            _extender?.Stop();
        }

        private static void WritePendingActionToMessageDisplay(string resourceStringName)
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

        private static void LoadPropertyProvider()
        {
            var nodes = MonoAddinsHelper.GetSelectedNodes<TypeExtensionNode>(PropertyProvidersPath);
            foreach (var node in nodes)
            {
                ServiceManager.GetInstance().GetService<IPropertiesManager>()
                    .AddPropertyProvider((IPropertyProvider)node.CreateInstance());
            }
        }

        private void LoadTransactionCoordinator()
        {
            WritePendingActionToMessageDisplay("CreatingTransactionCoordinator");
            var typeExtensionNode =
                MonoAddinsHelper.GetSingleSelectedExtensionNode<TypeExtensionNode>(TransactionCoordinatorExtensionPath);
            _transactionCoordinator = (IService)typeExtensionNode.CreateInstance();
            _transactionCoordinator.Initialize();
            ServiceManager.GetInstance().AddService(_transactionCoordinator);
        }

        private void LoadBank()
        {
            WritePendingActionToMessageDisplay("CreatingBank");
            var typeExtensionNode =
                MonoAddinsHelper.GetSingleSelectedExtensionNode<TypeExtensionNode>(BankExtensionPath);
            _bank = (IService)typeExtensionNode.CreateInstance();
            _bank.Initialize();
            ServiceManager.GetInstance().AddService(_bank);
        }

        private void LoadServices()
        {
            WritePendingActionToMessageDisplay("LoadingAccountingServices");
            var nodes = MonoAddinsHelper.GetSelectedNodes<TypeExtensionNode>(ServicesExtensionPath);
            foreach (var node in nodes)
            {
                var service = (IService)node.CreateInstance();
                service.Initialize();
                ServiceManager.GetInstance().AddService(service);
                _services.Add(service);
            }
        }

        private void LoadTransactionHistory()
        {
            WritePendingActionToMessageDisplay("CreatingTransactionHistory");
            var typeExtensionNode =
                MonoAddinsHelper.GetSingleSelectedExtensionNode<TypeExtensionNode>(TransactionHistoryExtensionPath);
            _transactionHistory = (IService)typeExtensionNode.CreateInstance();
            _transactionHistory.Initialize();
            ServiceManager.GetInstance().AddService(_transactionHistory);
        }

        private void LoadRunnables()
        {
            WritePendingActionToMessageDisplay("LoadingAccountingRunnables");
            _runnablesManager.StartRunnables(RunnablesExtensionPath);
        }

        private void RunBootExtender()
        {
            var typeExtensionNode = MonoAddinsHelper.GetSelectedNode<TypeExtensionNode>(ExtenderExtensionPath);
            _extender = (IRunnable)typeExtensionNode.CreateInstance();
            _extender.Initialize();
            if (RunState == RunnableState.Running)
            {
                _extender.Run();
            }

            _extender = null;
        }

        private void RegisterLogAdapters()
        {
            var logAdapterService = ServiceManager.GetInstance().GetService<ILogAdaptersService>();
            logAdapterService.RegisterLogAdapter(new BillEventLogAdapter());
            logAdapterService.RegisterLogAdapter(new HandpayEventLogAdapter());
            logAdapterService.RegisterLogAdapter(new TransferInEventLogAdapter());
            logAdapterService.RegisterLogAdapter(new TransferOutEventLogAdapter());
            logAdapterService.RegisterLogAdapter(new VoucherInEventLogAdapter());
            logAdapterService.RegisterLogAdapter(new VoucherOutEventLogAdapter());
            logAdapterService.RegisterLogAdapter(new KeyedCreditsEventLogAdapter());
            logAdapterService.RegisterLogAdapter(new HardMeterOutEventLogAdapter());
        }

        private void UnRegisterLogAdapters()
        {
            var logAdapterService = ServiceManager.GetInstance().GetService<ILogAdaptersService>();
            logAdapterService.UnRegisterLogAdapter(EventLogType.BillIn.GetDescription(typeof(EventLogType)));
            logAdapterService.UnRegisterLogAdapter(EventLogType.Handpay.GetDescription(typeof(EventLogType)));
            logAdapterService.UnRegisterLogAdapter(EventLogType.TransferIn.GetDescription(typeof(EventLogType)));
            logAdapterService.UnRegisterLogAdapter(EventLogType.TransferOut.GetDescription(typeof(EventLogType)));
            logAdapterService.UnRegisterLogAdapter(EventLogType.VoucherIn.GetDescription(typeof(EventLogType)));
            logAdapterService.UnRegisterLogAdapter(EventLogType.VoucherOut.GetDescription(typeof(EventLogType)));
            logAdapterService.UnRegisterLogAdapter(EventLogType.KeyedCredit.GetDescription(typeof(EventLogType)));
            logAdapterService.UnRegisterLogAdapter(EventLogType.HardMeterOut.GetDescription(typeof(EventLogType)));
        }

        private void UnLoadLayer()
        {
            UnRegisterLogAdapters();

            WritePendingActionToMessageDisplay("UnloadingAccountingRunnables");
            _runnablesManager.StopRunnables();

            WritePendingActionToMessageDisplay("UnloadingAccountingServices");
            foreach (var service in _services)
            {
                ServiceManager.GetInstance().RemoveService(service);
            }

            _services.Clear();

            if (_transactionHistory != null)
            {
                WritePendingActionToMessageDisplay("UnloadingTransactionHistory");
                ServiceManager.GetInstance().RemoveService(_transactionHistory);
                _transactionHistory = null;
            }

            WritePendingActionToMessageDisplay("UnloadingTransactionHandlers");
            _transactionHandlersManager.StopRunnables();

            if (_bank != null)
            {
                WritePendingActionToMessageDisplay("UnloadingBank");
                ServiceManager.GetInstance().RemoveService(_bank);
                _bank = null;
            }

            if (_transactionalMeterReader != null)
            {
                WritePendingActionToMessageDisplay("UnloadingTransactionalMeterReader");
                ServiceManager.GetInstance().RemoveService(_transactionalMeterReader);
                _transactionalMeterReader = null;
            }

            if (_transactionCoordinator != null)
            {
                WritePendingActionToMessageDisplay("UnloadingTransactionCoordinator");
                ServiceManager.GetInstance().RemoveService(_transactionCoordinator);
                _transactionCoordinator = null;
            }
        }
    }
}
