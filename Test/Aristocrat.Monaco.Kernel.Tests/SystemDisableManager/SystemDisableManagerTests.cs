namespace Aristocrat.Monaco.Kernel.Tests.SystemDisableManager
{
    using System;
    using System.Linq;
    using System.Threading;
    using Contracts.MessageDisplay;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using SystemDisableManager = Kernel.SystemDisableManager;

    [TestClass]
    public class SystemDisableManagerTests
    {
        private Mock<IEventBus> _eventBusMock;
        private Mock<IMessageDisplay> _messageDisplay;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _eventBusMock = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _messageDisplay = MoqServiceManager.CreateAndAddService<IMessageDisplay>(MockBehavior.Strict);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void WhenConstructExpectSuccess()
        {
            var target = new SystemDisableManager();

            Assert.IsNotNull(target);
            Assert.AreEqual(0, target.CurrentDisableKeys.Count());
            //Assert.AreEqual(0, target.DisableReasons.Count());
            Assert.IsFalse(target.DisableImmediately);
        }

        [TestMethod]
        public void WhenDisposeExpectSuccess()
        {
            var target = new SystemDisableManager();
            target.Initialize();
            // There's nothing to verify at the moment other than to ensure this doesn't throw
            target.Dispose();
        }

        [TestMethod]
        public void WhenGetNameExpectSuccess()
        {
            var target = new SystemDisableManager();

            Assert.IsFalse(string.IsNullOrEmpty(target.Name));
        }

        [TestMethod]
        public void WhenGetServiceTypeExpectSuccess()
        {
            var target = new SystemDisableManager();

            Assert.AreEqual(new[] { typeof(ISystemDisableManager) }.GetType(), target.ServiceTypes.GetType());
        }

        [TestMethod]
        public void WhenInitializeExpectSuccess()
        {
            var target = new SystemDisableManager();
            target.Initialize();
        }

        [TestMethod]
        public void WhenDisableExpectDisabled()
        {
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisableAddedEvent>())).Verifiable();
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisabledEvent>())).Verifiable();
            IDisplayableMessage displayedMessage = null;
            _messageDisplay.Setup(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()))
                .Callback<IDisplayableMessage>(theMessage => { displayedMessage = theMessage; });

            var enableKey = Guid.NewGuid();
            var priority = SystemDisablePriority.Normal;
            var messagePriority = GetMessagePriority(priority);
            var reason = "Test";

            var target = new SystemDisableManager();
            target.Initialize();
            target.Disable(enableKey, priority, () => reason);

            Assert.IsTrue(target.IsDisabled);
            Assert.IsTrue(target.CurrentDisableKeys.Contains(enableKey));
            Assert.IsFalse(target.DisableImmediately);
            _messageDisplay.Verify(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()), Times.Once());
            Assert.IsNotNull(displayedMessage);
            Assert.AreEqual(reason, displayedMessage.Message);
            Assert.AreEqual(DisplayableMessageClassification.HardError, displayedMessage.Classification);
            Assert.AreEqual(messagePriority, displayedMessage.Priority);

            _eventBusMock.Verify();
        }

        [TestMethod]
        public void WhenDisableWithDurationExpectEnabledWhenTimeLapses()
        {
            var guid = Guid.NewGuid();
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisableAddedEvent>())).Verifiable();
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisabledEvent>())).Verifiable();
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisableRemovedEvent>())).Verifiable();
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemEnabledEvent>())).Verifiable();
            IDisplayableMessage displayedMessage = null;
            var removeGuid = Guid.Empty;
            _messageDisplay.Setup(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()))
                .Callback<IDisplayableMessage>(theMessage => { displayedMessage = theMessage; });
            _messageDisplay.Setup(m => m.RemoveMessage(It.IsAny<Guid>()))
                .Callback<Guid>(theGuid => { removeGuid = theGuid; });

            var reason = "Test";
            var priority = SystemDisablePriority.Normal;
            var messagePriority = GetMessagePriority(priority);

            var target = new SystemDisableManager();
            target.Initialize();
            target.Disable(guid, priority, () => reason, TimeSpan.FromMilliseconds(5));

            Assert.IsTrue(target.IsDisabled);
            _messageDisplay.Verify(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()), Times.Once());
            Assert.IsNotNull(displayedMessage);
            Assert.AreEqual(reason, displayedMessage.Message);
            Assert.AreEqual(DisplayableMessageClassification.HardError, displayedMessage.Classification);
            Assert.AreEqual(messagePriority, displayedMessage.Priority);

            Thread.Sleep(TimeSpan.FromMilliseconds(250));

            Assert.IsFalse(target.IsDisabled);
            _messageDisplay.Verify(m => m.RemoveMessage(It.IsAny<Guid>()), Times.Once());
            Assert.AreEqual(guid, removeGuid);

            _eventBusMock.Verify();
        }

        [TestMethod]
        public void WhenDisableWithSameKeyExpectUpdated()
        {
            var enableKey = Guid.NewGuid();

            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisableAddedEvent>()));
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisableUpdatedEvent>()));
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisabledEvent>()));
            IDisplayableMessage displayedMessage = null;
            _messageDisplay.Setup(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()))
                .Callback<IDisplayableMessage>(theMessage => { displayedMessage = theMessage; });

            const string reason = "text";
            var priority = SystemDisablePriority.Normal;
            var messagePriority = GetMessagePriority(priority);
            var target = new SystemDisableManager();
            target.Initialize();
            target.Disable(enableKey, SystemDisablePriority.Normal, () => reason);

            Assert.IsTrue(target.IsDisabled);
            Assert.IsTrue(target.CurrentDisableKeys.Contains(enableKey));
            Assert.IsFalse(target.DisableImmediately);
            _messageDisplay.Verify(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()), Times.Once());
            Assert.IsNotNull(displayedMessage);
            Assert.AreEqual(reason, displayedMessage.Message);
            Assert.AreEqual(DisplayableMessageClassification.HardError, displayedMessage.Classification);
            Assert.AreEqual(messagePriority, displayedMessage.Priority);

            const string updatedReason = "Updated text";

            // Disable again with same key
            target.Disable(enableKey, SystemDisablePriority.Normal, () => updatedReason);

            Assert.IsTrue(target.IsDisabled);
            Assert.IsTrue(target.CurrentDisableKeys.Count() == 1);
            //Assert.IsTrue(target.DisableReasons.Contains(updatedReason));

            _eventBusMock.Verify(mock => mock.Publish(It.IsAny<SystemDisableAddedEvent>()), Times.Once());
            _eventBusMock.Verify(mock => mock.Publish(It.IsAny<SystemDisabledEvent>()), Times.Once());
            _eventBusMock.Verify(mock => mock.Publish(It.IsAny<SystemDisableUpdatedEvent>()), Times.Once());
            Assert.AreEqual(updatedReason, displayedMessage.Message);
            _messageDisplay.Verify(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()), Times.Exactly(2));
        }

        [TestMethod]
        public void WhenDisableWithHigherPriorityExpectEscalated()
        {
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisableAddedEvent>()));
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisabledEvent>()));
            IDisplayableMessage displayedMessage = null;
            _messageDisplay.Setup(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()))
                .Callback<IDisplayableMessage>(theMessage => { displayedMessage = theMessage; });

            var reason1 = "First";
            var priority = SystemDisablePriority.Normal;
            var messagePriority = GetMessagePriority(priority);
            var target = new SystemDisableManager();
            target.Initialize();
            target.Disable(Guid.NewGuid(), priority, () => reason1);

            Assert.IsTrue(target.IsDisabled);
            Assert.IsFalse(target.DisableImmediately);
            _messageDisplay.Verify(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()), Times.Once());
            Assert.IsNotNull(displayedMessage);
            Assert.AreEqual(reason1, displayedMessage.Message);
            Assert.AreEqual(DisplayableMessageClassification.HardError, displayedMessage.Classification);
            Assert.AreEqual(messagePriority, displayedMessage.Priority);

            var reason2 = "Second";
            priority = SystemDisablePriority.Immediate;
            messagePriority = GetMessagePriority(priority);
            target.Disable(Guid.NewGuid(), priority, () => reason2);

            Assert.IsTrue(target.IsDisabled);
            Assert.IsTrue(target.CurrentDisableKeys.Count() == 2);
            Assert.IsTrue(target.DisableImmediately);
            _messageDisplay.Verify(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()), Times.Exactly(2));
            Assert.AreEqual(reason2, displayedMessage.Message);
            Assert.AreEqual(DisplayableMessageClassification.HardError, displayedMessage.Classification);
            Assert.AreEqual(messagePriority, displayedMessage.Priority);

            // Events should have been posted twice
            _eventBusMock.Verify(mock => mock.Publish(It.IsAny<SystemDisableAddedEvent>()), Times.Exactly(2));
            _eventBusMock.Verify(mock => mock.Publish(It.IsAny<SystemDisabledEvent>()), Times.Exactly(2));
        }

        [TestMethod]
        public void WhenEscalateWithSameKeyExpectEscalated()
        {
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisableAddedEvent>()));
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisableUpdatedEvent>()));
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisabledEvent>()));
            IDisplayableMessage displayedMessage = null;
            _messageDisplay.Setup(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()))
                .Callback<IDisplayableMessage>(theMessage => { displayedMessage = theMessage; });

            var enableKey = Guid.NewGuid();
            var reason = "Test";
            var priority = SystemDisablePriority.Normal;
            var messagePriority = GetMessagePriority(priority);
            var target = new SystemDisableManager();
            target.Initialize();
            target.Disable(enableKey, priority, () => reason);

            Assert.IsFalse(target.DisableImmediately);
            _messageDisplay.Verify(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()), Times.Once());
            Assert.IsNotNull(displayedMessage);
            Assert.AreEqual(reason, displayedMessage.Message);
            Assert.AreEqual(DisplayableMessageClassification.HardError, displayedMessage.Classification);
            Assert.AreEqual(messagePriority, displayedMessage.Priority);

            priority = SystemDisablePriority.Immediate;
            messagePriority = GetMessagePriority(priority);

            // Disable again with same key, but a higher priority
            target.Disable(enableKey, priority, () => reason);

            _messageDisplay.Verify(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()), Times.Exactly(2));
            Assert.AreEqual(reason, displayedMessage.Message);
            Assert.AreEqual(DisplayableMessageClassification.HardError, displayedMessage.Classification);
            Assert.AreEqual(messagePriority, displayedMessage.Priority);

            _eventBusMock.Verify(mock => mock.Publish(It.IsAny<SystemDisableAddedEvent>()), Times.Once());
            _eventBusMock.Verify(mock => mock.Publish(It.IsAny<SystemDisabledEvent>()), Times.Exactly(2));
        }

        [TestMethod]
        public void WhenDisableWithSamePriorityExpectNoChange()
        {
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisableAddedEvent>()));
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisabledEvent>()));
            IDisplayableMessage displayedMessage = null;
            _messageDisplay.Setup(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()))
                .Callback<IDisplayableMessage>(theMessage => { displayedMessage = theMessage; });

            var target = new SystemDisableManager();
            target.Initialize();
            var reason1 = "First";
            var priority = SystemDisablePriority.Normal;
            var messagePriority = GetMessagePriority(priority);
            target.Disable(Guid.NewGuid(), priority, () => reason1);

            Assert.IsTrue(target.IsDisabled);
            Assert.IsFalse(target.DisableImmediately);
            _messageDisplay.Verify(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()), Times.Once());
            Assert.IsNotNull(displayedMessage);
            Assert.AreEqual(reason1, displayedMessage.Message);
            Assert.AreEqual(DisplayableMessageClassification.HardError, displayedMessage.Classification);
            Assert.AreEqual(messagePriority, displayedMessage.Priority);

            var reason2 = "Second";
            target.Disable(Guid.NewGuid(), priority, () => reason2);

            Assert.IsTrue(target.IsDisabled);
            Assert.IsTrue(target.CurrentDisableKeys.Count() == 2);
            Assert.IsFalse(target.DisableImmediately);

            _messageDisplay.Verify(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()), Times.Exactly(2));
            Assert.AreEqual(reason2, displayedMessage.Message);
            Assert.AreEqual(DisplayableMessageClassification.HardError, displayedMessage.Classification);
            Assert.AreEqual(messagePriority, displayedMessage.Priority);

            // Event should have been posted twice
            _eventBusMock.Verify(mock => mock.Publish(It.IsAny<SystemDisableAddedEvent>()), Times.Exactly(2));

            // No change in the system disabled state
            _eventBusMock.Verify(mock => mock.Publish(It.IsAny<SystemDisabledEvent>()), Times.Once);
        }

        [TestMethod]
        public void WhenEnableExpectEnabled()
        {
            var key = Guid.NewGuid();
            Guid removeGuid = Guid.Empty;
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisableAddedEvent>()));
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisabledEvent>()));
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisableRemovedEvent>())).Verifiable();
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemEnabledEvent>())).Verifiable();
            IDisplayableMessage displayedMessage = null;
            _messageDisplay.Setup(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()))
                .Callback<IDisplayableMessage>(theMessage => { displayedMessage = theMessage; });
            _messageDisplay.Setup(m => m.RemoveMessage(It.IsAny<Guid>()))
                .Callback<Guid>(theGuid => { removeGuid = theGuid; });

            var target = new SystemDisableManager();
            var reason = "First";
            var priority = SystemDisablePriority.Normal;
            var messagePriority = GetMessagePriority(priority);
            target.Initialize();
            target.Disable(key, priority, () => reason);

            Assert.IsTrue(target.IsDisabled);
            _messageDisplay.Verify(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()), Times.Once());
            Assert.IsNotNull(displayedMessage);
            Assert.AreEqual(reason, displayedMessage.Message);
            Assert.AreEqual(DisplayableMessageClassification.HardError, displayedMessage.Classification);
            Assert.AreEqual(messagePriority, displayedMessage.Priority);

            target.Enable(key);

            Assert.IsFalse(target.IsDisabled);
            Assert.IsTrue(!target.CurrentDisableKeys.Any());
            _messageDisplay.Verify(m => m.RemoveMessage(It.IsAny<Guid>()), Times.Once());
            Assert.AreEqual(key, removeGuid);

            _eventBusMock.Verify();
        }

        [TestMethod]
        public void WhenEnableWithSameKeyExpectEnabledOnlyOnce()
        {
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisableAddedEvent>()));
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisabledEvent>()));
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisableRemovedEvent>()));
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemEnabledEvent>()));
            IDisplayableMessage displayedMessage = null;
            Guid removeGuid = Guid.Empty;
            _messageDisplay.Setup(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()))
                .Callback<IDisplayableMessage>(theMessage => { displayedMessage = theMessage; });
            _messageDisplay.Setup(m => m.RemoveMessage(It.IsAny<Guid>()))
                .Callback<Guid>(theGuid => { removeGuid = theGuid; });

            var key = Guid.NewGuid();

            var target = new SystemDisableManager();
            target.Initialize();
            var reason = "First";
            var priority = SystemDisablePriority.Normal;
            var messagePriority = GetMessagePriority(priority);
            target.Disable(key, priority, () => reason, TimeSpan.FromMilliseconds(250));

            _messageDisplay.Verify(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()), Times.Once());
            Assert.IsNotNull(displayedMessage);
            Assert.AreEqual(reason, displayedMessage.Message);
            Assert.AreEqual(DisplayableMessageClassification.HardError, displayedMessage.Classification);
            Assert.AreEqual(messagePriority, displayedMessage.Priority);

            target.Enable(key);
            target.Enable(key);

            _messageDisplay.Verify(m => m.RemoveMessage(It.IsAny<Guid>()), Times.Once());
            Assert.AreEqual(key, removeGuid);

            _eventBusMock.Verify(mock => mock.Publish(It.IsAny<SystemDisableRemovedEvent>()), Times.Once);
            _eventBusMock.Verify(mock => mock.Publish(It.IsAny<SystemEnabledEvent>()), Times.Once);
        }

        [TestMethod]
        public void WhenEnableBeforeTimeLapsesExpectEnabledOnlyOnce()
        {
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisableAddedEvent>()));
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisabledEvent>()));
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisableRemovedEvent>()));
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemEnabledEvent>()));
            var key = Guid.NewGuid();
            IDisplayableMessage displayedMessage = null;
            Guid removeGuid = Guid.Empty;
            _messageDisplay.Setup(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()))
                .Callback<IDisplayableMessage>(theMessage => { displayedMessage = theMessage; });
            _messageDisplay.Setup(m => m.RemoveMessage(It.IsAny<Guid>()))
                .Callback<Guid>(theGuid => { removeGuid = theGuid; });

            var target = new SystemDisableManager();
            target.Initialize();
            var reason = "First";
            var priority = SystemDisablePriority.Normal;
            var messagePriority = GetMessagePriority(priority);

            target.Disable(key, priority, () => reason, TimeSpan.FromMilliseconds(250));

            Assert.IsTrue(target.IsDisabled);

            _messageDisplay.Verify(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()), Times.Once());
            Assert.IsNotNull(displayedMessage);
            Assert.AreEqual(reason, displayedMessage.Message);
            Assert.AreEqual(DisplayableMessageClassification.HardError, displayedMessage.Classification);
            Assert.AreEqual(messagePriority, displayedMessage.Priority);

            target.Enable(key);

            Thread.Sleep(500);

            _messageDisplay.Verify(m => m.RemoveMessage(It.IsAny<Guid>()), Times.Once());
            Assert.AreEqual(key, removeGuid);

            _eventBusMock.Verify(mock => mock.Publish(It.IsAny<SystemDisableRemovedEvent>()), Times.Once);
            _eventBusMock.Verify(mock => mock.Publish(It.IsAny<SystemEnabledEvent>()), Times.Once);
        }

        [TestMethod]
        public void WhenEnableWithMultipleDisablesExpectStillDisabled()
        {
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisableAddedEvent>()));
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisabledEvent>()));
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemDisableRemovedEvent>()));
            _eventBusMock.Setup(e => e.Publish(It.IsAny<SystemEnabledEvent>()));
            IDisplayableMessage displayedMessage = null;
            Guid removeGuid = Guid.Empty;
            _messageDisplay.Setup(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()))
                .Callback<IDisplayableMessage>(theMessage => { displayedMessage = theMessage; });
            _messageDisplay.Setup(m => m.RemoveMessage(It.IsAny<Guid>()))
                .Callback<Guid>(theGuid => { removeGuid = theGuid; });

            var key = Guid.NewGuid();

            var target = new SystemDisableManager();
            target.Initialize();

            target.Disable(key, SystemDisablePriority.Normal, () => "First");

            var reason2 = "Second";
            var priority = SystemDisablePriority.Normal;
            var messagePriority = GetMessagePriority(priority);
            target.Disable(Guid.NewGuid(), priority, () => reason2);

            Assert.IsTrue(target.IsDisabled);
            Assert.IsTrue(target.CurrentDisableKeys.Count() == 2);

            target.Enable(key);

            Assert.IsTrue(target.IsDisabled);
            Assert.IsTrue(target.CurrentDisableKeys.All(k => k != key));

            _messageDisplay.Verify(m => m.DisplayMessage(It.IsAny<IDisplayableMessage>()), Times.Exactly(2));
            Assert.IsNotNull(displayedMessage);
            Assert.AreEqual(reason2, displayedMessage.Message);
            Assert.AreEqual(DisplayableMessageClassification.HardError, displayedMessage.Classification);
            Assert.AreEqual(messagePriority, displayedMessage.Priority);
            _messageDisplay.Verify(m => m.RemoveMessage(It.IsAny<Guid>()), Times.Once());
            Assert.AreEqual(key, removeGuid);

            _eventBusMock.Verify(mock => mock.Publish(It.IsAny<SystemDisableRemovedEvent>()), Times.Once);

            // Event shouldn't have been posted
            _eventBusMock.Verify(mock => mock.Publish(It.IsAny<SystemEnabledEvent>()), Times.Never);
        }

        private DisplayableMessagePriority GetMessagePriority(SystemDisablePriority priority)
        {
            return priority == SystemDisablePriority.Immediate
                ? DisplayableMessagePriority.Immediate
                : DisplayableMessagePriority.Normal;
        }
    }
}
