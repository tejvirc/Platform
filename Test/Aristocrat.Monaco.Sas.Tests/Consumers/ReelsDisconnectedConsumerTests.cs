namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using System;
    using Hardware.Contracts.Reel.Events;
    using Aristocrat.Sas.Client;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Consumers;
    using Test.Common;

    [TestClass]
    public class ReelsDisconnectedConsumerTests
    {
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private ReelsDisconnectedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Default);
            _target = new ReelsDisconnectedConsumer(_exceptionHandler.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullExceptionHandlerTest()
        {
            _target = new ReelsDisconnectedConsumer(null);
        }

        [TestMethod]
        public void ConsumeTest()
        {
            _target.Consume(new DisconnectedEvent(1));
            _exceptionHandler.Verify(x => x.ReportException(It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.ReelMechanismDisconnected)), Times.AtLeastOnce);
        }
    }
}
