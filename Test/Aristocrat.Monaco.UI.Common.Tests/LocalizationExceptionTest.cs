namespace Aristocrat.Monaco.UI.Common.Tests
{
    #region Using

    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Contains tests for the LocalizationException class
    /// </summary>
    [TestClass]
    public class LocalizationExceptionTest
    {
        [TestMethod]
        public void ParameterlessConstructorTest()
        {
            Assert.IsNotNull(new LocalizationException());
        }

        [TestMethod]
        public void ConstructorWithMessageTest()
        {
            var message = "This is a test of LocalizationException";
            var target = new LocalizationException(message);

            Assert.IsNotNull(target);
            Assert.AreEqual(message, target.Message);
            Assert.IsNull(target.InnerException);
        }

        [TestMethod]
        public void ConstructorWithMessageAndInnerExceptionTest()
        {
            string message = "Test";
            var exception = new Exception();
            var target = new LocalizationException(message, exception);

            Assert.IsNotNull(target);
            Assert.AreEqual(message, target.Message);
            Assert.AreEqual(exception, target.InnerException);
        }

        //[TestMethod]
        //public void SerializedConstructorTest()
        //{
        //    var expectedMessage = "This is a test of LocalizationException";

        //    // Just need a specific exception type for the inner exception
        //    Exception innerException = new ArgumentException("this is the inner exception");

        //    var runnableException = new LocalizationException(expectedMessage, innerException);
        //    var stream = new MemoryStream();

        //    var formatter = new BinaryFormatter();

        //    formatter.Serialize(stream, runnableException);

        //    stream.Position = 0;

        //    var deserializedException = (LocalizationException)formatter.Deserialize(stream);

        //    Assert.AreEqual(expectedMessage, deserializedException.Message);
        //    Assert.AreEqual(innerException.Message, deserializedException.InnerException.Message);
        //}
    }
}