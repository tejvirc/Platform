namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Contracts;
    using Contracts.Models;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;
    using Vgt.Client12.Application.OperatorMenu;

    [TestClass]
    public class AttractConfigurationProviderTests
    {
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
        private readonly Mock<IPersistentStorageManager> _storageManager = new Mock<IPersistentStorageManager>(MockBehavior.Default);
        private readonly Mock<IPersistentStorageAccessor> _block = new Mock<IPersistentStorageAccessor>(MockBehavior.Default);
        private readonly Mock<IGameProvider> _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);
        private readonly Mock<IGameOrderSettings> _gameOrder = new Mock<IGameOrderSettings>(MockBehavior.Default);
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>(MockBehavior.Default);

        private readonly Mock<IPersistentStorageTransaction> _storageTransaction =
            new Mock<IPersistentStorageTransaction>(MockBehavior.Strict);

        private List<IAttractInfo> _attractInfo;
        private List<IGameDetail> _gameDetail;

        private AttractConfigurationProvider _target;

        private readonly IDictionary<int, Dictionary<string, object>> _keyValuePairs =
            new Dictionary<int, Dictionary<string, object>>();

        private Action<GameEnabledEvent> _gameEnabledHandler;
        private Action<GameDisabledEvent> _gameDisabledHandler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            MoqServiceManager.CreateAndAddService<IOperatorMenuLauncher>(MockBehavior.Strict);

            _gameDetail = MockGameInfo.GetMockGameDetailInfo().ToList();
            _gameOrder.Setup(x => x.GetPositionPriority(It.IsAny<string>()))
                .Returns((string theme) => _gameDetail.FindIndex(x => x.ThemeId == theme));
            _gameProvider.Setup(g => g.GetEnabledGames()).Returns(_gameDetail.Where(g => g.Enabled).ToList());
            _attractInfo = MockGameInfo.GetMockAttractInfo().ToList();

            _propertiesManager.Setup(p => p.GetProperty(It.IsAny<string>(), It.IsAny<object>()))
                .Returns<string, object>((s, o) => o);
            _propertiesManager.Setup(p => p.SetProperty(It.IsAny<string>(), It.IsAny<object>()));

            _block.Setup(m => m.StartTransaction()).Returns(_storageTransaction.Object);
            _block.Setup(b => b.GetAll()).Returns(_keyValuePairs);
            _block.Setup(b => b.Count).Returns(_keyValuePairs.Count);

            _storageManager.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(true);
            _storageManager.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_block.Object);
            _storageManager.Setup(s => s.ResizeBlock(It.IsAny<string>(), It.IsAny<int>()));

            _storageTransaction.Setup(st => st.Commit());
            _storageTransaction.Setup(st => st.Dispose());

            _target = CreateProvider();
        }

        [TestCleanup]
        public void MyTestCleanup()
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
        }

        [DataRow(true, false, false, false, false)]
        [DataRow(false, true, false, false, false)]
        [DataRow(false, false, true, false, false)]
        [DataRow(false, false, false, true, false)]
        [DataRow(false, false, false, false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorTest(
            bool nullStorage,
            bool nullProperties,
            bool nullGameProvider,
            bool nullGameOrder,
            bool nullEventBus)
        {
            _target = CreateProvider(nullStorage, nullProperties, nullGameProvider, nullGameOrder, nullEventBus);
        }

        [TestMethod]
        public void WhenBlockIsFirstCreatedEnsureAttractInfoIsLoadedFromEnabledGames()
        {
            // When no games enabled, no attract sequence
            _gameProvider.Setup(g => g.GetEnabledGames()).Returns(new List<IGameDetail>());
            var attSeq = _target.GetAttractSequence().ToList();
            Assert.IsFalse(attSeq.Any()); // No data in storage

            _gameProvider.Setup(g => g.GetEnabledGames()).Returns(_gameDetail.Where(g => g.Enabled).ToList());

            // Ensure games are selected
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.SlotAttractSelected, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.KenoAttractSelected, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.PokerAttractSelected, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.BlackjackAttractSelected, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.RouletteAttractSelected, It.IsAny<bool>()))
                .Returns(true);

            // block is empty
            _block.Setup(b => b.Count).Returns(0);

            _target.Initialize();

            var enabledGames = _gameDetail.Where(g => g.Enabled).ToList();
            SetupTest(enabledGames);

            var expectedAttractSequence = GetAttractFromEnabledGames(_gameDetail).ToList();
            ResetSequenceNumbers(expectedAttractSequence);

            // Simulate storage block
            SetupStorage(expectedAttractSequence);

            // Accessing attract sequence here, will select initial list from enabled games.
            var attractSequence = _target.GetAttractSequence().ToList();

            // Attract sequence count is equal to enabled games count
            Assert.AreEqual(enabledGames.Count, attractSequence.Count);

            // All elements in attract sequence are from enabled games at this point
            foreach (var ai in attractSequence)
            {
                Assert.IsNotNull(enabledGames.Single(g => g.ThemeId == ai.ThemeId));
            }
        }

        [TestMethod]
        public void WhenBlockExistsEnsureAttractInfoIsLoadedFromStorage()
        {
            var enabledGames = _gameDetail.Where(game => game.Enabled).ToList();

            var expectedAttractSequence = GetAttractFromEnabledGames(_gameDetail).ToList();
            // Simulate storage block
            SetupStorage(expectedAttractSequence);

            _target.Initialize();

            var attractSequence = _target.GetAttractSequence().ToList();

            // Attract sequence retrieved count is equal to data from storage
            // This will include the attracts which are not selected so that we can
            // display them on configuration page for selection.
            Assert.AreEqual(expectedAttractSequence.ToList().Count, attractSequence.Count());

            // Attract sequence retrieved doesn't contain disabled games.
            foreach (var ai in attractSequence)
            {
                Assert.IsTrue(
                    enabledGames.Any(
                        g => g.ThemeId == ai.ThemeId)); // Sequence retrieved doesn't contain disabled games.
                Assert.IsNotNull(
                    _attractInfo.Single(g => g.ThemeId == ai.ThemeId)); // Sequence retrieved is from storage.
            }
        }

        private IEnumerable<IAttractInfo> GetAttractFromEnabledGames(IEnumerable<IGameDetail> gameDetails)
        {
            var enabledGames = gameDetails.Where(game => game.Enabled).ToList();

            var attractSequence = enabledGames.Join(
                _attractInfo,
                detail => new { ThemeId = detail.ThemeId, detail.GameType },
                info => new { info.ThemeId, info.GameType },
                (detail, info) => info).ToList();

            ResetSequenceNumbers(attractSequence);

            return attractSequence;
        }

        private void ResetSequenceNumbers(IEnumerable<IAttractInfo> attractSequence)
        {
            var count = 0;
            foreach (var ai in attractSequence)
            {
                ai.SequenceNumber = ++count;
            }
        }

        [TestMethod]
        public void WhenBlockExistsEnsureModifiedAttractInfoIsSavedSuccessfully()
        {
            SetupStorage(_attractInfo);

            _target.Initialize();

            var attractSequence = _target.GetAttractSequence().ToList();

            // Change selection
            if (attractSequence.Any(g => g.GameType == GameType.Slot))
            {
                if (attractSequence.Any(g => g.GameType == GameType.Slot && g.IsSelected))
                {
                    // Disable all Slot games
                    foreach (var ai in attractSequence.Where(ai => ai.GameType == GameType.Slot))
                    {
                        ai.IsSelected = false;
                    }
                }
            }

            SetupStorage(attractSequence);

            // Save new sequence
            _target.SaveAttractSequence(attractSequence);
            _propertiesManager
                .Setup(p => p.GetProperty(GamingConstants.DefaultAttractSequenceOverridden, It.IsAny<bool>()))
                .Returns(true);
            // Expected attract sequence
            var listSelected = attractSequence.Where(ai => ai.IsSelected);
            var listUnSelected = attractSequence.Where(ai => !ai.IsSelected);
            var expectedSavedAttractSequence = listSelected.ToList();
            expectedSavedAttractSequence.AddRange(listUnSelected);
            ResetSequenceNumbers(expectedSavedAttractSequence);

            //Get saved attract sequence
            var savedAttractSequence = _target.GetAttractSequence().ToList();

            _storageTransaction.Verify();

            Assert.AreEqual(expectedSavedAttractSequence.Count, savedAttractSequence.Count());

            Assert.IsTrue(Enumerable.SequenceEqual(expectedSavedAttractSequence, savedAttractSequence, new AttractComparer()));
            
        }

        [TestMethod]
        public void WhenNewGamesAreEnabledEnsureTheyAreAddedToAttractInfoForDisplay()
        {

            var attractSequence = GetAttractFromEnabledGames(_gameDetail).ToList();

            // Simulate storage block
            SetupStorage(attractSequence);

            _target.Initialize();

            attractSequence = _target.GetAttractSequence().ToList();

            var newGame = new MockGameInfo
            {
                Id = 1000,
                GameType = GameType.Keno,
                ThemeId = "ThemeIdDummy",
                ThemeName = "ThemeNameDummy",
                Active = true
            };
            // Add new enabled game.
            _gameDetail.Add(newGame);

            _gameProvider.Setup(g => g.GetEnabledGames()).Returns(_gameDetail.Where(g => g.Enabled).ToList());

            // Expected attract sequence
            var expectedAttractSequence = attractSequence.ToList();
            expectedAttractSequence.Add(new AttractInfo
            {
                ThemeId = newGame.ThemeId,
                GameType = newGame.GameType,
                IsSelected = true
            });
            ResetSequenceNumbers(expectedAttractSequence);
            // Simulate storage block
            SetupStorage(expectedAttractSequence);

            // Newly saved attract sequence
            var updatedAttractSequence = _target.GetAttractSequence().ToList();

            Assert.AreEqual(attractSequence.Count() + 1, updatedAttractSequence.Count());
            Assert.AreEqual(expectedAttractSequence.Count(), updatedAttractSequence.Count());

            _propertiesManager.Verify();
        }

        [TestMethod]
        public void WhenExistingGameIsDisabledAndNewGamesAreEnabledEnsureProperAttractSequenceIsReturned()
        {
            //1 . Ensure current attract sequence
            var currentAttractSequence = GetAttractFromEnabledGames(_gameDetail).ToList();

            // Simulate storage block
            SetupStorage(currentAttractSequence);

            _target.Initialize();

            var savedSequence = _target.GetAttractSequence().ToList();

            Assert.IsTrue(Enumerable.SequenceEqual(currentAttractSequence, savedSequence, new AttractComparer()));

            //2. Add new game
            var newGame = new MockGameInfo
            {
                Id = 1000,
                GameType = GameType.Keno,
                ThemeId = "ThemeIdDummy",
                ThemeName = "ThemeNameDummy",
                Active = true
            };
            _gameDetail.Add(newGame);

            //3. Disable existing game
            var gameDetails = _gameDetail.ToList();
            var disabledGame = (MockGameInfo)gameDetails.FirstOrDefault();
            if (disabledGame != null)
            {
                disabledGame.Active = false;
            }

            _gameProvider.Setup(g => g.GetEnabledGames()).Returns(gameDetails.Where(g => g.Enabled).ToList());

            // Expected attract sequence
            var expectedAttractSequence = currentAttractSequence.ToList();
            expectedAttractSequence.Add(new AttractInfo
            {
                ThemeId = newGame.ThemeId,
                GameType = newGame.GameType,
                IsSelected = true
            });
            var removedItem = expectedAttractSequence.FirstOrDefault(g => g.ThemeId == disabledGame.ThemeId);
            expectedAttractSequence.Remove(removedItem);

            ResetSequenceNumbers(expectedAttractSequence);

            // Simulate storage block
            SetupStorage(expectedAttractSequence);

            //4. Verify newly saved attract sequence
            var updatedAttractSequence = _target.GetAttractSequence().ToList();

            Assert.AreEqual(currentAttractSequence.Count(), updatedAttractSequence.Count());
            Assert.AreEqual(expectedAttractSequence.Count(), updatedAttractSequence.Count());

            _propertiesManager.Verify();
        }

        [TestMethod]
        public void WhenDefaultSequenceNotOverridenAndGameIsEnabledAttractInfoIsUpdatedSuccessfully()
        {
            SetupEvents();

            var currentAttractSequence = GetAttractFromEnabledGames(_gameDetail).ToList();

            // Simulate storage block
            SetupStorage(currentAttractSequence);

            _target = CreateProvider();

            _target.Initialize();

            Assert.IsNotNull(_gameEnabledHandler);
            Assert.IsNotNull(_gameDisabledHandler);

            var originalSequence = _target.GetAttractSequence().ToList();

            Assert.IsTrue(_gameDetail.Any(g => !g.Enabled));

            // Enable a game
            var newEnabledGame = (MockGameInfo)_gameDetail.First(g => !g.Enabled);
            newEnabledGame.Active = true;

            // Ensure the game is enabled
            var game = _gameDetail.First(g => g.Id == newEnabledGame.Id);
            Assert.IsTrue(game.Enabled);

            var expectedSavedAttractSequence = originalSequence.ToList();
            expectedSavedAttractSequence.Add(new AttractInfo
            {
                ThemeId = game.ThemeId,
                GameType = game.GameType,
                IsSelected = true
            });
            
            expectedSavedAttractSequence = expectedSavedAttractSequence.OrderBy(g => g.ThemeId).ToList();
            ResetSequenceNumbers(expectedSavedAttractSequence);

            SetupStorage(expectedSavedAttractSequence);

            // Default sequence not overriden
            _propertiesManager
                .Setup(p => p.GetProperty(GamingConstants.DefaultAttractSequenceOverridden, It.IsAny<bool>()))
                .Returns(false);

            // Send game enabled event
            _gameEnabledHandler(new GameEnabledEvent(newEnabledGame.Id, GameStatus.None, newEnabledGame.ThemeId));
            _gameProvider.Setup(g => g.GetEnabledGames()).Returns(_gameDetail.Where(g => g.Enabled).ToList());

            var newSequence = _target.GetAttractSequence().ToList();

            _storageTransaction.Verify();

            Assert.AreEqual(expectedSavedAttractSequence.Count, newSequence.Count());
            Assert.IsTrue(expectedSavedAttractSequence.SequenceEqual(newSequence, new AttractComparer()));

            _eventBus.Verify();
        }

        [TestMethod]
        public void WhenDefaultSequenceNotOverridenAndGameIsDisabledAttractInfoIsUpdatedSuccessfully()
        {
            SetupEvents();

            var currentAttractSequence = GetAttractFromEnabledGames(_gameDetail).ToList();

            // Simulate storage block
            SetupStorage(currentAttractSequence);

            _target = CreateProvider();

            _target.Initialize();

            Assert.IsNotNull(_gameDisabledHandler);

            var originalSequence = _target.GetAttractSequence().ToList();

            Assert.IsTrue(_gameDetail.Any(g => g.Enabled));

            // Disable a game
            var newDisabledGame = (MockGameInfo)_gameDetail.First(g => g.Enabled);
            newDisabledGame.Active = false;

            // Ensure the game is disabled
            var game = _gameDetail.First(g => g.Id == newDisabledGame.Id);
            Assert.IsFalse(game.Enabled);

            // Created expected result
            var expectedSavedAttractSequence = originalSequence.ToList();
            var removeFromAttract = originalSequence.First(g => g.ThemeId == newDisabledGame.ThemeId);
            expectedSavedAttractSequence.Remove(removeFromAttract);
            ResetSequenceNumbers(expectedSavedAttractSequence);

            SetupStorage(expectedSavedAttractSequence);

            // Default sequence not overriden
            _propertiesManager
                .Setup(p => p.GetProperty(GamingConstants.DefaultAttractSequenceOverridden, It.IsAny<bool>()))
                .Returns(false);

            // Send game disabled event
            _gameDisabledHandler(new GameDisabledEvent(newDisabledGame.Id, GameStatus.None, newDisabledGame.ThemeId));

            _gameProvider.Setup(g => g.GetEnabledGames()).Returns(_gameDetail.Where(g => g.Enabled).ToList());

            var newSequence = _target.GetAttractSequence().ToList();

            _storageTransaction.Verify();

            Assert.AreEqual(expectedSavedAttractSequence.Count, newSequence.Count());
            Assert.IsTrue(expectedSavedAttractSequence.SequenceEqual(newSequence, new AttractComparer()));

            _eventBus.Verify();
        }

        [TestMethod]
        public void WhenDefaultSequenceOverridenAndGameIsEnabledAttractInfoIsUpdatedSuccessfully()
        {
            SetupEvents();

            var currentAttractSequence = GetAttractFromEnabledGames(_gameDetail).ToList();

            // Simulate storage block
            SetupStorage(currentAttractSequence);

            _target = CreateProvider();

            _target.Initialize();

            Assert.IsNotNull(_gameEnabledHandler);
            Assert.IsNotNull(_gameDisabledHandler);

            var originalSequence = _target.GetAttractSequence();

            Assert.IsTrue(_gameDetail.Any(g => !g.Enabled));

            // Enable a game
            var newEnabledGame = (MockGameInfo)_gameDetail.First(g => !g.Enabled);
            newEnabledGame.Active = true;

            // Ensure the game is enabled
            var game = _gameDetail.First(g => g.Id == newEnabledGame.Id);
            Assert.IsTrue(game.Enabled);

            var expectedSavedAttractSequence = originalSequence.ToList();
            expectedSavedAttractSequence.Add(new AttractInfo
            {
                ThemeId = game.ThemeId,
                GameType = game.GameType,
                IsSelected = true
            });

            ResetSequenceNumbers(expectedSavedAttractSequence);

            SetupStorage(expectedSavedAttractSequence);

            _propertiesManager
                .Setup(p => p.GetProperty(GamingConstants.DefaultAttractSequenceOverridden, It.IsAny<bool>()))
                .Returns(true);
            _gameProvider.Setup(g => g.GetEnabledGames()).Returns(_gameDetail.Where(g => g.Enabled).ToList());

            // Send game enabled event
            _gameEnabledHandler(new GameEnabledEvent(newEnabledGame.Id, GameStatus.None, newEnabledGame.ThemeId));
            
            var newSequence = _target.GetAttractSequence().ToList();

            _storageTransaction.Verify();
            
            Assert.AreEqual(expectedSavedAttractSequence.Count, newSequence.Count());
            Assert.IsTrue(expectedSavedAttractSequence.SequenceEqual(newSequence, new AttractComparer()));

            _eventBus.Verify();
        }

        [TestMethod]
        public void WhenDefaultSequenceOverridenAndGameIsDisabledAttractInfoIsUpdatedSuccessfully()
        {
            SetupEvents();

            var currentAttractSequence = GetAttractFromEnabledGames(_gameDetail).ToList();

            // Simulate storage block
            SetupStorage(currentAttractSequence);

            _target = CreateProvider();

            _target.Initialize();

            Assert.IsNotNull(_gameDisabledHandler);

            var originalSequence = _target.GetAttractSequence().ToList();

            Assert.IsTrue(_gameDetail.Any(g => g.Enabled));

            // Disable a game
            var newDisabledGame = (MockGameInfo)_gameDetail.First(g => g.Enabled);
            newDisabledGame.Active = false;

            // Ensure the game is disabled
            var game = _gameDetail.First(g => g.Id == newDisabledGame.Id);
            Assert.IsFalse(game.Enabled);

            // Created expected result
            var expectedSavedAttractSequence = originalSequence.ToList();
            var removeFromAttract = originalSequence.First(g => g.ThemeId == newDisabledGame.ThemeId);
            expectedSavedAttractSequence.Remove(removeFromAttract);
            ResetSequenceNumbers(expectedSavedAttractSequence);

            SetupStorage(expectedSavedAttractSequence);

            _propertiesManager
                .Setup(p => p.GetProperty(GamingConstants.DefaultAttractSequenceOverridden, It.IsAny<bool>()))
                .Returns(true);
            // Send game disabled event
            _gameDisabledHandler(new GameDisabledEvent(newDisabledGame.Id, GameStatus.None, newDisabledGame.ThemeId));

            var test = _gameDetail.Where(g => g.Enabled).ToList();
            _gameProvider.Setup(g => g.GetEnabledGames()).Returns(_gameDetail.Where(g => g.Enabled).ToList());

            var newSequence = _target.GetAttractSequence().ToList();

            _storageTransaction.Verify();
            
            Assert.AreEqual(expectedSavedAttractSequence.Count, newSequence.Count());
            Assert.IsTrue(expectedSavedAttractSequence.SequenceEqual(newSequence, new AttractComparer()));

            _eventBus.Verify();
        }

        private void SetupEvents()
        {
            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<object>(),
                        It.IsAny<Action<GameEnabledEvent>>()))
                .Callback<object, Action<GameEnabledEvent>>((y, x) => _gameEnabledHandler = x).Verifiable();

            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<object>(),
                        It.IsAny<Action<GameDisabledEvent>>()))
                .Callback<object, Action<GameDisabledEvent>>((y, x) => _gameDisabledHandler = x).Verifiable();

            _eventBus.Setup(x => x.UnsubscribeAll(It.IsAny<AttractConfigurationProvider>()));
        }
        private void SetupStorage(IEnumerable<IAttractInfo> attractInfo)
        {
            if (attractInfo == null)
            {
                return;
            }

            _keyValuePairs.Clear();

            var currentIndex = 1;

            foreach (var ai in attractInfo)
            {
                
                var myDict = new Dictionary<string, object>
                {
                    { "AttractGame.GameTypeName", ai.GameType },
                    { "AttractGame.GameThemeId", ai.ThemeId },
                    { "AttractGame.SequenceNumber", ai.SequenceNumber },
                    { "AttractGame.GameIsSelected", ai.IsSelected },
                };
                _storageTransaction.SetupSet(st => st[currentIndex, "AttractGame.GameTypeName"] = (int)ai.GameType);
                _storageTransaction.SetupSet(st => st[currentIndex, "AttractGame.GameThemeId"] = ai.ThemeId);
                _storageTransaction.SetupSet(st => st[currentIndex, "AttractGame.SequenceNumber"] = ai.SequenceNumber);
                _storageTransaction.SetupSet(st => st[currentIndex, "AttractGame.GameIsSelected"] = ai.IsSelected);

                _keyValuePairs.Add(new KeyValuePair<int, Dictionary<string, object>>(currentIndex, myDict));

                currentIndex++;
            }

            _block.Setup(b => b.Count).Returns(_keyValuePairs.Count);
            _block.Setup(b => b.GetAll()).Returns(_keyValuePairs);
        }

        private void SetupTest(IEnumerable<IGameDetail> enabledGames, bool selected = true)
        {
            var index = 0;
            foreach (var ai in enabledGames)
            {
                var currentIndex = index;
                _storageTransaction.SetupSet(st => st[currentIndex, "AttractGame.TypeName"] = (int)ai.GameType);
                _storageTransaction.SetupSet(st => st[currentIndex, "AttractGame.ThemeName"] = ai.ThemeName);
                _storageTransaction.SetupSet(st => st[currentIndex, "AttractGame.ThemeId"] = ai.ThemeId);
                _storageTransaction.SetupSet(st => st[currentIndex, "AttractGame.GameIsSelected"] = selected);
                ++index;
            }
        }

        private AttractConfigurationProvider CreateProvider(
            bool nullStorage = false,
            bool nullProperties = false,
            bool nullGameProvider = false,
            bool nullGameOrder = false,
            bool nullEventBus = false)
        {
            return new AttractConfigurationProvider(
                nullStorage ? null : _storageManager.Object,
                nullProperties ? null : _propertiesManager.Object,
                nullGameProvider ? null : _gameProvider.Object,
                nullGameOrder ? null : _gameOrder.Object,
                nullEventBus ? null : _eventBus.Object);
        }

    private class AttractComparer : IEqualityComparer<IAttractInfo>
        {
            public bool Equals(IAttractInfo x, IAttractInfo y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x is null || y is null)
                {
                    return false;
                }

                return x.GameType == y.GameType && x.IsSelected == y.IsSelected && x.ThemeId == y.ThemeId
                       && x.SequenceNumber == y.SequenceNumber;
            }

            public int GetHashCode(IAttractInfo obj)
            {
                return (obj.GameType, obj.ThemeId, obj.IsSelected, obj.SequenceNumber).GetHashCode();
            }
        }
    }
}