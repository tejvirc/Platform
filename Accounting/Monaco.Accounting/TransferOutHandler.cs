namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Contracts;
    using Contracts.Transactions;
    using Contracts.TransferOut;
    using Handpay;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Mono.Addins;

    [CLSCompliant(false)]
    public class TransferOutHandler : ITransferOutHandler, IService, IDisposable
    {
        private const string TransferServicesExtensionPath = "/Accounting/TransferOutProviders";

        private static readonly Guid RequestorId = new Guid("{A4303A75-B3E4-4C96-8F7D-F02A9A983BE0}");
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IBank _bank;
        private readonly IEventBus _bus;
        private readonly IPersistentStorageManager _storage;
        private readonly ITransactionCoordinator _transactionCoordinator;
        private readonly IPropertiesManager _properties;
        private readonly IMoneyLaunderingMonitor _moneyLaunderingMonitor;
        private readonly List<(ITransferOutProvider instance, bool permitted)> _providers;
        private readonly ConcurrentQueue<(Guid traceId, Action action)> _pending = new ConcurrentQueue<(Guid, Action)>();
        private readonly object _lock = new object();

        private TransferOutTransaction _current;

        private bool _disposed;

        public TransferOutHandler()
            : this(
                ServiceManager.GetInstance().GetService<IBank>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<ITransactionCoordinator>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                LoadProviders(),
                ServiceManager.GetInstance().GetService<IMoneyLaunderingMonitor>())
        {
        }

        public TransferOutHandler(
            IBank bank,
            IEventBus bus,
            ITransactionCoordinator transactionCoordinator,
            IPersistentStorageManager storage,
            IPropertiesManager properties,
            IEnumerable<(ITransferOutProvider provider, bool permitted)> providers,
            IMoneyLaunderingMonitor moneyLaunderingMonitor)
        {
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _transactionCoordinator = transactionCoordinator ?? throw new ArgumentNullException(nameof(transactionCoordinator));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _moneyLaunderingMonitor = moneyLaunderingMonitor ?? throw new ArgumentNullException(nameof(moneyLaunderingMonitor));

            if (providers == null)
            {
                throw new ArgumentNullException(nameof(providers));
            }

            _providers = providers.ToList();
        }

        private TransferOutTransaction CurrentTransaction
        {
            get { return _current = _current ?? _storage.GetEntity<TransferOutTransaction>(); }
            set
            {
                if (value != null)
                {
                    _current = _storage.UpdateEntity(value);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Name => typeof(TransferOutHandler).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(ITransferOutHandler) };

        public void Initialize()
        {
            RecoverInternal();
        }

        public bool InProgress
        {
            get
            {
                lock (_lock)
                {
                    return CurrentTransaction.TransactionId != Guid.Empty;
                }
            }
        }

        public bool Pending => _pending.Count > 0;

        public bool TransferOut(TransferOutReason reason = TransferOutReason.CashOut)
        {
            if (InProgress)
            {
                Logger.Warn($"A transfer is currently in progress - {CurrentTransaction.TransactionId}");
                return false;
            }

            if (!IsValid(reason))
            {
                return false;
            }

            var transactionId = _transactionCoordinator.RequestTransaction(RequestorId, 0, TransactionType.Write, true);
            if (transactionId == Guid.Empty)
            {
                return false;
            }

            CurrentTransaction = new TransferOutTransaction(new CancellationTokenSource())
            {
                TransactionId = transactionId,
                OwnedTransaction = true
            };

            Task.Run(
                () => TransferAsync(CurrentTransaction, CurrentTransaction.Token, reason, Enumerable.Empty<long>()),
                CurrentTransaction.Token);

            return true;
        }

        public bool TransferOut(AccountType account, long amount, TransferOutReason reason)
        {
            if (InProgress)
            {
                Logger.Warn($"A transfer is currently in progress - {CurrentTransaction.TransactionId}");
                return false;
            }

            var transactionId = _transactionCoordinator.RequestTransaction(RequestorId, 0, TransactionType.Write, true);
            if (transactionId == Guid.Empty)
            {
                return false;
            }

            CurrentTransaction = new TransferOutTransaction(new CancellationTokenSource())
            {
                TransactionId = transactionId,
                OwnedTransaction = true
            };

            if (!IsValid(account, amount, reason))
            {
                ClearTransaction();
                return false;
            }

            Task.Run(
                () => TransferAsync(
                    CurrentTransaction,
                    account,
                    amount,
                    CurrentTransaction.Token,
                    reason,
                    Enumerable.Empty<long>()),
                CurrentTransaction.Token);

            return true;
        }

        public bool TransferOut<TProvider>(AccountType account, long amount, TransferOutReason reason)
            where TProvider : ITransferOutProvider
        {
            if (InProgress)
            {
                Logger.Warn($"A transfer is currently in progress - {CurrentTransaction.TransactionId}");
                return false;
            }

            if (!IsValid(account, amount, reason))
            {
                return false;
            }

            var transactionId = _transactionCoordinator.RequestTransaction(RequestorId, 0, TransactionType.Write, true);
            if (transactionId == Guid.Empty)
            {
                return false;
            }

            CurrentTransaction = new TransferOutTransaction(new CancellationTokenSource())
            {
                TransactionId = transactionId,
                OwnedTransaction = true
            };

            Task.Run(
                () => TransferAsync(
                    CurrentTransaction,
                    account,
                    amount,
                    CurrentTransaction.Token,
                    reason,
                    Enumerable.Empty<long>(),
                    providers => providers.Select(p => p.instance).OfType<TProvider>().Cast<ITransferOutProvider>()),
                CurrentTransaction.Token);

            return true;
        }

        public bool TransferOut(Guid transactionId,
            IReadOnlyCollection<long> associatedTransactions,
            TransferOutReason reason,
            Guid traceId)
        {
            lock (_lock)
            {
                if (InProgress)
                {
                    _pending.Enqueue((traceId,
                        () => TransferOut(
                            transactionId,
                            associatedTransactions,
                            reason,
                            traceId)));
                    Logger.Info("A transfer is currently in progress. Request has been queued");
                    return true;
                }

                if (!IsValid(transactionId, reason))
                {
                    return false;
                }

                CurrentTransaction = new TransferOutTransaction(new CancellationTokenSource())
                {
                    TransactionId = transactionId,
                    OwnedTransaction = false,
                    TraceId = traceId
                };
            }

            Task.Run(
                () => TransferAsync(CurrentTransaction, CurrentTransaction.Token, reason, associatedTransactions),
                CurrentTransaction.Token);

            return true;
        }

        public bool TransferOut(
            Guid transactionId,
            AccountType account,
            long amount,
            IReadOnlyCollection<long> associatedTransactions,
            TransferOutReason reason,
            Guid traceId)
        {
            lock (_lock)
            {
                if (InProgress)
                {
                    _pending.Enqueue((traceId,
                        () => TransferOut(
                            transactionId,
                            account,
                            amount,
                            associatedTransactions,
                            reason,
                            traceId)));
                    Logger.Info("A transfer is currently in progress. Request has been queued");
                    return true;
                }

                if (!IsValid(transactionId, account, amount, reason))
                {
                    return false;
                }

                CurrentTransaction = new TransferOutTransaction(new CancellationTokenSource())
                {
                    TransactionId = transactionId,
                    OwnedTransaction = false,
                    TraceId = traceId
                };
            }

            Task.Run(
                () => TransferAsync(
                    CurrentTransaction,
                    account,
                    amount,
                    CurrentTransaction.Token,
                    reason,
                    associatedTransactions),
                CurrentTransaction.Token);

            return true;
        }

        public bool TransferOut<TProvider>(
            Guid transactionId,
            AccountType account,
            long amount,
            IReadOnlyCollection<long> associatedTransactions,
            TransferOutReason reason,
            Guid traceId) where TProvider : ITransferOutProvider
        {
            lock (_lock)
            {
                if (InProgress)
                {
                    _pending.Enqueue((traceId,
                        () => TransferOut<TProvider>(
                            transactionId,
                            account,
                            amount,
                            associatedTransactions,
                            reason,
                            traceId)));
                    Logger.Info("A transfer is currently in progress. Request has been queued");
                    return true;
                }

                // Types other than cashout do not hit the credit meter
                if (reason != TransferOutReason.LargeWin &&
                    reason != TransferOutReason.BonusPay &&
                    (reason == TransferOutReason.CashOut && !IsValid(transactionId, account, amount, reason) ||
                     !IsValid(transactionId, reason)))
                {
                    return false;
                }

                CurrentTransaction = new TransferOutTransaction(new CancellationTokenSource())
                {
                    TransactionId = transactionId,
                    OwnedTransaction = false,
                    TraceId = traceId
                };
            }

            Task.Run(
                () => TransferAsync(
                    CurrentTransaction,
                    account,
                    amount,
                    CurrentTransaction.Token,
                    reason,
                    associatedTransactions,
                    providers => providers.Select(p => p.instance).OfType<TProvider>().Cast<ITransferOutProvider>()),
                CurrentTransaction.Token);

            return true;
        }

        public bool TransferOutWithContinuation<TProvider>(
            Guid transactionId,
            long cashableAmount,
            long promoAmount,
            long nonCashAmount,
            IReadOnlyCollection<long> associatedTransactions,
            TransferOutReason reason,
            Guid traceId) where TProvider : ITransferOutProvider
        {
            // We're only going to support these types for now.  This can be extended as needed
            if (reason != TransferOutReason.LargeWin &&
                reason != TransferOutReason.BonusPay &&
                reason != TransferOutReason.CashWin)
            {
                return false;
            }

            lock (_lock)
            {
                if (InProgress)
                {
                    return false;
                }

                CurrentTransaction = new TransferOutTransaction(new CancellationTokenSource())
                {
                    TransactionId = transactionId,
                    OwnedTransaction = false,
                    TraceId = traceId
                };
            }

            Task.Run(
                () => TransferAsync(
                    CurrentTransaction,
                    cashableAmount,
                    promoAmount,
                    nonCashAmount,
                    CurrentTransaction.Token,
                    reason,
                    associatedTransactions,
                    providers => providers.TakeFrom(p => p.instance is TProvider).Select(p => p.instance)));

            return true;
        }

        public IReadOnlyCollection<Guid> Recover()
        {
            lock (_lock)
            {
                var recoveryItems = _pending.Select(x => x.traceId).ToList();

                // Any open, owned transaction should have been recovered at start up
                if (CurrentTransaction == null || CurrentTransaction.TransactionId == Guid.Empty || CurrentTransaction.OwnedTransaction)
                {
                    return recoveryItems;
                }

                var current = InternalRecovery();
                if (current != Guid.Empty)
                {
                    recoveryItems.Add(current);
                }

                return recoveryItems;
            }
        }

        public Guid Recover(Guid transactionId)
        {
            lock (_lock)
            {
                if (_pending.Any(x => x.traceId == transactionId))
                {
                    return transactionId;
                }

                // Any open, owned transaction should have been recovered at start up
                if (CurrentTransaction == null || CurrentTransaction.TransactionId != transactionId ||
                    CurrentTransaction.OwnedTransaction)
                {
                    return Guid.Empty;
                }

                return InternalRecovery();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                while (_pending.TryDequeue(out _)) { }

                UnloadProviders();
                _providers.Clear();

                // ReSharper disable once UseNullPropagation
                if (CurrentTransaction != null)
                {
                    CurrentTransaction?.Dispose();
                }
            }

            _disposed = true;
        }

        private static IEnumerable<(ITransferOutProvider provider, bool permitted)> LoadProviders()
        {
            var providers = new List<(ITransferOutProvider provider, bool permitted)>();

            // This is the configured list for the active jurisdiction
            var permitted = MonoAddinsHelper.GetSelectedNodes<TypeExtensionNode>(TransferServicesExtensionPath);

            // This is the list of all discovered providers
            var nodes = AddinManager.GetExtensionNodes<TypeExtensionNode>(TransferServicesExtensionPath);
            foreach (var node in nodes)
            {
                // A provider may expose itself as a service, so we're going to init and add it here to ensure we only have one instance
                var provider = (ITransferOutProvider)node.CreateInstance();
                if (provider is IService service)
                {
                    service.Initialize();
                    ServiceManager.GetInstance().AddService(service);
                }

                providers.Add((provider, permitted.Any(p => p.Type == node.Type)));
            }

            return providers;
        }

        private Guid InternalRecovery()
        {
            if (CurrentTransaction.IsActive)
            {
                return CurrentTransaction.TraceId;
            }

            // Don't re-persist since this transaction is already started we re-create
            // We need to state we are recovering
            _current = new TransferOutTransaction(new CancellationTokenSource())
            {
                TraceId = _current.TraceId,
                OwnedTransaction = _current.OwnedTransaction,
                TransactionId = _current.TransactionId
            };

            // Check each provider to see if there is anything to recover
            if (!_providers.Any(provider => provider.instance.CanRecover(CurrentTransaction.TransactionId)))
            {
                Logger.Warn(
                    $"Failed to recover the current transaction.  TransactionId={CurrentTransaction.TransactionId}, TraceId={CurrentTransaction.TraceId}");

                // If providers have nothing to recover we need to abandon the transaction
                ClearTransaction();
                return Guid.Empty;
            }

            Task.Run(() => RecoverAsync(CurrentTransaction, CancellationToken.None));
            return CurrentTransaction.TraceId;
        }

        private void UnloadProviders()
        {
            foreach (var provider in _providers)
            {
                if (provider.instance is IService service)
                {
                    ServiceManager.GetInstance().RemoveService(service);
                }
            }
        }

        private async Task TransferAsync(
            TransferOutTransaction transaction,
            CancellationToken token,
            TransferOutReason reason,
            IEnumerable<long> associatedTransactions,
            ProvidersFilter providersFilter = null)
        {
            var cashable = _bank.QueryBalance(AccountType.Cashable);
            var promo = _bank.QueryBalance(AccountType.Promo);
            var nonCash = _bank.QueryBalance(AccountType.NonCash);

            await TransferAsync(
                transaction,
                cashable,
                promo,
                nonCash,
                token,
                reason,
                associatedTransactions,
                providersFilter);
        }

        private async Task TransferAsync(
            TransferOutTransaction transaction,
            AccountType account,
            long amount,
            CancellationToken token,
            TransferOutReason reason,
            IEnumerable<long> associatedTransactions,
            ProvidersFilter providersFilter = null)
        {
            var cashable = account == AccountType.Cashable ? amount : 0;
            var promo = account == AccountType.Promo ? amount : 0;
            var nonCash = account == AccountType.NonCash ? amount : 0;

            await TransferAsync(
                transaction,
                cashable,
                promo,
                nonCash,
                token,
                reason,
                associatedTransactions,
                providersFilter);
        }

        private async Task<bool> TransferAsync(
            TransferOutTransaction transaction,
            long cashable,
            long promo,
            long nonCash,
            CancellationToken token,
            TransferOutReason reason,
            IEnumerable<long> associatedTransactions,
            ProvidersFilter providersFilter = null)
        {
            var handled = false;
            var clearTransaction = true;

            if (providersFilter == null)
            {
                providersFilter = providers => providers.Where(p => p.permitted).Select(p => p.instance);
            }

            _bus.Publish(new TransferOutStartedEvent(transaction.TransactionId, cashable, promo, nonCash));

            var transactions = associatedTransactions as long[] ?? associatedTransactions.ToArray();

            var cashableRemaining = cashable;
            var promoRemaining = promo;
            var nonCashRemaining = nonCash;

            long Remaining()
            {
                return cashableRemaining + promoRemaining + nonCashRemaining;
            }

            try
            {
                _properties.SetProperty(AccountingConstants.TransferOutContext, new TransferOutContext());

                var excessiveThresholdReached = _moneyLaunderingMonitor.IsThresholdReached();

                foreach (var provider in providersFilter(_providers))
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    if (excessiveThresholdReached && !(provider is HandpayProvider))
                    {
                        Logger.Info($"Transfer out skipped for: {provider.GetType().Name} due to excessive threshold.");

                        continue;
                    }

                    var result = await provider.Transfer(
                        transaction.TransactionId,
                        cashableRemaining,
                        promoRemaining,
                        nonCashRemaining,
                        transactions,
                        reason,
                        transaction.TraceId,
                        token);

                    if (!result.Success)
                    {
                        Logger.Info($"Transfer out failed for: {provider.GetType().Name}");

                        continue;
                    }

                    cashableRemaining -= result.TransferredCashable;
                    promoRemaining -= result.TransferredPromo;
                    nonCashRemaining -= result.TransferredNonCash;

                    if (Remaining() == 0)
                    {
                        if (transaction.OwnedTransaction)
                        {
                            _transactionCoordinator.ReleaseTransaction(transaction.TransactionId);
                        }

                        handled = true;

                        break;
                    }

                    if (result.IsPartialTransferOut)
                    {
                        handled = false;
                        break;
                    }

                    Logger.Info($"Remaining - Cashable {cashableRemaining}, Promo: {promoRemaining}, NonCash: {nonCash}");
                }
            }
            catch (TransferOutException ex)
            {
                handled = false;

                if (ex.CanRecover)
                {
                    clearTransaction = false;
                }
            }

            var traceId = transaction.TraceId;

            lock (_lock)
            {
                if (clearTransaction)
                {
                    ClearTransaction();
                }

                if (handled)
                {
                    _bus.Publish(new TransferOutCompletedEvent(cashable, promo, nonCash, Pending, traceId));
                }
                else
                {
                    _bus.Publish(
                        new TransferOutFailedEvent(cashableRemaining, promoRemaining, nonCashRemaining, traceId));
                }

                ProcessPendingTransfers();
            }

            return handled;
        }

        private void ClearTransaction()
        {
            if (CurrentTransaction.OwnedTransaction)
            {
                _transactionCoordinator.ReleaseTransaction(CurrentTransaction.TransactionId);
            }

            CurrentTransaction.Dispose();
            CurrentTransaction = new TransferOutTransaction();
        }

        private bool IsValid(TransferOutReason reason)
        {
            if (HasCashableCredits() || !reason.AffectsBalance())
            {
                return true;
            }

            Logger.Warn("Transfer out failed because of a zero bank balance");
            return false;
        }

        private bool IsValid(Guid transactionId, TransferOutReason reason)
        {
            if (!IsValid(reason))
            {
                return false;
            }

            if (_transactionCoordinator.VerifyCurrentTransaction(transactionId))
            {
                return true;
            }

            Logger.Warn($"Transfer out failed because transaction ID {transactionId} is not the current transaction");
            return false;
        }

        private bool IsValid(AccountType account, long amount, TransferOutReason reason)
        {
            if (!IsValid(reason))
            {
                return false;
            }

            if (!reason.AffectsBalance())
            {
                return true;
            }

            var balance = _bank.QueryBalance(account);
            if (balance >= amount)
            {
                return true;
            }

            Logger.Warn($"Transfer out failed with an insufficient amount ({amount}) greater than {account} balance ({balance})");
            return false;
        }

        private bool IsValid(Guid transactionId, AccountType account, long amount, TransferOutReason reason)
        {
            return IsValid(transactionId, reason) && IsValid(account, amount, reason);
        }

        private bool HasCashableCredits()
        {
            return HasCashableCredits(AccountType.Cashable) || HasCashableCredits(AccountType.Promo) ||
                   HasCashableCredits(AccountType.NonCash);
        }

        private bool HasCashableCredits(AccountType account)
        {
            return _bank.QueryBalance(account) > 0;
        }

        private void RecoverInternal()
        {
            // We're not going to recover any transaction that is owned by another instance.  They will need to initiate recovery on those transactions
            if (CurrentTransaction == null || CurrentTransaction.TransactionId == Guid.Empty || !CurrentTransaction.OwnedTransaction)
            {
                return;
            }

            Task.Run(() => RecoverAsync(CurrentTransaction, CurrentTransaction.Token), CurrentTransaction.Token);
        }

        private async Task RecoverAsync(TransferOutTransaction transaction, CancellationToken token)
        {
            _bus.Publish(new TransferOutStartedEvent(CurrentTransaction.TransactionId, 0, 0, 0));

            foreach (var (instance, _) in _providers.Where(p => p.permitted))
            {
                if (await instance.Recover(transaction, token))
                {
                    break;
                }
            }

            var traceId = transaction.TraceId;

            lock (_lock)
            {
                ClearTransaction();

                _bus.Publish(new TransferOutCompletedEvent(0, 0, 0, false, traceId));

                ProcessPendingTransfers();
            }
        }

        private void ProcessPendingTransfers()
        {
            if (!InProgress && _pending.TryDequeue(out var transfer))
            {
                transfer.action.Invoke();
            }
        }

        private delegate IEnumerable<ITransferOutProvider> ProvidersFilter(IEnumerable<(ITransferOutProvider instance, bool permitted)> providers);

        [Entity(PersistenceLevel.Critical)]
        private sealed class TransferOutTransaction : IRecoveryTransaction, IDisposable
        {
            private CancellationTokenSource _cts;

            private bool _disposed;

            public TransferOutTransaction()
                : this(null)
            {
            }

            public TransferOutTransaction(CancellationTokenSource cts)
            {
                _cts = cts;
            }

            [Field]
            public Guid TransactionId { get; set; }

            [Field]
            public bool OwnedTransaction { get; set; }

            [Field]
            public Guid TraceId { get; set; }

            public CancellationToken Token => _cts?.Token ?? CancellationToken.None;

            public bool IsActive => _cts != null;

            public void Dispose()
            {
                Dispose(true);
            }

            private void Dispose(bool disposing)
            {
                if (_disposed)
                {
                    return;
                }

                if (disposing)
                {
                    if (_cts != null)
                    {
                        _cts.Cancel(false);
                        _cts.Dispose();
                    }
                }

                _cts = null;

                _disposed = true;
            }
        }
    }
}