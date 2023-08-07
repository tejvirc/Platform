namespace Aristocrat.Monaco.Sas.AftTransferProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Accounting.Contracts.Wat;
    using Application.Contracts;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.Events;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Hardware.Contracts.Ticket;
    using IPrinter = Hardware.Contracts.Printer.IPrinter;
    using Kernel;
    using log4net;

    /// <summary>Definition of the AftTransferProviderBase class.</summary>
    public abstract class AftTransferProviderBase : IService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IAutoPlayStatusProvider _autoPlayStatusProvider;

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the AftTransferProviderBase class.
        /// </summary>
        protected AftTransferProviderBase(
            IAftLockHandler aftLockHandler,
            ISasHost sasHost,
            ITime timeService,
            IPropertiesManager propertiesManager,
            ITransactionCoordinator transactionCoordinator,
            IAutoPlayStatusProvider autoPlayStatusProvider)
        {
            Printer = ServiceManager.GetInstance().TryGetService<IPrinter>();
            AftLockHandler = aftLockHandler;
            SasHost = sasHost;
            TimeService = timeService;
            PropertiesManager = propertiesManager;
            TransactionCoordinator = transactionCoordinator;
            AftLockHandler.OnLocked += OnLockAcquired;
            _autoPlayStatusProvider = autoPlayStatusProvider ?? throw new ArgumentNullException(nameof(autoPlayStatusProvider));
        }

        /// <inheritdoc />
        public string Name => ServiceTypes?.ElementAt(0).ToString();

        /// <inheritdoc />
        public abstract ICollection<Type> ServiceTypes { get; }

        /// <summary>Gets the SasHost.</summary>
        protected ISasHost SasHost { get; }

        /// <summary>Gets the time service</summary>
        protected ITime TimeService { get; }

        /// <summary>Gets the properties manager</summary>
        protected IPropertiesManager PropertiesManager { get; }

        /// <summary> Aft Lock handler </summary>
        protected IAftLockHandler AftLockHandler { get; }

        /// <summary> The Sas client </summary>
        protected ITransactionCoordinator TransactionCoordinator { get; }

        /// <summary>Gets the printer.</summary>
        protected IPrinter Printer { get; }

        /// <summary>Gets a value indicating whether Aft is disabled or not.</summary>
        protected bool IsAftDisabled => AftDisableConditions.None != AftState;

        /// <summary>Gets or sets the guid that is used to learn the transaction guid in case of transaction request getting queued.</summary>
        protected Guid TransactionRequestId { get; set; }

        /// <summary>Gets or sets the persisted transaction id.</summary>
        protected Guid TransactionId { get; set; }

        /// <summary>Gets or sets the information for the last Aft request</summary>
        protected AftData CurrentTransfer { get; set; }

        /// <summary>Initializes the service.</summary>
        public virtual void Initialize()
        {
            CurrentTransfer = new AftData();
            Logger.Debug("Initialized!");
        }

        /// <summary>Gets or sets the Aft state</summary>
        public AftDisableConditions AftState { get; set; }

        /// <summary> Gets or sets a value indicating whether a transaction is pending. </summary>
        public bool TransactionPending { get; set; }

        /// <summary> Gets or sets a value indicating whether a transfer-out is pending. </summary>
        public bool TransferOutPending { get; set; }

        /// <summary> Gets a value indicating whether it's waiting for a key-off. </summary>
        public virtual bool WaitingForKeyOff => false;

        /// <summary>whether this cash-out is requested from the gaming machine.</summary>
        internal virtual bool CashOutFromGamingMachineRequest { get; set; }

        /// <summary>whether hard cash out mode is activated.</summary>
        internal virtual bool HardCashOutMode => false;

        /// <summary>
        ///     Prints the ticket and updates the receipt status of the aft in progress.
        /// </summary>
        /// <param name="ticket">The ticket to print.</param>
        protected async Task PrintReceipt(Ticket ticket)
        {
            Logger.Debug("Setting receipt status to printing in progress.");
            SasHost.SetAftReceiptStatus(ReceiptStatus.ReceiptPrintingInProgress);
            var printed = Printer != null && Printer.CanPrint && (await Printer.Print(ticket));
#if !(RETAIL)
            if (printed)
            {
                var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
                eventBus?.Publish(new AftPrintReceiptEvent(ticket));
            }
#endif
            Logger.Debug($"Setting receipt status to printed = {printed}.");
            SasHost.SetAftReceiptStatus(printed ? ReceiptStatus.ReceiptPrinted : ReceiptStatus.NoReceiptRequested);
        }

        /// <summary> Called when the lockup is keyed off. </summary>
        public virtual void OnKeyedOff()
        {
        }

        /// <summary> Called when the aft state changes </summary>
        public virtual void OnStateChanged()
        {
        }

        /// <summary>Resets any state variables associated with an Aft cash-out request from host. </summary>
        internal virtual void ResetCashOutRequestState()
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary> Called when the system is disabled. </summary>
        public virtual void OnSystemDisabled()
        {
        }

        /// <summary>
        ///     Handles disposing resources
        /// </summary>
        /// <param name="disposing">Whether or not to dispose resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (!disposing)
            {
                AftLockHandler.OnLocked -= OnLockAcquired;
            }

            _disposed = true;
        }

        /// <summary>Puts the amounts in the specified AftData object into an IDictionary.</summary>
        /// <param name="data">The AftData object to read.</param>
        /// <returns>An IDictionary containing amounts in millicents by AccountType.</returns>
        protected static IDictionary<AccountType, ulong> GetTransferAmountsDictionary(AftData data)
        {
            IDictionary<AccountType, ulong> transferAmounts = new Dictionary<AccountType, ulong>();
            AddAmount(transferAmounts, AccountType.Cashable, (ulong)((long)data.CashableAmount).CentsToMillicents());
            AddAmount(transferAmounts, AccountType.NonCash, (ulong)((long)data.RestrictedAmount).CentsToMillicents());
            AddAmount(transferAmounts, AccountType.Promo, (ulong)((long)data.NonRestrictedAmount).CentsToMillicents());
            return transferAmounts;
        }

        /// <summary>Should be called by both on and off when a transfer is complete.</summary>
        /// <param name="data">AftData object to modify.</param>
        /// <param name="transaction">The IList to read.</param>
        /// <param name="logger">The derived type specific logger</param>
        protected void HandleTransferComplete(AftData data, ITransaction transaction, ILog logger)
        {
            PopulateAftDataAmounts(data, transaction);

            var transferTime = transaction.TransactionDateTime;
            var localTime = TimeService.GetLocationTime(transferTime);

            logger.Debug($"Setting last transfer time to: {transferTime:o} and receipt time to: {localTime:o}");

            // The Sas engine expects this to be in local time
            data.TransactionDateTime = localTime;

            //un-pause autoplay for player
            _autoPlayStatusProvider.UnpausePlayerAutoPlay();
        }

        /// <summary>Retrieves the transaction guid from AftLockHandler, if any.</summary>
        /// <returns>The transaction guid, or Guid.Empty if the AftLockHandler doesn't have one.</returns>
        protected Guid RetrieveTransactionIdFromLock()
        {
            TransactionId = AftLockHandler.RetrieveTransactionId();

            Logger.Debug($"Retrieved transaction ID {TransactionId} from IAftLockHandler");
            return TransactionId;
        }

        /// <summary>Releases the transaction guid, if any.</summary>
        protected void ReleaseTransactionId()
        {
            if (TransactionId != Guid.Empty)
            {
                Logger.Debug($"Releasing transaction ID: {TransactionId}.");
                TransactionCoordinator.ReleaseTransaction(TransactionId);
                TransactionId = Guid.Empty;
            }
            else
            {
                Logger.Warn("Trying to release an empty transaction!");
            }
        }

        /// <summary>Handles the acquired lock.</summary>
        protected abstract void HandleLockAcquired();

        /// <summary>This is called anytime an event or some condition causes Aft to be disabled.</summary>
        /// <param name="isEnabled">True if the Aft is enabled, false if disabled.</param>
        protected abstract void AftDisabledByEvent(bool isEnabled);

        /// <summary>Terminates the lock now that it has been consumed.</summary>
        protected void TerminateLock()
        {
            AftLockHandler.AftLock(false, 0);
            Logger.Debug("Aft lock is terminated!");
        }

        /// <summary>Puts the amounts specified in the IList into the specified AftData object.</summary>
        private static void PopulateAftDataAmounts(AftData data, ITransaction transaction)
        {
            data.CashableAmount = 0;
            data.RestrictedAmount = 0;
            data.NonRestrictedAmount = 0;

            switch (transaction)
            {
                case WatTransaction watOff:
                    if (watOff.TransferredCashableAmount > 0)
                    {
                        data.CashableAmount = (ulong)watOff.TransferredCashableAmount;
                    }
                    if (watOff.TransferredNonCashAmount > 0)
                    {
                        data.RestrictedAmount = (ulong)watOff.TransferredNonCashAmount;
                    }
                    if (watOff.TransferredPromoAmount > 0)
                    {
                        data.NonRestrictedAmount = (ulong)watOff.TransferredPromoAmount;
                    }
                    break;
                case WatOnTransaction watOn:
                    if (watOn.TransferredCashableAmount > 0)
                    {
                        data.CashableAmount = (ulong)watOn.TransferredCashableAmount;
                    }
                    if (watOn.TransferredNonCashAmount > 0)
                    {
                        data.RestrictedAmount = (ulong)watOn.TransferredNonCashAmount;
                    }
                    if (watOn.TransferredPromoAmount > 0)
                    {
                        data.NonRestrictedAmount = (ulong)watOn.TransferredPromoAmount;
                    }
                    break;
            }
        }

        /// <summary>Adds the specified amount and credit type to the specified dictionary.</summary>
        private static void AddAmount(
                IDictionary<AccountType, ulong> transferAmounts,
                AccountType accountType,
                ulong amount)
        {
            if (amount != 0)
            {
                transferAmounts[accountType] = amount;
            }
        }

        private void OnLockAcquired(object sender, EventArgs args)
        {
            HandleLockAcquired();
        }
    }
}

