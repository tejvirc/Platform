﻿namespace Aristocrat.Monaco.Bingo.Tests.Services.Reporting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Common.GameOverlay;
    using Gaming.Contracts;
    using Gaming.Contracts.Meters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using TransactionType = Common.TransactionType;
    using Common.Storage;
    using Gaming.Contracts.Central;

    [TestClass]
    public class MeterChangeMonitorTests
    {
        private static readonly TestClassification MockClassification = new();
        private readonly Mock<IReportTransactionQueueService> _bingoTransactionReportHandler = new(MockBehavior.Default);
        private readonly Mock<IGameMeterManager> _meterManager = new(MockBehavior.Default);
        private readonly Mock<ICentralProvider> _centralProvider = new(MockBehavior.Default);

        private MeterChangeMonitor _target;

        private TestMeter _cashPlayed;
        private TestMeter _cashWon;
        private TestMeter _gamesPlayed;
        private TestMeter _gamesWon;

        [TestInitialize]
        public void Initialize()
        {
            _cashPlayed = new TestMeter(GamingMeters.WageredAmount, MockClassification);
            _cashWon = new TestMeter(GamingMeters.EgmPaidGameWonAmount, MockClassification);
            _gamesPlayed = new TestMeter(GamingMeters.PlayedCount, MockClassification);
            _gamesWon = new TestMeter(GamingMeters.WonCount, MockClassification);

            _meterManager.Setup(m => m.GetMeter(GamingMeters.WageredAmount)).Returns(_cashPlayed);
            _meterManager.Setup(m => m.GetMeter(GamingMeters.TotalEgmPaidGameWonAmount)).Returns(_cashWon);
            _meterManager.Setup(m => m.GetMeter(GamingMeters.PlayedCount)).Returns(_gamesPlayed);
            _meterManager.Setup(m => m.GetMeter(GamingMeters.WonCount)).Returns(_gamesWon);

            var transactions = new List<CentralTransaction>
            {
                new (0, DateTime.UtcNow, 1, 1000, string.Empty, 1000, 1)
                {
                    Descriptions = new List<IOutcomeDescription>
                    {
                        new BingoGameDescription
                        {
                            Patterns = Enumerable.Empty<BingoPattern>(),
                            GameTitleId = 0,
                            GameSerial = 0,
                            DenominationId = 0
                        }
                    }
                }
            };
            
            _centralProvider.Setup(m => m.Transactions).Returns(transactions);

            _target = new MeterChangeMonitor(
                _meterManager.Object,
                _bingoTransactionReportHandler.Object,
                _centralProvider.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _target.Dispose();
        }

        [DataRow(true, false, false, DisplayName = "MeterManager Null")]
        [DataRow(false, true, false, DisplayName = "TransactionReportHandler Null")]
        [DataRow(false, false, true, DisplayName = "TransactionHistory Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool meterNull, bool reportingNull, bool historyNull)
        {
            _target = new MeterChangeMonitor(
                meterNull ? null : _meterManager.Object,
                reportingNull ? null : _bingoTransactionReportHandler.Object,
                historyNull ? null : _centralProvider.Object);
        }

        [TestMethod]
        public void OnCashPlayedChangedTest()
        {
            const long amount = 123000;
            _bingoTransactionReportHandler.Setup(
                m => m.AddNewTransactionToQueue(
                    TransactionType.CashPlayed,
                    amount.MillicentsToCents(),
                    0,
                    0,
                    0,
                    0))
                .Verifiable();

            _cashPlayed.Increment(amount);
            _bingoTransactionReportHandler.Verify();
        }

        [TestMethod]
        public void OnCashWonChangedTest()
        {
            const long amount = 123000;
            _bingoTransactionReportHandler.Setup(
                m => m.AddNewTransactionToQueue(
                    TransactionType.CashWon,
                    amount.MillicentsToCents(),
                    0,
                    0,
                    0,
                    0))
                .Verifiable();

            _cashWon.Increment(amount);
            _bingoTransactionReportHandler.Verify();
        }

        [TestMethod]
        public void OnGamesPlayedChangedTest()
        {
            const long amount = 1230;
            _bingoTransactionReportHandler.Setup(
                m => m.AddNewTransactionToQueue(
                    TransactionType.GamesPlayed,
                    amount,
                    0,
                    0,
                    0,
                    0))
                .Verifiable();

            _gamesPlayed.Increment(amount);
            _bingoTransactionReportHandler.Verify();
        }

        [TestMethod]
        public void OnGamesWonChangedTest()
        {
            const long amount = 345;
            _bingoTransactionReportHandler.Setup(
                m => m.AddNewTransactionToQueue(
                    TransactionType.GamesWon,
                    amount,
                    0,
                    0,
                    0,
                    0))
                .Verifiable();
            
            _gamesWon.Increment(amount);
            _bingoTransactionReportHandler.Verify();
        }

        [TestMethod]
        public void OnCashPlayedChangedWithZeroAmountTest()
        {
            const long amount = 0;
            _bingoTransactionReportHandler.Setup(
                m => m.AddNewTransactionToQueue(
                    TransactionType.CashPlayed,
                    amount.MillicentsToCents(),
                    0,
                    0,
                    0,
                    0))
                .Verifiable();

            _cashPlayed.Increment(amount);
            _bingoTransactionReportHandler.Verify(
                m => m.AddNewTransactionToQueue(
                    TransactionType.CashPlayed,
                    amount.MillicentsToCents(),
                    0,
                    0,
                    0,
                    0), Times.Never);
        }

        [TestMethod]
        public void UpdateGameInformationNoWinsTest()
        {
            const long amount = 55555L;
            const long gameSerial = 123456L;
            const uint gameTitleId = 12;
            const int denominationId = 3;
            var transaction = new List<CentralTransaction>
            {
                new (0, DateTime.UtcNow, 1, 1000, string.Empty, 1000, 1)
                {
                    Descriptions = new List<IOutcomeDescription>
                    {
                        new BingoGameDescription
                        {
                            Patterns = Enumerable.Empty<BingoPattern>(),
                            GameTitleId = gameTitleId,
                            GameSerial = gameSerial,
                            DenominationId = denominationId
                        }
                    }
                }
            };

            _centralProvider.Setup(m => m.Transactions).Returns(transaction);
            
            _bingoTransactionReportHandler.Setup(
                m => m.AddNewTransactionToQueue(
                    TransactionType.GamesWon,
                    amount,
                    gameSerial,
                    gameTitleId,
                    0,
                    denominationId))
                .Verifiable();

            _gamesWon.Increment(amount);
            _bingoTransactionReportHandler.Verify(
                m => m.AddNewTransactionToQueue(
                    TransactionType.GamesWon,
                    amount,
                    gameSerial,
                    gameTitleId,
                    0,
                    denominationId), Times.Once);
        }

        [TestMethod]
        public void UpdateGameInformationTest()
        {
            var amount = 55555L;
            var gameSerial = 123456L;
            uint gameTitleId = 12;
            var denominationId = 3;
            var paytableId = 44;
            var transaction = new List<CentralTransaction>
            {
                new (0, DateTime.UtcNow, 1, 1000, string.Empty, 1000, 1)
                {
                    Descriptions = new List<IOutcomeDescription>
                    {
                        new BingoGameDescription
                        {
                            Patterns = new List<BingoPattern> { new(string.Empty, 1, 123, 100, 20, paytableId, false, 0x80, 1) },
                            GameTitleId = gameTitleId,
                            GameSerial = gameSerial,
                            DenominationId = denominationId
                        }
                    }
                }
            };

            _centralProvider.Setup(m => m.Transactions).Returns(transaction);
            
            _bingoTransactionReportHandler.Setup(
                m => m.AddNewTransactionToQueue(
                    TransactionType.GamesWon,
                    amount,
                    gameSerial,
                    gameTitleId,
                    paytableId,
                    denominationId))
                .Verifiable();

            _gamesWon.Increment(amount);

            _bingoTransactionReportHandler.Verify(
                m => m.AddNewTransactionToQueue(
                    TransactionType.GamesWon,
                    amount,
                    gameSerial,
                    gameTitleId,
                    paytableId,
                    denominationId), Times.Once);
        }

        private class TestMeter : IMeter
        {
            public TestMeter(string name, MeterClassification classification)
            {
                Name = name;
                Classification = classification;
            }

            public string Name { get; set; }

            public MeterClassification Classification { get; set; }

            public long Lifetime { get; set; }

            public long Period => throw new NotImplementedException();

            public long Session => throw new NotImplementedException();

            public event EventHandler<MeterChangedEventArgs> MeterChangedEvent;

            public void Increment(long amount)
            {
                Lifetime = amount;
                MeterChangedEvent?.Invoke(this, new MeterChangedEventArgs(amount));
            }
        }

        private class TestClassification : MeterClassification
        {
            public TestClassification() : base("Currency", 1000) { }
            public override string CreateValueString(long meterValue) => string.Empty;
        }
    }
}