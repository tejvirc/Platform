namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Bingo.Consumers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class HardwareFaultConsumerTests
    {
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IReportEventQueueService> _reportingService = new(MockBehavior.Strict);

        private HardwareFaultConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new HardwareFaultConsumer(_eventBus.Object, _consumerContext.Object, _reportingService.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
        }

        [DataRow(true, false, DisplayName = "Reporting Service Null")]
        [DataRow(false, true, DisplayName = "EventBus Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool reportingNull, bool eventBusNull)
        {
            _target = new HardwareFaultConsumer(
                eventBusNull ? null : _eventBus.Object,
                _consumerContext.Object,
                reportingNull ? null : _reportingService.Object);
        }

        [DataRow(ReelControllerFaults.CommunicationError, ReportableEvent.ReelHome, DisplayName = "Communication Error")]
        [DataRow(ReelControllerFaults.HardwareError, ReportableEvent.ReelWatchdog, DisplayName = "Hardware Error")]
        [DataTestMethod]
        public void ConsumesTest(ReelControllerFaults reelControllerFaults, ReportableEvent reportEvent)
        {
            var @event = new HardwareFaultEvent(0, reelControllerFaults);
            _reportingService.Setup(m => m.AddNewEventToQueue(reportEvent)).Verifiable();

            _target.Consume(@event);

            _reportingService.Verify(m => m.AddNewEventToQueue(reportEvent), Times.Once());
        }
    }
}