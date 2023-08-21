namespace Aristocrat.Monaco.Accounting.Wat
{
    using Application.Contracts;
    using Contracts;
    using Contracts.TransferOut;
    using Contracts.Wat;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Transactions;
    using Localization.Properties;

    /// <summary>
    ///     Handles wager account transfer (WAT) transactions
    /// </summary>
    [CLSCompliant(false)]
    public class WatOffProvider : TransferOutProviderBase, IWatOffProvider, IDisposable
    {
        private const int DeviceId = 1;
        private const int TransactionRequestTimeout = 0;

        private static readonly Guid RequestorId = new Guid("{C25C2B10-8290-40CA-A6DD-528CE4CD4A15}");

        private readonly IBank _bank;
        private readonly IEventBus _bus;
        private readonly IMeterManager _meters;
        private readonly IPersistentStorageManager _storage;
        private readonly ITransactionCoordinator _transactionCoordinator;
        private readonly IMessageDisplay _messageDisplay;
        private readonly ITransactionHistory _transactions;
        private readonly IFundTransferProvider _fundTransferProvider;
        private bool _disposed;

        public WatOffProvider()
            : this(
                ServiceManager.GetInstance().GetService<IBank>(),
                ServiceManager.GetInstance().GetService<ITransactionHistory>(),
                ServiceManager.GetInstance().GetService<IMeterManager>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<ITransactionCoordinator>(),
                ServiceManager.GetInstance().GetService<IMessageDisplay>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IFundTransferProvider>())
        {
        }

        public WatOffProvider(
            IBank bank,
            ITransactionHistory transactions,
            IMeterManager meters,
            IPersistentStorageManager storage,
            ITransactionCoordinator transactionCoordinator,
            IMessageDisplay messageDisplay,
            IEventBus bus,
            IFundTransferProvider fundTransferProvider)
        {
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _transactionCoordinator =
                transactionCoordinator ?? throw new ArgumentNullException(nameof(transactionCoordinator));
            _messageDisplay = messageDisplay ?? throw new ArgumentNullException(nameof(messageDisplay));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _fundTransferProvider = fundTransferProvider ?? throw new ArgumentNullException(nameof(fundTransferProvider));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<TransferResult> Transfer(
            Guid transactionId,
            long cashable,
            long promo,
            long nonCashable,
            IReadOnlyCollection<long> associatedTransactions,
            TransferOutReason reason,
            Guid traceId,
            CancellationToken cancellationToken)
        {
            var validator = _fundTransferProvider.GetWatTransferOffProvider();
            if (validator == null || !validator.CanTransfer)
            {
                Logger.Info($"No validator or validation is currently not allowed - {transactionId}");
                return TransferResult.Failed;
            }

            var transaction = new WatTransaction(
                DeviceId,
                DateTime.UtcNow,
                cashable,
                promo,
                nonCashable,
                false,
                string.Empty,
                reason)
            {
                Status = WatStatus.Initiated,
                BankTransactionId = transactionId,
                AssociatedTransactions = associatedTransactions,
                TraceId = traceId
            };

            // This needs to be allocated before we initiate the transfer. This does, however, mean we must complete the transaction (commit, cancel, abort, etc.)
            _transactions.AddTransaction(transaction);

            return await InitiateTransferAndCommit(validator, transaction);
        }

        public bool CanRecover(Guid transactionId)
        {
            return false;
        }

        public Task<bool> Recover(IRecoveryTransaction transaction, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public string Name => nameof(WatOffProvider);

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IWatOffProvider) };

        /// <inheritdoc />
        public bool Active { get; private set; }

        /// <inheritdoc />
        public void Initialize()
        {
            RecoverInternal();
        }

        /// <inheritdoc />
        public bool RequestTransfer(
            string requestId,
            long cashable,
            long promo,
            long nonCashable,
            bool reduceAmount)
        {
            return RequestTransferInternal(Guid.Empty, requestId, cashable, promo, nonCashable, reduceAmount);
        }

        public bool RequestTransfer(
            Guid transactionId,
            string requestId,
            long cashable,
            long promo,
            long nonCashable,
            bool reduceAmount)
        {
            if (!_transactionCoordinator.VerifyCurrentTransaction(transactionId))
            {
                Logger.Info("Requests must include a valid transaction Id");
                return false;
            }

            return RequestTransferInternal(transactionId, requestId, cashable, promo, nonCashable, reduceAmount);
        }

        /// <inheritdoc />
        public bool CancelTransfer(string requestId, int hostException)
        {
            var transaction = _transactions.RecallTransactions<WatTransaction>()
                .LastOrDefault(t => t.RequestId == requestId);
            if (transaction == null)
            {
                Logger.Info($"Failed to locate a WAT transaction with requestId - {requestId}");
                return false;
            }

            return CancelTransfer(transaction, hostException);
        }

        /// <inheritdoc />
        public bool CancelTransfer(WatTransaction transaction, int hostException)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            var validator = _fundTransferProvider.GetWatTransferOffProvider(true);
            if (validator == null)
            {
                Logger.Info($"Failed to find a validator - {transaction}");
                return false;
            }

            Logger.Debug($"Attempting to ack WAT transaction - {transaction}");

            var currentTransaction = _transactions.RecallTransactions<WatTransaction>()
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
            _transactions.UpdateTransaction(currentTransaction);

            _bus.Publish(new WatTransferCancelRequestedEvent(currentTransaction));

            Logger.Info($"WAT transaction cancel requested: {currentTransaction}");

            Task.Run(() => Commit(validator, transaction));

            return true;
        }

        /// <inheritdoc />
        public void AcknowledgeTransfer(string requestId)
        {
            var transaction = _transactions.RecallTransactions<WatTransaction>()
                .LastOrDefault(t => t.RequestId == requestId);
            if (transaction == null)
            {
                Logger.Info($"Failed to locate a WAT transaction with requestId - {requestId}");
                return;
            }

            AcknowledgeTransfer(transaction);
        }

        /// <inheritdoc />
        public void AcknowledgeTransfer(WatTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            Logger.Debug($"Attempting to ack WAT transaction - {transaction}");

            var currentTransaction = _transactions.RecallTransactions<WatTransaction>()
                .FirstOrDefault(t => t.TransactionId == transaction.TransactionId && t.Status == WatStatus.Committed);

            if (currentTransaction == null)
            {
                Logger.Debug($"WAT transaction is unknown or has already been committed: {transaction}");
                return;
            }

            currentTransaction.Status = WatStatus.Complete;
            _transactions.UpdateTransaction(currentTransaction);

            _bus.Publish(new WatTransferCompletedEvent(currentTransaction));

            Logger.Debug($"WAT transaction has been acknowledged: {currentTransaction}");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _bus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void RecoverInternal()
        {
            var transaction = _transactions.RecallTransactions<WatTransaction>()
                .SingleOrDefault(
                    t => t.Status != WatStatus.Committed &&
                         t.Status != WatStatus.Complete &&
                         t.Status != WatStatus.Rejected);

            if (transaction == null)
            {
                return;
            }

            Logger.Info($"Canceling the WAT Off transaction={transaction} due to power on failure");

            transaction.Status = WatStatus.Committed;
            transaction.EgmException = (int)WatExceptionCode.PowerFailure;
            transaction.TransferredCashableAmount = 0;
            transaction.TransferredNonCashAmount = 0;
            transaction.TransferredPromoAmount = 0;
            _transactions.UpdateTransaction(transaction);
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

            var validator = _fundTransferProvider.GetWatTransferOffProvider(true);
            if (validator == null || !validator.CanTransfer)
            {
                Logger.Info($"No validator or validation is currently not allowed - {requestId}");
                return false;
            }

            var createdTransaction = false;

            if (transactionId == Guid.Empty)
            {
                transactionId = _transactionCoordinator.RequestTransaction(
                    RequestorId,
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
            var transaction = new WatTransaction(
                DeviceId,
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
            _transactions.AddTransaction(transaction);

            //** Not sure I like this, but we need to return back to the caller before continuing
            Task.Run(
                async () =>
                {
                    var transferResult = await InitiateTransferAndCommit(validator, transaction);
                    if (transferResult.Success)
                    {
                        foreach (var displayMessage in WatExtensions.GetWatTransferMessage(
                            ResourceKeys.TransferedOutComplete,
                            transaction.TransferredCashableAmount,
                            transaction.TransferredPromoAmount,
                            transaction.TransferredNonCashAmount))
                        {
                            _messageDisplay.DisplayMessage(
                                new DisplayableMessage(
                                    () => displayMessage,
                                    DisplayableMessageClassification.Informative,
                                    DisplayableMessagePriority.Normal,
                                    typeof(WatTransferCommittedEvent)));
                        }
                    }
                });

            return true;
        }

        private async Task<TransferResult> InitiateTransferAndCommit(IWatTransferOffProvider validator, WatTransaction transaction)
        {
            try
            {
                Active = true;

                _bus.Publish(new WatTransferInitiatedEvent(transaction));

                if (!await validator.InitiateTransfer(transaction))
                {
                    // NOTE: The validator is responsible for setting any applicable exception codes
                    transaction.AuthorizedCashableAmount = 0;
                    transaction.AuthorizedPromoAmount = 0;
                    transaction.AuthorizedNonCashAmount = 0;
                    transaction.Status = WatStatus.Rejected;

                    _transactions.UpdateTransaction(transaction);

                    _bus.Publish(new WatTransferCompletedEvent(transaction));

                    Logger.Warn($"Failed to initiate transfer - {transaction}");

                    return TransferResult.Failed;
                }

                transaction.Status = WatStatus.Authorized;
                _transactions.UpdateTransaction(transaction);

                _bus.Publish(new WatTransferAuthorizedEvent(transaction));

                Logger.Warn($"Authorized WAT transfer - {transaction}");
                return await Commit(validator, transaction);
            }
            finally
            {
                Active = false;
            }
        }

        private static bool AffectsBalance(TransferOutReason reason)
        {
            return reason != TransferOutReason.BonusPay &&
                   reason != TransferOutReason.LargeWin &&
                   reason != TransferOutReason.CashWin;
        }

        private async Task<TransferResult> Commit(IWatTransferOffProvider validator, WatTransaction transaction)
        {
            using (var scope = _storage.ScopedTransaction())
            {
                // The host may deny the transfer by setting all of the values to zero, but the behavior is the same (mark the transaction as complete)

                if (transaction.AuthorizedCashableAmount > 0)
                {
                    if (AffectsBalance(transaction.Reason))
                    {
                        _bank.Withdraw(
                            AccountType.Cashable,
                            transaction.AuthorizedCashableAmount,
                            transaction.BankTransactionId);
                    }

                    transaction.TransferredCashableAmount = transaction.AuthorizedCashableAmount;

                    _meters.GetMeter(AccountingMeters.WatOffCashableAmount)
                        .Increment(transaction.AuthorizedCashableAmount);
                    _meters.GetMeter(AccountingMeters.WatOffCashableCount).Increment(1);
                }

                if (transaction.AuthorizedPromoAmount > 0)
                {
                    if (AffectsBalance(transaction.Reason))
                    {
                        _bank.Withdraw(
                            AccountType.Promo,
                            transaction.AuthorizedPromoAmount,
                            transaction.BankTransactionId);
                    }

                    transaction.TransferredPromoAmount = transaction.AuthorizedPromoAmount;

                    _meters.GetMeter(AccountingMeters.WatOffCashablePromoAmount)
                        .Increment(transaction.AuthorizedPromoAmount);
                    _meters.GetMeter(AccountingMeters.WatOffCashablePromoCount).Increment(1);
                }

                if (transaction.AuthorizedNonCashAmount > 0)
                {
                    if (AffectsBalance(transaction.Reason))
                    {
                        _bank.Withdraw(
                            AccountType.NonCash,
                            transaction.AuthorizedNonCashAmount,
                            transaction.BankTransactionId);
                    }

                    transaction.TransferredNonCashAmount = transaction.AuthorizedNonCashAmount;

                    _meters.GetMeter(AccountingMeters.WatOffNonCashableAmount)
                        .Increment(transaction.AuthorizedNonCashAmount);
                    _meters.GetMeter(AccountingMeters.WatOffNonCashableCount).Increment(1);
                }

                transaction.Status = WatStatus.Committed;
                _transactions.UpdateTransaction(transaction);

                // The only time this class creates a transaction is when the transfer is host initiated, so in that case we need to release the transaction
                //  The alternative would be to add an explicit attribute to the transaction
                if (transaction.OwnsBankTransaction)
                {
                    _transactionCoordinator.ReleaseTransaction(transaction.BankTransactionId);
                }

                scope.Complete();
            }

            await validator.CommitTransfer(transaction);

            Logger.Warn($"Committed WAT transfer - {transaction}");

            _bus.Publish(new WatTransferCommittedEvent(transaction));

            return new TransferResult(
                transaction.TransferredCashableAmount,
                transaction.TransferredPromoAmount,
                transaction.TransferredNonCashAmount);
        }
    }
}