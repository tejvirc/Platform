namespace Aristocrat.Monaco.Application.Contracts.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DoorMessageTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            var target = new DoorMessage
            {
                Time = DateTime.MaxValue,
                DoorId = 1,
                IsOpen = false,
                ValidationPassed = true
            };

            Assert.IsNotNull(target);
            Assert.AreEqual(DateTime.MaxValue, target.Time);
            Assert.AreEqual(1, target.DoorId);
            Assert.IsFalse(target.IsOpen);
            Assert.IsTrue(target.ValidationPassed);
        }

        [TestMethod]
        public void EqualityTests()
        {
            var target1 = new DoorMessage();
            var target2 = new DoorMessage
            {
                Time = DateTime.MaxValue,
                DoorId = 1,
                IsOpen = false,
                ValidationPassed = true
            };

            Assert.IsFalse(target1 == target2);
            Assert.IsTrue(target1 != target2);

            // Equals test when not a DoorMessage
            Assert.IsFalse(target1.Equals(string.Empty));
        }

        [TestMethod]
        public void GetHashCodeTest()
        {
            var target1 = new DoorMessage();

            // since we get different hashes based on the machine
            // that runs the test, only run this for code coverage.
            target1.GetHashCode();
        }
    }
}