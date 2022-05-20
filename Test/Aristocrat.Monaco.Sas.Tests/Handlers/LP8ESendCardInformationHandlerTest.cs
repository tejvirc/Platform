namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    /// <summary>
    /// Contains unit tests for LP8ESendCardInformationHandler
    /// </summary>
    [TestClass]
    public class LP8ESendCardInformationHandlerTest
    {
        private LP8ESendCardInformationHandler _target;
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);
        private readonly HandInformation _defaultHandInformation = new HandInformation();
        private readonly HandInformation _testHand =
            new HandInformation
            {
                Cards = new [] { GameCard.Joker, GameCard.Clubs3, GameCard.Hearts4, GameCard.Diamonds5, GameCard.Spades2 }, 
                IsHeld = new [] { HoldStatus.Held, HoldStatus.Held, HoldStatus.Held, HoldStatus.Held, HoldStatus.Held },
                FinalHand = true
            };

        [TestInitialize]
        public void TestInitialize()
        {
            _target = new LP8ESendCardInformationHandler(_propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullPropertiesManagerTest()
        {
            _target = new LP8ESendCardInformationHandler(null);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendCardInformation));
        }

        [TestMethod]
        public void HandleDefaultTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.PokerHandInformation, It.IsAny<HandInformation>())).Returns(_defaultHandInformation);

            var result = _target.Handle(null);
            Assert.AreEqual(SasCard.Unknown, result.Card1);
            Assert.AreEqual(SasCard.Unknown, result.Card2);
            Assert.AreEqual(SasCard.Unknown, result.Card3);
            Assert.AreEqual(SasCard.Unknown, result.Card4);
            Assert.AreEqual(SasCard.Unknown, result.Card5);
            Assert.IsFalse(result.FinalHand);
        }

        [TestMethod]
        public void HandleJokerTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.PokerHandInformation, It.IsAny<HandInformation>())).Returns(_testHand);

            var result = _target.Handle(null);
            Assert.AreEqual(SasCard.Joker, result.Card1);
            Assert.AreEqual(SasCard.Clubs3, result.Card2);
            Assert.AreEqual(SasCard.Hearts4, result.Card3);
            Assert.AreEqual(SasCard.Diamonds5, result.Card4);
            Assert.AreEqual(SasCard.Spades2, result.Card5);
            Assert.IsTrue(result.FinalHand);
        }
    }
}
