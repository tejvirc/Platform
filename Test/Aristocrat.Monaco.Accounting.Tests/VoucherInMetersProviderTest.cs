namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Application.Contracts;
    using Aristocrat.Monaco.Kernel.Contracts.LockManagement;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     This is a test class for VoucherInMetersProviderTest and is intended
    ///     to contain all VoucherInMetersProviderTest Unit Tests
    /// </summary>
    [TestClass]
    public class VoucherInMetersProviderTest
    {
        private const string StorageBlockName = "Aristocrat.Monaco.Accounting.VoucherInMetersProvider";
        private dynamic _accessor;
        private Mock<IPersistentStorageAccessor> _block;
        private Mock<IPersistentStorageManager> _persistentStorage;
        private Mock<IPropertiesManager> _propertiesManager;

        private Mock<ILockManager> _lockManager;
        private Mock<IDisposable> _disposable;

        private VoucherInMetersProvider _target;

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _persistentStorage = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);
            _block = MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Strict);

            _persistentStorage.Setup(m => m.BlockExists(StorageBlockName)).Returns(true);
            _persistentStorage.Setup(m => m.GetBlock(StorageBlockName)).Returns(_block.Object);
            _block.SetupGet(m => m.Level).Returns(PersistenceLevel.Critical);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(
                    m => m.GetProperty(ApplicationConstants.OccurrenceMeterRolloverText, It.IsAny<int>()))
                .Returns(100L);
            _propertiesManager.Setup(
                    m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<long>()))
                .Returns(100L);

            _disposable = new Mock<IDisposable>(MockBehavior.Default);
            _disposable.Setup(d => d.Dispose()).Verifiable();
            _lockManager = MoqServiceManager.CreateAndAddService<ILockManager>(MockBehavior.Default);
            _lockManager.Setup(l => l.AcquireExclusiveLock(It.IsAny<IEnumerable<ILockable>>())).Returns(_disposable.Object);

            _target = new VoucherInMetersProvider();
            _accessor = new DynamicPrivateObject(_target);
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            AddinManager.Shutdown();
            MoqServiceManager.RemoveInstance();

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        ///     A test for CreateVoucherInMeters
        /// </summary>
        [TestMethod]
        public void CreateVoucherInMetersTest()
        {
            Assert.IsTrue(_target.MeterNames.Contains("TotalVouchersIn"));
            Assert.IsTrue(_target.MeterNames.Contains("TotalVouchersInCount"));
            Assert.IsTrue(_target.MeterNames.Contains("VoucherInCashableValue"));
            Assert.IsTrue(_target.MeterNames.Contains("VoucherInCashableValue"));
            Assert.IsTrue(_target.MeterNames.Contains("VoucherInCashablePromotionalValue"));
            Assert.IsTrue(_target.MeterNames.Contains("VoucherInCashablePromotionalCount"));
            Assert.IsTrue(_target.MeterNames.Contains("VoucherInNonCashablePromotionalValue"));
            Assert.IsTrue(_target.MeterNames.Contains("VoucherInNonCashablePromotionalCount"));
            Assert.IsTrue(_target.MeterNames.Contains("VoucherInNonTransferablePromotionalValue"));
            Assert.IsTrue(_target.MeterNames.Contains("VoucherInNonTransferablePromotionalCount"));
        }
    }
}