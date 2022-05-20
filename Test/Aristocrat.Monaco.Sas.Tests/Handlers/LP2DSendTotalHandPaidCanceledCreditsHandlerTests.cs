namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    [TestClass]
    public class LP2DSendTotalHandPaidCanceledCreditsHandlerTests
    {
        private LP2DSendTotalHandPaidCanceledCreditsHandler _target;
        private Mock<IMeterManager> _meterManager;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _meterManager = new Mock<IMeterManager>(MockBehavior.Default);

            _target = new LP2DSendTotalHandPaidCanceledCreditsHandler(_meterManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullMeterManagerTest()
        {
            _target = new LP2DSendTotalHandPaidCanceledCreditsHandler(null);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendTotalHandPaidCanceledCredits));
        }

        [TestMethod]
        public void HandleTestGameId0()
        {
            var data = new SendTotalHandPaidCanceledCreditsData { GameId = 0, AccountingDenom = 1 };

            const long expectedAmountMilliCents = 123456780000;

            var testClassification = new TestMeterClassification();

            // mocks for canceled credits meter with value 12345678000
            _meterManager.Setup(m => m.IsMeterProvided(AccountingMeters.HandpaidCancelAmount)).Returns(true);
            var canceledCreditsMeter = new Mock<IMeter>();
            canceledCreditsMeter.Setup(m => m.Lifetime).Returns(expectedAmountMilliCents);
            canceledCreditsMeter.Setup(m => m.Classification).Returns(testClassification);
            _meterManager.Setup(m => m.GetMeter(AccountingMeters.HandpaidCancelAmount)).Returns(canceledCreditsMeter.Object);

            var actual = _target.Handle(data);

            Assert.IsTrue(actual.Succeeded);
            Assert.AreEqual((ulong)expectedAmountMilliCents.MillicentsToCents(), actual.MeterValue);
        }

        [TestMethod]
        public void HandleTestGameId1()
        {
            var data = new SendTotalHandPaidCanceledCreditsData { GameId = 1, AccountingDenom = 1 };

            var actual = _target.Handle(data);

            Assert.IsFalse(actual.Succeeded);
        }
    }
}