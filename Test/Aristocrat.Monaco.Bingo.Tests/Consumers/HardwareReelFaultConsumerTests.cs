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
    public class HardwareReelFaultConsumerTests
    {
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IReportEventQueueService> _reportingService = new(MockBehavior.Strict);
        private HardwareReelFaultConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new HardwareReelFaultConsumer(_eventBus.Object, _consumerContext.Object, _reportingService.Object);
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
            _target = new HardwareReelFaultConsumer(
                eventBusNull ? null : _eventBus.Object,
                _consumerContext.Object,
                reportingNull ? null : _reportingService.Object);
        }

        [DataRow(ReelFaults.LowVoltage, 0, ReportableEvent.ReelBrownOut, DisplayName = "Reel Low Voltage Error")]
        [DataRow(ReelFaults.ReelTamper, 0, ReportableEvent.ReelFeedback, DisplayName = "Reel Tamper Error")]
        [DataRow(ReelFaults.ReelStall, 0, ReportableEvent.ReelError14, DisplayName = "Reel Stall unknown reel")]
        [DataRow(ReelFaults.ReelStall, 1, ReportableEvent.ReelError8, DisplayName = "Reel 1 Stall")]
        [DataRow(ReelFaults.ReelStall, 2, ReportableEvent.ReelError9, DisplayName = "Reel 2 Stall")]
        [DataRow(ReelFaults.ReelStall, 3, ReportableEvent.ReelError10, DisplayName = "Reel 3 Stall")]
        [DataRow(ReelFaults.ReelStall, 4, ReportableEvent.ReelError11, DisplayName = "Reel 4 Stall")]
        [DataRow(ReelFaults.ReelStall, 5, ReportableEvent.ReelError12, DisplayName = "Reel 5 Stall")]
        [DataRow(ReelFaults.ReelStall, 6, ReportableEvent.ReelError13, DisplayName = "Reel 6 Stall")]
        [DataTestMethod]
        public void ConsumesTest(ReelFaults reelFaults, int reelId, ReportableEvent reportEvent)
        {
            var @event = new HardwareReelFaultEvent(reelFaults, reelId);
            _reportingService.Setup(m => m.AddNewEventToQueue(reportEvent)).Verifiable();

            _target.Consume(@event);

            _reportingService.Verify(m => m.AddNewEventToQueue(reportEvent), Times.Once());
        }
    }
}