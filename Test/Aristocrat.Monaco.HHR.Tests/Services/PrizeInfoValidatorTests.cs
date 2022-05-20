namespace Aristocrat.Monaco.Hhr.Tests.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Events;
    using Hhr.Services;
    using Hhr.Services.GamePlay;
    using Gaming.Contracts;
    using Gaming.Contracts.Payment;
    using Gaming.Contracts.Progressives;
    using Kernel;
    using Storage.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class PrizeInfoValidatorTests
    {
        private PrizeInfoValidator _target;
        private Mock<IPrizeInformationEntityHelper> _prizeInformationEntityHelper;
        private Mock<ITransactionHistory> _transactionHistory;
        private Mock<IGamePlayEntityHelper> _gamePlayEntityHelper;
        private Mock<IEventBus> _eventBus;
        private Mock<IOutcomeValidatorProvider> _outcomeValidation;
        private Mock<IGameHistoryLog> _gameLog;

        private bool _prizeCalculationEventFired;

        public PrizeInfoValidatorTests()
        {
            SetupMocks();
            SetupEventBus();
            _target = new PrizeInfoValidator(_prizeInformationEntityHelper.Object, _transactionHistory.Object,
                                             _gamePlayEntityHelper.Object, _eventBus.Object, _outcomeValidation.Object);
        }

        [DataRow(0, 0, 0u, 0u, 0, 0, DisplayName = "No Winnings")]
        [DataRow(100, 100, 100u, 100u, 100, 500, DisplayName = "When Game Calculates Same Win As Server Does")]
        [DataRow(100, 100, 100u, 100u, 100, 400, DisplayName = "When Game Calculates Different Win Than Server Does Test 1")]
        [DataRow(100, 200, 100u, 100u, 100, 500, DisplayName = "When Game Calculates Different Win Than Server Does Test 2")]
        [DataTestMethod]
        public void CentralHandler_PrizeCalculationErrorTest(long raceSet1AmountWon, long raceSet2AmountWon,
            uint raceSet1ExtraWinnings,
            uint raceSet2ExtraWinnings, long totalProgressiveAmountWon, long gameCalculatedWin)
        {
            _prizeCalculationEventFired = false;
            SetupPrizeInfo(raceSet1AmountWon, raceSet2AmountWon, raceSet1ExtraWinnings, raceSet2ExtraWinnings,
                totalProgressiveAmountWon);
            SetGameWinAmount(gameCalculatedWin);
            _target.Validate(_gameLog.Object);
            Assert.IsTrue(
                raceSet1AmountWon + raceSet2AmountWon + raceSet1ExtraWinnings + raceSet2ExtraWinnings +
                totalProgressiveAmountWon != gameCalculatedWin
                    ? _prizeCalculationEventFired
                    : !_prizeCalculationEventFired);
        }

        [DataRow(0, 0, 0, 0, 0, false, false, DisplayName = "No Jackpot Info")]
        [DataRow(100, 110, 100, 110, 10, false, false, DisplayName = "JP Hit and Committed are same")]
        [DataRow(100, 110, 100, 110, 10, true, false, DisplayName = "Win amount for leverls are different")]
        [DataRow(100, 110, 100, 110, 9, false, false, DisplayName = "JP Hit and Jackpot Trans are same, but not committed transactions")]
        [DataRow(100, 110, 100, 109, 9, false, false, DisplayName = "JP Hit and Jackpot Trans counts are different")]
        [DataRow(100, 110, 100, 110, 10, false, true, DisplayName = "JP Hit and Jackpot Trans are same but not level wise")]
        [DataTestMethod]
        public void JackpotHitVsCommittedTest(int jackpotHitStartId, int jackpotHitEndId, int jackpotTransactionStartId,
            int jackpotTransactionEndId, int jackpotCommittedCount, bool alterWinAmount, bool alterLevelCount)
        {
            _prizeCalculationEventFired = false;
            var resultExpected = jackpotHitStartId == jackpotTransactionStartId &&
                                 jackpotHitEndId == jackpotTransactionEndId &&
                                 jackpotTransactionEndId - jackpotTransactionStartId ==
                                 jackpotCommittedCount && !alterWinAmount && !alterLevelCount;
            SetupProgressivePrizeInfo(jackpotHitStartId, jackpotHitEndId, alterWinAmount);
            SetupJackpot(jackpotHitStartId, jackpotHitEndId);
            SetupJackpotTransactions(jackpotTransactionStartId, jackpotTransactionEndId, jackpotCommittedCount,
                alterLevelCount);
            _gamePlayEntityHelper.Setup(m => m.PrizeCalculationError).Returns(false);
            _target.Validate(_gameLog.Object);
            Assert.IsTrue(_prizeCalculationEventFired == !resultExpected);
        }

        private void SetupMocks()
        {
            _prizeInformationEntityHelper = new Mock<IPrizeInformationEntityHelper>();
            _transactionHistory = new Mock<ITransactionHistory>();
            _gamePlayEntityHelper = new Mock<IGamePlayEntityHelper>();
            _eventBus = new Mock<IEventBus>();
            _outcomeValidation = new Mock<IOutcomeValidatorProvider>();

            _gameLog = new Mock<IGameHistoryLog>();
            _gameLog.Setup(g => g.ShallowCopy()).Returns(_gameLog.Object);
        }

        private void SetGameWinAmount(long totalWinAmountByGame)
        {
            _gameLog.Setup(g => g.FinalWin).Returns(totalWinAmountByGame);
        }

        private void SetupProgressivePrizeInfo(int jackpotIdStart, int jackpotIdEnd, bool alterWin)
        {
            var progressiveLevelsHit = new List<(int, int)>();
            var progressiveLevelsHitAmount = new List<(int, long)>();
            for (var jpId = jackpotIdStart; jpId < jackpotIdEnd; jpId++)
            {
                progressiveLevelsHit.Add((jpId, 1));
                progressiveLevelsHitAmount.Add((jpId, alterWin ? 110L : 100L));
            }

            _prizeInformationEntityHelper.Setup(m => m.PrizeInformation).Returns(new PrizeInformation
            {
                ProgressiveLevelsHit = progressiveLevelsHit,
                ProgressiveLevelAmountHit = progressiveLevelsHitAmount
            });
        }

        private void SetupPrizeInfo(long raceSet1AmountWon, long raceSet2AmountWonWithoutProgressives,
            uint raceSet1ExtraWinnings,
            uint raceSet2ExtraWinnings, long totalProgressiveAmountWon)
        {
            _prizeInformationEntityHelper.Setup(m => m.PrizeInformation).Returns(new PrizeInformation
            {
                RaceSet1AmountWon = raceSet1AmountWon,
                RaceSet2AmountWonWithoutProgressives = raceSet2AmountWonWithoutProgressives,
                RaceSet1ExtraWinnings = raceSet1ExtraWinnings,
                RaceSet2ExtraWinnings = raceSet2ExtraWinnings,
                TotalProgressiveAmountWon = totalProgressiveAmountWon
            });
        }

        private void SetupJackpot(int jackpotIdStart, int jackpotIdEnd)
        {
            var jackpots = Enumerable.Range(jackpotIdStart, jackpotIdEnd - jackpotIdStart)
                .Select(jpId => new JackpotInfo { TransactionId = jpId, LevelId = jpId, WinAmount = 100000L }).ToList();
            _gameLog.Setup(g => g.Jackpots).Returns(jackpots);
        }

        private void SetupJackpotTransactions(int jackpotIdStart, int jackpotIdEnd, long committedCount,
            bool alterLevelCount)
        {
            Assert.IsTrue(jackpotIdStart + committedCount <= jackpotIdEnd);
            var jackpotTransactions = Enumerable.Range(jackpotIdStart, (jackpotIdEnd - jackpotIdStart))
                .Select(jpId => new JackpotTransaction
                {
                    TransactionId = jpId,
                    State = (jpId >= jackpotIdStart + committedCount)
                        ? ProgressiveState.Pending
                        : ProgressiveState.Committed,
                    LevelId = alterLevelCount ? jpId + (jpId % 2) : jpId
                })
                .ToList();

            _transactionHistory.Setup(t => t.RecallTransactions<JackpotTransaction>()).Returns(jackpotTransactions);
        }

        private void SetupEventBus()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<PrizeCalculationErrorEvent>()))
                .Callback<object>(e => _prizeCalculationEventFired = true);
        }

    }
}