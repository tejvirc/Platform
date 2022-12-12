using System;
using System.Collections.Generic;
using Aristocrat.Monaco.Kernel;
using Aristocrat.Monaco.Gaming.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Aristocrat.Monaco.Gaming.Contracts;
using Aristocrat.Monaco.Kernel.Contracts.MessageDisplay;

namespace Aristocrat.Monaco.Gaming.Tests
{
    [TestClass]
    public class GameMessageDisplayHandlerUnitTest
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IRuntime> _runtimeService;
        private Mock<IMessageDisplay> _messageDisplay;
        private Mock<IPropertiesManager> _properties;
        private Mock<IGameDiagnostics> _gameReplay;

        /// <summary>
        ///     Method to setup objects for the test run.
        /// </summary>
        [TestInitialize]
        public void MyTestInitialize()
        {
            _eventBus = new Mock<IEventBus>();
            _runtimeService = new Mock<IRuntime>();
            _messageDisplay = new Mock<IMessageDisplay>();
            _properties = new Mock<IPropertiesManager>();
            _gameReplay = new Mock<IGameDiagnostics>();

            _properties.Setup(a => a.GetProperty(It.IsAny<string>(), false)).Returns(false);
            _gameReplay.SetupGet(a => a.IsActive).Returns(false);
        }

        /// <summary>
        ///     Cleans up after each test
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: messageDisplay")]
        public void WhenMessageBusIsNullExpectException()
        {
            var platformMessageBroadcaster = new GameMessageDisplayHandler(_runtimeService.Object, _eventBus.Object, null, null, null);

            Assert.IsNull(platformMessageBroadcaster);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: runtimeService")]
        public void WhenRuntimeServiceIsNullExpectException()
        {
            var platformMessageBroadcaster = new GameMessageDisplayHandler(null, _eventBus.Object, _messageDisplay.Object, _properties.Object, _gameReplay.Object);

            Assert.IsNull(platformMessageBroadcaster);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: eventbus")]
        public void WhenEventBusIsNullExpectException()
        {
            var platformMessageBroadcaster = new GameMessageDisplayHandler(_runtimeService.Object, null, _messageDisplay.Object, _properties.Object, _gameReplay.Object);

            Assert.IsNull(platformMessageBroadcaster);
        }

        [TestMethod]
        public void HandleGameExit()
        {
            var callArgs = new List<Action<GameProcessExitedEvent>>();

            // access invocation arguments
            _eventBus.Setup(foo => foo.Subscribe<GameProcessExitedEvent>(It.IsAny<object>(), It.IsAny<Action<GameProcessExitedEvent>>()))
                .Callback((object p, Action<GameProcessExitedEvent> s) => callArgs.Add(s));

            var platformMessageBroadcaster = new GameMessageDisplayHandler(_runtimeService.Object, _eventBus.Object, _messageDisplay.Object, _properties.Object, _gameReplay.Object);
            Assert.IsNotNull(platformMessageBroadcaster);

            // hit the message handler. it is private, so use the event bus mock to grab it
            callArgs[0].Invoke(new GameProcessExitedEvent(1));

            _messageDisplay.Verify(x => x.RemoveMessageDisplayHandler(platformMessageBroadcaster), Times.Exactly(1));

        }
    }
}
