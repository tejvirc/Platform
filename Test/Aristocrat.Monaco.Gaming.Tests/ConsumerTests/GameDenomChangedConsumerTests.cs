namespace Aristocrat.Monaco.Gaming.Tests.ConsumerTests
{
    using System;
    using System.Collections.Generic;
    using Consumers;
    using Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using Tickets;

    [TestClass]
    public class GameDenomChangedConsumerTests
    {
        private readonly Mock<IEventBus> _bus = new Mock<IEventBus>(MockBehavior.Default);
        private readonly Mock<IGamePlayState> _gamePlay = new Mock<IGamePlayState>(MockBehavior.Default);
        private readonly Mock<IGameService> _gameService = new Mock<IGameService>(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _properties = new Mock<IPropertiesManager>(MockBehavior.Default);
        private GameDenomChangedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.AddService(_bus);
            _target = CreateConsumer();
        }

        [DataRow(true, false, false, false, DisplayName = "Null Properties Manager Test")]
        [DataRow(false, true, false, false, DisplayName = "Null Game Service Test")]
        [DataRow(false, false, true, false, DisplayName = "Null Game Play State Test")]
        [DataRow(false, false, false, true, DisplayName = "Null Event Bus Test")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorTest(
            bool nullProperties = false,
            bool nullGameService = false,
            bool nullGamePlayState = false,
            bool nullEventBus = false)
        {
            _target = CreateConsumer(nullProperties, nullGameService, nullGamePlayState, nullEventBus);
        }

        [DataRow(1, 1000L, "1", 1, false, true, true)]
        [DataRow(1, 1000L, "1", 1, true, true, false)]
        [DataRow(1, 1000L, "1", 3, true, true, false)]
        [DataRow(1, 1000L, "1", 3, false, true, false)]
        [DataRow(1, 1000L, "1", 1, false, false, true)]
        [DataRow(1, 1000L, "1", 1, true, false, false)]
        [DataRow(1, 1000L, "1", 3, true, false, false)]
        [DataRow(1, 1000L, "1", 3, false, false, false)]
        [DataTestMethod]
        public void ConsumeTest(
            int selectedGameId,
            long selectedDenom,
            string wagerCategory,
            int changedGameId,
            bool isSelectedDenomActive,
            bool isGameIdle,
            bool isGameShutdown)
        {
            _properties.Setup(x => x.GetProperty(GamingConstants.SelectedGameId, It.IsAny<int>()))
                .Returns(selectedGameId);
            _properties.Setup(x => x.GetProperty(GamingConstants.SelectedDenom, It.IsAny<long>()))
                .Returns(selectedDenom);
            _gamePlay.Setup(x => x.Idle).Returns(isGameIdle);
            _bus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<GameIdleEvent>>()))
                .Callback<object, Action<GameIdleEvent>>(
                    (_, action) => action.Invoke(
                        new GameIdleEvent(selectedGameId, selectedDenom, wagerCategory, new GameHistoryLog(0))));
            var game = new TestGameProfile
            {
                Id = changedGameId,
                ActiveDenominations = isSelectedDenomActive ? new List<long> { selectedDenom } : new List<long>()
            };

            _target.Consume(new GameDenomChangedEvent(changedGameId, game, false, 1));
            if (isGameShutdown)
            {
                _gameService.Verify(x => x.ShutdownBegin(), Times.Once);
            }
            else
            {
                _gameService.Verify(x => x.ShutdownBegin(), Times.Never);
            }
        }

        private GameDenomChangedConsumer CreateConsumer(
            bool nullProperties = false,
            bool nullGameService = false,
            bool nullGamePlayState = false,
            bool nullEventBus = false)
        {
            return new GameDenomChangedConsumer(
                nullProperties ? null : _properties.Object,
                nullGameService ? null : _gameService.Object,
                nullGamePlayState ? null : _gamePlay.Object,
                nullEventBus ? null : _bus.Object);
        }
    }
}