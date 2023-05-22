namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Gaming.Contracts.Configuration;
    using Contracts;
    using Contracts.Lobby;
    using Gaming.Commands;
    using Gaming.Runtime;
    using Gaming.Runtime.Client;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.Cabinet;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ConfigureClientCommandHandlerTests
    {
        private Mock<IPlayerBank> _playerBank;
        private Mock<IRuntime> _runtime;
        private Mock<IGameHistory> _gameHistory;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ILobbyStateManager> _lobbyStateManager;
        private Mock<IGameRecovery> _gameRecovery;
        private Mock<IGameDiagnostics> _gameDiagnostic;
        private Mock<IAudio> _audio;
        private Mock<IGameProvider> _gameProvider;
        private Mock<IGameCategoryService> _gameCategoryService;
        private Mock<ICabinetDetectionService> _cabinetDetectionService;
        private Mock<IGameHelpTextProvider> _helpTextProvider;
        private Mock<IHardwareHelper> _hardwareHelper;
        private Mock<IAttendantService> _attendantService;
        private Mock<IGameConfigurationProvider> _gameConfiguration;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _playerBank = new Mock<IPlayerBank>();
            _gameHistory = new Mock<IGameHistory>();
            _propertiesManager = new Mock<IPropertiesManager>();
            _lobbyStateManager = new Mock<ILobbyStateManager>();
            _gameRecovery = new Mock<IGameRecovery>();
            _gameDiagnostic = new Mock<IGameDiagnostics>();
            _audio = new Mock<IAudio>();
            _gameProvider = new Mock<IGameProvider>();
            _gameCategoryService = new Mock<IGameCategoryService>();
            _cabinetDetectionService = new Mock<ICabinetDetectionService>();
            _helpTextProvider = new Mock<IGameHelpTextProvider>();
            _hardwareHelper = new Mock<IHardwareHelper>();
            _runtime = new Mock<IRuntime>();
            _attendantService = new Mock<IAttendantService>();
            _gameConfiguration = new Mock<IGameConfigurationProvider>();
            _attendantService.Setup(attendant => attendant.IsServiceRequested).Returns(true);
            _propertiesManager.Setup(manager => manager.GetProperty(It.IsAny<string>(), It.IsAny<object>()))
                .Returns<string, object>((_, o) => o);
            var currentGame = new Mock<IGameDetail>();
            currentGame.Setup(m => m.Denominations)
                .Returns(new List<IDenomination> { new Mock<IDenomination>().Object });
            _gameProvider.Setup(m => m.GetGame(It.IsAny<int>())).Returns(currentGame.Object);
            _gameProvider.Setup(m => m.GetEnabledGames()).Returns(new List<IGameDetail>());

            _hardwareHelper.Setup(h => h.CheckForVirtualButtonDeckHardware()).Returns(true);
            _hardwareHelper.Setup(h => h.CheckForUsbButtonDeckHardware()).Returns(true);
            _helpTextProvider.Setup(m => m.AllHelpTexts).Returns(new Dictionary<string, Func<string>>());
            _gameCategoryService.Setup(m => m.SelectedGameCategorySetting).Returns(new GameCategorySetting());
            _lobbyStateManager.Setup(m => m.AllowSingleGameAutoLaunch).Returns(false);
            _cabinetDetectionService.Setup(m => m.ButtonDeckType).Returns(It.IsAny<string>());
            _propertiesManager
                .Setup(m => m.GetProperty(GamingConstants.GameConfigurableStartMethods, It.IsAny<object>()))
                .Returns(Array.Empty<GameStartConfigurableMethod>());

            CurrencyExtensions.SetCultureInfo(CultureInfo.CurrentCulture, null, null, true, true, "c");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenRuntimeServiceIsNullExpectException()
        {
            var handler = new ConfigureClientCommandHandler(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenGameHistoryIsNullExpectException()
        {
            var handler = new ConfigureClientCommandHandler(
                _runtime.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenGameRecoveryIsNullExpectException()
        {
            var handler = new ConfigureClientCommandHandler(
                _runtime.Object,
                _gameHistory.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenGameReplayIsNullExpectException()
        {
            var handler = new ConfigureClientCommandHandler(
                _runtime.Object,
                _gameHistory.Object,
                _gameRecovery.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenLobbyStateManagerIsNullExpectException()
        {
            var handler = new ConfigureClientCommandHandler(
                _runtime.Object,
                _gameHistory.Object,
                _gameRecovery.Object,
                _gameDiagnostic.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPropertiesManagerIsNullExpectException()
        {
            var handler = new ConfigureClientCommandHandler(
                _runtime.Object,
                _gameHistory.Object,
                _gameRecovery.Object,
                _gameDiagnostic.Object,
                _lobbyStateManager.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPlayerBankIsNullExpectException()
        {
            var handler = new ConfigureClientCommandHandler(
                _runtime.Object,
                _gameHistory.Object,
                _gameRecovery.Object,
                _gameDiagnostic.Object,
                _lobbyStateManager.Object,
                _propertiesManager.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenAudioIsNullExpectException()
        {
            var handler = new ConfigureClientCommandHandler(
                _runtime.Object,
                _gameHistory.Object,
                _gameRecovery.Object,
                _gameDiagnostic.Object,
                _lobbyStateManager.Object,
                _propertiesManager.Object,
                _playerBank.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenGameProviderIsNullExpectException()
        {
            var handler = new ConfigureClientCommandHandler(
                _runtime.Object,
                _gameHistory.Object,
                _gameRecovery.Object,
                _gameDiagnostic.Object,
                _lobbyStateManager.Object,
                _propertiesManager.Object,
                _playerBank.Object,
                _audio.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenGameCategoryServiceIsNullExpectException()
        {
            var handler = new ConfigureClientCommandHandler(
                _runtime.Object,
                _gameHistory.Object,
                _gameRecovery.Object,
                _gameDiagnostic.Object,
                _lobbyStateManager.Object,
                _propertiesManager.Object,
                _playerBank.Object,
                _audio.Object,
                _gameProvider.Object,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCabinetServiceIsNullExpectException()
        {
            var handler = new ConfigureClientCommandHandler(
                _runtime.Object,
                _gameHistory.Object,
                _gameRecovery.Object,
                _gameDiagnostic.Object,
                _lobbyStateManager.Object,
                _propertiesManager.Object,
                _playerBank.Object,
                _audio.Object,
                _gameProvider.Object,
                _gameCategoryService.Object,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenGameHelpTextServiceIsNullExpectException()
        {
            var handler = new ConfigureClientCommandHandler(
                _runtime.Object,
                _gameHistory.Object,
                _gameRecovery.Object,
                _gameDiagnostic.Object,
                _lobbyStateManager.Object,
                _propertiesManager.Object,
                _playerBank.Object,
                _audio.Object,
                _gameProvider.Object,
                _gameCategoryService.Object,
                _cabinetDetectionService.Object,
                null,
                null,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenHardwareHelperIsNullExpectException()
        {
            var handler = new ConfigureClientCommandHandler(
                _runtime.Object,
                _gameHistory.Object,
                _gameRecovery.Object,
                _gameDiagnostic.Object,
                _lobbyStateManager.Object,
                _propertiesManager.Object,
                _playerBank.Object,
                _audio.Object,
                _gameProvider.Object,
                _gameCategoryService.Object,
                _cabinetDetectionService.Object,
                _helpTextProvider.Object,
                null,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenAttendantServiceIsNullExpectException()
        {
            var handler = new ConfigureClientCommandHandler(
                _runtime.Object,
                _gameHistory.Object,
                _gameRecovery.Object,
                _gameDiagnostic.Object,
                _lobbyStateManager.Object,
                _propertiesManager.Object,
                _playerBank.Object,
                _audio.Object,
                _gameProvider.Object,
                _gameCategoryService.Object,
                _cabinetDetectionService.Object,
                _helpTextProvider.Object,
                _hardwareHelper.Object,
                null,
                null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var handler = new ConfigureClientCommandHandler(
                _runtime.Object,
                _gameHistory.Object,
                _gameRecovery.Object,
                _gameDiagnostic.Object,
                _lobbyStateManager.Object,
                _propertiesManager.Object,
                _playerBank.Object,
                _audio.Object,
                _gameProvider.Object,
                _gameCategoryService.Object,
                _cabinetDetectionService.Object,
                _helpTextProvider.Object,
                _hardwareHelper.Object,
                _attendantService.Object,
                _gameConfiguration.Object);

            Assert.IsNotNull(handler);
        }

        [DataRow(GameStartMethodOption.BetOrMaxBet, "Bet, MaxBet")]
        [DataRow(GameStartMethodOption.LineReelOrMaxBet, "Line, MaxBet")]
        [DataRow(GameStartMethodOption.Bet, "Bet")]
        [DataRow(GameStartMethodOption.LineOrReel, "Line")]
        [DataRow(GameStartMethodOption.None, "")]
        [DataTestMethod]
        public void CheckGameStartMethod(GameStartMethodOption param, string expectedResult)
        {
            IDictionary<string, string> localDict = null;
            _runtime.Setup(
                    m => m.UpdateParameters(
                        It.IsAny<IDictionary<string, string>>(),
                        ConfigurationTarget.GameConfiguration))
                .Callback<IDictionary<string, string>, ConfigurationTarget>(
                    (dictionary, _) => localDict = dictionary);

            var handler = new ConfigureClientCommandHandler(
                _runtime.Object,
                _gameHistory.Object,
                _gameRecovery.Object,
                _gameDiagnostic.Object,
                _lobbyStateManager.Object,
                _propertiesManager.Object,
                _playerBank.Object,
                _audio.Object,
                _gameProvider.Object,
                _gameCategoryService.Object,
                _cabinetDetectionService.Object,
                _helpTextProvider.Object,
                _hardwareHelper.Object,
                _attendantService.Object,
                _gameConfiguration.Object);

            Assert.IsNotNull(handler);

            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.GameStartMethod, GameStartMethodOption.Bet))
                .Returns(param);
            handler.Handle(new ConfigureClient());
            Assert.AreEqual(localDict["/Runtime/StartGame&buttons"], expectedResult);
        }

        [DataRow(1000, true, 1000)]
        [DataRow(0, false, -1)]
        [DataRow(100, false, -1)]
        [DataTestMethod]
        public void CheckGameRoundDuration(int minimumReelDuration, bool shouldBeStored, int expectedResult)
        {
            IDictionary<string, string> localDict = null;

            _runtime
                .Setup(
                    m => m.UpdateParameters(
                        It.IsAny<IDictionary<string, string>>(),
                        ConfigurationTarget.GameConfiguration))
                .Callback<IDictionary<string, string>, ConfigurationTarget>(
                    (dictionary, _) => localDict = dictionary);

            var handler = new ConfigureClientCommandHandler(
                _runtime.Object,
                _gameHistory.Object,
                _gameRecovery.Object,
                _gameDiagnostic.Object,
                _lobbyStateManager.Object,
                _propertiesManager.Object,
                _playerBank.Object,
                _audio.Object,
                _gameProvider.Object,
                _gameCategoryService.Object,
                _cabinetDetectionService.Object,
                _helpTextProvider.Object,
                _hardwareHelper.Object,
                _attendantService.Object,
                _gameConfiguration.Object);

            Assert.IsNotNull(handler);

            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.GameRoundDurationMs, GamingConstants.DefaultMinimumGameRoundDurationMs)).Returns(minimumReelDuration);
            handler.Handle(new ConfigureClient());

            Assert.AreEqual(shouldBeStored, localDict.ContainsKey("/Runtime/GameRoundDurationMs"));

            if (shouldBeStored)
            {
                Assert.AreEqual(expectedResult.ToString(), localDict["/Runtime/GameRoundDurationMs"]);
            }
        }

        [DataRow(true, "true")]
        [DataRow(false, "false")]
        [DataTestMethod]
        public void GivenPlayerInformationDisplayEnabledWhenHandleThenConvertedCorrectly(bool value, string expected)
        {
            IDictionary<string, string> localDict = null;

            _runtime
                .Setup(
                    m => m.UpdateParameters(
                        It.IsAny<IDictionary<string, string>>(),
                        ConfigurationTarget.GameConfiguration))
                .Callback<IDictionary<string, string>, ConfigurationTarget>(
                    (dictionary, _) => localDict = dictionary);

            var handler = new ConfigureClientCommandHandler(
                _runtime.Object,
                _gameHistory.Object,
                _gameRecovery.Object,
                _gameDiagnostic.Object,
                _lobbyStateManager.Object,
                _propertiesManager.Object,
                _playerBank.Object,
                _audio.Object,
                _gameProvider.Object,
                _gameCategoryService.Object,
                _cabinetDetectionService.Object,
                _helpTextProvider.Object,
                _hardwareHelper.Object,
                _attendantService.Object,
                _gameConfiguration.Object);


            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.PlayerInformationDisplay.Enabled, false)).Returns(value)
                .Verifiable();
            handler.Handle(new ConfigureClient());

            Assert.AreEqual(expected, localDict["/Runtime/IKey"]);

            _propertiesManager.Verify();
        }

        [DataRow(true, "allowed")]
        [DataRow(false, "disallowed")]
        [DataTestMethod]
        public void GivenPlayerInformationDisplayRestrictedModeUseWhenHandleThenConvertedCorrectly(bool value, string expected)
        {
            IDictionary<string, string> localDict = null;

            _runtime
                .Setup(
                    m => m.UpdateParameters(
                        It.IsAny<IDictionary<string, string>>(),
                        ConfigurationTarget.GameConfiguration))
                .Callback<IDictionary<string, string>, ConfigurationTarget>(
                    (dictionary, _) => localDict = dictionary);

            var handler = new ConfigureClientCommandHandler(
                _runtime.Object,
                _gameHistory.Object,
                _gameRecovery.Object,
                _gameDiagnostic.Object,
                _lobbyStateManager.Object,
                _propertiesManager.Object,
                _playerBank.Object,
                _audio.Object,
                _gameProvider.Object,
                _gameCategoryService.Object,
                _cabinetDetectionService.Object,
                _helpTextProvider.Object,
                _hardwareHelper.Object,
                _attendantService.Object,
                _gameConfiguration.Object);


            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.PlayerInformationDisplay.RestrictedModeUse, false)).Returns(value)
                .Verifiable();
            handler.Handle(new ConfigureClient());

            Assert.AreEqual(expected, localDict["/Runtime/IKey&restrictedModeUse"]);

            _propertiesManager.Verify();
        }

    }
}