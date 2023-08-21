namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using Aristocrat.Monaco.Hardware.Contracts.Persistence;
    using Contracts;
    using Contracts.Models;
    using Hardware.Contracts.Audio;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GameCategoryServiceTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventBusIsNullExpectException()
        {
            var service = new GameCategoryService(null, null, null, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenGameProviderIsNullExpectException()
        {
            var eventBus = new Mock<IEventBus>();
            var service = new GameCategoryService(eventBus.Object, null, null, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPropertiesIsNullExpectException()
        {
            var provider = new Mock<IGameProvider>();
            var eventBus = new Mock<IEventBus>();
            var service = new GameCategoryService(eventBus.Object, provider.Object, null, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPersistenceIsNullExpectException()
        {
            var provider = new Mock<IGameProvider>();
            var eventBus = new Mock<IEventBus>();
            var properties = new Mock<IPropertiesManager>();
            var service = new GameCategoryService(eventBus.Object, provider.Object, properties.Object, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var provider = new Mock<IGameProvider>();
            var eventBus = new Mock<IEventBus>();
            var properties = new Mock<IPropertiesManager>();
            var persistentStorage = new Mock<IPersistentStorageManager>();

            var service = new GameCategoryService(eventBus.Object, provider.Object, properties.Object, persistentStorage.Object);

            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void WhenSelectedGameCategorySettingExpectSuccess()
        {
            var provider = new Mock<IGameProvider>();
            var eventBus = new Mock<IEventBus>();
            var properties = new Mock<IPropertiesManager>();
            var persistentStorage = new Mock<IPersistentStorageManager>();

            var blockAccessor = new Mock<IPersistentStorageAccessor>();

            blockAccessor.Setup(a => a.StartTransaction()).Returns((new Mock<IPersistentStorageTransaction>()).Object);
            persistentStorage.Setup(a => a.ScopedTransaction()).Returns(new Mock<IScopedTransaction>().Object);
            persistentStorage
                .Setup(a => a.CreateBlock(It.IsAny<PersistenceLevel>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(blockAccessor.Object);

            var totalGameTypes = Enum.GetValues(typeof(GameType)).Length * 3;
            var count = 0;
            properties.Setup(a => a.GetProperty(It.IsAny<string>(), It.IsAny<object>())).Returns(
                () =>
                {
                    if (count < totalGameTypes)
                    {
                        ++count; return true;
                    }

                    return 1;
                });

            var gameDetail = new Mock<IGameDetail>();
            gameDetail.SetupGet(g => g.GameType).Returns(GameType.Slot);
            provider.Setup(a => a.GetGame(It.IsAny<int>())).Returns(gameDetail.Object);
            var service = new GameCategoryService(eventBus.Object, provider.Object, properties.Object, persistentStorage.Object);
            service.Initialize();

            Assert.AreEqual(service.SelectedGameCategorySetting.AutoPlay, true);
            Assert.AreEqual(service.SelectedGameCategorySetting.PlayerSpeed, 2);
            Assert.AreEqual(service.SelectedGameCategorySetting.VolumeScalar, VolumeScalar.Scale80);
            Assert.AreEqual(service.SelectedGameCategorySetting.ShowPlayerSpeedButton, true);
        }

        [TestMethod]
        public void WhenGetGameCategorySettingExpectSuccess()
        {
            var provider = new Mock<IGameProvider>();
            var eventBus = new Mock<IEventBus>();
            var properties = new Mock<IPropertiesManager>();
            var persistentStorage = new Mock<IPersistentStorageManager>();
            var blockAccessor = new Mock<IPersistentStorageAccessor>();

            properties.Setup(p => p.GetProperty(It.IsAny<string>(), It.IsAny<object>())).Returns(true);
            blockAccessor.Setup(a => a.StartTransaction()).Returns((new Mock<IPersistentStorageTransaction>()).Object);
            persistentStorage.Setup(a => a.ScopedTransaction()).Returns(new Mock<IScopedTransaction>().Object);
            persistentStorage
                .Setup(a => a.CreateBlock(It.IsAny<PersistenceLevel>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(blockAccessor.Object);

            var service = new GameCategoryService(eventBus.Object, provider.Object, properties.Object, persistentStorage.Object);
            service.Initialize();

            var lookUp = service.GetGameCategorySetting(GameType.Keno);

            Assert.AreEqual(lookUp.AutoPlay, true);
            Assert.AreEqual(lookUp.PlayerSpeed, 2);
            Assert.AreEqual(lookUp.VolumeScalar, VolumeScalar.Scale60);
            Assert.AreEqual(lookUp.ShowPlayerSpeedButton, true);
        }

        [TestMethod]
        public void WhenUpdateGameCategorySettingExpectSuccess()
        {
            var provider = new Mock<IGameProvider>();
            var eventBus = new Mock<IEventBus>();
            var properties = new Mock<IPropertiesManager>();
            var persistentStorage = new Mock<IPersistentStorageManager>();

            var blockAccessor = new Mock<IPersistentStorageAccessor>();

            properties.Setup(p => p.GetProperty(It.IsAny<string>(), It.IsAny<object>())).Returns(true);
            blockAccessor.Setup(a => a.StartTransaction()).Returns((new Mock<IPersistentStorageTransaction>()).Object);
            persistentStorage.Setup(a => a.ScopedTransaction()).Returns(new Mock<IScopedTransaction>().Object);
            persistentStorage
                .Setup(a => a.CreateBlock(It.IsAny<PersistenceLevel>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(blockAccessor.Object);

            var service = new GameCategoryService(eventBus.Object, provider.Object, properties.Object, persistentStorage.Object);
            service.Initialize();

            var result = new GameCategorySetting
            {
                AutoPlay = true,
                ShowPlayerSpeedButton = false,
                VolumeScalar = VolumeScalar.Scale20,
                PlayerSpeed = 2
            };

            service.UpdateGameCategory(GameType.Slot, result);
            var lookUp = service.GetGameCategorySetting(GameType.Slot);

            Assert.AreEqual(lookUp.AutoPlay, result.AutoPlay);
            Assert.AreEqual(lookUp.PlayerSpeed, result.PlayerSpeed);
            Assert.AreEqual(lookUp.VolumeScalar, result.VolumeScalar);
            Assert.AreEqual(lookUp.ShowPlayerSpeedButton, result.ShowPlayerSpeedButton);
        }
    }
}
