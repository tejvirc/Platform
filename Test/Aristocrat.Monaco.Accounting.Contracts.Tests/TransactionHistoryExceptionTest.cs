namespace Aristocrat.Monaco.Accounting.Contracts.Tests
{
    #region Using

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Summary description for TransactionHistoryExceptionTest
    /// </summary>
    [TestClass]
    public class TransactionHistoryExceptionTest
    {
        [TestMethod]
        public void Constructor1Test()
        {
            var target = new TransactionHistoryException();
            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void Constructor2Test()
        {
            string message = "Just a test";
            var target = new TransactionHistoryException(message);
            Assert.IsNotNull(target);
            Assert.AreEqual(message, target.Message);
        }

        [TestMethod]
        public void Constructor3Test()
        {
            string message = "Just a test";
            Exception innerException = new Exception();
            var target = new TransactionHistoryException(message, innerException);
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
        //    var target = new TransactionHistoryException(message);

        //    MemoryStream memoryStream = new MemoryStream();

        //    BinaryFormatter binaryFormatter = new BinaryFormatter();
        //    binaryFormatter.Serialize(memoryStream, target);

        //    memoryStream.Seek(0, SeekOrigin.Begin);

        //    TransactionHistoryException deserializedException =
        //        (TransactionHistoryException)binaryFormatter.Deserialize(memoryStream);

        //    Assert.AreEqual(message, deserializedException.Message);
        //}

        [TestMethod]
        public void AttachTransactionTest()
        {
            ITransaction transaction = new MyTransaction();
            var target = new TransactionHistoryException();

            target.AttachTransaction(transaction);

            Assert.AreEqual(transaction, target.Transaction);
        }

        public class MyTransaction : ITransaction
        {
            public long Amount
            {
                get { throw new NotImplementedException(); }
            }

            public int DeviceId
            {
                get { throw new NotImplementedException(); }
            }

            public long LogSequence
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public string Name
            {
                get { throw new NotImplementedException(); }
            }

            public IEnumerable<long> AssociatedTransactions { get; set; }

            public DateTime TransactionDateTime
            {
                get { throw new NotImplementedException(); }
            }

            public long TransactionId
            {
                get { throw new NotImplementedException(); }

                set { throw new NotImplementedException(); }
            }

            public AccountType TypeOfAccount
            {
                get { throw new NotImplementedException(); }
            }

            public object Clone()
            {
                throw new NotImplementedException();
            }
        }
    }
}