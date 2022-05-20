namespace Aristocrat.Monaco.Accounting.Tests
{
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Soap;
    using Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This is a test class for TransactionExceptionTest and is intended
    ///     to contain all TransactionExceptionTest Unit Tests
    /// </summary>
    [TestClass]
    public class TransactionExceptionTest
    {
        /// <summary>
        ///     An instance of TestContext that provides information about the current test run.
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
        ///     A test for TransactionException Constructor
        /// </summary>
        [TestMethod]
        public void TransactionExceptionConstructorTest3()
        {
            var message = "Test exception message";
            var target = new TransactionException(message);

            Assert.AreEqual(message, target.Message);
        }

        /// <summary>
        ///     A test for TransactionException Constructor
        /// </summary>
        [TestMethod]
        public void TransactionExceptionConstructorTest2()
        {
            var target = new TransactionException();

            // As long as an exception is created and another exception is not thrown
            // then this test is considered passing
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///     A test for TransactionException Constructor
        /// </summary>
        [TestMethod]
        public void TransactionExceptionConstructorTest1()
        {
            var message = "This is a test exception message";
            var exception = new TransactionException(message);
            var stream = new FileStream("exception.dat", FileMode.Create);

            try
            {
                var formatter = new SoapFormatter(
                    null,
                    new StreamingContext(StreamingContextStates.File));

                formatter.Serialize(stream, exception);

                stream.Position = 0;
                var deserializedException = (TransactionException)formatter.Deserialize(stream);

                throw deserializedException;
            }
            catch (TransactionException deserializedException)
            {
                Assert.AreEqual(message, deserializedException.Message);
            }
            finally
            {
                stream.Close();
            }
        }

        /// <summary>
        ///     A test for TransactionException Constructor
        /// </summary>
        [TestMethod]
        public void TransactionExceptionConstructorTest()
        {
            var message = "Test exception message";
            var innerException = new TransactionException("inner exception");

            var target = new TransactionException(message, innerException);

            Assert.AreEqual(innerException, target.InnerException);
            Assert.AreEqual(message, target.Message);
            Assert.AreEqual("inner exception", target.InnerException.Message);
        }
    }
}