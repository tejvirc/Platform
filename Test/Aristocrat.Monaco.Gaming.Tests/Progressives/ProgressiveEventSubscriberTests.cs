
namespace Aristocrat.Monaco.Gaming.Tests.Progressives
{
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Protocol;
    using Aristocrat.Monaco.Common;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked;
    using Aristocrat.Monaco.Gaming.Progressives;
    using Aristocrat.Monaco.Test.Common;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Unit test class for ProgressiveEventSubscriberTests
    /// </summary>
    [TestClass]
    public class ProtocolProgressiveEventsHelperTests
    {
        private IProtocolProgressiveEventsRegistry _target;
        private Mock<IEventBus> _mockEventBus;

        private Mock<IMultiProtocolConfigurationProvider> _multiProtocolConfigurationProvider;

        // A mock dictionary of Progressive eventTypes and corresponding registered event handlers.
        private Dictionary<Type, IProtocolProgressiveEventHandler> _protocolProgressiveEventHandlers;

        // A dictionary that contains the protocol name as key and the event types notified against it.
        private Dictionary<string, List<Type>> _testResultBuffer;
        private readonly List<CommsProtocol> _protocolList = new List<CommsProtocol> {CommsProtocol.SAS, CommsProtocol.MGAM, CommsProtocol.G2S, CommsProtocol.Test, CommsProtocol.HHR};
        [TestInitialize]
        public void Init()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _mockEventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _multiProtocolConfigurationProvider =
                MoqServiceManager.CreateAndAddService<IMultiProtocolConfigurationProvider>(MockBehavior.Default);
            _target = new ProtocolProgressiveEventsRegistry();
            _testResultBuffer = new Dictionary<string, List<Type>>();
            _protocolProgressiveEventHandlers = new Dictionary<Type, IProtocolProgressiveEventHandler>();
        }

        [TestMethod]
        public void ProgressiveEventSubscriber_WhenProgressiveHitEventFired_CorrectHandlerCalled()
        {
            // Setup
            var progressiveHitEvent = new ProgressiveHitEvent(new JackpotTransaction(),
                new Mock<IViewableProgressiveLevel>().Object, false);
            _multiProtocolConfigurationProvider.Setup(c => c.MultiProtocolConfiguration).Returns(
                GenerateMultiConfiguration(ProtocolNames.SAS, ProtocolNames.MGAM, ProtocolNames.HHR));
            SetupEventHandlers(progressiveHitEvent);

            // Action.
            // publish the event of type ProgressiveHitEvent.
            _mockEventBus.Object.Publish(progressiveHitEvent);

            // Assert
            // Assert Only one protocol received the Progressive event.
            Assert.AreEqual(_testResultBuffer.Count, 1);
            // Assert HHR protocol received the Progressive event.
            Assert.AreEqual(_testResultBuffer.ContainsKey(ProtocolNames.HHR), true);
            Assert.AreEqual(_testResultBuffer[ProtocolNames.HHR].Contains(typeof(ProgressiveHitEvent)), true);
            _testResultBuffer.Clear();
            CleanUpEventHandler<ProgressiveHitEvent>(ProtocolNames.HHR);
            Assert.AreEqual(_protocolProgressiveEventHandlers.Count, 0);
        }

        [TestMethod]
        public void ProgressiveEventSubscriber_WhenNoProtocolHandlingProgressive_NoHandlerCalled()
        {
            // Setup
            var progressiveHitEvent = new ProgressiveHitEvent(new JackpotTransaction(),
                new Mock<IViewableProgressiveLevel>().Object, false);
            _multiProtocolConfigurationProvider.Setup(c => c.MultiProtocolConfiguration).Returns(
                GenerateMultiConfiguration(ProtocolNames.SAS, ProtocolNames.MGAM, ProtocolNames.None));
            SetupEventHandlers(progressiveHitEvent, false);

            // Action
            _mockEventBus.Object.Publish(progressiveHitEvent);

            // Assert
            // Assert no event is delivered to any handler.
            Assert.AreEqual(_testResultBuffer.Count, 0);
            _testResultBuffer.Clear();
            Assert.AreEqual(_protocolProgressiveEventHandlers.Count, 0);
        }

