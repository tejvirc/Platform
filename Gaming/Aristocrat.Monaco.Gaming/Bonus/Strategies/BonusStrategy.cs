namespace Aristocrat.Monaco.Gaming.Bonus.Strategies
{
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Transactions;
    using Accounting.Contracts.TransferOut;
    using Accounting.Contracts.Vouchers;
    using Accounting.Contracts.Wat;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Common;
    using Contracts;
    using Contracts.Bonus;
    using Contracts.Meters;
    using Contracts.Payment;
    using Contracts.Session;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using Payment;
    using Runtime;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public abstract class BonusStrategy
    {
        private const string LegacyBonusHostTransactionPrefix = "LEGACY";
        protected static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().ReflectedType);

        private readonly IBank _bank;
        private readonly IEventBus _bus;
        private readonly IMessageDisplay _messages;
        private readonly IGameMeterManager _meters;
        private readonly IPlayerService _players;
        private readonly IPropertiesManager _properties;
        private readonly IRuntime _runtime;
        private readonly ITransactionHistory _transactions;
        private readonly IGameHistory _history;
        private readonly ITransferOutHandler _transferHandler;
        private readonly IPersistentStorageManager _storage;
        private readonly IPaymentDeterminationProvider _bonusPayDetermination;

        protected BonusStrategy(
            IPropertiesManager properties,
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
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _transferHandler = transferHandler ?? throw new ArgumentNullException(nameof(transferHandler));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _history = history ?? throw new ArgumentNullException(nameof(history));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _messages = messages ?? throw new ArgumentNullException(nameof(messages));
            _players = players ?? throw new ArgumentNullException(nameof(players));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _bonusPayDetermination = bonusPayDetermination ?? throw new ArgumentNullException(nameof(bonusPayDetermination));

            _bonusPayDetermination.BonusHandler ??= new BonusPaymentDeterminationHandler(_properties, _bank);

        }

        protected BonusTransaction ToTransaction(int deviceId, IBonusRequest request)
        {
            var gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
            var denom = _properties.GetValue(GamingConstants.SelectedDenom, 0L);

            var bonus = new BonusTransaction(
                deviceId,
                DateTime.UtcNow,
                request.BonusId,
                request.CashableAmount,
                request.NonCashAmount,
                request.PromoAmount,
                gameId,
                denom,
                request.PayMethod)
            {
                Mode = request.Mode,
                IdRequired = request.IdRequired,
                IdNumber = request.IdNumber,
                PlayerId = request.PlayerId,
                Message = request.Message,
                MessageDuration = request.MessageDuration,
                SourceID = request.SourceID,
                JackpotNumber = request.JackpotNumber,
                Protocol = request.Protocol,
            };

            _transactions.AddTransaction(bonus);

            _bus.Publish(new BonusPendingEvent(bonus));

            return bonus;
        }

        protected bool Validate(BonusTransaction transaction, IBonusRequest request)
        {
            if (request.Exception != BonusException.None)
            {
                Failed(transaction, request.Exception);

                return false;
            }

            if (transaction.IdRequired)
            {
                if (!_players.HasActiveSession)
                {
                    Failed(transaction, BonusException.NoPlayerId);

                    return false;
                }

                if (!string.IsNullOrEmpty(transaction.IdNumber) &&
                    !_players.ActiveSession.Player.Number.Equals(transaction.IdNumber))
                {
                    Failed(transaction, BonusException.InvalidPlayerId);

                    return false;
                }

                if (!string.IsNullOrEmpty(transaction.PlayerId) &&
                    !_players.ActiveSession.Player.PlayerId.Equals(transaction.PlayerId))
                {
                    Failed(transaction, BonusException.InvalidPlayerId);

                    return false;
                }
            }

            // When requested this will verify that the current player is eligible.  Some duration between game start and now
            if (!request.OverrideEligibility && _history.CurrentLog != null &&
                DateTime.UtcNow - _history.CurrentLog.StartDateTime > request.EligibilityTimer)
            {
                Failed(transaction, BonusException.Failed, BonusExceptionInfo.Ineligible);

                return false;
            }

            if (request is IAwardLimit limited)
            {
                var totalAmount = transaction.Mode == BonusMode.WagerMatch
                    ? transaction.WagerMatchAwardAmount
                    : transaction.TotalAmount;

                if (limited.DisplayLimit > 0 && totalAmount > limited.DisplayLimit)
                {
                    Failed(transaction, BonusException.Failed,
                        transaction.Mode == BonusMode.WagerMatch || transaction.Mode == BonusMode.WagerMatchAllAtOnce ? BonusExceptionInfo.WagerMatchLimitExceeded : BonusExceptionInfo.LimitExceeded);

                    transaction.Message = limited.DisplayLimitText;
                    transaction.MessageDuration = limited.DisplayLimitTextDuration;

                    DisplayMessage(transaction);

                    _bus.Publish(new DisplayLimitExceededEvent(transaction));

                    return false;
                }
            }

            return true;
        }

        protected (bool success, TaskCompletionSource<bool> pending) Pay(
            BonusTransaction transaction,
            Guid transactionId,
            long cashableAmount,
            long nonCashAmount,
            long promoAmount,
            TransferOutReason reason = TransferOutReason.BonusPay)
        {
            TaskCompletionSource<bool> pendingPayout = null;
            try
            {
                switch (GetPayMethod(transaction, Total()))
                {
                    case PayMethod.Any:
                    case PayMethod.Credit:
                        PayToCredits(transactionId, cashableAmount, nonCashAmount, promoAmount);
                        CompletePayment(transaction, cashableAmount, nonCashAmount, promoAmount);
                        break;
                    case PayMethod.Handpay:
                        pendingPayout = PayTo<IHandpayProvider>(
                            transaction,
                            transactionId,
                            cashableAmount,
                            nonCashAmount,
                            promoAmount,
                            reason);
                        break;
                    case PayMethod.Voucher:
                        pendingPayout = PayTo<IVoucherOutProvider>(
                            transaction,
                            transactionId,
                            cashableAmount,
                            nonCashAmount,
                            promoAmount,
                            reason);
                        break;
                    case PayMethod.Wat:
                        pendingPayout = PayTo<IWatOffProvider>(
                            transaction,
                            transactionId,
                            cashableAmount,
                            nonCashAmount,
                            promoAmount,
                            reason);
                        break;
                }

                Logger.Info($"Paid bonus: {transaction}");
            }
            catch (TransactionException ex)
            {
                transaction.Message = BonusException.PayMethodNotAvailable.GetDescription(typeof(BonusException));

                _messages.RemoveMessage(transaction.DisplayMessageId);

                Failed(transaction, BonusException.PayMethodNotAvailable);

                Logger.Error($"Failed to pay bonus: {transaction}", ex);

                return (false, null);
            }
            catch (Exception ex)
            {
                Failed(transaction, BonusException.Failed);

                Logger.Error($"Failed to pay bonus: {transaction}", ex);

                return (false, null);
            }

            return (true, pendingPayout);

            long Total()
            {
                return cashableAmount + nonCashAmount + promoAmount;
            }
        }

        protected TaskCompletionSource<bool> RecoverOrPay<T>(
            BonusTransaction transaction,
            Guid transactionId,
            long cashableAmount,
            long nonCashAmount,
            long promoAmount,
            TransferOutReason reason)
            where T : ITransferOutProvider
        {
            var (results, pending) = RecoverTransfer(transaction, transactionId, cashableAmount, nonCashAmount, promoAmount);
            return results.Total == nonCashAmount + cashableAmount + promoAmount
                ? pending
                : PayTo<T>(
                    transaction,
                    transactionId,
                    cashableAmount - results.TransferredCashable,
                    nonCashAmount - results.TransferredNonCash,
                    promoAmount - results.TransferredPromo,
                    reason);
        }

        protected (TransferResult, TaskCompletionSource<bool> pending) RecoverTransfer(
            BonusTransaction transaction,
            Guid transactionId,
            long cashableAmount,
            long nonCashAmount,
            long promoAmount)
        {
            var currentPaidCashable = transaction.PaidCashableAmount;
            var currentPaidNonCash = transaction.PaidNonCashAmount;
            var currentPaidPromo = transaction.PaidPromoAmount;
            var traceId = _transferHandler.Recover(transactionId);
            if (traceId != Guid.Empty)
            {
                return (new TransferResult(cashableAmount, promoAmount, nonCashAmount), GetTransferCompletionSource(
                    transaction,
                    cashableAmount - currentPaidCashable,
                    nonCashAmount - currentPaidNonCash,
                    promoAmount - currentPaidPromo,
                    traceId));
            }

            var (cashable, promo, nonCash) = _transactions.RecallTransactions().OfType<ITransactionContext>()
                .Where(
                    x => (x as ITransactionConnector)?.AssociatedTransactions.Contains(transaction.TransactionId) ?? false)
                .Select(x => x.GetTransactionAmounts()).Aggregate(
                    (current, next) => (current.cashable + next.cashable, current.promo + next.promo,
                        current.nonCash + next.nonCash));
            var remainingCashable = cashableAmount - cashable;
            var remainingPromo = promoAmount - promo;
            var remainingNonCash = nonCashAmount - nonCash;
            transaction.PayMethod = GetActualBonusPayMethod(
                _transactions.RecallTransactions()
                    .FirstOrDefault(
                        x => (x as ITransactionConnector)?.AssociatedTransactions.Contains(transaction.TransactionId) ??
                             false),
                transaction);
            if (remainingCashable + remainingPromo + remainingNonCash > 0)
            {
                CompletePayment(
                    transaction,
                    cashable - currentPaidCashable,
                     nonCash - currentPaidNonCash,
                     promo - currentPaidPromo);
                return (new TransferResult(cashable, nonCash, promo), null);
            }

            var res = new TaskCompletionSource<bool>();
            res.TrySetResult(true);
            CompletePayment(
                transaction,
                 cashableAmount - currentPaidCashable,
                 nonCashAmount - currentPaidNonCash,
                 promoAmount - currentPaidPromo); // Commit only the amount that has not been paid yet

            return (new TransferResult(cashableAmount, promoAmount, nonCashAmount), res);
        }

        protected (bool success, TaskCompletionSource<bool> pending) Recover(
            BonusTransaction transaction,
            Guid transactionId,
            long cashableAmount,
            long nonCashAmount,
            long promoAmount,
            TransferOutReason reason = TransferOutReason.BonusPay)
        {
            TaskCompletionSource<bool> pendingPayout = null;

            try
            {
                switch (GetPayMethod(transaction, Total()))
                {
                    case PayMethod.Any:
                    case PayMethod.Credit:
                        return Pay(
                            transaction,
                            transactionId,
                            cashableAmount,
                            nonCashAmount,
                            promoAmount); // Nothing to recover here just pay
                    case PayMethod.Handpay:
                        pendingPayout = RecoverOrPay<IHandpayProvider>(
                            transaction,
                            transactionId,
                            cashableAmount,
                            nonCashAmount,
                            promoAmount,
                            reason);
                        break;
                    case PayMethod.Voucher:
                        pendingPayout = RecoverOrPay<IVoucherOutProvider>(
                            transaction,
                            transactionId,
                            cashableAmount,
                            nonCashAmount,
                            promoAmount,
                            reason);
                        break;
                    case PayMethod.Wat:
                        pendingPayout = RecoverOrPay<IWatOffProvider>(
                            transaction,
                            transactionId,
                            cashableAmount,
                            nonCashAmount,
                            promoAmount,
                            reason);
                        break;
                }

                Logger.Info($"Recovered bonus: {transaction}");
            }
            catch (TransactionException ex)
            {
                transaction.Message = BonusException.PayMethodNotAvailable.GetDescription(typeof(BonusException));

                _messages.RemoveMessage(transaction.DisplayMessageId);

                Failed(transaction, BonusException.PayMethodNotAvailable);

                Logger.Error($"Failed to pay bonus: {transaction}", ex);

                return (false, null);
            }
            catch (Exception ex)
            {
                Failed(transaction, BonusException.Failed);

                Logger.Error($"Failed to pay bonus: {transaction}", ex);

                return (false, null);
            }

            return (true, pendingPayout);

            long Total()
            {
                return cashableAmount + nonCashAmount + promoAmount;
            }
        }

        protected virtual void CompletePayment(
            BonusTransaction transaction,
            long cashableAmount,
            long nonCashAmount,
            long promoAmount)
        {
            transaction.PaidCashableAmount += cashableAmount;
            transaction.PaidPromoAmount += promoAmount;
            transaction.PaidNonCashAmount += nonCashAmount;

            _transactions.UpdateTransaction(transaction);

            UpdateMeters(transaction, cashableAmount, nonCashAmount, promoAmount);

            UpdateDeductibleMeters(transaction);
        }

        protected void Commit(BonusTransaction transaction, BonusException exception = BonusException.None, BonusExceptionInfo exceptionInfo = BonusExceptionInfo.None)
        {
            transaction.PaidDateTime = DateTime.UtcNow;
            transaction.State = BonusState.Committed;
            transaction.Exception = (int)exception;
            transaction.ExceptionInformation = (int)exceptionInfo;

            Update(transaction);

            if (exception == BonusException.None)
            {
                _bus.Publish(new BonusAwardedEvent(transaction));
            }
        }

        protected void Update(BonusTransaction transaction)
        {
            transaction.LastUpdate = DateTime.UtcNow;

            _transactions.UpdateTransaction(transaction);
        }

        protected bool InternalCancel(BonusTransaction transaction)
        {
            if (transaction.State != BonusState.Pending)
            {
                return false;
            }

            transaction.PaidCashableAmount = 0;
            transaction.PaidNonCashAmount = 0;
            transaction.PaidPromoAmount = 0;

            Commit(transaction, BonusException.Cancelled);

            _bus.Publish(new BonusCancelledEvent(transaction));

            Logger.Info($"Cancelled pending bonus: {transaction}");

            return true;
        }

        protected void Failed(BonusTransaction transaction, BonusException exception, BonusExceptionInfo exceptionInfo = BonusExceptionInfo.None)
        {
            Commit(transaction, exception, exceptionInfo);

            _bus.Publish(new BonusFailedEvent(transaction));

            Logger.Info($"Bonus award failed: {transaction}");
        }

        protected void DisplayMessage(BonusTransaction transaction)
        {
            var displayMessage = new DisplayableMessage(
                Message,
                DisplayableMessageClassification.Informative,
                DisplayableMessagePriority.Normal,
                typeof(BonusAwardedEvent),
                transaction.DisplayMessageId);

            if (transaction.MessageDuration == TimeSpan.MaxValue)
            {
                _messages.DisplayMessage(displayMessage);
            }
            else if (transaction.MessageDuration != TimeSpan.Zero)
            {
                _messages.DisplayMessage(displayMessage, (int)transaction.MessageDuration.TotalMilliseconds);
            }

            string Message()
            {
                return !string.IsNullOrEmpty(transaction.Message)
                   ? transaction.Message
                   : Localizer.For(CultureFor.Player).FormatString(ResourceKeys.BonusAwardTitle) + " " +
                     (transaction.CashableAmount + transaction.NonCashAmount + transaction.PromoAmount)
                     .MillicentsToDollars().FormattedCurrencyString();
            }
        }

        protected void RemoveMessage(BonusTransaction transaction)
        {
            _messages.RemoveMessage(transaction.DisplayMessageId);

            transaction.DisplayMessageId = Guid.Empty;

            Update(transaction);
        }

        protected PayMethod GetPayMethod(BonusTransaction transaction, long totalAmount)
        {
            var payMethod = _bonusPayDetermination.BonusHandler.GetBonusPayMethod(transaction, totalAmount);
            return payMethod;
        }

        private void PayToCredits(
            Guid transactionId,
            long cashableAmount,
            long nonCashAmount,
            long promoAmount)
        {
            _bank.Deposit(AccountType.Cashable, cashableAmount, transactionId);
            _bank.Deposit(AccountType.NonCash, nonCashAmount, transactionId);
            _bank.Deposit(AccountType.Promo, promoAmount, transactionId);

            _runtime.UpdateBalance(_bank.QueryBalance().MillicentsToCents());
        }

        private TaskCompletionSource<bool> PayTo<T>(
            BonusTransaction transaction,
            Guid transactionId,
            long cashableAmount,
            long nonCashAmount,
            long promoAmount,
            TransferOutReason reason)
            where T : ITransferOutProvider
        {
            var traceId = Guid.NewGuid();

            _history.AppendCashOut(
                new CashOutInfo
                {
                    Amount = cashableAmount + nonCashAmount + promoAmount,
                    Reason = reason,
                    TraceId = traceId,
                    AssociatedTransactions = new[] { transaction.TransactionId }
                });

            if (!_transferHandler.TransferOutWithContinuation<T>(
                transactionId,
                cashableAmount,
                promoAmount,
                nonCashAmount,
                new[] { transaction.TransactionId },
                reason,
                traceId))
            {
                throw new TransactionException("Failed to initiate payout");
            }

            return GetTransferCompletionSource(transaction, cashableAmount, nonCashAmount, promoAmount, traceId);
        }

        private TaskCompletionSource<bool> GetTransferCompletionSource(
            BonusTransaction transaction,
            long cashableAmount,
            long nonCashAmount,
            long promoAmount,
            Guid traceId)
        {
            var paid = new TaskCompletionSource<bool>();

            _bus.Subscribe<TransferOutCompletedEvent>(
                this,
                evt =>
                {
                    if (evt.TraceId != traceId)
                    {
                        return;
                    }

                    transaction.PayMethod = GetActualBonusPayMethod(
                        _transactions.RecallTransactions()
                            .FirstOrDefault(x => (x as ITransactionContext)?.TraceId == traceId),
                        transaction);
                    using (var scope = _storage.ScopedTransaction())
                    {
                        CompletePayment(transaction, cashableAmount, nonCashAmount, promoAmount);
                        scope.Complete();
                    }

                    paid.TrySetResult(true);
                    UnsubscribeTransferEvents();
                });

            _bus.Subscribe<TransferOutFailedEvent>(
                this,
                evt =>
                {
                    if (evt.TraceId != traceId)
                    {
                        return;
                    }

                    using (var scope = _storage.ScopedTransaction())
                    {
                        _history.CompleteCashOut(traceId);

                        transaction.Message = BonusException.PayMethodNotAvailable.GetDescription(typeof(BonusException));

                        _messages.RemoveMessage(transaction.DisplayMessageId);

                        Failed(transaction, BonusException.PayMethodNotAvailable);
                        scope.Complete();
                    }
                    paid.TrySetResult(false);
                    UnsubscribeTransferEvents();
                });

            return paid;
        }

        private PayMethod GetActualBonusPayMethod(ITransaction transaction, BonusTransaction bonus)
        {
            switch (transaction)
            {
                case HandpayTransaction _:
                    return PayMethod.Handpay;
                case VoucherOutTransaction _:
                    return PayMethod.Voucher;
                case WatTransaction _:
                    return PayMethod.Wat;
                default:
                    return bonus.PayMethod;
            }
        }

        private void UpdateMeters(
            BonusTransaction transaction,
            long cashableAmount,
            long nonCashAmount,
            long promoAmount)
        {
            if (transaction.Mode == BonusMode.GameWin)
            {
                return;
            }

            if (transaction.PayMethod == PayMethod.Handpay && transaction.IsAttendantPaid(_transactions))
            {
                _meters.GetMeter(transaction.GameId, transaction.Denom, GamingMeters.HandPaidBonusCashableInAmount).Increment(cashableAmount);
                _meters.GetMeter(transaction.GameId, transaction.Denom, GamingMeters.HandPaidBonusNonCashInAmount).Increment(nonCashAmount);
                _meters.GetMeter(transaction.GameId, transaction.Denom, GamingMeters.HandPaidBonusPromoInAmount).Increment(promoAmount);

                _meters.GetMeter(BonusMeters.HandPaidBonusCount).Increment(1);
            }
            else
            {
                _meters.GetMeter(transaction.GameId, transaction.Denom, GamingMeters.EgmPaidBonusCashableInAmount).Increment(cashableAmount);
                _meters.GetMeter(transaction.GameId, transaction.Denom, GamingMeters.EgmPaidBonusNonCashInAmount).Increment(nonCashAmount);
                _meters.GetMeter(transaction.GameId, transaction.Denom, GamingMeters.EgmPaidBonusPromoInAmount).Increment(promoAmount);

                _meters.GetMeter(BonusMeters.EgmPaidBonusCount).Increment(1);
            }
        }

        private void UpdateDeductibleMeters(BonusTransaction transaction)
        {
            if (!transaction.BonusId.StartsWith(LegacyBonusHostTransactionPrefix))
            {
                return;
            }

            if (transaction.PayMethod == PayMethod.Handpay && transaction.IsAttendantPaid(_transactions))
            {
                switch (transaction.Mode)
                {
                    case BonusMode.NonDeductible:
                        _meters.GetMeter(transaction.GameId, transaction.Denom, GamingMeters.HandPaidBonusNonDeductibleAmount).Increment(transaction.CashableAmount);
                        _meters.GetMeter(transaction.GameId, transaction.Denom, GamingMeters.HandPaidBonusNonDeductibleCount).Increment(1);
                        break;
                    case BonusMode.WagerMatchAllAtOnce:
                        _meters.GetMeter(transaction.GameId, transaction.Denom, GamingMeters.HandPaidBonusWagerMatchAmount).Increment(transaction.CashableAmount);
                        _meters.GetMeter(transaction.GameId, transaction.Denom, GamingMeters.WagerMatchBonusCount).Increment(1);
                        break;
                    default:
                        _meters.GetMeter(transaction.GameId, transaction.Denom, GamingMeters.HandPaidBonusDeductibleAmount).Increment(transaction.CashableAmount);
                        _meters.GetMeter(transaction.GameId, transaction.Denom, GamingMeters.HandPaidBonusDeductibleCount).Increment(1);
                        break;
                }
            }
            else
            {
                switch (transaction.Mode)
                {
                    case BonusMode.NonDeductible:
                        _meters.GetMeter(transaction.GameId, transaction.Denom, GamingMeters.EgmPaidBonusNonDeductibleAmount).Increment(transaction.CashableAmount);
                        _meters.GetMeter(transaction.GameId, transaction.Denom, GamingMeters.EgmPaidBonusNonDeductibleCount).Increment(1);
                        break;
                    case BonusMode.WagerMatchAllAtOnce:
                        _meters.GetMeter(transaction.GameId, transaction.Denom, GamingMeters.EgmPaidBonusWagerMatchAmount).Increment(transaction.CashableAmount);
                        _meters.GetMeter(transaction.GameId, transaction.Denom, GamingMeters.WagerMatchBonusCount).Increment(1);
                        break;
                    default:
                        _meters.GetMeter(transaction.GameId, transaction.Denom, GamingMeters.EgmPaidBonusDeductibleAmount).Increment(transaction.CashableAmount);
                        _meters.GetMeter(transaction.GameId, transaction.Denom, GamingMeters.EgmPaidBonusDeductibleCount).Increment(1);
                        break;
                }
            }
        }

        private void UnsubscribeTransferEvents()
        {
            Task.Run(() =>
            {
                _bus.Unsubscribe<TransferOutFailedEvent>(this);
                _bus.Unsubscribe<TransferOutCompletedEvent>(this);
            });
        }
    }
}