namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using Accounting.Contracts;
    using Contracts;
    using Gaming.Commands;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GameStateServiceTests
    {
        private Mock<IEventBus> _eventBus;
        private Mock<ISystemDisableManager>  _disable;
        private Mock<ICommandHandlerFactory> _command;
        private Mock<IGameHistory> _history;
        private Mock<IPropertiesManager> _properties;
        private Mock<IMoneyLaunderingMonitor> _monitor;
        private Mock<ITransferOutHandler> _transferHandler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _eventBus = new Mock<IEventBus>();
            _disable = new Mock<ISystemDisableManager>();
            _command = new Mock<ICommandHandlerFactory>();
            _history = new Mock<IGameHistory>();
            _properties = new Mock<IPropertiesManager>();
            _monitor = new Mock<IMoneyLaunderingMonitor>();
            _transferHandler = new Mock<ITransferOutHandler>();
        }

        [DataRow(true, false, false, false, false, false, false)]
        [DataRow(false, true, false, false, false, false, false)]
        [DataRow(false, false, true, false, false, false, false)]
        [DataRow(false, false, false, true, false, false, false)]
        [DataRow(false, false, false, false, true, false, false)]
        [DataRow(false, false, false, false, false, true, false)]
        [DataRow(false, false, false, false, false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorTest(
            bool nullBus,
            bool nullDisable,
            bool nullFactory,
            bool nullHistory,
            bool nullProperties,
            bool nullTransfer,
            bool nullMonitor
            )
        {
            var target = CreateTarget(nullBus, nullDisable, nullFactory, nullHistory, nullProperties, nullTransfer, nullMonitor);
        }

        private GamePlayState CreateTarget(
            bool nullBus = false,
            bool nullDisable = false,
            bool nullFactory = false,
            bool nullHistory = false,
            bool nullProperties = false,
            bool nullTransfer = false,
            bool nullMonitor = false
            )
        {
            return new GamePlayState(
                nullBus ? null : _eventBus.Object,
                nullDisable ? null : _disable.Object,
                nullFactory ? null : _command.Object,
                nullHistory ? null : _history.Object,
                nullProperties ? null : _properties.Object,
                nullTransfer ? null : _transferHandler.Object,
                nullMonitor ? null : _monitor.Object
               );
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var bus = new Mock<IEventBus>();
            var disable = new Mock<ISystemDisableManager>();
            var factory = new Mock<ICommandHandlerFactory>();
            var history = new Mock<IGameHistory>();
            var properties = new Mock<IPropertiesManager>();
            var transferHandler = new Mock<ITransferOutHandler>();
            var monitor = new Mock<IMoneyLaunderingMonitor>();

            var service = new GamePlayState(
                bus.Object,
                disable.Object,
                factory.Object,
                history.Object,
                properties.Object,
                transferHandler.Object,
                monitor.Object);

            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void WhenDisposeExpectSuccess()
        {
            var bus = new Mock<IEventBus>();
            var disable = new Mock<ISystemDisableManager>();
            var factory = new Mock<ICommandHandlerFactory>();
            var history = new Mock<IGameHistory>();
            var properties = new Mock<IPropertiesManager>();
            var transferHandler = new Mock<ITransferOutHandler>();
            var monitor = new Mock<IMoneyLaunderingMonitor>();

            var service = new GamePlayState(
                bus.Object,
                disable.Object,
                factory.Object,
                history.Object,
                properties.Object,
                transferHandler.Object,
                monitor.Object);

            Assert.IsNotNull(service);

            service.Dispose();

            bus.Verify(b => b.UnsubscribeAll(service));
        }

        /*
                [TestMethod]
                public void WhenInitializeExpectSuccess()
                {
                    var bus = new Mock<IEventBus>();
  
                    // Initialize is called in the ctor
                    var service = new GamePlayState(bus.Object);
  
                    bus.Verify(
                        b =>
                            b.Subscribe<GameEndedEvent>(
                                It.Is<object>(o => o == service),
                                It.IsAny<EventCallback>()));
                    bus.Verify(
                        b =>
                            b.Subscribe<GameStartedEvent>(
                                It.Is<object>(o => o == service),
                                It.IsAny<EventCallback>()));
  
                    Assert.IsFalse(service.InGameRound);
                }
  
                [TestMethod]
                public void WhenReceiveGameEndedEventExpectNotInGameRound()
                {
                    var bus = new Mock<IEventBus>();
  
                    EventCallback callback = null;
                    bus.Setup(b => b.Subscribe<GameEndedEvent>(It.IsAny<object>(), It.IsAny<EventCallback>()))
                        .Callback((object subscriber, EventCallback eventCallback) => { callback = eventCallback; });
  
                    // Initialize is called in the ctor
                    var service = new GameStateService(bus.Object);
  
                    Assert.IsNotNull(callback);
  
                    callback.Invoke(new GameEndedEvent());
  
                    Assert.IsFalse(service.InGameRound);
                }
  
                [TestMethod]
                public void WhenReceiveGameStartedEventExpectInGameRound()
                {
                    var bus = new Mock<IEventBus>();
  
                    EventCallback callback = null;
                    bus.Setup(b => b.Subscribe<GameStartedEvent>(It.IsAny<object>(), It.IsAny<EventCallback>()))
                        .Callback((object subscriber, EventCallback eventCallback) => { callback = eventCallback; });
  
                    // Initialize is called in the ctor
                    var service = new GameStateService(bus.Object);
  
                    Assert.IsNotNull(callback);
                    Assert.IsFalse(service.InGameRound);
  
                    callback.Invoke(new GameStartedEvent());
  
                    Assert.IsTrue(service.InGameRound);
                }
        */
    }
}