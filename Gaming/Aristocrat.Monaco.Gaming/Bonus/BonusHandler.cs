namespace Aristocrat.Monaco.Gaming.Bonus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts.OperatorMenu;
    using Contracts;
    using Contracts.Bonus;
    using Hardware.Contracts;
    using Hardware.Contracts.IdReader;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    /// <summary>
    ///     An implementation of <see cref="IBonusHandler" />
    /// </summary>
    public class BonusHandler : IBonusHandler, IService, IDisposable
    {
        private const int DeviceId = 1;

        private static readonly Guid RequestorId = new("{0E764637-41DE-4DF1-A0A7-3E018CDE0FBC}");
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

        private readonly IPlayerBank _bank;
        private readonly IEventBus _bus;
        private readonly IGamePlayState _gamePlayState;
        private readonly IPersistentStorageManager _storage;
        private readonly IBonusStrategyFactory _strategies;
        private readonly ITransactionCoordinator _transactionCoordinator;
        private readonly ITransactionHistory _transactionHistory;

        private readonly object _sync = new object();

        private DateTime _delayPeriodExpiration;
        private BonusTransactionRequest _current;

        private bool InAuditMode { get; set; }
        private bool _disposed;

        public BonusHandler(
            IBonusStrategyFactory strategies,
            IGamePlayState gamePlayState,
            ITransactionHistory transactionHistory,
            IPlayerBank bank,
            IPersistentStorageManager storage,
            ITransactionCoordinator transactionCoordinator,
            IEventBus bus)
        {
            _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _transactionCoordinator = transactionCoordinator ?? throw new ArgumentNullException(nameof(transactionCoordinator));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));

            _bus.Subscribe<PrimaryGameStartedEvent>(this, _ => ApplyGameEndDelay());
            _bus.Subscribe<GameIdleEvent>(this, Handle);
            _bus.Subscribe<TransferOutCompletedEvent>(this, Handle);
            _bus.Subscribe<SetValidationEvent>(this, _ => CancelAll(CancellationReason.IdInvalidated), e => e.Identity == null);
            _bus.Subscribe<DisabledEvent>(this, _ => CancelAll(CancellationReason.IdInvalidated));
            _bus.Subscribe<OperatorMenuEnteredEvent>(this, _ => InAuditMode = true);
            _bus.Subscribe<TransactionCompletedEvent>(this, Handle);
            _bus.Subscribe<OperatorMenuExitedEvent>(this, _ =>
            {
                InAuditMode = false;
                Commit();
            });

            MaxPending = _transactionHistory.GetMaxTransactions<BonusTransaction>();
        }

        public TimeSpan GameEndDelay { get; private set; }

        public TimeSpan DelayDuration => _delayPeriodExpiration < DateTime.UtcNow
            ? TimeSpan.Zero
            : _delayPeriodExpiration - DateTime.UtcNow;

        public int DelayedGames { get; private set; }

        public bool EvaluateBoth { get; private set; }

        public IReadOnlyCollection<BonusTransaction> Transactions
        {
            get
            {
                lock (_sync)
                {
                    return _transactionHistory.RecallTransactions<BonusTransaction>();
                }
            }
        }

        public int MaxPending { get; set; }

        private BonusTransactionRequest CurrentTransaction
        {
            get { return _current ??= _storage.GetEntity<BonusTransactionRequest>(); }
            set
            {
                if (value != null)
                {
                    _current = _storage.UpdateEntity(value);
                }
            }
        }

        public void SetGameEndDelay(TimeSpan delay)
        {
            SetGameEndDelay(delay, TimeSpan.MaxValue, 0, false);
        }

        public void SetGameEndDelay(TimeSpan delay, TimeSpan duration, int numberOfGames, bool useBoth)
        {
            GameEndDelay = delay;
            DelayedGames = numberOfGames;
            EvaluateBoth = useBoth;

            if (duration != TimeSpan.Zero)
            {
                _delayPeriodExpiration = duration == TimeSpan.MaxValue ? DateTime.MaxValue : DateTime.UtcNow + duration;
            }
            else
            {
                _delayPeriodExpiration = DateTime.MinValue;
            }

            _gamePlayState.SetGameEndDelay(delay);

            if (GameEndDelay == TimeSpan.Zero || DelayDuration == TimeSpan.Zero && DelayedGames == 0)
            {
                EndGameDelay();
            }
            else
            {
                _bus.Publish(new GameDelayPeriodStartedEvent(DeviceId));
            }
        }

        public void SkipGameEndDelay()
        {
            if (_gamePlayState.InGameRound)
            {
                _gamePlayState.SetGameEndDelay(TimeSpan.Zero);
            }
        }

        public BonusTransaction Award<T>(T request) where T : IBonusRequest
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var strategy = _strategies.Create(request.Mode);
            if (strategy == null)
            {
                return null;
            }

            lock (_sync)
            {
                var bonus = strategy.CreateTransaction(DeviceId, request);

                Logger.Info($"Created pending bonus: {bonus}");
                if (bonus.Exception == (int)BonusException.None
                    && strategy.CanPay(bonus)
                    && (request is not StandardBonus standardBonus || standardBonus.AllowedWhileInAuditMode || !InAuditMode))
                {
                    Commit();
                }

                return bonus;
            }
        }

        public void Acknowledge(long transactionId)
        {
            Acknowledge(Transactions.FirstOrDefault(t => t.TransactionId == transactionId));
        }

        public void Acknowledge(string bonusId)
        {
            Acknowledge(Transactions.FirstOrDefault(t => t.BonusId == bonusId));
        }

        public bool Commit()
        {
            return Commit(Guid.Empty);
        }

        public bool Commit(Guid transactionId)
        {
            lock (_sync)
            {
                var pending = GetPendingBonusTransactions().ToList();
                if (pending.Count == 0 || _transactionCoordinator.IsTransactionActive ||
                    CurrentTransaction != null && CurrentTransaction.TransactionId != Guid.Empty)
                {
                    return false;
                }

                var ownedTransaction = false;
                if (transactionId == Guid.Empty)
                {
                    transactionId = _transactionCoordinator.RequestTransaction(RequestorId, (int)DefaultTimeout.TotalMilliseconds, TransactionType.Write);
                    if (transactionId == Guid.Empty)
                    {
                        return false;
                    }

                    ownedTransaction = true;
                }

                CurrentTransaction = new BonusTransactionRequest
                {
                    TransactionId = transactionId,
                    OwnedTransaction = ownedTransaction
                };

                Task.Run(async () =>
                {
                    await CommitAsync(CurrentTransaction, pending);
                    if (ownedTransaction && GetPendingBonusTransactions().Any())
                    {
                        Commit();
                    }
                });

                return true;
            }
        }

        private IEnumerable<BonusTransaction> GetPendingBonusTransactions()
        {
            return Transactions
                .Where(t => t.State == BonusState.Pending && _strategies.Create(t.Mode).CanPay(t))
                .OrderBy(t => t.Mode)
                .ThenBy(t => t.TransactionId);
        }

        private async Task CommitAsync(BonusTransactionRequest transactionRequest, IEnumerable<BonusTransaction> transactions)
        {
            IContinuationContext context = null;

            try
            {
                foreach (var transaction in transactions)
                {
                    context = await PayBonus(transaction, transactionRequest.TransactionId, context);
                }
            }
            finally
            {
                ClearTransaction();

                _bus.Publish(new BonusCommitCompletedEvent());
            }
        }

        public bool Cancel(long transactionId)
        {
            lock (_sync)
            {
                var bonus = Transactions.FirstOrDefault(t => t.TransactionId == transactionId);

                return Cancel(bonus);
            }
        }

        public bool Cancel(string bonusId)
        {
            lock (_sync)
            {
                var bonus = Transactions.FirstOrDefault(t => t.BonusId == bonusId);

                return Cancel(bonus);
            }
        }

        public bool Exists(string bonusId)
        {
            return Transactions.Any(t => t.BonusId == bonusId);
        }

        public bool Exists(long transactionId)
        {
            return Transactions.Any(t => t.TransactionId == transactionId);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Name => typeof(BonusHandler).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(IBonusHandler) };

        public void Initialize()
        {
            Task.Run(Recover);
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

        private async Task<IContinuationContext> PayBonus(BonusTransaction bonus, Guid transactionId, IContinuationContext context)
        {
            var strategy = _strategies.Create(bonus.Mode);
            if (strategy != null)
            {
                return await strategy.Pay(bonus, transactionId, context);
            }

            Logger.Error($"Failed to find handler for {bonus}");
            return null;
        }

        private async Task Recover()
        {
            // We're not going to recover any transaction that is owned/initiated outside the scope of this class
            if (CurrentTransaction == null || CurrentTransaction.TransactionId == Guid.Empty || !CurrentTransaction.OwnedTransaction)
            {
                ClearTransaction();
                return;
            }

            var pending = Transactions.ToList();

            foreach (var transaction in pending)
            {
                var strategy = _strategies.Create(transaction.Mode);
                await (strategy?.Recover(transaction, CurrentTransaction.TransactionId) ?? Task.CompletedTask);
            }

            lock (_sync)
            {
                ClearTransaction();

                if (GetPendingBonusTransactions().Any())
                {
                    Commit();
                }
            }
        }

        private void Handle(GameIdleEvent evt)
        {
            EvaluateGameEndDelay();

            if (_bank.Balance == 0)
            {
                CancelAll(CancellationReason.ZeroCredits);
            }
        }

        private void Handle(TransferOutCompletedEvent evt)
        {
            Commit();
        }

        private void Handle(TransactionCompletedEvent evt)
        {
            Commit();
        }

        private void ApplyGameEndDelay()
        {
            if (GameEndDelay == TimeSpan.Zero)
            {
                return;
            }

            EvaluateGameEndDelay();

            if (DelayDuration != TimeSpan.Zero || DelayedGames > 0)
            {
                _gamePlayState.SetGameEndDelay(GameEndDelay);

                if (DelayedGames > 0)
                {
                    DelayedGames--;
                }
            }
        }

        private void EvaluateGameEndDelay()
        {
            if (GameEndDelay == TimeSpan.Zero)
            {
                return;
            }

            var reset = false;

            if (EvaluateBoth && DelayDuration == TimeSpan.Zero && DelayedGames == 0)
            {
                reset = true;
            }
            else if (!EvaluateBoth && (DelayDuration == TimeSpan.Zero || DelayedGames == 0))
            {
                reset = true;
            }

            // TODO: This needs to be tracked better since it's currently only applied at game idle
            if (reset)
            {
                EndGameDelay();
            }
        }

        private void EndGameDelay()
        {
            GameEndDelay = TimeSpan.Zero;
            _delayPeriodExpiration = DateTime.MinValue;
            DelayedGames = 0;
            EvaluateBoth = true;

            _bus.Publish(new GameDelayPeriodEndedEvent(DeviceId));
        }

        private void Acknowledge(BonusTransaction bonus)
        {
            if (bonus == null)
            {
                return;
            }

            bonus.State = BonusState.Acknowledged;
            bonus.LastUpdate = DateTime.UtcNow;

            lock (_sync)
            {
                _transactionHistory.UpdateTransaction(bonus);
            }

            Logger.Info($"Acknowledged pending bonus: {bonus}");
        }

        private bool Cancel(BonusTransaction bonus)
        {
            if (bonus == null)
            {
                return false;
            }

            var strategy = _strategies.Create(bonus.Mode);

            return strategy?.Cancel(bonus) ?? false;
        }

        private void CancelAll(CancellationReason reason)
        {
            var pending = Transactions.Where(t => t.State == BonusState.Pending);
            foreach (var transaction in pending)
            {
                var strategy = _strategies.Create(transaction.Mode);

                strategy?.Cancel(transaction, reason);
            }
        }

        private void ClearTransaction()
        {
            if (CurrentTransaction.OwnedTransaction)
            {
                _transactionCoordinator.ReleaseTransaction(CurrentTransaction.TransactionId);
            }

            CurrentTransaction = new BonusTransactionRequest();
        }

        [Entity(PersistenceLevel.Critical)]
        private sealed class BonusTransactionRequest
        {
            [Field]
            public Guid TransactionId { get; set; }

            [Field]
            public bool OwnedTransaction { get; set; }
        }
    }
}