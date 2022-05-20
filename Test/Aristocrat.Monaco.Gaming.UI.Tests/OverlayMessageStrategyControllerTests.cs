namespace Aristocrat.Monaco.Gaming.UI.Tests
{
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Runtime;
    using Aristocrat.Monaco.Gaming.UI.CompositionRoot;
    using Aristocrat.Monaco.Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class OverlayMessageStrategyControllerTests
    {
        private Mock<IPropertiesManager> _properties;
        private Mock<IOverlayMessageStrategyFactory> _overlayMessageStrategyFactory;
        private Mock<IPresentationService> _presentationService;
        private Mock<ISystemDisableManager> _disableManager;

        [TestInitialize]
        public void TestInitialization()
        {
            _properties = new Mock<IPropertiesManager>(MockBehavior.Default);
            _overlayMessageStrategyFactory = new Mock<IOverlayMessageStrategyFactory>(MockBehavior.Default);
            _presentationService = new Mock<IPresentationService>(MockBehavior.Default);
            _disableManager = new Mock<ISystemDisableManager>(MockBehavior.Default);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void GameRegisterPresentationTest()
        {
            var registered = true;
            var presentationTypes = new PresentationOverrideTypes[2];

            presentationTypes[0] = PresentationOverrideTypes.PrintingCashoutTicket;
            presentationTypes[1] = PresentationOverrideTypes.PrintingCashwinTicket;

            var eventBus = new Mock<IEventBus>(MockBehavior.Default);
            var mockController = new Mock<IOverlayMessageStrategyController>(MockBehavior.Default);

            var gameControlledOverlayStrategy = new Mock<GameControlledOverlayMessageStrategy>(mockController.Object, _presentationService.Object);
            var enhancedOverlayStrategy = new Mock<EnhancedOverlayMessageStrategy>(_properties.Object, eventBus.Object, _disableManager.Object);

            _properties.Setup(r => r.GetProperty(ApplicationConstants.PlatformEnhancedDisplayEnabled, true)).Returns(true);
            _overlayMessageStrategyFactory.Setup(r => r.Create(OverlayMessageStrategyOptions.GameDriven)).Returns(gameControlledOverlayStrategy.Object);
            _overlayMessageStrategyFactory.Setup(r => r.Create(OverlayMessageStrategyOptions.Enhanced)).Returns(enhancedOverlayStrategy.Object);

            var controller = new OverlayMessageStrategyController(_overlayMessageStrategyFactory.Object, _properties.Object, _presentationService.Object);

            controller.RegisterPresentation(registered, presentationTypes);

            Assert.AreEqual(gameControlledOverlayStrategy.Object, controller.OverlayStrategy);
            Assert.AreEqual(enhancedOverlayStrategy.Object, controller.FallBackStrategy);
            Assert.IsTrue(controller.GameRegistered);
            for (var i = 0; i < presentationTypes.Length; i++)
            {
                Assert.AreEqual(presentationTypes[i], controller.RegisteredPresentations[i]);
            }
        }

        [TestMethod]
        public void NoGameRegistrationTest()
        {
            var registered = false;
            var presentationTypes = new PresentationOverrideTypes[0];

            var eventBus = new Mock<IEventBus>(MockBehavior.Default);

            var enhancedOverlayStrategy = new Mock<EnhancedOverlayMessageStrategy>(_properties.Object, eventBus.Object, _disableManager.Object);

            _properties.Setup(r => r.GetProperty(ApplicationConstants.PlatformEnhancedDisplayEnabled, true)).Returns(true);
            _overlayMessageStrategyFactory.Setup(r => r.Create(OverlayMessageStrategyOptions.Enhanced)).Returns(enhancedOverlayStrategy.Object);

            var controller = new OverlayMessageStrategyController(_overlayMessageStrategyFactory.Object, _properties.Object, _presentationService.Object);

            controller.RegisterPresentation(registered, presentationTypes);

            Assert.AreEqual(enhancedOverlayStrategy.Object, controller.OverlayStrategy);
            Assert.AreEqual(enhancedOverlayStrategy.Object, controller.FallBackStrategy);
            Assert.IsFalse(controller.GameRegistered);
            Assert.IsTrue(controller.RegisteredPresentations.Count == 0);
        }
    }
}
