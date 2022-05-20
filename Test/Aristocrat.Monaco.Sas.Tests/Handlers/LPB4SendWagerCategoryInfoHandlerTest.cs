namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Aristocrat.Sas.Client.Metering;
    using Gaming.Contracts;
    using Gaming.Contracts.Meters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    /// <summary>
    ///     Tests for the LPB4SendWagerCategoryInfoHanlderTest class
    /// </summary>
    [TestClass]
    public class LPB4SendWagerCategoryInfoHandlerTest
    {
        private const int ExpectedBadGameId = 666;
        private const long ExpectedCoinIn = 123456;
        private const long ExpectedTotalCoinIn = 654321;
        private const int ExpectedGameId = 12345;
        private const int ExpectedZeroGameId = 0;
        private const int ExpectedWagerCat = 6;
        private const int ExpectedPlaybackPercentage = 9321;
        private const int ExpectedMaxPlaybackPercentage = 8800;
        private const string ExpectedMaxPlaybackPercentCat = "10";
        private static readonly IReadOnlyCollection<IWagerCategory> WagerCategories = new List<IWagerCategory>
        {
            new WagerCategory("1", 98.76M, 1, 5, 100_000),
            new WagerCategory("2", 97.56M, 5, 10, 200_000),
            new WagerCategory("3", 96.54M, 25, 100, 300_000),
            new WagerCategory("4", 95.43M, 50, 150, 400_000),
            new WagerCategory("5", 94.32M, 75, 300, 500_000),
            new WagerCategory("6", ExpectedPlaybackPercentage / 100M, 100, 500, 600_000),
            new WagerCategory("7", 92.10M, 250, 1_000, 700_000),
            new WagerCategory("8", 91.98M, 500, 2_500, 800_000),
            new WagerCategory("9", 90.98M, 750, 3_750, 900_000),
            new WagerCategory(ExpectedMaxPlaybackPercentCat, ExpectedMaxPlaybackPercentage / 100M, 1000, 5_000, 1_000_000)
        };

        private LPB4SendWagerCategoryInfoHandler _target;
        private Mock<IGameProvider> _gameProvider;
        private Mock<IGameMeterManager> _gameMeterManager;

        [TestInitialize]
        public void MyTestInitialize()
        {
            const long testUpperBounds = 1000000000;
            const long denomValue = 1000;
            var coinInMeter = new Mock<IMeter>();
            var totalCoinInMeter = new Mock<IMeter>();

            var sasMeterName = SasMeterCollection.SasMeterForCode(SasMeterId.TotalCoinIn).MappedMeterName;

            _gameProvider = new Mock<IGameProvider>(MockBehavior.Strict);
            var gameDetail = SetupMockGame(ExpectedGameId, denomValue);
            _gameProvider.Setup(m => m.GetGame(ExpectedGameId)).Returns(gameDetail.Object);
            _gameProvider.Setup(m => m.GetGame(ExpectedBadGameId)).Returns((IGameDetail)null);

            _gameProvider.Setup(x => x.GetAllGames()).Returns(new List<IGameDetail> { gameDetail.Object });

            coinInMeter.Setup(m => m.Lifetime).Returns(ExpectedCoinIn);
            coinInMeter.Setup(m => m.Classification).Returns(new TestMeterClassification("TestClassification", testUpperBounds));

            totalCoinInMeter.Setup(m => m.Lifetime).Returns(ExpectedTotalCoinIn);
            totalCoinInMeter.Setup(m => m.Classification).Returns(new TestMeterClassification("TestClassification", testUpperBounds));

            _gameMeterManager = new Mock<IGameMeterManager>(MockBehavior.Default);
            _gameMeterManager.Setup(m => m.GetMeter(ExpectedGameId, denomValue, ExpectedWagerCat.ToString(), GamingMeters.WagerCategoryWageredAmount)).Returns(coinInMeter.Object);
            _gameMeterManager.Setup(m => m.IsMeterProvided(ExpectedGameId, denomValue, ExpectedWagerCat.ToString(), GamingMeters.WagerCategoryWageredAmount)).Returns(true);
            _gameMeterManager.Setup(m => m.GetMeter(ExpectedGameId, denomValue, ExpectedMaxPlaybackPercentCat, GamingMeters.WagerCategoryWageredAmount)).Returns(coinInMeter.Object);
            _gameMeterManager.Setup(m => m.IsMeterProvided(ExpectedGameId, denomValue, ExpectedMaxPlaybackPercentCat, GamingMeters.WagerCategoryWageredAmount)).Returns(true);
            _gameMeterManager.Setup(m => m.GetMeter(ExpectedGameId, denomValue, ExpectedMaxPlaybackPercentCat, GamingMeters.WagerCategoryWageredAmount)).Returns(coinInMeter.Object);
            _gameMeterManager.Setup(m => m.IsMeterProvided(ExpectedGameId, denomValue, ExpectedMaxPlaybackPercentCat, GamingMeters.WagerCategoryWageredAmount)).Returns(true);
            _gameMeterManager.Setup(m => m.GetMeter(ExpectedGameId, denomValue, GamingMeters.WageredAmount)).Returns(coinInMeter.Object);
            _gameMeterManager.Setup(m => m.IsMeterProvided(ExpectedGameId, denomValue, GamingMeters.WageredAmount)).Returns(true);
            _gameMeterManager.Setup(m => m.GetMeter(ExpectedGameId, GamingMeters.WageredAmount)).Returns(coinInMeter.Object);
            _gameMeterManager.Setup(m => m.IsMeterProvided(ExpectedGameId, GamingMeters.WageredAmount)).Returns(true);

            _gameMeterManager.Setup(m => m.GetMeter(sasMeterName)).Returns(totalCoinInMeter.Object);
            _gameMeterManager.Setup(m => m.IsMeterProvided(sasMeterName)).Returns(true);

            _target = new LPB4SendWagerCategoryInfoHandler(_gameMeterManager.Object, _gameProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullGameMeterManagerTest()
        {
            _target = new LPB4SendWagerCategoryInfoHandler(null, _gameProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullGameProviderTest()
        {
            _target = new LPB4SendWagerCategoryInfoHandler(_gameMeterManager.Object, null);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendWagerCategoryInformation));
        }

        [DataRow(ExpectedGameId, ExpectedWagerCat, true, ExpectedCoinIn, ExpectedPlaybackPercentage)]
        [DataRow(ExpectedBadGameId, ExpectedWagerCat, false, 0L, 0)]
        [DataRow(ExpectedGameId, 666, false, 0L, 0)]
        [DataRow(ExpectedZeroGameId, ExpectedWagerCat, true, 0, 0)]
        [DataRow(ExpectedZeroGameId, 0, true, ExpectedTotalCoinIn, 0)]
        [DataRow(ExpectedGameId, 0, true, ExpectedCoinIn, ExpectedMaxPlaybackPercentage)]
        [DataTestMethod]
        public void HandleTest(int gameId, int wagerId, bool valid, long expectedCoinIn, int paybackPercentage)
        {
            var expected = _target.Handle(new LongPollReadWagerData { GameId = gameId, WagerCategory = wagerId, AccountingDenom = 1 });
            Assert.AreEqual(valid, expected.IsValid);
            Assert.AreEqual(expectedCoinIn.MillicentsToCents(), expected.CoinInMeter);
            Assert.AreEqual(paybackPercentage, expected.PaybackPercentage);
        }

        private Mock<IGameDetail> SetupMockGame(int gameId, long denomValue)
        {
            return SetupMockGame(gameId, WagerCategories, denomValue);
        }

        private Mock<IGameDetail> SetupMockGame(int gameId, IReadOnlyCollection<IWagerCategory> wagerCategories, long denomValue)
        {
            var newGame = new Mock<IGameDetail>(MockBehavior.Strict);
            newGame.Setup(c => c.Id).Returns(gameId);
            newGame.Setup(c => c.WagerCategories).Returns(wagerCategories);
            newGame.Setup(c => c.MaximumWagerCredits).Returns(wagerCategories.Max(w => w.MaxWagerCredits ?? 1));
            newGame.Setup(x => x.Denominations).Returns(new List<IDenomination> { new MockDenomination(denomValue, gameId) });
            return newGame;
        }
    }
}
