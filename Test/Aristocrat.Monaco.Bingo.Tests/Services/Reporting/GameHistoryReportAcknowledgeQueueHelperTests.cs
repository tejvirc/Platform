namespace Aristocrat.Monaco.Bingo.Tests.Services.Reporting
{
    using System;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Bingo.Client.Messages.GamePlay;
    using Bingo.Services.Reporting;
    using Common;
    using Common.GameOverlay;
    using Common.Storage;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GameHistoryReportAcknowledgeQueueHelperTests
    {
        private readonly Mock<ICentralProvider> _centralProvider = new(MockBehavior.Default);
        private readonly Mock<IGameHistory> _gameHistory = new(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _propertiesManager = new(MockBehavior.Default);
        private readonly Mock<ISystemDisableManager> _systemDisableManager = new(MockBehavior.Default);
        private GameHistoryReportAcknowledgeQueueHelper _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false, false, false)]
        [DataRow(false, true, false, false)]
        [DataRow(false, false, true, false)]
        [DataRow(false, false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorArgumentTest(
            bool nullCentralProvider,
            bool nullGameHistory,
            bool nullPropertiesManger,
            bool nullSystemDisable)
        {
            _ = CreateTarget(nullCentralProvider, nullGameHistory, nullPropertiesManger, nullSystemDisable);
        }

        [TestMethod]
        public void GetIdTest()
        {
            const long id = 1000;
            var item = new ReportGameOutcomeMessage { TransactionId = id };

            Assert.AreEqual(id, _target.GetId(item));
        }

        [TestMethod]
        public void ReadFromPersistenceTest()
        {
            const long gameTransactionId = 56789;
            const long wagerAmount = 250;
            const long winAmount = 100;
            const long startCredits = 10000;
            const long endCredits = 9850;
            const string machineSerial = "Test Serial";

            var startTime = DateTime.UtcNow - TimeSpan.FromSeconds(5);
            var joinTime = DateTime.UtcNow;
            var log = new Mock<IGameHistoryLog>();
            log.Setup(x => x.FinalWager).Returns(wagerAmount);
            log.Setup(x => x.TotalWon).Returns(winAmount);
            log.Setup(x => x.StartCredits).Returns(startCredits.CentsToMillicents());
            log.Setup(x => x.EndCredits).Returns(endCredits.CentsToMillicents());
            log.Setup(x => x.StartDateTime).Returns(startTime);
            log.Setup(x => x.TransactionId).Returns(gameTransactionId);

            var bingoCard = new BingoCard(123456) { DaubedBits = 562, IsGameEndWin = false };
            var bingoPattern = new BingoPattern("Test Pattern1", 563, 123456, winAmount, 25, 222, false, 562, 1);
            var description = new BingoGameDescription
            {
                DenominationId = 1000,
                BallCallNumbers =
                    Enumerable.Range(1, 58).Select(x => new BingoNumber(x, BingoNumberState.CardCovered)),
                JoinBallIndex = 40,
                Cards = new[] { bingoCard },
                FacadeKey = 5678,
                GameTitleId = 123,
                GameEndWinClaimAccepted = false,
                GameEndWinEligibility = 0,
                GameSerial = 1234,
                JoinTime = joinTime,
                Patterns = new[] { bingoPattern },
                Paytable = "Test Paytable",
                ProgressiveLevels = Array.Empty<long>(),
                ThemeId = 123
            };

            var centralTransaction = new CentralTransaction(0, DateTime.UtcNow, 123, 1000, "Test Wager", string.Empty, wagerAmount, 1)
            {
                Descriptions = new[] { description },
                AssociatedTransactions = new []{ gameTransactionId }
            };

            var transactions = new[] { centralTransaction };

            _gameHistory.Setup(x => x.GetGameHistory()).Returns(new[] { log.Object });
            _centralProvider.Setup(x => x.Transactions).Returns(transactions);
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<string>()))
                .Returns(machineSerial);
            var gameOutcomeMessages = _target.ReadPersistence();
            Assert.AreEqual(1, gameOutcomeMessages.Count);
            var outcomeMessage = gameOutcomeMessages.First();

            Assert.AreEqual(description.Paytable, outcomeMessage.Paytable);
            CollectionAssert.AreEquivalent(Enumerable.Range(1, 58).ToList(), outcomeMessage.BallCall.ToList());
            Assert.AreEqual(wagerAmount, outcomeMessage.BetAmount);
            Assert.AreEqual(startCredits, outcomeMessage.StartingBalance);
            Assert.AreEqual(endCredits, outcomeMessage.FinalBalance);
            Assert.AreEqual(description.DenominationId, outcomeMessage.DenominationId);
            Assert.AreEqual(description.FacadeKey, outcomeMessage.FacadeKey);
            Assert.AreEqual(description.GameSerial, outcomeMessage.GameSerial);
            Assert.AreEqual(40, outcomeMessage.JoinBall);
            Assert.AreEqual(joinTime, outcomeMessage.JoinTime);
            Assert.AreEqual(machineSerial, outcomeMessage.MachineSerial);
            Assert.AreEqual(winAmount, outcomeMessage.PaidAmount);
            Assert.AreEqual(startTime, outcomeMessage.StartTime);
            Assert.AreEqual(description.ThemeId, outcomeMessage.ThemeId);
            Assert.AreEqual(winAmount, outcomeMessage.TotalWin);
            Assert.AreEqual(centralTransaction.TransactionId, outcomeMessage.TransactionId);
            Assert.AreEqual(description.GameTitleId, outcomeMessage.GameTitleId);
            Assert.AreEqual(description.GameEndWinEligibility, outcomeMessage.GameEndWinEligibility);
            Assert.AreEqual(1, outcomeMessage.CardsPlayed.Count());
            Assert.AreEqual(1, outcomeMessage.WinResults.Count());

            var playedCard = outcomeMessage.CardsPlayed.First();
            Assert.AreEqual(bingoCard.DaubedBits, playedCard.BitPattern);
            Assert.AreEqual(bingoCard.SerialNumber, playedCard.SerialNumber);
            Assert.AreEqual(bingoCard.IsGameEndWin, playedCard.IsGameEndWin);

            var winResult = outcomeMessage.WinResults.First();
            Assert.AreEqual(bingoPattern.WinAmount, winResult.Payout);
            Assert.AreEqual(bingoPattern.BallQuantity, winResult.BallQuantity);
            Assert.AreEqual(bingoPattern.BitFlags, winResult.BitPattern);
            Assert.AreEqual(bingoPattern.CardSerial, winResult.CardSerial);
            Assert.AreEqual(bingoPattern.PatternId, winResult.PatternId);
            Assert.AreEqual(bingoPattern.Name, winResult.PatternName);
            Assert.AreEqual(bingoPattern.IsGameEndWin, winResult.IsGameEndWin);
        }

        [TestMethod]
        public void AlmostFullDisableTest()
        {
            _target.AlmostFullDisable();
            _systemDisableManager.Verify(m => m.Disable(
                BingoConstants.GameHistoryQueueDisableKey,
                SystemDisablePriority.Normal,
                It.IsAny<Func<string>>(),
                false,
                It.IsAny<Func<string>>(),
                null), Times.Once());
        }

        [TestMethod]
        public void AlmostFullClearTest()
        {
            _target.AlmostFullClear();
            _systemDisableManager.Verify(m => m.Enable(
                BingoConstants.GameHistoryQueueDisableKey), Times.Once());
        }

        private GameHistoryReportAcknowledgeQueueHelper CreateTarget(
            bool nullCentralProvider = false,
            bool nullGameHistory = false,
            bool nullPropertiesManger = false,
            bool nullSystemDisable = false)
        {
            return new GameHistoryReportAcknowledgeQueueHelper(
                nullCentralProvider ? null : _centralProvider.Object,
                nullGameHistory ? null : _gameHistory.Object,
                nullPropertiesManger ? null : _propertiesManager.Object,
                nullSystemDisable ? null : _systemDisableManager.Object);
        }
    }
}