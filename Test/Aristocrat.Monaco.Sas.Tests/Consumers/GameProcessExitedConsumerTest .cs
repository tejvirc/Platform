namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Sas.Client;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Consumers;
    using Sas.Exceptions;
    using Test.Common;

    [TestClass]
    public class GameProcessExitedConsumerTest
    {
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private Mock<IPropertiesManager> _propertiesManagerMock;
        private Mock<ILobbyStateManager> _lobbyStateManagerMock;

        private GameProcessExitedConsumer _target;

        [TestInitialize]
        public void Initialize()
        {
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Strict);
            _propertiesManagerMock = new Mock<IPropertiesManager>(MockBehavior.Default);
            _lobbyStateManagerMock = new Mock<ILobbyStateManager>(MockBehavior.Default);
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _target = new GameProcessExitedConsumer(_exceptionHandler.Object, _propertiesManagerMock.Object, _lobbyStateManagerMock.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConsumeEventTestUnexpectedExit()
        {
            const int gameId = 1;
            var expectedResult = new GameSelectedExceptionBuilder(0);
            GameSelectedExceptionBuilder actual = null;

            _exceptionHandler.Setup(m => m.ReportException(It.IsAny<GameSelectedExceptionBuilder>()))
                .Callback((ISasExceptionCollection g) => actual = g as GameSelectedExceptionBuilder)
                .Verifiable();

            _propertiesManagerMock.Setup(p => p.GetProperty(SasProperties.PreviousSelectedGameId, It.IsAny<int>()))
                .Returns(gameId);

            var @event = new GameProcessExitedEvent(0, true);

            _target.Consume(@event);

            Assert.IsNotNull(actual);
            CollectionAssert.AreEquivalent(expectedResult, actual);

            _exceptionHandler.Verify();
        }

        [TestMethod]
        public void ConsumeEventTestExpectedExit()
        {
            int actual = 1;
            int gameId = 0;

            _propertiesManagerMock.Setup(p => p.GetProperty(SasProperties.PreviousSelectedGameId, It.IsAny<int>()))
                .Returns(gameId);

            var @event = new GameProcessExitedEvent(0, false);

            _target.Consume(@event);

            Assert.AreEqual(1, actual);
        }
    }
}
