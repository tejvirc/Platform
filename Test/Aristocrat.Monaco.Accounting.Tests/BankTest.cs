namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Application.Contracts;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     This is a test class for BankTest and is intended
    ///     to contain all BankTest Unit Tests.
    /// </summary>
    [TestClass]
    public class BankTest
    {
        private const long MaxCreditLimit = 10000;
        private readonly char[] _currencyId = { 'U', 'S', 'D' };
        private dynamic _accessor;
        private Mock<IEventBus> _eventBus;
        private Mock<IPersistentStorageManager> _persistentStorage;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IPersistentStorageAccessor> _storageAccessor;
        private Bank _target;
        private Mock<IPersistentStorageTransaction> _transaction;
        private Mock<ITransactionCoordinator> _transactionCoordinator;
        private Mock<INoteAcceptorMonitor> _noteAcceptorMonitor;
        private Guid _transactionId;

        /// <summary>
        ///     Method to setup objects for the test run.
        /// </summary>
        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _storageAccessor = new Mock<IPersistentStorageAccessor>();
            _transaction = new Mock<IPersistentStorageTransaction>();

            _storageAccessor.Setup(m => m.StartTransaction()).Returns(_transaction.Object);

            _persistentStorage = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);
            _persistentStorage.Setup(m => m.BlockExists(typeof(Bank).ToString())).Returns(true);

            _persistentStorage.Setup(m => m.GetBlock(typeof(Bank).ToString()))
                .Returns(_storageAccessor.Object);

            _storageAccessor.SetupGet(m => m["Limit"]).Returns(MaxCreditLimit);
            _storageAccessor.SetupGet(m => m["CurrencyId"]).Returns(Encoding.ASCII.GetBytes(_currencyId));

            foreach (var item in Enum.GetNames(typeof(AccountType)))
            {
                _storageAccessor.SetupGet(m => m[item]).Returns((long)0);
            }

            _transactionCoordinator =
                MoqServiceManager.CreateAndAddService<ITransactionCoordinator>(MockBehavior.Strict);
            _transactionId = Guid.NewGuid();
            _transactionCoordinator.Setup(m => m.IsTransactionActive).Returns(true);
            _transactionCoordinator.Setup(m => m.VerifyCurrentTransaction(_transactionId)).Returns(true);
            _transactionCoordinator.Setup(m => m.VerifyCurrentTransaction(Guid.Empty)).Returns(false);
            _transactionCoordinator.Setup(m => m.AbandonTransactions(It.IsAny<Guid>()));

            _noteAcceptorMonitor = MoqServiceManager.CreateAndAddService<INoteAcceptorMonitor>(MockBehavior.Default);

            _target = new Bank();
            _accessor = new DynamicPrivateObject(_target);

            _target.Initialize();
        }

        /// <summary>
        ///     Cleans up after each test
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        /// <summary>
        ///     A test for ServiceType.
        /// </summary>
        [TestMethod]
        public void ServiceTypeTest()
        {
            Assert.AreEqual(1, _target.ServiceTypes.Count);
            Assert.AreEqual(typeof(IBank), _target.ServiceTypes.First());
        }

        /// <summary>
        ///     A test for Name.
        /// </summary>
        [TestMethod]
        public void NameTest()
        {
            Assert.AreEqual("Bank", _target.Name);
        }

        /// <summary>
        ///     A test for Limit.
        /// </summary>
        [TestMethod]
        public void LimitTest()
        {
            long expected = 8007;
            _propertiesManager.Setup(m => m.GetProperty(AccountingConstants.MaxCreditMeter, long.MaxValue)).Returns(expected);

            Assert.AreEqual(expected, _target.Limit);
        }

        /// <summary>
        ///     A test to make sure that the bank can read from pre-existing
        ///     persistent storage.
        /// </summary>
        [TestMethod]
        public void CreateBankWhenPersistentStorageIsAvailableTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<BankBalanceChangedEvent>())).Verifiable();

            _target.Deposit(AccountType.Cashable, 1000, _transactionId);
            _target.Withdraw(AccountType.Cashable, 500, _transactionId);

            Assert.AreEqual(500, _target.QueryBalance());

            _storageAccessor.SetupGet(m => m[AccountType.Cashable.ToString()]).Returns((long)500);
            _target = new Bank();
            _accessor = new DynamicPrivateObject(_target);

            Assert.AreEqual(500, _target.QueryBalance());

            _eventBus.Verify();
        }

        /// <summary>
        ///     A test to make sure that the bank's constructor can create a block for persistence.
        /// </summary>
        [TestMethod]
        public void CreateBankWhenPersistentStorageIsUnavailableTest()
        {
            _persistentStorage.Setup(m => m.BlockExists(typeof(Bank).ToString())).Returns(false);
            _persistentStorage.Setup(m => m.CreateBlock(PersistenceLevel.Critical, typeof(Bank).ToString(), 1))
                .Returns(_storageAccessor.Object)
                .Verifiable();

            var anotherTestBank = new Bank();

            Assert.IsNotNull(anotherTestBank);
            _propertiesManager.Verify();
            _persistentStorage.Verify();
        }

        [TestMethod]
        public void DepositAndWithdrawTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<BankBalanceChangedEvent>())).Verifiable();

            _target.Deposit(AccountType.Cashable, 1000000L, _transactionId);
            _target.Withdraw(AccountType.Cashable, 500000L, _transactionId);

            Assert.AreEqual(500000L, _target.QueryBalance());

            _eventBus.Verify();
        }

        /// <summary>
        ///     A test for Withdraw where the amount is larger than the account balance.
        /// </summary>
        [ExpectedException(typeof(BankException))]
        [TestMethod]
        public void WithdrawOverdrawTest()
        {
            _target.Withdraw(AccountType.Cashable, 1000, _transactionId);
        }

        /// <summary>
        ///     A test for Withdraw where the transaction Id doesn't match.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(BankException))]
        public void WithdrawIncorrectIdTest()
        {
            _target.Withdraw(AccountType.Cashable, 1000, Guid.Empty);
        }

        /// <summary>
        ///     A test for Withdraw where the AccountType is not found in the Bank.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(BankException))]
        public void WithdrawAccountMissingTest()
        {
            _accessor._accounts.Remove(AccountType.Cashable);
            _target.Withdraw(AccountType.Cashable, 1000, _transactionId);
        }

        /// <summary>
        ///     A test for Save.
        /// </summary>
        [TestMethod]
        public void SaveTest()
        {
            const long oldBalance = 1234;
            const long newBalance = 4567;

            _eventBus.Setup(m => m.Publish(It.IsAny<BankBalanceChangedEvent>())).Verifiable();
            _transaction.Setup(m => m.Commit()).Verifiable();

            foreach (var item in Enum.GetNames(typeof(AccountType)))
            {
                _storageAccessor.SetupSet(m => m[item] = (long)0);
            }

            _persistentStorage.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_storageAccessor.Object);

            _accessor.Save(oldBalance, newBalance, Guid.Empty);

            // Verify the correctness.
            _storageAccessor.Verify();
            _transaction.Verify();
            _eventBus.Verify();
        }

        /// <summary>
        ///     A test for QueryBalance.
        /// </summary>
        [TestMethod]
        public void QueryBalanceTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<BankBalanceChangedEvent>())).Verifiable();

            _target.Deposit(AccountType.Cashable, 1000, _transactionId);
            _target.Deposit(AccountType.NonCash, 3000, _transactionId);
            _target.Deposit(AccountType.Promo, 1076, _transactionId);

            Assert.AreEqual(1000 + 3000 + 1076, _target.QueryBalance());

            _eventBus.Verify();
        }

        /// <summary>
        ///     A test for QueryBalance.
        /// </summary>
        [TestMethod]
        public void QueryAccountBalanceTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<BankBalanceChangedEvent>())).Verifiable();

            _target.Deposit(AccountType.Cashable, 1050, _transactionId);

            Assert.AreEqual(1050, _target.QueryBalance(AccountType.Cashable));
            Assert.AreEqual(0, _target.QueryBalance(AccountType.NonCash));
            Assert.AreEqual(0, _target.QueryBalance(AccountType.Promo));

            _eventBus.Verify();
        }

        /// <summary>
        ///     A test for QueryBalance where the AccountType doesn't exist.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(BankException))]
        public void QueryAccountBalanceMissingTest()
        {
            _accessor._accounts.Remove(AccountType.Cashable);

            _target.QueryBalance(AccountType.Cashable);
        }

        /// <summary>
        ///     A test for Deposit.
        /// </summary>
        [TestMethod]
        public void DepositTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<BankBalanceChangedEvent>())).Verifiable();

            var depositAmount = 1000;

            _target.Deposit(AccountType.Cashable, depositAmount, _transactionId);
            Assert.AreEqual(depositAmount, _target.QueryBalance(AccountType.Cashable));

            _eventBus.Verify();
        }

        /// <summary>
        ///     A test for Deposit where the transaction Id doesn't match.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(BankException))]
        public void DepositIncorrectIdTest()
        {
            _target.Deposit(AccountType.Cashable, 1000, Guid.Empty);
        }

        /// <summary>
        ///     A test for Deposit where the AccountType is not found in the Bank.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(BankException))]
        public void DepositAccountMissingTest()
        {
            _accessor._accounts.Remove(AccountType.Cashable);
            _target.Deposit(AccountType.Cashable, 1000, _transactionId);
        }

        /// <summary>
        ///     A test for CheckWithdraw.
        /// </summary>
        [TestMethod]
        public void CheckWithdrawTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<BankBalanceChangedEvent>())).Verifiable();

            var depositAmount = 1000;

            _target.Deposit(AccountType.Cashable, depositAmount, _transactionId);
            Assert.IsTrue(_target.CheckWithdraw(AccountType.Cashable, 500, _transactionId));
            Assert.AreEqual(depositAmount, _target.QueryBalance(AccountType.Cashable));

            _eventBus.Verify();
        }

        /// <summary>
        ///     A test for CheckWithdraw where the transaction Id doesn't match.
        /// </summary>
        [TestMethod]
        public void CheckWithdrawWithEmptyTransactiondIdTest()
        {
            Assert.IsFalse(_target.CheckWithdraw(AccountType.Cashable, 500, Guid.Empty));
        }

        /// <summary>
        ///     A test for CheckWithdraw where the transaction Id doesn't match the current transaction Id.
        /// </summary>
        [TestMethod]
        public void CheckWithdrawWithCurrentTransactionNotActiveTest()
        {
            _transactionCoordinator.Setup(m => m.IsTransactionActive).Returns(false);
            Assert.IsFalse(_target.CheckWithdraw(AccountType.Cashable, 500, _transactionId));
        }

        /// <summary>
        ///     A test for CheckWithdraw where the transaction Id doesn't match.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(BankException))]
        public void CheckWithdrawIncorrectIdTest()
        {
            _transactionCoordinator.Setup(m => m.VerifyCurrentTransaction(_transactionId)).Returns(false);
            _target.CheckWithdraw(AccountType.Cashable, 500, _transactionId);
        }

        /// <summary>
        ///     A test for CheckWithdraw where the AccountType is not found in the Bank.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(BankException))]
        public void CheckWithdrawAccountMissingTest()
        {
            _accessor._accounts.Remove(AccountType.Cashable);
            _target.CheckWithdraw(AccountType.Cashable, 1000, _transactionId);
        }

        /// <summary>
        ///     A test for CheckWithdraw where the Bank cannot withdraw.
        /// </summary>
        [TestMethod]
        public void CheckWithdrawAccountEmptyTest()
        {
            Assert.IsFalse(_target.CheckWithdraw(AccountType.Cashable, 1000, _transactionId));
        }

        /// <summary>
        ///     A test for CheckDeposit.
        /// </summary>
        [TestMethod]
        public void CheckDepositTest()
        {
            Assert.IsTrue(_target.CheckDeposit(AccountType.Cashable, 1000, _transactionId));
        }

        /// <summary>
        ///     A test for CheckDeposit where the transaction Id doesn't match.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(BankException))]
        public void CheckDepositIncorrectIdTest()
        {
            _target.CheckDeposit(AccountType.Cashable, 1000, Guid.Empty);
        }

        /// <summary>
        ///     A test for CheckDeposit where the AccountType is not found in the Bank.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(BankException))]
        public void CheckDepositAccountMissingTest()
        {
            _accessor._accounts.Remove(AccountType.Cashable);
            _target.CheckDeposit(AccountType.Cashable, 1000, _transactionId);
        }

        /// <summary>Test for Dispose().</summary>
        [TestMethod]
        public void DisposeTest()
        {
            Assert.IsNotNull(_accessor._accountsLock);

            _target.Dispose();

            Assert.IsNull(_accessor._accountsLock);

            // Can be disposed again.
            _target.Dispose();

            Assert.IsNull(_accessor._accountsLock);
        }
    }
}