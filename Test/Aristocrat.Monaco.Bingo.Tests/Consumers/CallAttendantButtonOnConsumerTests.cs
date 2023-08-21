namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Bingo.Consumers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CallAttendantButtonOnConsumerTests
    {
        private CallAttendantButtonOnConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new (MockBehavior.Loose);
        private readonly Mock<IReportEventQueueService> _reportingService = new(MockBehavior.Strict);
        private readonly Mock<ISharedConsumer> _sharedConsumer = new(MockBehavior.Default);

        private readonly CallAttendantButtonOnEvent _event = new CallAttendantButtonOnEvent();

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new CallAttendantButtonOnConsumer(_eventBus.Object, _sharedConsumer.Object, _reportingService.Object);
        }

        [DataRow(true, false, DisplayName = "Reporting Service Null")]
        [DataRow(false, true, DisplayName = "EventBus Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool reportingNull, bool eventBusNull)
        {
            _target = new CallAttendantButtonOnConsumer(
                eventBusNull ? null : _eventBus.Object,
                _sharedConsumer.Object,
                reportingNull ? null : _reportingService.Object);

        }

        [TestMethod]
        public void ConsumesTest()
        {
            _reportingService.Setup(m => m.AddNewEventToQueue(ReportableEvent.CallAttendantButtonActivated)).Verifiable();

            _target.Consume(_event);

            _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.CallAttendantButtonActivated), Times.Once());
        }
    }
}