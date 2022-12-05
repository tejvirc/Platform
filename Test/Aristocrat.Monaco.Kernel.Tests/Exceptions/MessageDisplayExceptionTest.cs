namespace Aristocrat.Monaco.Kernel.Tests.Exceptions
{
    #region Using

    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Soap;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Summary description for MessageDisplayExceptionTest
    /// </summary>
    [TestClass]
    public class MessageDisplayExceptionTest
    {
        /// <summary>
        ///     A test for ServiceNotFoundException Constructor
        /// </summary>
        [TestMethod]
        public void ConstructorWithMessageTest()
        {
            var message = "This is a test exception message.";
            var target = new MessageDisplayException(message);

            Assert.AreEqual(message, target.Message);
            Assert.IsNull(target.InnerException);
        }

        /// <summary>
        ///     A test for MessageDisplayException Constructor
        /// </summary>
        [TestMethod]
        public void ConstructorTest()
        {
            var target = new MessageDisplayException();

            var expectedMessage = "Exception of type '" + target.GetType() + "' was thrown.";
            Assert.AreEqual(expectedMessage, target.Message);
            Assert.IsNull(target.InnerException);
        }

        /// <summary>
        ///     A test for MessageDisplayException Constructor
        /// </summary>
        [TestMethod]
        public void ConstructorTestWithSerialization()
        {
            var message = "This is a test exception message.";
            var exception = new MessageDisplayException(message);

            var stream = new FileStream("ServiceNotFoundException.dat", FileMode.Create);

            var formatter = new SoapFormatter(
                null,
                new StreamingContext(StreamingContextStates.File));

            formatter.Serialize(stream, exception);

            stream.Position = 0;
            var deserializedException = (MessageDisplayException)formatter.Deserialize(stream);

            Assert.AreEqual(message, deserializedException.Message);
        }

        /// <summary>
        ///     A test for MessageDisplayException Constructor
        /// </summary>
        [TestMethod]
        public void ConstructorTestWithMessageAndInnerException()
        {
            var message = "This is a test exception message.";
            var innerException = new FieldAccessException(message);

            var target = new MessageDisplayException(message, innerException);

            Assert.AreEqual(message, target.Message);
            Assert.AreEqual(innerException, target.InnerException);
            Assert.AreEqual(message, target.InnerException.Message);
        }
    }
}