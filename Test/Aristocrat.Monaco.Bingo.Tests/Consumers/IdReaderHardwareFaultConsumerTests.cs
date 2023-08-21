namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Aristocrat.Monaco.Hardware.Contracts.IdReader;
    using Bingo.Consumers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class IdReaderHardwareFaultConsumerTests
    {
        private IdReaderHardwareFaultConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IReportEventQueueService> _reportingService = new(MockBehavior.Strict);
        private readonly HardwareFaultEvent _event = new(IdReaderFaultTypes.OtherFault);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new IdReaderHardwareFaultConsumer(_eventBus.Object, _consumerContext.Object, _reportingService.Object);
        }

        [DataRow(true, false, DisplayName = "Reporting Service Null")]
        [DataRow(false, true, DisplayName = "EventBus Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool reportingNull, bool eventBusNull)
        {
            _target = new IdReaderHardwareFaultConsumer(
                eventBusNull ? null : _eventBus.Object,
                _consumerContext.Object,
                reportingNull ? null : _reportingService.Object);
        }

        [DataRow(IdReaderFaultTypes.ComponentFault, ReportableEvent.CardReaderHardwareError, DisplayName = "Component Fault")]
        [DataRow(IdReaderFaultTypes.FirmwareFault, ReportableEvent.CardReaderSoftwareError, DisplayName = "Firmware fault")]
        [DataRow(IdReaderFaultTypes.OtherFault, ReportableEvent.CardReaderError, DisplayName = "Other fault")]
        [DataTestMethod]
        public void ConsumesTest(IdReaderFaultTypes fault, ReportableEvent response)
        {
            var @event = new HardwareFaultEvent(fault);

            _reportingService.Setup(m => m.AddNewEventToQueue(response)).Verifiable();

            _target.Consume(@event);

            _reportingService.Verify(m => m.AddNewEventToQueue(response), Times.Once());
        }
    }
}