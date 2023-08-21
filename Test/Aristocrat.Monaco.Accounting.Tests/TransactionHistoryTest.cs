namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Application.Contracts;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     This is a test class for TransactionHistoryTest and is intended
    ///     to contain all TransactionHistoryTest Unit Tests.
    /// </summary>
    [TestClass]
    public class TransactionHistoryTest
    {
        private dynamic _accessor;
        private Mock<IEventBus> _eventBus;
        private Mock<IIdProvider> _idProvider;
        private Mock<IPathMapper> _pathMapper;

        private Mock<IPersistentStorageManager> _persistentStorage;
        private Mock<IPropertiesManager> _propertiesManager;
        private TransactionHistory _target;

        /// <summary>
        ///     Method to setup objects for the test run.
        /// </summary>
        [TestInitialize]
        public void MyTestInitialize()
        {
            File.Delete("Monaco.Accounting.addin.xml");
            File.Delete("Arsitocrat.Monaco.Kernel.addin.xml");
            var directory = Directory.GetCurrentDirectory();
            AddinManager.Initialize(directory);

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _persistentStorage = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _pathMapper = MoqServiceManager.CreateAndAddService<IPathMapper>(MockBehavior.Strict);
            _pathMapper.Setup(m => m.GetDirectory(It.IsAny<string>())).Returns(new DirectoryInfo("."));
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _idProvider = MoqServiceManager.CreateAndAddService<IIdProvider>(MockBehavior.Strict);

            _propertiesManager.Setup(m => m.GetProperty("Mono.SelectedAddinConfigurationHashCode", null)).Returns(null);

            // setups for TransactionHistoryProvider
            var block = new Mock<IPersistentStorageAccessor>(MockBehavior.Strict);
            _persistentStorage.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(false);
            _persistentStorage.Setup(
                    m => m.CreateBlock(It.IsAny<PersistenceLevel>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(block.Object);

            _target = new TransactionHistory();
            _target.Initialize();
            _accessor = new DynamicPrivateObject(_target);
        }

        /// <summary>
        ///     Method to cleanup objects for the test run.
        /// </summary>
        [TestCleanup]
        public void MyTestCleanUp()
        {
            try
            {
                AddinManager.Shutdown();
            }
            catch (InvalidOperationException)
            {
                // temporarily swallow exception
            }
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            Assert.AreEqual(1, _target.ServiceTypes.Count);
            Assert.IsTrue(_target.ServiceTypes.Contains(typeof(ITransactionHistory)));
        }

        [TestMethod]
        public void NameTest()
        {
            Assert.IsFalse(string.IsNullOrEmpty(_target.Name));
        }


        [TestMethod]
        public void SaveTransactionTest()
        {
            _idProvider.Setup(m => m.GetNextTransactionId()).Returns(1L);
            _idProvider.Setup(m => m.GetNextLogSequence(It.IsAny<Type>())).Returns(2L);
            _eventBus.Setup(m => m.Publish(It.IsAny<TransactionSavedEvent>())).Verifiable();
            var provider = new TransactionHistoryProvider(
                typeof(VoucherInTransaction),
                PersistenceLevel.Transient,
                100,
                false,
                false);
            Dictionary<Type, TransactionHistoryProvider> providers = _accessor._transactionProviders;
            providers[typeof(VoucherInTransaction)] = provider;

            _target.AddTransaction(new VoucherInTransaction());
            _eventBus.Verify();
        }

        [TestMethod]
        public void GetMaxTransactionsTest()
        {
            var expected = 123;
            var provider = new TransactionHistoryProvider(
                typeof(VoucherInTransaction),
                PersistenceLevel.Transient,
                expected,
                false,
                false);
            Dictionary<Type, TransactionHistoryProvider> providers = _accessor._transactionProviders;
            providers[typeof(VoucherInTransaction)] = provider;

            var actual = _target.GetMaxTransactions<VoucherInTransaction>();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RecallTransactionsByTypeTest()
        {
            var provider = new TransactionHistoryProvider(
                typeof(VoucherInTransaction),
                PersistenceLevel.Transient,
                100,
                false,
                false);
            Dictionary<Type, TransactionHistoryProvider> providers = _accessor._transactionProviders;
            providers[typeof(VoucherInTransaction)] = provider;
            provider.SaveTransaction(new VoucherInTransaction());

            var actual = _target.RecallTransactions<VoucherInTransaction>();

            Assert.AreEqual(1, actual.Count);
            Assert.IsInstanceOfType(actual.ToList()[0], typeof(VoucherInTransaction));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void UpdateTransactionNoTransactionIdTest()
        {
            // should throw exception
            _accessor.UpdateTransaction(new VoucherOutTransaction());
        }

        [TestMethod]
        public void UpdateTransactionTest()
        {
            _idProvider.Setup(m => m.GetNextTransactionId()).Returns(1L);
            _idProvider.Setup(m => m.GetNextLogSequence(It.IsAny<Type>())).Returns(2L);
            _eventBus.Setup(m => m.Publish(It.IsAny<TransactionSavedEvent>())).Verifiable();
            var provider = new TransactionHistoryProvider(
                typeof(VoucherInTransaction),
                PersistenceLevel.Transient,
                100,
                false,
                false);
            Dictionary<Type, TransactionHistoryProvider> providers = _accessor._transactionProviders;
            providers[typeof(VoucherInTransaction)] = provider;

            var transaction = new VoucherInTransaction();

            _target.AddTransaction(transaction);

            _accessor.UpdateTransaction(transaction);
        }
    }
}