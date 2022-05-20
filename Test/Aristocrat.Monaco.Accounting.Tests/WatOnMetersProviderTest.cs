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
    ///     This is a test class for WatOnMetersProvider and is intended
    ///     to contain all WatOnMetersProvider Unit Tests
    /// </summary>
    [TestClass]
    public class WatOnMetersProviderTest
    {
        private Mock<IPersistentStorageAccessor> _block;

        private Mock<IPersistentStorageManager> _persistentStorage;
        private Mock<IPropertiesManager> _propertiesManager;

        private Mock<ILockManager> _lockManager;
        private Mock<IDisposable> _disposable;

        private WatOnMetersProvider _target;

        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _persistentStorage = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);
            _block = MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Strict);
            _persistentStorage.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(false);
            _persistentStorage.Setup(m => m.CreateBlock(PersistenceLevel.Critical, It.IsAny<string>(), 1))
                .Returns(_block.Object);
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

            _target = new WatOnMetersProvider();
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

        [TestMethod]
        public void CreateWatOnMetersTest()
        {
            Assert.IsTrue(_target.MeterNames.Contains("WatOnCashableValue"));
            Assert.IsTrue(_target.MeterNames.Contains("WatOnCashableCount"));
            Assert.IsTrue(_target.MeterNames.Contains("WatOnCashablePromotionalValue"));
            Assert.IsTrue(_target.MeterNames.Contains("WatOnCashablePromotionalCount"));
            Assert.IsTrue(_target.MeterNames.Contains("WatOnNonCashablePromotionalValue"));
            Assert.IsTrue(_target.MeterNames.Contains("WatOnNonCashablePromotionalCount"));
            Assert.IsTrue(_target.MeterNames.Contains("TotalWatOn"));
            Assert.IsTrue(_target.MeterNames.Contains("TotalWatOnCount"));
        }
    }
}