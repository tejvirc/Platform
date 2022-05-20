namespace Aristocrat.Monaco.Hardware.Tests.ContractsTests
{
    using System;
    using Contracts.Door;
    using Contracts.SharedDevice;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This file contains the tests for DoorContracts
    /// </summary>
    [TestClass]
    public class DoorContractsTests
    {
        [TestMethod]
        public void DisabledEventTest()
        {
            // constructor with DisabledReasons test
            var disabledEvent = new DisabledEvent(DisabledReasons.Backend);
            Assert.IsNotNull(disabledEvent);
            Assert.AreEqual(DisabledReasons.Backend, disabledEvent.Reasons);
        }

        [TestMethod]
        public void EnabledEventTest()
        {
            // constructor with EnabledReasons test
            var enabledEvent = new EnabledEvent(EnabledReasons.Backend);
            Assert.IsNotNull(enabledEvent);
            Assert.AreEqual(EnabledReasons.Backend, enabledEvent.Reasons);
        }

        [TestMethod]
        public void LogicalDoorConstructorTest1()
        {
            // constructor with no params test
            var logicalDoor = new LogicalDoor();
            Assert.IsNotNull(logicalDoor);
            Assert.AreEqual(0, logicalDoor.PhysicalId);
            Assert.AreEqual(DoorState.Uninitialized, logicalDoor.State);
            Assert.IsTrue(logicalDoor.Closed);
            Assert.AreEqual(string.Empty, logicalDoor.Name);
            Assert.AreEqual(string.Empty, logicalDoor.LocalizedName);
        }

        [TestMethod]
        public void LogicalDoorConstructorTest2()
        {
            // constructor with params test
            var id = 10;
            DoorState state = DoorState.Disabled;
            var closed = true;
            string name = "Test Door";
            string localizedName = "??? Test Door???";
            DateTime lastOpened = DateTime.MinValue;

            var logicalDoor = new LogicalDoor(id, name, localizedName)
            {
                State = DoorState.Disabled,
                Closed = closed,
                LastOpenedDateTime = lastOpened
            };

            Assert.IsNotNull(logicalDoor);
            Assert.AreEqual(id, logicalDoor.PhysicalId);
            Assert.AreEqual(state, logicalDoor.State);
            Assert.AreEqual(closed, logicalDoor.Closed);
            Assert.AreEqual(name, logicalDoor.Name);
            Assert.AreEqual(localizedName, logicalDoor.LocalizedName);
            Assert.AreEqual(lastOpened, logicalDoor.LastOpenedDateTime);
        }

        [TestMethod]
        public void ClosedEventTest()
        {
            // constructor with no params test
            var closedEvent = new ClosedEvent();
            Assert.IsNotNull(closedEvent);

            // constructor with logicalId test
            int logicalId = 10;
            closedEvent = new ClosedEvent(logicalId, string.Empty);
            Assert.AreEqual(logicalId, closedEvent.LogicalId);
            Assert.IsFalse(closedEvent.WhilePoweredDown);

            // constructor with logicalId and WhilePoweredDown test
            closedEvent = new ClosedEvent(logicalId, true, string.Empty);
            Assert.IsNotNull(closedEvent);
            Assert.AreEqual(logicalId, closedEvent.LogicalId);
            Assert.IsTrue(closedEvent.WhilePoweredDown);
        }

        [TestMethod]
        public void OpenEventTest()
        {
            // constructor with no params test
            var openEvent = new OpenEvent();
            Assert.IsNotNull(openEvent);

            // constructor with logicalId test
            int logicalId = 10;
            openEvent = new OpenEvent(logicalId, string.Empty);
            Assert.AreEqual(logicalId, openEvent.LogicalId);
            Assert.IsFalse(openEvent.WhilePoweredDown);

            // constructor with logicalId and WhilePoweredDown test
            openEvent = new OpenEvent(logicalId, true, string.Empty);
            Assert.IsNotNull(openEvent);
            Assert.AreEqual(logicalId, openEvent.LogicalId);
            Assert.IsTrue(openEvent.WhilePoweredDown);
        }
    }
}