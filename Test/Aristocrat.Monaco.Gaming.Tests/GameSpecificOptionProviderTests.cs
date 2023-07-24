namespace Aristocrat.Monaco.Gaming.Tests.GameSpecificOptions
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts.GameSpecificOptions;
    using Gaming.GameSpecificOptions;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GameSpecificOptionProviderTests
    {
        private const string ThemeId1 = "ThemeId1";
        private const string ThemeId2 = "ThemeId2";

        private readonly List<GameSpecificOption> _testData = new List<GameSpecificOption>
        {
            new GameSpecificOption
            {
                Name = "Option1",
                Value = "On",
                OptionType = OptionType.Toggle,
                MinValue = 0,
                MaxValue = 0,
                ValueSet = new List<string>{"On", "Off"}
            },
            new GameSpecificOption
            {
                Name = "Option2",
                Value = "Red",
                OptionType = OptionType.List,
                MinValue = 0,
                MaxValue = 0,
                ValueSet = new List<string>{"Red", "Blue", "Orange"}
            },
            new GameSpecificOption
            {
                Name = "Option3",
                Value = "1",
                OptionType = OptionType.Number,
                MinValue = 1,
                MaxValue = 10,
                ValueSet = new List<string>()
            }
        };

        private readonly List<GameSpecificOption> _updatedTestData = new List<GameSpecificOption>
        {
            new GameSpecificOption
            {
                Name = "Option1",
                Value = "Off",
                OptionType = OptionType.Toggle,
                MinValue = 0,
                MaxValue = 0,
                ValueSet = new List<string>{"On", "Off"}
            },
            new GameSpecificOption
            {
                Name = "Option2",
                Value = "Orange",
                OptionType = OptionType.List,
                MinValue = 0,
                MaxValue = 0,
                ValueSet = new List<string>{"Red", "Blue", "Orange"}
            },
            new GameSpecificOption
            {
                Name = "Option3",
                Value = "5",
                OptionType = OptionType.Number,
                MinValue = 1,
                MaxValue = 10,
                ValueSet = new List<string>()
            }
        };
        
        private GameSpecificOptionProvider _gameSpecificOptionProvider;
        
        private Mock<IPersistentStorageManager> _mockStorageManager;
        private Mock<IPersistenceProvider> _mockPersistenceProvider;
        private Mock<IPropertiesManager> _mockPropertiesManager;
        
        [TestInitialize]
        public void Init()
        {            
            Mock<IPersistentTransaction> _mockSaveTransaction = new Mock<IPersistentTransaction>();
            Mock<IPersistentBlock> _mockSaveBlock = new Mock<IPersistentBlock>();
            _mockSaveBlock.Setup( x => x.GetOrCreateValue<
                    ConcurrentDictionary<string, string>>(It.IsAny<string>()))
                    .Returns(new ConcurrentDictionary<string, string>());
            _mockSaveBlock.Setup(x => x.Transaction()).Returns(_mockSaveTransaction.Object);

            _mockPersistenceProvider = new Mock<IPersistenceProvider>();
            _mockPersistenceProvider.Setup(x => x.GetOrCreateBlock(
                    It.IsAny<string>(), PersistenceLevel.Static))
                    .Returns(_mockSaveBlock.Object);

            _mockPropertiesManager = new Mock<IPropertiesManager>();
            _mockPropertiesManager.Setup(p => p.GetProperty(It.IsAny<string>(), It.IsAny<object>()))
                    .Returns<string, object>((s, o) => o);

            _mockStorageManager = new Mock<IPersistentStorageManager>(MockBehavior.Strict);
            _mockStorageManager.Setup(m => m.ScopedTransaction()).Returns((new Mock<IScopedTransaction>()).Object);

            _gameSpecificOptionProvider = new GameSpecificOptionProvider(
                    _mockStorageManager.Object,
                    _mockPersistenceProvider.Object,
                    _mockPropertiesManager.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _gameSpecificOptionProvider.Dispose();
        }

        [TestMethod]
        public void AddGameSpecificOptionsWithNoDataTest()
        {
            _gameSpecificOptionProvider.InitGameSpecificOptionsCache(ThemeId1, Enumerable.Empty<GameSpecificOption>().ToList());
            Assert.AreEqual(0, _gameSpecificOptionProvider.GetGameSpecificOptions(ThemeId1).Count());
            var actualData = _gameSpecificOptionProvider.GetGameSpecificOptions(ThemeId1);
        }
        
        [TestMethod]
        public void AddGameSpecificOptionsWithTest()
        {
            _gameSpecificOptionProvider.InitGameSpecificOptionsCache(ThemeId1, _testData);
            Assert.IsTrue(_gameSpecificOptionProvider.HasThemeId(ThemeId1));
            Assert.IsFalse(_gameSpecificOptionProvider.HasThemeId(ThemeId2));
            Assert.AreEqual(3, _gameSpecificOptionProvider.GetGameSpecificOptions(ThemeId1).Count());
            var actualData = _gameSpecificOptionProvider.GetGameSpecificOptions(ThemeId1).ToList();
            VerifyData(_testData, actualData);
        }

        [TestMethod]
        public void UpdateGameSpecificOptionsTest()
        {
            _gameSpecificOptionProvider.InitGameSpecificOptionsCache(ThemeId1, _testData);
            _gameSpecificOptionProvider.UpdateGameSpecificOptionsCache(ThemeId1, _updatedTestData);
            Assert.IsTrue(_gameSpecificOptionProvider.HasThemeId(ThemeId1));
            Assert.IsFalse(_gameSpecificOptionProvider.HasThemeId(ThemeId2));
            Assert.AreEqual(3, _gameSpecificOptionProvider.GetGameSpecificOptions(ThemeId1).Count());
            var actualData = _gameSpecificOptionProvider.GetGameSpecificOptions(ThemeId1).ToList();
            VerifyData(_updatedTestData, actualData);
        }        

        private void VerifyData(List<GameSpecificOption> expectedData, List<GameSpecificOption> actualData)
        {
            for (var i = 0; i < actualData.Count; i++)
            {
                Assert.AreEqual(expectedData[i].Name, actualData[i].Name, "Invalid Name");
                Assert.AreEqual(expectedData[i].Value, actualData[i].Value, "Invalid Value");
                Assert.AreEqual(expectedData[i].OptionType, actualData[i].OptionType, "Invalid OptionType");
                Assert.AreEqual(expectedData[i].MinValue, actualData[i].MinValue, "Invalid MinValue");
                Assert.AreEqual(expectedData[i].MaxValue, actualData[i].MaxValue, "Invalid MaxValue");
                Assert.AreEqual(expectedData[i].ValueSet, actualData[i].ValueSet, "Invalid ValueSet");
            }
        }
    }
}