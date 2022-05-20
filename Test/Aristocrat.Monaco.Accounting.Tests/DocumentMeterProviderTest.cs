namespace Aristocrat.Monaco.Accounting.Tests
{
    using Application.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Provides the unit tests for the DocumentMeterProvider class
    /// </summary>
    [TestClass]
    public class DocumentMeterProviderTest
    {
        private Mock<IPersistentStorageAccessor> _block;
        private Mock<IPersistentStorageManager> _persistentStorage;
        private Mock<IPropertiesManager> _propertiesManager;

        private DocumentMeterProvider _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _persistentStorage = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);
            _block = MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Strict);
            _block.SetupGet(m => m.Level).Returns(PersistenceLevel.Critical);

            _persistentStorage.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(true);
            _persistentStorage.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_block.Object);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.OccurrenceMeterRolloverText, It.IsAny<int>()))
                .Returns(100L);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMeterRolloverText, It.IsAny<long>()))
                .Returns(100L);

            _target = new DocumentMeterProvider();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConstructorWhenBlockExistsTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void ConstructorWhenBlockDoesntExistsTest()
        {
            _persistentStorage.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(false);
            _persistentStorage.Setup(m => m.CreateBlock(PersistenceLevel.Critical, It.IsAny<string>(), 1))
                .Returns(_block.Object);

            _target = new DocumentMeterProvider();

            Assert.IsNotNull(_target);
        }
    }
}