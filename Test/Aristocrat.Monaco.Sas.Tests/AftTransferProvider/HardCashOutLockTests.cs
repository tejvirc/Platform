namespace Aristocrat.Monaco.Sas.Tests.AftTransferProvider
{
    using Accounting.Contracts;
    using Application.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.AftTransferProvider;
    using System;
    using System.Collections.Generic;
    using Application.Contracts.Localization;
    using Test.Common;
    using Kernel.Contracts.MessageDisplay;

    [TestClass]
    public class HardCashOutLockTests
    {
        private static IEnumerable<object[]> CanCashoutTestData => new List<object[]>
        {
            new object[]
            {
                false,
                new List<Guid>(),
                true
            },
            new object[]
            {
                true,
                new List<Guid> { Guid.Empty },
                false
            },
            new object[]
            {
                true,
                new List<Guid> { ApplicationConstants.HostCashOutFailedDisableKey },
                true
            }
        };

        private Mock<ITransactionCoordinator> _transactionCoordinator;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ITransferOutHandler> _transferOutHandler;
        private Mock<ISystemDisableManager> _systemDisableManager;
        private Mock<IEventBus> _eventBus;
        private Mock<IBank> _bank;

        private HardCashOutLock _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            var locale = MoqServiceManager.CreateAndAddService<ILocalizerFactory>(MockBehavior.Strict);
            locale.Setup(x => x.For(It.IsAny<string>())).Returns(new Mock<ILocalizer>().Object);

            _transactionCoordinator = new Mock<ITransactionCoordinator>(MockBehavior.Default);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _transferOutHandler = new Mock<ITransferOutHandler>(MockBehavior.Default);
            _systemDisableManager = new Mock<ISystemDisableManager>(MockBehavior.Default);
            _eventBus = new Mock<IEventBus>(MockBehavior.Default);
            _bank = new Mock<IBank>(MockBehavior.Default);

            _target = new HardCashOutLock(
                _transactionCoordinator.Object,
                _propertiesManager.Object,
                _transferOutHandler.Object,
                _systemDisableManager.Object,
                _eventBus.Object,
                _bank.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullTransactionCoordinator()
        {
            _target = new HardCashOutLock(
                null,
                _propertiesManager.Object,
                _transferOutHandler.Object,
                _systemDisableManager.Object,
                _eventBus.Object,
                _bank.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullPropertiesManager()
        {
            _target = new HardCashOutLock(
                _transactionCoordinator.Object,
                null,
                _transferOutHandler.Object,
                _systemDisableManager.Object,
                _eventBus.Object,
                _bank.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullTransferOutHandler()
        {
            _target = new HardCashOutLock(
                _transactionCoordinator.Object,
                _propertiesManager.Object,
                null,
                _systemDisableManager.Object,
                _eventBus.Object,
                _bank.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullSystemDisabledManager()
        {
            _target = new HardCashOutLock(
                _transactionCoordinator.Object,
                _propertiesManager.Object,
                _transferOutHandler.Object,
                null,
                _eventBus.Object,
                _bank.Object);
        }


        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullNullEventBus()
        {
            _target = new HardCashOutLock(
                _transactionCoordinator.Object,
                _propertiesManager.Object,
                _transferOutHandler.Object,
                _systemDisableManager.Object,
                null,
                _bank.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullBank()
        {
            _target = new HardCashOutLock(
                _transactionCoordinator.Object,
                _propertiesManager.Object,
                _transferOutHandler.Object,
                _systemDisableManager.Object,
                _eventBus.Object,
                null);
        }

        [DynamicData(nameof(CanCashoutTestData))]
        [DataTestMethod]
        public void CanCashoutTest(bool isDisabled, IReadOnlyList<Guid> disabledGuids, bool canCashout)
        {
            _systemDisableManager.Setup(x => x.IsDisabled).Returns(isDisabled);
            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(disabledGuids);

            Assert.AreEqual(canCashout, _target.CanCashOut);
        }

        [TestMethod]
        public void PresentLockupTest()
        {
            _bank.Setup(x => x.QueryBalance(AccountType.Cashable)).Returns(100);
            _bank.Setup(x => x.QueryBalance(AccountType.Promo)).Returns(100);
            _bank.Setup(x => x.QueryBalance(AccountType.NonCash)).Returns(100);
            _propertiesManager.Setup(x => x.GetProperty(AftConstants.MaximumVoucherLimitPropertyName, It.IsAny<long>()))
                .Returns(10000L);
            _eventBus.Setup(x => x.Publish(It.IsAny<HardCashLockoutEvent>())).Verifiable();
            _systemDisableManager.Setup(
                x => x.Disable(ApplicationConstants.HostCashOutFailedDisableKey, SystemDisablePriority.Immediate, It.IsAny<string>(), It.IsAny<CultureProviderType>())).Verifiable();

            _target.PresentLockup();
            _eventBus.Verify();
            _systemDisableManager.Verify();
        }
    }
}