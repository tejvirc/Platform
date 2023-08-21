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
    public class DoorOpenMeteredConsumerTests
    {
        private DoorOpenMeteredConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new (MockBehavior.Loose);
        private readonly Mock<IReportEventQueueService> _reportingService = new(MockBehavior.Default);
        private readonly Mock<ISharedConsumer> _sharedConsumer = new(MockBehavior.Default);

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

        [DataTestMethod]
        [DataRow(DoorLogicalId.CashBox, ReportableEvent.StackerDoorOpened)]
        [DataRow(DoorLogicalId.Main, ReportableEvent.MainDoorOpened)]
        [DataRow(DoorLogicalId.TopBox, ReportableEvent.LcdDoorOpened)]
        [DataRow(DoorLogicalId.DropDoor, ReportableEvent.CashDoorOpened)]
        [DataRow(DoorLogicalId.Logic, ReportableEvent.LogicDoorOpened)]
        [DataRow(DoorLogicalId.Belly, ReportableEvent.BellyDoorOpened)]
        public void ConsumesTest(DoorLogicalId doorId, ReportableEvent expectedEvent)
        {
            _target.Consume(new DoorOpenMeteredEvent((int)doorId, false, false, "TestDoor"));
            _reportingService.Verify(m => m.AddNewEventToQueue(expectedEvent), Times.Once());
        }
    }
}