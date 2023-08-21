namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Soap;
    using Contracts;
    using Contracts.TransferOut;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     This is a test class for VoucherOutTransaction and is intended
    ///     to contain all VoucherOutTransaction Unit Tests
    /// </summary>
    [TestClass]
    public class VoucherOutTransactionTest
    {
        /// <summary>
        ///     The file name to use for unit test logging
        /// </summary>
        private const string LogFileName = "logfile.log";

        /// <summary>
        ///     Create a logger for use in this class
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     The storage location for anything set in the HardMeterService's persistent
        ///     storage.  Indexer 'set' calls to the mocked IPersistentStorageAccessor are
        ///     mocked to be stored here.
        /// </summary>
        private readonly Dictionary<string, object> _persistedData = new Dictionary<string, object>();

        /// <summary>
        ///     A fake implementation of IPersistentStorageManager
        /// </summary>
        private Mock<IPersistentStorageManager> _persistentStorage;

        /// <summary>
        ///     The mocked service manager.
        /// </summary>
        private Mock<IServiceManager> _serviceManager;

        /// <summary>
        ///     A fake implementation of IPersistentStorageAccessor.
        /// </summary>
        private Mock<IPersistentStorageAccessor> _storageAccessor;

        /// <summary>
        ///     A fake implementation of IPersistentStorageTransaction.
        /// </summary>
        private Mock<IPersistentStorageTransaction> _storageTransaction;

        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            _serviceManager = MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            _storageTransaction =
                MoqServiceManager.CreateAndAddService<IPersistentStorageTransaction>(MockBehavior.Strict);
            _storageTransaction.Setup(mock => mock.Dispose());

            _storageAccessor = MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Strict);
            _storageAccessor.Setup(mock => mock[It.IsAny<string>()]).Returns((string s) => _persistedData[s]);
            _storageAccessor.Setup(mock => mock[It.IsAny<int>(), It.IsAny<string>()])
                .Returns((int i, string s) => _persistedData[s]);
            _storageAccessor.Setup(mock => mock.StartUpdate(It.IsAny<bool>())).Returns(true);
            _storageAccessor.Setup(mock => mock.StartTransaction()).Returns(_storageTransaction.Object);
            _storageAccessor.Setup(mock => mock.Count).Returns(2);
            _storageAccessor.Setup(mock => mock.Commit());
            _storageAccessor.SetupSet(mock => mock[It.IsAny<string>()] = It.IsAny<object>())
                .Callback((string s, object o) => _persistedData[s] = o);

            _persistentStorage = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);
            _persistentStorage.Setup(mock => mock.BlockExists(It.IsAny<string>())).Returns(true);
            _persistentStorage.Setup(
                    mock => mock.CreateBlock(It.IsAny<PersistenceLevel>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(_storageAccessor.Object);
            _persistentStorage.Setup(
                mock => mock.CreateDynamicBlock(
                    It.IsAny<PersistenceLevel>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<BlockFormat>())).Returns(_storageAccessor.Object);
            _persistentStorage.Setup(mock => mock.GetBlock(It.IsAny<string>())).Returns(_storageAccessor.Object);
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
            var target = new VoucherOutTransaction();

            Assert.AreEqual(0, target.DeviceId);
            Assert.AreEqual(DateTime.MinValue, target.TransactionDateTime);
            Assert.AreEqual(0, target.TransactionId);
            Assert.AreEqual(0, target.Amount);
            Assert.AreEqual(AccountType.Cashable, target.TypeOfAccount);
            Assert.AreEqual(0, target.LogSequence);
            Assert.AreEqual(null, target.Barcode);
            Assert.AreEqual("Voucher Out", target.Name);
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
            const AccountType AccountType = AccountType.Promo;
            const long LogSequence = 29;
            const string Barcode = "123456789123456789";
            const int Expiration = 10;
            const string ManualVerification = "123456789123456789";

            var target = new VoucherOutTransaction(
                DeviceId,
                dateTime,
                Amount,
                AccountType,
                Barcode,
                Expiration,
                ManualVerification);

            target.LogSequence = LogSequence;
            target.TransactionId = TransactionId;

            Assert.AreEqual(DeviceId, target.DeviceId);
            Assert.AreEqual(dateTime, target.TransactionDateTime);
            Assert.AreEqual(TransactionId, target.TransactionId);
            Assert.AreEqual(Amount, target.Amount);
            Assert.AreEqual(AccountType, target.TypeOfAccount);
            Assert.AreEqual(LogSequence, target.LogSequence);
            Assert.AreEqual(Barcode, target.Barcode);
            Assert.AreEqual(Expiration, target.Expiration);
            Assert.AreEqual("Voucher Out", target.Name);
        }

        /// <summary>
        ///     A test for SetPersistence()
        /// </summary>
        [TestMethod]
        public void SetPersistenceTest()
        {
            // Create a transaction with arbitrary data
            const int DeviceId = 9;
            var dateTime = DateTime.Now;
            const long TransactionId = 356;
            const long Amount = 64477;
            const AccountType AccountType = AccountType.Cashable;
            const long LogSequence = 36;
            const string Barcode = "000111222";
            const int Expiration = 10;
            const string ManualVerification = "000111222";
            var bankTransactionId = Guid.NewGuid();
            const bool HostAcknowledged = false;
            const TransferOutReason Reason = TransferOutReason.CashOut;
            const long HostSequence = 0;
            const bool VoucherPrinted = false;
            const int VoucherSequence = 1;
            const int ReferenceId = 0;
            const bool HostOnline = true;

            _storageTransaction.SetupSet(mock => mock[0, "Expiration"] = Expiration)
                .Callback((int i, string s, object o) => _persistedData[s] = o);
            _storageTransaction.SetupSet(mock => mock[0, "HostAcknowledged"] = HostAcknowledged)
                .Callback((int i, string s, object o) => _persistedData[s] = o);
            _storageTransaction.SetupSet(mock => mock[0, "HostSequence"] = HostSequence)
                .Callback((int i, string s, object o) => _persistedData[s] = o);
            _storageTransaction.SetupSet(mock => mock[0, "VoucherPrinted"] = VoucherPrinted)
                .Callback((int i, string s, object o) => _persistedData[s] = o);
            _storageTransaction.SetupSet(mock => mock[0, "VoucherSequence"] = VoucherSequence)
                .Callback((int i, string s, object o) => _persistedData[s] = o);
            _storageTransaction.SetupSet(mock => mock[0, "BarcodeLength"] = Barcode.Length)
                .Callback((int i, string s, object o) => _persistedData[s] = o);
            _storageTransaction.SetupSet(
                    mock => mock[0, "Barcode"] = Convert.ToInt64(Barcode, CultureInfo.InvariantCulture))
                .Callback((int i, string s, object o) => _persistedData[s] = o);
            _storageTransaction.SetupSet(mock => mock[0, "DeviceId"] = DeviceId)
                .Callback((int i, string s, object o) => _persistedData[s] = o);
            _storageTransaction.SetupSet(mock => mock[0, "LogSequence"] = LogSequence)
                .Callback((int i, string s, object o) => _persistedData[s] = o);
            _storageTransaction.SetupSet(mock => mock[0, "TransactionDateTime"] = dateTime)
                .Callback((int i, string s, object o) => _persistedData[s] = o);
            _storageTransaction.SetupSet(mock => mock[0, "TransactionId"] = TransactionId)
                .Callback((int i, string s, object o) => _persistedData[s] = o);
            _storageTransaction.SetupSet(mock => mock[0, "Amount"] = Amount)
                .Callback((int i, string s, object o) => _persistedData[s] = o);
            _storageTransaction.SetupSet(mock => mock[0, "TypeOfAccount"] = (byte)AccountType)
                .Callback((int i, string s, object o) => _persistedData[s] = o);
            _storageTransaction.SetupSet(mock => mock[0, "ManualVerification"] = ManualVerification)
                .Callback((int i, string s, object o) => _persistedData[s] = o);
            _storageTransaction.SetupSet(mock => mock[0, "BankTransactionId"] = bankTransactionId)
                .Callback((int i, string s, object o) => _persistedData[s] = o);
            _storageTransaction.SetupSet(mock => mock[0, "ReferenceId"] = ReferenceId)
                .Callback((int i, string s, object o) => _persistedData[s] = o);
            _storageTransaction.SetupSet(mock => mock[0, "AssociatedTransactions"] = "[]")
                .Callback((int i, string s, object o) => _persistedData[s] = o);
            _storageTransaction.SetupSet(mock => mock[0, "TraceId"] = Guid.Empty)
                .Callback((int i, string s, object o) => _persistedData[s] = o);
            _storageTransaction.SetupSet(mock => mock[0, "Reason"] = (int)Reason)
                .Callback((int i, string s, object o) => _persistedData[s] = o);
            _storageTransaction.SetupSet(mock => mock[0, "HostOnline"] = (bool)HostOnline)
               .Callback((int i, string s, object o) => _persistedData[s] = o);
            _storageTransaction.SetupSet(mock => mock[0, "TicketDataBlob"] = "null")
                .Callback((int i, string s, object o) => _persistedData[s] = o);
            _storageTransaction.SetupSet(mock => mock[0, "LogDisplayType"] = string.Empty)
                .Callback((int i, string s, object o) => _persistedData[s] = o);
            _storageTransaction.Setup(mock => mock.Commit());

            var block = _storageAccessor.Object;

            // Create a transaction and get its actual byte array
            var target = new VoucherOutTransaction(
                DeviceId,
                dateTime,
                Amount,
                AccountType,
                Barcode,
                Expiration,
                ManualVerification);

            target.LogSequence = LogSequence;
            target.TransactionId = TransactionId;
            target.HostAcknowledged = false;
            target.VoucherSequence = VoucherSequence;
            target.BankTransactionId = bankTransactionId;
            target.HostOnline = HostOnline;

            target.SetPersistence(block, 0);

            // Make sure no data was modified
            Assert.AreEqual(111222, (long)block["Barcode"]);
            Assert.AreEqual(9, (int)block["BarcodeLength"]);
        }

        /// <summary>
        ///     A test for ReceivePersistence()
        /// </summary>
        [TestMethod]
        public void ReceivePersistenceTest()
        {
            // Create arbitrary transaction data
            const int DeviceId = 11;
            var dateTime = DateTime.Now;
            const long TransactionId = (long)1 + int.MaxValue;
            const long Amount = 199;
            const AccountType AccountType = AccountType.Promo;
            const long LogSequence = 2;
            const string Barcode = "000123456789123456";

            var values = new Dictionary<string, object>
            {
                { "DeviceId", DeviceId },
                { "LogSequence", LogSequence },
                { "TransactionDateTime", dateTime },
                { "TransactionId", TransactionId },
                { "Amount", Amount },
                { "TypeOfAccount", (byte)AccountType },
                { "Barcode", 123456789123456 },
                { "BarcodeLength", 18 },
                { "VoucherSequence", 1 },
                { "Expiration", 10 },
                { "HostAcknowledged", false },
                { "HostSequence", 0L },
                { "VoucherPrinted", false },
                { "ManualVerification", null },
                { "BankTransactionId", Guid.NewGuid() },
                { "AssociatedTransactions", "[]" },
                { "TraceId", Guid.Empty },
                { "PoolId", 0 },
                { "Reason", (int)TransferOutReason.CashOut },
                { "HostOnline", true},
                { "TicketDataBlob", null },
                { "LogDisplayType", string.Empty }
            };

            var target = new VoucherOutTransaction();
            target.SetData(values);

            Assert.AreEqual(Barcode, target.Barcode);
            Assert.AreEqual("Voucher Out", target.Name);
        }

        /// <summary>
        ///     A test for ReceivePersistence() with null barcode.
        /// </summary>
        [TestMethod]
        public void ReceivePersistenceBarcodeNullTest()
        {
            // Create arbitrary transaction data
            const int DeviceId = 11;
            var dateTime = DateTime.Now;
            const long TransactionId = (long)1 + int.MaxValue;
            const long Amount = 199;
            const AccountType AccountType = AccountType.Promo;
            const long LogSequence = 2;

            var values = new Dictionary<string, object>
            {
                { "DeviceId", DeviceId },
                { "LogSequence", LogSequence },
                { "TransactionDateTime", dateTime },
                { "TransactionId", TransactionId },
                { "Amount", Amount },
                { "TypeOfAccount", (byte)AccountType },
                { "Barcode", (long)0 },
                { "BarcodeLength", 18 },
                { "VoucherSequence", 1 },
                { "Expiration", 10 },
                { "HostAcknowledged", false },
                { "HostSequence", 0L },
                { "VoucherPrinted", false },
                { "ManualVerification", null },
                { "BankTransactionId", Guid.NewGuid() },
                { "AssociatedTransactions", "[]" },
                { "TraceId", Guid.Empty },
                { "PoolId", 0 },
                { "Reason", (int)TransferOutReason.CashOut },
                { "HostOnline", true },
                { "TicketDataBlob", null },
                { "LogDisplayType", string.Empty }
            };

            var target = new VoucherOutTransaction();
            target.SetData(values);
        }

        /// <summary>
        ///     A test for VoucherOutTransaction serialization
        /// </summary>
        [TestMethod]
        public void SerializationTest()
        {
            const long LogSequence = 585673566;
            const long TransactionId = 98;

            // Create a transaction with random data
            var original = new VoucherOutTransaction(
                1,
                DateTime.Now,
                35,
                AccountType.NonCash,
                "111222333444555666",
                10,
                "111222333444555666");

            original.LogSequence = LogSequence;
            original.TransactionId = TransactionId;

            var stream = new FileStream("VoucherOutTransaction.dat", FileMode.Create);
            var formatter = new SoapFormatter(
                null,
                new StreamingContext(StreamingContextStates.File));

            formatter.Serialize(stream, original);

            stream.Position = 0;

            var deserialized = (VoucherOutTransaction)formatter.Deserialize(stream);

            Assert.AreEqual(original.DeviceId, deserialized.DeviceId);
            Assert.AreEqual(original.TransactionDateTime, deserialized.TransactionDateTime);
            Assert.AreEqual(original.TransactionId, deserialized.TransactionId);
            Assert.AreEqual(original.Amount, deserialized.Amount);
            Assert.AreEqual(original.TypeOfAccount, deserialized.TypeOfAccount);
            Assert.AreEqual(original.LogSequence, deserialized.LogSequence);
            Assert.AreEqual(original.Barcode, deserialized.Barcode);
            Assert.AreEqual(original.Expiration, deserialized.Expiration);
            Assert.AreEqual(original.Name, deserialized.Name);
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
            const AccountType AccountType = AccountType.Promo;
            const long LogSequence = 41;
            const string Barcode = "999888777666555444";
            const int Expiration = 10;
            const string ManualVerification = "999888777666555444";

            var target = new VoucherOutTransaction(
                DeviceId,
                dateTime,
                Amount,
                AccountType,
                Barcode,
                Expiration,
                ManualVerification)
            {
                LogSequence = LogSequence,
                TransactionId = TransactionId,
                HostAcknowledged = false,
                VoucherPrinted = false
            };

            Assert.IsFalse(string.IsNullOrEmpty(target.ToString()));
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
            const AccountType AccountType = AccountType.Promo;
            const long LogSequence = 29;
            const string Barcode = "999888777666555444";
            const int Expiration = 10;
            const string ManualVerification = "999888777666555444";

            var target = new VoucherOutTransaction(
                DeviceId,
                dateTime,
                Amount,
                AccountType,
                Barcode,
                Expiration,
                ManualVerification);

            target.TransactionId = TransactionId;
            target.LogSequence = LogSequence;

            var copy = target.Clone() as VoucherOutTransaction;

            Assert.IsNotNull(copy);
            Assert.AreEqual(copy.Amount, Amount);
            Assert.AreEqual(copy.DeviceId, DeviceId);
            Assert.AreEqual(copy.Barcode, Barcode);
            Assert.AreEqual(copy.Expiration, Expiration);
            Assert.AreEqual(copy.TransactionDateTime, dateTime);
            Assert.AreEqual(copy.TransactionId, TransactionId);
            Assert.AreEqual(copy.LogSequence, LogSequence);
        }

        /// <summary>
        ///     Tests for equality against itself.
        /// </summary>
        [TestMethod]
        public void SelfEqualityTest()
        {
            Logger.InfoFormat("{0}() testing...", TestContext.TestName);

            // Create a transaction with arbitrary data
            const int DeviceId = 11;
            var dateTime = DateTime.Now;
            const long TransactionId = (long)1 + int.MaxValue;
            const long Amount = 199;
            const AccountType AccountType = AccountType.Promo;
            const long LogSequence = 29;
            const string Barcode = "999888777666555444";
            const int Expiration = 10;
            const string ManualVerification = "999888777666555444";

            var target = new VoucherOutTransaction(
                DeviceId,
                dateTime,
                Amount,
                AccountType,
                Barcode,
                Expiration,
                ManualVerification);

            target.TransactionId = TransactionId;
            target.LogSequence = LogSequence;

            var same = target.Clone() as VoucherOutTransaction;

            Assert.IsTrue(target.Equals(target));
            Assert.IsTrue(same.Equals(target));
            Assert.IsTrue(target == same);
            Assert.IsTrue(same == target);
            Assert.IsFalse(same != target);
            Assert.IsFalse(target != same);
            Assert.AreEqual(target.GetHashCode(), same.GetHashCode());
        }

        /// <summary>
        ///     Tests that two different transactions are compared to different results
        /// </summary>
        [TestMethod]
        public void InequalityTest()
        {
            // Create a transaction with arbitrary data
            const int DeviceId = 11;
            var dateTime = DateTime.Now;
            const long TransactionId = (long)1 + int.MaxValue;
            const long Amount = 199;
            const AccountType AccountType = AccountType.Promo;
            const long LogSequence = 29;
            const string Barcode = "999888777666555444";
            const string Barcode2 = "12345678901234567";
            const int Expiration = 10;
            const string ManualVerification = "999888777666555444";

            var target = new VoucherOutTransaction(
                DeviceId,
                dateTime,
                Amount,
                AccountType,
                Barcode,
                Expiration,
                ManualVerification);

            target.TransactionId = TransactionId;
            target.LogSequence = LogSequence;

            var different = new VoucherOutTransaction(
                DeviceId,
                dateTime,
                Amount,
                AccountType,
                Barcode2,
                Expiration,
                ManualVerification);

            different.LogSequence = LogSequence;
            different.TransactionId = TransactionId;

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
            const AccountType AccountType = AccountType.Promo;
            const long LogSequence = 29;
            const string Barcode = "999888777666555444";
            const int Expiration = 10;
            const string ManualVerification = "999888777666555444";

            var target = new VoucherOutTransaction(
                DeviceId,
                dateTime,
                Amount,
                AccountType,
                Barcode,
                Expiration,
                ManualVerification);

            target.TransactionId = TransactionId;
            target.LogSequence = LogSequence;

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
            const AccountType AccountType = AccountType.Promo;
            const long LogSequence = 29;
            const string Barcode = "999888777666555444";
            const int Expiration = 10;
            const string ManualVerification = "999888777666555444";

            var target = new VoucherOutTransaction(
                DeviceId,
                dateTime,
                Amount,
                AccountType,
                Barcode,
                Expiration,
                ManualVerification);

            target.TransactionId = TransactionId;
            target.LogSequence = LogSequence;

            const VoucherOutTransaction NullTransaction = null;

            Assert.IsTrue(target != null);
            Assert.IsTrue(target != NullTransaction);
            Assert.IsFalse(target == null);
            Assert.IsFalse(target == NullTransaction);
            Assert.IsFalse(target.Equals(NullTransaction));
            Assert.IsFalse(target.Equals(null));

            Assert.IsTrue(NullTransaction == null);
            Assert.IsFalse(NullTransaction != null);

            // in this case order may matter for the overrides
            Assert.IsTrue(null != target);
            Assert.IsTrue(NullTransaction != target);
            Assert.IsFalse(null == target);
            Assert.IsFalse(NullTransaction == target);

            Assert.IsTrue(null == NullTransaction);
            Assert.IsFalse(null != NullTransaction);
        }
    }
}