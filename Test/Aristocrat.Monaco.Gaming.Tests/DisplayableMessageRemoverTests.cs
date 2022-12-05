using System;
using System.Collections.Generic;
using Aristocrat.Monaco.Kernel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Aristocrat.Monaco.Gaming.Contracts;

namespace Aristocrat.Monaco.Gaming.Tests
{
    [TestClass]
    public class DisplayableMessageRemoverTests
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IPropertiesManager> _properties;
        private Mock<IMessageDisplay> _messageDisplay;
        private DisplayableMessageRemover _displayableMessageRemover;
        private List<Action<PrimaryGameStartedEvent>> _eventCallbacks;

        /// <summary>
        ///     Method to setup objects for the test run.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            _eventBus = new Mock<IEventBus>();
            _properties = new Mock<IPropertiesManager>();
            _messageDisplay = new Mock<IMessageDisplay>();
            _eventCallbacks = new List<Action<PrimaryGameStartedEvent>>();

            _eventBus.Setup(foo => foo.Subscribe(It.IsAny<object>(), It.IsAny<Action<PrimaryGameStartedEvent>>()))
                .Callback((object p, Action<PrimaryGameStartedEvent> s) => _eventCallbacks.Add(s));

            _properties.Setup(bar => bar.GetProperty(GamingConstants.MessageClearStyle, It.IsAny<MessageClearStyle>()))
                .Returns(MessageClearStyle.GameStart);

            _messageDisplay.Setup(baz => baz.RemoveMessage(It.IsAny<Guid>())).Verifiable();

            _displayableMessageRemover = new DisplayableMessageRemover(_eventBus.Object, _properties.Object, _messageDisplay.Object);
        }

        /// <summary>
        ///     Cleans up after each test
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod]
        public void SendGameStart_WhenMessageShowing_ExpectRemove()
        {
            DisplayableMessage message = new DisplayableMessage(() => "george", DisplayableMessageClassification.HardError, DisplayableMessagePriority.Immediate, typeof(PlatformBootedEvent));
            _displayableMessageRemover.DisplayMessage(message);

            // Hit the message handler. It is private, so we used the event bus mock to grab it in Initialize.
            _eventCallbacks[0].Invoke(new PrimaryGameStartedEvent(1, 2, "geoff", new GameHistoryLog(3)));

            _messageDisplay.Verify(x => x.RemoveMessage(message.Id), Times.Exactly(1));
        }
    }
}
