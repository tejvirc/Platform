namespace Aristocrat.Monaco.Application.Tests.EdgeLight.Handlers
{
    using System;
    using System.Collections.Generic;
    using Application.EdgeLight.Handlers;
    using Contracts;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.EdgeLighting;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class EdgeLightAsTowerLightHandlerTests
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IEdgeLightingController> _edgeLightController;
        private EdgeLightAsTowerLightHandler _edgeLightAsTowerLightHandler;
        private Action<OperatorMenuEnteredEvent> _operatorMenuEnteredEventAction;
        private Action<OperatorMenuExitedEvent> _operatorMenuExitedEventAction;

        private void SetUpEventHandler()
        {
            _eventBus
                .Setup(
                    m => m.Subscribe(
                        It.IsAny<EdgeLightAsTowerLightHandler>(),
                        It.IsAny<Action<OperatorMenuEnteredEvent>>()))
                .Callback<object, Action<OperatorMenuEnteredEvent>>(
                    (edgeLightAsTowerLightHandler, callback) => { _operatorMenuEnteredEventAction = callback; });

            _eventBus
                .Setup(
                    m => m.Subscribe(
                        It.IsAny<EdgeLightAsTowerLightHandler>(),
                        It.IsAny<Action<OperatorMenuExitedEvent>>()))
                .Callback<object, Action<OperatorMenuExitedEvent>>(
                    (edgeLightAsTowerLightHandler, callback) => { _operatorMenuExitedEventAction = callback; });
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
            _eventBus.Setup(x => x.UnsubscribeAll(It.IsAny<EdgeLightAsTowerLightHandler>())).Verifiable();
            _edgeLightAsTowerLightHandler?.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventBusIsNullExpectException()
        {
            _edgeLightAsTowerLightHandler = new EdgeLightAsTowerLightHandler(
                null,
                _propertiesManager.Object,
                _edgeLightController.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEdgeLightingControllerIsNullExpectException()
        {
            _edgeLightAsTowerLightHandler = new EdgeLightAsTowerLightHandler(
                _eventBus.Object,
                _propertiesManager.Object,
                null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPropertiesManagerIsNullExpectException()
        {
            _edgeLightAsTowerLightHandler = new EdgeLightAsTowerLightHandler(
                _eventBus.Object,
                null,
                _edgeLightController.Object);
        }

        [TestMethod]
        public void CreatingDefault()
        {
            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.EdgeLightAsTowerLightEnabled, false))
                .Returns(false);
            _edgeLightAsTowerLightHandler = new EdgeLightAsTowerLightHandler(
                _eventBus.Object,
                _propertiesManager.Object,
                _edgeLightController.Object);
            Assert.IsTrue(_edgeLightAsTowerLightHandler.Enabled == false);
            Assert.IsTrue(_edgeLightAsTowerLightHandler.Name == nameof(EdgeLightAsTowerLightHandler));
            _eventBus.Setup(x => x.UnsubscribeAll(It.IsAny<EdgeLightAsTowerLightHandler>())).Verifiable();
            _edgeLightAsTowerLightHandler?.Dispose();
            // To cover the if already Disposed case.
            _edgeLightAsTowerLightHandler?.Dispose();
        }

        [TestMethod]
        public void CreatingDefaultWithEnabled()
        {
            SetUpEventHandler();
            _edgeLightController.Setup(x => x.SetPriorityComparer(It.IsAny<IComparer<StripPriority>>()))
                .Callback<IComparer<StripPriority>>(
                    (priorityComparer) =>
                    {
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.PlatformTest , StripPriority.BarTopTowerLight) > 0);
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.Absolute , StripPriority.PlatformTest) > 0);
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.PlatformTest , StripPriority.DoorOpen) > 0);
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.CashOut , StripPriority.DoorOpen) > 0);
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.LobbyView , StripPriority.DoorOpen) > 0);
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.PlatformControlled , StripPriority.DoorOpen) > 0);
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.DoorOpen , StripPriority.LowPriority) > 0);
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.BarTopBottomStripDisable , StripPriority.LowPriority) > 0);
                    });
            _propertiesManager
                .Setup(m => m.GetProperty(ApplicationConstants.EdgeLightAsTowerLightEnabled, false))
                .Returns(true);
            _edgeLightAsTowerLightHandler = new EdgeLightAsTowerLightHandler(
                _eventBus.Object,
                _propertiesManager.Object,
                _edgeLightController.Object);
            Assert.IsTrue(_edgeLightAsTowerLightHandler.Enabled);
            Assert.IsTrue(_edgeLightAsTowerLightHandler.Name == nameof(EdgeLightAsTowerLightHandler));
            _edgeLightController.Setup(x => x.SetPriorityComparer(It.IsAny<IComparer<StripPriority>>()))
                .Callback<IComparer<StripPriority>>(
                    (priorityComparer) =>
                    {
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.PlatformTest , StripPriority.BarTopBottomStripDisable) > 0);
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.Absolute , StripPriority.PlatformTest) > 0);
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.PlatformTest , StripPriority.DoorOpen) > 0);
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.AuditMenu, StripPriority.BarTopTowerLight) > 0);
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.DoorOpen , StripPriority.BarTopTowerLight) > 0);
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.BarTopTowerLight, StripPriority.BarTopTowerLight) == 0);
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.DoorOpen , StripPriority.CashOut) > 0);
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.BarTopBottomStripDisable , StripPriority.AuditMenu) > 0);
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.AuditMenu , StripPriority.DoorOpen) > 0);
                    });
            _operatorMenuEnteredEventAction?.Invoke(new OperatorMenuEnteredEvent());

            _edgeLightController.Setup(x => x.SetPriorityComparer(It.IsAny<IComparer<StripPriority>>()))
                .Callback<IComparer<StripPriority>>(
                    (priorityComparer) =>
                    {
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.PlatformTest , StripPriority.BarTopTowerLight) > 0);
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.Absolute , StripPriority.PlatformTest) > 0);
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.PlatformTest, StripPriority.DoorOpen) > 0);
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.CashOut, StripPriority.DoorOpen) > 0);
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.LobbyView, StripPriority.DoorOpen) > 0);
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.PlatformControlled, StripPriority.DoorOpen) > 0);
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.GamePriority, StripPriority.DoorOpen) > 0);
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.DoorOpen, StripPriority.LowPriority) > 0);
                        Assert.IsTrue(priorityComparer.Compare(StripPriority.BarTopBottomStripDisable, StripPriority.LowPriority) > 0);
                    });
            _operatorMenuExitedEventAction?.Invoke(new OperatorMenuExitedEvent());
            _eventBus.Setup(x => x.UnsubscribeAll(It.IsAny<EdgeLightAsTowerLightHandler>())).Verifiable();
            _edgeLightAsTowerLightHandler?.Dispose();
            // To cover the if already Disposed case.
            _edgeLightAsTowerLightHandler?.Dispose();
            _eventBus.VerifyAll();
        }
    }
}
