namespace Aristocrat.Monaco.Asp.Tests.Client.DataSources
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Asp.Client.DataSources;
    using Aristocrat.Monaco.Asp.Client.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Aristocrat.Monaco.Gaming.Contracts.Events.OperatorMenu;
    using Moq;

    /// <summary>
    ///     MultiGame Denom Attribute DataSource Test
    /// </summary>
    [TestClass]
    public class MultiGameDenomAttributeDataSourceTest
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IAspGameProvider> _aspGameProvider;
        private MultiGameDenomAttributeDataSource _source;
        private Action<GameConfigurationSaveCompleteEvent> _gameConfigurationSaveCompleteActionCallback;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _aspGameProvider = new Mock<IAspGameProvider>(MockBehavior.Strict);
            _eventBus = new Mock<IEventBus>();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<MultiGameDenomAttributeDataSource>(), It.IsAny<Action<GameConfigurationSaveCompleteEvent>>()))
                .Callback<object, Action<GameConfigurationSaveCompleteEvent>>((subscriber, callback) => _gameConfigurationSaveCompleteActionCallback = callback);
            _source = new MultiGameDenomAttributeDataSource(_aspGameProvider.Object, _eventBus.Object);
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

        private void SetupMockGameProviderMultiGameMultiDenom()
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

        private void SetupMockGameProviderMultiGameSingleDenom()
        {
            var mockTuple = new List<(IGameDetail, IDenomination)>
            {
                (BuildGameDetail(1, "Chili7's", "1.02-66389", "99", 93.07M), BuildDenominations(true, 12, 5000, 600)),
                (BuildGameDetail(2, "Wild Yukon", "1.02-66394", "01", 92.99M), BuildDenominations(true, 12, 5000, 600)),
                (BuildGameDetail(3, "Buffalo", "1.01-66851", "03", 92.00M), BuildDenominations(true, 12, 5000, 600)),
            };

            _aspGameProvider.Setup(g => g.GetEnabledGames()).Returns(() => mockTuple);
        }

        private void SetupMockGameProviderSingleGameSingleDenom()
        {
            var mockTuple = new List<(IGameDetail, IDenomination)>
            {
                (BuildGameDetail(1, "Chili7's", "1.02-66389", "99", 93.07M), BuildDenominations(true, 12, 20000, 600)),
            };

            _aspGameProvider.Setup(g => g.GetEnabledGames()).Returns(() => mockTuple);
        }

        private void SetupMockGameProviderSingleGameMultiDenom()
        {
            var mockTuple = new List<(IGameDetail, IDenomination)>
            {
                (BuildGameDetail(1, "Chili7's", "1.02-66389", "99", 93.07M), BuildDenominations(true, 12, 1000, 600)),
                (BuildGameDetail(1, "Chili7's", "1.02-66389", "99", 93.07M), BuildDenominations(true, 11, 2000, 250)),
            };

            _aspGameProvider.Setup(g => g.GetEnabledGames()).Returns(() => mockTuple);
        }

        /// <summary>
        ///     Test for Not Null MultiGame Denom Attribute DataSource
        /// </summary>
        [TestMethod]
        public void DataSourceTest() => Assert.IsNotNull(_source);

        [TestMethod]
        public void DataSourceNameTest()
        {
            var expectedName = "Multi_Game_Denom";
            Assert.AreEqual(expectedName, _source.Name);
        }

        /// <summary>
        ///     Instantiate MultiGame Denom Attribute DataSource with Game Provider not null Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullGameProviderTest()
        {
            var _ = new MultiGameDenomAttributeDataSource(_aspGameProvider.Object, null);
        }

        /// <summary>
        ///     Get members value case multigame multidenom Test
        /// </summary>
        [TestMethod]
        public void GetMemberValues_MultiGameMultiDenom()
        {
            SetupMockGameProviderMultiGameMultiDenom();
            var denomInfo = _source.GetMemberValue(_source.Members[0]);
            Assert.AreEqual(AspConstants.DenomAttribute.MultiDenom, denomInfo);
            var denomValue = _source.GetMemberValue(_source.Members[1]);
            Assert.AreEqual(AspConstants.AccountingDenomination, denomValue);
        }

        /// <summary>
        ///     Get members value case multigame single denom Test
        /// </summary>
        [TestMethod]
        public void GetMemberValues_MultiGameSingleDenom()
        {
            SetupMockGameProviderMultiGameSingleDenom();
            var denomInfo = _source.GetMemberValue(_source.Members[0]);
            Assert.AreEqual(AspConstants.DenomAttribute.SingleDenom, denomInfo);
            var denomValue = _source.GetMemberValue(_source.Members[1]);
            Assert.AreEqual((long)5, denomValue);
        }

        /// <summary>
        ///     Get members value case single game single denom Test
        /// </summary>
        [TestMethod]
        public void GetMemberValues_SingleGameSingleDenom()
        {
            SetupMockGameProviderSingleGameSingleDenom();
            var denomInfo = _source.GetMemberValue(_source.Members[0]);
            Assert.AreEqual(AspConstants.DenomAttribute.SingleDenom, denomInfo);
            var denomValue = _source.GetMemberValue(_source.Members[1]);
            Assert.AreEqual((long)20, denomValue);
        }

        /// <summary>
        ///     Get members value case single game multidenom Test
        /// </summary>
        [TestMethod]
        public void GetMemberValues_SingleGameMultiDenom()
        {
            SetupMockGameProviderSingleGameMultiDenom();
            var denomInfo = _source.GetMemberValue(_source.Members[0]);
            Assert.AreEqual(AspConstants.DenomAttribute.MultiDenom, denomInfo);
            var denomValue = _source.GetMemberValue(_source.Members[1]);
            Assert.AreEqual(AspConstants.AccountingDenomination, denomValue);
        }

        /// <summary>
        ///     Event report On Operator Setting Changed event Test
        /// </summary>
        [TestMethod]
        public void HandleMultiDenomAttributeEventTest()
        {
            SetupMockGameProviderSingleGameMultiDenom();
            var memberValueChangedEventsReceived = 0;
            _source.MemberValueChanged += (sender, eventargs) => ++memberValueChangedEventsReceived;
            Assert.IsNotNull(_gameConfigurationSaveCompleteActionCallback);
            _gameConfigurationSaveCompleteActionCallback(new GameConfigurationSaveCompleteEvent());
            Assert.AreEqual(1, memberValueChangedEventsReceived);
        }

        [TestMethod]
        public void Dispose_ShouldUnsubscribeAll()
        {
            //Call dispose twice - should only unsubscribe/deregister from events once
            _source.Dispose();
            _source.Dispose();

            _eventBus.Verify(v => v.UnsubscribeAll(It.IsAny<object>()), Times.Once);
        }
    }
}
