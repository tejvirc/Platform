namespace Aristocrat.Monaco.Gaming.Bonus.Strategies
{
    using System;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Contracts;
    using Contracts.Bonus;
    using Contracts.Meters;
    using Contracts.Payment;
    using Contracts.Session;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Runtime;

    public class Standard : BonusStrategy, IBonusStrategy
    {
        private readonly IPersistentStorageManager _storage;
        private readonly IGamePlayState _gamePlay;
        private readonly IEventBus _bus;
        private readonly IPropertiesManager _properties;
        private readonly ISystemDisableManager _systemDisable;

        public Standard(
            IPersistentStorageManager storage,
            ITransactionHistory transactions,
            IGameHistory history,
            IGamePlayState gamePlay,
            IGameMeterManager meters,
            IRuntime runtime,
            IEventBus bus,
            IPropertiesManager properties,
            IBank bank,
            ITransferOutHandler transferHandler,
            IMessageDisplay messages,
            IPlayerService players,
            ISystemDisableManager systemDisable,
            IPaymentDeterminationProvider paymentDeterminationProvider,
            IBalanceUpdateService balanceUpdateService)
            : base(properties, bank, transferHandler, transactions, history, meters, runtime, bus, messages, players, storage, paymentDeterminationProvider, balanceUpdateService)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _gamePlay = gamePlay ?? throw new ArgumentNullException(nameof(gamePlay));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _systemDisable = systemDisable ?? throw new ArgumentNullException(nameof(systemDisable));
        }

        public BonusTransaction CreateTransaction<T>(int deviceId, T request) where T : IBonusRequest
        {
            if (!(request is StandardBonus standardBonus))
            {
                throw new ArgumentException(nameof(request));
            }

            using (var scope = _storage.ScopedTransaction())
            {
                var transaction = ToTransaction(deviceId, request);

                if (!_properties.GetValue(GamingConstants.IsGameRunning, false))
                {
                    Failed(transaction, BonusException.Failed);
                }
                else if (_systemDisable.IsDisabled && !standardBonus.AllowedWhileDisabled)
                {
                    Failed(transaction, BonusException.NotPlayable);
                }
                else
                {
                    Validate(transaction, request);
                }

                scope.Complete();

                return transaction;
            }
        }

        public bool CanPay(BonusTransaction transaction)
        {
            return transaction.State == BonusState.Pending && _gamePlay.UncommittedState == PlayState.Idle;
        }

        public async Task<IContinuationContext> Pay(BonusTransaction transaction, Guid transactionId, IContinuationContext context)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            if (!CanPay(transaction))
            {
                return null;
            }

            _bus.Publish(new BonusStartedEvent(transaction));

            using (var scope = _storage.ScopedTransaction())
            {
                var (success, pending) = Pay(
                        transaction,
                        transactionId,
                        transaction.CashableAmount,
                        transaction.NonCashAmount,
                        transaction.PromoAmount);

                // Must be committed before awaiting the pending transfer if there is one
                scope.Complete();

                if (pending != null)
                {
                    success = await pending.Task;
                }

                if (success)
                {
                    InternalDisplayMessage(transaction);
                }
            }

            return null;
        }

        public bool Cancel(BonusTransaction transaction)
        {
            return Cancel(transaction, CancellationReason.Any);
        }

        public bool Cancel(BonusTransaction transaction, CancellationReason reason)
        {
            return reason == CancellationReason.Any && InternalCancel(transaction);
        }

        public async Task Recover(BonusTransaction transaction, Guid transactionId)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            if (!CanPay(transaction))
            {
                return;
            }

            _bus.Publish(new BonusStartedEvent(transaction));

            using (var scope = _storage.ScopedTransaction())
            {
                var (success, pending) = Recover(
                    transaction,
                    transactionId,
                    transaction.CashableAmount,
                    transaction.NonCashAmount,
                    transaction.PromoAmount);

                // Must be committed before awaiting the pending transfer if there is one
                scope.Complete();

                if (pending != null)
                {
                    success = await pending.Task;
                }

                if (success)
                {
                    InternalDisplayMessage(transaction);
                }
            }
        }

        protected override void CompletePayment(BonusTransaction transaction, long cashableAmount, long nonCashAmount, long promoAmount)
        {
            base.CompletePayment(transaction, cashableAmount, nonCashAmount, promoAmount);

            if (transaction.PaidAmount == transaction.TotalAmount)
            {
                Commit(transaction);
            }
        }

        private void InternalDisplayMessage(BonusTransaction transaction)
        {
            if (transaction.MessageDuration == TimeSpan.MaxValue)
            {
                _bus.Subscribe<GameEndedEvent>(this, _ => HandleStateChange(this, transaction));
                _bus.Subscribe<PrimaryGameStartedEvent>(this, _ => HandleStateChange(this, transaction));
            }

            DisplayMessage(transaction);
        }

        private void HandleStateChange(Standard @this, BonusTransaction transaction)
        {
            RemoveMessage(transaction);

            _bus.UnsubscribeAll(@this);
        }
    }
}
