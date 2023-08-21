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
    ///     This is a test class for WatOffMetersProvider and is intended
    ///     to contain all WatOffMetersProvider Unit Tests
    /// </summary>
    [TestClass]
    public class WatOffMetersProviderTest
    {
        private Mock<IPersistentStorageAccessor> _block;
        private Mock<IPersistentStorageManager> _persistentStorage;
        private Mock<IPropertiesManager> _propertiesManager;

        private Mock<IDisposable> _disposable;

        private WatOffMetersProvider _target;

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

            _target = new WatOffMetersProvider();
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
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

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [TestMethod]
        public void CreateWatOffMetersTest()
        {
            Assert.IsTrue(_target.MeterNames.Contains("WatOffCashableValue"));
            Assert.IsTrue(_target.MeterNames.Contains("WatOffCashableCount"));
            Assert.IsTrue(_target.MeterNames.Contains("WatOffCashablePromotionalValue"));
            Assert.IsTrue(_target.MeterNames.Contains("WatOffCashablePromotionalCount"));
            Assert.IsTrue(_target.MeterNames.Contains("WatOffNonCashablePromotionalValue"));
            Assert.IsTrue(_target.MeterNames.Contains("WatOffNonCashablePromotionalCount"));
            Assert.IsTrue(_target.MeterNames.Contains("TotalWatOff"));
            Assert.IsTrue(_target.MeterNames.Contains("TotalWatOffCount"));
        }
    }
}