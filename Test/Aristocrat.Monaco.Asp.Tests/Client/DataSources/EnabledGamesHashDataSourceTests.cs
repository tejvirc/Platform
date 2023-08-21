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
    ///     Enabled Games Hash DataSource Test
    /// </summary>
    [TestClass]
    public class EnabledGamesHashDataSourceTests
    {
        private Mock<IEventBus> _eventBus;
        private Mock<IAspGameProvider> _aspGameProvider;
        private EnabledGamesHashDataSource _source;
        private Action<GameConfigurationSaveCompleteEvent> _gameConfigurationSaveCompleteActionCallback;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _aspGameProvider = new Mock<IAspGameProvider>(MockBehavior.Strict);
            _eventBus = new Mock<IEventBus>();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<EnabledGamesHashDataSource>(), It.IsAny<Action<GameConfigurationSaveCompleteEvent>>()))
                .Callback<object, Action<GameConfigurationSaveCompleteEvent>>((subscriber, callback) => _gameConfigurationSaveCompleteActionCallback = callback);
            _source = new EnabledGamesHashDataSource(_aspGameProvider.Object, _eventBus.Object);
        }

        private static IGameDetail SetupMockGameEnabledProfile(int id, string theme, string version, string variationId, decimal minimumRTP)
        {
            var mockGameProfile = new Mock<IGameDetail>(MockBehavior.Strict);
            mockGameProfile.SetupGet(g => g.Id).Returns(() => id);
            mockGameProfile.SetupGet(g => g.ThemeName).Returns(() => theme);
            mockGameProfile.SetupGet(g => g.Version).Returns(() => version);
            mockGameProfile.SetupGet(g => g.VariationId).Returns(() => variationId);
            mockGameProfile.SetupGet(g => g.MinimumPaybackPercent).Returns(() => minimumRTP);
            return mockGameProfile.Object;
        }

        private static IDenomination SetupMockDenomProfile(bool active, long denomId, long denomValue, int maxWagerInCredit)
        {
            var mockDenomProfile = new Mock<IDenomination>(MockBehavior.Strict);
            mockDenomProfile.SetupGet(d => d.Active).Returns(() => active);
            mockDenomProfile.SetupGet(d => d.Id).Returns(() => denomId);
            mockDenomProfile.SetupGet(d => d.MaximumWagerCredits).Returns(() => maxWagerInCredit);
            mockDenomProfile.SetupGet(d => d.Value).Returns(() => denomValue);
            return mockDenomProfile.Object;
        }
        private void SetupMockGameProviderSingleDenom()
        {
            var mockTuple = new List<(IGameDetail, IDenomination)>
            {
                (SetupMockGameEnabledProfile(3, "Buffalo", "1.02-66389", "99", 87.25M), SetupMockDenomProfile(true, 12, 5000, 600))
            };

            _aspGameProvider.Setup(g => g.GetEnabledGames()).Returns(() => mockTuple);
        }

        /// <summary>
        ///     Test Not Null for the Enabled Game Hash DataSource
        /// </summary>
        [TestMethod]
        public void DataSourceTest() => Assert.IsNotNull(_source);

        /// <summary>
        ///     Instantiate the Enabled Game Hash DataSource with Game Provider not null Test
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullGameProviderTest()
        {
            var _ = new EnabledGamesHashDataSource(_aspGameProvider.Object, null);
        }

        [TestMethod]
        public void DataSourceNameTest()
        {
            var expectedName = "Hash_Of_Enabled_Games";
            Assert.AreEqual(expectedName, _source.Name);
        }

        /// <summary>
        ///     Test Hash value changed with each of new different property configured.
        ///     Change of any element in the hashing formular will result to different hash
        /// </summary>
        [TestMethod]
        public void GetMemberValuesMultiPropertiesChangedTest()
        {
            SetupMockGameProviderSingleDenom();

            var activeDenomHash = 5000.GetHashCode();
            var variationHash = "99".GetHashCode();
            var minRtpHash = 87.25M.GetHashCode();
            var gameVersionHash = "1.02-66389".GetHashCode();

            var resultAtOrigin = activeDenomHash ^ variationHash ^ minRtpHash ^ gameVersionHash;
            var GetMemberHashValue = _source.GetMemberValue(_source.Members[0]);
            Assert.AreNotEqual(0, GetMemberHashValue);
            Assert.AreEqual(resultAtOrigin.ToString("X8"), GetMemberHashValue);

            var resultOnlyNewDenomChanged = 1000.GetHashCode() ^ variationHash ^ minRtpHash ^ gameVersionHash;
            Assert.AreNotEqual(resultOnlyNewDenomChanged.ToString("X8"), GetMemberHashValue);

            var resultOnlyNewVariationChanged = activeDenomHash ^ "01".GetHashCode() ^ minRtpHash ^ gameVersionHash;
            Assert.AreNotEqual(resultOnlyNewVariationChanged.ToString("X8"), GetMemberHashValue);

            var resultOnlyNewRtpChanged = activeDenomHash ^ variationHash ^ 89.75M.GetHashCode() ^ gameVersionHash;
            Assert.AreNotEqual(resultOnlyNewRtpChanged.ToString("X8"), GetMemberHashValue);

            var resultOnlyNewVersionChanged = activeDenomHash ^ variationHash ^ minRtpHash ^ "1.02-66311".GetHashCode();
            Assert.AreNotEqual(resultOnlyNewVersionChanged.ToString("X8"), GetMemberHashValue);
        }

        /// <summary>
        ///     Event report On Operator Menu Game Configuration Changed Test
        /// </summary>
        [TestMethod]
        public void HandleEnabledGamesHashEventTest()
        {
            SetupMockGameProviderSingleDenom();

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
