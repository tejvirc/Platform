using Aristocrat.Monaco.Accounting.Contracts;
using Aristocrat.Monaco.Application.Contracts;
using Aristocrat.Monaco.Common;
using Aristocrat.Monaco.Gaming.Contracts;
using Aristocrat.Monaco.Gaming.Contracts.Events;
using Aristocrat.Monaco.Gaming.Contracts.Lobby;
using Aristocrat.Monaco.Gaming.Contracts.Models;
using Aristocrat.Monaco.Gaming.Runtime;
using Aristocrat.Monaco.Kernel;
using Aristocrat.Monaco.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Addins;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Aristocrat.Monaco.Gaming.Tests
{

    [TestClass]
    public class LobbyClockServiceTests
    {
        private const long GamePlayingIntervalInMilliseconds = 600_000; // 10 minutes
        private const long GameIdleIntervalInMilliseconds = 25_000;
        private const long NoCreditIntervalInMilliseconds = 30_000;

        private LobbyClockService _lobbyClockService;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IEventBus> _eventBus;
        private Mock<ILobbyStateManager> _lobbyStateManager;
        private Mock<ISystemDisableManager> _systemDisableManager;
        private Mock<IBank> _bank;
        private Mock<IGameProvider> _gameProvider;
        private Mock<IRuntime> _runtime;

        // Just a simple timer not Started() yet
        private Mock<ISystemTimerWrapper> _mainTimer;

        private Mock<IStopwatch> _sessionFlashesCountdown;
        private Mock<IStopwatch> _timeSinceLastGameCountdown;
        private Mock<IStopwatch> _insufficientCreditCountdown;
        private Mock<IStopwatch> _timeBetweenFlashesCountdown;

        private Action<PrimaryGameStartedEvent> _primaryGameStartedEventHandler;
        private Action<GameEndedEvent> _gameEndedEvent;
        private Action<BankBalanceChangedEvent> _bankBalanceChangedEvent;
        private Action<CashOutButtonPressedEvent> _cashOutButtonPressedEvent;

        [TestInitialize]
        public void TestInitialize()
        {

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            //Properties Manager
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Loose);
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.CurrencyMultiplierKey, 1d)).Returns(100000d);
            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);
            _lobbyStateManager = MoqServiceManager.CreateAndAddService<ILobbyStateManager>(MockBehavior.Strict);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);

            _eventBus.Setup(f => f.Publish(It.IsAny<LobbyClockFlashChangedEvent>())).Verifiable();
            _eventBus.Setup(f => f.Subscribe(It.IsAny<LobbyClockService>(), It.IsAny<Action<PrimaryGameStartedEvent>>()))
                .Callback<object, Action<PrimaryGameStartedEvent>>((p, s) => _primaryGameStartedEventHandler = s);
            _eventBus.Setup(f => f.Subscribe(It.IsAny<LobbyClockService>(), It.IsAny<Action<GameEndedEvent>>()))
                .Callback<object, Action<GameEndedEvent>>((p, s) => _gameEndedEvent = s);
            _eventBus.Setup(f => f.Subscribe(It.IsAny<LobbyClockService>(), It.IsAny<Action<BankBalanceChangedEvent>>()))
                .Callback<object, Action<BankBalanceChangedEvent>>((p, s) => _bankBalanceChangedEvent = s);
            _eventBus.Setup(f => f.Subscribe(It.IsAny<LobbyClockService>(), It.IsAny<Action<CashOutButtonPressedEvent>>()))
                .Callback<object, Action<CashOutButtonPressedEvent>>((p, s) => _cashOutButtonPressedEvent = s);
            Setup();
        }

        [TestMethod]
        public void LobbyClockTestFlashInLobby()
        {
            // Set Lobby State
            _lobbyStateManager.Setup(t => t.IsInState(LobbyState.Chooser)).Returns(true);

            _lobbyClockService =
                new LobbyClockService(_eventBus.Object,
                                      _propertiesManager.Object,
                                      _systemDisableManager.Object,
                                      _bank.Object,
                                      _gameProvider.Object,
                                      _lobbyStateManager.Object,
                                      _runtime.Object);

            _lobbyClockService.SetupTimers(_mainTimer.Object,
                                      _sessionFlashesCountdown.Object,
                                      _timeSinceLastGameCountdown.Object,
                                      _insufficientCreditCountdown.Object,
                                      _timeBetweenFlashesCountdown.Object);

            _lobbyClockService.Initialize();
            _timeBetweenFlashesCountdown.Setup(x => x.ElapsedMilliseconds).Returns(1500);

            // LobbyClockService is now InSession
            _primaryGameStartedEventHandler.Invoke(new PrimaryGameStartedEvent(1, 2, "test", new GameHistoryLog(3)));

            _lobbyClockService.LobbyFlashCheckState(_lobbyClockService, new EventArgs() as ElapsedEventArgs);
            _lobbyClockService.LobbyFlashCheckState(_lobbyClockService, new EventArgs() as ElapsedEventArgs);
            _lobbyClockService.LobbyFlashCheckState(_lobbyClockService, new EventArgs() as ElapsedEventArgs);
            _lobbyClockService.LobbyFlashCheckState(_lobbyClockService, new EventArgs() as ElapsedEventArgs);
            _eventBus.Verify(t => t.Publish(It.IsAny<LobbyClockFlashChangedEvent>()), Times.Exactly(5));
        }


        [TestMethod]
        public void LobbyClockFlashInGame()
        {
            // Set Lobby State
            _lobbyStateManager.Setup(t => t.IsInState(LobbyState.Game)).Returns(true);

            _lobbyClockService =
                new LobbyClockService(_eventBus.Object,
                                      _propertiesManager.Object,
                                      _systemDisableManager.Object,
                                      _bank.Object,
                                      _gameProvider.Object,
                                      _lobbyStateManager.Object,
                                      _runtime.Object);

            _lobbyClockService.SetupTimers(_mainTimer.Object,
                                      _sessionFlashesCountdown.Object,
                                      _timeSinceLastGameCountdown.Object,
                                      _insufficientCreditCountdown.Object,
                                      _timeBetweenFlashesCountdown.Object);

            _lobbyClockService.Initialize();
            _timeBetweenFlashesCountdown.Setup(x => x.ElapsedMilliseconds).Returns(1500);

            // LobbyClockService is now InSession
            _primaryGameStartedEventHandler.Invoke(new PrimaryGameStartedEvent(1, 2, "test", new GameHistoryLog(3)));

            _lobbyClockService.LobbyFlashCheckState(_lobbyClockService, new EventArgs() as ElapsedEventArgs);
            _lobbyClockService.LobbyFlashCheckState(_lobbyClockService, new EventArgs() as ElapsedEventArgs);
            _lobbyClockService.LobbyFlashCheckState(_lobbyClockService, new EventArgs() as ElapsedEventArgs);
            _lobbyClockService.LobbyFlashCheckState(_lobbyClockService, new EventArgs() as ElapsedEventArgs);

            _runtime.Verify(t => t.OnSessionTickFlashClock(), Times.Exactly(5));
        }

        [TestMethod]
        public void LobbyClockTryFlashNotInSession()
        {
            // Set Lobby State
            _lobbyStateManager.Setup(t => t.IsInState(LobbyState.Chooser)).Returns(true);

            _lobbyClockService =
                new LobbyClockService(_eventBus.Object,
                                      _propertiesManager.Object,
                                      _systemDisableManager.Object,
                                      _bank.Object,
                                      _gameProvider.Object,
                                      _lobbyStateManager.Object,
                                      _runtime.Object);

            _lobbyClockService.SetupTimers(_mainTimer.Object,
                                      _sessionFlashesCountdown.Object,
                                      _timeSinceLastGameCountdown.Object,
                                      _insufficientCreditCountdown.Object,
                                      _timeBetweenFlashesCountdown.Object);

            _lobbyClockService.Initialize();
            _timeBetweenFlashesCountdown.Setup(x => x.ElapsedMilliseconds).Returns(1500);
            _lobbyClockService.LobbyFlashCheckState(_lobbyClockService, new EventArgs() as ElapsedEventArgs);

            _eventBus.Verify(t => t.Publish(It.IsAny<LobbyClockFlashChangedEvent>()), Times.Never);
        }

        [TestMethod]
        public void LobbyClockFlashInGameAndInLobby()
        {
            // Set Lobby State
            _lobbyStateManager.Setup(t => t.IsInState(LobbyState.GameLoading)).Returns(true);

            _lobbyClockService =
                new LobbyClockService(_eventBus.Object,
                                      _propertiesManager.Object,
                                      _systemDisableManager.Object,
                                      _bank.Object,
                                      _gameProvider.Object,
                                      _lobbyStateManager.Object,
                                      _runtime.Object);

            _lobbyClockService.SetupTimers(_mainTimer.Object,
                                      _sessionFlashesCountdown.Object,
                                      _timeSinceLastGameCountdown.Object,
                                      _insufficientCreditCountdown.Object,
                                      _timeBetweenFlashesCountdown.Object);

            _lobbyClockService.Initialize();
            _timeBetweenFlashesCountdown.Setup(x => x.ElapsedMilliseconds).Returns(1500);

            // LobbyClockService is now InSession
            _primaryGameStartedEventHandler.Invoke(new PrimaryGameStartedEvent(1, 2, "test", new GameHistoryLog(3)));

            _lobbyClockService.LobbyFlashCheckState(_lobbyClockService, new EventArgs() as ElapsedEventArgs);
            _lobbyStateManager.Setup(t => t.IsInState(LobbyState.Game)).Returns(true);
            _lobbyClockService.LobbyFlashCheckState(_lobbyClockService, new EventArgs() as ElapsedEventArgs);
            _lobbyClockService.LobbyFlashCheckState(_lobbyClockService, new EventArgs() as ElapsedEventArgs);
            _lobbyStateManager.Setup(t => t.IsInState(LobbyState.Game)).Returns(false);
            _lobbyStateManager.Setup(t => t.IsInState(LobbyState.Chooser)).Returns(true);
            _lobbyClockService.LobbyFlashCheckState(_lobbyClockService, new EventArgs() as ElapsedEventArgs);
            _lobbyClockService.LobbyFlashCheckState(_lobbyClockService, new EventArgs() as ElapsedEventArgs);
            _lobbyClockService.LobbyFlashCheckState(_lobbyClockService, new EventArgs() as ElapsedEventArgs);
            _runtime.Verify(t => t.OnSessionTickFlashClock(), Times.Exactly(2));
            _eventBus.Verify(t => t.Publish(It.IsAny<LobbyClockFlashChangedEvent>()), Times.Exactly(3));
        }

        [TestMethod]
        public void LobbyClockDisableDuringFlash()
        {
            // Set Lobby State
            _lobbyStateManager.Setup(t => t.IsInState(LobbyState.Chooser)).Returns(true);

            _lobbyClockService =
                new LobbyClockService(_eventBus.Object,
                                      _propertiesManager.Object,
                                      _systemDisableManager.Object,
                                      _bank.Object,
                                      _gameProvider.Object,
                                      _lobbyStateManager.Object,
                                      _runtime.Object);

            _lobbyClockService.SetupTimers(_mainTimer.Object,
                                      _sessionFlashesCountdown.Object,
                                      _timeSinceLastGameCountdown.Object,
                                      _insufficientCreditCountdown.Object,
                                      _timeBetweenFlashesCountdown.Object);

            _lobbyClockService.Initialize();
            _timeBetweenFlashesCountdown.Setup(x => x.ElapsedMilliseconds).Returns(1500);
            _primaryGameStartedEventHandler.Invoke(new PrimaryGameStartedEvent(1, 2, "test", new GameHistoryLog(3)));

            _lobbyClockService.LobbyFlashCheckState(_lobbyClockService, new EventArgs() as ElapsedEventArgs);
            _lobbyClockService.LobbyFlashCheckState(_lobbyClockService, new EventArgs() as ElapsedEventArgs);
            _lobbyClockService.LobbyFlashCheckState(_lobbyClockService, new EventArgs() as ElapsedEventArgs);
            _systemDisableManager.Setup(t => t.IsDisabled).Returns(true);

            _eventBus.Verify(t => t.Publish(It.IsAny<LobbyClockFlashChangedEvent>()), Times.Exactly(4));
        }

        [TestMethod]
        public void LobbyClockOnCashoutTest()
        {
            // Set Lobby State
            _lobbyStateManager.Setup(t => t.IsInState(LobbyState.Game)).Returns(true);

            _sessionFlashesCountdown.Setup(t => t.Stop()).Verifiable();
            _insufficientCreditCountdown.Setup(t => t.Stop()).Verifiable();
            _timeSinceLastGameCountdown.Setup(t => t.Stop()).Verifiable();

            _lobbyClockService =
                new LobbyClockService(_eventBus.Object,
                                      _propertiesManager.Object,
                                      _systemDisableManager.Object,
                                      _bank.Object,
                                      _gameProvider.Object,
                                      _lobbyStateManager.Object,
                                      _runtime.Object);

            _lobbyClockService.SetupTimers(_mainTimer.Object,
                                      _sessionFlashesCountdown.Object,
                                      _timeSinceLastGameCountdown.Object,
                                      _insufficientCreditCountdown.Object,
                                      _timeBetweenFlashesCountdown.Object);

            _lobbyClockService.Initialize();
            _timeBetweenFlashesCountdown.Setup(x => x.ElapsedMilliseconds).Returns(1500);
            _primaryGameStartedEventHandler.Invoke(new PrimaryGameStartedEvent(1, 2, "test", new GameHistoryLog(3)));
            _lobbyClockService.LobbyFlashCheckState(_lobbyClockService, new EventArgs() as ElapsedEventArgs);
            _lobbyClockService.LobbyFlashCheckState(_lobbyClockService, new EventArgs() as ElapsedEventArgs);
            _lobbyClockService.LobbyFlashCheckState(_lobbyClockService, new EventArgs() as ElapsedEventArgs);
            _cashOutButtonPressedEvent.Invoke(new CashOutButtonPressedEvent());


            _runtime.Verify(t => t.OnSessionTickFlashClock(), Times.Exactly(4));

            _sessionFlashesCountdown.Verify(t => t.Stop(), Times.Once);
            _insufficientCreditCountdown.Verify(t => t.Stop(), Times.Once);
            _timeSinceLastGameCountdown.Verify(t => t.Stop(), Times.Once);
        }

        [TestMethod]
        public void LobbyClockInsufficientCreditTest()
        {
            // Set Lobby State
            _lobbyStateManager.Setup(t => t.IsInState(LobbyState.Chooser)).Returns(true);

            _sessionFlashesCountdown.Setup(t => t.Reset()).Verifiable();
            _insufficientCreditCountdown.Setup(t => t.Reset()).Verifiable();
            _insufficientCreditCountdown.Setup(t => t.Start()).Verifiable();
            _timeSinceLastGameCountdown.Setup(t => t.Reset()).Verifiable();

            _lobbyClockService =
                new LobbyClockService(_eventBus.Object,
                                      _propertiesManager.Object,
                                      _systemDisableManager.Object,
                                      _bank.Object,
                                      _gameProvider.Object,
                                      _lobbyStateManager.Object,
                                      _runtime.Object);

            _lobbyClockService.SetupTimers(_mainTimer.Object,
                                      _sessionFlashesCountdown.Object,
                                      _timeSinceLastGameCountdown.Object,
                                      _insufficientCreditCountdown.Object,
                                      _timeBetweenFlashesCountdown.Object);

            _lobbyClockService.Initialize();
            //Skip initial Flash
            _sessionFlashesCountdown.Setup(t => t.IsRunning).Returns(true);
            _timeBetweenFlashesCountdown.Setup(x => x.ElapsedMilliseconds).Returns(1500);
            CreateInsufficientFundsForNextGame();
            _primaryGameStartedEventHandler.Invoke(new PrimaryGameStartedEvent(1, 2, "test", new GameHistoryLog(3)));
            _bankBalanceChangedEvent.Invoke(new BankBalanceChangedEvent(1L, 2L, new Guid()));

            _insufficientCreditCountdown.Setup(x => x.ElapsedMilliseconds).Returns(NoCreditIntervalInMilliseconds + 1);
            _lobbyClockService.LobbyFlashCheckState(_lobbyClockService, new EventArgs() as ElapsedEventArgs);

            _insufficientCreditCountdown.Verify(t => t.Start(), Times.Once);
            _timeSinceLastGameCountdown.Verify(t => t.Reset(), Times.Exactly(2));
            _timeBetweenFlashesCountdown.Verify(t => t.Reset(), Times.Once);
            _sessionFlashesCountdown.Verify(t => t.Reset(), Times.Once);
        }

        private void Setup()
        {
            _systemDisableManager = new Mock<ISystemDisableManager>();
            _bank = new Mock<IBank>();
            _gameProvider = new Mock<IGameProvider>();
            _lobbyStateManager = new Mock<ILobbyStateManager>();
            _runtime = new Mock<IRuntime>();

            // Just a simple timer not Started() yet, We do not want to Start() this timer
            _mainTimer = new Mock<ISystemTimerWrapper>();

            _sessionFlashesCountdown = new Mock<IStopwatch>();
            _timeSinceLastGameCountdown = new Mock<IStopwatch>();
            _insufficientCreditCountdown = new Mock<IStopwatch>();
            _timeBetweenFlashesCountdown = new Mock<IStopwatch>();
        }

        private void CreateInsufficientFundsForNextGame()
        {
            _bank.Setup(t => t.QueryBalance()).Returns(0);
        }
    }
}
/*
    [TestMethod]
    public void PrimaryGameStartedEventHandlerTest()
    {
        var systemDisableManager = new Mock<ISystemDisableManager>();
        var bank = new Mock<IBank>();
        var gameProvider = new Mock<IGameProvider>();
        var lobbyStateManager = new Mock<ILobbyStateManager>();
        var runtime = new Mock<IRuntime>();
        runtime.Setup(t => t.OnSessionTickFlashClock()).Verifiable();
        lobbyStateManager.Setup(t => t.CurrentState).Returns(LobbyState.Game);
        _lobbyClockService =
            new LobbyClockService(_eventBus.Object,
                                  _propertiesManager.Object,
                                  systemDisableManager.Object,
                                  bank.Object, gameProvider.Object,
                                  lobbyStateManager.Object,
                                  runtime.Object);
        _lobbyClockService.Initialize();
        Task.Run(() => _primaryGameStartedEventHandler.Invoke(new PrimaryGameStartedEvent(1, 2, "test", new GameHistoryLog(3))));
        Thread.Sleep(3200);
        lobbyStateManager.Setup(t => t.CurrentState).Returns(LobbyState.GameLoading);
        Thread.Sleep(1500);
        lobbyStateManager.Setup(t => t.CurrentState).Returns(LobbyState.Chooser);
        Thread.Sleep(5000);
        runtime.Verify(t => t.OnSessionTickFlashClock(), Times.Exactly(3));
        _eventBus.Verify(t => t.Publish(It.IsAny<LobbyClockFlashChangedEvent>()), Times.Exactly(2));
        //_eventBus.Verify(t => t.Publish(It.IsAny<LobbyClockFlashChangedEvent>()), Times.Exactly(5));
    }
    [TestMethod]
    public void PrimaryGameStartedEventCallbackTest()
    {
        var systemDisableManager = new Mock<ISystemDisableManager>();
        var bank = new Mock<IBank>();
        var gameProvider = new Mock<IGameProvider>();
        var runtime = new Mock<IRuntime>();
        runtime.Setup(t => t.OnSessionTickFlashClock()).Verifiable();

        _lobbyClockService =
            new LobbyClockService(_eventBus.Object,
                                  _propertiesManager.Object,
                                  systemDisableManager.Object,
                                  bank.Object, gameProvider.Object,
                                  _lobbyStateManager.Object,
                                  runtime.Object);
        _lobbyClockService.Initialize();
        var value = Task.Run(() => _primaryGameStartedEventHandler.Invoke(new PrimaryGameStartedEvent(1, 2, "test", new GameHistoryLog(3))));
        //lobbyStateManager.SetupSequence(t => t.CurrentState).Returns(queue.Dequeue()).Verifiable();
        Task.WaitAll(value);
        _lobbyStateManager.Verify(t => t.CurrentState, Times.Exactly(13));
        //runtime.Verify(t => t.OnSessionTickFlashClock(), Times.Exactly(4));
        _eventBus.Verify(t => t.Publish(It.IsAny<LobbyClockFlashChangedEvent>()), Times.Exactly(5));
        //_eventBus.Verify(t => t.Publish(It.IsAny<LobbyClockFlashChangedEvent>()), Times.Exactly(1));
        //_eventBus.Verify(t => t.Publish(It.IsAny<LobbyClockFlashChangedEvent>()), Times.Exactly(5));
    }
}
}*/