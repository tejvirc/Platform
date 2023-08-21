namespace Aristocrat.Monaco.Accounting.Tests
{
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Soap;
    using Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This is a test class for TransactionStartedEvent and is intended
    ///     to contain all TransactionStartedEventTest Unit Tests
    /// </summary>
    [TestClass]
    public class TransactionStartedEventTest
    {
        /// <summary>
        ///     A test for TransactionType
        /// </summary>
        [TestMethod]
        public void TransactionType()
        {
            var target = new TransactionStartedEvent();
            var expected = Contracts.TransactionType.Read;
            target.TransactionType = Contracts.TransactionType.Read;
            var actual = target.TransactionType;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///     A test for the parameterless constructor
        /// </summary>
        [TestMethod]
        public void ParameterlessConstructor()
        {
            var target = new TransactionStartedEvent();
            Assert.AreEqual(Contracts.TransactionType.Write, target.TransactionType);
        }

        /// <summary>
        ///     A test for the one-argument constructor
        /// </summary>
        [TestMethod]
        public void ConstructorWithType()
        {
            var target = new TransactionStartedEvent(Contracts.TransactionType.Read);
            Assert.AreEqual(Contracts.TransactionType.Read, target.TransactionType);
        }

        /// <summary>
        ///     A test for serialization
        /// </summary>
        [TestMethod]
        public void SerializeTest()
        {
            var expectedTransactionType = Contracts.TransactionType.Read;
            var originalEvent = new TransactionStartedEvent(expectedTransactionType);
            var expectedGuid = originalEvent.GloballyUniqueId;

            var stream = new FileStream("TransactionStartedEvent.dat", FileMode.Create);
            var formatter = new SoapFormatter(
                null,
                new StreamingContext(StreamingContextStates.File));

            formatter.Serialize(stream, originalEvent);

            stream.Position = 0;

            var target = (TransactionStartedEvent)formatter.Deserialize(stream);

            Assert.AreEqual(expectedGuid, target.GloballyUniqueId);
            Assert.AreEqual(expectedTransactionType, target.TransactionType);
        }
    }
}