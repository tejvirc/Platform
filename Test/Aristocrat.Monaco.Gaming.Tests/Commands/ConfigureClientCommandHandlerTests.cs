namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Accounting.Contracts.HandCount;
    using Application.Contracts.Currency;
    using Contracts.Configuration;
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
        private Mock<IHandCountService> _handCount;
        private Mock<IGameHistory> _gameHistory;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ILobbyStateManager> _lobbyStateManager;
        private Mock<IGameRecovery> _gameRecovery;
        private Mock<IGameDiagnostics> _gameDiagnostic;
        private Mock<IAudio> _audio;
        private Mock<IGameProvider> _gameProvider;
        private Mock<IGameCategoryService> _gameCategoryService;
        private Mock<ICabinetDetectionService> _cabinetDetectionService;
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
            _hardwareHelper = new Mock<IHardwareHelper>();
            _runtime = new Mock<IRuntime>();
            _handCount = new Mock<IHandCountService>();
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
            _gameCategoryService.Setup(m => m.SelectedGameCategorySetting).Returns(new GameCategorySetting());
            _lobbyStateManager.Setup(m => m.AllowSingleGameAutoLaunch).Returns(false);
            _cabinetDetectionService.Setup(m => m.ButtonDeckType).Returns(It.IsAny<string>());
            _propertiesManager
                .Setup(m => m.GetProperty(GamingConstants.GameConfigurableStartMethods, It.IsAny<object>()))
                .Returns(Array.Empty<GameStartConfigurableMethod>());

            // set up currency
            string minorUnitSymbol = "c";
            string cultureName = "en-US";
            CultureInfo culture = new CultureInfo(cultureName);

            RegionInfo region = new RegionInfo(cultureName);
            CurrencyExtensions.Currency = new Currency(region.ISOCurrencySymbol, region, culture, minorUnitSymbol);
            CurrencyExtensions.SetCultureInfo(region.ISOCurrencySymbol, culture, null, null, true, true, minorUnitSymbol);
        }

        [DataTestMethod]
        [DataRow(true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, DisplayName = "null Runtime")]
        [DataRow(false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, true, DisplayName = "null HandCount")]
        [DataRow(false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, true, DisplayName = "null GameHistory")]
        [DataRow(false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, true, DisplayName = "null GameRecovery")]
        [DataRow(false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, true, DisplayName = "null GameDiagnostics")]
        [DataRow(false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, true, DisplayName = "null LobbyStateManager")]
        [DataRow(false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, true, DisplayName = "null PropertiesManager")]
        [DataRow(false, false, false, false, false, false, false, true, false, false, false, false, false, false, false, true, DisplayName = "null PlayerBank")]
        [DataRow(false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, true, DisplayName = "null Audio")]
        [DataRow(false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, true, DisplayName = "null GameProvider")]
        [DataRow(false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, true, DisplayName = "null GameCategoryServicer")]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, true, DisplayName = "null CabinetDetectionService")]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, true, DisplayName = "null HardwareHelper")]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, true, DisplayName = "null AttendantService")]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, true, DisplayName = "null GameConfigurationProvider")]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, DisplayName = "all non-null services, expect success.")]
        public void ConfigureClientCommandHandlerTestExpectException(
            bool nullRuntime,
            bool nullHandCount,
            bool nullGameHistory,
            bool nullGameRecovery,
            bool nullGameDiagnostics,
            bool nullLobbyStateManager,
            bool nullPropertiesManager,
            bool nullPlayerBank,
            bool nullAudio,
            bool nullGameProvider,
            bool nullGameCategoryService,
            bool nullCabinetDetectionService,
            bool nullHardwareHelper,
            bool nullAttendantService,
            bool nullGameConfigurationProvider,
            bool throwException)
        {
            if (throwException)
        {
                Assert.ThrowsException<ArgumentNullException>(() => GetGameRecoveryService(
                    nullRuntime,
                    nullHandCount,
                    nullGameHistory,
                    nullGameRecovery,
                    nullGameDiagnostics,
                    nullLobbyStateManager,
                    nullPropertiesManager,
                    nullPlayerBank,
                    nullAudio,
                    nullGameProvider,
                    nullGameCategoryService,
                    nullCabinetDetectionService,
                    nullHardwareHelper,
                    nullAttendantService,
                    nullGameConfigurationProvider));
        }
            else
            {
                Assert.IsNotNull(GetGameRecoveryService(
                    nullRuntime,
                    nullHandCount,
                    nullGameHistory,
                    nullGameRecovery,
                    nullGameDiagnostics,
                    nullLobbyStateManager,
                    nullPropertiesManager,
                    nullPlayerBank,
                    nullAudio,
                    nullGameProvider,
                    nullGameCategoryService,
                    nullCabinetDetectionService,
                    nullHardwareHelper,
                    nullAttendantService,
                    nullGameConfigurationProvider));
            }
        }

        private ConfigureClientCommandHandler GetGameRecoveryService(
                bool nullRuntime,
                bool nullHandCount,
                bool nullGameHistory,
                bool nullGameRecovery,
                bool nullGameDiagnostics,
                bool nullLobbyStateManager,
                bool nullPropertiesManager,
                bool nullPlayerBank,
                bool nullAudio,
                bool nullGameProvider,
                bool nullGameCategoryService,
                bool nullCabinetDetectionService,
                bool nullHardwareHelper,
                bool nullAttendantService,
                bool nullGameConfigurationProvider)
        {
            return new ConfigureClientCommandHandler(
                nullRuntime ? null : _runtime.Object,
                nullHandCount ? null: _handCount.Object,
                nullGameHistory ? null : _gameHistory.Object,
                nullGameRecovery ? null : _gameRecovery.Object,
                nullGameDiagnostics ? null : _gameDiagnostic.Object,
                nullLobbyStateManager ? null : _lobbyStateManager.Object,
                nullPropertiesManager ? null : _propertiesManager.Object,
                nullPlayerBank ? null : _playerBank.Object,
                nullAudio ? null : _audio.Object,
                nullGameProvider ? null : _gameProvider.Object,
                nullGameCategoryService ? null : _gameCategoryService.Object,
                nullCabinetDetectionService ? null : _cabinetDetectionService.Object,
                nullHardwareHelper ? null : _hardwareHelper.Object,
                nullAttendantService ? null : _attendantService.Object,
                nullGameConfigurationProvider ? null : _gameConfiguration.Object);
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
                _handCount.Object,
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
                _handCount.Object,
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
                _handCount.Object,
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
                _handCount.Object,
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