        [TestMethod]
        public void ProgressiveEventSubscriber_WhenNoProtocolHandlingAProgressiveEvent_NoHandlerCalled()
        {
            // Setup
            var progressiveHitEvent = new ProgressiveHitEvent(new JackpotTransaction(),
                new Mock<IViewableProgressiveLevel>().Object, false);
            var progressiveCommitEvent =
                new ProgressiveCommitEvent(new JackpotTransaction(), new Mock<IViewableProgressiveLevel>().Object);
            _multiProtocolConfigurationProvider.Setup(c => c.MultiProtocolConfiguration).Returns(
                GenerateMultiConfiguration(ProtocolNames.SAS, ProtocolNames.MGAM, ProtocolNames.MGAM));
            // Handles Progressive hit event
            SetupEventHandlers(progressiveHitEvent);
            // Not subscribe for this event by passing second argument false.
            SetupEventHandlers(progressiveCommitEvent, false);

            // Action
            _mockEventBus.Object.Publish(progressiveHitEvent);
            _mockEventBus.Object.Publish(progressiveCommitEvent);
            // Assert.
            // Asset only one protocol received event handler  callback
            Assert.AreEqual(_testResultBuffer.Count, 1);
            // Assert only Mgam protocol received event handler callback.
            Assert.AreEqual(_testResultBuffer.ContainsKey(ProtocolNames.MGAM), true);
            // Assert only ProgressiveHit event is received
            Assert.AreEqual(_testResultBuffer[ProtocolNames.MGAM].Count, 1);
            Assert.AreEqual(_testResultBuffer[ProtocolNames.MGAM].Contains(typeof(ProgressiveHitEvent)), true);
            _testResultBuffer.Clear();
            CleanUpEventHandler<ProgressiveHitEvent>(ProtocolNames.MGAM);
            Assert.AreEqual(_protocolProgressiveEventHandlers.Count, 0);
            Assert.AreEqual(_protocolProgressiveEventHandlers.Count, 0);
        }

        [TestMethod]
        public void ProgressiveEventSubscriber_WhenMultipleProgressiveEventsFired_HandlerIsCalledForAllTypes()
        {
            // Setup
            var jackpot = new JackpotTransaction();
            var level = new Mock<IViewableProgressiveLevel>();
            var levels = new List<IViewableLinkedProgressiveLevel>();
            for (var i = 0; i < 10; ++i) levels.Add(new LinkedProgressiveLevel());
            var propertyChangedEvent = new PropertyChangedEvent("test");
            var progressiveHitEvent = new ProgressiveHitEvent(jackpot, level.Object, false);
            var progressiveCommitEvent = new ProgressiveCommitEvent(jackpot, level.Object);
            var progressiveCommitAckEvent = new ProgressiveCommitAckEvent(jackpot);
            var linkedProgressiveExpiredEvent =
                new LinkedProgressiveExpiredEvent(levels, new List<IViewableLinkedProgressiveLevel>());
            _multiProtocolConfigurationProvider.Setup(c => c.MultiProtocolConfiguration)
                .Returns(GenerateMultiConfiguration(ProtocolNames.SAS, ProtocolNames.MGAM, ProtocolNames.MGAM));
            SetupEventHandlers(progressiveHitEvent);
            SetupEventHandlers(progressiveCommitEvent);
            SetupEventHandlers(propertyChangedEvent);
            SetupEventHandlers(progressiveCommitAckEvent);
            SetupEventHandlers(linkedProgressiveExpiredEvent);

            // Action, Firing all events one by one
            _mockEventBus.Object.Publish(progressiveHitEvent);
            _mockEventBus.Object.Publish(progressiveCommitEvent);
            _mockEventBus.Object.Publish(propertyChangedEvent);
            _mockEventBus.Object.Publish(progressiveCommitAckEvent);
            _mockEventBus.Object.Publish(linkedProgressiveExpiredEvent);

            // Assert All event handlers are called in the protocol that handles progressives.
            Assert.AreEqual(_testResultBuffer.Count, 1);
            Assert.AreEqual(_testResultBuffer.ContainsKey(ProtocolNames.MGAM), true);
            Assert.AreEqual(_testResultBuffer[ProtocolNames.MGAM].Contains(typeof(ProgressiveHitEvent)), true);
            Assert.AreEqual(_testResultBuffer[ProtocolNames.MGAM].Contains(typeof(ProgressiveCommitEvent)), true);
            Assert.AreEqual(_testResultBuffer[ProtocolNames.MGAM].Contains(typeof(PropertyChangedEvent)), true);
            Assert.AreEqual(_testResultBuffer[ProtocolNames.MGAM].Contains(typeof(ProgressiveCommitAckEvent)), true);
            Assert.AreEqual(_testResultBuffer[ProtocolNames.MGAM].Contains(typeof(LinkedProgressiveExpiredEvent)), true);
            _testResultBuffer.Clear();
            CleanUpEventHandler<ProgressiveHitEvent>(ProtocolNames.MGAM);
            CleanUpEventHandler<ProgressiveCommitEvent>(ProtocolNames.MGAM);
            CleanUpEventHandler<PropertyChangedEvent>(ProtocolNames.MGAM);
            CleanUpEventHandler<ProgressiveCommitAckEvent>(ProtocolNames.MGAM);
            CleanUpEventHandler<LinkedProgressiveExpiredEvent>(ProtocolNames.MGAM);
            Assert.AreEqual(_protocolProgressiveEventHandlers.Count, 0);
        }

