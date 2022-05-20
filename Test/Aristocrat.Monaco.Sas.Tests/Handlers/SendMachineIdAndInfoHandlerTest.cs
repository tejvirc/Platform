namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.PackageManifest.Extension.v100;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using PackageManifest.Models;
    using Progressive;
    using Sas.Handlers;
    using WagerCategory = Gaming.Contracts.WagerCategory;

    [TestClass]
    public class SendMachineIdAndInfoHandlerTest
    {
        // managers for multiple test cases
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IGameProvider> _gameProvider;
        private Mock<IProtocolLinkedProgressiveAdapter> _protocolLinkedProgressiveAdapter;
        private SendMachineIdAndInfoHandler _target;
        private const string TestGameId = "AT";
        private const string TestAdditionalId = "000";
        private const int TestGameOptions = 0;
        private const int TestDenom = 1;
        private const int TestProgGroup = 1;
        private const int BasePercentLength = 4;
        private const long TestDenomValue = 1000;

        private static readonly IReadOnlyCollection<IWagerCategory> WagerCategories = new List<IWagerCategory>
        {
            new WagerCategory("WC1", 98.76M, 1, 5, 100_000),
            new WagerCategory("WC2", 97.656M, 5, 10, 200_000),
            new WagerCategory("WC3", 96.54M, 25, 100, 300_000),
            new WagerCategory("WC4", 95.43M, 50, 150, 400_000),
            new WagerCategory("WC5", 94.32M, 75, 300, 500_000),
            new WagerCategory("WC6", 93.21M, 100, 500, 600_000),
            new WagerCategory("WC7", 92.10M, 250, 1_000, 700_000),
            new WagerCategory("WC8", 91.098M, 500, 2_500, 800_000),
            new WagerCategory("WC9", 90.98M, 750, 3_750, 900_000),
            new WagerCategory("WC10", 89.87M, 1000, 5_000, 1_000_000),
            new WagerCategory("WC11", 88.87M, 1000, 10_000, 1_000_000)
        };

        private class GameData
        {
            public GameData(int id, int maxWager, long basePercent, string paytableId, string themeId, string variationId)
            {
                Id = id;
                MaxWager = maxWager;
                BasePercent = basePercent;
                PaytableId = paytableId;
                ThemeId = themeId;
                VariationId = variationId;
            }

            public int Id { get; }
            public int MaxWager { get; }
            public long BasePercent { get; }
            public string VariationId { get; }
            public string PaytableId { get; }
            public string ThemeId { get; }
        }

        private static readonly IReadOnlyCollection<GameData> GameGoodData = new List<GameData>
        {
            new GameData(1, 5, 9876, "01", "02", "03"),
            new GameData(2, 25, 9765,  "03", "04", "05"), // no wager category
            new GameData(3, 150, 9543, "05","06", "07"),
            new GameData(4, 100, 9654, "07", "08", "09")
        };

        private static readonly IReadOnlyCollection<GameData> GameOversizedData = new List<GameData>
        {
            new GameData(1, 10_000, 95432, "15", "16", "17"),// oversized rtp & max wager
            new GameData(2, 2_500, 91098, "13","14", "15"),  // oversized rtp & max wager
            new GameData(3, 10, 97656, "09", "10", "11"),    // oversized rtp
            new GameData(4, 300, 9432,  "11", "12", "13")    // oversized max wager
        };

        [TestInitialize]
        public void MyTestInitialize()
        {
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);
            _protocolLinkedProgressiveAdapter = new Mock<IProtocolLinkedProgressiveAdapter>(MockBehavior.Default);
            _target = CreateHandler();

            // configure property
            _propertiesManager.Setup(c => c.GetProperty(It.IsAny<string>(), It.IsAny<object>())).Returns((uint)TestProgGroup);
        }

        private SendMachineIdAndInfoHandler CreateHandler(
            bool nullProperties = false,
            bool nullGameProvider = false,
            bool nullLinkedProgressive = false)
        {
            return new SendMachineIdAndInfoHandler(
                nullProperties ? null : _propertiesManager.Object,
                nullGameProvider ? null : _gameProvider.Object,
                nullLinkedProgressive ? null : _protocolLinkedProgressiveAdapter.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(2, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendMachineIdAndInformation));
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendGameNConfiguration));
        }

        [DataTestMethod]
        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorTest(
            bool nullProperties,
            bool nullGameProvider,
            bool nullLinkedProgressive)
        {
            _target = CreateHandler(
                nullProperties,
                nullGameProvider,
                nullLinkedProgressive);
        }

        [DataTestMethod]
        // Single Game Good Data:
        [DataRow(0, 5, "9876", "00E6C3", TestAdditionalId, true, true, 1L, (byte)1)] // machine info
        [DataRow(1, 5, "9876", "01", "_03", true, true, 1L, (byte)1)] // game 1 found
        [DataRow(3, 0, "", "", "", true, true, 1L, (byte)1)] // game 3 not found
        [DataRow(1, 5, "9876", "01", "_03", true, true, 1L, (byte)1)]
        [DataRow(1, 5, "9876", "01", "_03", true, true, 5L, (byte)2)]
        [DataRow(1, 5, "9876", "01", "_03", true, true, 10L, (byte)3)]
        [DataRow(1, 5, "9876", "01", "_03", true, true, 25L, (byte)4)]
        // Single Game Oversized Data:
        [DataRow(0, 0xFF, "8887", "00FCCD", TestAdditionalId, true, false, 1L, (byte)1)] // machine info
        [DataRow(1, 0xFF, "8887", "15", "_17", true, false, 1L, (byte)1)] // game 1 found
        [DataRow(4, 0, "", "", "", true, false, 1L, (byte)1)] // game 4 not found
        // Multi-game Good Data:
        [DataRow(0, 150, "7268", "006627", TestAdditionalId, false, true, 1L, (byte)1)] // machine info
        [DataRow(1, 5, "9876", "01", "_03", false, true, 1L, (byte)1)] // game 1 found
        [DataRow(2, 25, "0000", "03", "_05", false, true, 1L, (byte)1)] // game 2 found no wager category
        [DataRow(5, 0, "", "", "", false, true, 1L, (byte)1)] // game 5 not found
        // Multi-game Oversized Data:
        [DataRow(0, 0xFF, "9299", "00FC28", TestAdditionalId, false, false, 1L, (byte)1)] // machine info
        [DataRow(1, 0xFF, "8887", "15", "_17", false, false, 1L, (byte)1)] // game 1 found, oversized rtp & max wager
        [DataRow(2, 0xFF, "9110", "13", "_15", false, false, 1L, (byte)1)] // game 2 found, oversized rtp & max wager
        [DataRow(3, 10, "9766", "09", "_11", false, false, 1L, (byte)1)] // game 3 found, oversized rtp
        [DataRow(4, 0xFF, "9432", "11", "_13", false, false, 1L, (byte)1)] // game 4 found, oversized max wager
        [DataRow(5, 0, "", "", "", false, false, 1L, (byte)1)] // game 5 not found
        public void HandleTest(
            int gameId,
            int maxWagerValueExpected,
            string basePercentExpected,
            string paytableIdExpected,
            string additionalId,
            bool singleGame,
            bool goodData,
            long accountingDenom,
            byte denomCode)
        {
            const string linkedLevelName = "TestLevel";

            var gameExists = false;
            // configure a game(s)
            var games = new List<Mock<IGameDetail>>();
            foreach (var gameInfo in goodData ? GameGoodData : GameOversizedData)
            {
                gameExists |= (gameInfo.Id == gameId);
                games.Add(
                    SetupMockGame(
                        gameInfo.Id,
                        gameInfo.MaxWager,
                        gameInfo.BasePercent,
                        gameInfo.PaytableId,
                        gameInfo.ThemeId,
                        gameInfo.VariationId));

                if (singleGame)
                {
                    break;
                }
            }

            SetupMockGameProvider(games, gameId == 0);
            var mockLevel = new Mock<IViewableProgressiveLevel>();
            mockLevel.Setup(x => x.GameId).Returns(gameId);
            mockLevel.Setup(x => x.Denomination).Returns(new List<long> { TestDenomValue });
            mockLevel.Setup(x => x.AssignedProgressiveId).Returns(
                new AssignableProgressiveId(AssignableProgressiveType.Linked, linkedLevelName));
            _protocolLinkedProgressiveAdapter.Setup(x => x.ViewConfiguredProgressiveLevels())
                .Returns(new List<IViewableProgressiveLevel> { mockLevel.Object });
            var mockLinkedLevel = new Mock<IViewableLinkedProgressiveLevel>();
            var expectedLinkLevel = mockLinkedLevel.Object;
            mockLinkedLevel.Setup(x => x.ProtocolName).Returns(ProgressiveConstants.ProtocolName);
            _protocolLinkedProgressiveAdapter.Setup(x => x.ViewLinkedProgressiveLevel(linkedLevelName, out expectedLinkLevel)).Returns(true);
            _propertiesManager.Setup(c => c.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                 .Returns(new SasFeatures { ProgressiveGroupId = TestProgGroup });

            var result = _target.Handle(
                new LongPollGameNConfigurationData { GameNumber = (ulong)gameId, AccountingDenom = accountingDenom });

            if (gameId == 0 || gameExists)
            {
                var expected = new LongPollMachineIdAndInfoResponse(
                    TestGameId,
                    additionalId,
                    denomCode,
                    (byte)maxWagerValueExpected,
                    TestProgGroup,
                    TestGameOptions,
                    paytableIdExpected,
                    basePercentExpected);
                Assert.AreEqual(expected.GameId, result.GameId);
                Assert.AreEqual(expected.AdditionalId, result.AdditionalId);
                Assert.AreEqual(expected.Denomination, result.Denomination);
                Assert.AreEqual(expected.MaxBet, result.MaxBet);
                Assert.AreEqual(expected.ProgressiveGroup, result.ProgressiveGroup);
                Assert.AreEqual(expected.GameOptions, result.GameOptions);
                Assert.AreEqual(expected.PaytableId, result.PaytableId);
                Assert.AreEqual(expected.TheoreticalRtpPercent, result.TheoreticalRtpPercent);
                Assert.AreEqual(expected.PaytableId.Length, result.PaytableId.Length);
                Assert.AreEqual(BasePercentLength, result.TheoreticalRtpPercent.Length);
            }
            else
            {
                Assert.AreSame(result, null);
            }
        }

        [TestMethod]
        public void NoGamesEnabledTest()
        {
            var games = GameGoodData.Select(
                gameInfo => SetupMockGame(
                    gameInfo.Id,
                    gameInfo.MaxWager,
                    gameInfo.BasePercent,
                    gameInfo.PaytableId,
                    gameInfo.ThemeId,
                    gameInfo.VariationId)).ToList();

            _gameProvider.Setup(x => x.GetEnabledGames()).Returns(new List<IGameDetail>());
            SetupMockGameProvider(games, false);
            _propertiesManager.Setup(c => c.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                 .Returns(new SasFeatures { ProgressiveGroupId = TestProgGroup });

            var data = new LongPollGameNConfigurationData { GameNumber = 0, AccountingDenom = 1 };
            var response = _target.Handle(data);
            Assert.IsNotNull(response);
            Assert.AreEqual((byte)0, response.MaxBet);
            Assert.AreEqual("0000", response.TheoreticalRtpPercent);
            Assert.AreEqual("006627", response.PaytableId);
            Assert.AreEqual((byte)0, response.ProgressiveGroup);
            Assert.AreEqual(TestGameId, response.GameId);
            Assert.AreEqual(TestAdditionalId, response.AdditionalId);
            Assert.AreEqual(TestDenom, response.Denomination);
            Assert.AreEqual((uint)0, response.GameOptions);
        }

        private static Mock<IGameDetail> SetupMockGame(
            int id,
            int maxWager,
            long basePercent,
            string paytableId,
            string themeId,
            string variationId,
            bool denomEnabled = true)
        {
            var newGame = new Mock<IGameDetail>(MockBehavior.Default);
            newGame.Setup(c => c.MaximumWagerCredits).Returns(maxWager);
            newGame.Setup(c => c.BetOptionList).Returns(new BetOptionList(new List<c_betOption>()));
            newGame.Setup(c => c.LineOptionList).Returns(new LineOptionList(new List<c_lineOption>()));
            newGame.Setup(c => c.MaximumPaybackPercent).Returns(basePercent / 100M);
            newGame.Setup(c => c.PaytableId).Returns(paytableId);
            newGame.Setup(x => x.PaytableName).Returns(paytableId);
            newGame.Setup(c => c.ThemeId).Returns(themeId);
            newGame.Setup(c => c.WagerCategories).Returns(WagerCategories);
            newGame.Setup(x => x.VariationId).Returns(variationId);
            newGame.Setup(x => x.Id).Returns(id);
            newGame.Setup(x => x.ActiveDenominations)
                .Returns(denomEnabled ? new List<long> { TestDenomValue } : new List<long>());
            newGame.Setup(x => x.Denominations)
                .Returns(new List<IDenomination> { new MockDenomination(TestDenomValue, id, denomEnabled) });
            return newGame;
        }

        private void SetupMockGameProvider(IEnumerable<Mock<IGameDetail>> multiGameList, bool enabledGames)
        {
            var gameList = multiGameList.Select(nextMock => nextMock.Object).ToList();
            if (enabledGames)
            {
                _gameProvider.Setup(c => c.GetEnabledGames()).Returns(gameList);
            }

            _gameProvider.Setup(c => c.GetAllGames()).Returns(gameList);
        }
    }
}
