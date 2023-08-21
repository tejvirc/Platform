namespace Aristocrat.Monaco.Gaming.UI.Tests.PlayerInfoDisplay
{
    using Contracts.PlayerInfoDisplay;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Runtime;
    using SimpleInjector;
    using UI.PlayerInfoDisplay;

    [TestClass]
    public class PlayerInfoDisplayManagerFactoryTests
    {
        private Mock<IPlayerInfoDisplayFeatureProvider> _playerInfoDisplayFeatureProvider;
        private Mock<IPlayerInfoDisplayScreensContainer> _payerInfoDisplayScreensContainer;
        private Mock<IGameResourcesModelProvider> _gameResourcesModelProvider;
        private Mock<IEventBus> _eventBus;
        private Mock<IRuntime> _runtime;
        private Container _container;
        private PlayerInfoDisplayManagerFactory _underTest;

        [TestInitialize]
        public void Setup()
        {
            _container = new Container();
            _playerInfoDisplayFeatureProvider = new Mock<IPlayerInfoDisplayFeatureProvider>();
            _gameResourcesModelProvider = new Mock<IGameResourcesModelProvider>();
            _eventBus = new Mock<IEventBus>();
            _runtime = new Mock<IRuntime>();
            _container.Register(() => _eventBus.Object, Lifestyle.Singleton);
            _container.Register(() => _runtime.Object, Lifestyle.Singleton);
            _container.Register(() => _gameResourcesModelProvider.Object, Lifestyle.Singleton);;
            _payerInfoDisplayScreensContainer = new Mock<IPlayerInfoDisplayScreensContainer>();
            _underTest = new PlayerInfoDisplayManagerFactory(_container, _playerInfoDisplayFeatureProvider.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
        }

        [TestMethod]
        public void GivenIsPlayerInfoDisplaySupportedTrueWhenCreateThenDefaultManager()
        {
            _playerInfoDisplayFeatureProvider.Setup(x => x.IsPlayerInfoDisplaySupported).Returns(true).Verifiable();

            var result = _underTest.Create(_payerInfoDisplayScreensContainer.Object);

            Assert.IsInstanceOfType(result, typeof(DefaultPlayerInfoDisplayManager));
            _playerInfoDisplayFeatureProvider.Verify();
        }

        [TestMethod]
        public void GivenIsPlayerInfoDisplaySupportedFalseWhenCreateThenNotSupportedManager()
        {
            _playerInfoDisplayFeatureProvider.Setup(x => x.IsPlayerInfoDisplaySupported).Returns(false).Verifiable();

            var result = _underTest.Create(_payerInfoDisplayScreensContainer.Object);

            Assert.IsInstanceOfType(result, typeof(PlayerInfoDisplayNotSupportedManager));
            _playerInfoDisplayFeatureProvider.Verify();
        }
    }
}