        private IEnumerable<ProtocolConfiguration> GenerateMultiConfiguration(string validationProtocol,
            string fundTransferProtocol,
            string progressiveProtocol)
        {
            var multiProtocolConfiguration = new List<ProtocolConfiguration>();
            _protocolList.ForEach(x => multiProtocolConfiguration.Add(new ProtocolConfiguration(x)));
            multiProtocolConfiguration.ForEach(config =>
            {
                config.IsValidationHandled = config.Protocol == EnumParser.ParseOrThrow<CommsProtocol>(validationProtocol);
                config.IsFundTransferHandled = config.Protocol == EnumParser.ParseOrThrow<CommsProtocol>(fundTransferProtocol);
                config.IsProgressiveHandled = config.Protocol == EnumParser.ParseOrThrow<CommsProtocol>(progressiveProtocol);
            });
            return multiProtocolConfiguration;
        }

        [TestMethod]
        public void ProgressiveEventSubscriber_WhenProgressiveEventFiredFromProtocol_CorrectHandlerCalled()
        {
            // Setup
            var mgamProgressiveHitEvent = new ProgressiveHitEvent(new JackpotTransaction(),
                new Mock<IViewableProgressiveLevel>().Object, false);
            var HHRProgressiveHitEvent = new ProgressiveHitEvent(new JackpotTransaction(),
                new Mock<IViewableProgressiveLevel>().Object, false);
            var SASProgressiveHitEvent = new ProgressiveHitEvent(new JackpotTransaction(),
                new Mock<IViewableProgressiveLevel>().Object, false);
            _multiProtocolConfigurationProvider.Setup(c => c.MultiProtocolConfiguration).Returns(
                GenerateMultiConfiguration(ProtocolNames.SAS, ProtocolNames.MGAM, ProtocolNames.HHR));
            int progressiveEventsHandledCount = 0;
            ProgressiveHitEvent eventRecieved = null;
            _mockEventBus.Setup(x => x.Publish(It.IsAny<IEvent>()))
                .Callback<IEvent>(e =>
                {
                    progressiveEventsHandledCount++;
                    eventRecieved = e as ProgressiveHitEvent;
                });

            // Action.
            // publish multiple events of type ProgressiveHitEvent from various protocols.
            _target.PublishProgressiveEvent(ProtocolNames.MGAM, mgamProgressiveHitEvent);
            _target.PublishProgressiveEvent(ProtocolNames.HHR, HHRProgressiveHitEvent);
            _target.PublishProgressiveEvent(ProtocolNames.SAS, SASProgressiveHitEvent);

            // Assert
            // Assert subsriber received only one Progressive event(HHR).
            Assert.AreEqual(progressiveEventsHandledCount, 1);
            Assert.AreEqual(eventRecieved, HHRProgressiveHitEvent);
        }

        // Mock Event handler call back for progressive events.
        private void NotifyHandlerCalled(string protocolName, Type type)
        {
            if (!_testResultBuffer.ContainsKey(protocolName))
                _testResultBuffer.Add(protocolName, new List<Type> {type});
            else
                _testResultBuffer[protocolName].Add(type);
        }

        private void CleanUpEventHandler<T>(string protocolName) where T : IEvent
        {
            var localCopy = new Dictionary<Type, IProtocolProgressiveEventHandler>(_protocolProgressiveEventHandlers);
            foreach (var protocolProgressiveEventHandler in localCopy)
                _target.UnSubscribeProgressiveEvent<T>(protocolName, protocolProgressiveEventHandler.Value);
        }

        private void SetupEventHandlers<T>(T @event, bool subscribe = true) where T : IEvent
        {
            // Mock Subscribe
            _mockEventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<T>>()))
                .Callback<object, Action<T>>((x, y) =>
                {
                    // list of event handlers for progressive events(mock event bus).
                    _protocolProgressiveEventHandlers.Add(typeof(T), x as IProtocolProgressiveEventHandler);
                });

            // Mock Unsubscribe
            _mockEventBus.Setup(x => x.Unsubscribe<T>(It.IsAny<object>()))
                .Callback<object>(o =>
                {
                    foreach (var i in _protocolProgressiveEventHandlers.Where(d => d.Value == o).ToList())
                        _protocolProgressiveEventHandlers.Remove(i.Key);
                });

            _protocolList.ForEach(x =>
            {
                var handler = new Mock<IProtocolProgressiveEventHandler>();
                // Setting up the event handler call back for progressive events.
                handler.Setup(c => c.HandleProgressiveEvent(@event as IEvent))
                    .Callback<IEvent>(e => NotifyHandlerCalled(EnumParser.ToName(x), typeof(T)));
                if (subscribe) _target.SubscribeProgressiveEvent<T>(EnumParser.ToName(x), handler.Object);
            });

            _mockEventBus.Setup(x => x.Publish(It.IsAny<T>()))
                .Callback<T>(e =>
                {
                    if (_protocolProgressiveEventHandlers.ContainsKey(typeof(T)))
                        // calling the mock event handlers Handle function which in turn call the NotifyHandlerCalled
                        _protocolProgressiveEventHandlers[typeof(T)].HandleProgressiveEvent(e);
                });
        }
    }
}
