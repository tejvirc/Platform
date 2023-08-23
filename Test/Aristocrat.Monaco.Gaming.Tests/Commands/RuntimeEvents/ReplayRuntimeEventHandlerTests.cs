namespace Aristocrat.Monaco.Gaming.Tests.Commands.RuntimeEvents
{
    using Contracts;
    using Gaming.Commands;
    using Gaming.Commands.RuntimeEvents;
    using Gaming.Runtime.Client;
    using Google.Protobuf.WellKnownTypes;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;

    [TestClass]
    public class ReplayRuntimeEventHandlerTests
    {
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
        private readonly Mock<IEventBus> _bus = new Mock<IEventBus>(MockBehavior.Default);
        private readonly Mock<IGameDiagnostics> _gameDiagnostics = new Mock<IGameDiagnostics>(MockBehavior.Default);
        private readonly Mock<IGameProvider> _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);

        private ReplayRuntimeEventHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateEventHandler();

            var mockContext = new Mock<IDiagnosticContext<IGameHistoryLog>>(MockBehavior.Default);
            _gameDiagnostics.SetupGet(m => m.Context).Returns(mockContext.Object);

            const int activeId = 1;
            const long denomValue = 1000;

            var denom = new Mock<IDenomination>();
            denom.Setup(x => x.Id).Returns(activeId);
            denom.Setup(x => x.Value).Returns(denomValue);
            denom.Setup(x => x.Active).Returns(true);

            var gameDetail = new Mock<IGameDetail>();
            gameDetail.Setup(x => x.Id).Returns(activeId);
            gameDetail.Setup(x => x.Denominations).Returns(new List<IDenomination> { denom.Object });

            _gameProvider.Setup(x => x.GetActiveGame()).Returns((gameDetail.Object, denom.Object));
        }

        [DataTestMethod]
        [DataRow(true, false)]
        [DataRow(false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorTest(
            bool nullProperties,
            bool nullBus)
        {
            _target = CreateEventHandler(
                nullProperties,
                nullBus);
        }

        [TestMethod]
        public void HandleReplayGameRoundEventBegin()
        {
            GameRoundEventSetup();

            _target.HandleEvent(
                new ReplayGameRoundEvent(
                GameRoundEventState.Present,
                GameRoundEventAction.Begin,
                new List<string>()));

            _bus.Verify(b => b.Publish(It.IsAny<GamePresentationStartedEvent>()), Times.Once);
        }

        [TestMethod]
        public void HandleReplayGameRoundEventPending()
        {
            GameRoundEventSetup();

            _target.HandleEvent(
                new ReplayGameRoundEvent(
                GameRoundEventState.Present,
                GameRoundEventAction.Pending,
                new List<string>()));

            _bus.Verify(b => b.Publish(It.IsAny<GameWinPresentationStartedEvent>()), Times.Once);
        }

        [TestMethod]
        public void HandleReplayGameRoundEventCompleted()
        {
            GameRoundEventSetup();

            _target.HandleEvent(
                new ReplayGameRoundEvent(
                GameRoundEventState.Present,
                GameRoundEventAction.Completed,
                new List<string>()));

            _bus.Verify(b => b.Publish(It.IsAny<GamePresentationEndedEvent>()), Times.Once);
        }

        [TestMethod]
        public void HandleReplayGameRoundEventInvoked()
        {
            GameRoundEventSetup();

            _target.HandleEvent(
                new ReplayGameRoundEvent(
                GameRoundEventState.Present,
                GameRoundEventAction.Invoked,
                new List<string>()));

            _bus.Verify(b => b.Publish(It.IsAny<GameReplayCompletedEvent>()), Times.Once);
        }

        private void GameRoundEventSetup()
        {
            var gameId = 2;
            var selectedDenom = 5L;
            var denom = new Denomination { Value = selectedDenom };
            var detail = new GameDetail { Id = gameId, Denominations = new List<Denomination> { denom } };
            var wagerCategory = new WagerCategory("Id", 0);

            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, It.IsAny<object>())).Returns(true);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.SelectedGameId, It.IsAny<object>())).Returns(gameId);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.Games, It.IsAny<object>())).Returns(new List<IGameDetail> { detail });
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.SelectedDenom, It.IsAny<object>())).Returns(selectedDenom);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.SelectedWagerCategory, It.IsAny<object>())).Returns(wagerCategory);
        }

        private ReplayRuntimeEventHandler CreateEventHandler(
            bool nullProperties = false,
            bool nullGameProvider = false,
            bool nullBus = false,
            bool nullGameDiagnostics = false)
        {
            return new ReplayRuntimeEventHandler(
                nullProperties ? null : _propertiesManager.Object,
                nullGameProvider ? null : _gameProvider.Object,
                nullBus ? null : _bus.Object,
                nullGameDiagnostics ? null : _gameDiagnostics.Object);
        }
    }
}
