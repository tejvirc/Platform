namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using Contracts;
    using Hardware.Contracts.KeySwitch;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Vgt.Client12.Application.OperatorMenu;

    [TestClass]
    public class GameDiagnosticsTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCreateWithNullEventBusExpectException()
        {
            var replay = new GameDiagnostics(null, null, null, null, null);

            Assert.IsNull(replay);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCreateWithNullPropertiesManagerExpectException()
        {
            var eventBus = new Mock<IEventBus>();
            var replay = new GameDiagnostics(eventBus.Object, null, null, null, null);

            Assert.IsNull(replay);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCreateWithNullOperatorMenuLauncherExpectException()
        {
            var eventBus = new Mock<IEventBus>();
            var properties = new Mock<IPropertiesManager>();
            var replay = new GameDiagnostics(eventBus.Object, properties.Object, null, null, null);

            Assert.IsNull(replay);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCreateWithNullButtonDeckFilterExpectException()
        {
            var eventBus = new Mock<IEventBus>();
            var properties = new Mock<IPropertiesManager>();
            var operatorMenu = new Mock<IOperatorMenuLauncher>();
            var replay = new GameDiagnostics(eventBus.Object, properties.Object, operatorMenu.Object, null, null);

            Assert.IsNull(replay);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var eventBus = new Mock<IEventBus>();
            var properties = new Mock<IPropertiesManager>();
            var operatorMenu = new Mock<IOperatorMenuLauncher>();
            var buttonDeck = new Mock<IButtonDeckFilter>();
            var gameService = new Mock<IGameService>();

            var replay = new GameDiagnostics(eventBus.Object, properties.Object, operatorMenu.Object, buttonDeck.Object, gameService.Object);

            Assert.IsNotNull(replay);
        }

        [TestMethod]
        public void StartReplay_WhenGameNotRunningExpectGameReplayStartedEvent()
        {
            var eventBus = new Mock<IEventBus>();
            var properties = new Mock<IPropertiesManager>();
            var operatorMenu = new Mock<IOperatorMenuLauncher>();
            var buttonDeck = new Mock<IButtonDeckFilter>();
            var parameters = new Mock<IDiagnosticContext>();
            var gameService = new Mock<IGameService>();

            properties.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, false)).Returns(false);
            properties.Setup(m => m.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);
            var replay = new GameDiagnostics(eventBus.Object, properties.Object, operatorMenu.Object, buttonDeck.Object, gameService.Object);
            replay.Start(1, 1, string.Empty, parameters.Object);

            eventBus.Verify(b => b.Publish(It.IsAny<GameDiagnosticsStartedEvent>()), Times.Once);
        }

        [TestMethod]
        public void StartReplay_WhenGameRunningExpectGameRequestExit()
        {
            var eventBus = new Mock<IEventBus>();
            var properties = new Mock<IPropertiesManager>();
            var operatorMenu = new Mock<IOperatorMenuLauncher>();
            var buttonDeck = new Mock<IButtonDeckFilter>();
            var parameters = new Mock<IDiagnosticContext>();
            var gameService = new Mock<IGameService>();

            properties.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, false)).Returns(true);
            properties.Setup(m => m.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);

            var replay = new GameDiagnostics(eventBus.Object, properties.Object, operatorMenu.Object, buttonDeck.Object, gameService.Object);
            replay.Start(1, 1, string.Empty, parameters.Object);

            gameService.Verify(b => b.ShutdownBegin(), Times.Once);
        }

        [TestMethod]
        public void StartReplay_WhenGameRunningExpectGameReplayStartedEvent()
        {
            var eventBus = new Mock<IEventBus>();
            var properties = new Mock<IPropertiesManager>();
            var operatorMenu = new Mock<IOperatorMenuLauncher>();
            var buttonDeck = new Mock<IButtonDeckFilter>();
            var parameters = new Mock<IDiagnosticContext>();
            var gameService = new Mock<IGameService>();

            properties.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, false)).Returns(true);
            properties.Setup(m => m.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);

            Action<GameProcessExitedEvent> handler = null;

            eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<GameProcessExitedEvent>>()))
                .Callback<object, Action<GameProcessExitedEvent>>((o, h) => handler = h);

            var replay = new GameDiagnostics(eventBus.Object, properties.Object, operatorMenu.Object, buttonDeck.Object, gameService.Object);
            replay.Start(1, 1, string.Empty, parameters.Object);
            handler.Invoke(new GameProcessExitedEvent(1, false));

            eventBus.Verify(b => b.Publish(It.IsAny<GameDiagnosticsStartedEvent>()), Times.Once);
        }

        [TestMethod]
        public void EndReplay_ExpectGameRequestExitEvent()
        {
            var eventBus = new Mock<IEventBus>();
            var properties = new Mock<IPropertiesManager>();
            var operatorMenu = new Mock<IOperatorMenuLauncher>();
            var buttonDeck = new Mock<IButtonDeckFilter>();
            var parameters = new Mock<IDiagnosticContext>();
            var gameService = new Mock<IGameService>();

            properties.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, false)).Returns(false);
            properties.Setup(m => m.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);
            var replay = new GameDiagnostics(eventBus.Object, properties.Object, operatorMenu.Object, buttonDeck.Object, gameService.Object);
            replay.Start(1, 1, string.Empty, parameters.Object);
            replay.End();

            gameService.Verify(b => b.ShutdownBegin(), Times.Once);
        }

        [TestMethod]
        public void EndReplay_ExpectIsReplayingFalse()
        {
            var eventBus = new Mock<IEventBus>();
            var properties = new Mock<IPropertiesManager>();
            var operatorMenu = new Mock<IOperatorMenuLauncher>();
            var buttonDeck = new Mock<IButtonDeckFilter>();
            var parameters = new Mock<IDiagnosticContext>();
            var gameService = new Mock<IGameService>();

            properties.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, false)).Returns(false);
            properties.Setup(m => m.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);
            Action<GameProcessExitedEvent> handler = null;

            eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<GameProcessExitedEvent>>()))
                .Callback<object, Action<GameProcessExitedEvent>>((o, h) => handler = h);

            var replay = new GameDiagnostics(eventBus.Object, properties.Object, operatorMenu.Object, buttonDeck.Object, gameService.Object);
            replay.Start(1, 1, string.Empty, parameters.Object);
            replay.End();

            handler.Invoke(new GameProcessExitedEvent(1, false));

            Assert.IsFalse(replay.IsActive);
        }

        [TestMethod]
        public void EndReplay_WhenGameWasRunningExpectRelaunch()
        {
            var eventBus = new Mock<IEventBus>();
            var properties = new Mock<IPropertiesManager>();
            var operatorMenu = new Mock<IOperatorMenuLauncher>();
            var buttonDeck = new Mock<IButtonDeckFilter>();
            var parameters = new Mock<IDiagnosticContext>();
            var gameService = new Mock<IGameService>();

            properties.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, false)).Returns(true);
            properties.Setup(m => m.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);

            var replay = new GameDiagnostics(eventBus.Object, properties.Object, operatorMenu.Object, buttonDeck.Object, gameService.Object);
            replay.Start(1, 1, string.Empty, parameters.Object);
            replay.End();

            // Once for initial game exit, again for replay exit.
            gameService.Verify(b => b.ShutdownBegin(), Times.Exactly(2));
            Assert.IsTrue(replay.RelaunchGameId != 0);
        }

        [TestMethod]
        public void WhenGameWasNotRunningAndOperatorMenuExitDuringReplayExpectEndReplay()
        {
            var eventBus = new Mock<IEventBus>();
            var properties = new Mock<IPropertiesManager>();
            var operatorMenu = new Mock<IOperatorMenuLauncher>();
            var buttonDeck = new Mock<IButtonDeckFilter>();
            var parameters = new Mock<IDiagnosticContext>();
            var gameService = new Mock<IGameService>();

            properties.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, false)).Returns(false);
            properties.Setup(m => m.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);
            operatorMenu.Setup(m => m.IsOperatorKeyDisabled).Returns(true);

            Action<GameProcessExitedEvent> exitedHandler = null;
            eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<GameProcessExitedEvent>>()))
                .Callback<object, Action<GameProcessExitedEvent>>((o, h) => exitedHandler = h);

            Action<OnEvent> keyHandler = null;
            eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<OnEvent>>()))
                .Callback<object, Action<OnEvent>>((o, h) => keyHandler = h);

            var replay = new GameDiagnostics(eventBus.Object, properties.Object, operatorMenu.Object, buttonDeck.Object, gameService.Object);
            replay.Start(1, 1, string.Empty, parameters.Object);

            replay.End();

            exitedHandler.Invoke(new GameProcessExitedEvent(1, false));

            Assert.IsFalse(replay.IsActive);
        }

        [TestMethod]
        public void WhenGameWasRunningAndOperatorMenuExitDuringReplayExpectEndReplayAndRelaunch()
        {
            var eventBus = new Mock<IEventBus>();
            var properties = new Mock<IPropertiesManager>();
            var operatorMenu = new Mock<IOperatorMenuLauncher>();
            var buttonDeck = new Mock<IButtonDeckFilter>();
            var parameters = new Mock<IDiagnosticContext>();
            var gameService = new Mock<IGameService>();

            properties.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, false)).Returns(true);
            properties.Setup(m => m.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);
            operatorMenu.Setup(m => m.IsOperatorKeyDisabled).Returns(true);

            Action<GameProcessExitedEvent> exitedHandler = null;
            eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<GameProcessExitedEvent>>()))
                .Callback<object, Action<GameProcessExitedEvent>>((o, h) => exitedHandler = h);

            Action<OnEvent> keyHandler = null;
            eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<OnEvent>>()))
                .Callback<object, Action<OnEvent>>((o, h) => keyHandler = h);

            var replay = new GameDiagnostics(eventBus.Object, properties.Object, operatorMenu.Object, buttonDeck.Object, gameService.Object);
            replay.Start(1, 1, string.Empty, parameters.Object);

            // Handle prior game exit.
            exitedHandler.Invoke(new GameProcessExitedEvent(1, false));

            replay.End();

            // Handle replay game exit.
            exitedHandler.Invoke(new GameProcessExitedEvent(1, false));
            Assert.IsFalse(replay.IsActive);
            Assert.IsTrue(replay.RelaunchGameId != 0);
        }
    }
}