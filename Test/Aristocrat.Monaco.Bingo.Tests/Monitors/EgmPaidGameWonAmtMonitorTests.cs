﻿namespace Aristocrat.Monaco.Bingo.Tests.Monitors
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.TransferOut;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Bingo.Monitors;
    using Bingo.Services.Reporting;
    using Common.Storage;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using TransactionType = Common.TransactionType;

    [TestClass]
    public class EgmPaidGameWonAmtMonitorTests
    {
        private static readonly TestClassification MockClassification = new();
        private readonly Mock<IReportTransactionQueueService> _bingoTransactionReportHandler = new(MockBehavior.Default);
        private readonly Mock<IMeterManager> _meterManager = new(MockBehavior.Default);
        private readonly Mock<IBingoGameProvider> _bingoGameProvider = new(MockBehavior.Default);
        private readonly Mock<ITransactionHistory> _transactionHistory = new(MockBehavior.Default);
        private readonly Mock<IGameHistory> _gameHistory = new(MockBehavior.Default);
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);

        private TestMeter _meter;
        private EgmPaidGameWonAmtMeterMonitor _target;

        [TestInitialize]
        public void Initialize()
        {
            _meter = new TestMeter(GamingMeters.EgmPaidGameWonAmount, MockClassification);
            _meterManager.Setup(m => m.GetMeter(GamingMeters.EgmPaidGameWonAmount)).Returns(_meter);
            _target = CreateTarget();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
        }

        [DataRow(true, false, false, false, false, false)]
        [DataRow(false, true, false, false, false, false)]
        [DataRow(false, false, true, false, false, false)]
        [DataRow(false, false, false, true, false, false)]
        [DataRow(false, false, false, false, true, false)]
        [DataRow(false, false, false, false, false, true)]
        [DataTestMethod]
        public void NullConstructorParametersTest(
            bool meterNull,
            bool bingoGameNull,
            bool reportingNull,
            bool transactionHistoryNull,
            bool gameHistoryNull,
            bool eventBusNull)
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => _ = CreateTarget(
                    meterNull,
                    bingoGameNull,
                    reportingNull,
                    transactionHistoryNull,
                    gameHistoryNull,
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
            var traceId = Guid.NewGuid();
            var bingoGame = new BingoGameDescription
            {
                Paytable = paytableId.ToString(),
                GameSerial = gameSerial,
                DenominationId = denominationId,
                GameTitleId = titleId
            };

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
                    TraceId = traceId,
                    KeyOffType = KeyOffType.LocalCredit,
                    KeyOffCashableAmount = amount
                }
            };

            _transactionHistory.Setup(x => x.RecallTransactions<HandpayTransaction>()).Returns(handpayTransactions);

            var gameHistoryLog = new Mock<IGameHistoryLog>(MockBehavior.Default);
            _gameHistory.Setup(x => x.CurrentLog).Returns(gameHistoryLog.Object);
            gameHistoryLog.Setup(x => x.CashOutInfo).Returns(new CashOutInfo[]
            {
                new()
                {
                    Handpay = false,
                    Amount = amount,
                    TraceId = traceId,
                    Reason = TransferOutReason.LargeWin
                }
            });

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
            var traceId = Guid.NewGuid();
            var bingoGame = new BingoGameDescription
            {
                Paytable = paytableId.ToString(),
                GameSerial = gameSerial,
                DenominationId = denominationId,
                GameTitleId = titleId
            };

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
                    TraceId = traceId,
                    KeyOffType = KeyOffType.LocalCredit,
                    KeyOffCashableAmount = handpayAmount
                }
            };

            _transactionHistory.Setup(x => x.RecallTransactions<HandpayTransaction>()).Returns(handpayTransactions);

            var gameHistoryLog = new Mock<IGameHistoryLog>(MockBehavior.Default);
            _gameHistory.Setup(x => x.CurrentLog).Returns(gameHistoryLog.Object);
            gameHistoryLog.Setup(x => x.CashOutInfo).Returns(new CashOutInfo[]
            {
                new()
                {
                    Handpay = false,
                    Amount = handpayAmount,
                    TraceId = traceId,
                    Reason = TransferOutReason.LargeWin
                }
            });

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

        private EgmPaidGameWonAmtMeterMonitor CreateTarget(
            bool meterNull = false,
            bool bingoGameNull = false,
            bool reportingNull = false,
            bool transactionHistoryNull = false,
            bool gameHistoryNull = false,
            bool eventBusNull = false)
        {
            return new EgmPaidGameWonAmtMeterMonitor(
                meterNull ? null : _meterManager.Object,
                bingoGameNull ? null : _bingoGameProvider.Object,
                reportingNull ? null : _bingoTransactionReportHandler.Object,
                transactionHistoryNull ? null : _transactionHistory.Object,
                gameHistoryNull ? null : _gameHistory.Object,
                eventBusNull ? null : _eventBus.Object);
        }
    }
}