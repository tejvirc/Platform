namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Bingo.Consumers;
    using Bingo.Services.Reporting;
    using Common;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class NoteAcceptorHardwareFaultClearConsumerTests
    {
        private NoteAcceptorHardwareFaultClearConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IReportEventQueueService> _reportingService = new(MockBehavior.Default);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new NoteAcceptorHardwareFaultClearConsumer(_eventBus.Object, _consumerContext.Object, _reportingService.Object);
        }

        [DataRow(true, false, DisplayName = "Reporting Service Null")]
        [DataRow(false, true, DisplayName = "EventBus Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool reportingNull, bool eventBusNull)
        {
            _target = new NoteAcceptorHardwareFaultClearConsumer(
                eventBusNull ? null : _eventBus.Object,
                _consumerContext.Object,
                reportingNull ? null : _reportingService.Object);
        }

        [DataRow(NoteAcceptorFaultTypes.StackerDisconnected, ReportableEvent.StackerInserted, DisplayName = "Stacker Inserted")]
        [DataRow(NoteAcceptorFaultTypes.StackerFull, ReportableEvent.BillAcceptorStackerFullClear, DisplayName = "Stacker Full Clear")]
        [DataRow(NoteAcceptorFaultTypes.FirmwareFault, ReportableEvent.BillAcceptorErrorClear, DisplayName = "Error Clear")]
        [DataTestMethod]
        public void ConsumesTest(NoteAcceptorFaultTypes fault, ReportableEvent response)
        {
            var @event = new HardwareFaultClearEvent(fault);
            _target.Consume(@event);

            if (response is not ReportableEvent.BillAcceptorErrorClear)
            {
                _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.BillAcceptorErrorClear), Times.Once());
            }

            _reportingService.Verify(m => m.AddNewEventToQueue(response), Times.Once());
        }
    }
}