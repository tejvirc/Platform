namespace Aristocrat.Monaco.Application.Tests.EdgeLight.Handlers
{
    using System;
    using System.Collections.Generic;
    using Application.EdgeLight.Handlers;
    using Contracts.EdgeLight;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class EdgeLightAuditMenuHandlerTests
    {
        private Mock<IEventBus> _eventBus;
        private Mock<ISystemDisableManager> _systemDisableManager;
        private Mock<IEdgeLightingStateManager> _edgLightStateManager;
        private EdgeLightAuditMenuHandler _edgeLightAuditMenuHandler;
        private Action<OperatorMenuEnteredEvent> _operatorMenuEnteredEventAction;
        private Action<OperatorMenuExitedEvent> _operatorMenuExitedEventAction;
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
                        It.IsAny<EdgeLightAuditMenuHandler>(),
                        It.IsAny<Action<OperatorMenuEnteredEvent>>()))
                .Callback<object, Action<OperatorMenuEnteredEvent>>(
                    (edgeLightAuditMenuHandler, callback) => { _operatorMenuEnteredEventAction = callback; });

            _eventBus
                .Setup(
                    m => m.Subscribe(
                        It.IsAny<EdgeLightAuditMenuHandler>(),
                        It.IsAny<Action<OperatorMenuExitedEvent>>()))
                .Callback<object, Action<OperatorMenuExitedEvent>>(
                    (edgeLightAuditMenuHandler, callback) => { _operatorMenuExitedEventAction = callback; });
        }

        [TestInitialize]
        public void Initialize()
        {
            _eventBus = new Mock<IEventBus>(MockBehavior.Strict);
            _edgLightStateManager = new Mock<IEdgeLightingStateManager>(MockBehavior.Strict);
            _systemDisableManager = new Mock<ISystemDisableManager>(MockBehavior.Strict);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _eventBus.Setup(x => x.UnsubscribeAll(It.IsAny<EdgeLightAuditMenuHandler>())).Verifiable();
            _edgeLightAuditMenuHandler?.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventBusIsNullExpectException()
        {
            _edgeLightAuditMenuHandler = new EdgeLightAuditMenuHandler(
                null,
                _systemDisableManager.Object,
                _edgLightStateManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenSystemDisableManagerIsNullExpectException()
        {
            _edgeLightAuditMenuHandler = new EdgeLightAuditMenuHandler(
                _eventBus.Object,
                null,
                _edgLightStateManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEdgeLightStateManagerIsNullExpectException()
        {
            _edgeLightAuditMenuHandler = new EdgeLightAuditMenuHandler(
                _eventBus.Object,
                _systemDisableManager.Object,
                null);
        }

        [TestMethod]
        public void DisposingObjectTwice()
        {
            SetUpEventHandler();
            _systemDisableManager
                .SetupGet(x => x.CurrentDisableKeys).Returns(new List<Guid>());
            _edgLightStateManager.Setup(x => x.ClearState(It.IsAny<IEdgeLightToken>()));
            _edgeLightAuditMenuHandler = new EdgeLightAuditMenuHandler(
                _eventBus.Object,
                _systemDisableManager.Object,
                _edgLightStateManager.Object);
            _eventBus.Setup(x => x.UnsubscribeAll(It.IsAny<EdgeLightAuditMenuHandler>())).Verifiable();
            _edgeLightAuditMenuHandler?.Dispose();
            _edgeLightAuditMenuHandler?.Dispose();
            _eventBus.VerifyAll();
        }

        [TestMethod]
        public void CreatingDefaultWithEnabled()
        {
            SetUpEventHandler();
            _systemDisableManager
                .SetupGet(x => x.CurrentDisableKeys).Returns(new List<Guid>());
            _edgLightStateManager.Setup(x => x.ClearState(It.IsAny<IEdgeLightToken>()));
            _edgeLightAuditMenuHandler = new EdgeLightAuditMenuHandler(
                _eventBus.Object,
                _systemDisableManager.Object,
                _edgLightStateManager.Object);
            Assert.IsTrue(_edgeLightAuditMenuHandler.Enabled);
            Assert.IsTrue(_edgeLightAuditMenuHandler.Name == nameof(EdgeLightAuditMenuHandler));
            _edgLightStateManager.Setup(x => x.SetState(EdgeLightState.OperatorMode)).Returns(new EdgeLightToken(20));
            _operatorMenuEnteredEventAction.Invoke(new OperatorMenuEnteredEvent());
            _edgLightStateManager.Setup(x => x.ClearState(It.IsAny<IEdgeLightToken>())).Callback<IEdgeLightToken>(
                (edgeLightToken) =>
                {
                    Assert.IsTrue(edgeLightToken.Id == 20);
                });
            _operatorMenuExitedEventAction.Invoke(new OperatorMenuExitedEvent());
            _eventBus.Setup(x => x.UnsubscribeAll(It.IsAny<EdgeLightAuditMenuHandler>())).Verifiable();
            _edgeLightAuditMenuHandler?.Dispose();
            _eventBus.VerifyAll();
        }
    }
}
