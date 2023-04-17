namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GameOrderSettingsTests
    {
        private const string GameA = "aaa";
        private const string GameB = "bbb";
        private const string GameC = "ccc";
        private const string GameD = "ddd";
        private const string WasOperatorChangedField = "WasOperatorChanged";

        private IList<IGameInfo> _baseGames;
        // Note: Many of these tests expect a NullReferenceException because they call SaveGameOrder.
        // Setting up GameOrderSettings to have a working test storage accessor isn't necessary because
        // we aren't testing SaveGameOrder, just the SetGameOrder functionality before it.

        private GameOrderSettings _gameOrderSettings;

        [TestInitialize]
        public void Initialize()
        {
            var storage = new Mock<IPersistentStorageManager>();
            var bus = new Mock<IEventBus>();
            var accessor = new Mock<IPersistentStorageAccessor>();

            var blockName = typeof(GameOrderSettings).FullName + ".Data";

            accessor.Setup(m => m[WasOperatorChangedField]).Returns(false);
            storage.Setup(m => m.BlockExists(blockName)).Returns(true);
            storage.Setup(m => m.GetBlock(typeof(GameOrderSettings).FullName + ".Data")).Returns(accessor.Object);

            _gameOrderSettings = new GameOrderSettings(storage.Object, bus.Object)
            {
                IconOrder = new List<string> { GameA, GameB, GameC }
            };

            _baseGames = new List<IGameInfo>
            {
                new GameInfo { ThemeId = GameA },
                new GameInfo { ThemeId = GameB },
                new GameInfo { ThemeId = GameC }
            };
        }

        [TestMethod]
        public void ExpectCorrectInitialOrder()
        {
            Assert.AreEqual(_gameOrderSettings.IconOrder[0], GameA);
            Assert.AreEqual(_gameOrderSettings.IconOrder[1], GameB);
            Assert.AreEqual(_gameOrderSettings.IconOrder[2], GameC);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void SetGameOrderFromConfig_Success()
        {
            var config = new List<string> { GameB, GameC, GameA };

            _gameOrderSettings.IconOrder = new List<string>();
            _gameOrderSettings.SetIconOrderFromConfig(_baseGames, config);

            Assert.AreEqual(_gameOrderSettings.IconOrder[0], GameB);
            Assert.AreEqual(_gameOrderSettings.IconOrder[1], GameC);
            Assert.AreEqual(_gameOrderSettings.IconOrder[2], GameA);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void SetGameOrderFromConfigTwice_Success()
        {
            var config = new List<string> { GameB, GameC, GameA };

            _gameOrderSettings.IconOrder = new List<string>();
            _gameOrderSettings.SetIconOrderFromConfig(_baseGames, config);

            Assert.AreEqual(_gameOrderSettings.IconOrder[0], GameB);
            Assert.AreEqual(_gameOrderSettings.IconOrder[1], GameC);
            Assert.AreEqual(_gameOrderSettings.IconOrder[2], GameA);

            config = new List<string> { GameC, GameA, GameB };

            _gameOrderSettings.IconOrder = new List<string>();
            _gameOrderSettings.SetIconOrderFromConfig(_baseGames, config);

            Assert.AreEqual(_gameOrderSettings.IconOrder[0], GameC);
            Assert.AreEqual(_gameOrderSettings.IconOrder[1], GameA);
            Assert.AreEqual(_gameOrderSettings.IconOrder[2], GameB);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void SetGameOrder_Success()
        {
            var IconOrder = new List<string> { GameC, GameB, GameA };
            _gameOrderSettings.SetIconOrder(IconOrder, false);

            Assert.AreEqual(_gameOrderSettings.IconOrder[0], GameC);
            Assert.AreEqual(_gameOrderSettings.IconOrder[1], GameB);
            Assert.AreEqual(_gameOrderSettings.IconOrder[2], GameA);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void UpdatePositionPriorityToBeginningOfList_Success()
        {
            _gameOrderSettings.UpdateIconPositionPriority(GameB, 0);

            Assert.AreEqual(_gameOrderSettings.IconOrder[0], GameB);
            Assert.AreEqual(_gameOrderSettings.IconOrder[1], GameA);
            Assert.AreEqual(_gameOrderSettings.IconOrder[2], GameC);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void UpdatePositionPriorityToEndOfList_Success()
        {
            _gameOrderSettings.UpdateIconPositionPriority(GameB, 10);

            Assert.AreEqual(_gameOrderSettings.IconOrder[0], GameA);
            Assert.AreEqual(_gameOrderSettings.IconOrder[1], GameC);
            Assert.AreEqual(_gameOrderSettings.IconOrder[2], GameB);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void UpdatePositionPriorityForNewGame_Success()
        {
            _gameOrderSettings.UpdateIconPositionPriority(GameD, 2);

            Assert.AreEqual(_gameOrderSettings.IconOrder[0], GameA);
            Assert.AreEqual(_gameOrderSettings.IconOrder[1], GameD);
            Assert.AreEqual(_gameOrderSettings.IconOrder[2], GameB);
            Assert.AreEqual(_gameOrderSettings.IconOrder[3], GameC);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void OnGameAdded_ShouldAddToBeginningOfList()
        {
            _gameOrderSettings.OnGameAdded(GameD);

            Assert.AreEqual(_gameOrderSettings.IconOrder[0], GameD);
            Assert.AreEqual(_gameOrderSettings.IconOrder[1], GameA);
            Assert.AreEqual(_gameOrderSettings.IconOrder[2], GameB);
            Assert.AreEqual(_gameOrderSettings.IconOrder[3], GameC);
        }

        // TODO Add some more tests in LobbyViewModelTests:
        // OnGameRemoved_ShouldRemainInOrderList
        // OnGameRemovedAndReAdded_ShouldRemainInSamePosition

        private class GameInfo : IGameInfo
        {
            public string ThemeId { get; set; }
            public DateTime InstallDateTime { get; set; }
        }
    }
}
