namespace Aristocrat.Monaco.Hardware.Contracts.Tests
{
    #region Using

    using System;
    using Door;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SharedDevice;

    #endregion

    /// <summary>
    ///     This file contains the unit tests for door related events
    /// </summary>
    [TestClass]
    public class DoorEventsTest
    {
        [TestMethod]
        public void DoorOpenMeteredEventConstructor1Test()
        {
            var target = new DoorOpenMeteredEvent();

            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void DoorOpenMeteredEventConstructor2Test()
        {
            int doorId = 1234;
            var target = new DoorOpenMeteredEvent(doorId, false, false, string.Empty);

            Assert.IsNotNull(target);
            Assert.AreEqual(doorId, target.LogicalId);
        }

        [TestMethod]
        public void LogicalDoorConstructor1Test()
        {
            var target = new LogicalDoor();

            Assert.IsNotNull(target);
            Assert.AreEqual(0, target.PhysicalId);
            Assert.AreEqual(DoorState.Uninitialized, target.State);
            Assert.IsTrue(target.Closed);
            Assert.AreEqual(string.Empty, target.Name);
            Assert.AreEqual(string.Empty, target.LocalizedName);
        }

        [TestMethod]
        public void LogicalDoorConstructor2Test()
        {
            int id = 123;
            DoorState state = DoorState.Disabled;
            string name = "Test door";
            string localizedName = "door test";
            var target = new LogicalDoor(id, name, localizedName)
            {
                State = state,
                Closed = true
            };

            Assert.IsNotNull(target);
            Assert.AreEqual(id, target.PhysicalId);
            Assert.AreEqual(state, target.State);
            Assert.IsTrue(target.Closed);
            Assert.AreEqual(name, target.Name);
            Assert.AreEqual(localizedName, target.LocalizedName);
        }

        [TestMethod]
        public void LogicalDoorLastOpenedDateTimeTest()
        {
            var target = new LogicalDoor();

            DateTime time = DateTime.MaxValue;
            target.LastOpenedDateTime = time;
            Assert.AreEqual(time, target.LastOpenedDateTime);
        }

        [TestMethod]
        public void OpenEventConstructorTest()
        {
            int id = 123;
            var target = new OpenEvent(id, string.Empty);

            Assert.IsNotNull(target);
            Assert.AreEqual(id, target.LogicalId);
            Assert.IsFalse(target.WhilePoweredDown);
        }

        [TestMethod]
        public void EnabledEventConstructorTest()
        {
            EnabledReasons reason = EnabledReasons.Configuration;
            var target = new EnabledEvent(reason);

            Assert.IsNotNull(target);
            Assert.AreEqual(reason, target.Reasons);
        }

        [TestMethod]
        public void DisabledEventConstructorTest()
        {
            DisabledReasons reason = DisabledReasons.Configuration;
            var target = new DisabledEvent(reason);

            Assert.IsNotNull(target);
            Assert.AreEqual(reason, target.Reasons);
        }
    }
}