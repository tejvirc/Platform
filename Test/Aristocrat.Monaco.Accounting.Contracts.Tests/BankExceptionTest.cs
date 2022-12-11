namespace Aristocrat.Monaco.Accounting.Contracts.Tests
{
    #region Using

    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Summary description for BankExceptionTest
    /// </summary>
    [TestClass]
    public class BankExceptionTest
    {
        [TestMethod]
        public void Constructor1Test()
        {
            var target = new BankException();
            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void Constructor2Test()
        {
            string message = "Just a test";
            var target = new BankException(message);
            Assert.IsNotNull(target);
            Assert.AreEqual(message, target.Message);
        }

        [TestMethod]
        public void Constructor3Test()
        {
            string message = "Just a test";
            Exception innerException = new Exception();
            var target = new BankException(message, innerException);
            Assert.IsNotNull(target);
            Assert.AreEqual(message, target.Message);
            Assert.AreEqual(innerException, target.InnerException);
        }

        ///// <summary>
        /////     A test for serializing and deserializing the BankException.
        ///// </summary>
        //[TestMethod]
        //public void SerializeClearedEventTest()
        //{
        //    string message = "Just a test";
        //    Exception innerException = new Exception();
        //    var target = new BankException(message);

        //    MemoryStream memoryStream = new MemoryStream();

        //    BinaryFormatter binaryFormatter = new BinaryFormatter();
        //    binaryFormatter.Serialize(memoryStream, target);

        //    memoryStream.Seek(0, SeekOrigin.Begin);

        //    BankException deserializedException =
        //        (BankException)binaryFormatter.Deserialize(memoryStream);

        //    Assert.AreEqual(message, deserializedException.Message);
        //}
    }
}