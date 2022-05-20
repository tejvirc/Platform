namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor;
    using Bingo.Consumers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class NoteAcceptorHardwareFaultConsumerTests
    {
        private NoteAcceptorHardwareFaultConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IReportEventQueueService> _reportingService = new(MockBehavior.Strict);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new NoteAcceptorHardwareFaultConsumer(_eventBus.Object, _consumerContext.Object, _reportingService.Object);
        }

        [DataRow(true, false, DisplayName = "Reporting Service Null")]
        [DataRow(false, true, DisplayName = "EventBus Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool reportingNull, bool eventBusNull)
        {
            _target = new NoteAcceptorHardwareFaultConsumer(
                eventBusNull ? null : _eventBus.Object,
                _consumerContext.Object,
                reportingNull ? null : _reportingService.Object);
        }

        [DataRow(NoteAcceptorFaultTypes.FirmwareFault, ReportableEvent.BillAcceptorSoftwareError, DisplayName = "Firmware Fault")]
        [DataRow(NoteAcceptorFaultTypes.OpticalFault, ReportableEvent.BillAcceptorHardwareFailure, DisplayName = "Optical Fault")]
        [DataRow(NoteAcceptorFaultTypes.ComponentFault, ReportableEvent.BillAcceptorHardwareFailure, DisplayName = "Component Fault")]
        [DataRow(NoteAcceptorFaultTypes.NvmFault, ReportableEvent.BillAcceptorHardwareFailure, DisplayName = "NvRam Fault")]
        [DataRow(NoteAcceptorFaultTypes.StackerFull, ReportableEvent.BillAcceptorStackerIsFull, DisplayName = "Stacker Full Fault")]
        [DataRow(NoteAcceptorFaultTypes.StackerFault, ReportableEvent.BillAcceptorHardwareFailure, DisplayName = "Stacker Fault")]
        [DataRow(NoteAcceptorFaultTypes.CheatDetected, ReportableEvent.BillAcceptorCheatDetected, DisplayName = "Cheat Detected Fault")]
        [DataRow(NoteAcceptorFaultTypes.OtherFault, ReportableEvent.BillAcceptorError, DisplayName = "Other Fault")]
        [DataRow(NoteAcceptorFaultTypes.MechanicalFault, ReportableEvent.BillAcceptorError, DisplayName = "Mechanical Fault")]
        [DataRow(NoteAcceptorFaultTypes.StackerJammed, ReportableEvent.BillAcceptorStackerJammed, DisplayName = "Stacker Jammed Fault")]
        [DataRow(NoteAcceptorFaultTypes.NoteJammed, ReportableEvent.BillAcceptorStackerJammed, DisplayName = "Note Jammed Fault")]
        [DataTestMethod]
        public void ConsumesTest(NoteAcceptorFaultTypes fault, ReportableEvent response)
        {
            HardwareFaultEvent _event = new(fault);
            _reportingService.Setup(m => m.AddNewEventToQueue(response)).Verifiable();

            _target.Consume(_event);

            _reportingService.Verify(m => m.AddNewEventToQueue(response), Times.Once());
        }

        [TestMethod]
        public void ConsumesNoneTest()
        {
            _reportingService.Setup(m => m.AddNewEventToQueue(It.IsAny<ReportableEvent>())).Verifiable();

            _target.Consume(new(NoteAcceptorFaultTypes.None));

            _reportingService.Verify(m => m.AddNewEventToQueue(It.IsAny<ReportableEvent>()), Times.Never());
        }

        [TestMethod]
        public void ConsumesStackerDisconnectedTest()
        {
            _reportingService.Setup(m => m.AddNewEventToQueue(ReportableEvent.StackerRemoved)).Verifiable();
            _reportingService.Setup(m => m.AddNewEventToQueue(ReportableEvent.CashDrop)).Verifiable();

            _target.Consume(new(NoteAcceptorFaultTypes.StackerDisconnected));

            _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.StackerRemoved), Times.Once());
            _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.CashDrop), Times.Once());
        }

    }
}