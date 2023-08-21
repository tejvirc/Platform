namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Consumers;
    using Test.Common;

    [TestClass]
    public class NoteAcceptorDocumentRejectedConsumerTests
    {
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private Mock<ISasNoteAcceptorProvider> _sasNoteAcceptorProvider;
        private NoteAcceptorDocumentRejectedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ISharedConsumer>(MockBehavior.Default);
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Default);
            _sasNoteAcceptorProvider = new Mock<ISasNoteAcceptorProvider>(MockBehavior.Default);
            _sasNoteAcceptorProvider.SetupGet(s => s.DiagnosticTestActive).Returns(false);
            _target = new NoteAcceptorDocumentRejectedConsumer(_exceptionHandler.Object, _sasNoteAcceptorProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullExceptionHandler()
        {
            _target = new NoteAcceptorDocumentRejectedConsumer(null, null);
        }

        [TestMethod]
        public void ConsumeTest()
        {
            _exceptionHandler.Setup(
                    x => x.ReportException(
                        It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.BillRejected)))
                .Verifiable();
            _target.Consume(new DocumentRejectedEvent());

            _exceptionHandler.Verify();
        }
    }
}