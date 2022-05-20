namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    ///     Contains the unit tests for the PokerHandProvider class
    /// </summary>
    [TestClass]
    public class PokerHandProviderTests
    {
        private PokerHandProvider _target;
        private readonly List<HoldStatus> _noCardsHeld = new List<HoldStatus> { HoldStatus.NotHeld, HoldStatus.NotHeld, HoldStatus.NotHeld, HoldStatus.NotHeld, HoldStatus.NotHeld };
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>(MockBehavior.Strict);
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _propertiesManager.Setup(m => m.SetProperty(GamingConstants.PokerHandInformation, It.IsAny<HandInformation>())).Verifiable();

            _target = new PokerHandProvider(_eventBus.Object, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullEventBusTest()
        {
            _target = new PokerHandProvider(null, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullPropertiesManagerTest()
        {
            _target = new PokerHandProvider(_eventBus.Object, null);
        }

        [TestMethod]
        public void DealTest()
        {
            List<GameCard> expectedDealCards = new List<GameCard> { GameCard.Spades5, GameCard.SpadesKing, GameCard.ClubsAce, GameCard.DiamondsQueen, GameCard.HeartsKing };
            var dealCards = new List<Hand>
            {
                new Hand { Row = 0, Cards = new long[] { 3, 11, 38, 49, 24 }},
                new Hand { Row = 1, Cards = new long[0]},
                new Hand { Row = 2, Cards = new long[0]}
            };

            _target.UpdateDealtCards(dealCards);

            Assert.IsFalse(_target.FinalHand);
            CollectionAssert.AreEqual(_noCardsHeld, _target.CardsHeld.ToArray());
            CollectionAssert.AreEqual(expectedDealCards, _target.CurrentHand.ToArray());
            _propertiesManager.Verify(
                m => m.SetProperty(GamingConstants.PokerHandInformation, It.IsAny<HandInformation>()),
                Times.Once);

            _eventBus.Verify(m => m.Publish(It.IsAny<CardsHeldEvent>()), Times.Never);
        }

        [TestMethod]
        public void DrawHoldTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<CardsHeldEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<PlayerRequestedDrawEvent>())).Verifiable();
            List<GameCard> expectedFinalCards = new List<GameCard> { GameCard.Spades5, GameCard.SpadesQueen, GameCard.ClubsAce, GameCard.DiamondsQueen, GameCard.Clubs6 };
            List<HoldStatus> expectedCardsHeld = new List<HoldStatus> { HoldStatus.Held, HoldStatus.NotHeld, HoldStatus.Held, HoldStatus.Held, HoldStatus.NotHeld };

            var dealCards = new List<Hand>
            {
                new Hand { Row = 0, Cards = new long[] { 3, 11, 38, 49, 24 }},
                new Hand { Row = 1, Cards = new long[0]},
                new Hand { Row = 2, Cards = new long[0]}
            };

            _target.UpdateDealtCards(dealCards);

            var drawCards = new List<Hand>
            {
                new Hand { Row = 0, Cards = new long[] { 10, 30 }},
                new Hand { Row = 1, Cards = new long[0]},
                new Hand { Row = 2, Cards = new long[0]}
            };

            var holdCards = new List<Hand>
            {
                new Hand { Row = 0, Cards = new long[] { 3, 38, 49 }},
                new Hand { Row = 1, Cards = new long[0]},
                new Hand { Row = 2, Cards = new long[0]}
            };

            _target.UpdateHoldCards(holdCards);
            _target.UpdateDrawCards(drawCards);

            Assert.IsTrue(_target.FinalHand);
            CollectionAssert.AreEqual(expectedCardsHeld, _target.CardsHeld.ToArray());
            CollectionAssert.AreEqual(expectedFinalCards, _target.CurrentHand.ToArray());
            _propertiesManager.Verify(
                m => m.SetProperty(GamingConstants.PokerHandInformation, It.IsAny<HandInformation>()),
                Times.Exactly(3));
            _eventBus.Verify(m => m.Publish(It.IsAny<CardsHeldEvent>()), Times.Once);
        }

        [TestMethod]
        public void AllHeldTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<CardsHeldEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<PlayerRequestedDrawEvent>())).Verifiable();
            List<GameCard> expectedFinalCards = new List<GameCard> { GameCard.Spades5, GameCard.SpadesKing, GameCard.ClubsAce, GameCard.DiamondsQueen, GameCard.HeartsKing };
            List<HoldStatus> expectedCardsHeld = new List<HoldStatus> { HoldStatus.Held, HoldStatus.Held, HoldStatus.Held, HoldStatus.Held, HoldStatus.Held };

            var dealCards = new List<Hand>
            {
                new Hand { Row = 0, Cards = new long[] { 3, 11, 38, 49, 24 }},
                new Hand { Row = 1, Cards = new long[0]},
                new Hand { Row = 2, Cards = new long[0]}
            };

            _target.UpdateDealtCards(dealCards);

            var drawCards = new List<Hand>
            {
                new Hand { Row = 0, Cards = new long[0]},
                new Hand { Row = 1, Cards = new long[0]},
                new Hand { Row = 2, Cards = new long[0]}
            };

            var holdCards = new List<Hand>
            {
                new Hand { Row = 0, Cards = new long[] { 3, 11, 38, 49, 24 }},
                new Hand { Row = 1, Cards = new long[0]},
                new Hand { Row = 2, Cards = new long[0]}
            };

            _target.UpdateHoldCards(holdCards);
            _target.UpdateDrawCards(drawCards);

            Assert.IsTrue(_target.FinalHand);
            CollectionAssert.AreEqual(expectedCardsHeld, _target.CardsHeld.ToArray());
            CollectionAssert.AreEqual(expectedFinalCards, _target.CurrentHand.ToArray());
            _propertiesManager.Verify(
                m => m.SetProperty(GamingConstants.PokerHandInformation, It.IsAny<HandInformation>()),
                Times.Exactly(3));
            _eventBus.Verify(m => m.Publish(It.IsAny<CardsHeldEvent>()), Times.Once);
        }

        [TestMethod]
        public void NoneHeldTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<CardsHeldEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<PlayerRequestedDrawEvent>())).Verifiable();
            List<GameCard> expectedFinalCards = new List<GameCard> { GameCard.SpadesQueen, GameCard.Hearts9, GameCard.Clubs6, GameCard.Diamonds3, GameCard.DiamondsKing };
            List<HoldStatus> expectedCardsHeld = new List<HoldStatus> { HoldStatus.NotHeld, HoldStatus.NotHeld, HoldStatus.NotHeld, HoldStatus.NotHeld, HoldStatus.NotHeld };

            var dealCards = new List<Hand>
            {
                new Hand { Row = 0, Cards = new long[] { 3, 11, 38, 49, 24 }},
                new Hand { Row = 1, Cards = new long[0]},
                new Hand { Row = 2, Cards = new long[0]}
            };

            _target.UpdateDealtCards(dealCards);

            var drawCards = new List<Hand>
            {
                new Hand { Row = 0, Cards = new long[] { 10, 20, 30, 40, 50 }},
                new Hand { Row = 1, Cards = new long[0]},
                new Hand { Row = 2, Cards = new long[0]}
            };

            var holdCards = new List<Hand>
            {
                new Hand { Row = 0, Cards = new long[0]},
                new Hand { Row = 1, Cards = new long[0]},
                new Hand { Row = 2, Cards = new long[0]}
            };

            _target.UpdateHoldCards(holdCards);
            _target.UpdateDrawCards(drawCards);

            Assert.IsTrue(_target.FinalHand);
            CollectionAssert.AreEqual(expectedCardsHeld, _target.CardsHeld.ToArray());
            CollectionAssert.AreEqual(expectedFinalCards, _target.CurrentHand.ToArray());
            _propertiesManager.Verify(
                m => m.SetProperty(GamingConstants.PokerHandInformation, It.IsAny<HandInformation>()),
                Times.Exactly(3));
            _eventBus.Verify(m => m.Publish(It.IsAny<CardsHeldEvent>()), Times.Once);
        }
    }
}
