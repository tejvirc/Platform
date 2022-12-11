namespace Aristocrat.Monaco.Hardware.Tests.Persistence
{
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using Contracts.Persistence;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ProtoBuf;

    /// <summary>
    ///     Tests for PersistentStorageClearedEvent
    /// </summary>
    [TestClass]
    public class PersistentStorageClearedEventTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        ///     A test for serializing and deserializing the PersistentStorageClearStartedEvent.
        /// </summary>
        [TestMethod]
        public void SerializeClearedEventTest()
        {
            var clearedEvent = new PersistentStorageClearedEvent(PersistenceLevel.Static);

            var memoryStream = new MemoryStream();

            Serializer.Serialize(memoryStream, clearedEvent);

            memoryStream.Seek(0, SeekOrigin.Begin);

            var deserializedEvent =
                Serializer.Deserialize<PersistentStorageClearedEvent>(memoryStream);

            Assert.AreEqual(PersistenceLevel.Static, deserializedEvent.Level);
        }

        [TestMethod]
        public void ConstructorTest()
        {
            var target = new PersistentStorageClearedEvent(PersistenceLevel.Static);

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
                (PersistentStorageClearedEvent)
                typeof(PersistentStorageClearedEvent).GetConstructor(
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
            var target = new PersistentStorageClearedEvent(PersistenceLevel.Static);
            var info = new SerializationInfo(typeof(PersistenceLevel), new FormatterConverter());

            target.GetObjectData(info, new StreamingContext(StreamingContextStates.All));

            var result = info.GetValue("PersistenceLevel", typeof(PersistenceLevel));

            Assert.AreEqual(PersistenceLevel.Static, result);
        }
    }
}
