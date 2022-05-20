namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using System.Collections.Generic;
    using Contracts.TransferOut;
    using Contracts.Wat;
    using Hardware.Contracts.Persistence;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     This class performs a series of tests on WatOffTransaction.
    /// </summary>
    [TestClass]
    public class WatOffTransactionUnitTest
    {
        // some random values for the transaction
        private const int DeviceId = 23;

        private const long TransactionId = 98784;
        private const long Amount = 45000;
        private const long LogSequence = 41;
        private const string RequestId = "123456789123456789";
        private static readonly DateTime DateTime = DateTime.Now;
        private Mock<IPersistentStorageAccessor> _block;

        private WatTransaction _target;

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            _block = MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Strict);
            _target = new WatTransaction();
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
            Assert.AreEqual(0, _target.PromoAmount);
            Assert.AreEqual(0, _target.NonCashAmount);
            Assert.AreEqual(0, _target.LogSequence);
            Assert.IsNull(_target.RequestId);
            Assert.AreEqual("Transfer Out", _target.Name);
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
            const string hostTransactionId1 = "123456789123456789";

            _target = new WatTransaction(
                deviceId1,
                dateTime,
                amount1,
                0,
                0,
                false,
                hostTransactionId1)
            {
                TransactionId = transactionId1,
                LogSequence = logSequence1
            };

            Assert.AreEqual(deviceId1, _target.DeviceId);
            Assert.AreEqual(dateTime, _target.TransactionDateTime);
            Assert.AreEqual(transactionId1, _target.TransactionId);
            Assert.AreEqual(amount1, _target.CashableAmount);
            Assert.AreEqual(0, _target.PromoAmount);
            Assert.AreEqual(0, _target.NonCashAmount);
            Assert.AreEqual(logSequence1, _target.LogSequence);
            Assert.AreEqual(hostTransactionId1, _target.RequestId);
            Assert.AreEqual("Transfer Out", _target.Name);
        }

        /// <summary>
        ///     A test for SetPersistence() by creating a WatOffTransaction from persistence block.
        ///     Then channges the WatOffTransaction to Host Id2 and SetPersitence() to  the block to that.
        ///     Then to insure the block is set correctly we change WatOffTransaction's Host Id(the 1st one) back and
        ///     ReceivePersistence
        ///     Since the WatOffTransaction's host id is now host id2, this ensures that the block was changed correctly from
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
            transaction.SetupSet(m => m[element, "RequestId"] = RequestId);
            transaction.SetupSet(m => m[element, "DeviceId"] = DeviceId);
            transaction.SetupSet(m => m[element, "LogSequence"] = LogSequence);
            transaction.SetupSet(m => m[element, "TransactionDateTime"] = DateTime);
            transaction.SetupSet(m => m[element, "TransactionId"] = TransactionId);
            transaction.SetupSet(m => m[element, "CashableAmount"] = Amount);
            transaction.SetupSet(m => m[element, "PromoAmount"] = 0L);
            transaction.SetupSet(m => m[element, "NonCashAmount"] = 0L);

            transaction.SetupSet(m => m[element, "AllowReducedAmounts"] = false);
            transaction.SetupSet(m => m[element, "PayMethod"] = WatPayMethod.Credit);
            transaction.SetupSet(m => m[element, "RequestId"] = null);
            transaction.SetupSet(m => m[element, "AuthorizedCashableAmount"] = 0L);
            transaction.SetupSet(m => m[element, "AuthorizedPromoAmount"] = 0L);
            transaction.SetupSet(m => m[element, "AuthorizedNonCashAmount"] = 0L);
            transaction.SetupSet(m => m[element, "HostException"] = 0);
            transaction.SetupSet(m => m[element, "TransferredCashableAmount"] = 0L);
            transaction.SetupSet(m => m[element, "TransferredPromoAmount"] = 0L);
            transaction.SetupSet(m => m[element, "TransferredNonCashAmount"] = 0L);
            transaction.SetupSet(m => m[element, "EgmException"] = 0);
            transaction.SetupSet(m => m[element, "Status"] = WatStatus.RequestReceived);
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
                { "AllowReducedAmounts", true },
                { "AssociatedTransactions", string.Empty },
                { "PayMethod", WatPayMethod.Voucher },
                { "PromoAmount", Amount + 1 },
                { "NonCashAmount", Amount + 2 },
                { "RequestId", RequestId },

                { "AuthorizedCashableAmount", Amount + 3 },
                { "AuthorizedPromoAmount", Amount + 4 },
                { "AuthorizedNonCashAmount", Amount + 5 },
                { "HostException", 7 },
                { "TransferredCashableAmount", Amount + 6 },
                { "TransferredPromoAmount", Amount + 7 },
                { "TransferredNonCashAmount", Amount + 8 },
                { "EgmException", 9 },
                { "Status", WatStatus.Authorized },
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
            Assert.AreEqual(Amount + 1, _target.PromoAmount);
            Assert.AreEqual(Amount + 2, _target.NonCashAmount);
            Assert.AreEqual(LogSequence, _target.LogSequence);
            Assert.AreEqual(RequestId, _target.RequestId);
            Assert.AreEqual("Transfer Out", _target.Name);
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

            var copy = _target.Clone() as WatTransaction;

            Assert.IsNotNull(copy);
            Assert.AreEqual(copy.CashableAmount, Amount);
            Assert.AreEqual(copy.DeviceId, DeviceId);
            Assert.AreEqual(copy.RequestId, RequestId);
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

            var same = _target.Clone() as WatTransaction;

            Assert.IsTrue(_target.Equals(_target));
            Assert.IsTrue(same.Equals(_target));
            Assert.IsTrue(_target == same);
            Assert.IsTrue(same == _target);
            Assert.IsFalse(same != _target);
            Assert.IsFalse(_target != same);
            Assert.AreEqual(_target.GetHashCode(), same.GetHashCode());
        }

        /// <summary>
        ///     Tests that two transactions that differ only by RequestId compare as different.
        /// </summary>
        [TestMethod]
        public void RequestIdInequalityTest()
        {
            _target = CreateTransaction();

            const string hostTransactionId2 = "987654321098765432";

            var different = new WatTransaction(
                DeviceId,
                DateTime,
                Amount,
                0,
                0,
                false,
                hostTransactionId2)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId
            };

            Assert.IsFalse(_target.Equals(different));
            Assert.IsFalse(different.Equals(_target));
            Assert.IsFalse(_target.Equals(different as object));
            Assert.IsFalse(different.Equals(_target as object));
            Assert.IsFalse(_target == different);
            Assert.IsFalse(different == _target);
            Assert.IsTrue(different != _target);
            Assert.IsTrue(_target != different);
            Assert.AreNotEqual(_target.GetHashCode(), different.GetHashCode());
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
            Assert.IsFalse(_target.Equals(null));

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

            const WatTransaction Null = null;

            Assert.IsTrue(_target != null);
            Assert.IsTrue(_target != Null);
            Assert.IsFalse(_target == null);
            Assert.IsFalse(_target == Null);
            Assert.IsFalse(_target.Equals(Null));
            Assert.IsFalse(_target.Equals(null));

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

        [TestMethod]
        public void RequestIdEmptyShouldReturnEgmInitiatedTest()
        {
            _target = CreateTransaction();
            _target.RequestId = string.Empty;

            Assert.AreEqual(_target.Direction, WatDirection.EgmInitiated);
        }

        [TestMethod]
        public void RequestIdNullShouldReturnEgmInitiatedTest()
        {
            _target = CreateTransaction();
            _target.RequestId = null;

            Assert.AreEqual(_target.Direction, WatDirection.EgmInitiated);
        }

        [TestMethod]
        public void RequestIdShouldReturnHostInitiatedTest()
        {
            _target = CreateTransaction();
            _target.RequestId = "0";

            Assert.AreEqual(_target.Direction, WatDirection.HostInitiated);
        }

        /// <summary>
        ///     Creates a wat off transaction with random data.
        /// </summary>
        /// <returns>The transaction.</returns>
        private WatTransaction CreateTransaction()
        {
            _target = new WatTransaction(
                DeviceId,
                DateTime,
                Amount,
                0,
                0,
                false,
                RequestId)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId
            };

            return _target;
        }
    }
}