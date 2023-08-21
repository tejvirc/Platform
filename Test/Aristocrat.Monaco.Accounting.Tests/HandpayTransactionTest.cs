namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Soap;
    using Contracts;
    using Contracts.Handpay;
    using Contracts.TransferOut;
    using Hardware.Contracts.Persistence;
    using Kernel.MarketConfig.Models.Accounting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     This is a test class for HandpayTransaction and is intended
    ///     to contain all HandpayTransactionTest Unit Tests
    /// </summary>
    [TestClass]
    public class HandpayTransactionTest
    {
        private const int ExpirationDate = 100;
        private const int DeviceId = 23;
        private const long TransactionId = 98784;
        private const long Amount = 45000;
        private const AccountType AccountTypeUsed = AccountType.Promo;
        private const long LogSequence = 41;
        private static readonly DateTime DateTime = DateTime.Now;
        private Mock<IPersistentStorageAccessor> _block;
        private HandpayTransaction _target;

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            _block = MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Strict);

            _target = new HandpayTransaction();
            ////_accessor = new DynamicPrivateObject(_target);
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
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
            Assert.AreEqual(_target.Name, "Handpay");

        }

            /// <summary>
            ///     A test for ReceivePersistence
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
                { "Amount", Amount },
                { "AssociatedTransactions", string.Empty },
                { "PromoAmount", 0L },
                { "NonCashAmount", 0L },
                { "WagerAmount", 123L },
                { "Printed", false },
                { "Barcode", "12345678" },
                { "RequestAcknowledged", false },
                { "HandpayType", HandpayType.CancelCredit },
                { "KeyOffType", KeyOffType.LocalCredit },
                { "KeyOffCashableAmount", Amount },
                { "KeyOffPromoAmount", 0L },
                { "KeyOffNonCashAmount", 0L },
                { "KeyOffDateTime", DateTime.UtcNow },
                { "HandpayState", HandpayState.Acknowledged },
                { "PrintTicket", true },
                { "BankTransactionId", Guid.NewGuid() },
                { "HostSequence", 0L },
                { "ReceiptSequence", 0 },
                { "TraceId", Guid.Empty },
                { "Read", false },
                { "Expiration", ExpirationDate },
                { "HostOnline", true },
                { "TicketDataBlob", null },
                { "Reason", TransferOutReason.CashOut }
            };
            Assert.IsTrue(_target.SetData(values));
        }

        /// <summary>
        ///     A test for ReceivePersistence with a leading zeros in barcode
        /// </summary>
        [TestMethod]
        public void ReceivePersistenceLeadingZerosBarcodeTest()
        {
            var values = new Dictionary<string, object>
            {
                { "DeviceId", DeviceId },
                { "LogSequence", LogSequence },
                { "TransactionDateTime", DateTime },
                { "TransactionId", TransactionId },
                { "Amount", Amount },
                { "AssociatedTransactions", string.Empty },
                { "PromoAmount", 0L },
                { "NonCashAmount", 0L },
                { "WagerAmount", 123L },
                { "Printed", false },
                { "Barcode", "000000000000000000" },
                { "RequestAcknowledged", false },
                { "HandpayType", HandpayType.CancelCredit },
                { "KeyOffType", KeyOffType.LocalCredit },
                { "KeyOffCashableAmount", Amount },
                { "KeyOffPromoAmount", 0L },
                { "KeyOffNonCashAmount", 0L },
                { "KeyOffDateTime", DateTime.UtcNow },
                { "HandpayState", HandpayState.Acknowledged },
                { "PrintTicket", true },
                { "BankTransactionId", Guid.NewGuid() },
                { "HostSequence", 0L },
                { "ReceiptSequence", 0 },
                { "TraceId", Guid.Empty },
                { "Read", false },
                { "Expiration", ExpirationDate },
                { "HostOnline", true },
                { "TicketDataBlob", null },
                { "Reason", TransferOutReason.CashOut }
            };

            Assert.IsTrue(_target.SetData(values));
        }

        /// <summary>
        ///     A test for ReceivePersistence with a zero length barcode
        /// </summary>
        [TestMethod]
        public void ReceivePersistenceNullBarcodeTest()
        {
            var values = new Dictionary<string, object>
            {
                { "DeviceId", DeviceId },
                { "LogSequence", LogSequence },
                { "TransactionDateTime", DateTime },
                { "TransactionId", TransactionId },
                { "Amount", Amount },
                { "AssociatedTransactions", string.Empty },
                { "PromoAmount", 0L },
                { "NonCashAmount", 0L },
                { "WagerAmount", 123L },
                { "Printed", false },
                { "Barcode", string.Empty },
                { "RequestAcknowledged", false },
                { "HandpayType", HandpayType.CancelCredit },
                { "KeyOffType", KeyOffType.LocalCredit },
                { "KeyOffCashableAmount", Amount },
                { "KeyOffPromoAmount", 0L },
                { "KeyOffNonCashAmount", 0L },
                { "KeyOffDateTime", DateTime.UtcNow },
                { "HandpayState", HandpayState.Acknowledged },
                { "PrintTicket", true },
                { "BankTransactionId", Guid.NewGuid() },
                { "HostSequence", 0L },
                { "ReceiptSequence", 0 },
                { "TraceId", Guid.Empty },
                { "Read", false },
                { "Expiration", ExpirationDate },
                { "HostOnline", true },
                { "TicketDataBlob", null },
                { "Reason", TransferOutReason.CashOut }
            };

            Assert.IsTrue(_target.SetData(values));
        }

        /// <summary>
        ///     A test for SetPersistence
        /// </summary>
        [TestMethod]
        public void SetPersistenceTest()
        {
            var actual = CreateTransaction();

            var element = 0;
            var transaction = new Mock<IPersistentStorageTransaction>(MockBehavior.Loose);
            _block.Setup(m => m.Count).Returns(1);
            _block.Setup(m => m.StartTransaction()).Returns(transaction.Object);

            transaction.SetupSet(m => m[element, "DeviceId"] = DeviceId);
            transaction.SetupSet(m => m[element, "LogSequence"] = LogSequence);
            transaction.SetupSet(m => m[element, "TransactionDateTime"] = DateTime);
            transaction.SetupSet(m => m[element, "TransactionId"] = TransactionId);
            transaction.SetupSet(m => m[element, "Amount"] = Amount);
            transaction.SetupSet(m => m[element, "TypeOfAccount"] = (byte)AccountTypeUsed);

            transaction.SetupSet(m => m[element, "Printed"] = false);
            transaction.SetupSet(m => m[element, "HandpayType"] = HandpayType.CancelCredit);
            transaction.SetupSet(m => m[element, "KeyOffType"] = KeyOffType.LocalCredit);
            transaction.SetupSet(m => m[element, "KeyOffAmount"] = Amount);
            transaction.SetupSet(m => m[element, "KeyOffDateTime"] = DateTime.UtcNow);
            transaction.SetupSet(m => m[element, "HandpayState"] = HandpayState.Acknowledged);
            transaction.SetupSet(m => m[element, "HostSequence"] = 0);
            transaction.SetupSet(m => m[element, "ReceiptSequence"] = 0);

            transaction.Setup(m => m.Dispose()).Verifiable();
            transaction.Setup(m => m.Commit()).Verifiable();

            actual.SetPersistence(_block.Object, 0);

            _block.Verify();
            transaction.Verify();
        }

        /// <summary>
        ///     A test for the parameterized constructor and the properties,
        ///     all of which are read-only.
        /// </summary>
        [TestMethod]
        public void ParameterizedConstructorTest()
        {
            _target = CreateTransaction();

            Assert.AreEqual(DeviceId, _target.DeviceId);
            Assert.AreEqual(DateTime, _target.TransactionDateTime);
            Assert.AreEqual(TransactionId, _target.TransactionId);
            Assert.AreEqual(Amount, _target.CashableAmount);
            Assert.AreEqual(LogSequence, _target.LogSequence);
            Assert.AreEqual("Handpay", _target.Name);
        }

        /// <summary>
        ///     A test for CanceledCreditsTransaction serialization
        /// </summary>
        [TestMethod]
        public void SerializationTest()
        {
            var original = CreateTransaction();

            var stream = new FileStream("CanceledCreditsTransaction.dat", FileMode.Create);
            var formatter = new SoapFormatter(
                null,
                new StreamingContext(StreamingContextStates.File));

            formatter.Serialize(stream, original);

            stream.Position = 0;

            var deserialized = (HandpayTransaction)formatter.Deserialize(stream);

            Assert.AreEqual(original.DeviceId, deserialized.DeviceId);
            Assert.AreEqual(original.TransactionDateTime, deserialized.TransactionDateTime);
            Assert.AreEqual(original.TransactionId, deserialized.TransactionId);
            Assert.AreEqual(original.CashableAmount, deserialized.CashableAmount);
            Assert.AreEqual(original.PromoAmount, deserialized.PromoAmount);
            Assert.AreEqual(original.NonCashAmount, deserialized.NonCashAmount);
            Assert.AreEqual(original.LogSequence, deserialized.LogSequence);
            Assert.AreEqual(original.Name, deserialized.Name);
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

            var copy = _target.Clone() as HandpayTransaction;

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

            var same = _target.Clone() as HandpayTransaction;

            Assert.IsTrue(_target.Equals(_target));
            Assert.IsTrue(same.Equals(_target));
            Assert.IsTrue(_target == same);
            Assert.IsTrue(same == _target);
            Assert.IsFalse(same != _target);
            Assert.IsFalse(_target != same);
            Assert.AreEqual(_target.GetHashCode(), same.GetHashCode());
        }

        /// <summary>
        ///     Tests that two different transactions are compared to different results
        /// </summary>
        [TestMethod]
        public void InequalityTest()
        {
            _target = CreateTransaction();

            var different = new HandpayTransaction(
                DeviceId,
                DateTime,
                Amount,
                0,
                0,
                100,
                HandpayType.CancelCredit,
                true,
                Guid.NewGuid())
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId + 1
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

            const HandpayTransaction Null = null;

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

        /// <summary>
        ///     A test for the parameterized constructor and the properties,
        ///     all of which are read-only.  In this test the barcode is empty.
        /// </summary>
        [TestMethod]
        public void EmptyBarcodeConstructorTest()
        {
            _target = new HandpayTransaction(
                DeviceId,
                DateTime,
                Amount,
                0,
                0,
                100,
                HandpayType.CancelCredit,
                true,
                Guid.NewGuid());

            Assert.AreEqual(DeviceId, _target.DeviceId);
            Assert.AreEqual(DateTime, _target.TransactionDateTime);
            Assert.AreEqual(0, _target.TransactionId);
            Assert.AreEqual(Amount, _target.CashableAmount);
            Assert.AreEqual(0, _target.LogSequence);
            Assert.AreEqual("Handpay", _target.Name);
        }

        /// <summary>
        ///     A test for the parameterized constructor and the properties,
        ///     all of which are read-only.  In this test the barcode is zero.
        /// </summary>
        [TestMethod]
        public void ZeroBarcodeConstructorTest()
        {
            _target = new HandpayTransaction(
                DeviceId,
                DateTime,
                Amount,
                0,
                0,
                100,
                HandpayType.CancelCredit,
                true,
                Guid.NewGuid());

            Assert.AreEqual(DeviceId, _target.DeviceId);
            Assert.AreEqual(DateTime, _target.TransactionDateTime);
            Assert.AreEqual(0, _target.TransactionId);
            Assert.AreEqual(Amount, _target.CashableAmount);
            Assert.AreEqual(0, _target.LogSequence);
            Assert.AreEqual("Handpay", _target.Name);
        }

        /// <summary>
        ///     A test for the IsCreditType() method
        /// </summary>
        [TestMethod]
        public void IsCreditTypeTest()
        {
            _target = new HandpayTransaction(
                DeviceId,
                DateTime,
                Amount,
                0,
                0,
                100,
                HandpayType.CancelCredit,
                true,
                Guid.NewGuid());

            _target.KeyOffType = KeyOffType.Cancelled;
            Assert.IsFalse(_target.IsCreditType());

            _target.KeyOffType = KeyOffType.LocalCredit;
            Assert.IsTrue(_target.IsCreditType());

            _target.KeyOffType = KeyOffType.LocalHandpay;
            Assert.IsFalse(_target.IsCreditType());

            _target.KeyOffType = KeyOffType.LocalVoucher;
            Assert.IsFalse(_target.IsCreditType());

            _target.KeyOffType = KeyOffType.LocalWat;
            Assert.IsFalse(_target.IsCreditType());

            _target.KeyOffType = KeyOffType.RemoteCredit;
            Assert.IsTrue(_target.IsCreditType());

            _target.KeyOffType = KeyOffType.RemoteHandpay;
            Assert.IsFalse(_target.IsCreditType());

            _target.KeyOffType = KeyOffType.RemoteVoucher;
            Assert.IsFalse(_target.IsCreditType());

            _target.KeyOffType = KeyOffType.RemoteWat;
            Assert.IsFalse(_target.IsCreditType());

            _target.KeyOffType = KeyOffType.Unknown;
            Assert.IsFalse(_target.IsCreditType());
        }

        /// <summary>
        ///     Creates a Handpay transaction with random data.
        /// </summary>
        /// <returns>The created transaction.</returns>
        private HandpayTransaction CreateTransaction()
        {
            return new HandpayTransaction(
                DeviceId,
                DateTime,
                Amount,
                0,
                0,
                100,
                HandpayType.CancelCredit,
                true,
                Guid.NewGuid())
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId,
                Expiration = ExpirationDate
            };
        }
    }
}