namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Bingo.Consumers;
    using Bingo.Services.Reporting;
    using Common;
    using Hardware.Contracts.Door;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DoorClosedMeteredConsumerTests
    {
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<IReportEventQueueService> _reportingService = new(MockBehavior.Strict);
        private readonly Mock<ISharedConsumer> _sharedConsumer = new(MockBehavior.Strict);
        private readonly DoorClosedMeteredEvent _event = new DoorClosedMeteredEvent((int)DoorLogicalId.Main, "Main Door");
        private DoorClosedMeteredConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new DoorClosedMeteredConsumer(_eventBus.Object, _sharedConsumer.Object, _reportingService.Object);
        }


        [DataRow(true, false, DisplayName = "Reporting Service Null")]
        [DataRow(false, true, DisplayName = "EventBus Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool reportingNull, bool eventBusNull)
        {
            _target = new DoorClosedMeteredConsumer(
                eventBusNull ? null : _eventBus.Object,
                _sharedConsumer.Object,
                reportingNull ? null : _reportingService.Object);
        }

        [TestMethod]
        public void ConsumesTest()
        {
            _reportingService.Setup(m => m.AddNewEventToQueue(ReportableEvent.MainDoorClosed)).Verifiable();

            _target.Consume(_event);

            _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.MainDoorClosed), Times.Once());
        }
    }
}