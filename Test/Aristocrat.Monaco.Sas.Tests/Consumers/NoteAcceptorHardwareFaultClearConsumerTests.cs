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
    public class NoteAcceptorHardwareFaultClearConsumerTests
    {
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private NoteAcceptorHardwareFaultClearConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ISharedConsumer>(MockBehavior.Default);
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Default);
            _target = new NoteAcceptorHardwareFaultClearConsumer(_exceptionHandler.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullExceptionHandler()
        {
            _target = new NoteAcceptorHardwareFaultClearConsumer(null);
        }

        [TestMethod]
        public void ConsumeTest()
        {
            _exceptionHandler.Setup(
                    x => x.ReportException(
                        It.Is<ISasExceptionCollection>(
                            ex => ex.ExceptionCode == GeneralExceptionCode.CashBoxWasInstalled)))
                .Verifiable();
            _target.Consume(new HardwareFaultClearEvent(NoteAcceptorFaultTypes.StackerDisconnected));

            _exceptionHandler.Verify();
        }
    }
}