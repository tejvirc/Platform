namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Soap;
    using Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This is a test class for TransactionCompletedEvent and is intended
    ///     to contain all TransactionCompletedEventTest Unit Tests
    /// </summary>
    [TestClass]
    public class TransactionCompletedEventTest
    {
        /// <summary>
        ///     A test for getting the TransactionId property
        /// </summary>
        [TestMethod]
        public void TransactionIdTest()
        {
            var expected = Guid.NewGuid();
            var target = new TransactionCompletedEvent(expected);
            var actual = target.TransactionId;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///     A test for the parameterless constructor
        /// </summary>
        [TestMethod]
        public void ParameterlessConstructorTest()
        {
            var target = new TransactionCompletedEvent();
            Assert.AreEqual(Guid.Empty, target.TransactionId);
        }

        /// <summary>
        ///     A test for the 1-parameter constructor
        /// </summary>
        [TestMethod]
        public void ParameterConstructorTest()
        {
            var expected = Guid.NewGuid();
            var target = new TransactionCompletedEvent(expected);
            Assert.AreEqual(expected, target.TransactionId);
        }

        /// <summary>
        ///     A test for serialization
        /// </summary>
        [TestMethod]
        public void SerializeTest()
        {
            var expected = Guid.NewGuid();
            var originalEvent = new TransactionCompletedEvent(expected);
            var expectedGuid = originalEvent.GloballyUniqueId;

            var stream = new FileStream("TransactionCompletedEvent.dat", FileMode.Create);
            var formatter = new SoapFormatter(
                null,
                new StreamingContext(StreamingContextStates.File));

            formatter.Serialize(stream, originalEvent);

            stream.Position = 0;

            var target = (TransactionCompletedEvent)formatter.Deserialize(stream);

            Assert.AreEqual(expected, target.TransactionId);
            Assert.AreEqual(expectedGuid, target.GloballyUniqueId);
        }
    }
}