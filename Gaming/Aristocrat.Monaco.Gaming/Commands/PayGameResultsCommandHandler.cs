namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.TransferOut;
    using Application.Contracts.Extensions;
    using Common.PerformanceCounters;
    using Contracts;
    using Contracts.Events;
    using Contracts.Payment;
    using Contracts.Progressives;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Payment;
    using Progressives;

    /// <summary>
    ///     Command handler for the <see cref="PayGameResults" /> command.
    /// </summary>
    [CounterDescription("Pay Results", PerformanceCounterType.AverageTimer32)]
    public class PayGameResultsCommandHandler : ICommandHandler<PayGameResults>
    {
        private readonly IPlayerBank _bank;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly IProgressiveGameProvider _progressiveGame;
        private readonly IGameHistory _gameHistory;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly IPropertiesManager _properties;
        private readonly IGameProvider _gameProvider;
        private readonly IEventBus _eventBus;
        private readonly IPaymentDeterminationProvider _largeWinDetermination;
        private readonly IOutcomeValidatorProvider _outcomeValidation;

        /// <summary>
        ///     Initializes a new instance of the Aristocrat.Monaco.Gaming.Commands.PayGameResultsCommandHandler class.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        public PayGameResultsCommandHandler(
            IEventBus eventBus,
            IPlayerBank bank,
            IPersistentStorageManager persistentStorage,
            IGameHistory gameHistory,
            IPropertiesManager properties,
            IGameProvider gameProvider,
            ICommandHandlerFactory commandFactory,
            IProgressiveGameProvider progressiveGame,
            IPaymentDeterminationProvider largeWinDetermination,
            IOutcomeValidatorProvider outcomeValidation)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _progressiveGame = progressiveGame ?? throw new ArgumentNullException(nameof(progressiveGame));
            _largeWinDetermination = largeWinDetermination ?? throw new ArgumentNullException(nameof(largeWinDetermination));
            _outcomeValidation = outcomeValidation ?? throw new ArgumentNullException(nameof(outcomeValidation));
        }

        /// <inheritdoc />
        public void Handle(PayGameResults command)
        {
            _eventBus.Publish(new ProhibitMoneyInEvent());

            if (command.Win <= 0)
            {
                _outcomeValidation.Handler?.Validate(_gameHistory.CurrentLog);
                return;
            }

            var (_, denomination) = _gameProvider.GetGame(
                _properties.GetValue(GamingConstants.SelectedGameId, 0),
                _properties.GetValue(GamingConstants.SelectedDenom, 0L));

            using var scope = _persistentStorage.ScopedTransaction();

            // Wait until we have exclusive access to Bank as win has been determined
            _bank.WaitForLock();

            _gameHistory.PayResults();

            var jackpotCommits = CreateProgressiveCommits().ToArray();

            if (_properties.GetValue(GamingConstants.MeterFreeGamesIndependently, false))
            {
                if (denomination.SecondaryAllowed)
                {
                    // The flow is different for games that support secondary games.
                    //  The win is not updated as the game progresses when metering free games is enabled
                    _bank.AddWin(command.Win);

                    var checkResult = new CheckResult(command.Win);
                    _commandFactory.Create<CheckResult>().Handle(checkResult);
                    command.PendingTransaction = checkResult.ForcedCashout;
                }
            }
            else
            {
                var winInMillicents = command.Win * GamingConstants.Millicents;

                // Handle jackpots where payment method is forced
                foreach (var jackpot in jackpotCommits.Where(item => item.PayMethod != PayMethod.Any))
                {
                    command.PendingTransaction |= PayJackpot(jackpot);
                    winInMillicents -= jackpot.PaidAmount; // remove the already paid amount
                }

                var largeWinHandler = _largeWinDetermination.Handler ?? new PaymentDeterminationHandler(_properties);
                var paymentResults = largeWinHandler.GetPaymentResults(winInMillicents);
                command.PendingTransaction = HandleLargeWins(paymentResults);
                command.PendingTransaction |= HandleCreditMeterPay(paymentResults);
            }

            _progressiveGame.CommitProgressiveWin(jackpotCommits);
            _outcomeValidation.Handler?.Validate(_gameHistory.CurrentLog);

            scope.Complete();
        }

        private bool HandleCreditMeterPay(IReadOnlyCollection<PaymentDeterminationResult> paymentResults)
        {
            var maxCreditLimit = _properties.GetValue(AccountingConstants.MaxCreditMeter, long.MaxValue);
            var maxCreditStrategy =
                _properties.GetValue(GamingConstants.GameWinMaxCreditCashOutStrategy, MaxCreditCashOutStrategy.Win);
            var amount = paymentResults.Sum(item => item.MillicentsToPayToCreditMeter);
            var largeWinStrategy =
                _properties.GetValue(GamingConstants.GameWinLargeWinCashOutStrategy, LargeWinCashOutStrategy.Handpay);
            if (largeWinStrategy == LargeWinCashOutStrategy.None)
            {
                amount += paymentResults.Sum(item => item.MillicentsToPayUsingLargeWinStrategy);
            }

            var updatedBalance = _bank.Balance + amount;
            if (updatedBalance > maxCreditLimit && maxCreditStrategy != MaxCreditCashOutStrategy.None ||
                maxCreditStrategy == MaxCreditCashOutStrategy.CreditLimit && updatedBalance == maxCreditLimit)
            {
                if (maxCreditStrategy == MaxCreditCashOutStrategy.CreditLimit)
                {
                    _bank.AddWin(amount.MillicentsToCents());
                    amount = maxCreditLimit * (_bank.Balance / maxCreditLimit);
                    return amount > 0 && CashOut(amount, TransferOutReason.CashOut, Guid.NewGuid());
                }

                if (amount > 0)
                {
                    return CashOut(amount, TransferOutReason.CashWin, Guid.NewGuid());
                }
            }
            else
            {
                _bank.AddWin(amount.MillicentsToCents());
            }

            return false;
        }

        private bool HandleLargeWins(IEnumerable<PaymentDeterminationResult> results)
        {
            var pendingTransaction = false;

            var largeWinStrategy =
                _properties.GetValue(GamingConstants.GameWinLargeWinCashOutStrategy, LargeWinCashOutStrategy.Handpay);

            foreach (var result in results)
            {
                switch (largeWinStrategy)
                {
                    case LargeWinCashOutStrategy.Handpay:
                    {
                        if (result.MillicentsToPayUsingLargeWinStrategy > 0)
                        {
                            pendingTransaction |= Handpay(
                                result.MillicentsToPayUsingLargeWinStrategy,
                                result.MillicentsToPayToCreditMeter,
                                TransferOutReason.LargeWin,
                                result.TransactionIdentifier);
                        }

                        break;
                    }
                    case LargeWinCashOutStrategy.Voucher:
                    {
                        _bank.AddWin(result.MillicentsToPayUsingLargeWinStrategy.MillicentsToCents());

                        pendingTransaction |= CashOut(
                            result.MillicentsToPayUsingLargeWinStrategy,
                            TransferOutReason.CashOut,
                            result.TransactionIdentifier,
                            result.MillicentsToPayToCreditMeter);

                        break;
                    }
                }
            }

            return pendingTransaction;
        }

        private bool Handpay(long amount, long wager, TransferOutReason reason, Guid traceId)
        {
            var result = _bank.ForceHandpay(traceId, amount, reason, _gameHistory.CurrentLog.TransactionId);

            LogCashOut(amount, wager, reason, traceId, true);

            return result;
        }

        private bool CashOut(long amount, TransferOutReason reason, Guid traceId, long wager = 0)
        {
            var result = _bank.CashOut(traceId, amount, reason, true, _gameHistory.CurrentLog.TransactionId);

            LogCashOut(amount, wager, reason, traceId, false);

            return result;
        }

        private void LogCashOut(long amount, long wager, TransferOutReason reason, Guid traceId, bool handpay)
        {
            _gameHistory.AppendCashOut(
                new CashOutInfo
                {
                    Amount = amount,
                    Wager = wager,
                    TraceId = traceId,
                    Reason = reason,
                    Handpay = handpay,
                    AssociatedTransactions = _gameHistory.CurrentLog.Jackpots
                        .Where(item => item.PayMethod == PayMethod.Any)
                        .Select(item => item.TransactionId)
                        .ToArray()
                });
        }

        private IEnumerable<PendingProgressivePayout> CreateProgressiveCommits()
        {
            foreach (var jackpot in _gameHistory.CurrentLog.Jackpots)
            {
                yield return new PendingProgressivePayout
                {
                    DeviceId = jackpot.DeviceId,
                    LevelId = jackpot.LevelId,
                    TransactionId = jackpot.TransactionId,
                    PayMethod = jackpot.PayMethod,
                    PaidAmount = jackpot.WinAmount
                };
            }
        }

        private bool PayJackpot(PendingProgressivePayout pendingPayout)
        {
            Guid traceId;

            var amount = pendingPayout.PaidAmount;

            switch (pendingPayout.PayMethod)
            {
                default:
                    return false;

                case PayMethod.Handpay:
                    traceId = Guid.NewGuid();
                    if (!_bank.ForceHandpay(traceId, amount, TransferOutReason.CashOut, pendingPayout.TransactionId))
                    {
                        return false;
                    }

                    break;
                case PayMethod.Voucher:
                    traceId = Guid.NewGuid();
                    if (!_bank.ForceVoucherOut(traceId, amount, TransferOutReason.CashOut, pendingPayout.TransactionId))
                    {
                        return false;
                    }

                    break;
            }

            _gameHistory.AppendCashOut(
                new CashOutInfo
                {
                    Amount = amount,
                    TraceId = traceId,
                    Reason = TransferOutReason.CashOut,
                    Handpay = pendingPayout.PayMethod == PayMethod.Handpay,
                    AssociatedTransactions = new[] { pendingPayout.TransactionId }
                });

            return true;
        }
    }
}