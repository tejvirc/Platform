namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Metering;
    using Gaming.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    /// <summary>
    ///     Tests for the ReadMultipleMetersHandler class
    /// </summary>
    [TestClass]
    public class ReadMultipleMetersHandlerTest
    {
        private ReadMultipleMetersHandler _target;
        private Mock<IMeterManager> _meterManager;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _meterManager = new Mock<IMeterManager>(MockBehavior.Default);

            _target = new ReadMultipleMetersHandler(_meterManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullMeterManagerTest()
        {
            _target = new ReadMultipleMetersHandler(null);
        }

        [TestMethod]
        public void CommandsTest()
        {
            // TODO: change the expected value and add checks for new long polls as we add more
            //       multi-meter read long polls
            Assert.AreEqual(5, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendMeters10Thru15));
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendMeters11Thru15));
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendGamesSincePowerUpLastDoorMeter));
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendBillCountMeters));
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendMeters));
        }

        [TestMethod]
        public void HandleTest()
        {
            var data = new LongPollReadMultipleMetersData();
            data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalCanceledCredits, MeterType.Lifetime));
            data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalCoinIn, MeterType.Lifetime));
            data.Meters.Add(new LongPollReadMeterData(SasMeters.TotalJackpot, MeterType.Lifetime));
            data.AccountingDenom = 1;

            const long expectedTotalOutMilliCents = 12345678000000;
            const long expectedWageredAmountMilliCents = 23456789000000;

            var testClassification = new TestMeterClassification();

            _meterManager.Setup(m => m.IsMeterProvided(SasMeterNames.TotalCanceledCredits)).Returns(true);
            var canceledCreditsMeter = new Mock<IMeter>();
            canceledCreditsMeter.Setup(m => m.Lifetime).Returns(expectedTotalOutMilliCents);
            canceledCreditsMeter.Setup(m => m.Classification).Returns(testClassification);
            _meterManager.Setup(m => m.GetMeter(SasMeterNames.TotalCanceledCredits)).Returns(canceledCreditsMeter.Object);

            _meterManager.Setup(m => m.IsMeterProvided(GamingMeters.WageredAmount)).Returns(true);
            var coinInMeter = new Mock<IMeter>();
            coinInMeter.Setup(m => m.Lifetime).Returns(expectedWageredAmountMilliCents);
            coinInMeter.Setup(m => m.Classification).Returns(testClassification);
            _meterManager.Setup(m => m.GetMeter(GamingMeters.WageredAmount)).Returns(coinInMeter.Object);

            // mocks for jackpot meter with value 0
            _meterManager.Setup(m => m.IsMeterProvided(GamingMeters.HandPaidTotalWonAmount)).Returns(false);

            var expected = new LongPollReadMultipleMetersResponse();
            expected.Meters.Add(
                SasMeters.TotalCanceledCredits,
                new LongPollReadMeterResponse(
                    SasMeters.TotalCanceledCredits,
                    (ulong)expectedTotalOutMilliCents.MillicentsToCents()));
            expected.Meters.Add(
                SasMeters.TotalCoinIn,
                new LongPollReadMeterResponse(
                    SasMeters.TotalCoinIn,
                    (ulong)expectedWageredAmountMilliCents.MillicentsToCents()));
            expected.Meters.Add(SasMeters.TotalJackpot, new LongPollReadMeterResponse(SasMeters.TotalJackpot, 0));

            var actual = _target.Handle(data);

            Assert.AreEqual(expected.Meters[SasMeters.TotalCanceledCredits].MeterValue, actual.Meters[SasMeters.TotalCanceledCredits].MeterValue);
            Assert.AreEqual(expected.Meters[SasMeters.TotalCoinIn].MeterValue, actual.Meters[SasMeters.TotalCoinIn].MeterValue);
            Assert.AreEqual(expected.Meters[SasMeters.TotalJackpot].MeterValue, actual.Meters[SasMeters.TotalJackpot].MeterValue);
            Assert.AreEqual(SasMeters.TotalCanceledCredits, actual.Meters[SasMeters.TotalCanceledCredits].Meter);
        }
    }
}
