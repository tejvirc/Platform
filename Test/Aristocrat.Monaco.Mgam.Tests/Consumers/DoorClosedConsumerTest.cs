namespace Aristocrat.Monaco.Mgam.Tests.Consumers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Monaco.Mgam.Services.Notification;
    using Hardware.Contracts.Door;
    using Mgam.Consumers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DoorClosedConsumerTest
    {
        private Mock<IDoorService> _doors;
        private Mock<INotificationLift> _notificationLift;

        private DoorClosedConsumer _target;

        private const string DoorName = "TestDoor";

        [TestInitialize]
        public void MyTestInitialize()
        {
            _doors = new Mock<IDoorService>(MockBehavior.Default);
            _notificationLift = new Mock<INotificationLift>(MockBehavior.Default);

            var doorList = new Dictionary<int, LogicalDoor>
            {
                {0, new LogicalDoor(0, DoorName, DoorName)}
            };
            _doors.Setup(d => d.LogicalDoors).Returns(doorList);
        }

        [DataRow(false, true, DisplayName = "Null Door Service Object")]
        [DataRow(true, false, DisplayName = "Null Notification Lift Object")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullParameterTest(
            bool doors,
            bool notificationLift)
        {
            _target = new DoorClosedConsumer(
                doors ? _doors.Object : null,
                notificationLift ? _notificationLift.Object : null);
        }

        [TestMethod]
        public void SuccessfulConstructorTest()
        {
            CreateNewTarget();
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void TestConsumerNotifies()
        {
            CreateNewTarget();

            var consumed = _target.Consume(new ClosedEvent(0, DoorName), CancellationToken.None).Wait(10);

            _notificationLift.Verify(n => n.Notify(NotificationCode.DoorClosed, DoorName), Times.Once());
            Assert.IsTrue(consumed);
        }

        private void CreateNewTarget()
        {
            _target = new DoorClosedConsumer(
                _doors.Object,
                _notificationLift.Object);
        }
    }
}
