namespace Aristocrat.Monaco.Application.Contracts.Tests
{
    #region Using

    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Vgt.Client12.Application.OperatorMenu;

    #endregion

    /// <summary>
    ///     Tests the exception classes in the Accounting.Contracts namespace
    /// </summary>
    [TestClass]
    public class ExceptionTests
    {
        /// <summary>
        ///     A test for OperatorMenuException Constructor
        /// </summary>
        [TestMethod]
        public void OperatorMenuExceptionConstructorTest()
        {
            var target = new OperatorMenuException();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(OperatorMenuException));
            Assert.IsNull(target.InnerException);
        }

        /// <summary>
        ///     A test for OperatorMenuException Constructor with a message
        /// </summary>
        [TestMethod]
        public void OperatorMenuExceptionConstructorWithMessage()
        {
            var message = "This is a test of OperatorMenuException";
            var target = new OperatorMenuException(message);
            Assert.AreEqual(message, target.Message);
            Assert.IsNull(target.InnerException);
        }

        /// <summary>
        ///     A test for OperatorMenuException Constructor with message and inner exception.
        /// </summary>
        [TestMethod]
        public void OperatorMenuExceptionConstructorTestWithInnerException()
        {
            var message = "This is a test of OperatorMenuException";

            // Just need a specific exception type for the inner exception
            Exception inner = new ArgumentException("this is the inner exception");

            var target = new OperatorMenuException(message, inner);

            Assert.AreEqual(message, target.Message);
            Assert.AreEqual(inner, target.InnerException);
        }

        /// <summary>
        ///     A test for OperatorMenuException Constructor using serialization.
        /// </summary>
        [TestMethod]
        public void OperatorMenuExceptionConstructorTestWithSerialization()
        {
            var expectedMessage = "This is a test of OperatorMenuException";

            // Just need a specific exception type for the inner exception
            Exception innerException = new ArgumentException("this is the inner exception");

            var runnableException = new OperatorMenuException(expectedMessage, innerException);
            var stream = new MemoryStream();

            var formatter = new BinaryFormatter();

            formatter.Serialize(stream, runnableException);

            stream.Position = 0;

            var deserializedException = (OperatorMenuException)formatter.Deserialize(stream);

            Assert.AreEqual(expectedMessage, deserializedException.Message);
            Assert.AreEqual(innerException.Message, deserializedException.InnerException.Message);
        }

        /// <summary>
        ///     A test for MeterNotFoundException Constructor
        /// </summary>
        [TestMethod]
        public void MeterNotFoundExceptionConstructorTest()
        {
            var target = new MeterNotFoundException();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(MeterNotFoundException));
            Assert.IsNull(target.InnerException);
        }

        /// <summary>
        ///     A test for MeterNotFoundException Constructor with a message
        /// </summary>
        [TestMethod]
        public void MeterNotFoundExceptionConstructorWithMessage()
        {
            var message = "This is a test of MeterNotFoundException";
            var target = new MeterNotFoundException(message);
            Assert.AreEqual(message, target.Message);
            Assert.IsNull(target.InnerException);
        }

        /// <summary>
        ///     A test for MeterNotFoundException Constructor with message and inner exception.
        /// </summary>
        [TestMethod]
        public void MeterNotFoundExceptionConstructorTestWithInnerException()
        {
            var message = "This is a test of MeterNotFoundException";

            // Just need a specific exception type for the inner exception
            Exception inner = new ArgumentException("this is the inner exception");

            var target = new MeterNotFoundException(message, inner);

            Assert.AreEqual(message, target.Message);
            Assert.AreEqual(inner, target.InnerException);
        }

        /// <summary>
        ///     A test for MeterNotFoundException Constructor using serialization.
        /// </summary>
        [TestMethod]
        public void MeterNotFoundExceptionConstructorTestWithSerialization()
        {
            var expectedMessage = "This is a test of MeterNotFoundException";

            // Just need a specific exception type for the inner exception
            Exception innerException = new ArgumentException("this is the inner exception");

            var runnableException = new MeterNotFoundException(expectedMessage, innerException);
            var stream = new MemoryStream();

            var formatter = new BinaryFormatter();

            formatter.Serialize(stream, runnableException);

            stream.Position = 0;

            var deserializedException = (MeterNotFoundException)formatter.Deserialize(stream);

            Assert.AreEqual(expectedMessage, deserializedException.Message);
            Assert.AreEqual(innerException.Message, deserializedException.InnerException.Message);
        }

        /// <summary>
        ///     A test for MeterException Constructor
        /// </summary>
        [TestMethod]
        public void MeterExceptionConstructorTest()
        {
            var target = new MeterException();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(MeterException));
            Assert.IsNull(target.InnerException);
        }

        /// <summary>
        ///     A test for MeterNotFoundException Constructor with a message
        /// </summary>
        [TestMethod]
        public void MeterExceptionConstructorWithMessage()
        {
            var message = "This is a test of MeterException";
            var target = new MeterException(message);
            Assert.AreEqual(message, target.Message);
            Assert.IsNull(target.InnerException);
        }

        /// <summary>
        ///     A test for MeterNotFoundException Constructor with message and inner exception.
        /// </summary>
        [TestMethod]
        public void MeterExceptionConstructorTestWithInnerException()
        {
            var message = "This is a test of MeterException";

            // Just need a specific exception type for the inner exception
            Exception inner = new ArgumentException("this is the inner exception");

            var target = new MeterException(message, inner);

            Assert.AreEqual(message, target.Message);
            Assert.AreEqual(inner, target.InnerException);
        }

        /// <summary>
        ///     A test for MeterNotFoundException Constructor using serialization.
        /// </summary>
        [TestMethod]
        public void MeterExceptionConstructorTestWithSerialization()
        {
            var expectedMessage = "This is a test of MeterException";

            // Just need a specific exception type for the inner exception
            Exception innerException = new ArgumentException("this is the inner exception");

            var runnableException = new MeterException(expectedMessage, innerException);
            var stream = new MemoryStream();

            var formatter = new BinaryFormatter();

            formatter.Serialize(stream, runnableException);

            stream.Position = 0;

            var deserializedException = (MeterException)formatter.Deserialize(stream);

            Assert.AreEqual(expectedMessage, deserializedException.Message);
            Assert.AreEqual(innerException.Message, deserializedException.InnerException.Message);
        }
    }
}