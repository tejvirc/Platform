namespace Aristocrat.Monaco.Gaming.Bonus.Strategies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.TransferOut;
    using Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Bonus;
    using Contracts.Meters;
    using Contracts.Payment;
    using Contracts.Session;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Runtime;

    public class GameWinBonusStrategy : BonusStrategy, IBonusStrategy
    {
        private readonly IPropertiesManager _properties;
        private readonly IGamePlayState _gamePlay;
        private readonly IGameHistory _history;
        private readonly IGameMeterManager _meters;
        private readonly IEventBus _bus;
        private readonly IPersistentStorageManager _storage;
        private readonly ITransactionHistory _transactions;

        public GameWinBonusStrategy(
            IPropertiesManager properties,
            IGamePlayState gamePlay,
            IBank bank,
            ITransferOutHandler transferHandler,
            ITransactionHistory transactions,
            IGameHistory history,
            IGameMeterManager meters,
            IRuntime runtime,
            IEventBus bus,
            IMessageDisplay messages,
            IPlayerService players,
            IPersistentStorageManager storage,
            IPaymentDeterminationProvider bonusPayDetermination)
            : base(
                properties,
                bank,
                transferHandler,
                transactions,
                history,
                meters,
                runtime,
                bus,
                messages,
                players,
                storage,
                bonusPayDetermination)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _gamePlay = gamePlay ?? throw new ArgumentNullException(nameof(gamePlay));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _history = history ?? throw new ArgumentNullException(nameof(history));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public BonusTransaction CreateTransaction<T>(int deviceId, T request) where T : IBonusRequest
        {
            if (request is not GameWinBonus)
            {
                throw new ArgumentException(nameof(request));
            }

            using var scope = _storage.ScopedTransaction();
            var transaction = ToTransaction(deviceId, request);
            var gameRound = _history.CurrentLog;
            if (!_properties.GetValue(GamingConstants.IsGameRunning, false) || _history.CurrentLog is null)
            {
                Failed(transaction, BonusException.Failed);
            }
            else
            {
                Validate(transaction, request);
                transaction.AssociatedTransactions =
                    transaction.AssociatedTransactions.Concat(new List<long> { gameRound.TransactionId });
                _transactions.UpdateTransaction(transaction);
            }

            scope.Complete();
            return transaction;
        }

        public bool CanPay(BonusTransaction transaction)
        {
            var playState = _gamePlay.UncommittedState;
            return transaction.State == BonusState.Pending &&
                   playState is PlayState.GameEnded or PlayState.PresentationIdle;
        }

        public Task<IContinuationContext> Pay(
            BonusTransaction transaction,
            Guid transactionId,
            IContinuationContext context)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            return !CanPay(transaction)
                ? Task.FromResult<IContinuationContext>(null)
                : PayInternal(transaction, transactionId, context);
        }

        public bool Cancel(BonusTransaction transaction)
        {
            return Cancel(transaction, CancellationReason.Any);
        }

        public bool Cancel(BonusTransaction transaction, CancellationReason reason)
        {
            return reason == CancellationReason.Any && InternalCancel(transaction);
        }

        public Task Recover(BonusTransaction transaction, Guid transactionId)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            return transaction.State != BonusState.Pending
                ? Task.CompletedTask
                : RecoverInternal(transaction, transactionId);
        }

        protected override void CompletePayment(
            BonusTransaction transaction,
            long cashableAmount,
            long nonCashAmount,
            long promoAmount)
        {
            base.CompletePayment(transaction, cashableAmount, nonCashAmount, promoAmount);
            UpdateMeters(transaction, cashableAmount, nonCashAmount, promoAmount);
            if (transaction.PaidAmount != transaction.TotalAmount)
            {
                return;
            }

            Commit(transaction);
            _history.AddGameWinBonus((cashableAmount + nonCashAmount + promoAmount).MillicentsToCents());
        }

        private async Task<IContinuationContext> PayInternal(
            BonusTransaction transaction,
            Guid transactionId,
            IContinuationContext context)
        {
            using var scope = _storage.ScopedTransaction();
            var total = transaction.CashableAmount + transaction.NonCashAmount + transaction.PromoAmount;

            transaction.PayMethod = GetPayMethod(transaction, total);

            _bus.Publish(new BonusStartedEvent(transaction));

            var (success, pending) = Pay(
                transaction,
                transactionId,
                transaction.CashableAmount,
                transaction.NonCashAmount,
                transaction.PromoAmount,
                transaction.PayMethod == PayMethod.Handpay
                    ? TransferOutReason.LargeWin
                    : TransferOutReason.CashWin);
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

            return context;
        }

        private async Task RecoverInternal(BonusTransaction transaction, Guid transactionId)
        {
            var success = false;
            TaskCompletionSource<bool> pending;
            using (var scope = _storage.ScopedTransaction())
            {
                (_, pending) = RecoverTransfer(
                    transaction,
                    transactionId,
                    transaction.CashableAmount,
                    transaction.NonCashAmount,
                    transaction.PromoAmount);
                scope.Complete();
            }

            if (pending != null)
            {
                success = await pending.Task;
            }

            if (success)
            {
                InternalDisplayMessage(transaction);
            }
        }

        private void InternalDisplayMessage(BonusTransaction transaction)
        {
            if (transaction.MessageDuration == TimeSpan.MaxValue)
            {
                _bus.Subscribe<CashOutStartedEvent>(this, _ => HandleStateChange(this, transaction));
                _bus.Subscribe<GamePlayInitiatedEvent>(this, _ => HandleStateChange(this, transaction));
            }

            DisplayMessage(transaction);
        }

        private void HandleStateChange(GameWinBonusStrategy @this, BonusTransaction transaction)
        {
            RemoveMessage(transaction);
            _bus.UnsubscribeAll(@this);
        }

        private void UpdateMeters(
            BonusTransaction transaction,
            long cashableAmount,
            long nonCashAmount,
            long promoAmount)
        {
            switch (transaction.PayMethod)
            {
                case PayMethod.Handpay when transaction.IsAttendantPaid(_transactions):
                    _meters.GetMeter(transaction.GameId, transaction.Denom, BonusMeters.HandPaidGameWinBonusAmount)
                        .Increment(cashableAmount + nonCashAmount + promoAmount);
                    _meters.GetMeter(transaction.GameId, transaction.Denom, BonusMeters.HandPaidGameWinBonusCount)
                        .Increment(1);
                    break;
                default:
                    _meters.GetMeter(transaction.GameId, transaction.Denom, BonusMeters.EgmPaidGameWinBonusAmount)
                        .Increment(cashableAmount + nonCashAmount + promoAmount);
                    _meters.GetMeter(transaction.GameId, transaction.Denom, BonusMeters.EgmPaidGameWinBonusCount)
                        .Increment(1);
                    break;
            }
        }
    }
}