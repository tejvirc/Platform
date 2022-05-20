namespace Aristocrat.Monaco.Kernel.Tests
{
    #region Using

    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Soap;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Summary description for UnknownPropertyExceptionTest
    /// </summary>
    [TestClass]
    public class UnknownPropertyExceptionTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void UnknownPropertyExceptionConstructor1Test()
        {
            var target = new UnknownPropertyException();
            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void UnknownPropertyExceptionConstructor2Test()
        {
            var expectedMessage = "test";
            var target = new UnknownPropertyException(expectedMessage);
            Assert.IsNotNull(target);

            Assert.AreEqual(expectedMessage, target.Message);
        }

        [TestMethod]
        public void UnknownPropertyExceptionConstructor3Test()
        {
            var expectedMessage = "test";
            var inner = new Exception();
            var target = new UnknownPropertyException(expectedMessage, inner);
            Assert.IsNotNull(target);

            Assert.AreEqual(expectedMessage, target.Message);
            Assert.AreEqual(inner, target.InnerException);
        }

        [TestMethod]
        public void UnknownPropertyExceptionSerializingConstructorTest()
        {
            var message = "This is a test exception message.";
            var exception = new UnknownPropertyException(message);

            var stream = new FileStream("UnknownPropertyException.dat", FileMode.Create);

            var formatter = new SoapFormatter(
                null,
                new StreamingContext(StreamingContextStates.File));

            formatter.Serialize(stream, exception);

            stream.Position = 0;
            var deserializedException = (UnknownPropertyException)formatter.Deserialize(stream);

            Assert.AreEqual(message, deserializedException.Message);
        }
    }
}