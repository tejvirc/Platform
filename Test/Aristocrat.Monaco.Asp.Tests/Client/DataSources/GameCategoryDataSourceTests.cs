namespace Aristocrat.Monaco.Asp.Tests.Client.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Aristocrat.Monaco.Asp.Client.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Events.OperatorMenu;
    using Aristocrat.Monaco.Kernel;
    using Asp.Client.DataSources;
    using Gaming.Contracts;
    using Gaming.Contracts.Meters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GameCategoryDataSourceTests
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IAspGameProvider> _aspGameProvider;
        private Mock<IGameMeterManager> _gameMeterManager;
        private GameCategoryDataSource _source;
        private Action<GameConfigurationSaveCompleteEvent> _gameConfigurationSaveCompleteActionCallback;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _aspGameProvider = new Mock<IAspGameProvider>(MockBehavior.Strict);
            _gameMeterManager = new Mock<IGameMeterManager>(MockBehavior.Strict);
            _eventBus = new Mock<IEventBus>();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<GameCategoryDataSource>(), It.IsAny<Action<GameConfigurationSaveCompleteEvent>>()))
                .Callback<object, Action<GameConfigurationSaveCompleteEvent>>((subscriber, callback) => _gameConfigurationSaveCompleteActionCallback = callback);
            SetupMockGameProvider();
            SetupMockGameMeterManager();
            _source = new GameCategoryDataSource(_aspGameProvider.Object, _gameMeterManager.Object, _eventBus.Object);
        }

        [TestMethod]
        public void GameCategoryDataSourceTest()
        {
            Assert.IsNotNull(_source);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullGameProviderTest()
        {
            var _ = new GameCategoryDataSource(null, _gameMeterManager.Object, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullGameMeterManagerTest()
        {
            var _ = new GameCategoryDataSource(_aspGameProvider.Object, null, null);
        }

        [TestMethod]
        public void DataSourceNameTest()
        {
            var expectedName = "GameCategory";
            Assert.AreEqual(expectedName, _source.Name);
        }

        [TestMethod]
        public void MembersTest()
        {
            var expectedMembers = new List<string>();
            for (byte gameNumber = 1; gameNumber <= 64; gameNumber++)
            {
                expectedMembers.Add($"Credit_Denomination_{gameNumber}");
                expectedMembers.Add($"Game_ID_{gameNumber}");
                expectedMembers.Add($"Version_Number_{gameNumber}");
                expectedMembers.Add($"Game_Name_{gameNumber}");
                expectedMembers.Add($"Theoretical_Rtn_{gameNumber}");
                expectedMembers.Add($"Total_Games_Completed_{gameNumber}");
                expectedMembers.Add($"Max_Bet_In_Cents_{gameNumber}");
            }

            var actualMembers = _source.Members;
            Assert.AreEqual(expectedMembers.Count, actualMembers.Count);
            Assert.IsTrue(actualMembers.SequenceEqual(expectedMembers));
        }

        [TestMethod]
        public void GetMemberValueCreditDenomTest()
        {
            var value = _source.GetMemberValue("Credit_Denomination_1");
            Assert.AreEqual(1, (long)value);
            value = _source.GetMemberValue("Credit_Denomination_2");
            Assert.AreEqual(2, (long)value);
            value = _source.GetMemberValue("Credit_Denomination_3");
            Assert.AreEqual(1, (long)value);
            value = _source.GetMemberValue("Credit_Denomination_4");
            Assert.AreEqual(2, (long)value);
            value = _source.GetMemberValue("Credit_Denomination_5");
            Assert.AreEqual(1, (long)value);
            value = _source.GetMemberValue("Credit_Denomination_6");
            Assert.AreEqual(2, (long)value);
            value = _source.GetMemberValue("Credit_Denomination_7");
            Assert.AreEqual(0, (long)value);
        }

        [TestMethod]
        public void GetMemberValueGameIdTest()
        {
            var value = _source.GetMemberValue("Game_ID_1");
            Assert.AreEqual("00010012", value);

            value = _source.GetMemberValue("Game_ID_2");
            Assert.AreEqual("00010011", value);

            value = _source.GetMemberValue("Game_ID_3");
            Assert.AreEqual("00020012", value);

            value = _source.GetMemberValue("Game_ID_4");
            Assert.AreEqual("00020011", value);

            value = _source.GetMemberValue("Game_ID_5");
            Assert.AreEqual("00030012", value);

            value = _source.GetMemberValue("Game_ID_6");
            Assert.AreEqual("00030011", value);

            value = _source.GetMemberValue("Game_ID_7");
            Assert.AreEqual(string.Empty, value);
        }

        [TestMethod]
        public void GetMemberValueGameVersionTest()
        {
            var value = _source.GetMemberValue("Version_Number_1");
            Assert.AreEqual("10266389", value);

            value = _source.GetMemberValue("Version_Number_3");
            Assert.AreEqual("10266394", value);

            value = _source.GetMemberValue("Version_Number_6");
            Assert.AreEqual("10166851", value);

            value = _source.GetMemberValue("Version_Number_7");
            Assert.AreEqual(string.Empty, value);
        }

        [TestMethod]
        public void GetMemberValueGameNameTest()
        {
            var value = _source.GetMemberValue("Game_Name_1");
            Assert.AreEqual("Chili7's", value);

            value = _source.GetMemberValue("Game_Name_2");
            Assert.AreEqual("Chili7's", value);

            value = _source.GetMemberValue("Game_Name_3");
            Assert.AreEqual("Wild Yukon", value);

            value = _source.GetMemberValue("Game_Name_4");
            Assert.AreEqual("Wild Yukon", value);

            value = _source.GetMemberValue("Game_Name_5");
            Assert.AreEqual("Buffalo", value);

            value = _source.GetMemberValue("Game_Name_6");
            Assert.AreEqual("Buffalo", value);

            value = _source.GetMemberValue("Game_Name_7");
            Assert.AreEqual(string.Empty, value);
        }

        [TestMethod]
        public void GetMemberValueGameRtpTest()
        {
            var value = _source.GetMemberValue("Theoretical_Rtn_1");
            Assert.AreEqual("93.07%", value);

            value = _source.GetMemberValue("Theoretical_Rtn_3");
            Assert.AreEqual("92.99%", value);

            value = _source.GetMemberValue("Theoretical_Rtn_6");
            Assert.AreEqual("92.00%", value);

            value = _source.GetMemberValue("Theoretical_Rtn_7");
            Assert.AreEqual("0.00%", value);
        }

        [TestMethod]
        public void GetMemberValueGamesPlayedTest()
        {
            var value = _source.GetMemberValue("Total_Games_Completed_1");
            Assert.AreEqual(10, (long)value);
            value = _source.GetMemberValue("Total_Games_Completed_2");
            Assert.AreEqual(20, (long)value);
            value = _source.GetMemberValue("Total_Games_Completed_3");
            Assert.AreEqual(30, (long)value);
            value = _source.GetMemberValue("Total_Games_Completed_4");
            Assert.AreEqual(40, (long)value);
            value = _source.GetMemberValue("Total_Games_Completed_5");
            Assert.AreEqual(50, (long)value);
            value = _source.GetMemberValue("Total_Games_Completed_6");
            Assert.AreEqual(60, (long)value);
            value = _source.GetMemberValue("Total_Games_Completed_7");
            Assert.AreEqual(0, (long)value);
        }

        [TestMethod]
        public void GetMemberValueMaxBetInCentsTest()
        {
            var value = _source.GetMemberValue("Max_Bet_In_Cents_1");
            Assert.AreEqual(600, (long)value);
            value = _source.GetMemberValue("Max_Bet_In_Cents_2");
            Assert.AreEqual(500, (long)value);
            value = _source.GetMemberValue("Max_Bet_In_Cents_3");
            Assert.AreEqual(600, (long)value);
            value = _source.GetMemberValue("Max_Bet_In_Cents_7");
            Assert.AreEqual(0, (long)value);
        }

        [TestMethod]
        public void SavedGameConfigurationMeterChangedTest()
        {
            var memberValueChangedEventsReceived = 0;
            _source.MemberValueChanged += (sender, eventargs) => ++memberValueChangedEventsReceived;

            var mockTuple = new List<(IGameDetail, IDenomination)>
            {
                (BuildGameDetail(2, "Buffalo", "1.01-66851", "03", 92.00M), BuildDenominations(true, 12, 1000, 600)),
                (BuildGameDetail(3, "Chili7's", "1.02-66389", "99", 93.07M), BuildDenominations(true, 11, 2000, 250))
            };
            _aspGameProvider.Setup(g => g.GetEnabledGames()).Returns(() => mockTuple);

            var mockMeter = new Mock<IMeter>(MockBehavior.Strict);
            mockMeter.Setup(x => x.Lifetime).Returns(10);
            _gameMeterManager.Setup(m => m.GetMeter(2, 1000, GamingMeters.PlayedCount)).Returns(mockMeter.Object);

            _gameConfigurationSaveCompleteActionCallback(new GameConfigurationSaveCompleteEvent());

            var value = _source.GetMemberValue("Game_Name_1");
            Assert.AreEqual("Buffalo", value);

            mockMeter.Raise(x => x.MeterChangedEvent += null, new MeterChangedEventArgs(1));
            Assert.AreEqual(1, memberValueChangedEventsReceived);
        }

        [TestMethod]
        public void Dispose_ShouldUnsubscribeAll()
        {
            //Call dispose twice - should only unsubscribe/deregister from events once
            _source.Dispose();
            _source.Dispose();

            _eventBus.Verify(v => v.UnsubscribeAll(It.IsAny<object>()), Times.Once);

            //Verify .Net event deregistration
            //_meters.ForEach(f => f.VerifyRemove(s => s.MeterChangedEvent -= It.IsAny<EventHandler<MeterChangedEventArgs>>(), Times.Once));
        }

        private IGameDetail BuildGameDetail(int id, string theme, string version, string variationId, decimal minimumRTP)
        {
            var currentGame = new Mock<IGameDetail>();
            currentGame.SetupGet(g => g.Id).Returns(() => id);
            currentGame.SetupGet(g => g.ThemeName).Returns(() => theme);
            currentGame.SetupGet(g => g.Version).Returns(() => version);
            currentGame.SetupGet(g => g.VariationId).Returns(() => variationId);
            currentGame.SetupGet(g => g.MinimumPaybackPercent).Returns(() => minimumRTP);

            return currentGame.Object;
        }

        private IDenomination BuildDenominations(bool active, long denomId, long denomValue, int maxWagerCredit)
        {
            var denom = new Mock<IDenomination>();
            denom.SetupGet(g => g.Active).Returns(() => active);
            denom.SetupGet(g => g.Id).Returns(() => denomId);
            denom.SetupGet(g => g.Value).Returns(() => denomValue);
            denom.SetupGet(g => g.MaximumWagerCredits).Returns(() => maxWagerCredit);
            return denom.Object;
        }

        private void SetupMockGameProvider()
        {
            var mockTuple = new List<(IGameDetail, IDenomination)>
            {
                (BuildGameDetail(1, "Chili7's", "1.02-66389", "99", 93.07M), BuildDenominations(true, 12, 1000, 600)),
                (BuildGameDetail(1, "Chili7's", "1.02-66389", "99", 93.07M), BuildDenominations(true, 11, 2000, 250)),
                (BuildGameDetail(2, "Wild Yukon", "1.02-66394", "01", 92.99M), BuildDenominations(true, 12, 1000, 600)),
                (BuildGameDetail(2, "Wild Yukon", "1.02-66394", "01", 92.99M), BuildDenominations(true, 11, 2000, 250)),
                (BuildGameDetail(3, "Buffalo", "1.01-66851", "03", 92.00M), BuildDenominations(true, 12, 1000, 600)),
                (BuildGameDetail(3, "Buffalo", "1.01-66851", "03", 92.00M), BuildDenominations(true, 11, 2000, 250)),
            };

            _aspGameProvider.Setup(g => g.GetEnabledGames()).Returns(() => mockTuple);
        }

        private void SetupMockGameMeterManager()
        {
            const int gameCount = 6;
            var gameId = 1;
            var denomValue = 1000;
            for (var i = 1; i <= gameCount; i++)
            {
                var playedCount = new Mock<IMeter>(MockBehavior.Strict);
                playedCount.Setup(m => m.Lifetime).Returns(i*10);
                if (i <= 2) gameId = 1;
                else if (i <= 4) gameId = 2;
                else gameId = 3;
                denomValue = (i%2 != 0)? 1000 : 2000;
                _gameMeterManager.Setup(m => m.GetMeter(gameId, denomValue, GamingMeters.PlayedCount)).Returns(playedCount.Object);
            }
        }
    }
}