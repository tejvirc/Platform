namespace Aristocrat.Monaco.Kernel.Tests
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Soap;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This is a test class for ServiceNotFoundExceptionTest and is intended
    ///     to contain all ServiceNotFoundExceptionTest Unit Tests
    /// </summary>
    [TestClass]
    public class ServiceNotFoundExceptionTest
    {
        /// <summary>
        ///     A test for ServiceNotFoundException Constructor
        /// </summary>
        [TestMethod]
        public void ConstructorWithMessageTest()
        {
            var message = "This is a test exception message.";
            var target = new ServiceNotFoundException(message);

            Assert.AreEqual(message, target.Message);
            Assert.IsNull(target.InnerException);
        }

        /// <summary>
        ///     A test for ServiceNotFoundException Constructor
        /// </summary>
        [TestMethod]
        public void ConstructorTest()
        {
            var target = new ServiceNotFoundException();

            var expectedMessage = "Exception of type '" + target.GetType() + "' was thrown.";
            Assert.AreEqual(expectedMessage, target.Message);
            Assert.IsNull(target.InnerException);
        }

        /// <summary>
        ///     A test for ServiceNotFoundException Constructor
        /// </summary>
        [TestMethod]
        public void ConstructorTestWithSerialization()
        {
            var message = "This is a test exception message.";
            var exception = new ServiceNotFoundException(message);

            var stream = new FileStream("ServiceNotFoundException.dat", FileMode.Create);

            var formatter = new SoapFormatter(
                null,
                new StreamingContext(StreamingContextStates.File));

            formatter.Serialize(stream, exception);

            stream.Position = 0;
            var deserializedException = (ServiceNotFoundException)formatter.Deserialize(stream);

            Assert.AreEqual(message, deserializedException.Message);
        }

        /// <summary>
        ///     A test for ServiceNotFoundException Constructor
        /// </summary>
        [TestMethod]
        public void ConstructorTestWithMessageAndInnerException()
        {
            var message = "This is a test exception message.";
            var innerException = new FieldAccessException(message);

            var target = new ServiceNotFoundException(message, innerException);

            Assert.AreEqual(message, target.Message);
            Assert.AreEqual(innerException, target.InnerException);
            Assert.AreEqual(message, target.InnerException.Message);
        }

        /// <summary>
        ///     A test for ServiceNotFoundException Constructor with a type parameter.
        /// </summary>
        [TestMethod]
        public void ConstructorTestWithType()
        {
            var type = typeof(IService);
            var target = new ServiceNotFoundException(type);

            var expected = string.Format(CultureInfo.InvariantCulture, "Service of type {0} was not found.", type);

            try
            {
                throw target;
            }
            catch (ServiceNotFoundException ex)
            {
                Assert.AreEqual(expected, ex.Message);
                Assert.AreEqual(type, ex.ServiceType);
            }
        }

        /// <summary>
        ///     A test for ServiceNotFoundException Constructor with a type parameter.
        /// </summary>
        [TestMethod]
        public void ConstructorTestWithTypeAndInnerException()
        {
            var type = typeof(IService);
            Exception inner = new ServiceException();
            var target = new ServiceNotFoundException(type, inner);

            Assert.IsNotNull(target.InnerException);

            var expected = string.Format(CultureInfo.InvariantCulture, "Service of type {0} was not found.", type);

            try
            {
                throw target;
            }
            catch (ServiceNotFoundException ex)
            {
                Assert.AreEqual(expected, ex.Message);
                Assert.AreEqual(type, ex.ServiceType);
                Assert.IsNotNull(target.InnerException);
            }
        }

        /// <summary>
        ///     Adds a service type for the exception.
        /// </summary>
        [TestMethod]
        public void AddServiceTypeTest()
        {
            var target = new ServiceNotFoundException();
            var expected = typeof(IService);

            target.ServiceType = expected;

            var expectedMessage = "Exception of type '" + target.GetType() + "' was thrown.";
            Assert.AreEqual(expectedMessage, target.Message);
            Assert.IsNull(target.InnerException);

            try
            {
                throw target;
            }
            catch (ServiceNotFoundException ex)
            {
                Assert.AreEqual(expected, ex.ServiceType);
            }
        }
    }
}