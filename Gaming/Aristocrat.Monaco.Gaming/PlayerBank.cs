namespace Aristocrat.Monaco.Gaming
{
    using Accounting.Contracts;
    using Application.Contracts;
    using Contracts;
    using Contracts.Session;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.TransferOut;
    using Accounting.Contracts.Vouchers;

    /// <summary>
    ///     An <see cref="IPlayerBank" /> implementation.
    /// </summary>
    public class PlayerBank : IPlayerBank, ITransactionRequestor, IService, IDisposable
    {
        private const long NoAssociatedTransactionId = -1;

        private const string CurrentTransactionId = "CurrentTransactionId";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly Guid RequestorId = new Guid("{6C508FA1-226D-49B2-B359-592C01139EAB}");

        public Guid RequestorGuid { get; } = RequestorId;

        private Guid _requestId;

        private static readonly IEnumerable<AccountType> AccountTypes = new List<AccountType>
        {
            AccountType.NonCash,
            AccountType.Promo,
            AccountType.Cashable
        };

        private readonly AutoResetEvent _waitForTransaction = new AutoResetEvent(false);

        private readonly IEventBus _bus;
        private readonly IMeterManager _meters;
        private readonly IPersistentStorageAccessor _persistentStorageAccessor;
        private readonly IPlayerService _players;
        private readonly ITransactionCoordinator _transactionCoordinator;
        private readonly ITransferOutHandler _transferOut;
        private readonly IBank _bank;
        private readonly TimeSpan _lockTimeout;
        private readonly IGameHistory _history;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerBank" /> class.
        /// </summary>
        /// <param name="bank">An <see cref="IBank" /> implementation.</param>
        /// <param name="transactionCoordinator">An <see cref="ITransactionCoordinator" /> implementation.</param>
        /// <param name="transferOut">An <see cref="ITransferOutHandler" /> instance.</param>
        /// <param name="persistentStorage">An <see cref="IPersistentStorageManager" /> instance.</param>
        /// <param name="meters">An <see cref="IMeterManager" /> instance</param>
        /// <param name="players">An <see cref="IPlayerService" /> instance</param>
        /// <param name="bus">An <see cref="IEventBus" /> instance.</param>
        /// <param name="history">An <see cref="IGameHistory"/> instance.</param>
        public PlayerBank(
            IBank bank,
            ITransactionCoordinator transactionCoordinator,
            ITransferOutHandler transferOut,
            IPersistentStorageManager persistentStorage,
            IMeterManager meters,
            IPlayerService players,
            IEventBus bus,
            IGameHistory history)
        {
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _transactionCoordinator =
                transactionCoordinator ?? throw new ArgumentNullException(nameof(transactionCoordinator));
            _transferOut = transferOut ?? throw new ArgumentNullException(nameof(transferOut));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _players = players ?? throw new ArgumentNullException(nameof(players));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _history = history ?? throw new ArgumentNullException(nameof(history));

            var storageName = GetType().ToString();

            var blockExists = persistentStorage.BlockExists(storageName);

            _persistentStorageAccessor = blockExists
                ? persistentStorage.GetBlock(storageName)
                : persistentStorage.CreateBlock(PersistenceLevel.Transient, storageName, 1);

            _lockTimeout = TimeSpan.FromSeconds(5);

            if (blockExists)
            {
                TransactionId = (Guid)_persistentStorageAccessor[CurrentTransactionId];

                if (TransactionId != Guid.Empty &&
                    (!_transactionCoordinator.VerifyCurrentTransaction(TransactionId) || !_history.IsRecoveryNeeded))
                {
                    _transactionCoordinator.AbandonTransactions(RequestorId);
                    _persistentStorageAccessor[CurrentTransactionId] = TransactionId = Guid.Empty;
                }
            }
        }

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_waitForTransaction != null)
                {
                    _waitForTransaction.Dispose();
                }
            }

            _disposed = true;
        }

        public void NotifyTransactionReady(Guid requestId)
        {
            _requestId = requestId;

            if (_requestId == Guid.Empty)
            {
                return;
            }

            _waitForTransaction.Set();
        }

        private bool ActiveTransaction => TransactionId != Guid.Empty;

        /// <inheritdoc />
        public long Balance => _bank.QueryBalance();

        /// <inheritdoc />
        public long Credits => Balance / GamingConstants.Millicents;

        /// <inheritdoc />
        public Guid TransactionId { get; private set; }

        /// <inheritdoc />
        public bool CashOut()
        {
            _bus.Publish(new CashOutStartedEvent(false, true));

            var success = _transferOut.TransferOut();

            if (!success)
            {
                _bus.Publish(new CashOutAbortedEvent());
            }

            return success;
        }

        /// <inheritdoc />
        public bool CashOut(bool forcedCashout)
        {
            return CashOut(Guid.Empty, forcedCashout, NoAssociatedTransactionId);
        }

        /// <inheritdoc />
        public bool CashOut(long amount, bool forcedCashout = false)
        {
            return CashOut(Guid.Empty, amount, TransferOutReason.CashOut, forcedCashout, NoAssociatedTransactionId);
        }

        /// <inheritdoc />
        public bool CashOut(Guid traceId, bool forcedCashout, long associatedTransaction)
        {
            _bus.Publish(new CashOutStartedEvent(forcedCashout, true));

            var associated = associatedTransaction == NoAssociatedTransactionId
                ? Enumerable.Empty<long>().ToArray()
                : new[] { associatedTransaction };

            var success = ActiveTransaction
                ? _transferOut.TransferOut(TransactionId, associated, TransferOutReason.CashOut, traceId)
                : _transferOut.TransferOut();

            if (!success)
            {
                _bus.Publish(new CashOutAbortedEvent());
            }

            return success;
        }

        /// <inheritdoc />
        public bool CashOut(
            Guid traceId,
            long amount,
            TransferOutReason reason,
            bool forcedCashout,
            long associatedTransaction,
            bool forcedByMaxWin = false)
        {
            _bus.Publish(
                new CashOutStartedEvent(
                    forcedCashout,
                    reason == TransferOutReason.CashWin ? Balance == 0 : Balance == amount,
                    forcedByMaxWin));

            var associated = associatedTransaction == NoAssociatedTransactionId
                ? Enumerable.Empty<long>().ToArray()
                : new[] { associatedTransaction };

            var success = ActiveTransaction
                ? _transferOut.TransferOut(TransactionId, AccountType.Cashable, amount, associated, reason, traceId)
                : _transferOut.TransferOut(AccountType.Cashable, amount, reason);

            if (!success)
            {
                _bus.Publish(new CashOutAbortedEvent());
            }

            return success;
        }

        /// <inheritdoc />
        public bool ForceHandpay(Guid traceId, long amount, TransferOutReason reason, long associatedTransaction)
        {
            _bus.Publish(new CashOutStartedEvent(false, Balance == amount));

            var associated = associatedTransaction == NoAssociatedTransactionId
                ? Enumerable.Empty<long>().ToArray()
                : new[] { associatedTransaction };

            var success = ActiveTransaction
                ? _transferOut.TransferOut<IHandpayProvider>(TransactionId, AccountType.Cashable, amount, associated, reason, traceId)
                : _transferOut.TransferOut<IHandpayProvider>(AccountType.Cashable, amount, reason);

            if (!success)
            {
                _bus.Publish(new CashOutAbortedEvent());
            }

            return success;
        }

        /// <inheritdoc />
        public bool ForceVoucherOut(Guid traceId, long amount, TransferOutReason reason, long associatedTransaction)
        {
            _bus.Publish(new CashOutStartedEvent(false, Balance == amount));

            var associated = associatedTransaction == NoAssociatedTransactionId
                ? Enumerable.Empty<long>().ToArray()
                : new[] { associatedTransaction };

            return ActiveTransaction
                ? _transferOut.TransferOut<IVoucherOutProvider>(TransactionId, AccountType.Cashable, amount, associated, reason, traceId)
                : _transferOut.TransferOut<IVoucherOutProvider>(AccountType.Cashable, amount, reason);
        }

        /// <inheritdoc />
        public void WaitForLock()
        {
            if (TransactionId != Guid.Empty)
            {
                return;
            }

            Logger.Debug("Starting a player bank transaction, will wait if necessary");

            var transactionId = _transactionCoordinator.RequestTransaction(this, TransactionType.Write);

            if (transactionId != Guid.Empty)
            {
                TransactionId = transactionId;

                Logger.Debug($"Player bank transaction {TransactionId} created");

                _persistentStorageAccessor[CurrentTransactionId] = TransactionId;

                return;
            }

            // If another transaction is occurring right now, wait until it completes so this one
            // can proceed
            _waitForTransaction.WaitOne();

            TransactionId = _transactionCoordinator.RetrieveTransaction(_requestId);

            Logger.Debug($"Player bank transaction {TransactionId} created");

            _persistentStorageAccessor[CurrentTransactionId] = TransactionId;

            _requestId = Guid.Empty;
        }

        /// <inheritdoc />
        public bool Lock()
        {
            return Lock(_lockTimeout);
        }

        /// <inheritdoc />
        public bool Lock(TimeSpan timeout)
        {
            if (TransactionId != Guid.Empty)
            {
                return true;
            }

            Logger.Debug("Starting a player bank transaction");

            var transactionId = _transactionCoordinator.RequestTransaction(
                RequestorId,
                (int)timeout.TotalMilliseconds,
                TransactionType.Write);

            if (transactionId != Guid.Empty)
            {
                TransactionId = transactionId;

                Logger.Debug($"Player bank transaction {TransactionId} created");

                _persistentStorageAccessor[CurrentTransactionId] = TransactionId;

                return true;
            }

            Logger.Error("Failed to acquire a player bank transaction");

            return false;
        }

        /// <inheritdoc />
        public void Unlock()
        {
            if (TransactionId != Guid.Empty)
            {
                Logger.Debug($"Ending player bank transaction {TransactionId}.");

                _persistentStorageAccessor[CurrentTransactionId] = Guid.Empty;

                _transactionCoordinator.ReleaseTransaction(TransactionId);
                TransactionId = Guid.Empty;
            }
        }

        /// <inheritdoc />
        public void Wager(long amount)
        {
            ValidateTransaction();

            var betRemaining = amount * GamingConstants.Millicents;
            foreach (var account in AccountTypes)
            {
                var bet = GetBetAmount(account, betRemaining);
                if (bet <= 0)
                {
                    continue;
                }

                Withdraw(account, bet, TransactionId);
                betRemaining -= bet;
            }
        }

        /// <inheritdoc />
        public void AddWin(long amount)
        {
            if (amount <= 0)
            {
                return;
            }

            ValidateTransaction();

            _bank.Deposit(AccountType.Cashable, amount * GamingConstants.Millicents, TransactionId);
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IPlayerBank) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        private void ValidateTransaction()
        {
            if (TransactionId != Guid.Empty &&
                _transactionCoordinator.VerifyCurrentTransaction(TransactionId))
            {
                return;
            }

            const string message = "Transactions outside of game play are not permitted";
            Logger.Fatal(message);
            throw new InvalidOperationException(message);
        }

        private long GetBetAmount(AccountType accountType, long betAmount)
        {
            if (betAmount <= 0)
            {
                return 0;
            }

            var accountBalance = _bank.QueryBalance(accountType);
            if (accountBalance > 0)
            {
                return accountBalance >= betAmount ? betAmount : accountBalance;
            }

            return 0;
        }

        private void Withdraw(AccountType account, long amount, Guid transactionId)
        {
            var meterDictionary = new Dictionary<AccountType, (string wageredMeter, string cardedMeter)>
            {
                {
                    AccountType.Cashable,
                    (GamingMeters.WageredCashableAmount, PlayerMeters.CardedWageredCashableAmount)
                },
                {
                    AccountType.Promo,
                    (GamingMeters.WageredPromoAmount, PlayerMeters.CardedWageredPromoAmount)
                },
                {
                    AccountType.NonCash,
                    (GamingMeters.WageredNonCashableAmount, PlayerMeters.CardedWageredNonCashableAmount)
                }
            };

            if (_bank.CheckWithdraw(account, amount, transactionId))
            {
                _bank.Withdraw(account, amount, transactionId);

                if (account == AccountType.Promo)
                {
                    _history.AddPromoWager(amount);
                }

                _meters.GetMeter(meterDictionary[account].wageredMeter).Increment(amount);
                if (_players.HasActiveSession)
                {
                    _meters.GetMeter(meterDictionary[account].cardedMeter).Increment(amount);
                }
            }
            else
            {
                var message = $"Failed to withdraw {amount} from {account} account for transaction {transactionId}";
                Logger.Fatal(message);
                throw new BankException(message);
            }
        }
    }
}
