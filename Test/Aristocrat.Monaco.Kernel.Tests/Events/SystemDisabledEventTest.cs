namespace Aristocrat.Monaco.Kernel.Tests
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Soap;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Test.Common;

    [TestClass]
    public class SystemDisabledEventTest
    {
        [TestMethod]
        public void SystemDisabledEventNoArgConstructorTest()
        {
            var target = new SystemDisabledEvent();

            Assert.IsNotNull(target);
            Assert.AreEqual(SystemDisablePriority.Normal, target.Priority);
        }

        [TestMethod]
        public void SystemDisabledEvent1ArgConstructorTest()
        {
            var expectedPriority = SystemDisablePriority.Immediate;
            var target = new SystemDisabledEvent(expectedPriority);

            Assert.IsNotNull(target);
            Assert.AreEqual(expectedPriority, target.Priority);
        }

        [TestMethod]
        public void SystemDisabledEventSerializeTest()
        {
            var originalEvent = new SystemDisabledEvent(SystemDisablePriority.Immediate);

            var stream = new FileStream("SystemDisabledEvent.dat", FileMode.Create);
            var formatter = new SoapFormatter(
                null,
                new StreamingContext(StreamingContextStates.File));

            formatter.Serialize(stream, originalEvent);

            stream.Position = 0;

            var target = (SystemDisabledEvent)formatter.Deserialize(stream);

            Assert.AreEqual(originalEvent.GloballyUniqueId, target.GloballyUniqueId);
            Assert.AreEqual(originalEvent.Priority, target.Priority);
        }

        [TestMethod]
        public void SystemEnabledEventConstructorTest()
        {
            var target = new SystemEnabledEvent();

            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void SystemEnabledEventSerializeTest()
        {
            var originalEvent = new SystemEnabledEvent();

            var stream = new FileStream("SystemEnabledEvent.dat", FileMode.Create);
            var formatter = new SoapFormatter(
                null,
                new StreamingContext(StreamingContextStates.File));

            formatter.Serialize(stream, originalEvent);

            stream.Position = 0;

            var target = (SystemEnabledEvent)formatter.Deserialize(stream);

            Assert.AreEqual(originalEvent.GloballyUniqueId, target.GloballyUniqueId);
        }

        [TestMethod]
        public void SystemDisableAddedEventConstructorTest()
        {
            var testGuid = default(Guid);
            var testReason = "Test Reason";
            var testSystemIdleStateAffected = true;
            var target = new SystemDisableAddedEvent(SystemDisablePriority.Normal, testGuid, testReason, testSystemIdleStateAffected);
            Assert.IsNotNull(target);
            Assert.AreEqual(SystemDisablePriority.Normal, target.Priority);
            Assert.AreEqual(testGuid, target.DisableId);
            Assert.AreEqual(testReason, target.DisableReasons);
            Assert.AreEqual(testSystemIdleStateAffected, target.SystemIdleStateAffected);
        }

        [TestMethod]
        public void SystemDisableAddedSerializeTest()
        {
            var original = new SystemDisableAddedEvent(SystemDisablePriority.Normal, default(Guid), string.Empty, false);
            using (var stream = new FileStream("SystemDisableAddedEvent.dat", FileMode.Create))
            {
                var formatter = new SoapFormatter(null, new StreamingContext(StreamingContextStates.File));
                formatter.Serialize(stream, original);
                stream.Position = 0;
                var target = (SystemDisableAddedEvent)formatter.Deserialize(stream);
                Assert.AreEqual(original.GloballyUniqueId, target.GloballyUniqueId);
            }
        }

        [TestMethod]
        public void SystemDisableRemovedEventConstructorTest()
        {
            var testGuid = default(Guid);
            var testReason = "Test Reason";
            var testSystemDisabled = true;
            var testSystemIdleStateAffected = false;
            var target = new SystemDisableRemovedEvent(SystemDisablePriority.Normal, testGuid, testReason, testSystemDisabled, testSystemIdleStateAffected);
            Assert.IsNotNull(target);
            Assert.AreEqual(SystemDisablePriority.Normal, target.Priority);
            Assert.AreEqual(testGuid, target.DisableId);
            Assert.AreEqual(testReason, target.DisableReasons);
            Assert.AreEqual(testSystemDisabled, target.SystemDisabled);
            Assert.AreEqual(testSystemIdleStateAffected, target.SystemIdleStateAffected);
        }

        [TestMethod]
        public void SystemDisableRemovedSerializeTest()
        {
            AssertEx.IsAttributeDefined(typeof(SystemDisableRemovedEvent), typeof(SerializableAttribute));
        }
    }
}
