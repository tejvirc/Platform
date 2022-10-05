namespace Aristocrat.Monaco.Bingo.UI.Tests.Services
{
    using System;
    using Aristocrat.Monaco.Bingo.UI.Services;
    using Aristocrat.Monaco.OverlayServer.Data.Bingo;
    using Aristocrat.Monaco.Protocol.Common.Storage.Entity;
    using Common.Storage.Model;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class LegacyAttractProviderTests
    {
        private const string GamesConfigurationString = @"[
  {
    GameTitleId: ""61"",
    ThemeSkinId: ""294"",
    PaytableId: ""12414"",
    Denomination: ""1000"",
    QuickStopMode: ""false"",
    EvaluationTypePaytable: ""APP"",
    PlatformGameId: ""1"",
    Bets: [
      18, 36, 54, 72, 90, 108, 126, 144, 162, 180, 198, 216, 234, 252, 270, 288,
      306, 324, 342, 360,
    ],
    HelpUrl: ""https://localhost:7520/testingHelp/61/12414/index.html""
  },
  {
    GameTitleId: ""272"",
    ThemeSkinId: ""294"",
    PaytableId: ""46082"",
    Denomination: ""1000"",
    QuickStopMode: ""false"",
    EvaluationTypePaytable: ""APP"",
    PlatformGameId: ""3"",
    Bets: [
      1, 2, 3, 4, 5, 6, 7, 8, 9, 18, 27, 36, 45, 54, 63, 72, 81, 90, 99, 108,
      117, 126, 135, 144, 153, 162, 171, 180,
    ],
    HelpUrl: ""https://localhost:7520/gamehelp/61/46082/index.html""
  },
]";

        private const int GameId = 1;
        private const long Denom = 1000;
        private const long TitleId = 61;

        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _propertiesManager = new(MockBehavior.Default);
        private readonly Mock<IGameDetail> _gameDetail = new(MockBehavior.Default);
        private readonly Mock<IDenomination> _denomination = new(MockBehavior.Default);

        private LegacyAttractProvider _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            var settingsModel = new BingoServerSettingsModel
            {
                GamesConfigurationText = GamesConfigurationString
            };

            _denomination.Setup(x => x.Value).Returns(Denom);
            _gameDetail.Setup(x => x.Id).Returns(GameId);
            _gameDetail.Setup(x => x.CdsThemeId).Returns(TitleId.ToString());
            _gameDetail.Setup(x => x.Denominations).Returns(new[] { _denomination.Object });
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.SelectedGameId, It.IsAny<int>()))
                .Returns(GameId);
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.SelectedDenom, It.IsAny<long>()))
                .Returns(Denom);
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.IsGameRunning, It.IsAny<bool>())).Returns(true);
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.Games, It.IsAny<object>()))
                .Returns(new[] { _gameDetail.Object });
            _unitOfWorkFactory
                .Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, BingoServerSettingsModel>>()))
                .Returns(settingsModel);
            _target = new LegacyAttractProvider(_propertiesManager.Object, _unitOfWorkFactory.Object);
        }

        [DataRow(true, false)]
        [DataRow(false, true)]
        [DataTestMethod]
        public void NullConstructorArgumentsTest(bool nullProperties, bool nullUnitOfWork)
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => _ = new LegacyAttractProvider(
                    nullProperties ? null : _propertiesManager.Object,
                    nullUnitOfWork ? null : _unitOfWorkFactory.Object));
        }

        [TestMethod]
        public void GetAttractUrlTest()
        {
            var bingoAttractSettings = new BingoDisplayConfigurationBingoAttractSettings
            {
                CyclePatterns = true,
                BetAmountFormattingText = "Bet {0}",
                BallsCalledWithinFormattingText = "Balls Called {0}",
                DisplayAmountsAsCredits = false,
                PatternCycleTimeMilliseconds = 1000,
                PayAmountFormattingText = "Paid {0}"
            };

            var url = _target.GetLegacyAttractUri(
                bingoAttractSettings);
            var parameters = QueryHelpers.ParseQuery(url.Query);
            Assert.IsTrue(parameters.TryGetValue("baseUrl", out var baseUrl));
            Assert.IsTrue(parameters.TryGetValue("patternsUrlFormat", out var patternsUrlFormat));
            Assert.IsTrue(parameters.TryGetValue("gameName", out var gameName));
            Assert.IsTrue(parameters.TryGetValue("paytableName", out var paytableName));
            Assert.IsTrue(parameters.TryGetValue("ballsInFormat", out var ballsInFormat));
            Assert.IsTrue(parameters.TryGetValue("betFormat", out var betFormat));
            Assert.IsTrue(parameters.TryGetValue("payoutFormat", out var payoutFormat));
            Assert.IsTrue(parameters.TryGetValue("useCredits", out var useCredits));
            Assert.AreEqual("https://localhost:7520", baseUrl.ToString().TrimEnd('/'));
            Assert.AreEqual("/testingHelp/61/12414/scripts/pattern.js", patternsUrlFormat.ToString());
            Assert.AreEqual(TitleId, int.Parse(gameName));
            Assert.AreEqual("12414", paytableName.ToString());
            Assert.AreEqual(bingoAttractSettings.BallsCalledWithinFormattingText, ballsInFormat.ToString());
            Assert.AreEqual(bingoAttractSettings.BetAmountFormattingText, betFormat.ToString());
            Assert.AreEqual(bingoAttractSettings.PayAmountFormattingText, payoutFormat.ToString());
            Assert.AreEqual(bingoAttractSettings.DisplayAmountsAsCredits, bool.Parse(useCredits));
        }
    }
}