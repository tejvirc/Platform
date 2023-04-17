namespace Aristocrat.Monaco.Bingo.Tests.Monitors
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Bingo.Monitors;
    using Bingo.Services.Reporting;
    using Common.Storage;
    using Gaming.Contracts;
    using Gaming.Contracts.Bonus;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using TransactionType = Common.TransactionType;

    [TestClass]
    public class EgmPaidGameWinBonusAmountMeterMonitorTests
    {
        private static readonly TestClassification MockClassification = new();
        private readonly Mock<IReportTransactionQueueService> _bingoTransactionReportHandler = new(MockBehavior.Default);
        private readonly Mock<IMeterManager> _meterManager = new(MockBehavior.Default);
        private readonly Mock<IBingoGameProvider> _bingoGameProvider = new(MockBehavior.Default);
        private readonly Mock<ITransactionHistory> _transactionHistory = new(MockBehavior.Default);
        private readonly Mock<IBonusHandler> _bonusHandler = new(MockBehavior.Default);
        private readonly Mock<IGameHistory> _gameHistory = new(MockBehavior.Default);
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);

        private TestMeter _meter;
        private EgmPaidGameWinBonusAmountMeterMonitor _target;

        [TestInitialize]
        public void Initialize()
        {
            _meter = new TestMeter(GamingMeters.EgmPaidGameWonAmount, MockClassification);
            _meterManager.Setup(m => m.GetMeter(BonusMeters.EgmPaidGameWinBonusAmount)).Returns(_meter);
            _target = CreateTarget();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
        }

        [DataRow(true, false, false, false, false, false, false)]
        [DataRow(false, true, false, false, false, false, false)]
        [DataRow(false, false, true, false, false, false, false)]
        [DataRow(false, false, false, true, false, false, false)]
        [DataRow(false, false, false, false, true, false, false)]
        [DataRow(false, false, false, false, false, true, false)]
        [DataRow(false, false, false, false, false, false, true)]
        [DataTestMethod]
        public void NullConstructorParametersTest(
            bool meterNull,
            bool bingoGameNull,
            bool reportingNull,
            bool bonusHandlerNull,
            bool gameHistoryNull,
            bool transactionHistoryNull,
            bool eventBusNull)
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => _ = CreateTarget(
                    meterNull,
                    bingoGameNull,
                    reportingNull,
                    bonusHandlerNull,
                    gameHistoryNull,
                    transactionHistoryNull,
                    eventBusNull));
        }

        [TestMethod]
        public void NoBingoGameFoundTest()
        {
            const long amount = 12340000;
            _gameHistory.Setup(x => x.CurrentLog).Returns(new Mock<IGameHistoryLog>().Object);
            _bingoGameProvider.Setup(x => x.GetBingoGame()).Returns((BingoGameDescription)null);
            _meter.Increment(amount);
            _bingoTransactionReportHandler.Verify(
                x => x.AddNewTransactionToQueue(
                    It.IsAny<TransactionType>(),
                    It.IsAny<long>(),
                    It.IsAny<uint>(),
                    It.IsAny<int>(),
                    It.IsAny<long>(),
                    It.IsAny<int>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [TestMethod]
        public void NoBingoGameHistoryFoundTest()
        {
            const long amount = 12340000;
            _gameHistory.Setup(x => x.CurrentLog).Returns((IGameHistoryLog)null);
            _bingoGameProvider.Setup(x => x.GetBingoGame()).Returns(new BingoGameDescription());
            _meter.Increment(amount);
            _bingoTransactionReportHandler.Verify(
                x => x.AddNewTransactionToQueue(
                    It.IsAny<TransactionType>(),
                    It.IsAny<long>(),
                    It.IsAny<uint>(),
                    It.IsAny<int>(),
                    It.IsAny<long>(),
                    It.IsAny<int>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [TestMethod]
        public void ZeroIncrementTest()
        {
            const long amount = 0;
            const int paytableId = 123;
            const long gameSerial = 123456789;
            const int denominationId = 1;
            const uint titleId = 61;
            var bingoGame = new BingoGameDescription
            {
                Paytable = paytableId.ToString(),
                GameSerial = gameSerial,
                DenominationId = denominationId,
                GameTitleId = titleId
            };

            _transactionHistory.Setup(x => x.RecallTransactions<HandpayTransaction>())
                .Returns(Array.Empty<HandpayTransaction>());
            var gameHistoryLog = new Mock<IGameHistoryLog>(MockBehavior.Default);
            _gameHistory.Setup(x => x.CurrentLog).Returns(gameHistoryLog.Object);
            gameHistoryLog.Setup(x => x.CashOutInfo).Returns(Array.Empty<CashOutInfo>());
            _bingoGameProvider.Setup(x => x.GetBingoGame()).Returns(bingoGame);
            _meter.Increment(amount);
            _bingoTransactionReportHandler.Verify(
                x => x.AddNewTransactionToQueue(
                    It.IsAny<TransactionType>(),
                    It.IsAny<long>(),
                    It.IsAny<uint>(),
                    It.IsAny<int>(),
                    It.IsAny<long>(),
                    It.IsAny<int>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [TestMethod]
        public void ReportTransactionNoHandpayTest()
        {
            const long amount = 12340000;
            const int paytableId = 123;
            const long gameSerial = 123456789;
            const int denominationId = 1;
            const uint titleId = 61;
            const long gameTransactionId = 3;
            var bingoGame = new BingoGameDescription
            {
                Paytable = paytableId.ToString(),
                GameSerial = gameSerial,
                DenominationId = denominationId,
                GameTitleId = titleId
            };

            _bonusHandler.Setup(x => x.Transactions).Returns(
                new BonusTransaction[] { new() { AssociatedTransactions = new[] { gameTransactionId } } });
            _transactionHistory.Setup(x => x.RecallTransactions<HandpayTransaction>())
                .Returns(Array.Empty<HandpayTransaction>());
            var gameHistoryLog = new Mock<IGameHistoryLog>(MockBehavior.Default);
            _gameHistory.Setup(x => x.CurrentLog).Returns(gameHistoryLog.Object);
            gameHistoryLog.Setup(x => x.TransactionId).Returns(gameTransactionId);
            _bingoGameProvider.Setup(x => x.GetBingoGame()).Returns(bingoGame);
            _meter.Increment(amount);
            _bingoTransactionReportHandler.Verify(
                x => x.AddNewTransactionToQueue(
                    TransactionType.CashWon,
                    amount.MillicentsToCents(),
                    titleId,
                    denominationId,
                    gameSerial,
                    paytableId,
                    string.Empty),
                Times.Once);
        }

        [TestMethod]
        public void ReportTransactionHandpayOnlyTest()
        {
            const long amount = 12340000;
            const int paytableId = 123;
            const long gameSerial = 123456789;
            const int denominationId = 1;
            const uint titleId = 61;
            const string barcode = "TestBarcode";
            const long gameTransactionId = 3;
            const long bonusTransactionId = 4;
            var bingoGame = new BingoGameDescription
            {
                Paytable = paytableId.ToString(),
                GameSerial = gameSerial,
                DenominationId = denominationId,
                GameTitleId = titleId
            };

            _bonusHandler.Setup(x => x.Transactions).Returns(
                new BonusTransaction[]
                {
                    new()
                    {
                        TransactionId = bonusTransactionId, AssociatedTransactions = new[] { gameTransactionId }
                    }
                });
            var handpayTransactions = new List<HandpayTransaction>
            {
                new HandpayTransaction(
                    0,
                    DateTime.UtcNow,
                    amount,
                    0,
                    0,
                    0,
                    HandpayType.GameWin,
                    true,
                    Guid.NewGuid())
                {
                    Barcode = barcode,
                    KeyOffType = KeyOffType.LocalCredit,
                    KeyOffCashableAmount = amount,
                    AssociatedTransactions = new []{ bonusTransactionId }
                }
            };

            _transactionHistory.Setup(x => x.RecallTransactions<HandpayTransaction>()).Returns(handpayTransactions);

            var gameHistoryLog = new Mock<IGameHistoryLog>(MockBehavior.Default);
            _gameHistory.Setup(x => x.CurrentLog).Returns(gameHistoryLog.Object);
            gameHistoryLog.Setup(x => x.TransactionId).Returns(gameTransactionId);
            _bingoGameProvider.Setup(x => x.GetBingoGame()).Returns(bingoGame);
            _meter.Increment(amount);
            _bingoTransactionReportHandler.Verify(
                x => x.AddNewTransactionToQueue(
                    TransactionType.LargeWin,
                    amount.MillicentsToCents(),
                    titleId,
                    denominationId,
                    gameSerial,
                    paytableId,
                    barcode),
                Times.Once);
        }

        [TestMethod]
        public void ReportTransactionHandpayAndGameWinTest()
        {
            const long handpayAmount = 12340000;
            const long gameWinAmount = 5430000;
            const int paytableId = 123;
            const long gameSerial = 123456789;
            const int denominationId = 1;
            const uint titleId = 61;
            const string barcode = "TestBarcode";
            const long gameTransactionId = 3;
            const long bonusTransactionId = 4;
            var bingoGame = new BingoGameDescription
            {
                Paytable = paytableId.ToString(),
                GameSerial = gameSerial,
                DenominationId = denominationId,
                GameTitleId = titleId
            };

            _bonusHandler.Setup(x => x.Transactions).Returns(
                new BonusTransaction[]
                {
                    new()
                    {
                        TransactionId = bonusTransactionId, AssociatedTransactions = new[] { gameTransactionId }
                    }
                });
            var handpayTransactions = new List<HandpayTransaction>
            {
                new HandpayTransaction(
                    0,
                    DateTime.UtcNow,
                    handpayAmount,
                    0,
                    0,
                    0,
                    HandpayType.GameWin,
                    true,
                    Guid.NewGuid())
                {
                    Barcode = barcode,
                    KeyOffType = KeyOffType.LocalCredit,
                    KeyOffCashableAmount = handpayAmount,
                    AssociatedTransactions = new []{ bonusTransactionId }
                }
            };

            _transactionHistory.Setup(x => x.RecallTransactions<HandpayTransaction>()).Returns(handpayTransactions);

            var gameHistoryLog = new Mock<IGameHistoryLog>(MockBehavior.Default);
            _gameHistory.Setup(x => x.CurrentLog).Returns(gameHistoryLog.Object);
            gameHistoryLog.Setup(x => x.TransactionId).Returns(gameTransactionId);
            _bingoGameProvider.Setup(x => x.GetBingoGame()).Returns(bingoGame);
            _meter.Increment(handpayAmount + gameWinAmount);
            _bingoTransactionReportHandler.Verify(
                x => x.AddNewTransactionToQueue(
                    TransactionType.LargeWin,
                    handpayAmount.MillicentsToCents(),
                    titleId,
                    denominationId,
                    gameSerial,
                    paytableId,
                    barcode),
                Times.Once);
            _bingoTransactionReportHandler.Verify(
                x => x.AddNewTransactionToQueue(
                    TransactionType.CashWon,
                    gameWinAmount.MillicentsToCents(),
                    titleId,
                    denominationId,
                    gameSerial,
                    paytableId,
                    string.Empty),
                Times.Once);
        }

        private EgmPaidGameWinBonusAmountMeterMonitor CreateTarget(
            bool meterNull = false,
            bool bingoGameNull = false,
            bool reportingNull = false,
            bool bonusHandlerNull = false,
            bool gameHistoryNull = false,
            bool transactionHistoryNull = false,
            bool eventBusNull = false)
        {
            return new EgmPaidGameWinBonusAmountMeterMonitor(
                meterNull ? null : _meterManager.Object,
                bingoGameNull ? null : _bingoGameProvider.Object,
                reportingNull ? null : _bingoTransactionReportHandler.Object,
                bonusHandlerNull ? null : _bonusHandler.Object,
                gameHistoryNull ? null : _gameHistory.Object,
                transactionHistoryNull ? null : _transactionHistory.Object,
                eventBusNull ? null : _eventBus.Object);
        }
    }
}