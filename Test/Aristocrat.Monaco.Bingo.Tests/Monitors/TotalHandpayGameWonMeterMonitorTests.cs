namespace Aristocrat.Monaco.Bingo.Tests.Monitors
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Bingo.Monitors;
    using Common.Storage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using TransactionType = Common.TransactionType;

    [TestClass]
    public class TotalHandpayGameWonMeterMonitorTests
    {
        private static readonly TestClassification MockClassification = new();
        private readonly Mock<IReportTransactionQueueService> _bingoTransactionReportHandler = new(MockBehavior.Default);
        private readonly Mock<IMeterManager> _meterManager = new(MockBehavior.Default);
        private readonly Mock<IBingoGameProvider> _bingoGameProvider = new(MockBehavior.Default);
        private readonly Mock<ITransactionHistory> _transactionHistory = new(MockBehavior.Default);

        private TestMeter _meter;
        private TotalHandpayGameWonMeterMonitor _target;

        [TestInitialize]
        public void Initialize()
        {
            _meter = new TestMeter(AccountingMeters.TotalHandpaidGameWonAmount, MockClassification);
            _meterManager.Setup(m => m.GetMeter(AccountingMeters.TotalHandpaidGameWonAmount)).Returns(_meter);
            _target = CreateTarget();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
        }

        [DataRow(true, false, false, false, DisplayName = "MeterManager Null")]
        [DataRow(false, true, false, false, DisplayName = "BingoGameProvider Null")]
        [DataRow(false, false, true, false, DisplayName = "TransactionReportHandler Null")]
        [DataRow(false, false, false, true, DisplayName = "TransactionHistory Null")]
        [DataTestMethod]
        public void NullConstructorParametersTest(
            bool meterNull,
            bool bingoGameNull,
            bool reportingNull,
            bool transactionHistoryNull)
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => _ = CreateTarget(meterNull, bingoGameNull, reportingNull, transactionHistoryNull));
        }

        [TestMethod]
        public void NoBingoGameFoundTest()
        {
            const long amount = 12340000;
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
        public void ZeroIncrementTest()
        {
            const long amount = 0;
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
        public void ReportTransactionTest()
        {
            const long amount = 12340000;
            const int paytableId = 123;
            const string barcode = "TestBarcode";
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
                    Barcode = barcode
                }
            };

            _transactionHistory.Setup(x => x.RecallTransactions<HandpayTransaction>()).Returns(handpayTransactions);
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

        private TotalHandpayGameWonMeterMonitor CreateTarget(
            bool meterNull = false,
            bool bingoGameNull = false,
            bool reportingNull = false,
            bool transactionHistoryNull = false)
        {
            return new TotalHandpayGameWonMeterMonitor(
                meterNull ? null : _meterManager.Object,
                bingoGameNull ? null : _bingoGameProvider.Object,
                reportingNull ? null : _bingoTransactionReportHandler.Object,
                transactionHistoryNull ? null : _transactionHistory.Object);
        }
    }
}