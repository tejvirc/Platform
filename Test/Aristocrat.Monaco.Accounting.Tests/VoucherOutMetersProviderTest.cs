namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Application.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     This is a test class for VoucherOutMetersProviderTest and is intended
    ///     to contain all VoucherOutMetersProviderTest Unit Tests
    /// </summary>
    [TestClass]
    public class VoucherOutMetersProviderTest
    {
        private const string StorageBlockName = "Aristocrat.Monaco.Accounting.VoucherOutMetersProvider";
        private Mock<IPersistentStorageAccessor> _block;
        private Mock<IPersistentStorageManager> _persistentStorage;
        private Mock<IPropertiesManager> _propertiesManager;

        private Mock<IDisposable> _disposable;

        private VoucherOutMetersProvider _target;

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
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OccurrenceMeterRolloverText, It.IsAny<int>()))
                .Returns(100L);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<long>()))
                .Returns(100L);

            _disposable = new Mock<IDisposable>(MockBehavior.Default);
            _disposable.Setup(d => d.Dispose()).Verifiable();

            _target = new VoucherOutMetersProvider();
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
        ///     A test for CreateVoucherOutMeters
        /// </summary>
        [TestMethod]
        public void CreateVoucherOutMetersTest()
        {
            Assert.IsTrue(_target.MeterNames.Contains("TotalVouchersOut"));
            Assert.IsTrue(_target.MeterNames.Contains("TotalVouchersOutCount"));
            Assert.IsTrue(_target.MeterNames.Contains("VoucherOutCashableValue"));
            Assert.IsTrue(_target.MeterNames.Contains("VoucherOutCashableValue"));
            Assert.IsTrue(_target.MeterNames.Contains("VoucherOutCashablePromotionalValue"));
            Assert.IsTrue(_target.MeterNames.Contains("VoucherOutCashablePromotionalCount"));
            Assert.IsTrue(_target.MeterNames.Contains("VoucherOutNonCashablePromotionalValue"));
            Assert.IsTrue(_target.MeterNames.Contains("VoucherOutNonCashablePromotionalCount"));
        }
   }
}