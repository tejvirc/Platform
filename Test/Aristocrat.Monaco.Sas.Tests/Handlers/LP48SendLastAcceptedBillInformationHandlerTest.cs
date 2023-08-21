namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;
    using Test.Common;

    /// <summary>
    ///     Contains the unit tests for the LP48SendLastAcceptedBillHandlerTest class
    /// </summary>
    [TestClass]
    public class LP48SendLastAcceptedBillHandlerTest
    {
        private const long LastBillCount = 1234;
        private const BillDenominationCodes LastBillDenominationCode = BillDenominationCodes.Ten;
        private const BillAcceptorCountryCode LastBillAcceptorCountryCode = BillAcceptorCountryCode.Euro;
        private LP48SendLastAcceptedBillInformationHandler _target;
        private Mock<IMeterManager> _meterManager;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ITransactionHistory> _transactionHistory;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _transactionHistory = new Mock<ITransactionHistory>(MockBehavior.Default);
            _target = new LP48SendLastAcceptedBillInformationHandler(_meterManager.Object, _transactionHistory.Object);

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMultiplierKey, 1d)).Returns(100_000d);
            _transactionHistory.Setup(x => x.RecallTransactions<BillTransaction>()).Returns(new List<BillTransaction>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullIMeterMangerTest()
        {
            // test will fail if exception wasn't thrown 
            _target = new LP48SendLastAcceptedBillInformationHandler(null, _transactionHistory.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullIPropertiesManagerTest()
        {
            // test will fail if exception wasn't thrown 
            _target = new LP48SendLastAcceptedBillInformationHandler(_meterManager.Object, null);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendLastBillInformation));
        }

        [DataTestMethod]
        [DataRow(true, LastBillCount, LastBillCount, LastBillAcceptorCountryCode, LastBillDenominationCode, DisplayName = "Handle normal test")]
        [DataRow(false, LastBillCount, 0, LastBillAcceptorCountryCode, LastBillDenominationCode, DisplayName = "Handle meter not provided test")]
        [DataRow(true, -100, 0, 0, 0, DisplayName = "Handle meter negative value test")]
        public void HandleTest(
            bool meterProvided,
            long meterValue,
            long expectedMeterValue,
            BillAcceptorCountryCode billCountryCode,
            BillDenominationCodes billDenomincationCode)
        {
            var meterBill = new Mock<IMeter>();
            meterBill.Setup(m => m.Lifetime).Returns(meterValue);
            _meterManager.Setup(m => m.IsMeterProvided(AccountingMeters.BillCount10s)).Returns(meterProvided);
            _meterManager.Setup(m => m.GetMeter(AccountingMeters.BillCount10s)).Returns(meterBill.Object);
            var amount = new decimal(10).DollarsToMillicents();

            _transactionHistory.Setup(x => x.RecallTransactions<BillTransaction>())
                .Returns(new List<BillTransaction>
                {
                    new BillTransaction(ISOCurrencyCode.EUR.ToString().ToCharArray(), 0, DateTime.Now, amount)
                    {
                        State = CurrencyState.Accepted
                    }
                });

            var expected = new SendLastAcceptedBillInformationResponse(billCountryCode, billDenomincationCode, (ulong)expectedMeterValue);
            var actual = _target.Handle(new LongPollData());

            Assert.AreEqual(expected.CountryCode, actual.CountryCode);
            Assert.AreEqual(expected.DenominationCode, actual.DenominationCode);
            Assert.AreEqual(expected.Count, actual.Count);
        }
    }
}