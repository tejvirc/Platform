namespace Aristocrat.Monaco.Gaming.UI.Tests.ViewModels.OperatorMenu
{
    using Gaming.Contracts.Models;
    using System.Collections.Generic;
    using Application.Contracts.Localization;
    using Contracts;
    using Contracts.Progressives;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using UI.ViewModels.OperatorMenu;
    using Aristocrat.Monaco.Gaming.Progressives;
    using System.Globalization;
    using SimpleInjector;
    using Gaming.Contracts.Lobby;

    [TestClass]
    public class GamePreferencesViewModelTest
    {
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IGameProvider> _gameProvider;
        private Mock<ILocalizerFactory> _localizerFactory;
        private Mock<ILocalization> _localization;
        private Mock<IContainerService> _containerService;
        private Mock<ILobbyStateManager> _lobbyStateManager = new Mock<ILobbyStateManager>(MockBehavior.Default);
        private Mock<IProgressiveConfigurationProvider> _progressiveConfig;
        private Container _container;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _gameProvider = MoqServiceManager.CreateAndAddService<IGameProvider>(MockBehavior.Strict);
            _localizerFactory = MoqServiceManager.CreateAndAddService<ILocalizerFactory>(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _containerService = MoqServiceManager.CreateAndAddService<IContainerService>(MockBehavior.Loose);
            _progressiveConfig = MoqServiceManager.CreateAndAddService<IProgressiveConfigurationProvider>(MockBehavior.Loose);
            _localization = MoqServiceManager.CreateAndAddService<ILocalization>(MockBehavior.Strict);

            _gameProvider.Setup(g => g.GetGame(It.IsAny<int>())).Returns((IGameDetail)null);
            _gameProvider.Setup(g => g.GetAllGames()).Returns(new List<IGameDetail>());
            _gameProvider.Setup(g => g.GetEnabledGames()).Returns(new List<IGameDetail>());

            _localizerFactory.Setup(m => m.For(It.IsAny<string>())).Returns(new Mock<ILocalizer>().Object);

            _container = new Container();
            _container.Register(() => _gameProvider.Object, Lifestyle.Singleton);
            _container.Register(() => _lobbyStateManager.Object, Lifestyle.Singleton);

            _containerService.SetupGet(c => c.Container).Returns(_container);

            var playerCultureProvider = new Mock<IPlayerCultureProvider>();
            playerCultureProvider.SetupGet(p => p.AvailableCultures).Returns(new List<CultureInfo>()
            {
                new CultureInfo("en-US")
            });
            playerCultureProvider.SetupGet(p => p.LanguageOptions).Returns(new List<LanguageOption>()
            {
                new LanguageOption()
                {
                    Locale = "en-US",
                    Enabled = true,
                    IsMandatory = true
                }
            });

            _localization.Setup(l => l.GetProvider(It.IsAny<string>())).Returns(playerCultureProvider.Object);

            _propertiesManager.Setup(m => m.GetProperty(It.IsAny<string>(), It.IsAny<object>()))
                .Returns<string, object>((s, o) => o);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.AttractModeEnabled, It.IsAny<bool>()))
                .Returns(It.IsAny<bool>());
            _propertiesManager.Setup(
                    x => x.GetProperty(GamingConstants.ProgressiveLobbyIndicatorType, It.IsAny<object>()))
                .Returns(ProgressiveLobbyIndicator.Disabled);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void CheckTotalGameStartMethodsMatchesTotalOptionsInXsd()
        {
            var gamePrefViewModel = new GamePreferencesViewModel();
            var optionsToShow = new List<GameStartMethodOption>{ GameStartMethodOption.Bet, GameStartMethodOption.LineOrReel };
            Assert.AreEqual(optionsToShow.Count, gamePrefViewModel.GameStartMethods.Count);
        }

        [TestMethod]
        public void CheckGameStartMethodInfoIsSet()
        {
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.GameStartMethodConfigurable, false))
                .Returns(true);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.GameStartMethod, GameStartMethodOption.Bet))
                .Returns(GameStartMethodOption.LineOrReel);

            var gamePrefViewModel = new GamePreferencesViewModel();
            
            Assert.AreEqual(gamePrefViewModel.IsGameStartMethodConfigurable, true);
            Assert.AreEqual(gamePrefViewModel.GameStartMethod, GameStartMethodOption.LineOrReel);
        }

        [TestMethod]
        public void RouletteOptionsEnabled()
        {
            Mock<IGameDetail> game1 = new Mock<IGameDetail>(MockBehavior.Default);
            game1.Setup((g) => g.GameType).Returns(GameType.Roulette);
            List<IGameDetail> gameList = new List<IGameDetail>
            {
                game1.Object,
            };
            _gameProvider.Setup(g => g.GetAllGames()).Returns(gameList);

            var gamePrefViewModel = new GamePreferencesViewModel();
            Assert.AreEqual(gamePrefViewModel.RouletteOptionsEnabled, true);
        }

        [TestMethod]
        public void RouletteOptionsDisabled()
        {
            Mock<IGameDetail> game1 = new Mock<IGameDetail>(MockBehavior.Default);
            game1.Setup((g) => g.GameType).Returns(GameType.Blackjack);
            Mock<IGameDetail> game2 = new Mock<IGameDetail>(MockBehavior.Default);
            game2.Setup((g) => g.GameType).Returns(GameType.Slot);
            List<IGameDetail> gameList = new List<IGameDetail>
            {
                game1.Object,
                game2.Object
            };
            _gameProvider.Setup(g => g.GetAllGames()).Returns(gameList);

            var gamePrefViewModel = new GamePreferencesViewModel();
            Assert.AreEqual(gamePrefViewModel.RouletteOptionsEnabled, false);
        }
    }
}
