namespace Aristocrat.Monaco.Hhr.Services.GamePlay
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Events;
    using Gaming.Contracts;
    using Gaming.Contracts.Payment;
    using Gaming.Contracts.Progressives;
    using Kernel;
    using log4net;
    using Storage.Helpers;

    public class PrizeInfoValidator : IOutcomeValidator
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IPrizeInformationEntityHelper _prizeInformationEntityHelper;
        private readonly ITransactionHistory _transactionHistory;
        private readonly IGamePlayEntityHelper _gamePlayEntityHelper;
        private readonly IEventBus _eventBus;

        public PrizeInfoValidator(IPrizeInformationEntityHelper priceInformationEntityHelper,
                                  ITransactionHistory transactionHistory,
                                  IGamePlayEntityHelper gamePlayEntityHelper,
                                  IEventBus eventBus,
                                  IOutcomeValidatorProvider outcomeValidation)
        {
            _prizeInformationEntityHelper = priceInformationEntityHelper ?? throw new ArgumentException(nameof(priceInformationEntityHelper));
            _transactionHistory = transactionHistory ?? throw new ArgumentException(nameof(transactionHistory));
            _gamePlayEntityHelper = gamePlayEntityHelper ?? throw new ArgumentException(nameof(gamePlayEntityHelper));
            _eventBus = eventBus ?? throw new ArgumentException(nameof(eventBus));

            // Register as the guy who will validate outcomes from the game against the HHR server.
            outcomeValidation.Handler = this;
        }

        public void Validate(IGameHistoryLog gameHistory)
        {
            if (!ValidatePrize(gameHistory))
            {
                _gamePlayEntityHelper.PrizeCalculationError = true;
                _eventBus.Publish(new PrizeCalculationErrorEvent());
            }
        }

        private bool ValidatePrize(IGameHistoryLog gameHistory)
        {
            // Make sure the amount that the server returned and what game ended up paying is same.
            // if not, disable the machine with non-recoverable lockup.
            var prizeInfo = _prizeInformationEntityHelper.PrizeInformation;
            if (prizeInfo == null)
            {
                Logger.Error("No prize information found when validating prize. Locking the EGM.");
                return false;
            }

            var normalAmountWon = prizeInfo.RaceSet1AmountWon + prizeInfo.RaceSet2AmountWonWithoutProgressives;
            var extraWinnings = prizeInfo.RaceSet1ExtraWinnings + prizeInfo.RaceSet2ExtraWinnings;
            var progressiveWins = prizeInfo.TotalProgressiveAmountWon;
            if (gameHistory.FinalWin != normalAmountWon + extraWinnings + progressiveWins)
            {
                Logger.Error($"Prize calculation mismatch between central server and the game. Locking the EGM. Final Win={gameHistory.FinalWin}, Normal={normalAmountWon}, Extra={extraWinnings}, Progressive={progressiveWins}");
                return false;
            }

            if (!gameHistory.Jackpots.Any())
            {
                return true;
            }

            var transactionsFromTransactionHistory =
                _transactionHistory.RecallTransactions<JackpotTransaction>();

            var transIdsFromGameHistory = gameHistory.Jackpots.Select(jp => jp.TransactionId);

            var lastGameRoundJackpotTransactions =
                transactionsFromTransactionHistory.Where(
                    t =>
                        transIdsFromGameHistory.Contains(t.TransactionId));
            var lastRoundJackpotTransactions = lastGameRoundJackpotTransactions as JackpotTransaction[] ??
                                               lastGameRoundJackpotTransactions.ToArray();

            var idsFromGameHistory = transIdsFromGameHistory as long[] ?? transIdsFromGameHistory.ToArray();

            // jackpot hit count check
            if (lastRoundJackpotTransactions.Length != idsFromGameHistory.Length)
            {
                Logger.Error(
                    $"Total jackpots hit by server : {lastRoundJackpotTransactions.Length} is not matching with game hit count : {idsFromGameHistory.Length}");
                return false;
            }

            // jackpot committed check
            if (lastRoundJackpotTransactions.Any(j => j.State != ProgressiveState.Committed))
            {
                Logger.Error(
                    $"Total jackpots hit {idsFromGameHistory.Length}, Total Jackpots committed = {lastRoundJackpotTransactions.Count(j => j.State == ProgressiveState.Committed)}");
                return false;
            }

            // jackpot hit level count check
            foreach (var (levelId, count) in prizeInfo.ProgressiveLevelsHit)
            {
                var gameLevelHitCount = gameHistory.Jackpots.Count(x => x.LevelId == levelId);
                var serverLevelHitCount = lastRoundJackpotTransactions.Count(x => x.LevelId == levelId);
                if (gameLevelHitCount == serverLevelHitCount)
                {
                    continue;
                }

                Logger.Error(
                    $"Progressive level: {levelId} hit count:{count} miss match between server count:{gameLevelHitCount} and the game count:{gameLevelHitCount}.");
                return false;
            }

            // jackpot hit level wise win amount check
            foreach (var (levelId, amount) in prizeInfo.ProgressiveLevelAmountHit)
            {
                var levelWinByGame = gameHistory.Jackpots.Where(x => x.LevelId == levelId).Sum(x => x.WinAmount);
                if (amount == levelWinByGame.MillicentsToCents())
                {
                    continue;
                }

                Logger.Error(
                    $"Progressive level win for {levelId} levels doesn't match between server win:{amount}and the game win: {levelWinByGame}.");
                return false;
            }

            return true;
        }
    }
}