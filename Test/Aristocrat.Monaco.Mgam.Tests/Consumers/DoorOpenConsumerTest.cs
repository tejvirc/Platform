namespace Aristocrat.Monaco.Mgam.Tests.Consumers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts.Identification;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Mgam.Services.Notification;
    using Hardware.Contracts.Door;
    using Mgam.Consumers;
    using Mgam.Services.DropMode;
    using Mgam.Services.Lockup;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DoorOpenConsumerTest
    {
        private Mock<IDoorService> _doors;
        private Mock<IEmployeeLogin> _employeeLogin;
        private Mock<ILockup> _lockup;
        private Mock<INotificationLift> _notificationLift;
        private Mock<IDropMode> _dropMode;

        private DoorOpenConsumer _target;

        private const string DoorName = "TestDoor";

        [TestInitialize]
        public void MyTestInitialize()
        {
            _doors = new Mock<IDoorService>(MockBehavior.Default);
            _employeeLogin = new Mock<IEmployeeLogin>(MockBehavior.Default);
            _lockup = new Mock<ILockup>(MockBehavior.Default);
            _notificationLift = new Mock<INotificationLift>(MockBehavior.Default);
            _dropMode = new Mock<IDropMode>(MockBehavior.Default);

            var doorList = new Dictionary<int, LogicalDoor>
            {
                {0, new LogicalDoor(0, DoorName, DoorName)}
            };
            _doors.Setup(d => d.LogicalDoors).Returns(doorList);

            _lockup.Setup(l => l.LockupForEmployeeCard(null, SystemDisablePriority.Immediate)).Verifiable();
        }

        [DataRow(false, true, true, true, true, DisplayName = "Null Door Service Object")]
        [DataRow(true, false, true, true, true, DisplayName = "Null Employee Login Object")]
        [DataRow(true, true, false, true, true, DisplayName = "Null Lockup Object")]
        [DataRow(true, true, true, false, true, DisplayName = "Null Notification Lift Object")]
        [DataRow(true, true, true, true, false, DisplayName = "Null Drop Mode Object")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullParameterTest(
            bool doors,
            bool employeeLogin,
            bool lockup,
            bool notificationLift,
            bool dropMode)
        {
            _target = new DoorOpenConsumer(
                doors ? _doors.Object : null,
                employeeLogin ? _employeeLogin.Object : null,
                lockup ? _lockup.Object : null,
                notificationLift ? _notificationLift.Object : null,
                dropMode ? _dropMode.Object : null);
        }

        [TestMethod]
        public void SuccessfulConstructorTest()
        {
            CreateNewTarget();
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void TestConsumerNotifiesInDropMode()
        {
            CreateNewTarget();

            _dropMode.SetupGet(d => d.Active).Returns(true);
            _notificationLift.Setup(n => n.Notify(NotificationCode.DoorOpened, DoorName)).Returns(Task.CompletedTask).Verifiable();

            _target.Consume(new OpenEvent(0, DoorName), CancellationToken.None).Wait(10);

            _notificationLift.Verify();
        }

        [TestMethod]
        public void TestConsumerNotifiesWhenLocked()
        {
            CreateNewTarget();

            _dropMode.SetupGet(d => d.Active).Returns(false);
            _employeeLogin.SetupGet(e => e.IsLoggedIn).Returns(false);
            _notificationLift.Setup(n => n.Notify(NotificationCode.LockedDoorOpen, DoorName)).Returns(Task.CompletedTask).Verifiable();

            _target.Consume(new OpenEvent(0, DoorName), CancellationToken.None).Wait(10);

            _notificationLift.Verify();
        }

        [TestMethod]
        public void TestConsumerNotifiesWhenLoggedIn()
        {
            CreateNewTarget();

            _dropMode.SetupGet(d => d.Active).Returns(false);
            _employeeLogin.SetupGet(e => e.IsLoggedIn).Returns(true);
            _notificationLift.Setup(n => n.Notify(NotificationCode.DoorOpened, DoorName)).Returns(Task.CompletedTask).Verifiable();

            _target.Consume(new OpenEvent(0, DoorName), CancellationToken.None).Wait(10);

            _notificationLift.Verify();
        }

        private void CreateNewTarget()
        {
            _target = new DoorOpenConsumer(
                _doors.Object,
                _employeeLogin.Object,
                _lockup.Object,
                _notificationLift.Object,
                _dropMode.Object);
        }
    }
}
