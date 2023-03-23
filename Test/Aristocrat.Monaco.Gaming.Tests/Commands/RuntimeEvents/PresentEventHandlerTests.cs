namespace Aristocrat.Monaco.Gaming.Tests.Commands.RuntimeEvents
{
    using Contracts;
    using Contracts.Meters;
    using Contracts.Payment;
    using Contracts.Session;
    using Gaming.Commands;
    using Gaming.Commands.RuntimeEvents;
    using Gaming.Runtime;
    using Gaming.Runtime.Client;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using PlayMode = Gaming.Runtime.Client.PlayMode;

    [TestClass]
    public class PresentEventHandlerTests
    {
        private readonly Mock<IPropertiesManager> _propertiesManager =
            new Mock<IPropertiesManager>(MockBehavior.Default);

        private readonly Mock<ICommandHandlerFactory> _commandHandlerFactory =
            new Mock<ICommandHandlerFactory>(MockBehavior.Default);

        private readonly Mock<IRuntime> _runtime = new Mock<IRuntime>(MockBehavior.Default);
        private readonly Mock<IGamePlayState> _gamePlayState = new Mock<IGamePlayState>(MockBehavior.Default);
        private readonly Mock<IGameHistory> _gameHistory = new Mock<IGameHistory>(MockBehavior.Default);

        private readonly Mock<IPersistentStorageManager> _persistentStorage =
            new Mock<IPersistentStorageManager>(MockBehavior.Default);

        private readonly Mock<IGameMeterManager> _gameMeterManager = new Mock<IGameMeterManager>(MockBehavior.Default);
        private readonly Mock<IPlayerBank> _bank = new Mock<IPlayerBank>(MockBehavior.Default);
        private readonly Mock<IPlayerService> _playerService = new Mock<IPlayerService>(MockBehavior.Default);

        private readonly Mock<IGameCashOutRecovery> _gameCashoutRecovery =
            new Mock<IGameCashOutRecovery>(MockBehavior.Default);

        private readonly Mock<IEventBus> _bus = new Mock<IEventBus>(MockBehavior.Default);

        private readonly Mock<IPaymentDeterminationProvider> _largeWinDetermination =
            new Mock<IPaymentDeterminationProvider>(MockBehavior.Default);

        private PresentEventHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.MeterFreeGamesIndependently, It.IsAny<bool>()))
                .Returns(false);
            _target = CreateEventHandler();
        }

        [DataTestMethod]
        [DataRow(true, false, false, false, false, false, false, false, false, false, false, false)]
        [DataRow(false, true, false, false, false, false, false, false, false, false, false, false)]
        [DataRow(false, false, true, false, false, false, false, false, false, false, false, false)]
        [DataRow(false, false, false, true, false, false, false, false, false, false, false, false)]
        [DataRow(false, false, false, false, true, false, false, false, false, false, false, false)]
        [DataRow(false, false, false, false, false, true, false, false, false, false, false, false)]
        [DataRow(false, false, false, false, false, false, true, false, false, false, false, false)]
        [DataRow(false, false, false, false, false, false, false, true, false, false, false, false)]
        [DataRow(false, false, false, false, false, false, false, false, true, false, false, false)]
        [DataRow(false, false, false, false, false, false, false, false, false, true, false, false)]
        [DataRow(false, false, false, false, false, false, false, false, false, false, true, false)]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorTest(
            bool nullProperties,
            bool nullCommandHandler,
            bool nullRuntime,
            bool nullGamePlayState,
            bool nullGameHistory,
            bool nullPersistence,
            bool nullMeters,
            bool nullBank,
            bool nullPlayerService,
            bool nullCashoutRecovery,
            bool nullBus,
            bool nullDetermination)
        {
            _target = CreateEventHandler(
                nullProperties,
                nullCommandHandler,
                nullRuntime,
                nullGamePlayState,
                nullGameHistory,
                nullPersistence,
                nullMeters,
                nullBank,
                nullPlayerService,
                nullCashoutRecovery,
                nullBus,
                nullDetermination);
        }

        [DataTestMethod]
        [DataRow(PlayMode.Normal, PlayState.Initiated, false, true, false, true, true, true)]
        [DataRow(PlayMode.Normal, PlayState.Initiated, false, false, false, false, true, true)]
        [DataRow(PlayMode.Normal, PlayState.Initiated, false, true, true, false, false, false)]
        [DataRow(PlayMode.Normal, PlayState.Initiated, true, true, false, false, true, true)]
        [DataRow(PlayMode.Demo, PlayState.Initiated, false, true, false, true, true, true)]
        [DataRow(PlayMode.Demo, PlayState.Initiated, false, false, false, false, true, true)]
        [DataRow(PlayMode.Demo, PlayState.Initiated, false, true, true, false, false, false)]
        [DataRow(PlayMode.Demo, PlayState.Initiated, true, true, false, false, true, true)]
        [DataRow(PlayMode.Recovery, PlayState.Idle, false, true, false, false, false, false)]
        [DataRow(PlayMode.Recovery, PlayState.Idle, false, false, false, true, false, false)]
        [DataRow(PlayMode.Normal, PlayState.PresentationIdle, false, false, false, true, true, true)]
        [DataRow(PlayMode.Normal, PlayState.GameEnded, false, false, false, true, true, true)]
        [DataRow(PlayMode.Normal, PlayState.Idle, false, false, false, true, true, true)]
        [DataRow(PlayMode.Replay, PlayState.Idle, false, true, false, false, false, false)]
        public void HandleInvokedGameRoundEvent(
            PlayMode playMode,
            PlayState playState,
            bool meterFreeGames,
            bool pendingCashoutRecovery,
            bool cashoutRecovering,
            bool expectHandpayCleared,
            bool expectGameEnd,
            bool expectAllowSubGameRounds)
        {
            const long finalWin = 1000;
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.MeterFreeGamesIndependently, It.IsAny<bool>()))
                .Returns(meterFreeGames);
            _target = CreateEventHandler();

            _gamePlayState.Setup(x => x.CurrentState).Returns(playState);
            _gameCashoutRecovery.Setup(x => x.HasPending).Returns(pendingCashoutRecovery);
            _gameCashoutRecovery.Setup(x => x.Recover()).Returns(cashoutRecovering);
            var gameHistoryLog = new Mock<IGameHistoryLog>(MockBehavior.Default);
            gameHistoryLog.Setup(x => x.FinalWin).Returns(finalWin);
            _gameHistory.Setup(x => x.CurrentLog).Returns(gameHistoryLog.Object);

            _target.HandleEvent(
                new GameRoundEvent(
                    GameRoundEventState.Present,
                    GameRoundEventAction.Invoked,
                    playMode,
                    new List<string>(),
                    100,
                    finalWin,
                    100,
                    new byte[] { }));

            _runtime.Verify(
                x => x.UpdateFlag(RuntimeCondition.PendingHandpay, false),
                expectHandpayCleared ? Times.Once() : Times.Never());
            _runtime.Verify(
                x => x.UpdateFlag(RuntimeCondition.AllowSubGameRound, true),
                expectAllowSubGameRounds ? Times.Once() : Times.Never());
            _gamePlayState.Verify(x => x.End(finalWin), expectGameEnd ? Times.Once() : Times.Never());
        }

        [DataTestMethod]
        [DataRow(PlayMode.Normal, true)]
        [DataRow(PlayMode.Demo, true)]
        [DataRow(PlayMode.Recovery, true)]
        [DataRow(PlayMode.Replay, true)]
        public void HandleBeginSpinTest(PlayMode playMode, bool presentationStartedRaised)
        {
            const int activeId = 1;
            const long denomValue = 1000;
            const string testWagerCategoryId = "TestingId";
            var gameDetail = new Mock<IGameDetail>();
            var denom = new Mock<IDenomination>();
            var wagerCategory = new Mock<IWagerCategory>();
            wagerCategory.Setup(x => x.Id).Returns(testWagerCategoryId);
            denom.Setup(x => x.Id).Returns(activeId);
            denom.Setup(x => x.Value).Returns(denomValue);
            denom.Setup(x => x.Active).Returns(true);
            gameDetail.Setup(x => x.Id).Returns(activeId);
            gameDetail.Setup(x => x.Denominations).Returns(new List<IDenomination> { denom.Object });
            var gameHistoryLog = new Mock<IGameHistoryLog>(MockBehavior.Default);
            _gameHistory.Setup(x => x.CurrentLog).Returns(gameHistoryLog.Object);
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.IsGameRunning, It.IsAny<bool>()))
                .Returns(true);
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.SelectedDenom, It.IsAny<long>()))
                .Returns(denomValue);
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.SelectedGameId, It.IsAny<int>()))
                .Returns(activeId);
            _propertiesManager
                .Setup(x => x.GetProperty(GamingConstants.SelectedWagerCategory, It.IsAny<IWagerCategory>()))
                .Returns(wagerCategory.Object);
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.Games, It.IsAny<object>()))
                .Returns(new List<IGameDetail> { gameDetail.Object });

            _target.HandleEvent(
                new GameRoundEvent(
                    GameRoundEventState.Present,
                    GameRoundEventAction.Begin,
                    playMode,
                    new List<string>(),
                    0,
                    0,
                    0,
                    new byte[] { }));
            _bus.Verify(
                x => x.Publish(
                    It.Is<GamePresentationStartedEvent>(
                        evt => MatchingEvent(evt, activeId, denomValue, testWagerCategoryId))),
                presentationStartedRaised ? Times.Once() : Times.Never());
        }

        private static bool MatchingEvent(BaseGameEvent evt, int gameId, long denom, string wagerCategoryId)
        {
            return evt.GameId == gameId && evt.Denomination == denom && evt.WagerCategory == wagerCategoryId;
        }

        private PresentEventHandler CreateEventHandler(
            bool nullProperties = false,
            bool nullCommandHandler = false,
            bool nullRuntime = false,
            bool nullGamePlayState = false,
            bool nullGameHistory = false,
            bool nullPersistence = false,
            bool nullMeters = false,
            bool nullBank = false,
            bool nullPlayerService = false,
            bool nullCashoutRecovery = false,
            bool nullBus = false,
            bool nullDetermination = false)
        {
            return new PresentEventHandler(
                nullProperties ? null : _propertiesManager.Object,
                nullCommandHandler ? null : _commandHandlerFactory.Object,
                nullRuntime ? null : _runtime.Object,
                nullGamePlayState ? null : _gamePlayState.Object,
                nullGameHistory ? null : _gameHistory.Object,
                nullPersistence ? null : _persistentStorage.Object,
                nullMeters ? null : _gameMeterManager.Object,
                nullBank ? null : _bank.Object,
                nullPlayerService ? null : _playerService.Object,
                nullCashoutRecovery ? null : _gameCashoutRecovery.Object,
                nullBus ? null : _bus.Object,
                nullDetermination ? null : _largeWinDetermination.Object);
        }
    }
}