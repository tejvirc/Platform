namespace Aristocrat.Monaco.Application.Tests.EdgeLight.Handlers
{
    using System;
    using Application.EdgeLight;
    using Application.EdgeLight.Handlers;
    using Contracts;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class EdgeLightBrightnessHandlerTests
    {
        private const int MaxChannelBrightness = 100;
        private Mock<IEventBus> _eventBus;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IEdgeLightingController> _edgeLightController;
        private EdgeLightBrightnessHandler _edgeLightBrightnessHandler;
        private Action<MaximumOperatorBrightnessChangedEvent> _maximumBrightnessChangeAction;

        private void SetUpEventHandler()
        {
            _eventBus
                .Setup(
                    m => m.Subscribe(
                        It.IsAny<EdgeLightBrightnessHandler>(),
                        It.IsAny<Action<MaximumOperatorBrightnessChangedEvent>>()))
            .Callback<object, Action<MaximumOperatorBrightnessChangedEvent>>(
                    (edgeLightBrightnessHandler, callback) =>
                    {
                        _maximumBrightnessChangeAction = callback;
                    });
        }

        [TestInitialize]
        public void Initialize()
        {
            _eventBus = new Mock<IEventBus>(MockBehavior.Strict);
            _propertiesManager = new Mock<IPropertiesManager>();
            _edgeLightController = new Mock<IEdgeLightingController>(MockBehavior.Strict);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _eventBus.Setup(x => x.UnsubscribeAll(It.IsAny<EdgeLightBrightnessHandler>())).Verifiable();
            _edgeLightBrightnessHandler?.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventBusIsNullExpectException()
        {
            _edgeLightBrightnessHandler = new EdgeLightBrightnessHandler(
                null,
                _edgeLightController.Object,
                _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEdgeLightingControllerIsNullExpectException()
        {
            _edgeLightBrightnessHandler = new EdgeLightBrightnessHandler(
                _eventBus.Object,
                null,
                _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPropertiesManagerIsNullExpectException()
        {
            _edgeLightBrightnessHandler = new EdgeLightBrightnessHandler(
                _eventBus.Object,
                _edgeLightController.Object,
                null);
        }

        [TestMethod]
        public void DisposingObjectTwice()
        {
            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.EdgeLightingBrightnessControlEnabled, false))
                .Returns(false);
            _edgeLightBrightnessHandler = new EdgeLightBrightnessHandler(
                _eventBus.Object,
                _edgeLightController.Object,
                _propertiesManager.Object);
            _eventBus.Setup(x => x.UnsubscribeAll(It.IsAny<EdgeLightBrightnessHandler>())).Verifiable();
            _edgeLightBrightnessHandler?.Dispose();
            // To cover the if already Disposed case.
            _edgeLightBrightnessHandler?.Dispose();
            _eventBus.VerifyAll();
        }

        [TestMethod]
        public void CreatingDefault()
        {
            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.EdgeLightingBrightnessControlEnabled, false))
                .Returns(false);
            _edgeLightBrightnessHandler = new EdgeLightBrightnessHandler(
                _eventBus.Object,
                _edgeLightController.Object,
                _propertiesManager.Object);
            Assert.IsTrue(_edgeLightBrightnessHandler.Enabled == false);
            Assert.IsTrue(_edgeLightBrightnessHandler.Name == nameof(EdgeLightBrightnessHandler));
            _eventBus.Setup(x => x.UnsubscribeAll(It.IsAny<EdgeLightBrightnessHandler>())).Verifiable();
            _edgeLightBrightnessHandler?.Dispose();
            // To cover the if already Disposed case.
            _edgeLightBrightnessHandler?.Dispose();
        }

        [TestMethod]
        public void CreatingDefaultWithEnabledTrue()
        {
            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.EdgeLightingBrightnessControlEnabled, false))
                .Returns(true);
            _propertiesManager.Setup(
                    m => m.GetProperty(
                        ApplicationConstants.EdgeLightingBrightnessControlMin,
                        ApplicationConstants.DefaultEdgeLightingMinimumBrightness))
                .Returns(0);
            _propertiesManager
                .Setup(
                    n => n.GetProperty(ApplicationConstants.EdgeLightingBrightnessControlDefault, MaxChannelBrightness))
                .Returns(100);
            _propertiesManager
                .Setup(
                    m => m.GetProperty(
                        ApplicationConstants.MaximumAllowedEdgeLightingBrightnessKey, 100))
                .Returns(90);
            SetUpEventHandler();
            _edgeLightController.Setup(
                    x => x.SetBrightnessLimits(It.IsAny<EdgeLightingBrightnessLimits>(), It.IsAny<StripPriority>()))
                .Callback<EdgeLightingBrightnessLimits, StripPriority>(
                    (x, y) =>
                    {
                        Assert.IsTrue(x.MaximumAllowed == 90);
                        Assert.IsTrue(x.MinimumAllowed == 0);
                        Assert.IsTrue(y < StripPriority.DoorOpen);
                    });
            _edgeLightController.Setup(
                x => x.SetBrightnessForPriority(It.IsAny<int>(), It.IsAny<StripPriority>()));
            _edgeLightController.Setup(
                x => x.GetBrightnessLimits(It.IsAny<StripPriority>())).Returns(new EdgeLightingBrightnessLimits(){MaximumAllowed = 100, MinimumAllowed = 0});
            _edgeLightBrightnessHandler = new EdgeLightBrightnessHandler(
                _eventBus.Object,
                _edgeLightController.Object,
                _propertiesManager.Object);
            Assert.IsTrue(_edgeLightBrightnessHandler.Enabled);
            Assert.IsTrue(_edgeLightBrightnessHandler.Name == nameof(EdgeLightBrightnessHandler));
            _edgeLightController.Setup(
                    x => x.SetBrightnessLimits(It.IsAny<EdgeLightingBrightnessLimits>(), It.IsAny<StripPriority>()))
                .Callback<EdgeLightingBrightnessLimits, StripPriority>(
                    (x, y) =>
                    {
                        Assert.IsTrue(x.MaximumAllowed == 50);
                        Assert.IsTrue(x.MinimumAllowed == 0);
                        Assert.IsTrue(y <= StripPriority.AuditMenu);
                    });
            _maximumBrightnessChangeAction.Invoke(new MaximumOperatorBrightnessChangedEvent(50));
            _eventBus.Setup(x => x.UnsubscribeAll(It.IsAny<EdgeLightBrightnessHandler>())).Verifiable();
            _edgeLightBrightnessHandler?.Dispose();
            _eventBus.VerifyAll();
        }
    }
}
