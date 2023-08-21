namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using Accounting.Contracts;
    using Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    ///     Provides unit tests for the CashableLockupProvider class
    /// </summary>
    [TestClass]
    public class CashableLockupProviderTest
    {
        private readonly Mock<IPropertiesManager>
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);

        private readonly Mock<IFundsTransferDisable> _fundsTransferDisable =
            new Mock<IFundsTransferDisable>(MockBehavior.Strict);

        private readonly Mock<ITransactionCoordinator> _transactionCoordinator =
            new Mock<ITransactionCoordinator>(MockBehavior.Strict);

        private CashableLockupProvider _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new CashableLockupProvider(
                _propertiesManager.Object,
                _fundsTransferDisable.Object,
                _transactionCoordinator.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullPropertiesManagerTest()
        {
            _ = new CashableLockupProvider(null, _fundsTransferDisable.Object, _transactionCoordinator.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullFundsTransferDisableTest()
        {
            _ = new CashableLockupProvider(_propertiesManager.Object, null, _transactionCoordinator.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullTransactionCoordinatorDisableTest()
        {
            _ = new CashableLockupProvider(_propertiesManager.Object, _fundsTransferDisable.Object, null);
        }

        [TestMethod]
        public void LockupBehaviorNotAllowedReturnsFalseTest()
        {
            _propertiesManager.Setup(
                    m => m.GetProperty(GamingConstants.LockupBehavior, CashableLockupStrategy.NotAllowed))
                .Returns(CashableLockupStrategy.NotAllowed);
            _transactionCoordinator.Setup(t => t.IsTransactionActive).Returns(false);

            Assert.IsFalse(_target.CanCashoutInLockup(true, true, null));
        }

        [TestMethod]
        public void LockupBehaviorForceCashoutReturnsFalseTest()
        {
            _propertiesManager.Setup(
                    m => m.GetProperty(GamingConstants.LockupBehavior, CashableLockupStrategy.NotAllowed))
                .Returns(CashableLockupStrategy.ForceCashout);
            _fundsTransferDisable.Setup(m => m.TransferOffDisabled).Returns(false);
            _transactionCoordinator.Setup(t => t.IsTransactionActive).Returns(false);

            Assert.IsFalse(_target.CanCashoutInLockup(true, true, () => { }));
        }

        [TestMethod]
        public void LockupBehaviorAllowedReturnsFalseWhenNotLockedUpTest()
        {
            _propertiesManager.Setup(
                    m => m.GetProperty(GamingConstants.LockupBehavior, CashableLockupStrategy.NotAllowed))
                .Returns(CashableLockupStrategy.Allowed);
            _transactionCoordinator.Setup(t => t.IsTransactionActive).Returns(false);

            Assert.IsFalse(_target.CanCashoutInLockup(false, true, null));
        }

        [TestMethod]
        public void LockupBehaviorAllowedReturnsFalseWhenNotAllowedToCashoutTest()
        {
            _propertiesManager.Setup(
                    m => m.GetProperty(GamingConstants.LockupBehavior, CashableLockupStrategy.NotAllowed))
                .Returns(CashableLockupStrategy.Allowed);
            _fundsTransferDisable.Setup(m => m.TransferOffDisabled).Returns(false);
            _transactionCoordinator.Setup(t => t.IsTransactionActive).Returns(false);

            Assert.IsFalse(_target.CanCashoutInLockup(true, false, null));
        }

        [TestMethod]
        public void LockupBehaviorAllowedReturnsTrueTest()
        {
            _propertiesManager.Setup(
                    m => m.GetProperty(GamingConstants.LockupBehavior, CashableLockupStrategy.NotAllowed))
                .Returns(CashableLockupStrategy.Allowed);
            _fundsTransferDisable.Setup(m => m.TransferOffDisabled).Returns(false);
            _transactionCoordinator.Setup(t => t.IsTransactionActive).Returns(false);

            Assert.IsTrue(_target.CanCashoutInLockup(true, true, null));
        }

        [TestMethod]
        public void LockupBehaviorAllowedReturnsFalseWhenTransferOffIsDisabledTest()
        {
            _propertiesManager.Setup(
                    m => m.GetProperty(GamingConstants.LockupBehavior, CashableLockupStrategy.NotAllowed))
                .Returns(CashableLockupStrategy.Allowed);
            _fundsTransferDisable.Setup(m => m.TransferOffDisabled).Returns(true);
            _transactionCoordinator.Setup(t => t.IsTransactionActive).Returns(false);

            Assert.IsFalse(_target.CanCashoutInLockup(true, true, null));
        }

        [TestMethod]
        public void LockupBehaviorAllowedReturnsFalseWhenTransactionActiveTest()
        {
            _propertiesManager.Setup(
                    m => m.GetProperty(GamingConstants.LockupBehavior, CashableLockupStrategy.NotAllowed))
                .Returns(CashableLockupStrategy.Allowed);
            _fundsTransferDisable.Setup(m => m.TransferOffDisabled).Returns(false);
            _transactionCoordinator.Setup(t => t.IsTransactionActive).Returns(true);

            Assert.IsFalse(_target.CanCashoutInLockup(true, true, null));
        }
    }
}