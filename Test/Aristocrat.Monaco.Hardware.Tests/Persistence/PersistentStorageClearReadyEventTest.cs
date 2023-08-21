namespace Aristocrat.Monaco.Hardware.Tests.Persistence
{
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using Contracts.Persistence;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ProtoBuf;

    /// <summary>
    ///     Tests for PersistentStorageClearReadyEvent
    /// </summary>
    [TestClass]
    public class PersistentStorageClearReadyEventTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void SerializePersistentStorageClearReadyEventTest()
        {
            var clearedEvent = new PersistentStorageClearReadyEvent(PersistenceLevel.Static);

            var memoryStream = new MemoryStream();

            Serializer.Serialize(memoryStream, clearedEvent);

            memoryStream.Seek(0, SeekOrigin.Begin);

            var deserializedEvent =
                Serializer.Deserialize<PersistentStorageClearReadyEvent>(memoryStream);

            Assert.AreEqual(PersistenceLevel.Static, deserializedEvent.Level);
        }

        [TestMethod]
        public void ConstructorTest()
        {
            var target = new PersistentStorageClearReadyEvent(PersistenceLevel.Static);

            Assert.IsNotNull(target);
            Assert.AreEqual(PersistenceLevel.Static, target.Level);
        }

        [TestMethod]
        public void ProtectedConstructorTest()
        {
            var expectedLevel = PersistenceLevel.Static;
            var context = new StreamingContext(StreamingContextStates.All);
            var info = new SerializationInfo(typeof(long), new FormatterConverter());
            info.AddValue("PersistenceLevel", expectedLevel);

            var target =
                (PersistentStorageClearReadyEvent)
                typeof(PersistentStorageClearReadyEvent).GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.Instance,
                    null,
                    new[] { typeof(SerializationInfo), typeof(StreamingContext) },
                    null).Invoke(new object[] { info, context });

            Assert.IsNotNull(target);
            Assert.AreEqual(expectedLevel, target.Level);
        }

        [TestMethod]
        public void GetObjectDataTest()
        {
            var target = new PersistentStorageClearReadyEvent(PersistenceLevel.Static);
            var info = new SerializationInfo(typeof(PersistenceLevel), new FormatterConverter());

            target.GetObjectData(info, new StreamingContext(StreamingContextStates.All));

            var result = info.GetValue("PersistenceLevel", typeof(PersistenceLevel));

            Assert.AreEqual(PersistenceLevel.Static, result);
        }
    }
}
