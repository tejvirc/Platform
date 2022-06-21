namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using System;
    using Accounting.Contracts.Handpay;
    using Contracts.Client;
    using Kernel;
    using Sas.Consumers;
    using Test.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class HandpayKeyedOffConsumerTests
    {
        private Mock<ISasHandPayCommittedHandler> _sasHandPayCommittedHandler;
        private HandpayKeyedOffConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _sasHandPayCommittedHandler = new Mock<ISasHandPayCommittedHandler>(MockBehavior.Default);
            _target = new HandpayKeyedOffConsumer(_sasHandPayCommittedHandler.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
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