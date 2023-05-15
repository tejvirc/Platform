namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.PackageManifest.Extension.v100;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Gaming;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using PackageManifest.Models;
    using Progressive;
    using Sas.Consumers;
    using Sas.Exceptions;
    using Test.Common;

    [TestClass]
    public class PrimaryGameStartedConsumerTest
    {
        private Mock<IGameProvider> _gameProvider;
        private Mock<IMeterManager> _meterManager;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private Mock<IProtocolLinkedProgressiveAdapter> _progressiveAdapter;

        private readonly List<IDenomination> _denominations = new List<IDenomination>
        {
            new Denomination { Active = true, Value = 1_00_000, BetOption = "testBetOption", LineOption = "testLineOption"}
        };

        private readonly List<c_betOption> _betOptions = new List<c_betOption>
        {
            new c_betOption
            {
                name = "testBetOption",
                description = "test",
                bet = new c_bet[]{},
                maxInitialBet = 20,
                maxInitialBetSpecified = true,
                maxTotalBetSpecified = false
            }
        };

        private readonly List<c_lineOption> _lineOptions = new List<c_lineOption>
        {
            new c_lineOption
            {
                name = "testLineOption",
                description = "test",
                line = new c_line[]{}
            }
        };

        private readonly c_betLinePreset[] _betLinePresets =
        {
            new c_betLinePreset
            {
                betOption = "testBetOption",
                lineOption = "testLineOption"
            }
        };

        private PrimaryGameStartedConsumer _target;

        [TestInitialize]
        public void Initialize()
        {
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Strict);
            _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);
            _progressiveAdapter = new Mock<IProtocolLinkedProgressiveAdapter>(MockBehavior.Default);

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Strict);
            _meterManager.Setup(m => m.IsMeterProvided(GamingMeters.WageredAmount)).Returns(true);
            var coinIn = new Mock<IMeter>();
            coinIn.Setup(m => m.Lifetime).Returns(123456000).Verifiable();
            _meterManager.Setup(m => m.GetMeter(GamingMeters.WageredAmount))
                .Returns(coinIn.Object)
                .Verifiable();
            _progressiveAdapter.Setup(x => x.ViewConfiguredProgressiveLevels(It.IsAny<int>(), It.IsAny<long>()))
                .Returns(Enumerable.Empty<IViewableProgressiveLevel>());

            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _target = CreateConsumer();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataRow(true, false, false, false, false)]
        [DataRow(false, true, false, false, false)]
        [DataRow(false, false, true, false, false)]
        [DataRow(false, false, false, true, false)]
        [DataRow(false, false, false, false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorTest(
            bool nullExceptionHandler,
            bool nullGameProvider,
            bool nullMeterManager,
            bool nullPropertiesManager,
            bool nullProgressiveAdapter)
        {
            _target = CreateConsumer(
                nullExceptionHandler,
                nullGameProvider,
                nullMeterManager,
                nullPropertiesManager,
                nullProgressiveAdapter);
        }

        [TestMethod]
        public void ConsumeMultipleDenominationsTest()
        {
            GameStartedExceptionBuilder actual = null;
            _exceptionHandler.Setup(m => m.ReportException(It.IsAny<Func<byte, ISasExceptionCollection>>(), GeneralExceptionCode.GameHasStarted))
                .Callback((Func<byte, ISasExceptionCollection> g, GeneralExceptionCode _) => actual = g.Invoke(1) as GameStartedExceptionBuilder)
                .Verifiable();

            var gameId = 1;
            var denomination = 1_00_000; // $1 in millicents
            const string wagerCategory = "1";
            var historyLog = new Mock<IGameHistoryLog>(MockBehavior.Strict);

            historyLog.Setup(m => m.InitialWager).Returns(2_00); // $2.00 wager
            historyLog.Setup(m => m.ShallowCopy()).Returns(historyLog.Object);
            var game = new Mock<IGameDetail>(MockBehavior.Default);
            game.Setup(m => m.Denominations).Returns(_denominations);
            game.Setup(m => m.BetOptionList).Returns(new BetOptionList(_betOptions, _betLinePresets));
            game.Setup(m => m.LineOptionList).Returns(new LineOptionList(_lineOptions));
            game.Setup(m => m.ActiveDenominations).Returns(new List<long> { 1, 5, 10 });
            _gameProvider.Setup(m => m.GetGame(1)).Returns(game.Object);

            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.Games, It.IsAny<object>())).Returns(game);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasHosts, It.IsAny<object>()))
                .Returns(new List<Host>
                {
                    new Host { AccountingDenom = 1 },
                    new Host { AccountingDenom = 1 }
                });

            var expectedGameData = new GameStartData
            {
                CoinInMeter = 123456000,
                CreditsWagered = 2,
                ProgressiveGroup = 0,
                WagerType = 0x46
            };

            var expected = new GameStartedExceptionBuilder(expectedGameData, 1);
            var @event = new PrimaryGameStartedEvent(gameId, denomination, wagerCategory, historyLog.Object);

            _target.Consume(@event);

            // we said credits are $1 and we wagered $2
            Assert.IsNotNull(actual);
            CollectionAssert.AreEquivalent(expected, actual);

            _gameProvider.Verify();
            _exceptionHandler.Verify();
            _meterManager.Verify();
        }

        [TestMethod]
        public void ConsumeMultipleDenominationsWithProgressivesTest()
        {
            GameStartedExceptionBuilder actual = null;
            _exceptionHandler.Setup(m => m.ReportException(It.IsAny<Func<byte, ISasExceptionCollection>>(), GeneralExceptionCode.GameHasStarted))
                .Callback((Func<byte, ISasExceptionCollection> g, GeneralExceptionCode _) => actual = g.Invoke(1) as GameStartedExceptionBuilder)
                .Verifiable();

            const byte progressiveGroupId = 23;
            var gameId = 1;
            var denomination = 1_00_000; // $1 in millicents
            const string wagerCategory = "1";
            var historyLog = new Mock<IGameHistoryLog>(MockBehavior.Strict);
            var progressiveKey = "TestLevel";

            var mockLevel = new Mock<IViewableProgressiveLevel>();
            mockLevel.Setup(x => x.AssignedProgressiveId).Returns(
                new AssignableProgressiveId(AssignableProgressiveType.Linked, progressiveKey));
            var level = mockLevel.Object;
            var mockLinked = new Mock<IViewableLinkedProgressiveLevel>();
            mockLinked.Setup(x => x.ProtocolName).Returns(ProgressiveConstants.ProtocolName);
            var linkedLevel = mockLinked.Object;
            _progressiveAdapter.Setup(x => x.ViewLinkedProgressiveLevel(progressiveKey, out linkedLevel)).Returns(true);
            _progressiveAdapter.Setup(x => x.ViewConfiguredProgressiveLevels(It.IsAny<int>(), It.IsAny<long>()))
                .Returns(new List<IViewableProgressiveLevel> { level });
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { ProgressiveGroupId = progressiveGroupId });

            historyLog.Setup(m => m.InitialWager).Returns(2_00); // $2.00 wager
            historyLog.Setup(m => m.ShallowCopy()).Returns(historyLog.Object);
            var game = new Mock<IGameDetail>(MockBehavior.Default);
            game.Setup(m => m.Denominations).Returns(_denominations);
            game.Setup(m => m.BetOptionList).Returns(new BetOptionList(_betOptions, _betLinePresets));
            game.Setup(m => m.LineOptionList).Returns(new LineOptionList(_lineOptions));
            game.Setup(m => m.ActiveDenominations).Returns(new List<long> { 1, 5, 10 });
            _gameProvider.Setup(m => m.GetGame(1)).Returns(game.Object);

            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.Games, It.IsAny<object>())).Returns(game);
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasHosts, It.IsAny<object>()))
                .Returns(new List<Host>
                {
                    new Host { AccountingDenom = 1 },
                    new Host { AccountingDenom = 1 }
                });

            var expectedGameData = new GameStartData
            {
                CoinInMeter = 123456000,
                CreditsWagered = 2,
                ProgressiveGroup = progressiveGroupId,
                WagerType = 0x46
            };

            var expected = new GameStartedExceptionBuilder(expectedGameData, 1);
            var @event = new PrimaryGameStartedEvent(gameId, denomination, wagerCategory, historyLog.Object);

            _target.Consume(@event);

            // we said credits are $1 and we wagered $2
            Assert.IsNotNull(actual);
            CollectionAssert.AreEquivalent(expected, actual);

            _gameProvider.Verify();
            _exceptionHandler.Verify();
            _meterManager.Verify();
        }

        private PrimaryGameStartedConsumer CreateConsumer(
            bool nullExceptionHandler = false,
            bool nullGameProvider = false,
            bool nullMeterManager = false,
            bool nullPropertiesManager = false,
            bool nullProgressiveAdapter = false)
        {
            return new PrimaryGameStartedConsumer(
                nullExceptionHandler ? null : _exceptionHandler.Object,
                nullGameProvider ? null : _gameProvider.Object,
                nullMeterManager ? null : _meterManager.Object,
                nullPropertiesManager ? null : _propertiesManager.Object,
                nullProgressiveAdapter ? null : _progressiveAdapter.Object);
        }
    }
}