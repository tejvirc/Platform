namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using System;
    using Accounting.Contracts.Handpay;
    using Aristocrat.Monaco.Sas.VoucherValidation;
    using Contracts.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Consumers;

    [TestClass]
    public class HandpayKeyedOffConsumerTests
    {
        private Mock<ISasHandPayCommittedHandler> _sasHandPayCommittedHandler;
        private HandpayKeyedOffConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _sasHandPayCommittedHandler = new Mock<ISasHandPayCommittedHandler>(MockBehavior.Default);
            _target = new HandpayKeyedOffConsumer(_sasHandPayCommittedHandler.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullSasHandPayCommittedHandler()
        {
            _target = new HandpayKeyedOffConsumer(null);
        }

        [TestMethod]
        public void HandpayKeyedOffEventTest()
        {
            _sasHandPayCommittedHandler.Setup(x => x.HandPayReset(It.IsAny<HandpayTransaction>())).Verifiable();
            _target.Consume(new HandpayKeyedOffEvent(new HandpayTransaction()));

            _sasHandPayCommittedHandler.Verify();
        }
    }
}