namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Application.Contracts;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    [TestClass]
    public class PropertyProviderTests
    {
        private const int GameId = 7;
        private const long Denom = 1000;
        private Mock<IPropertiesManager> _propertiesManager;

        [TestInitialize]
        public void TestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);

            _propertiesManager.Setup(m => m.GetProperty("Mono.SelectedAddinConfigurationHashCode", null)).Returns(null);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (AddinManager.IsInitialized)
            {
                AddinManager.Shutdown();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPersistentStorageManagerIsNullExpectException()
        {
            var service = new PropertyProvider(null);

            Assert.IsNull(service);
        }

        [TestMethod]
        public void WhenGetCollectionExpectProperties()
        {
            var provider = Factory_CreateGamePropertyProvider();

            // We shouldn't care how many we have.  Just make sure we have something.
            Assert.IsTrue(provider.GetCollection.Any());
        }

        [TestMethod]
        public void WhenBlockExistsExpectSuccess()
        {
            const bool blockExists = true;

            var storageManager = new Mock<IPersistentStorageManager>();
            var accessor = new Mock<IPersistentStorageAccessor>();

            var storageName = typeof(PropertyProvider).ToString();

            storageManager.Setup(m => m.BlockExists(It.Is<string>(s => s == storageName))).Returns(blockExists);
            storageManager.Setup(m => m.GetBlock(It.Is<string>(s => s == storageName))).Returns(accessor.Object);

            accessor.Setup(a => a[GamingConstants.SelectedGameId]).Returns(GameId);
            accessor.Setup(a => a[GamingConstants.SelectedDenom]).Returns(Denom);

            var service = new PropertyProvider(storageManager.Object);
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void WhenBlockDoesNotExistExpectSuccess()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None)).Returns(ImportMachineSettings.None);
            const bool blockExists = false;

            var storageManager = new Mock<IPersistentStorageManager>();
            var accessor = new Mock<IPersistentStorageAccessor>();

            var storageName = typeof(PropertyProvider).ToString();

            storageManager.Setup(m => m.BlockExists(It.Is<string>(s => s == storageName))).Returns(blockExists);
            storageManager.Setup(
                    m => m.CreateBlock(
                        It.Is<PersistenceLevel>(l => l == PersistenceLevel.Transient),
                        It.Is<string>(s => s == storageName),
                        It.Is<int>(size => size == 1)))
                .Returns(accessor.Object);

            accessor.Setup(a => a[GamingConstants.SelectedGameId]).Returns(GameId);
            accessor.Setup(a => a[GamingConstants.SelectedDenom]).Returns(Denom);

            var service = new PropertyProvider(storageManager.Object);
            Assert.IsNotNull(service);

            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(UnknownPropertyException))]
        public void WhenGetUnknownPropertyExpectException()
        {
            var provider = Factory_CreateGamePropertyProvider();

            provider.GetProperty(Guid.NewGuid().ToString());
        }

        [TestMethod]
        public void WhenGetKnownPropertyExpectSuccess()
        {
            var provider = Factory_CreateGamePropertyProvider();

            var gameId = provider.GetProperty(GamingConstants.SelectedGameId);

            Assert.AreEqual(gameId, GameId);
        }

        [TestMethod]
        [ExpectedException(typeof(UnknownPropertyException))]
        public void WhenSetUnknownPropertyExpectException()
        {
            var provider = Factory_CreateGamePropertyProvider();

            provider.SetProperty(Guid.NewGuid().ToString(), null);
        }

        [TestMethod]
        public void WhenSetKnownPropertyExpectSuccess()
        {
            const int newGameId = 1;

            var provider = Factory_CreateGamePropertyProvider();

            provider.SetProperty(GamingConstants.SelectedGameId, newGameId);

            var current = provider.GetProperty(GamingConstants.SelectedGameId);

            Assert.AreEqual(current, newGameId);
        }

        [TestMethod]
        public void WhenSetNonPersistedPropertyExpectNotPersisted()
        {
            var storageManager = new Mock<IPersistentStorageManager>();
            var accessor = new Mock<IPersistentStorageAccessor>();

            var storageName = typeof(PropertyProvider).ToString();

            storageManager.Setup(m => m.BlockExists(It.Is<string>(s => s == storageName))).Returns(true);
            storageManager.Setup(m => m.GetBlock(It.Is<string>(s => s == storageName))).Returns(accessor.Object);

            accessor.Setup(a => a[GamingConstants.SelectedGameId]).Returns(GameId);
            accessor.Setup(a => a[GamingConstants.SelectedDenom]).Returns(Denom);

            var provider = new PropertyProvider(storageManager.Object);

            provider.SetProperty(GamingConstants.IsGameRunning, new object());

            accessor.VerifySet(a => a[GamingConstants.IsGameRunning] = It.IsAny<object>(), Times.Never());
        }

        [TestMethod]
        public void WhenSetGameIdExpectPersisted()
        {
            const int newGameId = 1;

            var storageManager = new Mock<IPersistentStorageManager>();
            var accessor = new Mock<IPersistentStorageAccessor>();

            var storageName = typeof(PropertyProvider).ToString();

            storageManager.Setup(m => m.BlockExists(It.Is<string>(s => s == storageName))).Returns(true);
            storageManager.Setup(m => m.GetBlock(It.Is<string>(s => s == storageName))).Returns(accessor.Object);

            accessor.Setup(a => a[GamingConstants.SelectedGameId]).Returns(GameId);
            accessor.Setup(a => a[GamingConstants.SelectedDenom]).Returns(Denom);

            var provider = new PropertyProvider(storageManager.Object);

            provider.SetProperty(GamingConstants.SelectedGameId, newGameId);

            accessor.VerifySet(a => a[GamingConstants.SelectedGameId] = newGameId);
        }

        [TestMethod]
        public void WhenSetDenomExpectPersisted()
        {
            const int newDenom = 5000;

            var storageManager = new Mock<IPersistentStorageManager>();
            var accessor = new Mock<IPersistentStorageAccessor>();

            var storageName = typeof(PropertyProvider).ToString();

            storageManager.Setup(m => m.BlockExists(It.Is<string>(s => s == storageName))).Returns(true);
            storageManager.Setup(m => m.GetBlock(It.Is<string>(s => s == storageName))).Returns(accessor.Object);

            accessor.Setup(a => a[GamingConstants.SelectedGameId]).Returns(GameId);
            accessor.Setup(a => a[GamingConstants.SelectedDenom]).Returns(Denom);

            var provider = new PropertyProvider(storageManager.Object);

            provider.SetProperty(GamingConstants.SelectedDenom, newDenom);

            accessor.VerifySet(a => a[GamingConstants.SelectedDenom] = newDenom);
        }

        [TestMethod]
        public void CheckGameStartMethodWhenBlockNotExists()
        {
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None)).Returns(ImportMachineSettings.None);
            var storageManager = new Mock<IPersistentStorageManager>();
            var accessor = new Mock<IPersistentStorageAccessor>();

            var storageName = typeof(PropertyProvider).ToString();

            storageManager.Setup(m => m.BlockExists(It.Is<string>(s => s == storageName))).Returns(false);
            storageManager.Setup(
                    m => m.CreateBlock(
                        It.Is<PersistenceLevel>(l => l == PersistenceLevel.Transient),
                        It.Is<string>(s => s == storageName),
                        It.Is<int>(size => size == 1)))
                .Returns(accessor.Object);
            var provider = new PropertyProvider(storageManager.Object);

            Assert.IsTrue(provider.GetCollection.Contains(new KeyValuePair<string, object>(GamingConstants.GameStartMethod, (int)GameStartMethodOption.Bet)));
            Assert.IsTrue(provider.GetCollection.Contains(new KeyValuePair<string, object>(GamingConstants.GameStartMethodConfigurable, false)));
            
            provider.SetProperty(GamingConstants.GameStartMethod, GameStartMethodOption.Bet);
            accessor.VerifySet(a => a[GamingConstants.GameStartMethod] = GameStartMethodOption.Bet);

            Assert.AreEqual(provider.GetProperty(GamingConstants.GameStartMethodConfigurable), false);

            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void CheckGameStartMethodWhenBlockExists()
        {
            var storageManager = new Mock<IPersistentStorageManager>();
            var accessor = new Mock<IPersistentStorageAccessor>();

            var storageName = typeof(PropertyProvider).ToString();

            storageManager.Setup(m => m.BlockExists(It.Is<string>(s => s == storageName))).Returns(true);
            storageManager.Setup(m => m.GetBlock(It.Is<string>(s => s == storageName))).Returns(accessor.Object);
            accessor.Setup(a => a[GamingConstants.GameStartMethod]).Returns(GameStartMethodOption.Bet);

            var provider = new PropertyProvider(storageManager.Object);

            Assert.IsTrue(provider.GetCollection.Contains(new KeyValuePair<string, object>(GamingConstants.GameStartMethod, GameStartMethodOption.Bet)));
            Assert.IsTrue(provider.GetCollection.Contains(new KeyValuePair<string, object>(GamingConstants.GameStartMethodConfigurable, false)));

            provider.SetProperty(GamingConstants.GameStartMethod, GameStartMethodOption.Bet);
            accessor.VerifySet(a => a[GamingConstants.GameStartMethod] = GameStartMethodOption.Bet);

            Assert.AreEqual(provider.GetProperty(GamingConstants.GameStartMethodConfigurable), false);
        }
        [DataRow(1000)]
        [DataRow(5000)]
        [TestMethod]
        public void CheckReelDurationWhenBlockNotExists(int reelDuration)
        {
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None)).Returns(ImportMachineSettings.None);
            var storageManager = new Mock<IPersistentStorageManager>();
            var accessor = new Mock<IPersistentStorageAccessor>();

            var storageName = typeof(PropertyProvider).ToString();

            storageManager.Setup(m => m.BlockExists(It.Is<string>(s => s == storageName))).Returns(false);
            storageManager.Setup(
                    m => m.CreateBlock(
                        It.Is<PersistenceLevel>(l => l == PersistenceLevel.Transient),
                        It.Is<string>(s => s == storageName),
                        It.Is<int>(size => size == 1)))
                .Returns(accessor.Object);
            var provider = new PropertyProvider(storageManager.Object);

            Assert.IsTrue(provider.GetCollection.Contains(new KeyValuePair<string, object>(GamingConstants.GameRoundDurationMs, GamingConstants.DefaultMinimumGameRoundDurationMs)));

            provider.SetProperty(GamingConstants.GameRoundDurationMs, reelDuration);
            accessor.VerifySet(a => a[GamingConstants.GameRoundDurationMs] = reelDuration);

            Assert.AreEqual(provider.GetProperty(GamingConstants.GameRoundDurationMs), reelDuration);

            MoqServiceManager.RemoveInstance();
        }

        [DataRow(1000)]
        [TestMethod]
        public void CheckReelDurationWhenBlockExists(int reelDuration)
        {
            var storageManager = new Mock<IPersistentStorageManager>();
            var accessor = new Mock<IPersistentStorageAccessor>();

            var storageName = typeof(PropertyProvider).ToString();

            storageManager.Setup(m => m.BlockExists(It.Is<string>(s => s == storageName))).Returns(true);
            storageManager.Setup(m => m.GetBlock(It.Is<string>(s => s == storageName))).Returns(accessor.Object);
            accessor.Setup(a => a[GamingConstants.GameRoundDurationMs]).Returns(GamingConstants.DefaultMinimumGameRoundDurationMs);

            var provider = new PropertyProvider(storageManager.Object);

            Assert.IsTrue(provider.GetCollection.Contains(new KeyValuePair<string, object>(GamingConstants.GameRoundDurationMs, GamingConstants.DefaultMinimumGameRoundDurationMs)));

            provider.SetProperty(GamingConstants.GameRoundDurationMs, reelDuration);
            accessor.VerifySet(a => a[GamingConstants.GameRoundDurationMs] = reelDuration);

            Assert.AreEqual(provider.GetProperty(GamingConstants.GameRoundDurationMs), reelDuration);
        }

        [DataRow(GamingConstants.PlayerInformationDisplay.Enabled, false)]
        [DataRow(GamingConstants.PlayerInformationDisplay.RestrictedModeUse, false)]
        [DataRow(GamingConstants.PlayerInformationDisplay.GameRulesScreenEnabled, false)]
        [DataRow(GamingConstants.PlayerInformationDisplay.PlayerInformationScreenEnabled, false)]
        [TestMethod]
        public void GivenPropertyProviderWhenInitThenPlayerInformationDisplayOptionsSetDefault(string name, bool expected)
        {
            _propertiesManager.Setup(mock => mock.GetProperty(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None))
                .Returns(ImportMachineSettings.None);
            var storageManager = new Mock<IPersistentStorageManager>();
            var storageName = typeof(PropertyProvider).ToString();
            storageManager.Setup(m => m.BlockExists(It.Is<string>(s => s == storageName)))
                .Returns(true);
            storageManager.Setup(m => m.GetBlock(It.Is<string>(s => s == storageName)))
                .Returns(Mock.Of<IPersistentStorageAccessor>());

            var provider = new PropertyProvider(storageManager.Object);

            Assert.AreEqual(expected, provider.GetProperty(name));

            MoqServiceManager.RemoveInstance();
        }


        private static PropertyProvider Factory_CreateGamePropertyProvider()
        {
            var storageManager = new Mock<IPersistentStorageManager>();
            var accessor = new Mock<IPersistentStorageAccessor>();

            var storageName = typeof(PropertyProvider).ToString();

            storageManager.Setup(m => m.BlockExists(It.Is<string>(s => s == storageName))).Returns(true);
            storageManager.Setup(m => m.GetBlock(It.Is<string>(s => s == storageName))).Returns(accessor.Object);

            accessor.Setup(a => a[GamingConstants.SelectedGameId]).Returns(GameId);
            accessor.Setup(a => a[GamingConstants.SelectedDenom]).Returns(Denom);

            return new PropertyProvider(storageManager.Object);
        }
    }
}
