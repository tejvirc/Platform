namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    [TestClass]
    public class LP1ASendCurrentCreditsHandlerTest
    {
        private Mock<IBank> _bank;
        private Mock<IPropertiesManager> _propertiesManager;
        private LP1ASendCurrentCreditsHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _bank = new Mock<IBank>(MockBehavior.Default);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);

            _target = new LP1ASendCurrentCreditsHandler(_bank.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullBankTest()
        {
            _target = new LP1ASendCurrentCreditsHandler(null);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendCurrentCredits));
        }

        [TestMethod]
        public void HandleTest()
        {
            const long expectedAmountMilliCents = 1234560000;
            var data = new LongPollReadMeterData(SasMeters.CurrentCredits, MeterType.Lifetime) { AccountingDenom = 1 };
            _bank.Setup(m => m.QueryBalance()).Returns(expectedAmountMilliCents);

            var expected = new LongPollReadMeterResponse(SasMeters.CurrentCredits, (ulong)expectedAmountMilliCents.MillicentsToCents());

            var actual = _target.Handle(data);

            Assert.AreEqual(expected.Meter, actual.Meter);
            Assert.AreEqual(expected.MeterValue, actual.MeterValue);
        }
    }
}
