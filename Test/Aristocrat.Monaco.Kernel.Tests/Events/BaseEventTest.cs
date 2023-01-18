namespace Aristocrat.Monaco.Kernel.Tests
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Soap;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;

    /// <summary>
    ///     This is a test class for BaseEventTest and is intended
    ///     to contain all BaseEventTest Unit Tests
    /// </summary>
    [TestClass]
    public class BaseEventTest
    {
        /// <summary>
        ///     The test context
        /// </summary>
        private TestContext testContextInstance;

        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }

            set { testContextInstance = value; }
        }

        /// <summary>
        ///     A constructor test for BaseEvent
        /// </summary>
        [TestMethod]
        public void BaseEventConstructorTest()
        {
            var beforeConstruction = DateTime.UtcNow;
            var testEvent = new TestEvent();
            var afterConstruction = DateTime.UtcNow;

            Assert.IsNotNull(testEvent.GloballyUniqueId);

            Assert.IsTrue(beforeConstruction <= testEvent.Timestamp);
            Assert.IsTrue(afterConstruction >= testEvent.Timestamp);
        }

        /// <summary>
        ///     A constructor test for BaseEvent with serialization
        /// </summary>
        [TestMethod]
        public void SerializationBaseEventConstructorTest()
        {
            var testEvent = new TestEvent();
            var expectedGuid = testEvent.GloballyUniqueId;
            var expectedTimestamp = testEvent.Timestamp;

            var stream = new FileStream("baseevent.dat", FileMode.Create);
            var formatter = new SoapFormatter(
                null,
                new StreamingContext(StreamingContextStates.File));

            formatter.Serialize(stream, testEvent);

            stream.Position = 0;

            var deserializedEvent = (TestEvent)formatter.Deserialize(stream);

            Assert.AreEqual(expectedGuid, deserializedEvent.GloballyUniqueId);
            Assert.AreEqual(expectedTimestamp, deserializedEvent.Timestamp.ToUniversalTime());
        }

        /// <summary>
        ///     A test for Timestamp
        /// </summary>
        [TestMethod]
        public void TimestampTest()
        {
            var beforeConstruction = DateTime.UtcNow;
            var testEvent = new TestEvent();
            var afterConstruction = DateTime.UtcNow;

            Assert.IsTrue(beforeConstruction <= testEvent.Timestamp);
            Assert.IsTrue(afterConstruction >= testEvent.Timestamp);
        }
    }
}