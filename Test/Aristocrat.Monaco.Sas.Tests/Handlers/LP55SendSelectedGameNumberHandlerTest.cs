namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    [TestClass]
    public class LP55SendSelectedGameNumberHandlerTest
    {
        private LP55SendSelectedGameNumberHandler _target;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IGameProvider> _gameProvider;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);
            _target = new LP55SendSelectedGameNumberHandler(_propertiesManager.Object, _gameProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPropertiesManagerTest()
        {
            _target = new LP55SendSelectedGameNumberHandler(null, _gameProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullGameProviderTest()
        {
            _target = new LP55SendSelectedGameNumberHandler(_propertiesManager.Object, null);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendSelectedGameNumber));
        }

        [TestMethod]
        public void NoEnabledGameReturnsZero()
        {
            _propertiesManager.Setup(
                pm => pm.GetProperty(
                    It.Is<string>(p => p.Equals(GamingConstants.IsGameRunning)),
                    It.Is<bool>(d => !d))).Returns(false);
            var response = _target.Handle(new LongPollData());

            Assert.AreEqual(0, response.Data);
            _propertiesManager.VerifyAll();
        }

        [TestMethod]
        public void EnabledGameGetsSelectedId()
        {
            const int gameId = 5;
            const int expectedGameId = 5;
            const long denom = 1000;
            _propertiesManager.Setup(
                pm => pm.GetProperty(
                    It.Is<string>(p => p.Equals(GamingConstants.IsGameRunning)),
                    It.IsAny<bool>())).Returns(true);
            _propertiesManager.Setup(
                pm => pm.GetProperty(
                    It.Is<string>(p => p.Equals(GamingConstants.SelectedGameId)),
                    It.IsAny<int>())).Returns(gameId);
            _propertiesManager.Setup(
                pm => pm.GetProperty(
                    It.Is<string>(p => p.Equals(GamingConstants.SelectedDenom)),
                    It.IsAny<long>())).Returns(denom);

            var game = new Mock<IGameDetail>(MockBehavior.Default);
            game.Setup(x => x.Id).Returns(gameId);
            var denomination = new Mock<IDenomination>(MockBehavior.Default);
            denomination.Setup(x => x.Id).Returns(expectedGameId);
            denomination.Setup(x => x.Value).Returns(denom);
            game.Setup(x => x.Denominations).Returns(new List<IDenomination> { denomination.Object });

            _gameProvider.Setup(x => x.GetGame(gameId)).Returns(game.Object);

            var response = _target.Handle(new LongPollData());
            Assert.AreEqual(expectedGameId, response.Data);
            _propertiesManager.VerifyAll();
        }

        [TestMethod]
        public void EnabledGameGetsInvalidGameId()
        {
            const int gameId = 5;
            const int expectedGameId = 0;
            const long denom = 1000;
            _propertiesManager.Setup(
                pm => pm.GetProperty(
                    It.Is<string>(p => p.Equals(GamingConstants.IsGameRunning)),
                    It.IsAny<bool>())).Returns(true);
            _propertiesManager.Setup(
                pm => pm.GetProperty(
                    It.Is<string>(p => p.Equals(GamingConstants.SelectedGameId)),
                    It.IsAny<int>())).Returns(gameId);
            _propertiesManager.Setup(
                pm => pm.GetProperty(
                    It.Is<string>(p => p.Equals(GamingConstants.SelectedDenom)),
                    It.IsAny<long>())).Returns(denom);

            _gameProvider.Setup(x => x.GetGame(gameId)).Returns((IGameDetail)null);

            var response = _target.Handle(new LongPollData());
            Assert.AreEqual(expectedGameId, response.Data);
            _propertiesManager.VerifyAll();
        }
    }
}