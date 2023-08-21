namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Metering;
    using Gaming.Contracts;
    using Gaming.Contracts.Meters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    /// <summary>
    ///     Tests for the ReadSingleMeterHandler class
    /// </summary>
    [TestClass]
    public class ReadSingleMeterHandlerTests
    {
        private ReadSingleMeterHandler _target;
        private Mock<IMeterManager> _meterManager;
        private Mock<IGameMeterManager> _gameMeterManager;
        private Mock<IGameProvider> _gameProvider;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _meterManager = new Mock<IMeterManager>(MockBehavior.Default);
            _gameMeterManager = new Mock<IGameMeterManager>(MockBehavior.Default);
            _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);

            _gameProvider.Setup(m => m.GetAllGames()).Returns(new List<IGameDetail>());

            _target = new ReadSingleMeterHandler(_meterManager.Object, _gameMeterManager.Object, _gameProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullMeterManagerTest()
        {
            _target = new ReadSingleMeterHandler(null, _gameMeterManager.Object, _gameProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullGameMeterManagerTest()
        {
            _target = new ReadSingleMeterHandler(_meterManager.Object, null, _gameProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullGameProviderTest()
        {
            _target = new ReadSingleMeterHandler(_meterManager.Object, _gameMeterManager.Object, null);
        }

        [DataRow(LongPoll.SendCanceledCreditsMeter)]
        [DataRow(LongPoll.SendCoinInMeter)]
        [DataRow(LongPoll.SendCoinOutMeter)]
        [DataRow(LongPoll.SendDropMeter)]
        [DataRow(LongPoll.SendJackpotMeter)]
        [DataRow(LongPoll.SendGamesWonMeter)]
        [DataRow(LongPoll.SendGamesLostMeter)]
        [DataRow(LongPoll.SendTrueCoinIn)]
        [DataRow(LongPoll.SendTrueCoinOut)]
        [DataRow(LongPoll.SendCurrentHopperLevel)]
        [DataRow(LongPoll.SendOneDollarInMeter)]
        [DataRow(LongPoll.SendTwoDollarInMeter)]
        [DataRow(LongPoll.SendFiveDollarInMeter)]
        [DataRow(LongPoll.SendTenDollarInMeter)]
        [DataRow(LongPoll.SendTwentyDollarInMeter)]
        [DataRow(LongPoll.SendFiftyDollarInMeter)]
        [DataRow(LongPoll.SendOneHundredDollarInMeter)]
        [DataRow(LongPoll.SendTwoHundredDollarInMeter)]
        [DataRow(LongPoll.SendTwentyFiveDollarInMeter)]
        [DataRow(LongPoll.SendTwoThousandDollarInMeter)]
        [DataRow(LongPoll.SendTwoThousandFiveHundredDollarInMeter)]
        [DataRow(LongPoll.SendFiveThousandDollarInMeter)]
        [DataRow(LongPoll.SendTenThousandDollarInMeter)]
        [DataRow(LongPoll.SendTwentyThousandDollarInMeter)]
        [DataRow(LongPoll.SendTwentyFiveThousandDollarInMeter)]
        [DataRow(LongPoll.SendFiftyThousandDollarInMeter)]
        [DataRow(LongPoll.SendOneHundredThousandDollarInMeter)]
        [DataRow(LongPoll.SendTwoHundredFiftyDollarInMeter)]
        [DataRow(LongPoll.SendCoinAcceptedFromExternalAcceptor)]
        [DataRow(LongPoll.SendNumberOfBillsInStacker)]
        [DataRow(LongPoll.SendCreditAmountOfBillsInStacker)]
        [DataTestMethod]
        public void CommandsTest(LongPoll longPoll)
        {
            Assert.IsTrue(_target.Commands.Contains(longPoll), $"{longPoll} not handled by ReadSingleMeterHandler");
        }

        [TestMethod]
        public void HandleMultiDenomMeterRead()
        {
            const long denom = 1;
            var data = new LongPollReadMeterData(SasMeters.TotalJackpot, MeterType.Lifetime)
            {
                TargetDenomination = denom,
                MultiDenomPoll = true,
                AccountingDenom = 1
            };

            const long expectedResponseMilliCents = 12345678000;
            var testClassification = new TestMeterClassification();
            _gameMeterManager.Setup(m => m.IsMeterProvided(denom.CentsToMillicents(), GamingMeters.TotalHandPaidAmt)).Returns(true);
            var totalWon = new Mock<IMeter>();
            totalWon.Setup(m => m.Lifetime).Returns(expectedResponseMilliCents);
            totalWon.Setup(m => m.Classification).Returns(testClassification);
            _gameMeterManager.Setup(m => m.GetMeter(denom.CentsToMillicents(), GamingMeters.TotalHandPaidAmt)).Returns(totalWon.Object);

            var expected = new LongPollReadMeterResponse(SasMeters.TotalJackpot, (ulong)expectedResponseMilliCents.MillicentsToCents());

            var actual = _target.Handle(data);

            Assert.AreEqual(expected.Meter, actual.Meter);
            Assert.AreEqual(expected.MeterValue, actual.MeterValue);
        }

        [TestMethod]
        public void HandleTestCurrencyMeter()
        {
            var data = new LongPollReadMeterData(SasMeters.TotalCanceledCredits, MeterType.Lifetime)
            {
                AccountingDenom = 1
            };

            const long expectedResponseMilliCents = 12345678000;
            var testClassification = new TestMeterClassification();
            _meterManager.Setup(m => m.IsMeterProvided(SasMeterNames.TotalCanceledCredits)).Returns(true);
            var canceledCreditsMeter = new Mock<IMeter>();
            canceledCreditsMeter.Setup(m => m.Lifetime).Returns(expectedResponseMilliCents);
            canceledCreditsMeter.Setup(m => m.Classification).Returns(testClassification);
            _meterManager.Setup(m => m.GetMeter(SasMeterNames.TotalCanceledCredits)).Returns(canceledCreditsMeter.Object);

            var expected = new LongPollReadMeterResponse(SasMeters.TotalCanceledCredits, (ulong)expectedResponseMilliCents.MillicentsToCents());

            var actual = _target.Handle(data);

            Assert.AreEqual(expected.Meter, actual.Meter);
            Assert.AreEqual(expected.MeterValue, actual.MeterValue);
        }

        [TestMethod]
        public void HandleTestDollarMeter()
        {
            var data = new LongPollReadMeterData(SasMeters.TotalBillIn, MeterType.Lifetime)
            {
                AccountingDenom = 1
            };

            const long expectedResponseMilliCents = 12345600000;
            var testClassification = new TestMeterClassification();
            _meterManager.Setup(m => m.IsMeterProvided(AccountingMeters.CurrencyInAmount)).Returns(true);
            var currencyInMeter = new Mock<IMeter>();
            currencyInMeter.Setup(m => m.Lifetime).Returns(expectedResponseMilliCents);
            currencyInMeter.Setup(m => m.Classification).Returns(testClassification);
            _meterManager.Setup(m => m.GetMeter(AccountingMeters.CurrencyInAmount)).Returns(currencyInMeter.Object);

            var expected = new LongPollReadMeterResponse(SasMeters.TotalBillIn, (ulong)Math.Floor(expectedResponseMilliCents.MillicentsToDollars()));

            var actual = _target.Handle(data);

            Assert.AreEqual(expected.Meter, actual.Meter);
            Assert.AreEqual(expected.MeterValue, actual.MeterValue);
        }

        [TestMethod]
        public void HandleTestOccurenceMeter()
        {
            var data = new LongPollReadMeterData(SasMeters.DollarIn1, MeterType.Lifetime) { AccountingDenom = 1 };

            const ulong expectedResponse = 125305678;
            var testClassification = new TestMeterClassification();
            _meterManager.Setup(m => m.IsMeterProvided(AccountingMeters.BillCount1s)).Returns(true);
            var canceledCreditsMeter = new Mock<IMeter>();
            canceledCreditsMeter.Setup(m => m.Lifetime).Returns((long)expectedResponse);
            canceledCreditsMeter.Setup(m => m.Classification).Returns(testClassification);
            _meterManager.Setup(m => m.GetMeter(AccountingMeters.BillCount1s)).Returns(canceledCreditsMeter.Object);

            var expected = new LongPollReadMeterResponse(SasMeters.DollarIn1, expectedResponse);

            var actual = _target.Handle(data);

            Assert.AreEqual(expected.Meter, actual.Meter);
            Assert.AreEqual(expected.MeterValue, actual.MeterValue);
        }
    }
}
