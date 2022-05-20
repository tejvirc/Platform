namespace Aristocrat.Monaco.Bingo.Common.Tests
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Soap;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Exceptions;

    /// <summary>
    ///     Tests for the BingoSerialNumberException class
    /// </summary>
    [TestClass]
    public class BingoSerialNumberExceptionTest
    {
        [TestMethod]
        public void ConstructorWithMessageTest()
        {
            var message = "This is a test exception message.";
            var target = new BingoSerialNumberException(message);

            Assert.AreEqual(message, target.Message);
            Assert.IsNull(target.InnerException);
        }

        [TestMethod]
        public void ConstructorTest()
        {
            var target = new BingoSerialNumberException();

            var expectedMessage = "Exception of type '" + target.GetType() + "' was thrown.";
            Assert.AreEqual(expectedMessage, target.Message);
            Assert.IsNull(target.InnerException);
        }

        [TestMethod]
        public void ConstructorTestWithSerialization()
        {
            var message = "This is a test exception message.";
            var exception = new BingoSerialNumberException(message);

            var stream = new FileStream("BingoSerialNumberException.dat", FileMode.Create);

            var formatter = new SoapFormatter(
                null,
                new StreamingContext(StreamingContextStates.File));

            formatter.Serialize(stream, exception);

            stream.Position = 0;
            var deserializedException = (BingoSerialNumberException)formatter.Deserialize(stream);

            Assert.AreEqual(message, deserializedException.Message);
        }

        [TestMethod]
        public void ConstructorTestWithMessageAndInnerException()
        {
            var message = "This is a test exception message.";
            var innerException = new FieldAccessException(message);

            var target = new BingoSerialNumberException(message, innerException);

            Assert.AreEqual(message, target.Message);
            Assert.AreEqual(innerException, target.InnerException);
            Assert.AreEqual(message, target.InnerException.Message);
        }
    }
}