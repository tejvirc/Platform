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
    public class NoteAcceptorDisconnectedConsumerTests
    {
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private NoteAcceptorDisconnectedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ISharedConsumer>(MockBehavior.Default);
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Default);
            _target = new NoteAcceptorDisconnectedConsumer(_exceptionHandler.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullExceptionHandler()
        {
            _target = new NoteAcceptorDisconnectedConsumer(null);
        }

        [TestMethod]
        public void ConsumeTest()
        {
            _exceptionHandler.Setup(
                x => x.ReportException(
                    It.Is<ISasExceptionCollection>(
                        ex => ex.ExceptionCode == GeneralExceptionCode.BillAcceptorHardwareFailure))).Verifiable();
            _target.Consume(new DisconnectedEvent());

            _exceptionHandler.Verify();
        }
    }
}