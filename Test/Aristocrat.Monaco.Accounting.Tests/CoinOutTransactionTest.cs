namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts.Hopper;
    using Contracts.TransferOut;
    using Hardware.Contracts.Persistence;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     This class performs a series of tests on Coin Out Transaction.
    /// </summary>
    [TestClass]
    public class CoinOutTransactionTest
    {
        // some random values for the transaction
        private const int DeviceId = 23;

        private const long TransactionId = 98784;
        private const long Amount = 45000;
        private const long LogSequence = 41;
        private static readonly DateTime DateTime = DateTime.Now;
        private Mock<IPersistentStorageAccessor> _block;

        private CoinOutTransaction _target;

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            _block = MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Strict);
            _target = new CoinOutTransaction();
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
            Assert.IsNotNull(_target);
            Assert.AreEqual(0, _target.DeviceId);
            Assert.AreEqual(DateTime.MinValue, _target.TransactionDateTime);
            Assert.AreEqual(0, _target.TransactionId);
            Assert.AreEqual(0, _target.CashableAmount);
            Assert.AreEqual(0, _target.LogSequence);
        }

        /// <summary>
        ///     A test for the parameterized constructor and the properties,
        ///     all of which are read-only.
        /// </summary>
        [TestMethod]
        public void ParameterizedConstructorTest()
        {
            // Create a transaction with arbitrary data
            const int deviceId1 = 11;
            var dateTime = DateTime.Now;
            const long transactionId1 = (long)1 + int.MaxValue;
            const long amount1 = 199;
            const long logSequence1 = 29;

            _target = new CoinOutTransaction(
                deviceId1,
                dateTime,
                amount1,
                0)
            {
                TransactionId = transactionId1,
                LogSequence = logSequence1
            };

            Assert.AreEqual(deviceId1, _target.DeviceId);
            Assert.AreEqual(dateTime, _target.TransactionDateTime);
            Assert.AreEqual(transactionId1, _target.TransactionId);
            Assert.AreEqual(amount1, _target.CashableAmount);
            Assert.AreEqual(logSequence1, _target.LogSequence);
        }

        /// <summary>
        ///     A test for SetPersistence() by creating a Coin Out Transaction from persistence block.
        ///     Then channges the Coin Out Transaction to Host Id2 and SetPersitence() to  the block to that.
        ///     Then to insure the block is set correctly we change Coin Out Transaction's Host Id(the 1st one) back and
        ///     ReceivePersistence
        ///     Since the Coin Out Transaction's host id is now host id2, this ensures that the block was changed correctly from
        ///     SetPersistence()
        /// </summary>
        [TestMethod]
        public void SetPersistenceTest()
        {
            _target = CreateTransaction();
            var element = 0;
            var transaction = new Mock<IPersistentStorageTransaction>(MockBehavior.Strict);
            _block.Setup(m => m.Count).Returns(1);
            _block.Setup(m => m.StartTransaction()).Returns(transaction.Object);
            transaction.SetupSet(m => m[element, "DeviceId"] = DeviceId);
            transaction.SetupSet(m => m[element, "LogSequence"] = LogSequence);
            transaction.SetupSet(m => m[element, "TransactionDateTime"] = DateTime);
            transaction.SetupSet(m => m[element, "TransactionId"] = TransactionId);
            transaction.SetupSet(m => m[element, "CashableAmount"] = Amount);

            transaction.SetupSet(m => m[element, "AuthorizedCashableAmount"] = 0L);
            transaction.SetupSet(m => m[element, "TransferredCashableAmount"] = 0L);
            transaction.SetupSet(m => m[element, "Exception"] = false);
            transaction.SetupSet(m => m[element, "BankTransactionId"] = Guid.Empty);
            transaction.SetupSet(m => m[element, "OwnsBankTransaction"] = false);
            transaction.SetupSet(m => m[element, "AssociatedTransactions"] = "[]");
            transaction.SetupSet(m => m[element, "OwnsBankTransaction"] = false);
            transaction.SetupSet(m => m[element, "TraceId"] = Guid.Empty).Verifiable();
            transaction.SetupSet(m => m[element, "Reason"] = TransferOutReason.CashOut).Verifiable();

            transaction.Setup(m => m.Dispose()).Verifiable();
            transaction.Setup(m => m.Commit()).Verifiable();

            _target.SetPersistence(_block.Object, 0);

            _block.Verify();
            transaction.Verify();
        }

        /// <summary>
        ///     A test for ReceivePersistence() where the data is good
        /// </summary>
        [TestMethod]
        public void ReceivePersistenceTest()
        {
            var values = new Dictionary<string, object>
            {
                { "DeviceId", DeviceId },
                { "LogSequence", LogSequence },
                { "TransactionDateTime", DateTime },
                { "TransactionId", TransactionId },
                { "CashableAmount", Amount },
                { "AssociatedTransactions", string.Empty },

                { "AuthorizedCashableAmount", Amount + 3 },
                { "TransferredCashableAmount", Amount + 6 },
                { "Exception", false },
                { "BankTransactionId", Guid.NewGuid() },
                { "OwnsBankTransaction", false },
                { "TraceId", Guid.Empty },
                { "Reason", TransferOutReason.CashOut }
            };

            _target.SetData(values);
            Assert.AreEqual(DeviceId, _target.DeviceId);
            Assert.AreEqual(DateTime, _target.TransactionDateTime);
            Assert.AreEqual(TransactionId, _target.TransactionId);
            Assert.AreEqual(Amount, _target.CashableAmount);
            Assert.AreEqual(LogSequence, _target.LogSequence);
        }

        /// <summary>
        ///     A test for ToString()
        /// </summary>
        [TestMethod]
        public void ToStringTest()
        {
            _target = CreateTransaction();

            Assert.IsNotNull(_target.ToString());
        }

        /// <summary>
        ///     Tests the clone method
        /// </summary>
        [TestMethod]
        public void CloneTest()
        {
            _target = CreateTransaction();

            var copy = _target.Clone() as CoinOutTransaction;

            Assert.IsNotNull(copy);
            Assert.AreEqual(copy.CashableAmount, Amount);
            Assert.AreEqual(copy.DeviceId, DeviceId);
            Assert.AreEqual(copy.TransactionDateTime, DateTime);
            Assert.AreEqual(copy.TransactionId, TransactionId);
            Assert.AreEqual(copy.LogSequence, LogSequence);
        }

        /// <summary>
        ///     Tests for equality against itself.
        /// </summary>
        [TestMethod]
        public void SelfEqualityTest()
        {
            _target = CreateTransaction();

            var same = _target.Clone() as CoinOutTransaction;

            Assert.IsTrue(_target.Equals(_target));
            Assert.IsTrue(same?.Equals(_target));
            Assert.IsTrue(_target == same);
            Assert.IsTrue(same == _target);
            Assert.IsFalse(same != _target);
            Assert.IsFalse(_target != same);
            Assert.AreEqual(_target.GetHashCode(), same?.GetHashCode());
        }

        /// <summary>
        ///     Tests that a non transaction object will fail.
        /// </summary>
        [TestMethod]
        public void DifferentObjectTest()
        {
            _target = CreateTransaction();

            Assert.IsFalse(_target.Equals(new object()));
            Assert.IsFalse(_target == null);
            Assert.IsTrue(_target != null);
            Assert.IsFalse(_target?.Equals(null));

            _target = null;
            Assert.IsTrue(_target == null);
        }

        /// <summary>
        ///     This is for all variations of null comparison.
        /// </summary>
        [TestMethod]
        public void NullTests()
        {
            _target = CreateTransaction();

            const CoinOutTransaction Null = null;

            Assert.IsTrue(_target != null);
            Assert.IsTrue(_target != Null);
            Assert.IsFalse(_target == null);
            Assert.IsFalse(_target == Null);
            Assert.IsFalse(_target?.Equals(Null));
            Assert.IsFalse(_target?.Equals(null));

            Assert.IsTrue(Null == null);
            Assert.IsFalse(Null != null);

            // in this case order may matter for the overrides
            Assert.IsTrue(null != _target);
            Assert.IsTrue(Null != _target);
            Assert.IsFalse(null == _target);
            Assert.IsFalse(Null == _target);

            Assert.IsTrue(null == Null);
            Assert.IsFalse(null != Null);
        }

        /// <summary>
        ///     Creates a wat off transaction with random data.
        /// </summary>
        /// <returns>The transaction.</returns>
        private CoinOutTransaction CreateTransaction()
        {
            _target = new CoinOutTransaction(
                DeviceId,
                DateTime,
                Amount,
                0)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId
            };

            return _target;
        }
    }
}
