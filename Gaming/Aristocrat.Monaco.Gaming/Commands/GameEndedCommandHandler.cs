namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.TransferOut;
    using Application.Contracts.Extensions;
    using Common.PerformanceCounters;
    using Contracts;
    using Contracts.Barkeeper;
    using Contracts.Meters;
    using Contracts.Progressives;
    using Contracts.Session;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Progressives;
    using Runtime;
    using Runtime.Client;

    /// <summary>
    ///     Command handler for the <see cref="GameEnded" /> command.
    /// </summary>
    [CounterDescription("Game End", PerformanceCounterType.AverageTimer32)]
    public class GameEndedCommandHandler : ICommandHandler<GameEnded>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPlayerBank _bank;
        private readonly IGameHistory _gameHistory;
        private readonly IGameMeterManager _meters;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly IPlayerService _players;
        private readonly IPropertiesManager _properties;
        private readonly IGameRecovery _recovery;
        private readonly IRuntime _runtime;
        private readonly IGameProvider _gameProvider;
        private readonly ITransactionHistory _transactionHistory;
        private readonly IBarkeeperHandler _barkeeperHandler;
        private readonly IProgressiveGameProvider _progressiveGameProvider;
        private readonly IMaxWinOverlayService _maxWinOverlayService;

        private readonly bool _meterFreeGames;
        private readonly IProgressiveLevelProvider _progressiveLevelProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameEndedCommandHandler" /> class.
        /// </summary>
        public GameEndedCommandHandler(
            IPlayerBank bank,
            IPersistentStorageManager persistentStorage,
            IGameHistory gameHistory,
            IRuntime runtime,
            IGameProvider gameProvider,
            IPropertiesManager properties,
            IGameMeterManager meters,
            IPlayerService players,
            IGameRecovery recovery,
            ITransactionHistory transactionHistory,
            IBarkeeperHandler barkeeperHandler,
            IProgressiveGameProvider progressiveGameProvider,
            IProgressiveLevelProvider progressiveLevelProvider,
            IMaxWinOverlayService maxWinOverlayService)
        {
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _players = players ?? throw new ArgumentNullException(nameof(players));
            _recovery = recovery ?? throw new ArgumentNullException(nameof(recovery));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _barkeeperHandler = barkeeperHandler ?? throw new ArgumentNullException(nameof(barkeeperHandler));
            _progressiveGameProvider = progressiveGameProvider ?? throw new ArgumentNullException(nameof(progressiveGameProvider));
            _progressiveLevelProvider = progressiveLevelProvider ?? throw new ArgumentNullException(nameof(progressiveGameProvider));

            _meterFreeGames = _properties.GetValue(GamingConstants.MeterFreeGamesIndependently, false);
            _maxWinOverlayService = maxWinOverlayService ?? throw new ArgumentNullException(nameof(maxWinOverlayService));
        }

        /// <inheritdoc />
        public void Handle(GameEnded command)
        {
            using (var scope = _persistentStorage.ScopedTransaction())
            {
                _gameHistory.EndGame();

                IncrementMeters();

                _gameHistory.AddMeterSnapShotWithPersistentLog();

                _barkeeperHandler.GameEnded(_gameHistory.CurrentLog);

                _bank.Unlock();

                scope.Complete();
            }

            Logger.Debug("PendingHandpay set to false");
            _runtime.UpdateFlag(RuntimeCondition.PendingHandpay, false);

            if(!_maxWinOverlayService.ShowingMaxWinWarning)
            {
                _runtime.UpdateBalance(_bank.Credits);
            }
        }

        private void IncrementMeters()
        {
            var (game, denomination) = _gameProvider.GetGame(_properties.GetValue(GamingConstants.SelectedGameId, 0),
                _properties.GetValue(GamingConstants.SelectedDenom, 0L));

            var denomId = _properties.GetValue(GamingConstants.SelectedDenom, 0L);

            var log = _gameHistory.CurrentLog;

            if ((!_meterFreeGames || denomination.SecondaryAllowed) &&
                (log.Result == GameResult.Won || log.Result == GameResult.Lost || log.Result == GameResult.Tied))
            {
                var wagerCategory = _properties.GetValue<IWagerCategory>(GamingConstants.SelectedWagerCategory, null);

                _meters.IncrementGamesPlayed(game.Id, denomId, wagerCategory, log.Result, _players.HasActiveSession);

                var nonCreditHandpayTransactions = _transactionHistory.RecallTransactions<HandpayTransaction>()
                    .Where(t => !t.IsCreditType()).ToList();

                var handpayForCashoutInfo =
                    nonCreditHandpayTransactions.Where(
                        h => log.CashOutInfo.Any(co => co.TraceId == h.TraceId)).ToList();

                // mark cashout as handpay, if it was handpaid
                foreach (var co in handpayForCashoutInfo.Select(
                        handpayCashout => log.CashOutInfo.FirstOrDefault(c => c.TraceId == handpayCashout.TraceId))
                    .Where(co => co != null))
                {
                    co.Handpay = true;
                }

                // mark cashout as non-handpay, if it went to credit meter
                var creditHandpayTransactions = _transactionHistory.RecallTransactions<HandpayTransaction>()
                    .Where(t => t.IsCreditType() && log.CashOutInfo.Any(c => c.Handpay && c.TraceId == t.TraceId)).ToList();

                foreach (var co in creditHandpayTransactions.Select(
                        handpayCashout => log.CashOutInfo.FirstOrDefault(c => c.TraceId == handpayCashout.TraceId))
                    .Where(co => co != null))
                {
                    co.Handpay = false;
                }

                var progWin = log.Jackpots.Sum(info => info.WinAmount);
                if (log.SecondaryPlayed > 0 && log.Result == GameResult.Lost)
                {
                    progWin = 0;
                }

                if (progWin > 0)
                {
                    var totalPaidLinkedProgWonAmt = (from jackpot in log.Jackpots
                        let associatedProgressiveLevel =
                            _progressiveLevelProvider.GetProgressiveLevels(jackpot.PackName, game.Id, denomId).Where(
                                progressiveLevel => progressiveLevel.DeviceId == jackpot.DeviceId)
                        where associatedProgressiveLevel.First().LevelType == ProgressiveLevelType.LP
                        select jackpot.WinAmount).Sum();
                    if (totalPaidLinkedProgWonAmt > 0)
                    {
                        _meters.GetMeter(game.Id, denomId, GamingMeters.TotalPaidLinkedProgWonAmt)
                            .Increment(totalPaidLinkedProgWonAmt);
                    }
                }
                long handPaidProgWin = 0;

                var handPaidProgWinFromJackpotInfo = log.Jackpots.Where(
                        jackpot => log.CashOutInfo.Any(
                            cashout => cashout.AssociatedTransactions.Contains(jackpot.TransactionId) &&
                                       cashout.Handpay && cashout.Reason != TransferOutReason.CashWin))
                    .Sum(info => info.WinAmount);

                var handPaidProgWinFromCashoutInfo = log.CashOutInfo.Where(
                    cashout => log.Jackpots.Any(
                        jackpot => cashout.AssociatedTransactions.Contains(jackpot.TransactionId) && cashout.Handpay &&
                                   cashout.Reason != TransferOutReason.CashWin)).Sum(info => info.Amount);

                Logger.Debug($"HPaidProgWin - CashoutLog({handPaidProgWinFromCashoutInfo}) | JackpotLog({handPaidProgWinFromJackpotInfo})");

                var finalWinLog = log.FinalWin.CentsToMillicents();

                // Calculate final handpaid progressive win. If no progressive win, this will always result in 0
                if (finalWinLog == progWin)
                {
                    // Total game win is same as progressive win
                    // Actual handpaid progressive win would correspond to handpay amount cashed out.
                    handPaidProgWin = handPaidProgWinFromCashoutInfo;
                }
                else if(finalWinLog > progWin)
                {
                    // Total game win is more than progressive win
                    // Actual cashout log amount will include game win as well
                    // So, we use progressive win info from Jackpot logs
                    handPaidProgWin = handPaidProgWinFromJackpotInfo;
                }

                Logger.Debug($"HPaidProgWin - Final = {handPaidProgWin}");

                var pendingProgressives = GetPendingProgressives().ToArray();
                if (pendingProgressives.Any())
                {
                    _progressiveGameProvider.IncrementProgressiveWinMeters(pendingProgressives);
                }

                var egmPaidProgWinAmount = progWin - handPaidProgWin;

                if (egmPaidProgWinAmount > 0)
                {
                    _meters.GetMeter(game.Id, denomId, GamingMeters.EgmPaidProgWonAmount)
                        .Increment(egmPaidProgWinAmount);

                    var count = log.Jackpots.Count(
                        jackpot => log.CashOutInfo.Any(
                            cashout => cashout.AssociatedTransactions.Contains(jackpot.TransactionId) &&
                                       !cashout.Handpay));

                    if (count == 0)
                    {
                        count = log.Jackpots.Count();
                    }

                    _meters.GetMeter(game.Id, denomId, GamingMeters.EgmPaidProgWonCount).Increment(count);
                }

                if (handPaidProgWin > 0)
                {
                    _meters.GetMeter(game.Id, denomId, GamingMeters.HandPaidProgWonAmount).Increment(handPaidProgWin);

                    _meters.GetMeter(game.Id, denomId, GamingMeters.HandPaidProgWonCount).Increment(
                        log.Jackpots.Count(
                            jackpot => log.CashOutInfo.Any(
                                cashout => cashout.AssociatedTransactions.Contains(jackpot.TransactionId) &&
                                           cashout.Handpay)));
                }

                var gameWin = log.FinalWin.CentsToMillicents() - progWin;

                // Get amount that is hand paid and isn't headed for the credit meter.
                var handPaidGameWin = log.CashOutInfo.Where(
                        c => c.Handpay && c.Reason != TransferOutReason.CashWin && creditHandpayTransactions.All(t => t.TraceId != c.TraceId))
                    .Select(c => c.Amount)
                    .DefaultIfEmpty(0)
                    .Sum();

                handPaidGameWin -= handPaidProgWin;
                _meters.GetMeter(game.Id, denomId, GamingMeters.EgmPaidGameWonAmount)
                    .Increment(gameWin - handPaidGameWin);
                _meters.GetMeter(game.Id, denomId, GamingMeters.HandPaidGameWonAmount).Increment(handPaidGameWin);

                if (handPaidGameWin > 0 && progWin == 0)
                {
                    _meters.GetMeter(game.Id, denomId, GamingMeters.HandPaidGameWonCount).Increment(1);
                }
            }

            if (_players.HasActiveSession)
            {
                _meters.GetMeter(PlayerMeters.CardedGameWonAmount)
                    .Increment(log.FinalWin.CentsToMillicents());
            }
        }

        private IEnumerable<PendingProgressivePayout> GetPendingProgressives()
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
    }
}