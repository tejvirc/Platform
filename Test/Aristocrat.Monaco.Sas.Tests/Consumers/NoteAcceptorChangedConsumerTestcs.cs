namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Consumers;
    using Test.Common;

    [TestClass]
    public class NoteAcceptorChangedConsumerTest
    {
        private Mock<ISasExceptionHandler> _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Strict);
        private NoteAcceptorChangedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _target = new NoteAcceptorChangedConsumer(_exceptionHandler.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullHandlerTest()
        {
            _target = new NoteAcceptorChangedConsumer(null);

            // test passes if exception was thrown
        }

        [TestMethod]
        public void ConsumeTest()
        {
            _exceptionHandler.Setup(m => m.ReportException(It.IsAny<ISasExceptionCollection>())).Verifiable();

            _target.Consume(new NoteAcceptorChangedEvent());

            _exceptionHandler.Verify();
        }
    }
}
