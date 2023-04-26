namespace Aristocrat.Monaco.Gaming.Bonus.Strategies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Common;
    using Contracts;
    using Contracts.Bonus;
    using Contracts.Meters;
    using Contracts.Payment;
    using Contracts.Session;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Runtime;

    public class WagerMatch : BonusStrategy, IBonusStrategy
    {
        private readonly IEventBus _bus;
        private readonly IGamePlayState _gamePlay;
        private readonly IGameHistory _history;
        private readonly IGameMeterManager _meters;
        private readonly IPersistentStorageManager _storage;
        private readonly ITransactionHistory _transactions;

        public WagerMatch(
            IPersistentStorageManager storage,
            ITransactionHistory transactions,
            IGamePlayState gamePlay,
            IGameHistory history,
            IGameMeterManager meters,
            IRuntime runtime,
            IEventBus bus,
            IPropertiesManager properties,
            IBank bank,
            ITransferOutHandler transferHandler,
            IMessageDisplay messages,
            IPlayerService players,
            IPaymentDeterminationProvider paymentDeterminationProvider)
            : base(properties, bank, transferHandler, transactions, history, meters, runtime, bus, messages, players, storage, paymentDeterminationProvider)

        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _gamePlay = gamePlay ?? throw new ArgumentNullException(nameof(gamePlay));
            _history = history ?? throw new ArgumentNullException(nameof(history));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public BonusTransaction CreateTransaction<T>(int deviceId, T request) where T : IBonusRequest
        {
            using (var scope = _storage.ScopedTransaction())
            {
                var transaction = ToTransaction(deviceId, request);

                if (Validate(transaction, request))
                {
                    DisplayMessage(transaction);
                }

                scope.Complete();

                return transaction;
            }
        }

        public bool CanPay(BonusTransaction transaction, Guid transactionId)
        {
            return transaction.State == BonusState.Pending && _gamePlay.UncommittedState == PlayState.GameEnded && transactionId != Guid.Empty;
        }

        public async Task<IContinuationContext> Pay(BonusTransaction transaction, Guid transactionId, IContinuationContext context)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            if (!CanPay(transaction, transactionId))
            {
                return null;
            }

            if (CommitIfConsumed(transaction))
            {
                return null;
            }

            var previousAward = context as ContinuationContext;

            if (previousAward != null && previousAward.Context == 0)
            {
                return context;
            }

            var gameRound = _history.CurrentLog;

            var cashable = GetAuthorizedAmount(transaction.CashableAmount);
            var nonCash = GetAuthorizedAmount(transaction.NonCashAmount);
            var promo = GetAuthorizedAmount(transaction.PromoAmount);

            transaction.GameId = gameRound.GameId;
            transaction.Denom = gameRound.DenomId;

            _bus.Publish(new BonusStartedEvent(transaction));

            using (var scope = _storage.ScopedTransaction())
            {
                PersistLastAuthorizedAmount(transaction, nonCash, cashable, promo);

                var (_, pending) = Pay(transaction, transactionId, cashable, nonCash, promo);

                // Must be committed before awaiting the pending transfer if there is one
                scope.Complete();

                if (pending != null)
                {
                    await pending.Task;
                }
            }
            return new ContinuationContext(Remaining(cashable + nonCash + promo));

            long GetAuthorizedAmount(long requested)
            {
                if (requested == 0)
                {
                    return 0;
                }

                return Math.Min(
                    requested - transaction.PaidAmount,
                    previousAward?.Context ?? gameRound.FinalWager.CentsToMillicents());
            }

            long Remaining(long paid)
            {
                return (previousAward?.Context ?? gameRound.FinalWager.CentsToMillicents()) - paid;
            }
        }

        private void PersistLastAuthorizedAmount(BonusTransaction transaction, long nonCash, long cashable, long promo)
        {
            transaction.LastAuthorizedNonCashAmount = nonCash;
            transaction.LastAuthorizedCashableAmount = cashable;
            transaction.LastAuthorizedPromoAmount = promo;
            _transactions.UpdateTransaction(transaction);
        }

        public bool Cancel(BonusTransaction transaction)
        {
            return Cancel(transaction, CancellationReason.Any);
        }

        public bool Cancel(BonusTransaction transaction, CancellationReason reason)
        {
            if (reason == CancellationReason.ZeroCredits)
            {
                return false;
            }

            if (reason == CancellationReason.IdInvalidated &&
                string.IsNullOrEmpty(transaction.PlayerId) && string.IsNullOrEmpty(transaction.IdNumber))
            {
                return false;
            }

            RemoveMessage(transaction);

            if (transaction.PaidAmount > 0)
            {
                Commit(transaction);

                return true;
            }

            return InternalCancel(transaction);
        }

        public async Task Recover(BonusTransaction transaction, Guid transactionId)
        {
            if (transaction.State == BonusState.Pending)
            {
                DisplayMessage(transaction);
            }

            TaskCompletionSource<bool> pending = null;

            using var scope = _storage.ScopedTransaction();

            long totalAmount = transaction.LastAuthorizedCashableAmount
                               + transaction.LastAuthorizedNonCashAmount
                               + transaction.LastAuthorizedPromoAmount;

            try
            {
                switch (GetPayMethod(transaction, totalAmount))
                {
                    case PayMethod.Handpay:
                    case PayMethod.Voucher:
                    case PayMethod.Wat:
                        (_, pending) = RecoverTransfer(
                            transaction,
                            transactionId,
                            transaction.LastAuthorizedCashableAmount,
                            transaction.LastAuthorizedNonCashAmount,
                            transaction.LastAuthorizedPromoAmount);
                        break;
                }
            }
            catch (TransactionException ex)
            {
                transaction.Message = BonusException.PayMethodNotAvailable.GetDescription(typeof(BonusException));

                Failed(transaction, BonusException.PayMethodNotAvailable);

                Logger.Error($"Failed to pay bonus: {transaction}", ex);
            }
            catch (Exception ex)
            {
                Failed(transaction, BonusException.Failed);

                Logger.Error($"Failed to pay bonus: {transaction}", ex);
            }

            scope.Complete();

            if (pending != null)
            {
                await pending.Task;
            }

        }

        protected override void CompletePayment(BonusTransaction transaction, long cashableAmount, long nonCashAmount, long promoAmount)
        {
            base.CompletePayment(transaction, cashableAmount, nonCashAmount, promoAmount);

            UpdateMeters(transaction, cashableAmount + nonCashAmount + promoAmount);

            _transactions.UpdateTransaction(transaction);

            _bus.Publish(new PartialBonusPaidEvent(transaction, cashableAmount, nonCashAmount, promoAmount));

            CommitIfConsumed(transaction);
        }

        private bool CommitIfConsumed(BonusTransaction transaction)
        {
            if (transaction.WagerMatchAwardAmount - transaction.PaidAmount > 0)
            {
                return false;
            }

            RemoveMessage(transaction);

            Commit(transaction);

            return true;
        }

        private void UpdateMeters(BonusTransaction transaction, long paid)
        {
            if (paid <= 0)
            {
                return;
            }

            _meters.GetMeter(transaction.GameId, transaction.Denom, GamingMeters.WagerMatchBonusCount).Increment(1);
            if (transaction.PayMethod == PayMethod.Handpay && transaction.IsAttendantPaid(_transactions))
            {
                _meters.GetMeter(transaction.GameId, transaction.Denom, GamingMeters.HandPaidBonusWagerMatchAmount).Increment(paid);
            }
            else
            {
                _meters.GetMeter(transaction.GameId, transaction.Denom, GamingMeters.EgmPaidBonusWagerMatchAmount).Increment(paid);
            }
        }

        private class ContinuationContext : IContinuationContext<long>
        {
            public ContinuationContext(long remaining)
            {
                Context = remaining;
            }

            public long Context { get; }
        }
    }
}