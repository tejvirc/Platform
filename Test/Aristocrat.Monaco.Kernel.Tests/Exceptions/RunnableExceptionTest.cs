namespace Aristocrat.Monaco.Kernel.Tests
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This is a test class for RunnableExceptionTest and is intended
    ///     to contain all RunnableExceptionTest Unit Tests
    /// </summary>
    [TestClass]
    public class RunnableExceptionTest
    {
        /// <summary>
        ///     The test context
        /// </summary>
        private TestContext testContextInstance;

        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }

            set { testContextInstance = value; }
        }

        /// <summary>
        ///     A test for RunnableException Constructor
        /// </summary>
        [TestMethod]
        public void RunnableExceptionConstructorTest()
        {
            var target = new RunnableException();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(RunnableException));
            Assert.IsNull(target.InnerException);
        }

        /// <summary>
        ///     A test for RunnableException Constructor with a message
        /// </summary>
        [TestMethod]
        public void RunnableExceptionConstructorWithMessage()
        {
            var message = "This is a test of RunnableException";
            var target = new RunnableException(message);
            Assert.AreEqual(message, target.Message);
            Assert.IsNull(target.InnerException);
        }

        /// <summary>
        ///     A test for RunnableException Constructor with message and inner exception.
        /// </summary>
        [TestMethod]
        public void RunnableExceptionConstructorTestWithInnerException()
        {
            var message = "This is a test of RunnableException";

            // Just need a specific exception type for the inner exception
            Exception inner = new ArgumentException("this is the inner exception");

            var target = new RunnableException(message, inner);

            Assert.AreEqual(message, target.Message);
            Assert.AreEqual(inner, target.InnerException);
        }

        ///// <summary>
        /////     A test for RunnableException Constructor using serialization.
        ///// </summary>
        //[TestMethod]
        //public void RunnableExceptionConstructorTestWithSerialization()
        //{
        //    var expectedMessage = "This is a test of RunnableException";

        //    // Just need a specific exception type for the inner exception
        //    Exception innerException = new ArgumentException("this is the inner exception");

        //    var runnableException = new RunnableException(expectedMessage, innerException);
        //    var stream = new MemoryStream();

        //    var formatter = new BinaryFormatter();

        //    formatter.Serialize(stream, runnableException);

        //    stream.Position = 0;

        //    var deserializedException = (RunnableException)formatter.Deserialize(stream);

        //    Assert.AreEqual(expectedMessage, deserializedException.Message);
        //    Assert.AreEqual(innerException.Message, deserializedException.InnerException.Message);
        //}
    }
}