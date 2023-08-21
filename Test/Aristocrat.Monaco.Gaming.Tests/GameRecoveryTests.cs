namespace Aristocrat.Monaco.Gaming.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GameRecoveryTests
    {
        /* Refactor to GameHistoryTests
         * 
        private const string RecoveryPointBlockName =
            "Aristocrat.Monaco.Gaming.VideoLottery.RuntimeService.RecoveryPoint";
  
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCreateWithNullPersistenceManagerExpectException()
        {
            var recovery = new GameRecovery(null, null);
  
            Assert.IsNull(recovery);
        }
  
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCreateWithNullPropertiesManagerExpectException()
        {
            var persistence = new Mock<IPersistentStorageManager>();
            var recovery = new GameRecovery(persistence.Object, null);
  
            Assert.IsNull(recovery);
        }
  
        [TestMethod]
        public void WhenCreateWithValidParamsExpectSuccess()
        {
            var persistence = new Mock<IPersistentStorageManager>();
            var properties = new Mock<IPropertiesManager>();
  
            persistence.Setup(mock => mock.BlockExists(RecoveryPointBlockName)).Returns(true);
  
            var recovery = new GameRecovery(persistence.Object, properties.Object);
  
            Assert.IsNotNull(recovery);
        }
  
        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void WhenBlockNotCreatedDuringSaveExpectException()
        {
            var persistence = new Mock<IPersistentStorageManager>();
            var properties = new Mock<IPropertiesManager>();
  
            persistence.Setup(mock => mock.BlockExists(RecoveryPointBlockName)).Returns(false);
  
            var recovery = new GameRecovery(persistence.Object, properties.Object);
  
            Assert.IsNull(recovery);
        }
  
        [TestMethod]
        public void WhenLoadWithMismatchedGameIdFalse()
        {
            var persistence = new Mock<IPersistentStorageManager>();
            var properties = new Mock<IPropertiesManager>();
            var storageAccessor = new Mock<IPersistentStorageAccessor>();
  
            persistence.Setup(mock => mock.BlockExists(RecoveryPointBlockName)).Returns(true);
            persistence.Setup(mock => mock.GetBlock(RecoveryPointBlockName)).Returns(storageAccessor.Object);
            storageAccessor.Setup(mock => mock["GameId"]).Returns(1);
            properties.Setup(m => m.GetProperty(Constants.SelectedGameId, 0)).Returns(2);
  
            var recovery = new GameRecovery(persistence.Object, properties.Object);
  
            byte[] data;
            var result = recovery.Load(out data);
  
            Assert.IsFalse(result);
        }
  
        [TestMethod]
        public void WhenLoadWithNoDataExpectFalse()
        {
            var persistence = new Mock<IPersistentStorageManager>();
            var properties = new Mock<IPropertiesManager>();
            var storageAccessor = new Mock<IPersistentStorageAccessor>();
  
            persistence.Setup(mock => mock.BlockExists(RecoveryPointBlockName)).Returns(true);
            persistence.Setup(mock => mock.GetBlock(RecoveryPointBlockName)).Returns(storageAccessor.Object);
            storageAccessor.Setup(mock => mock["Data"]).Returns(null);
            storageAccessor.Setup(mock => mock["GameId"]).Returns(1);
            properties.Setup(m => m.GetProperty(Constants.SelectedGameId, 0)).Returns(1);
  
            var recovery = new GameRecovery(persistence.Object, properties.Object);
  
            byte[] data;
            var result = recovery.Load(out data);
  
            Assert.IsFalse(result);
        }
  
        [TestMethod]
        public void WhenLoadWithBlockExpectSuccess()
        {
            var persistence = new Mock<IPersistentStorageManager>();
            var properties = new Mock<IPropertiesManager>();
  
            var storageAccessor = new Mock<IPersistentStorageAccessor>();
            persistence.Setup(mock => mock.BlockExists(RecoveryPointBlockName)).Returns(true); // Block exists
            persistence.Setup(mock => mock.GetBlock(RecoveryPointBlockName)).Returns(storageAccessor.Object);
            properties.Setup(m => m.GetProperty(Constants.SelectedGameId, 0)).Returns(1);
            var expectedData = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
            storageAccessor.Setup(mock => mock["Data"]).Returns(expectedData);
            storageAccessor.Setup(mock => mock["GameId"]).Returns(1);
  
            var recovery = new GameRecovery(persistence.Object, properties.Object);
  
            byte[] data;
            var result = recovery.Load(out data);
  
            Assert.IsTrue(result);
            Assert.AreEqual(data, expectedData);
        }
  
        [TestMethod]
        public void WhenSaveWithExistingBlockExpectUpdated()
        {
            var persistence = new Mock<IPersistentStorageManager>();
            var properties = new Mock<IPropertiesManager>();
  
            var storageAccessor = new Mock<IPersistentStorageAccessor>();
            persistence.Setup(mock => mock.BlockExists(RecoveryPointBlockName)).Returns(true); // Block exists
            persistence.Setup(mock => mock.GetBlock(RecoveryPointBlockName)).Returns(storageAccessor.Object);
            properties.Setup(m => m.GetProperty(Constants.SelectedGameId, 0)).Returns(1);
            var transaction = new Mock<IPersistentStorageTransaction>();
            storageAccessor.Setup(mock => mock.StartTransaction()).Returns(transaction.Object);
            storageAccessor.Setup(mock => mock["GameId"]).Returns(1);
  
            var recovery = new GameRecovery(persistence.Object, properties.Object);
            var data = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
            recovery.Save(data);
  
            transaction.Verify(s => s.Commit());
        }
  
        [TestMethod]
        public void WhenSaveWithNoBlockExpectUpdated()
        {
            var persistence = new Mock<IPersistentStorageManager>();
            var properties = new Mock<IPropertiesManager>();
  
            var storageAccessor = new Mock<IPersistentStorageAccessor>();
  
            // Block does not exist
            persistence.Setup(mock => mock.BlockExists(RecoveryPointBlockName)).Returns(false);
            persistence.Setup(
                mock => mock.CreateDynamicBlock(
                    It.IsAny<PersistenceLevel>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<BlockFormat>(),
                    It.IsAny<int>())).Returns(storageAccessor.Object);
            properties.Setup(m => m.GetProperty(Constants.SelectedGameId, 0)).Returns(1);
            storageAccessor.Setup(mock => mock["GameId"]).Returns(1);
  
            var transaction = new Mock<IPersistentStorageTransaction>();
            storageAccessor.Setup(mock => mock.StartTransaction()).Returns(transaction.Object);
            var recovery = new GameRecovery(persistence.Object, properties.Object);
            var data = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
            recovery.Save(data);
  
            transaction.Verify(s => s.Commit());
        }
        */
    }
}
