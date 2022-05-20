namespace Aristocrat.Monaco.Application.Tests.Meters
{
    using Application.Meters;
    using Aristocrat.Monaco.Kernel.Contracts.LockManagement;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using Test.Common;

    /// <summary>
    ///     Contains tests for CabinetMetersProvider
    /// </summary>
    [TestClass]
    public class CabinetMetersProviderTest
    {
        private Mock<IPersistentStorageAccessor> _block;
        private Mock<IPersistentStorageManager> _persistentStorage;

        private Mock<IPropertiesManager> _propertiesManager;

        private Mock<ILockManager> _lockManager;
        private Mock<IDisposable> _disposable;

        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _persistentStorage = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);
            _block = MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OccurrenceMeterRolloverText, 100000000))
                .Returns(100000000L);

            _disposable = new Mock<IDisposable>(MockBehavior.Default);
            _disposable.Setup(d => d.Dispose()).Verifiable();
            _lockManager = MoqServiceManager.CreateAndAddService<ILockManager>(MockBehavior.Default);
            _lockManager.Setup(l => l.AcquireExclusiveLock(It.IsAny<IEnumerable<ILockable>>())).Returns(_disposable.Object);
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConstructorBlockExistsTest()
        {
            _persistentStorage.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(true);
            _persistentStorage.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_block.Object);

            var target = new CabinetMetersProvider();
            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void ConstructorBlockCreateTest()
        {
            _persistentStorage.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(false);
            _persistentStorage.Setup(m => m.CreateBlock(PersistenceLevel.Critical, It.IsAny<string>(), 1))
                .Returns(_block.Object);

            var target = new CabinetMetersProvider();
            Assert.IsNotNull(target);
        }
    }
}