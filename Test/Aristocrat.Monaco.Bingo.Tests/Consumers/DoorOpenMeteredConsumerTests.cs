namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Aristocrat.Monaco.Hardware.Contracts.Door;
    using Bingo.Consumers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DoorOpenMeteredConsumerTests
    {
        private DoorOpenMeteredConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new (MockBehavior.Loose);
        private readonly Mock<IReportEventQueueService> _reportingService = new(MockBehavior.Strict);
        private readonly Mock<ISharedConsumer> _sharedConsumer = new(MockBehavior.Default);

        private readonly DoorOpenMeteredEvent _event = new DoorOpenMeteredEvent((int)DoorLogicalId.Main, false, false, "Main Door");

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new DoorOpenMeteredConsumer(_eventBus.Object, _sharedConsumer.Object, _reportingService.Object);
        }

        [DataRow(true, false, DisplayName = "Reporting Service Null")]
        [DataRow(false, true, DisplayName = "EventBus Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool reportingNull, bool eventBusNull)
        {
            _target = new DoorOpenMeteredConsumer(
                eventBusNull ? null : _eventBus.Object,
                _sharedConsumer.Object,
                reportingNull ? null : _reportingService.Object);

        }

        [TestMethod]
        public void ConsumesTest()
        {
            _reportingService.Setup(m => m.AddNewEventToQueue(ReportableEvent.MainDoorOpened)).Verifiable();

            _target.Consume(_event);

            _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.MainDoorOpened), Times.Once());
        }
    }
}