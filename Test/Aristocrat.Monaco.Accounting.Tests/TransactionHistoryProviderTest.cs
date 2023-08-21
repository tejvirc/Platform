namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     This is a test class for TransactionHistoryProviderTest and is intended
    ///     to contain all TransactionHistoryProviderTest Unit Tests
    /// </summary>
    [TestClass]
    public class TransactionHistoryProviderTest
    {
        private const PersistenceLevel Level = PersistenceLevel.Critical;

        private Mock<IPersistentStorageAccessor> _block;
        private Mock<IPersistentStorageManager> _persistentStorage;

        /// <summary>
        ///     Helper method to check basic Asserts.
        /// </summary>
        /// <param name="target">Test TransactionHistoryProvider.</param>
        /// <param name="transactionType">Type of transaction.</param>
        /// <param name="level">Type of storage media.</param>
        /// <param name="size">Maximum number of transactions in target.</param>
        /// <param name="persistable">Indicates if the transaction implements IPersistable.</param>
        public static void AssertHelper(
            TransactionHistoryProvider target,
            Type transactionType,
            PersistenceLevel level,
            int size,
            bool persistable)
        {
            Assert.AreEqual(transactionType, target.TransactionType);
            Assert.AreEqual(level, target.Level);
            Assert.AreEqual(size, target.MaxTransactions);
            Assert.AreEqual(persistable, target.Persistable);
        }

        /// <summary>
        ///     Method to setup objects for the test run.
        /// </summary>
        [TestInitialize]
        public void MyTestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _block = MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Strict);
            _block.Setup(m => m.Count).Returns(100);
            _persistentStorage = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);
            _persistentStorage.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(true);
            _persistentStorage.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_block.Object);
        }

        /// <summary>
        ///     Method to cleanup objects for the test run.
        /// </summary>
        [TestCleanup]
        public void MyTestCleanUp()
        {
            MoqServiceManager.RemoveInstance();
            try
            {
                AddinManager.Shutdown();
            }
            catch (InvalidOperationException)
            {
                // temporarily swallow exception
            }
        }

        [TestMethod]
        public void SaveTransactionWithPrintableTransactionTest()
        {
            var type = typeof(TestTransaction);
            var size = 15;
            var persistable = true;
            var printable = true;
            SetupReceivePersistence(size);
            _persistentStorage.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(false);
            _persistentStorage.Setup(m => m.CreateBlock(PersistenceLevel.Critical, It.IsAny<string>(), size))
                .Returns(_block.Object);

            var target = new TransactionHistoryProvider(type, Level, size, persistable, printable);

            var deviceId = 5;
            var transactionDateTime = DateTime.Today;
            var tester = new TestTransaction(deviceId, "Test", transactionDateTime);
            var element = 0;
            var transaction = new Mock<IPersistentStorageTransaction>(MockBehavior.Strict);
            _persistentStorage.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_block.Object);
            _block.Setup(m => m.Count).Returns(1);
            _block.Setup(m => m.StartTransaction()).Returns(transaction.Object);
            transaction.SetupSet(m => m[element, "DeviceId"] = deviceId);
            transaction.SetupSet(m => m[element, "LogSequence"] = (long)0);
            transaction.SetupSet(m => m[element, "TransactionDateTime"] = transactionDateTime);
            transaction.SetupSet(m => m[element, "TransactionId"] = (long)0);
            transaction.Setup(m => m.Dispose()).Verifiable();
            transaction.Setup(m => m.Commit()).Verifiable();

            target.SaveTransaction(tester);

            AssertHelper(target, type, Level, size, persistable);
            var transactions = new List<ITransaction>(target.RecallTransactions());
            Assert.AreEqual(1, transactions.Count);
            IsTransactionDataEqual(tester, transactions[0]);
            _block.Verify();
            transaction.Verify();
            _persistentStorage.Verify();
        }

        /// <summary>
        ///     A test for SaveTransaction where the transaction at the front is overwritten.
        /// </summary>
        [TestMethod]
        public void SaveTransactionOverwrittenTest()
        {
            var type = typeof(TestTransaction);
            var size = 1;
            var persistable = true;
            var printable = false;
            SetupReceivePersistence(size);

            var target = new TransactionHistoryProvider(type, Level, size, persistable, printable);

            var deviceId = 5;
            var transactionDateTime = DateTime.Today;
            var tester = new TestTransaction(deviceId, "Test", transactionDateTime);
            var transaction = new Mock<IPersistentStorageTransaction>(MockBehavior.Strict);
            _block.Setup(m => m.Count).Returns(1);
            _block.Setup(m => m.StartTransaction()).Returns(transaction.Object);
            transaction.SetupSet(m => m[0, "DeviceId"] = deviceId);
            transaction.SetupSet(m => m[0, "LogSequence"] = (long)0);
            transaction.SetupSet(m => m[0, "TransactionDateTime"] = transactionDateTime);
            transaction.SetupSet(m => m[0, "TransactionId"] = (long)0);
            transaction.Setup(m => m.Dispose()).Verifiable();
            transaction.Setup(m => m.Commit()).Verifiable();

            target.SaveTransaction(tester);

            var transactions = new List<ITransaction>(target.RecallTransactions());
            Assert.AreEqual(1, transactions.Count);
            IsTransactionDataEqual(tester, transactions[0]);
            Assert.IsFalse(ReferenceEquals(tester, transactions[0]));

            var transactionDateTime1 = transactionDateTime.AddDays(1);

            var tester1 = new TestTransaction(deviceId, "Test", transactionDateTime);
            transaction.SetupSet(m => m[0, "LogSequence"] = (long)2);
            transaction.SetupSet(m => m[0, "TransactionDateTime"] = transactionDateTime1);

            target.SaveTransaction(tester1);

            AssertHelper(target, type, Level, size, persistable);

            transactions = new List<ITransaction>(target.RecallTransactions());
            Assert.AreEqual(1, transactions.Count);
            IsTransactionDataEqual(tester1, transactions[0]);
            Assert.IsFalse(ReferenceEquals(tester1, transactions[0]));
        }

        /// <summary>
        ///     A test for RecallTransactions.
        /// </summary>
        [TestMethod]
        public void RecallTransactionsTest()
        {
            var type = typeof(TestTransaction);
            var size = 15;
            var persistable = true;
            var printable = false;
            SetupReceivePersistence(size);

            var target = new TransactionHistoryProvider(type, Level, size, persistable, printable);

            var deviceId = 5;
            var transactionDateTime = DateTime.Today;
            var tester = new TestTransaction(deviceId, "Test", transactionDateTime);

            transactionDateTime = transactionDateTime.AddDays(1);
            var tester1 = new TestTransaction(deviceId, "Test", transactionDateTime);

            var transactions = new List<ITransaction> { tester, tester1 };
            dynamic accessor = new DynamicPrivateObject(target);
            accessor._transactions = transactions;

            var recalledTransactions = new List<ITransaction>(target.RecallTransactions());
            Assert.AreEqual(2, recalledTransactions.Count);
            IsTransactionDataEqual(tester, recalledTransactions[0]);
            Assert.IsFalse(ReferenceEquals(tester, recalledTransactions[0]));
            IsTransactionDataEqual(tester1, recalledTransactions[1]);
            Assert.IsFalse(ReferenceEquals(tester1, recalledTransactions[1]));
        }

        [TestMethod]
        public void ConstructorWithExistingBlockTest()
        {
            var type = typeof(TestTransaction);
            var size = 25;
            var persistable = false;
            var printable = false;

            var target = new TransactionHistoryProvider(type, Level, size, persistable, printable);

            AssertHelper(target, type, Level, size, persistable);
        }

        [TestMethod]
        public void ConstructorNoExistingBlockTest()
        {
            var type = typeof(TestTransaction);
            var size = 25;
            var persistable = true;
            var printable = false;

            _persistentStorage.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(false);
            _persistentStorage.Setup(m => m.CreateBlock(PersistenceLevel.Critical, It.IsAny<string>(), size))
                .Returns(_block.Object);

            var target = new TransactionHistoryProvider(type, Level, size, persistable, printable);

            AssertHelper(target, type, Level, size, persistable);
        }

        [TestMethod]
        public void TransactionHistoryProviderToStringTest()
        {
            var type = typeof(TestTransaction);
            var size = 25;
            var persistable = false;
            var printable = false;

            var target = new TransactionHistoryProvider(type, Level, size, persistable, printable);

            var expected = "This TransactionHistoryProvider has the following values:\n" +
                           "TransactionType: Aristocrat.Monaco.Accounting.Tests.TestTransaction\n" +
                           "MaxTransactions: 25\n" +
                           "PersistenceLevel: Critical\n" +
                           "Persistable: False\nPrintable: False\nIt currently contains 0 transactions";
            Assert.AreEqual(expected, target.ToString());
        }

        [TestMethod]
        public void UpdateTransactionTest()
        {
            var type = typeof(TestTransaction);
            var size = 15;
            var persistable = true;
            var printable = true;
            SetupReceivePersistence(size);
            _persistentStorage.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(false);
            _persistentStorage.Setup(m => m.CreateBlock(PersistenceLevel.Critical, It.IsAny<string>(), size))
                .Returns(_block.Object);

            var target = new TransactionHistoryProvider(type, Level, size, persistable, printable);
            dynamic accessor = new DynamicPrivateObject(target);

            var deviceId = 5;
            var transactionDateTime = DateTime.Today;
            var tester = new TestTransaction(deviceId, "Test", transactionDateTime)
            {
                TransactionId = 1
            };

            var element = 0;
            var transaction = new Mock<IPersistentStorageTransaction>(MockBehavior.Strict);
            _persistentStorage.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_block.Object);
            _block.Setup(m => m.Count).Returns(1);
            _block.Setup(m => m.StartTransaction()).Returns(transaction.Object);
            transaction.SetupSet(m => m[element, "DeviceId"] = deviceId);
            transaction.SetupSet(m => m[element, "LogSequence"] = (long)0);
            transaction.SetupSet(m => m[element, "TransactionDateTime"] = transactionDateTime);
            transaction.SetupSet(m => m[element, "TransactionId"] = (long)1);
            transaction.Setup(m => m.Dispose()).Verifiable();
            transaction.Setup(m => m.Commit()).Verifiable();

            target.SaveTransaction(tester);
            target.UpdateTransaction(tester);

            AssertHelper(target, type, Level, size, persistable);
            var transactions = new List<ITransaction>(target.RecallTransactions());
            Assert.AreEqual(1, transactions.Count);
            IsTransactionDataEqual(tester, transactions[0]);
            _block.Verify();
            transaction.Verify();
            _persistentStorage.Verify();
        }

        /// <summary>
        ///     Sets up persistence mocks needed by the constructor
        /// </summary>
        /// <param name="size">The number of storage blocks needed</param>
        private void SetupReceivePersistence(int size)
        {
            var results = new Dictionary<int, Dictionary<string, object>>();

            for (var i = 0; i < size; i++)
            {
                var i2 = i;

                var record = new Dictionary<string, object>
                {
                    { "DeviceId", 1 },
                    { "LogSequence", (long)i2 },
                    { "TransactionDateTime", DateTime.MaxValue },
                    { "TransactionId", (long)i2 },
                    { "Amount", (long)i2 * 1000 },
                    { "TypeOfAccount", (byte)AccountType.Cashable }
                };

                results.Add(i2, record);
            }

            _block.Setup(m => m.GetAll()).Returns(results);
        }

        /// <summary>
        ///     Tests whether or not the transactional data of two ITransactions are equal.
        /// </summary>
        /// <param name="expected">The expected ITransaction and its data.</param>
        /// <param name="actual">The actual ITransaction and its data.</param>
        private static void IsTransactionDataEqual(ITransaction expected, ITransaction actual)
        {
            Assert.AreEqual(expected.DeviceId, actual.DeviceId);
            Assert.AreEqual(expected.LogSequence, actual.LogSequence);
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.TransactionDateTime, actual.TransactionDateTime);
            Assert.AreEqual(expected.TransactionId, actual.TransactionId);
        }
    }
}