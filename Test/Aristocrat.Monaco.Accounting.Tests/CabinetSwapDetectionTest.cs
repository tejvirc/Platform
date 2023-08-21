namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using Application.Contracts.Localization;
    using Contracts;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Test.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    ///     Contains the unit tests for the CabinetSwapDetectionService class
    /// </summary>
    [TestClass]
    public class CabinetSwapDetectionServiceTests
    {
        private Mock<IPropertiesManager> _propertyManager;
        private Mock<IPersistentStorageManager> _storage;
        private Mock<ISystemDisableManager> _systemDisableManager;
        private Mock<ITransferOutHandler> _transferOutHandler;
        private Mock<IEventBus> _bus;
        private Mock<IBank> _bank;
        private Mock<IIO> _iio;
        private Mock<IPersistentStorageAccessor> _block;
        private Mock<ILocalizerFactory> _localizerFactory;

        private CabinetSwapDetectionService _target;
        private dynamic _accessor;

        [TestInitialize]
        public void TestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);

            _propertyManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);

            _block = MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Default);
            _storage = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Default);
            _storage.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(true);
            _storage.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_block.Object);

            _systemDisableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Default);
            _localizerFactory = MoqServiceManager.CreateAndAddService<ILocalizerFactory>(MockBehavior.Default);

            _transferOutHandler = MoqServiceManager.CreateAndAddService<ITransferOutHandler>(MockBehavior.Default);
            _bus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _bank = MoqServiceManager.CreateAndAddService<IBank>(MockBehavior.Default);
            _iio = MoqServiceManager.CreateAndAddService<IIO>(MockBehavior.Default);

            _propertyManager.Setup(m => m.GetProperty("CurrencyMultiplier", 0d)).Returns(1.0);
            _propertyManager.Setup(x => x.GetProperty(AccountingConstants.CashoutOnCarrierBoardRemovalEnabled, false))
                .Returns(true);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullPropertyManagerTest()
        {
            var unused = new CabinetSwapDetectionService(
                null,
                _storage.Object,
                _systemDisableManager.Object,
                _transferOutHandler.Object,
                _bus.Object,
                _bank.Object,
                _iio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullStorageTest()
        {
            var unused = new CabinetSwapDetectionService(
                _propertyManager.Object,
                null,
                _systemDisableManager.Object,
                _transferOutHandler.Object,
                _bus.Object,
                _bank.Object,
                _iio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullSystemDisableTest()
        {
            var unused = new CabinetSwapDetectionService(
                _propertyManager.Object,
                _storage.Object,
                null,
                _transferOutHandler.Object,
                _bus.Object,
                _bank.Object,
                _iio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullTransferOutTest()
        {
            var unused = new CabinetSwapDetectionService(
                _propertyManager.Object,
                _storage.Object,
                _systemDisableManager.Object,
                null,
                _bus.Object,
                _bank.Object,
                _iio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullEventBusTest()
        {
            var unused = new CabinetSwapDetectionService(
                _propertyManager.Object,
                _storage.Object,
                _systemDisableManager.Object,
                _transferOutHandler.Object,
                null,
                _bank.Object,
                _iio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullBankTest()
        {
            var unused = new CabinetSwapDetectionService(
                _propertyManager.Object,
                _storage.Object,
                _systemDisableManager.Object,
                _transferOutHandler.Object,
                _bus.Object,
                null,
                _iio.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullIIOTest()
        {
            var unused = new CabinetSwapDetectionService(
                _propertyManager.Object,
                _storage.Object,
                _systemDisableManager.Object,
                _transferOutHandler.Object,
                _bus.Object,
                _bank.Object,
                null);
        }

        [TestMethod]
        public void BoardSwappedWithZeroCreditBalanceTest()
        {
            _iio.Setup(m => m.WasCarrierBoardRemoved).Returns(true);

            _block.SetupGet(m => m[@"CarrierBoardWasRemoved"]).Returns(true);
            _storage.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_block.Object);
            _bank.Setup(m => m.QueryBalance()).Returns(0);

            _target = CreateTarget();
            _accessor = new DynamicPrivateObject(_target);
            _accessor.Initialize();

            // No credits, system should not lock up
            _systemDisableManager.Verify(
                m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null), Times.Never);
        }

        [TestMethod]
        public void BoardSwappedWithCreditBalanceTest()
        {
            _iio.Setup(m => m.WasCarrierBoardRemoved).Returns(true);

            _block.SetupGet(m => m[@"CarrierBoardWasRemoved"]).Returns(true);
            _storage.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_block.Object);
            _bank.Setup(m => m.QueryBalance()).Returns(10000);

            _localizerFactory.Setup(x => x.For(It.IsAny<string>())).Returns(new Mock<ILocalizer>().Object);
            var _localizer = new Mock<ILocalizer>();
            _localizer.Setup(x => x.GetString(It.IsAny<string>())).Returns("empty");
            _localizerFactory.Setup(x => x.For(It.IsAny<string>())).Returns(_localizer.Object);

            _target = CreateTarget();
            _accessor = new DynamicPrivateObject(_target);
            _accessor.Initialize();

            _systemDisableManager.Verify(
                m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null));

            _accessor.Handle(
                new TransferOutCompletedEvent(
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<bool>(),
                    It.IsAny<Guid>()));

            // The transfer out completed, ensure the system is unlocked
            _systemDisableManager.Verify(m => m.Enable(It.IsAny<Guid>()));
        }

        [TestMethod]
        public void MacAddressChangeWithZeroCreditBalanceTest()
        {
            _iio.Setup(m => m.WasCarrierBoardRemoved).Returns(false);
            _block.SetupGet(m => m[@"CarrierBoardWasRemoved"]).Returns(false);
            _block.SetupGet(m => m[@"LastRecordedMacAddressKey"]).Returns("111111111111");
            _storage.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_block.Object);
            _bank.Setup(m => m.QueryBalance()).Returns(0);

            _target = CreateTarget();
            _accessor = new DynamicPrivateObject(_target);
            _accessor.Initialize();

            _systemDisableManager.Verify(
                m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null), Times.Never);
        }

        [TestMethod]
        public void MacAddressChangedWithCreditBalanceTest()
        {
            _iio.Setup(m => m.WasCarrierBoardRemoved).Returns(false);
            _block.SetupGet(m => m[@"CarrierBoardWasRemoved"]).Returns(false);
            _block.SetupGet(m => m[@"LastRecordedMacAddressKey"]).Returns("111111111111");
            _storage.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_block.Object);
            _bank.Setup(m => m.QueryBalance()).Returns(10000);

            _localizerFactory.Setup(x => x.For(It.IsAny<string>())).Returns(new Mock<ILocalizer>().Object);
            var localizer = new Mock<ILocalizer>();
            localizer.Setup(x => x.GetString(It.IsAny<string>())).Returns("empty");
            _localizerFactory.Setup(x => x.For(It.IsAny<string>())).Returns(localizer.Object);

            _target = CreateTarget();
            _accessor = new DynamicPrivateObject(_target);
            _accessor.Initialize();

            // Ensure the system is locked at this point
            _systemDisableManager.Verify(
                m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null));

            _accessor.Handle(
                new TransferOutCompletedEvent(
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<bool>(),
                    It.IsAny<Guid>()));

            // The transfer out completed, ensure the system is unlocked
            _systemDisableManager.Verify(m => m.Enable(It.IsAny<Guid>()));
        }

        private CabinetSwapDetectionService CreateTarget()
        {
            return new CabinetSwapDetectionService(
                _propertyManager.Object,
                _storage.Object,
                _systemDisableManager.Object,
                _transferOutHandler.Object,
                _bus.Object,
                _bank.Object,
                _iio.Object);
        }
    }
}