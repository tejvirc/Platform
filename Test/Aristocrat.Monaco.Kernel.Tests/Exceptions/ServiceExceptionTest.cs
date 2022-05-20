namespace Aristocrat.Monaco.Kernel.Tests
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Soap;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This is a test class for ServiceExceptionTest and is intended
    ///     to contain all ServiceExceptionTest Unit Tests
    /// </summary>
    [TestClass]
    public class ServiceExceptionTest
    {
        /// <summary>
        ///     A test for ServiceException Constructor
        /// </summary>
        [TestMethod]
        public void ConstructorWithMessageTest()
        {
            var message = "This is a test exception message.";
            var target = new ServiceException(message);

            Assert.AreEqual(message, target.Message);
            Assert.IsNull(target.InnerException);
        }

        /// <summary>
        ///     A test for ServiceException Constructor
        /// </summary>
        [TestMethod]
        public void ConstructorTest()
        {
            var target = new ServiceException();

            var expectedMessage = "Exception of type '" + target.GetType() + "' was thrown.";
            Assert.AreEqual(expectedMessage, target.Message);
            Assert.IsNull(target.InnerException);
        }

        /// <summary>
        ///     A test for ServiceException Constructor
        /// </summary>
        [TestMethod]
        public void ConstructorTestWithSerialization()
        {
            var message = "This is a test exception message.";
            var exception = new ServiceException(message);

            var stream = new FileStream("ServiceException.dat", FileMode.Create);

            var formatter = new SoapFormatter(
                null,
                new StreamingContext(StreamingContextStates.File));

            formatter.Serialize(stream, exception);

            stream.Position = 0;
            var deserializedException = (ServiceException)formatter.Deserialize(stream);

            Assert.AreEqual(message, deserializedException.Message);
        }

        /// <summary>
        ///     A test for ServiceException Constructor
        /// </summary>
        [TestMethod]
        public void ConstructorTestWithMessageAndInnerException()
        {
            var message = "This is a test exception message.";
            var innerException = new FieldAccessException(message);

            var target = new ServiceException(message, innerException);

            Assert.AreEqual(message, target.Message);
            Assert.AreEqual(innerException, target.InnerException);
            Assert.AreEqual(message, target.InnerException.Message);
        }
    }
}