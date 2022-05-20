namespace Aristocrat.Monaco.Application.Tests.EdgeLight.Handlers
{
    using System;
    using Application.EdgeLight.Handlers;
    using Contracts;
    using Contracts.EdgeLight;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class EdgeLightBottomStripHandlerTests
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IEdgeLightingController> _edgeLightController;
        private EdgeLightBottomStripHandler _edgeLightBottomStripHandler;
        private Action<BottomStripOffEvent> _bottomStripOffEventAction;
        private Action<BottomStripOnEvent> _bottomStripOnEventAction;

        private class EdgeLightToken : IEdgeLightToken
        {
            public EdgeLightToken(int id = 100)
            {
                Id = id;
            }
            public int Id { get; }
        }

        private void SetUpEventHandler()
        {
            _eventBus
                .Setup(
                    m => m.Subscribe(
                        It.IsAny<EdgeLightBottomStripHandler>(),
                        It.IsAny<Action<BottomStripOffEvent>>()))
                .Callback<object, Action<BottomStripOffEvent>>(
                    (edgeLightBottomStripHandler, callback) => { _bottomStripOffEventAction = callback; });

            _eventBus
                .Setup(
                    m => m.Subscribe(
                        It.IsAny<EdgeLightBottomStripHandler>(),
                        It.IsAny<Action<BottomStripOnEvent>>()))
                .Callback<object, Action<BottomStripOnEvent>>(
                    (edgeLightBottomStripHandler, callback) => { _bottomStripOnEventAction = callback; });
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
            _eventBus.Setup(x => x.UnsubscribeAll(It.IsAny<EdgeLightBottomStripHandler>())).Verifiable();
            _edgeLightBottomStripHandler?.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventBusIsNullExpectException()
        {
            _edgeLightBottomStripHandler = new EdgeLightBottomStripHandler(
                null,
                _edgeLightController.Object,
                _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEdgeLightingControllerIsNullExpectException()
        {
            _edgeLightBottomStripHandler = new EdgeLightBottomStripHandler(
                _eventBus.Object,
                null,
                _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPropertiesManagerIsNullExpectException()
        {
            _edgeLightBottomStripHandler = new EdgeLightBottomStripHandler(
                _eventBus.Object,
                _edgeLightController.Object,
                null);
        }

        [TestMethod]
        public void CreatingDefaultWithEnabledFalse()
        {
            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.BottomStripEnabled, false))
                .Returns(false);
            _edgeLightBottomStripHandler = new EdgeLightBottomStripHandler(
                _eventBus.Object,
                _edgeLightController.Object,
                _propertiesManager.Object);
            Assert.IsTrue(_edgeLightBottomStripHandler.Enabled == false);
            Assert.IsTrue(_edgeLightBottomStripHandler.Name == nameof(EdgeLightBottomStripHandler));
            _eventBus.Setup(x => x.UnsubscribeAll(It.IsAny<EdgeLightBottomStripHandler>())).Verifiable();
            _edgeLightBottomStripHandler?.Dispose();
            // To cover the if already Disposed case.
            _edgeLightBottomStripHandler?.Dispose();
            _eventBus.VerifyAll();
        }

        [TestMethod]
        public void CreatingDefaultWithEnabledTrue()
        {
            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.BottomStripEnabled, false))
                .Returns(true);
            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.BottomEdgeLightingOnKey, false))
                .Returns(false);
            SetUpEventHandler();
            _edgeLightController.Setup(
                x => x.AddEdgeLightRenderer(
                    It.IsAny<SolidColorPatternParameters>())).Returns(new EdgeLightToken(50));
            _edgeLightBottomStripHandler = new EdgeLightBottomStripHandler(
                _eventBus.Object,
                _edgeLightController.Object,
                _propertiesManager.Object);
            Assert.IsTrue(_edgeLightBottomStripHandler.Enabled);
            Assert.IsTrue(_edgeLightBottomStripHandler.Name == nameof(EdgeLightBottomStripHandler));
            _edgeLightController.Setup(
                x => x.AddEdgeLightRenderer(
                    It.IsAny<SolidColorPatternParameters>())).Returns(new EdgeLightToken(50));
            _edgeLightController.Setup(
                x => x.RemoveEdgeLightRenderer(It.IsAny<IEdgeLightToken>())).Callback<IEdgeLightToken>(
                (edgeLightToken) =>
                {
                    Assert.IsTrue(edgeLightToken.Id == 50);
                });
            _bottomStripOnEventAction.Invoke(new BottomStripOnEvent());
            _bottomStripOffEventAction.Invoke(new BottomStripOffEvent());
        }
    }
}
