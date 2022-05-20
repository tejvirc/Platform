namespace Aristocrat.Monaco.Gaming.UI.Tests.PlayerInfoDisplay
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Contracts.PlayerInfoDisplay;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using UI.PlayerInfoDisplay;

    [TestClass]
    public class GameResourcesModelProviderTests
    {
        private GameResourcesModelProvider _underTest;
        private Mock<IGameProvider> _gameProvider;
        private Mock<IPlayerInfoDisplayFeatureProvider> _playerInfoDisplayFeatureProvider;

        [TestInitialize]
        public void Setup()
        {
            _playerInfoDisplayFeatureProvider = new Mock<IPlayerInfoDisplayFeatureProvider>();
            _gameProvider = new Mock<IGameProvider>();
            _underTest = new GameResourcesModelProvider(
                _gameProvider.Object
                , _playerInfoDisplayFeatureProvider.Object
                );
        }

        [TestCleanup]
        public void TearDown()
        {
        }

        [TestMethod]
        public void NullConstructorTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new GameResourcesModelProvider(null, _playerInfoDisplayFeatureProvider.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new GameResourcesModelProvider(_gameProvider.Object, null));
        }

        [TestMethod]
        [DataRow("pid_menu,screen,background,bla;pid_menu,button,bla","path1;path2")]
        [DataRow("pid_menu,screen,background;pid_menu,button","path1;path2")]
        public void GivenTagsAndPathsWhenFindThenPIDResourcesSelected(string tag, string path)
        {
            var id = 2;
            var tags = tag.Split(';');
            var paths = path.Split(';');
            var resources = new List<(HashSet<string> Tags, string FilePath)>();
            for (int i = tags.Length - 1; i >= 0; i--)
            {
                var hashTable = tags[i].Split(',').ToHashSet();
                resources.Add((hashTable, paths[i]));
            }

            var game = new Mock<IGameDetail>();
            var locale = Guid.NewGuid().ToString();
            var graphics = Mock.Of<ILocaleGameGraphics>();
            graphics.PlayerInfoDisplayResources = resources;
            game.Setup(x => x.LocaleGraphics)
                .Returns(new Dictionary<string, ILocaleGameGraphics>() { { locale, graphics }  });
            _gameProvider.Setup(x => x.GetGame(id))
                .Returns(game.Object)
                .Verifiable();
            _playerInfoDisplayFeatureProvider.Setup(x => x.ActiveLocaleCode)
                .Returns(locale)
                .Verifiable();


            var result = _underTest.Find(id);

            Assert.IsTrue(result.ScreenBackgrounds.Count == 1 && result.ScreenBackgrounds.All(x => x.FilePath == "path1"));
            Assert.IsTrue(result.Buttons.Count == 1 && result.Buttons.All(x => x.FilePath == "path2"));

        }

        [TestMethod]
        public void GivenNoGameWhenFindThenEmptyModel()
        {
            var id = 2;
            _gameProvider.Setup(x => x.GetGame(id))
                .Returns((IGameDetail)null)
                .Verifiable();

            var result = _underTest.Find(id);

            Assert.IsFalse(result.ScreenBackgrounds.Any());
            Assert.IsFalse(result.Buttons.Any());

        }

        [TestMethod]
        public void GivenNoLocaleWhenFindThenEmptyModel()
        {
            var id = 2;

            var game = new Mock<IGameDetail>();
            var locale = Guid.NewGuid().ToString();
            _gameProvider.Setup(x => x.GetGame(id))
                .Returns(game.Object)
                .Verifiable();
            _playerInfoDisplayFeatureProvider.Setup(x => x.ActiveLocaleCode)
                .Returns(locale)
                .Verifiable();

            var result = _underTest.Find(id);

            Assert.IsFalse(result.ScreenBackgrounds.Any());
            Assert.IsFalse(result.Buttons.Any());
        }
    }
}