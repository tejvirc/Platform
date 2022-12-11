namespace Aristocrat.Monaco.Application.Tests.NoteAcceptor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using Application.NoteAcceptor;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This is a test class for NoteAcceptorCoordinatingException
    /// </summary>
    [TestClass]
    public class NoteAcceptorCoordinatingExceptionTest
    {
        [TestMethod]
        public void NoteAcceptorCoordinatingExceptionConstructorTestDefault()
        {
            NoteAcceptorCoordinatingException exception = new NoteAcceptorCoordinatingException();
            Assert.IsNotNull(exception);

            string defaultString = string.Format(
                CultureInfo.CurrentCulture,
                "Exception of type '{0}' was thrown.",
                exception.GetType());

            Assert.AreEqual(exception.Message, defaultString);
        }

        [TestMethod]
        public void NoteAcceptorCoordinatingExceptionConstructorTestMessage()
        {
            const string message = "Test Exception Message";
            NoteAcceptorCoordinatingException exception = new NoteAcceptorCoordinatingException(message);
            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.Message, message);
        }

        [TestMethod]
        public void NoteAcceptorCoordinatingExceptionConstructorTestMessageAndInnerException()
        {
            const string primaryMessage = "Test Exception Message";
            const string innerMessage = "Test Inner Exception Message";

            NoteAcceptorCoordinatingException exception =
                new NoteAcceptorCoordinatingException(primaryMessage, new InvalidOperationException(innerMessage));
            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.Message, primaryMessage);
            Assert.AreEqual(exception.InnerException.Message, innerMessage);
        }

        //[TestMethod]
        //public void NoteAcceptorCoordinatingExceptionConstructorTestDeserialization()
        //{
        //    const string primaryMessage = "Test Exception Message";
        //    const string innerMessage = "Test Inner Exception Message";

        //    NoteAcceptorCoordinatingException exception =
        //        new NoteAcceptorCoordinatingException(primaryMessage, new InvalidOperationException(innerMessage));
        //    Assert.IsNotNull(exception);

        //    MemoryStream memoryStream = new MemoryStream();
        //    BinaryFormatter formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.All));
        //    formatter.Serialize(memoryStream, exception);

        //    memoryStream.Position = 0;
        //    NoteAcceptorCoordinatingException deserializedExcept =
        //        (NoteAcceptorCoordinatingException)formatter.Deserialize(memoryStream);

        //    Assert.AreEqual(exception.Message, deserializedExcept.Message);
        //    Assert.AreEqual(exception.InnerException.Message, deserializedExcept.InnerException.Message);
        //}
    }
}