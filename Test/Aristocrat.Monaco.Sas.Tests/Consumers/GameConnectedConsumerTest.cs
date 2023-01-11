namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Sas.Contracts.SASProperties;
    using Aristocrat.Sas.Client;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Consumers;
    using Sas.Exceptions;
    using Test.Common;

    [TestClass]
    public class GameConnectedConsumerTest
    {
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private Mock<IPropertiesManager> _propertiesManagerMock;
        private Mock<IGameProvider> _gameProvider;
        private GameConnectedConsumer _target;

        [TestInitialize]
        public void Initialize()
        {
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Strict);
            _propertiesManagerMock = new Mock<IPropertiesManager>(MockBehavior.Default);
            _gameProvider = new Mock<IGameProvider>(MockBehavior.Strict);

            // MoqServiceManager and eventBus mock are required for the Consumes base class constructor.
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _target = new GameConnectedConsumer(_exceptionHandler.Object, _propertiesManagerMock.Object, _gameProvider.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullExceptionHandlerTest()
        {
            _target = new GameConnectedConsumer(null, _propertiesManagerMock.Object, _gameProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPropertiesManagerTest()
        {
            _target = new GameConnectedConsumer(_exceptionHandler.Object, null, _gameProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullGameProviderTest()
        {
            _target = new GameConnectedConsumer(_exceptionHandler.Object, _propertiesManagerMock.Object, null);
        }

        [TestMethod]
        public void ConsumeEventTestWithInstalledGamesReplay()
        {
            int actual = 0;
            var mockGame1 = new Mock<IGameDetail>();
            var mockGame2 = new Mock<IGameDetail>();

            _gameProvider.Setup(m => m.GetEnabledGames())
                .Returns(new List<IGameDetail> { mockGame1.Object, mockGame2.Object });

            var @event = new GameConnectedEvent(true);

            _target.Consume(@event);

            Assert.AreEqual(0, actual);
        }

        [TestMethod]
        public void ConsumeEventTestOneInstalledGame()
        {
            const int gameId = 1;
            const long denom = 1000;
            const int denomId = 13;
            var expectedResult = new GameSelectedExceptionBuilder(denomId);
            GameSelectedExceptionBuilder actual = null;

            _exceptionHandler.Setup(m => m.ReportException(It.IsAny<GameSelectedExceptionBuilder>()))
                .Callback((ISasExceptionCollection g) => actual = g as GameSelectedExceptionBuilder)
                .Verifiable();
            var mockGame = new Mock<IGameDetail>();
            mockGame.Setup(x => x.Id).Returns(gameId);
            var mockDenom = new Mock<IDenomination>();
            mockDenom.Setup(x => x.Id).Returns(denomId);
            mockDenom.Setup(x => x.Value).Returns(denom);
            mockGame.Setup(x => x.Denominations).Returns(new List<IDenomination> { mockDenom.Object });

            _gameProvider.Setup(m => m.GetGame(gameId))
                .Returns(mockGame.Object);
            _propertiesManagerMock.Setup(p => p.GetProperty(GamingConstants.SelectedGameId, It.IsAny<int>()))
                .Returns(gameId);
            _propertiesManagerMock.Setup(x => x.GetProperty(GamingConstants.SelectedDenom, It.IsAny<long>()))
                .Returns(denom);
            _propertiesManagerMock.Setup(p => p.GetProperty(SasProperties.PreviousSelectedGameId, It.IsAny<int>()))
                .Returns(0);

            var @event = new GameConnectedEvent(false);

            _target.Consume(@event);

            Assert.IsNotNull(actual);
            CollectionAssert.AreEquivalent(expectedResult, actual);
            _exceptionHandler.Verify();
        }
    }
}
