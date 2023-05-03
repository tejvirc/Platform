namespace Aristocrat.Monaco.Gaming.Bonus.Strategies
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Contracts;
    using Contracts.Bonus;
    using Contracts.Meters;
    using Contracts.Payment;
    using Contracts.Session;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Newtonsoft.Json;
    using Runtime;

    public class MultipleJackpotTime : BonusStrategy, IBonusStrategy
    {
        private readonly IEventBus _bus;
        private readonly IPropertiesManager _properties;
        private readonly IPlayerBank _playerBank;
        private readonly IGamePlayState _gamePlay;
        private readonly IGameHistory _history;
        private readonly IGameMeterManager _meters;
        private readonly IPersistentStorageManager _storage;
        private readonly ITransactionHistory _transactions;

        public MultipleJackpotTime(IPersistentStorageManager storage,
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
            IPlayerBank playerBank,
            IPaymentDeterminationProvider paymentDeterminationProvider,
            IMaxWinOverlayService maxWinOverlayService)
            : base(properties, bank, transferHandler, transactions, history, meters, runtime, bus, messages, players, storage, paymentDeterminationProvider, maxWinOverlayService)

        {
            // TODO: Prevent changing the game, wager, denomination, coins bet per line or lines played until the bonus mode terminates
            // TODO: Prevent secondary games while the bonus is active

            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _gamePlay = gamePlay ?? throw new ArgumentNullException(nameof(gamePlay));
            _history = history ?? throw new ArgumentNullException(nameof(history));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _playerBank = playerBank ?? throw new ArgumentNullException(nameof(playerBank));
        }

        public BonusTransaction CreateTransaction<T>(int deviceId, T request) where T : IBonusRequest
        {
            if (request is not MultipleJackpotTimeBonus mjtBonus)
            {
                throw new ArgumentException(nameof(request));
            }

            using var scope = _storage.ScopedTransaction();
            var transaction = ToTransaction(deviceId, mjtBonus);

            transaction.MjtNumberOfGames = mjtBonus.Games;
            transaction.MjtMinimumWin = mjtBonus.MinimumWin;
            transaction.MjtMaximumWin = mjtBonus.MaximumWin;
            transaction.MjtWinMultiplier = mjtBonus.WinMultiplier;
            transaction.MjtWagerRestriction = mjtBonus.WagerRestriction;

            // Save off the original request. This will be used to drive the payment later
            transaction.Request = JsonConvert.SerializeObject(request);

            if (transaction.TotalAmount > 0)
            {
                Failed(transaction, BonusException.Failed, BonusExceptionInfo.InvalidAwardAmount);
            }
            else if (transaction.MjtNumberOfGames == 0 && mjtBonus.End == null)
            {
                Failed(transaction, BonusException.Failed);
            }
            else if ((mjtBonus.AutoPlay || mjtBonus.TimeoutRule == TimeoutRule.AutoStart) &&
                     !_properties.GetValue(GamingConstants.AutoPlayAllowed, true))
            {
                Failed(transaction, BonusException.Failed, BonusExceptionInfo.AutoPlayNotAllowed);
            }
            else if (mjtBonus.RejectLowCredits && mjtBonus.Games > 0 &&
                     _history.CurrentLog?.FinalWager.CentsToMillicents() * mjtBonus.Games > _playerBank.Balance)
            {
                Failed(transaction, BonusException.Failed, BonusExceptionInfo.InsufficientFunds);
            }
            else
            {
                Validate(transaction, request);
            }

            scope.Complete();

            return transaction;
        }

        public bool CanPay(BonusTransaction transaction)
        {
            var gameRound = _history.CurrentLog;

            // NOTE: Using GameEnded isn't technically correct, but PayGameResults doesn't fire if there is no win which prevents updating the wager and games played
            //  This may need to be changed, but it's currently not how the state machine works
            return transaction.MjtWinMultiplier > 1 && transaction.State == BonusState.Pending &&
                   (_gamePlay.CurrentState == PlayState.PayGameResults || (_gamePlay.CurrentState == PlayState.GameEnded && !transaction.AssociatedTransactions.Contains(gameRound.TransactionId)));
        }

        public async Task<IContinuationContext> Pay(BonusTransaction transaction, Guid transactionId, IContinuationContext context)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            // This allows us to skip this payment if the previous MJT instance is active (consecutive MJT awards)
            if (context is ContinuationContext { Context: true } previousAward)
            {
                return previousAward;
            }

            var gameRound = _history.CurrentLog;

            if (!CanPay(transaction))
            {
                return null;
            }

            var request = JsonConvert.DeserializeObject<MultipleJackpotTimeBonus>(transaction.Request);

            if (!HasStarted(transaction, request))
            {
                return null;
            }

            if (CommitIfComplete(transaction, request))
            {
                return null;
            }

            // If this is the first wager it will denote the start of the bonus
            if (transaction.MjtAmountWagered == 0)
            {
                transaction.MjtRequiredWager = gameRound.FinalWager.CentsToMillicents();

                _bus.Publish(new MultipleJackpotTimeStartedEvent(transaction));

                DisplayMessage(transaction);

                UpdateAutoPlay(request, true);
            }

            if (!IsWagerValid(transaction))
            {
                // If the wager is not valid (a mismatch) the bonus needs to be terminated (committed)
                Commit(transaction, request);

                return null;
            }

            using (var scope = _storage.ScopedTransaction())
            {
                transaction.GameId = gameRound.GameId;
                transaction.Denom = gameRound.DenomId;

                transaction.MjtAmountWagered += gameRound.InitialWager.CentsToMillicents();
                transaction.MjtBonusGamesPlayed++;

                transaction.AssociatedTransactions =
                    transaction.AssociatedTransactions.Concat(new List<long> { gameRound.TransactionId });

                _transactions.UpdateTransaction(transaction);

                var bonusAmount = GetMultipliedWin();

                (bool _, TaskCompletionSource<bool> pending) result = (true, null);

                if (bonusAmount > 0)
                {
                    _bus.Publish(new BonusStartedEvent(transaction));

                    result = Pay(
                        transaction,
                        transactionId,
                        request.AccountType == AccountType.Cashable ? bonusAmount : 0,
                        request.AccountType == AccountType.NonCash ? bonusAmount : 0,
                        request.AccountType == AccountType.Promo ? bonusAmount : 0);
                }
                else
                {
                    CommitIfComplete(transaction, request);
                }

                // Must be committed before awaiting the pending transfer if there is one
                scope.Complete();

                if (result.pending != null)
                {
                    await result.pending.Task;
                }
            }

            return new ContinuationContext(true);

            long GetMultipliedWin()
            {
                var baseGameWin = gameRound.InitialWin.CentsToMillicents();

                var applicableWin =
                    (request.MinimumWin > 0 && baseGameWin < request.MinimumWin) || (request.MaximumWin > 0 && baseGameWin > request.MaximumWin) ? 0 : baseGameWin;

                if (applicableWin == 0)
                {
                    return 0;
                }

                return (applicableWin * request.WinMultiplier) - baseGameWin;
            }
        }

        public bool Cancel(BonusTransaction transaction)
        {
            return Cancel(transaction, CancellationReason.Any);
        }

        public bool Cancel(BonusTransaction transaction, CancellationReason reason)
        {
            RemoveMessage(transaction);
            UpdateAutoPlay(JsonConvert.DeserializeObject<MultipleJackpotTimeBonus>(transaction.Request), false);
            if (transaction.PaidAmount <= 0)
            {
                return InternalCancel(transaction);
            }

            Commit(transaction);
            return true;
        }

        public Task Recover(BonusTransaction transaction, Guid transactionId)
        {
            if (transaction.State == BonusState.Pending)
            {
                DisplayMessage(transaction);
            }

            return Task.CompletedTask;
        }

        protected override void CompletePayment(BonusTransaction transaction, long cashableAmount, long nonCashAmount, long promoAmount)
        {
            base.CompletePayment(transaction, cashableAmount, nonCashAmount, promoAmount);

            var bonusAmount = cashableAmount + nonCashAmount + promoAmount;

            UpdateMeters(transaction, bonusAmount);

            if (bonusAmount > 0)
            {
                transaction.MjtBonusGamesPaid++;
            }

            _transactions.UpdateTransaction(transaction);

            _bus.Publish(new PartialBonusPaidEvent(transaction, cashableAmount, nonCashAmount, promoAmount));

            var request = JsonConvert.DeserializeObject<MultipleJackpotTimeBonus>(transaction.Request);
            CommitIfComplete(transaction, request);
        }

        private void UpdateMeters(BonusTransaction transaction, long paid)
        {
            _meters.GetMeter(BonusMeters.MjtGamesPlayedCount).Increment(1);
            if (paid <= 0)
            {
                return;
            }

            if (transaction.PayMethod == PayMethod.Handpay && transaction.IsAttendantPaid(_transactions))
            {
                _meters.GetMeter(BonusMeters.HandPaidMjtBonusAmount).Increment(paid);
                _meters.GetMeter(BonusMeters.HandPaidMjtBonusCount).Increment(1);
            }
            else
            {
                _meters.GetMeter(BonusMeters.EgmPaidMjtBonusAmount).Increment(paid);
                _meters.GetMeter(BonusMeters.EgmPaidMjtBonusCount).Increment(1);
            }
        }

        private bool HasStarted(BonusTransaction transaction, MultipleJackpotTimeBonus request)
        {
            var now = DateTime.UtcNow;

            if (request.Start == null || request.Start > now)
            {
                return false;
            }

            if (request.Timeout == TimeSpan.Zero)
            {
                return true;
            }

            // The specification for this isn't entirely clear since the timeout is stated to be the length of time to wait after startTime,
            //  but in the usage it says to handle the timeout when startTime is not specified.  We're going to use the transaction date time if there is no start time specified

            var start = request.Start ?? transaction.TransactionDateTime;

            switch (request.TimeoutRule)
            {
                case TimeoutRule.Ignore:
                    return true;
                case TimeoutRule.AutoStart when start + request.Timeout >= now:
                    if (IsWagerValid(transaction))
                    {
                        return true;
                    }

                    // If the wager is not valid, the bonus must be cancelled
                    Cancel(transaction);
                    return false;
                case TimeoutRule.ExitMode when transaction.MjtAmountWagered == 0 && _history.CurrentLog?.StartDateTime + request.Timeout >= now:
                    Failed(transaction, BonusException.Failed);

                    return false;
                default:
                    return true;
            }
        }

        private void Commit(BonusTransaction transaction, MultipleJackpotTimeBonus request)
        {
            UpdateAutoPlay(request, false);

            RemoveMessage(transaction);

            Commit(transaction);
        }

        private bool CommitIfComplete(BonusTransaction transaction, MultipleJackpotTimeBonus request)
        {
            var complete = false;

            if (request.Games > 0 && transaction.MjtBonusGamesPlayed >= request.Games)
            {
                complete = true;
            }
            else if (request.End != null && request.End <= DateTime.UtcNow)
            {
                complete = true;
            }
            else if (request.EndOnCashOut && _playerBank.Balance == 0)
            {
                complete = true;
            }

            if (!complete)
            {
                return false;
            }

            Commit(transaction, request);

            return true;
        }

        private bool IsWagerValid(BonusTransaction transaction)
        {
            var currentWager = _history.CurrentLog.FinalWager.CentsToMillicents();

            switch (transaction.MjtWagerRestriction)
            {
                case WagerRestriction.MaxBet when GetCurrentMaxWager() == currentWager:
                case WagerRestriction.CurrentBet when transaction.MjtRequiredWager == 0:
                case WagerRestriction.CurrentBet when transaction.MjtRequiredWager > 0 && currentWager == transaction.MjtRequiredWager:
                    return true;
            }

            return false;

            long GetCurrentMaxWager()
            {
                var (game, denom) = _properties.GetActiveGame();

                return game.MaximumWager(denom);
            }
        }

        private void UpdateAutoPlay(MultipleJackpotTimeBonus request, bool enable)
        {
            if (request.AutoPlay || request.TimeoutRule == TimeoutRule.AutoStart)
            {
                _bus.Publish(new AutoPlayRequestedEvent(enable));
            }
        }

        private class ContinuationContext : IContinuationContext<bool>
        {
            public ContinuationContext(bool active)
            {
                Context = active;
            }

            public bool Context { get; }
        }
    }
}