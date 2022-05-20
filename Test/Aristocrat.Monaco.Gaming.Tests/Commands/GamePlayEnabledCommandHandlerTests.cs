namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System;
    using Contracts;
    using Gaming.Commands;
    using Gaming.Runtime;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Vgt.Client12.Application.OperatorMenu;

    [TestClass]
    public class GamePlayEnabledCommandHandlerTests
    {
        private GamePlayEnabledCommandHandler _target;
        private Mock<IResponsibleGaming> _responsibleGaming;
        private Mock<IRuntime> _runtime;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IGamePlayState> _gamePlayState;
        private Mock<IOperatorMenuLauncher> _operatorMenu;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _responsibleGaming = new Mock<IResponsibleGaming>(MockBehavior.Default);
            _runtime = new Mock<IRuntime>(MockBehavior.Default);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _gamePlayState = new Mock<IGamePlayState>(MockBehavior.Default);
            _operatorMenu = new Mock<IOperatorMenuLauncher>(MockBehavior.Default);

            _target = CreateGamePlayEnabledCommandHandler();
        }

        [DataRow(true, false, false, false, false, DisplayName = "Null Responsible Gaming Test")]
        [DataRow(false, true, false, false, false, DisplayName = "Null Runtime Service Test")]
        [DataRow(false, false, true, false, false, DisplayName = "Null Properties Manager Test")]
        [DataRow(false, false, false, true, false, DisplayName = "Null Game Play State Test")]
        [DataRow(false, false, false, false, true, DisplayName = "Null Operator Menu Launcher Test")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorTest(
            bool nullResponsibleGaming,
            bool nullRuntime,
            bool nullProperties,
            bool nullGamePlay,
            bool nullOperatorMenu)
        {
            _target = CreateGamePlayEnabledCommandHandler(
                nullResponsibleGaming,
                nullRuntime,
                nullProperties,
                nullGamePlay,
                nullOperatorMenu);
        }

        [DataRow(true, false, true, true, DisplayName = "Disable Operator Menu when in a game round and configured for disabled during game play")]
        [DataRow(true, false, true, true, DisplayName = "Disable Operator Menu when in a game round and configured for disabled during game play")]
        [DataRow(false, false, true, false, DisplayName = "Don't disable Operator Menu when not in a game round")]
        [DataRow(true, false, false, false, DisplayName = "Don't Disable Operator Menu when in a game round and not configured for disabled during game play")]
        [DataRow(true, true, true, false, DisplayName = "Don't Disable Operator Menu when in a game round but in presentation idle and configured for disabled during game play")]
        [DataRow(true, true, false, false, DisplayName = "Don't Disable Operator Menu when in a game round and not configured for disabled during game play")]
        [DataTestMethod]
        public void OperatorMenuDisabledTest(
            bool inGameRound,
            bool inPresentationIdle,
            bool operatorMenuDisableDuringGame,
            bool expectedDisable)
        {
            _gamePlayState.Setup(x => x.InGameRound).Returns(inGameRound);
            _gamePlayState.Setup(x => x.InPresentationIdle).Returns(inPresentationIdle);
            _propertiesManager
                .Setup(x => x.GetProperty(GamingConstants.OperatorMenuDisableDuringGame, It.IsAny<bool>()))
                .Returns(operatorMenuDisableDuringGame);
            _target.Handle(new GamePlayEnabled());
            if (expectedDisable)
            {
                _operatorMenu.Verify(x => x.DisableKey(GamingConstants.OperatorMenuDisableKey), Times.Once);
            }
            else
            {
                _operatorMenu.Verify(x => x.DisableKey(GamingConstants.OperatorMenuDisableKey), Times.Never);
            }
        }

        private GamePlayEnabledCommandHandler CreateGamePlayEnabledCommandHandler(
            bool nullResponsibleGaming = false,
            bool nullRuntime = false,
            bool nullProperties = false,
            bool nullGamePlay = false,
            bool nullOperatorMenu = false)
        {
            return new GamePlayEnabledCommandHandler(
                nullResponsibleGaming ? null : _responsibleGaming.Object,
                nullRuntime ? null : _runtime.Object,
                nullProperties ? null : _propertiesManager.Object,
                nullGamePlay ? null : _gamePlayState.Object,
                nullOperatorMenu ? null : _operatorMenu.Object);
        }
    }
}