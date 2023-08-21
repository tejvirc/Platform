namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Gaming.Contracts;
    using Gaming.Contracts.Lobby;
    using Gaming.Contracts.Models;
    using Sas.Handlers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class LPAAEnableDisableGameAutoRebetHandlerTest
    {
        private LPAAEnableDisableGameAutoRebetHandler _target;
        private readonly Mock<IAutoPlayStatusProvider> _autoPlayStatusProvider = new Mock<IAutoPlayStatusProvider>();
        private readonly Mock<IPropertiesManager> _properties = new Mock<IPropertiesManager>();
        private readonly Mock<IGameProvider> _gameProviderMock = new Mock<IGameProvider>();
        private readonly Mock<IGameDetail> _gameDetail = new Mock<IGameDetail>();
        private readonly Mock<ILobbyStateManager> _lobbyStateManager = new Mock<ILobbyStateManager>();

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LPAAEnableDisableGameAutoRebetHandler(_properties.Object, _autoPlayStatusProvider.Object, _gameProviderMock.Object, _lobbyStateManager.Object);

            _properties.Setup(m => m.GetProperty(GamingConstants.AutoPlayAllowed, true)).Returns(true);
            _properties.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, false)).Returns(true);
            _properties.Setup(m => m.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);
            _properties.Setup(m => m.GetProperty(GamingConstants.AwaitingPlayerSelection, false)).Returns(false);

            _gameProviderMock.Setup(g => g.GetGame(1)).Returns(_gameDetail.Object);
            _gameDetail.Setup(m => m.AutoPlaySupported).Returns(true);
            _lobbyStateManager.Setup(x => x.CurrentState).Returns(LobbyState.Game);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.EnableDisableGameAutoRebet));
        }

        [TestMethod]
        public void HandleEnableTest()
        {
            // enable auto play
            byte enable = 1;
            _autoPlayStatusProvider.Setup(m => m.StartSystemAutoPlay()).Verifiable();
            var actual = _target.Handle(new LongPollSingleValueData<byte>(enable));
            Assert.IsTrue(actual.Data);
        }

        [TestMethod]
        public void HandleDisableTest()
        {
            //disable auto play
            byte enable = 0;
            _autoPlayStatusProvider.Setup(m => m.StopSystemAutoPlay()).Verifiable();
            var actual = _target.Handle(new LongPollSingleValueData<byte>(enable));
            Assert.IsTrue(actual.Data);
        }

        [TestMethod]
        public void HandleInvalidEnableValueTest()
        {
            //auto play invalid command
            byte enable = 2;
            var actual = _target.Handle(new LongPollSingleValueData<byte>(enable));
            Assert.IsFalse(actual.Data);
        }

        [TestMethod]
        public void HandleJurisdictionAutoPlayDisabledTest()
        {
            //auto play command for jurisdiction which doesn't allow auto play
            _properties.Setup(m => m.GetProperty(GamingConstants.AutoPlayAllowed, true)).Returns(false);
            byte enable = 1;
            var actual = _target.Handle(new LongPollSingleValueData<byte>(enable));
            Assert.IsFalse(actual.Data);
        }

        [TestMethod]
        public void HandleAutoPlayNotSupportedByGame()
        {
            _gameDetail.Setup(m => m.AutoPlaySupported).Returns(false);
            byte enable = 1;
            var actual = _target.Handle(new LongPollSingleValueData<byte>(enable));
            Assert.IsFalse(actual.Data);
        }

        [TestMethod]
        public void HandleGameNotSelectedByPlayer()
        {
            _properties.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, false)).Returns(false);
            byte enable = 1;
            var actual = _target.Handle(new LongPollSingleValueData<byte>(enable));
            Assert.IsFalse(actual.Data);
        }

        [DataRow(true, DisplayName = "When awaiting player selection, should not enable")]
        [DataRow(false, DisplayName = "When NOT awaiting player selection, should  enable")]
        [DataTestMethod]
        public void HandleAwaitingPlayerSelectionTest(bool awaitingPlayerSelection)
        {
            _properties.Setup(m => m.GetProperty(GamingConstants.AwaitingPlayerSelection, false)).Returns(awaitingPlayerSelection);
            byte enable = 1;
            var actual = _target.Handle(new LongPollSingleValueData<byte>(enable));
            Assert.AreEqual(actual.Data, !awaitingPlayerSelection);
        }
    }
}
