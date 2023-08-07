namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Contracts;
    using Contracts.Wat;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Localization.Properties;
    using log4net;

    /// <summary>
    ///     Waits for transfer requests from a host and attempts to start a new transaction in
    ///     which it will work with the bank to deposit funds.
    /// </summary>
    [CLSCompliant(false)]
    public sealed class WatOnHandler : TransferInProviderBase, IWatTransferOnHandler
    {
        private const int WatOnDeviceId = 1;
        private const int TransactionRequestTimeout = 0;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private static readonly Guid WatOnRequestorGuid = new Guid("{0F7495DC-CC8E-4d8f-97C9-211B426FE83B}");

        private readonly IEventBus _eventBus;
        private readonly ITransactionCoordinator _transactionCoordinator;
        private readonly ITransactionHistory _transactionHistory;
        private readonly IPersistentStorageManager _persistentStorageManager;
        private readonly IMeterManager _meterManager;
        private readonly IMessageDisplay _messageDisplay;
        private readonly IBank _bank;
        private readonly IFundTransferProvider _fundTransferProvider;

        /// <inheritdoc />
        public string Name => typeof(IWatTransferOnHandler).ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IWatTransferOnHandler) };

        public WatOnHandler()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<ITransactionCoordinator>(),
                ServiceManager.GetInstance().GetService<ITransactionHistory>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IMeterManager>(),
                ServiceManager.GetInstance().GetService<IMessageDisplay>(),
                ServiceManager.GetInstance().GetService<IBank>(),
                ServiceManager.GetInstance().GetService<IFundTransferProvider>())
        {
        }

        public WatOnHandler(
            IEventBus eventBus,
            ITransactionCoordinator transactionCoordinator,
            ITransactionHistory transactionHistory,
            IPersistentStorageManager persistentStorageManager,
            IPropertiesManager propertiesManager,
            IMeterManager meterManager,
            IMessageDisplay messageDisplay,
            IBank bank,
            IFundTransferProvider fundTransferProvider)
            : base(bank, meterManager, propertiesManager)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _transactionCoordinator =
                transactionCoordinator ?? throw new ArgumentNullException(nameof(transactionCoordinator));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _persistentStorageManager = persistentStorageManager ??
                                        throw new ArgumentNullException(nameof(persistentStorageManager));
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _messageDisplay = messageDisplay ?? throw new ArgumentNullException(nameof(messageDisplay));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _fundTransferProvider = fundTransferProvider?? throw new ArgumentNullException(nameof(fundTransferProvider));
        }

        /// <inheritdoc />
        public void Initialize()
        {
            Recover();
        }

        /// <inheritdoc />
        public bool CancelTransfer(string requestId, int hostException)
        {
            var transaction = _transactionHistory.RecallTransactions<WatOnTransaction>()
                .LastOrDefault(t => t.RequestId == requestId);
            if (transaction == null)
            {
                Logger.Info($"Failed to locate a WAT transaction with requestId - {requestId}");
                return false;
            }

            return CancelTransfer(transaction, hostException);
        }

        /// <inheritdoc />
        public bool CancelTransfer(WatOnTransaction transaction, int hostException)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            var validator = _fundTransferProvider.GetWatTransferOnProvider();
            if (validator == null)
            {
                Logger.Info($"Failed to find a validator - {transaction}");
                return false;
            }

            Logger.Debug($"Attempting to cancel WAT transaction - {transaction}");

            var currentTransaction = _transactionHistory.RecallTransactions<WatOnTransaction>()
                .FirstOrDefault(
                    t => t.TransactionId == transaction.TransactionId &&
                         (t.Status == WatStatus.Initiated || t.Status == WatStatus.RequestReceived));

            if (currentTransaction == null)
            {
                Logger.Debug($"WAT transaction is unknown or has already been committed: {transaction}");
                return false;
            }

            currentTransaction.Status = WatStatus.CancelReceived;
            currentTransaction.CashableAmount = 0;
            currentTransaction.PromoAmount = 0;
            currentTransaction.NonCashAmount = 0;
            currentTransaction.HostException = hostException;
            _transactionHistory.UpdateTransaction(currentTransaction);

            Logger.Info($"WAT On transaction cancel requested: {currentTransaction}");

            Task.Run(() => Commit(validator, transaction));

            return true;
        }

        /// <inheritdoc />
        public bool RequestTransfer(
            Guid transactionId,
            string requestId,
            long cashable,
            long promo,
            long nonCashable,
            bool reduceAmount)
        {
            return RequestTransferInternal(transactionId, requestId, cashable, promo, nonCashable, reduceAmount);
        }

        /// <inheritdoc />
        public void AcknowledgeTransfer(WatOnTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            Logger.Debug($"Attempting to ack WAT transaction - {transaction}");

            var currentTransaction = _transactionHistory.RecallTransactions<WatOnTransaction>()
                .FirstOrDefault(t => t.TransactionId == transaction.TransactionId && t.Status == WatStatus.Committed);

            if (currentTransaction == null)
            {
                Logger.Debug($"WAT transaction is unknown or has already been committed: {transaction}");
                return;
            }

            currentTransaction.Status = WatStatus.Complete;
            _transactionHistory.UpdateTransaction(currentTransaction);

            Logger.Debug($"WAT transaction has been acknowledged: {currentTransaction}");
        }

        /// <inheritdoc />
        public bool CanRecover(Guid transactionId)
        {
            return false;
        }

        /// <inheritdoc />
        public Task<bool> Recover(Guid transactionId, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        protected override void Recover()
        {
            var transaction = _transactionHistory.RecallTransactions<WatOnTransaction>()
                .SingleOrDefault(t => t.Status != WatStatus.Committed && t.Status != WatStatus.Complete);

            if (transaction == null)
            {
                return;
            }

            Logger.Info($"Canceling the WAT On transaction={transaction} due to power on failure");

            transaction.Status = WatStatus.Committed;
            transaction.EgmException = (int)WatExceptionCode.PowerFailure;
            transaction.TransferredCashableAmount = 0;
            transaction.TransferredNonCashAmount = 0;
            transaction.TransferredPromoAmount = 0;
            _transactionHistory.UpdateTransaction(transaction);
        }

        private bool RequestTransferInternal(
            Guid transactionId,
            string requestId,
            long cashable,
            long promo,
            long nonCashable,
            bool reduceAmount)
        {
            if (string.IsNullOrEmpty(requestId))
            {
                Logger.Info("Requests must include a valid request Id");
                return false;
            }

            var validator = _fundTransferProvider.GetWatTransferOnProvider();
            if (validator == null || !validator.CanTransfer)
            {
                Logger.Info($"No validator or validation is currently not allowed - {requestId}");
                return false;
            }

            var createdTransaction = false;

            if (transactionId == Guid.Empty)
            {
                transactionId = _transactionCoordinator.RequestTransaction(
                    WatOnRequestorGuid,
                    TransactionRequestTimeout,
                    TransactionType.Write,
                    true);
                if (transactionId == Guid.Empty)
                {
                    Logger.Warn($"Failed to allocate a transaction - {requestId}");
                    return false;
                }

                createdTransaction = true;
            }

            // This needs to be allocated before we initiate the transfer. This does, however, mean we must complete the transaction (commit, cancel, abort, etc.)
            var transaction = new WatOnTransaction(
                WatOnDeviceId,
                DateTime.UtcNow,
                cashable,
                promo,
                nonCashable,
                reduceAmount,
                requestId)
            {
                Status = WatStatus.RequestReceived,
                BankTransactionId = transactionId,
                OwnsBankTransaction = createdTransaction
            };

            using (var scope = _persistentStorageManager.ScopedTransaction())
            {
                _transactionHistory.AddTransaction(transaction);
                scope.Complete();
            }

            Task.Run(
                async () =>
                {
                    if (await InitiateTransfer(validator, transaction) &&
                        await Commit(validator, transaction) &&
                        transaction.TransactionAmount > 0)
                    {
                        foreach (var displayMessage in WatExtensions.GetWatTransferMessage(
                            ResourceKeys.TransferedInComplete,
                            transaction.TransferredCashableAmount,
                            transaction.TransferredPromoAmount,
                            transaction.TransferredNonCashAmount))
                        {
                            _messageDisplay.DisplayMessage(
                                new DisplayableMessage(
                                    () => displayMessage,
                                    DisplayableMessageClassification.Informative,
                                    DisplayableMessagePriority.Normal,
                                    typeof(WatOnCompleteEvent)));
                        }
                    }
                });

            return true;
        }

        private async Task<bool> InitiateTransfer(IWatTransferOnProvider validator, WatOnTransaction transaction)
        {
            _eventBus.Publish(new WatOnStartedEvent());

            if (!await validator.InitiateTransfer(transaction))
            {
                // NOTE: The validator is responsible for setting any applicable exception codes
                transaction.AuthorizedCashableAmount = 0;
                transaction.AuthorizedPromoAmount = 0;
                transaction.AuthorizedNonCashAmount = 0;
                transaction.TransferredCashableAmount = 0;
                transaction.TransferredPromoAmount = 0;
                transaction.TransferredNonCashAmount = 0;
                transaction.Status = WatStatus.Complete;

                _transactionHistory.UpdateTransaction(transaction);
                _eventBus.Publish(new WatOnCompleteEvent(transaction));

                Logger.Warn($"Failed to initiate transfer - {transaction}");

                return await Task.FromResult(false);
            }

            transaction.Status = WatStatus.Authorized;
            _transactionHistory.UpdateTransaction(transaction);

            Logger.Warn($"Authorized WAT transfer - {transaction}");

            return await Task.FromResult(true);
        }

        private async Task<bool> Commit(IWatTransferOnProvider validator, WatOnTransaction transaction)
        {
            if (CheckMaxCreditMeter(transaction.AuthorizedAmount))
            {
                await CommitTransfer(transaction);
            }
            else
            {
                Logger.Debug($"Unable to transfer any amount due to the credit limit being reached - {transaction}");

                transaction.TransferredCashableAmount = 0;
                transaction.TransferredNonCashAmount = 0;
                transaction.TransferredPromoAmount = 0;
                transaction.Status = WatStatus.Committed;
                _transactionHistory.UpdateTransaction(transaction);
            }

            await validator.CommitTransfer(transaction);

            Logger.Info($"Committed WAT transfer - {transaction}");

            _eventBus.Publish(new WatOnCompleteEvent(transaction));

            return true;
        }

        private Task CommitTransfer(WatOnTransaction transaction)
        {
            using (var scope = _persistentStorageManager.ScopedTransaction())
            {
                // The host may deny the transfer by setting all of the values to zero, but the behavior is the same (mark the transaction as complete)
                if (transaction.AuthorizedNonCashAmount > 0)
                {
                    transaction.TransferredNonCashAmount = transaction.AuthorizedNonCashAmount;

                    _bank.Deposit(AccountType.NonCash, transaction.TransferredNonCashAmount, transaction.BankTransactionId);
                    _meterManager.GetMeter(AccountingMeters.WatOnNonCashableAmount).Increment(transaction.TransferredNonCashAmount);
                    _meterManager.GetMeter(AccountingMeters.WatOnNonCashableCount).Increment(1);
                }

                if (transaction.AuthorizedPromoAmount > 0)
                {
                    transaction.TransferredPromoAmount = transaction.AuthorizedPromoAmount;

                    _bank.Deposit(AccountType.Promo, transaction.TransferredPromoAmount, transaction.BankTransactionId);
                    _meterManager.GetMeter(AccountingMeters.WatOnCashablePromoAmount).Increment(transaction.TransferredPromoAmount);
                    _meterManager.GetMeter(AccountingMeters.WatOnCashablePromoCount).Increment(1);
                }

                if (transaction.CashableAmount > 0)
                {
                    transaction.TransferredCashableAmount = transaction.AuthorizedCashableAmount;

                    _bank.Deposit(AccountType.Cashable, transaction.TransferredCashableAmount, transaction.BankTransactionId);
                    _meterManager.GetMeter(AccountingMeters.WatOnCashableAmount).Increment(transaction.TransferredCashableAmount);
                    _meterManager.GetMeter(AccountingMeters.WatOnCashableCount).Increment(1);
                }

                transaction.Status = WatStatus.Committed;
                _transactionHistory.UpdateTransaction(transaction);
                if (transaction.OwnsBankTransaction)
                {
                    _transactionCoordinator.ReleaseTransaction(transaction.BankTransactionId);
                }

                scope.Complete();
            }

            return Task.FromResult(true);
        }
    }
}