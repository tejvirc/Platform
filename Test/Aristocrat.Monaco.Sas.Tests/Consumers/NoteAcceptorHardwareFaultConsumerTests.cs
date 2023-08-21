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
    public class NoteAcceptorHardwareFaultConsumerTests
    {
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private NoteAcceptorHardwareFaultConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ISharedConsumer>(MockBehavior.Default);
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Default);
            _target = new NoteAcceptorHardwareFaultConsumer(_exceptionHandler.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullExceptionHandler()
        {
            _target = new NoteAcceptorHardwareFaultConsumer(null);
        }

        [DataRow(NoteAcceptorFaultTypes.ComponentFault, GeneralExceptionCode.BillAcceptorHardwareFailure, DisplayName = "Component Fault Reports a Bill Acceptor Hardware Failure Exception")]
        [DataRow(NoteAcceptorFaultTypes.FirmwareFault, GeneralExceptionCode.BillAcceptorHardwareFailure, DisplayName = "Firmware Fault Reports a Bill Acceptor Hardware Failure Exception")]
        [DataRow(NoteAcceptorFaultTypes.MechanicalFault, GeneralExceptionCode.BillAcceptorHardwareFailure, DisplayName = "Mechanical Fault Reports a Bill Acceptor Hardware Failure Exception")]
        [DataRow(NoteAcceptorFaultTypes.NoteJammed, GeneralExceptionCode.BillJam, DisplayName = "Note Jammed Reports a Bill Jam Exception")]
        [DataRow(NoteAcceptorFaultTypes.NvmFault, GeneralExceptionCode.BillAcceptorHardwareFailure, DisplayName = "Nvm Fault Reports a Bill Acceptor Hardware Failure Exception")]
        [DataRow(NoteAcceptorFaultTypes.OpticalFault, GeneralExceptionCode.BillAcceptorHardwareFailure, DisplayName = "Optical Fault Reports a Bill Acceptor Hardware Failure Exception")]
        [DataRow(NoteAcceptorFaultTypes.OtherFault, GeneralExceptionCode.BillAcceptorHardwareFailure, DisplayName = "Other Fault Reports a Bill Acceptor Hardware Failure Exception")]
        [DataRow(NoteAcceptorFaultTypes.StackerDisconnected, GeneralExceptionCode.CashBoxWasRemoved, DisplayName = "Stacker Disconnected Reports a Cash box removed Exception")]
        [DataRow(NoteAcceptorFaultTypes.StackerFault, GeneralExceptionCode.BillAcceptorHardwareFailure, DisplayName = "Stacker Fault Reports a Bill Acceptor Hardware Failure Exception")]
        [DataRow(NoteAcceptorFaultTypes.StackerFull, GeneralExceptionCode.CashBoxFullDetected, DisplayName = "Stacker Full Reports a Cash box Full Exception")]
        [DataRow(NoteAcceptorFaultTypes.StackerJammed, GeneralExceptionCode.BillJam, DisplayName = "Stacker Jammed Reports a Bill Jam Exception")]
        [DataRow(NoteAcceptorFaultTypes.None, null, DisplayName = "Fault Type none does not report an exception")]
        [DataTestMethod]
        public void ConsumeTest(NoteAcceptorFaultTypes fault, GeneralExceptionCode? expectedException)
        {
            _target.Consume(new HardwareFaultEvent(fault));

            if (expectedException.HasValue)
            {
                _exceptionHandler.Verify(
                    x => x.ReportException(
                        It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == expectedException.Value)),
                    Times.Once);
            }
            else
            {
                _exceptionHandler.Verify(x => x.ReportException(It.IsAny<ISasExceptionCollection>()), Times.Never);
            }
        }
    }
}