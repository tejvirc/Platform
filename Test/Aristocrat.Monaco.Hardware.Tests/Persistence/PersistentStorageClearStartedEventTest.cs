namespace Aristocrat.Monaco.Hardware.Tests.Persistence
{
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Contracts.Persistence;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     Tests for PersistentStorageClearStartedEvent
    /// </summary>
    [TestClass]
    public class PersistentStorageClearStartedEventTest
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
        public void SerializeClearStartedEventTest()
        {
            var clearedEvent = new PersistentStorageClearStartedEvent(PersistenceLevel.Static);

            var memoryStream = new MemoryStream();

            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(memoryStream, clearedEvent);

            memoryStream.Seek(0, SeekOrigin.Begin);

            var deserializedEvent =
                (PersistentStorageClearStartedEvent)binaryFormatter.Deserialize(memoryStream);

            Assert.AreEqual(PersistenceLevel.Static, deserializedEvent.Level);
        }
    }
}
