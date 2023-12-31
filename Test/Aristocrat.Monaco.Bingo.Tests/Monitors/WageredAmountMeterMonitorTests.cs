﻿namespace Aristocrat.Monaco.Bingo.Tests.Monitors
{
    using System;
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
    public class WageredAmountMeterMonitorTests
    {
        private static readonly TestClassification MockClassification = new();
        private readonly Mock<IReportTransactionQueueService> _bingoTransactionReportHandler = new(MockBehavior.Default);
        private readonly Mock<IMeterManager> _meterManager = new(MockBehavior.Default);
        private readonly Mock<IBingoGameProvider> _bingoGameProvider = new(MockBehavior.Default);
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);

        private TestMeter _meter;
        private WageredAmountMeterMonitor _target;

        [TestInitialize]
        public void Initialize()
        {
            _meter = new TestMeter(GamingMeters.WageredAmount, MockClassification);
            _meterManager.Setup(m => m.GetMeter(GamingMeters.WageredAmount)).Returns(_meter);
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
        [DataRow(false, false, false, true, DisplayName = "EventBus Null")]
        [DataTestMethod]
        public void NullConstructorParametersTest(
            bool meterNull,
            bool bingoGameNull,
            bool reportingNull,
            bool eventBusNull)
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => _ = CreateTarget(meterNull, bingoGameNull, reportingNull, eventBusNull));
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
                    TransactionType.CashPlayed,
                    amount.MillicentsToCents(),
                    titleId,
                    denominationId,
                    gameSerial,
                    paytableId,
                    string.Empty),
                Times.Once);
        }

        private WageredAmountMeterMonitor CreateTarget(
            bool meterNull = false,
            bool bingoGameNull = false,
            bool reportingNull = false,
            bool eventBusNull = false)
        {
            return new WageredAmountMeterMonitor(
                meterNull ? null : _meterManager.Object,
                bingoGameNull ? null : _bingoGameProvider.Object,
                reportingNull ? null : _bingoTransactionReportHandler.Object,
                eventBusNull ? null : _eventBus.Object);
        }
    }
}