namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using System;
    using Application.Contracts.NoteAcceptorMonitor;
    using Aristocrat.Sas.Client;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Consumers;
    using Test.Common;

    [TestClass]
    public class NoteAcceptorDocumentCheckOccurredConsumerTests
    {
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private NoteAcceptorDocumentCheckOccurredConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ISharedConsumer>(MockBehavior.Default);
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Default);
            _target = new NoteAcceptorDocumentCheckOccurredConsumer(_exceptionHandler.Object);
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
            _target = new NoteAcceptorDocumentCheckOccurredConsumer(null);
        }

        [TestMethod]
        public void ConsumeTest()
        {
            _exceptionHandler.Setup(
                    x => x.ReportException(
                        It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.GeneralTilt)))
                .Verifiable();
            _target.Consume(new NoteAcceptorDocumentCheckOccurredEvent());
            _exceptionHandler.Verify();
        }
    }
}
