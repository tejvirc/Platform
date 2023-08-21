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
        private readonly Mock<IReportEventQueueService> _reportingService = new(MockBehavior.Default);
        private readonly Mock<ISharedConsumer> _sharedConsumer = new(MockBehavior.Default);
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

        [DataTestMethod]
        [DataRow(DoorLogicalId.CashBox, ReportableEvent.StackerDoorClosed)]
        [DataRow(DoorLogicalId.Main, ReportableEvent.MainDoorClosed)]
        [DataRow(DoorLogicalId.TopBox, ReportableEvent.LcdDoorClosed)]
        [DataRow(DoorLogicalId.DropDoor, ReportableEvent.CashDoorClosed)]
        [DataRow(DoorLogicalId.Logic, ReportableEvent.LogicDoorClosed)]
        [DataRow(DoorLogicalId.Belly, ReportableEvent.BellyDoorClosed)]
        public void ConsumesTest(DoorLogicalId doorId, ReportableEvent expectedEvent)
        {
            _target.Consume(new DoorClosedMeteredEvent((int)doorId, "Test Door"));
            _reportingService.Verify(m => m.AddNewEventToQueue(expectedEvent), Times.Once());
        }
    }
}