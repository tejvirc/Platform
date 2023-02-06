namespace Aristocrat.Monaco.Bingo.Tests.Monitors
{
    using System;
    using Application.Contracts;
    using Bingo.Monitors;
    using Bingo.Services.Reporting;
    using Common.Storage;
    using Gaming.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using TransactionType = Common.TransactionType;

    [TestClass]
    public class PlayedCountMeterMonitorTests
    {
        private static readonly TestClassification MockClassification = new();
        private readonly Mock<IReportTransactionQueueService> _bingoTransactionReportHandler = new(MockBehavior.Default);
        private readonly Mock<IMeterManager> _meterManager = new(MockBehavior.Default);
        private readonly Mock<IBingoGameProvider> _bingoGameProvider = new(MockBehavior.Default);

        private TestMeter _meter;
        private PlayedCountMeterMonitor _target;

        [TestInitialize]
        public void Initialize()
        {
            _meter = new TestMeter(GamingMeters.PlayedCount, MockClassification);
            _meterManager.Setup(m => m.GetMeter(GamingMeters.PlayedCount)).Returns(_meter);
            _target = CreateTarget();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
        }

        [DataRow(true, false, false, DisplayName = "MeterManager Null")]
        [DataRow(false, true, false, DisplayName = "BingoGameProvider Null")]
        [DataRow(false, false, true, DisplayName = "TransactionReportHandler Null")]
        [DataTestMethod]
        public void NullConstructorParametersTest(
            bool meterNull,
            bool bingoGameNull,
            bool reportingNull)
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => _ = CreateTarget(meterNull, bingoGameNull, reportingNull));
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

            _bingoGameProvider.Setup(x => x.GetBingoGame()).Returns(bingoGame);
            _meter.Increment(amount);
            _bingoTransactionReportHandler.Verify(
                x => x.AddNewTransactionToQueue(
                    TransactionType.GamesPlayed,
                    amount,
                    titleId,
                    denominationId,
                    gameSerial,
                    paytableId,
                    string.Empty),
                Times.Once);
        }

        private PlayedCountMeterMonitor CreateTarget(
            bool meterNull = false,
            bool bingoGameNull = false,
            bool reportingNull = false)
        {
            return new PlayedCountMeterMonitor(
                meterNull ? null : _meterManager.Object,
                bingoGameNull ? null : _bingoGameProvider.Object,
                reportingNull ? null : _bingoTransactionReportHandler.Object);
        }
    }
}