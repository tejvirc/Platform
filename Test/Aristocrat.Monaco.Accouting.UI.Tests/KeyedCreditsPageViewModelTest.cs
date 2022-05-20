namespace Aristocrat.Monaco.Accounting.UI.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.Monaco.Kernel.Contracts.LockManagement;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using UI.ViewModels;

    /// <summary>
    ///     Summary description for KeyedCreditsPageViewModelTest
    /// </summary>
    [TestClass]
    public class KeyedCreditsPageViewModelTest
    {
        private KeyedCreditsPageViewModel _target;
        private Mock<IBank> _bank;
        private Mock<ITransactionCoordinator> _transactionCoordinator;
        private Mock<ILocalizerFactory> _localizerFactory;
        private Mock<IPropertiesManager> _propertyManager;
        private Mock<IMeterManager> _meterManager;
        private Mock<IPersistentStorageManager> _persistentStorageManager;
        private Mock<ITransactionHistory> _transactionHistory;
        private Guid _transactionGuid;
        private Mock<ILockManager> _lockManager;
        private Mock<IDisposable> _disposable;

        [TestInitialize]
        public void TestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _bank = MoqServiceManager.CreateAndAddService<IBank>(MockBehavior.Loose);
            _bank.Setup(m => m.CheckDeposit(It.IsAny<AccountType>(), It.IsAny<long>(), It.IsAny<Guid>())).Returns(true);
            _bank.Setup(m => m.CheckWithdraw(It.IsAny<AccountType>(), It.IsAny<long>(), It.IsAny<Guid>()))
                .Returns(true);
            _transactionCoordinator =
                MoqServiceManager.CreateAndAddService<ITransactionCoordinator>(MockBehavior.Loose);
            _transactionGuid = Guid.NewGuid();
            _transactionCoordinator.Setup(
                    m => m.RequestTransaction(It.IsAny<Guid>(), It.IsAny<int>(), TransactionType.Write))
                .Returns(_transactionGuid);
            _localizerFactory = MoqServiceManager.CreateAndAddService<ILocalizerFactory>(MockBehavior.Loose);
            _localizerFactory.Setup(m => m.For(It.IsAny<string>())).Returns<string>(
                name =>
                {
                    var localizer = new Mock<ILocalizer>();
                    localizer.Setup(m => m.CurrentCulture).Returns(new CultureInfo("es-US"));
                    localizer.Setup(m => m.GetString(It.IsAny<string>())).Returns<string>(s => s);
                    return localizer.Object;
                });
            _propertyManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Loose);
            _propertyManager
                .Setup(
                    m => m.GetProperty(
                        ApplicationConstants.CurrencyMultiplierKey,
                        ApplicationConstants.DefaultCurrencyMultiplier))
                .Returns(ApplicationConstants.DefaultCurrencyMultiplier);

            _disposable = new Mock<IDisposable>(MockBehavior.Default);
            _disposable.Setup(d => d.Dispose()).Verifiable();
            _lockManager = MoqServiceManager.CreateAndAddService<ILockManager>(MockBehavior.Default);
            _lockManager.Setup(l => l.AcquireExclusiveLock(It.IsAny<IEnumerable<ILockable>>())).Returns(_disposable.Object);

            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Loose);
            _meterManager.Setup(m => m.GetMeter(It.IsAny<string>()).Increment(It.IsAny<long>()));
            _persistentStorageManager =
                MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Loose);
            _persistentStorageManager.Setup(m => m.ScopedTransaction().Complete());
            _transactionHistory = MoqServiceManager.CreateAndAddService<ITransactionHistory>(MockBehavior.Loose);
            _transactionHistory.Setup(m => m.AddTransaction(It.IsAny<KeyedCreditsTransaction>()));
            _target = new KeyedCreditsPageViewModel();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        [DataRow(AccountType.Cashable)]
        [DataRow(AccountType.NonCash)]
        [DataRow(AccountType.Cashable)]
        public void KeyOnCreditsTest(AccountType accountType)
        {
            var credit = new KeyedCreditsPageViewModel.Credit(accountType, null, null, 0, false);
            _target.SelectedCredit = credit;
            _target.KeyedOnCreditAmount = 1;
            const long newValue = 100000;
            _bank.Setup(m => m.QueryBalance(accountType)).Returns(newValue);

            var obj = new PrivateObject(_target);
            obj.Invoke("ConfirmKeyOnCreditsPressed", new object[] { null });

            _bank.Verify(m => m.Deposit(accountType, newValue, _transactionGuid), Times.Once());

            var creditValue = _target.Credits.Single(c => c.AccountType == accountType).Value;
            Assert.AreEqual(newValue, creditValue);

            _meterManager.Verify(m => m.GetMeter(It.IsAny<string>()).Increment(newValue), Times.Once());
        }

        [TestMethod]
        [DataRow(AccountType.Cashable)]
        [DataRow(AccountType.NonCash)]
        [DataRow(AccountType.Cashable)]
        public void KeyOffCreditsKeyOffTest(AccountType accountType)
        {
            var credit = new KeyedCreditsPageViewModel.Credit(accountType, null, null, 0, false);
            _target.SelectedCredit = credit;
            _target.KeyedOnCreditAmount = 1;
            const long newValue = 100000;
            _bank.Setup(m => m.QueryBalance(accountType)).Returns(newValue);

            var obj = new PrivateObject(_target);
            obj.Invoke("ConfirmKeyOnCreditsPressed", new object[] { null });

            _target.Credits.Single(c => c.AccountType == accountType).KeyOff = true;

            obj.Invoke("ConfirmKeyOffCreditsPressed", new object[] { null });
            _bank.Verify(m => m.Withdraw(accountType, newValue, _transactionGuid), Times.Once());

            _meterManager.Verify(m => m.GetMeter(It.IsAny<string>()).Increment(newValue), Times.Exactly(2));
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(3)]
        [DataRow(7)]
        public void KeyOffCreditsKeyOffMultipleAccountsTest(int keyOnFreq)
        {
            var obj = new PrivateObject(_target);
            _bank.Setup(m => m.QueryBalance(It.IsAny<AccountType>())).Returns(0);

            var creditsPointer = 0;
            const int numOfAccountTypes = 3;
            while (creditsPointer < keyOnFreq)
            {
                var credit = _target.Credits[creditsPointer % numOfAccountTypes];
                _target.SelectedCredit = credit;
                _target.KeyedOnCreditAmount = 1;
                _bank.Setup(m => m.QueryBalance(credit.AccountType)).Returns(10000);
                obj.Invoke("ConfirmKeyOnCreditsPressed", new object[] { null });
                creditsPointer++;
            }

            _target.Credits.ForEach(c => c.KeyOff = true);
            var expectedWithdraws = keyOnFreq < numOfAccountTypes ? keyOnFreq : numOfAccountTypes;

            obj.Invoke("ConfirmKeyOffCreditsPressed", new object[] { null });

            // Bank withdraws from all accounts that had been keyed on
            _bank.Verify(m => m.Withdraw(It.IsAny<AccountType>(), It.IsAny<long>(), _transactionGuid), Times.Exactly(expectedWithdraws));
            // 2 Meters increment per action:
            // Amount meter and count increments for every Key On press and for each account keyed off
            _meterManager.Verify(m => m.GetMeter(It.IsAny<string>()).Increment(It.IsAny<long>()), Times.Exactly(2 * (keyOnFreq + expectedWithdraws)));
        }
    }
}