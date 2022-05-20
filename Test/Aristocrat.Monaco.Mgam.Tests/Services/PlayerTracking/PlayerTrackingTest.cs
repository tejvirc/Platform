namespace Aristocrat.Monaco.Mgam.Tests.Services.PlayerTracking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Common.Events;
    using Mgam.Services.Attributes;
    using Mgam.Services.PlayerTracking;
    using Gaming.Contracts;
    using Gaming.Contracts.InfoBar;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class PlayerTrackingTest
    {

        private Mock<ILogger<PlayerTracking>> _logger;
        private Mock<IAttributeManager> _attributes;
        private Mock<IEventBus> _eventBus;
        private PlayerTracking _target;

        private Action<AttributeChangedEvent> _attributeChangedHandler;
        private Action<WagerPlacedEvent> _wagerPlacedHandler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _logger = new Mock<ILogger<PlayerTracking>>(MockBehavior.Default);
            _attributes = new Mock<IAttributeManager>(MockBehavior.Default);
            _eventBus = new Mock<IEventBus>(MockBehavior.Default);

            _eventBus
                .Setup(
                    x => x.Subscribe(
                        It.IsAny<PlayerTracking>(),
                        It.IsAny<Action<AttributeChangedEvent>>()))
                .Callback<object, Action<AttributeChangedEvent>>((o, action) => _attributeChangedHandler = action);

            _eventBus
                .Setup(
                    x => x.Subscribe(
                        It.IsAny<PlayerTracking>(),
                        It.IsAny<Action<WagerPlacedEvent>>()))
                .Callback<object, Action<WagerPlacedEvent>>((o, action) => _wagerPlacedHandler = action);
        }

        [TestMethod]
        public void EndPlayerSessionTest()
        {
            InfoBarRegion expectedPlayerNameRegion = InfoBarRegion.Left;
            InfoBarRegion expectedPlayerPointsRegion = InfoBarRegion.Right;
            InfoBarRegion expectedPromotionalInfoRegion = InfoBarRegion.Center;
            int expectedEventCount = 3;

            List<InfoBarClearMessageEvent> actualEvents = new List<InfoBarClearMessageEvent>();

            _eventBus
                .Setup(x => x.Publish(It.IsAny<InfoBarClearMessageEvent>()))
                .Callback<InfoBarClearMessageEvent>((e) => actualEvents.Add(e))
                .Verifiable();

            _target = new PlayerTracking(_logger.Object, _attributes.Object, _eventBus.Object);
            _target.EndPlayerSession();

            Assert.AreEqual(expectedEventCount, actualEvents.Count);
            Assert.IsTrue(actualEvents.Any(x => x.Regions[0] == expectedPlayerNameRegion));
            Assert.IsTrue(actualEvents.Any(x => x.Regions[0] == expectedPlayerPointsRegion));
            Assert.IsTrue(actualEvents.Any(x => x.Regions[0] == expectedPromotionalInfoRegion));
        }

        [TestMethod]
        public void StartPlayerSessionTest()
        {
            InfoBarRegion expectedPlayerNameRegion = InfoBarRegion.Left;
            InfoBarRegion expectedPlayerPointsRegion = InfoBarRegion.Right;
            InfoBarRegion expectedPromotionalInfoRegion = InfoBarRegion.Center;
            string playerName = "Name";
            int playerPoints = 123;
            string expectedPlayerNameMessage = $"Player: {playerName}";
            string expectedPlayerPointsMessage = $"Points= {playerPoints:N0}";
            string expectedPromotionalInfoMessage = "Welcome";
            int expectedEventCount = 3;

            List<InfoBarDisplayStaticMessageEvent> actualEvents = new List<InfoBarDisplayStaticMessageEvent>();
            InfoBarDisplayTransientMessageEvent actualPromotionalInfoEvent = null;

            _attributes.Setup(x => x.Get(AttributeNames.PlayerTrackingPoints, 0)).Returns(playerPoints);

            _eventBus
                .Setup(x => x.Publish(It.IsAny<InfoBarDisplayStaticMessageEvent>()))
                .Callback<InfoBarDisplayStaticMessageEvent>((e) => actualEvents.Add(e))
                .Verifiable();

            _eventBus
                .Setup(x => x.Publish(It.IsAny<InfoBarDisplayTransientMessageEvent>()))
                .Callback<InfoBarDisplayTransientMessageEvent>((e) => actualPromotionalInfoEvent = e)
                .Verifiable();

            _target = new PlayerTracking(_logger.Object, _attributes.Object, _eventBus.Object);
            _target.StartPlayerSession(playerName, playerPoints, expectedPromotionalInfoMessage);
            _attributeChangedHandler.Invoke(new AttributeChangedEvent(AttributeNames.PlayerTrackingPoints));

            Assert.AreEqual(expectedEventCount, actualEvents.Count);
            Assert.IsTrue(actualEvents.Any(x => x.Region == expectedPlayerNameRegion && x.Message == expectedPlayerNameMessage));
            Assert.IsTrue(actualEvents.Any(x => x.Region == expectedPlayerPointsRegion && x.Message == expectedPlayerPointsMessage));
            Assert.AreEqual(expectedPromotionalInfoRegion, actualPromotionalInfoEvent.Region);
            Assert.AreEqual(expectedPromotionalInfoMessage, actualPromotionalInfoEvent.Message);
        }

        [TestMethod]
        public void AttributeChangedHandlersTest()
        {
            InfoBarRegion expectedPlayerPointsRegion = InfoBarRegion.Right;
            InfoBarRegion expectedPromotionalInfoRegion = InfoBarRegion.Center;
            string playerName = "Name";
            int playerPoints = 321;
            string expectedPlayerPointsMessage = $"Points= {playerPoints:N0}";
            string expectedPromotionalInfoMessage = "Welcome Again";

            _attributes.Setup(x => x.Get(AttributeNames.PlayerTrackingPoints, 0)).Returns(playerPoints);
            _attributes.Setup(x => x.Get(AttributeNames.PromotionalInfo, string.Empty)).Returns(expectedPromotionalInfoMessage);

            InfoBarDisplayStaticMessageEvent actualPlayerPointsEvent = null;
            InfoBarDisplayTransientMessageEvent actualPromotionalInfoEvent = null;

            _eventBus
                .Setup(x => x.Publish(It.IsAny<InfoBarDisplayStaticMessageEvent>()))
                .Callback<InfoBarDisplayStaticMessageEvent>((e) => actualPlayerPointsEvent = e)
                .Verifiable();

            _eventBus
                .Setup(x => x.Publish(It.IsAny<InfoBarDisplayTransientMessageEvent>()))
                .Callback<InfoBarDisplayTransientMessageEvent>((e) => actualPromotionalInfoEvent = e)
                .Verifiable();

            _target = new PlayerTracking(_logger.Object, _attributes.Object, _eventBus.Object);
            _target.StartPlayerSession(playerName, playerPoints, string.Empty);

            _attributeChangedHandler.Invoke(new AttributeChangedEvent(AttributeNames.PlayerTrackingPoints));
            _attributeChangedHandler.Invoke(new AttributeChangedEvent(AttributeNames.PromotionalInfo));

            Assert.AreEqual(expectedPlayerPointsRegion, actualPlayerPointsEvent.Region);
            Assert.AreEqual(expectedPlayerPointsMessage, actualPlayerPointsEvent.Message);
            Assert.AreEqual(expectedPromotionalInfoRegion, actualPromotionalInfoEvent.Region);
            Assert.AreEqual(expectedPromotionalInfoMessage, actualPromotionalInfoEvent.Message);
        }

        [TestMethod]
        public void WagerPlacedTest()
        {
            string playerName = "Name";
            int initialPlayerPoints = 100;
            int penniesPerPoint = 12;
            int wagerPennies = 60;
            int expectedUpdatedPoints = 105;
            string expectedInitialPlayerPointsMessage = $"Points= {initialPlayerPoints:N0}";
            string expectedUpdatedPlayerPointsMessage = $"Points= {expectedUpdatedPoints:N0}";

            _attributes.Setup(x => x.Get(AttributeNames.PlayerTrackingPoints, 0)).Returns(initialPlayerPoints);
            _attributes.Setup(x => x.Get(AttributeNames.PenniesPerPoint, 0)).Returns(penniesPerPoint);

            InfoBarDisplayStaticMessageEvent actualPlayerPointsEvent = null;

            _eventBus
                .Setup(x => x.Publish(It.IsAny<InfoBarDisplayStaticMessageEvent>()))
                .Callback<InfoBarDisplayStaticMessageEvent>((e) => actualPlayerPointsEvent = e)
                .Verifiable();

            var wagerEvent = new WagerPlacedEvent(wagerPennies);
            _eventBus.Setup(x => x.Publish(wagerEvent));

            _target = new PlayerTracking(_logger.Object, _attributes.Object, _eventBus.Object);
            _target.StartPlayerSession(playerName, initialPlayerPoints, string.Empty);

            _attributeChangedHandler.Invoke(new AttributeChangedEvent(AttributeNames.PlayerTrackingPoints));
            _attributeChangedHandler.Invoke(new AttributeChangedEvent(AttributeNames.PenniesPerPoint));

            Assert.AreEqual(expectedInitialPlayerPointsMessage, actualPlayerPointsEvent.Message);

            _wagerPlacedHandler.Invoke(wagerEvent);
            _attributes.Setup(x => x.Get(AttributeNames.PlayerTrackingPoints, 0)).Returns(expectedUpdatedPoints);
            _attributeChangedHandler.Invoke(new AttributeChangedEvent(AttributeNames.PlayerTrackingPoints));

            Assert.AreEqual(expectedUpdatedPlayerPointsMessage, actualPlayerPointsEvent.Message);
        }
    }
}
