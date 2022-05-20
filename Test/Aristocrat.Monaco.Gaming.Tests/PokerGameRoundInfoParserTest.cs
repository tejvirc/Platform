namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using GameRound;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    ///     Contains unit tests for the PokerGameRoundInfoParser class
    /// </summary>
    [TestClass]
    public class PokerGameRoundInfoParserTest
    {
        private PokerGameRoundInfoParser _target;
        private readonly Mock<IHandProvider> _provider = new Mock<IHandProvider>(MockBehavior.Strict);
        private readonly Mock<IGamePlayState> _gamePlayState = new Mock<IGamePlayState>(MockBehavior.Default);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _gamePlayState.Setup(x => x.InGameRound).Returns(true);
            _target = new PokerGameRoundInfoParser(_provider.Object, _gamePlayState.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullHandProviderTest()
        {
            _target = new PokerGameRoundInfoParser(null, _gamePlayState.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullGamePlayStateTest()
        {
            _target = new PokerGameRoundInfoParser(_provider.Object, null);
        }


        [TestMethod]
        public void ToFewEntriesTest()
        {
            _provider.Setup(m => m.UpdateDealtCards(It.IsAny<List<Hand>>())).Verifiable();
            var gameRoundInfo = new List<string> { "poker", "1" };
            _target.UpdateGameRoundInfo(gameRoundInfo);

            _provider.Verify(m => m.UpdateDealtCards(It.IsAny<List<Hand>>()), Times.Never);
        }

        [TestMethod]
        public void InvalidJsonTest()
        {
            _provider.Setup(m => m.UpdateDealtCards(It.IsAny<List<Hand>>())).Verifiable();
            var invalidJson = new List<string>
            {
                "poker",
                "1",
                "{ \"deal\": [{\"row\": 0, cards: [3, 11, 38, 49, 24]}{\"row\": 1, cards: [,]}{\"row\": 2, cards: [,]}]}"
            };

            _target.UpdateGameRoundInfo(invalidJson);

            _provider.Verify(m => m.UpdateDealtCards(It.IsAny<List<Hand>>()), Times.Never);
        }

        [TestMethod]
        public void DealAndDrawTest()
        {
            _provider.Setup(m => m.UpdateDealtCards(It.IsAny<List<Hand>>())).Verifiable();
            _provider.Setup(m => m.UpdateDrawCards(It.IsAny<List<Hand>>())).Verifiable();
            _provider.Setup(m => m.UpdateHoldCards(It.IsAny<List<Hand>>())).Verifiable();
            var deal = new List<string>
            {
                "poker",
                "1",
                "{ \"deal\": [{\"row\": 0, cards: [3, 11, 38, 49, 24]},{\"row\": 1, cards: []},{\"row\": 2, cards: []}]}"
            };

            var drawHold = new List<string>
            {
                "poker",
                "1",
                "{ \"hold\": [{\"row\": 0, cards: [3, 38, 49]},{\"row\": 1, cards: []},{\"row\": 2, cards: []}]}",
                "{ \"draw\": [{\"row\": 0, cards: [10, 30]},{\"row\": 1, cards: []},{\"row\": 2, cards: []}]}"
            };

            _target.UpdateGameRoundInfo(deal);
            _target.UpdateGameRoundInfo(drawHold);

            _provider.Verify(m => m.UpdateDealtCards(It.IsAny<List<Hand>>()), Times.Once);
            _provider.Verify(m => m.UpdateDrawCards(It.IsAny<List<Hand>>()), Times.Once);
            _provider.Verify(m => m.UpdateHoldCards(It.IsAny<List<Hand>>()), Times.Once);
        }
    }
}
