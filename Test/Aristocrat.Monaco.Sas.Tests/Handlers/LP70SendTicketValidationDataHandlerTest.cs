namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Threading;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;
    using Sas.Ticketing;
    using Sas.Contracts;
    using Aristocrat.Monaco.Sas.Storage.Models;

    [TestClass]
    public class LP70SendTicketValidationDataHandlerTest
    {
        private const int TimeoutWait = 1000;  // one second

        private LP70SendTicketValidationDataHandler _target;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ISasVoucherInProvider> _sasVoucherInProvider;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _propertiesManager.Setup(c => c.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { ValidationType = SasValidationType.SecureEnhanced });
            _sasVoucherInProvider = new Mock<ISasVoucherInProvider>(MockBehavior.Default);
            _target = new LP70SendTicketValidationDataHandler(_propertiesManager.Object, _sasVoucherInProvider.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendTicketValidationData));
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullPropertiesManagerTest()
        {
            _target = new LP70SendTicketValidationDataHandler(null, _sasVoucherInProvider.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullSasVoucherInProviderTest()
        {
            _target = new LP70SendTicketValidationDataHandler(_propertiesManager.Object, null);
        }

        [TestMethod]
        public void HandleCommandWithInvalidValidationType() 
        {
            var waiter = new AutoResetEvent(false);
            _sasVoucherInProvider.Setup(x => x.CurrentRedemptionStatus)
                .Returns(RedemptionStatusCode.TicketRejectedByHost);
            _sasVoucherInProvider.Setup(x => x.DenyTicket())
                .Callback(() => waiter.Set())
                .Verifiable();
            _propertiesManager.Setup(c => c.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { ValidationType = SasValidationType.None });
            _target = new LP70SendTicketValidationDataHandler(_propertiesManager.Object, _sasVoucherInProvider.Object);
            Assert.AreEqual(null, _target.Handle(new LongPollData()));
            Assert.IsTrue(waiter.WaitOne(TimeoutWait));
            _sasVoucherInProvider.Verify();
        }

        [TestMethod]
        public void HandleCommandWithPendingState()
        {
            const string expectedBarcode = "123456789";
            _sasVoucherInProvider.Setup(x => x.RequestValidationData())
                .Returns(new SendTicketValidationDataResponse { Barcode = expectedBarcode, ParsingCode = ParsingCode.Bcd});
            _sasVoucherInProvider.Setup(x => x.CurrentState).Returns(SasVoucherInState.ValidationRequestPending);
            var response = _target.Handle(new LongPollData());
            Assert.AreEqual(expectedBarcode, response.Barcode);
            Assert.AreEqual(ParsingCode.Bcd, response.ParsingCode);
        }

        [TestMethod]
        public void HandleCommandWithOtherStateThanPending()
        {
            _sasVoucherInProvider.Setup(x => x.CurrentState).Returns(SasVoucherInState.Idle);
            var response = _target.Handle(new LongPollData());
            Assert.IsNull(response.Barcode);
            Assert.AreEqual(ParsingCode.Bcd, response.ParsingCode);
        }
    }
}
