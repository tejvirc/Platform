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
    using Vgt.Client12.Application.OperatorMenu;

    [TestClass]
    public class GameExitedNormalConsumerTest
    {
        private Mock<ISasExceptionHandler> _exceptionHandlerMock;
        private Mock<IPropertiesManager> _propertiesManagerMock;
        private Mock<IOperatorMenuLauncher> _launcherMock;
        private Mock<ILobbyStateManager> _lobbyStateManagerMock;

        private GameExitedNormalConsumer _target;

        [TestInitialize]
        public void Initialize()
        {
            _exceptionHandlerMock = new Mock<ISasExceptionHandler>(MockBehavior.Strict);
            _propertiesManagerMock = new Mock<IPropertiesManager>(MockBehavior.Default);
            _launcherMock = new Mock<IOperatorMenuLauncher>(MockBehavior.Strict);
            _lobbyStateManagerMock = new Mock<ILobbyStateManager>(MockBehavior.Default);

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _target = new GameExitedNormalConsumer(_exceptionHandlerMock.Object, _propertiesManagerMock.Object, _launcherMock.Object, _lobbyStateManagerMock.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataTestMethod]
        [DataRow(true, DisplayName="Operator Menu shown (no $8c sent")]
        [DataRow(false, DisplayName = "Operator Menu not shown ($8c sent")]
        public void ConsumeEventTestUnexpectedExit(bool operatorMenuShowing)
        {
            var times = operatorMenuShowing ? Times.Never() : Times.Once();
            var expected = operatorMenuShowing ? 1 : 0;

            const int gameId = 1;
            var expectedResult = new GameSelectedExceptionBuilder(expected);
            GameSelectedExceptionBuilder actual = null;

            _exceptionHandlerMock.Setup(m => m.ReportException(It.IsAny<GameSelectedExceptionBuilder>()))
                .Callback((ISasExceptionCollection g) => actual = g as GameSelectedExceptionBuilder)
                .Verifiable();

            _propertiesManagerMock.Setup(p => p.GetProperty(SasProperties.PreviousSelectedGameId, It.IsAny<int>()))
                .Returns(gameId);

            _launcherMock.Setup(l => l.IsShowing).Returns(operatorMenuShowing);

            var @event = new GameExitedNormalEvent();

            _target.Consume(@event);

            if (!operatorMenuShowing)
            {
                Assert.IsNotNull(actual);
                CollectionAssert.AreEquivalent(expectedResult, actual);
            }

            _exceptionHandlerMock.Verify(m => m.ReportException(It.IsAny<GameSelectedExceptionBuilder>()), times);
        }

        [TestMethod]
        public void ConsumeEventTestExpectedExit()
        {
            var actual = 1;
            const int gameId = 0;

            _propertiesManagerMock.Setup(p => p.GetProperty(SasProperties.PreviousSelectedGameId, It.IsAny<int>()))
                .Returns(gameId);

            var @event = new GameExitedNormalEvent();

            _target.Consume(@event);

            Assert.AreEqual(1, actual);
        }
    }
}
