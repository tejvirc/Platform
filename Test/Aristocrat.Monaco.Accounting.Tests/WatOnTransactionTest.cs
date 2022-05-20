namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Contracts;
    using Contracts.Wat;
    using Hardware.Contracts.Persistence;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Definition of the WatOnTransactionTest class
    /// </summary>
    [TestClass]
    public class WatOnTransactionTest
    {
        private Mock<IPersistentStorageAccessor> _block;

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            _block = MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Strict);
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
        }

        /// <summary>
        ///     A test for the parameterless constructor.
        /// </summary>
        [TestMethod]
        public void ParameterlessConstructorTest()
        {
            var target = new WatOnTransaction();

            Assert.AreEqual(0, target.DeviceId);
            Assert.AreEqual(DateTime.MinValue, target.TransactionDateTime);
            Assert.AreEqual(0, target.TransactionId);
            Assert.AreEqual(0, target.CashableAmount);
            Assert.AreEqual(0, target.PromoAmount);
            Assert.AreEqual(0, target.NonCashAmount);
            Assert.AreEqual(0, target.LogSequence);
            Assert.IsFalse(target.AllowReducedAmounts);
            Assert.AreEqual(null, target.RequestId);
            Assert.AreEqual("Transfer In", target.Name);
        }

        /// <summary>
        ///     A test for the parameterized constructor and the properties,
        ///     all of which are read-only.
        /// </summary>
        [TestMethod]
        public void ParameterizedConstructorTest()
        {
            // Create a transaction with arbitrary data
            const int DeviceId = 11;
            var dateTime = DateTime.Now;
            const long TransactionId = (long)1 + int.MaxValue;
            const long Amount = 199;
            const long LogSequence = 29;
            const bool FullAmountTransferred = true;
            const string HostTransactionId = "123456789123456789";

            var target = new WatOnTransaction(
                DeviceId,
                dateTime,
                Amount,
                0,
                0,
                FullAmountTransferred,
                HostTransactionId)
            {
                TransactionId = TransactionId,
                LogSequence = LogSequence
            };

            Assert.AreEqual(DeviceId, target.DeviceId);
            Assert.AreEqual(dateTime, target.TransactionDateTime);
            Assert.AreEqual(TransactionId, target.TransactionId);
            Assert.AreEqual(Amount, target.CashableAmount);
            Assert.AreEqual(0, target.PromoAmount);
            Assert.AreEqual(0, target.NonCashAmount);
            Assert.AreEqual(LogSequence, target.LogSequence);
            Assert.AreEqual(FullAmountTransferred, target.AllowReducedAmounts);
            Assert.AreEqual(HostTransactionId, target.RequestId);
            Assert.AreEqual("Transfer In", target.Name);
        }

        /// <summary>
        ///     A test for ReceivePersistence() where the data is good
        /// </summary>
        [TestMethod]
        public void ReceivePersistenceTest()
        {
            // Create arbitrary transaction data
            const int DeviceId = 11;
            var dateTime = DateTime.Now;
            const long TransactionId = (long)1 + int.MaxValue;
            const long Cash = 199;
            const long Promo = 300;
            const long NonCash = 450;
            const long LogSequence = 2;
            const bool AllowReducedAmounts = true;
            const string RequestId = "000123456789123456";
            const int HostException = 3;
            const int EgmException = 6;
            const bool OwnsBankTransaction = false;
            const WatStatus Status = WatStatus.CancelReceived;
            var BankTransactionId = Guid.NewGuid();

            var values = new Dictionary<string, object>
            {
                { "DeviceId", DeviceId },
                { "LogSequence", LogSequence },
                { "TransactionDateTime", dateTime },
                { "TransactionId", TransactionId },
                { "CashableAmount", Cash },
                { "PromoAmount", Promo },
                { "NonCashAmount", NonCash },
                { "AllowReducedAmounts", AllowReducedAmounts },
                { "RequestId", RequestId },
                { "AuthorizedCashableAmount", Cash },
                { "AuthorizedPromoAmount", Promo },
                { "AuthorizedNonCashAmount", NonCash },
                { "TransferredCashableAmount", Cash },
                { "TransferredPromoAmount", Promo },
                { "TransferredNonCashAmount", NonCash },
                { "HostException", HostException },
                { "EgmException", EgmException },
                { "OwnsBankTransaction", OwnsBankTransaction },
                { "BankTransactionId", BankTransactionId },
                { "Status", Status }
            };

            var target = new WatOnTransaction();
            target.SetData(values);
        }

        /// <summary>
        ///     A test for SetPersistenceTest() where the data is good
        /// </summary>
        [TestMethod]
        public void SetPersistenceTest()
        {
            const int element = 0;

            // Create arbitrary transaction data
            const int DeviceId = 11;
            var dateTime = DateTime.Now;
            const long TransactionId = (long)1 + int.MaxValue;
            const long Cash = 199;
            const long Promo = 300;
            const long NonCash = 450;
            const long LogSequence = 2;
            const bool AllowReducedAmounts = true;
            const string RequestId = "000123456789123456";
            const int HostException = 3;
            const int EgmException = 6;
            const bool OwnsBankTransaction = false;
            const WatStatus Status = WatStatus.CancelReceived;
            var BankTransactionId = Guid.NewGuid();

            var target = new WatOnTransaction(
                DeviceId,
                dateTime,
                Cash,
                Promo,
                NonCash,
                AllowReducedAmounts,
                RequestId)
            {
                TransactionId = TransactionId,
                LogSequence = LogSequence,
                OwnsBankTransaction = OwnsBankTransaction,
                HostException = HostException,
                EgmException = EgmException,
                Status = Status,
                AuthorizedPromoAmount = Promo,
                AuthorizedCashableAmount = Cash,
                AuthorizedNonCashAmount = NonCash,
                TransferredNonCashAmount = NonCash,
                TransferredPromoAmount = Promo,
                TransferredCashableAmount = Cash,
                BankTransactionId = BankTransactionId
            };

            var transaction = new Mock<IPersistentStorageTransaction>(MockBehavior.Strict);
            _block.Setup(m => m.Count).Returns(1);
            _block.Setup(m => m.StartTransaction()).Returns(transaction.Object);
            transaction.SetupSet(m => m[element, "DeviceId"] = DeviceId);
            transaction.SetupSet(m => m[element, "LogSequence"] = LogSequence);
            transaction.SetupSet(m => m[element, "TransactionDateTime"] = dateTime);
            transaction.SetupSet(m => m[element, "TransactionId"] = TransactionId);
            transaction.SetupSet(m => m[element, "CashableAmount"] = Cash);
            transaction.SetupSet(m => m[element, "PromoAmount"] = Promo);
            transaction.SetupSet(m => m[element, "NonCashAmount"] = NonCash);
            transaction.SetupSet(m => m[element, "TransferredCashableAmount"] = Cash);
            transaction.SetupSet(m => m[element, "TransferredPromoAmount"] = Promo);
            transaction.SetupSet(m => m[element, "TransferredNonCashAmount"] = NonCash);
            transaction.SetupSet(m => m[element, "AuthorizedCashableAmount"] = Cash);
            transaction.SetupSet(m => m[element, "AuthorizedPromoAmount"] = Promo);
            transaction.SetupSet(m => m[element, "AuthorizedNonCashAmount"] = NonCash);
            transaction.SetupSet(m => m[element, "AllowReducedAmounts"] = AllowReducedAmounts);
            transaction.SetupSet(m => m[element, "RequestId"] = RequestId);
            transaction.SetupSet(m => m[element, "HostException"] = HostException);
            transaction.SetupSet(m => m[element, "EgmException"] = EgmException);
            transaction.SetupSet(m => m[element, "OwnsBankTransaction"] = OwnsBankTransaction);
            transaction.SetupSet(m => m[element, "BankTransactionId"] = BankTransactionId);
            transaction.SetupSet(m => m[element, "Status"] = Status);
            transaction.Setup(m => m.Dispose()).Verifiable();
            transaction.Setup(m => m.Commit()).Verifiable();

            target.SetPersistence(_block.Object, element);

            _block.Verify();
            transaction.Verify();
        }

        /// <summary>
        ///     A test for ToString()
        /// </summary>
        [TestMethod]
        public void ToStringTest()
        {
            // Create transaction with arbitrary data
            const int DeviceId = 23;
            var dateTime = DateTime.Now;
            const long TransactionId = 98784;
            const long Amount = 45000;
            const long LogSequence = 41;
            const bool FullAmountTransferred = true;
            const string HostTransactionId = "999888777666555444";

            var target = new WatOnTransaction(
                DeviceId,
                dateTime,
                Amount,
                0,
                0,
                FullAmountTransferred,
                HostTransactionId)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId
            };

            Assert.IsNotNull(target.ToString());
        }

        /// <summary>
        ///     Tests the clone method
        /// </summary>
        [TestMethod]
        public void CloneTest()
        {
            // Create a transaction with arbitrary data
            const int DeviceId = 11;
            var dateTime = DateTime.Now;
            const long TransactionId = (long)1 + int.MaxValue;
            const long Amount = 199;
            const long LogSequence = 29;
            const bool FullAmountTransferred = true;
            const int EgmException = 3;
            const int HostException = 10;
            const string HostTransactionId = "123456789123456789";

            var target = new WatOnTransaction(
                DeviceId,
                dateTime,
                Amount,
                0,
                0,
                FullAmountTransferred,
                HostTransactionId)
            {
                TransactionId = TransactionId,
                LogSequence = LogSequence,
                EgmException = EgmException,
                HostException = HostException
            };

            var copy = target.Clone() as WatOnTransaction;

            Assert.IsNotNull(copy);
            Assert.AreEqual(Amount, copy.CashableAmount);
            Assert.AreEqual(0, copy.PromoAmount);
            Assert.AreEqual(0, copy.NonCashAmount);
            Assert.AreEqual(copy.DeviceId, DeviceId);
            Assert.AreEqual(copy.RequestId, HostTransactionId);
            Assert.AreEqual(copy.TransactionDateTime, dateTime);
            Assert.AreEqual(copy.TransactionId, TransactionId);
            Assert.AreEqual(copy.LogSequence, LogSequence);
            Assert.AreEqual(copy.AllowReducedAmounts, FullAmountTransferred);
            Assert.AreEqual(HostException, copy.HostException);
            Assert.AreEqual(EgmException, copy.EgmException);
        }

        /// <summary>
        ///     Tests for equality against itself.
        /// </summary>
        [TestMethod]
        public void SelfEqualityTest()
        {
            // Create a transaction with arbitrary data
            const int DeviceId = 11;
            var dateTime = DateTime.Now;
            const long TransactionId = (long)1 + int.MaxValue;
            const long Amount = 199;
            const long LogSequence = 29;
            const bool FullAmountTransferred = true;
            const string HostTransactionId = "123456789123456789";

            var target = new WatOnTransaction(
                DeviceId,
                dateTime,
                Amount,
                0,
                0,
                FullAmountTransferred,
                HostTransactionId)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId
            };

            var same = target.Clone() as WatOnTransaction;

            Assert.AreEqual(same, target);
            Assert.IsTrue(target == same);
            Assert.IsTrue(same == target);
            Assert.IsFalse(same != target);
            Assert.IsFalse(target != same);
            Assert.AreEqual(target.GetHashCode(), same.GetHashCode());
        }

        /// <summary>
        ///     Tests that two transactions that differ only by FullAmountTransferred compare as being different.
        /// </summary>
        [TestMethod]
        public void FullAmountTransferredInequalityTest()
        {
            // Create a transaction with arbitrary data
            const int DeviceId = 11;
            var dateTime = DateTime.Now;
            const long TransactionId = (long)1 + int.MaxValue;
            const long Amount = 199;
            const long LogSequence = 29;
            const bool FullAmountTransferred1 = true;
            const bool FullAmountTransferred2 = false;
            const string HostTransactionId = "123456789123456789";

            var target = new WatOnTransaction(
                DeviceId,
                dateTime,
                Amount,
                0,
                0,
                FullAmountTransferred1,
                HostTransactionId)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId
            };

            var different = new WatOnTransaction(
                DeviceId,
                dateTime,
                Amount,
                0,
                0,
                FullAmountTransferred2,
                HostTransactionId)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId
            };

            Assert.IsFalse(target.Equals(different));
            Assert.IsFalse(different.Equals(target));
            Assert.IsFalse(target.Equals(different as object));
            Assert.IsFalse(different.Equals(target as object));
            Assert.IsFalse(target == different);
            Assert.IsFalse(different == target);
            Assert.IsTrue(different != target);
            Assert.IsTrue(target != different);
            Assert.AreNotEqual(target.GetHashCode(), different.GetHashCode());
        }

        /// <summary>
        ///     Tests that two transactions that differ only by HostTransactionId compare as being different.
        /// </summary>
        [TestMethod]
        public void HostTransactionIdInequalityTest()
        {
            // Create a transaction with arbitrary data
            const int DeviceId = 11;
            var dateTime = DateTime.Now;
            const long TransactionId = (long)1 + int.MaxValue;
            const long Amount = 199;
            const long LogSequence = 29;
            const bool FullAmountTransferred = true;
            const string HostTransactionId = "123456789123456789";
            const string HostTransactionId2 = "987654321098765432";

            var target = new WatOnTransaction(
                DeviceId,
                dateTime,
                Amount,
                0,
                0,
                FullAmountTransferred,
                HostTransactionId)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId
            };

            var different = new WatOnTransaction(
                DeviceId,
                dateTime,
                Amount,
                0,
                0,
                FullAmountTransferred,
                HostTransactionId2)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId
            };

            Assert.IsFalse(target.Equals(different));
            Assert.IsFalse(different.Equals(target));
            Assert.IsFalse(target.Equals(different as object));
            Assert.IsFalse(different.Equals(target as object));
            Assert.IsFalse(target == different);
            Assert.IsFalse(different == target);
            Assert.IsTrue(different != target);
            Assert.IsTrue(target != different);
            Assert.AreNotEqual(target.GetHashCode(), different.GetHashCode());
        }

        /// <summary>
        ///     Tests that a non transaction object will fail.
        /// </summary>
        [TestMethod]
        public void DifferentObjectTest()
        {
            // Create a transaction with arbitrary data
            const int DeviceId = 11;
            var dateTime = DateTime.Now;
            const long TransactionId = (long)1 + int.MaxValue;
            const long Amount = 199;
            const long LogSequence = 29;
            const bool FullAmountTransferred = true;
            const string HostTransactionId = "123456789123456789";

            var target = new WatOnTransaction(
                DeviceId,
                dateTime,
                Amount,
                0,
                0,
                FullAmountTransferred,
                HostTransactionId)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId
            };

            Assert.IsFalse(target.Equals(new object()));
            Assert.IsFalse(target == null);
            Assert.IsTrue(target != null);
            Assert.IsFalse(target.Equals(null));

            target = null;
            Assert.IsTrue(target == null);
        }

        /// <summary>
        ///     This is for all variations of null comparison.
        /// </summary>
        [TestMethod]
        public void NullTests()
        {
            // Create a transaction with arbitrary data
            const int DeviceId = 11;
            var dateTime = DateTime.Now;
            const long TransactionId = (long)1 + int.MaxValue;
            const long Amount = 199;
            const long LogSequence = 29;
            const bool FullAmountTransferred = true;
            const string HostTransactionId = "123456789123456789";

            var target = new WatOnTransaction(
                DeviceId,
                dateTime,
                Amount,
                0,
                0,
                FullAmountTransferred,
                HostTransactionId)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId
            };

            const WatOnTransaction Null = null;

            Assert.IsTrue(target != null);
            Assert.IsTrue(target != Null);
            Assert.IsFalse(target == null);
            Assert.IsFalse(target == Null);
            Assert.IsFalse(target.Equals(Null));
            Assert.IsFalse(target.Equals(null));

            Assert.IsTrue(Null == null);
            Assert.IsFalse(Null != null);

            // in this case order may matter for the overrides
            Assert.IsTrue(null != target);
            Assert.IsTrue(Null != target);
            Assert.IsFalse(null == target);
            Assert.IsFalse(Null == target);

            Assert.IsTrue(null == Null);
            Assert.IsFalse(null != Null);
        }
    }
}