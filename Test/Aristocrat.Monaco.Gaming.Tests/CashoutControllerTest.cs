namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using Aristocrat.Monaco.Application.Contracts;
    using Contracts;
    using Gaming.Runtime;
    using Gaming.Runtime.Client;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CashoutControllerTest
    {
        private Mock<IResponsibleGaming> _responsibleGaming;
        private Mock<IEventBus> _eventBusMock;
        private Mock<IGamePlayState> _gamePlayState;
        private Mock<IGameHistory> _gameHistory;
        private Mock<IRuntime> _runtime;
        private Mock<IPropertiesManager> _properties;
        private Action<HardwareWarningEvent> _hardwareWarningAction;
        private Action<HardwareWarningClearEvent> _hardwareWarningClearedAction;
        private Action<MissedStartupEvent> _missedStartupEventAction;
        private Action<SystemDisabledEvent> _disabledAction;
        private Action<SystemEnabledEvent> _enabledAction;
        private Action<GameIdleEvent> _idleAction;
        private Action<PrimaryGameStartedEvent> _gameStartedAction;
        private Action<SystemDownEvent> _downAction;
        private CashoutController _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _responsibleGaming = new Mock<IResponsibleGaming>(MockBehavior.Default);
            _eventBusMock = new Mock<IEventBus>(MockBehavior.Default);
            _gamePlayState = new Mock<IGamePlayState>(MockBehavior.Default);
            _gameHistory = new Mock<IGameHistory>(MockBehavior.Default);
            _runtime = new Mock<IRuntime>(MockBehavior.Default);
            _properties = new Mock<IPropertiesManager>(MockBehavior.Default);
            _target = CreateController();
            InitializeController();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
        }

        [DataRow(true, false, false, false, false, false)]
        [DataRow(false, true, false, false, false, false)]
        [DataRow(false, false, true, false, false, false)]
        [DataRow(false, false, false, true, false, false)]
        [DataRow(false, false, false, false, true, false)]
        [DataRow(false, false, false, false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorTest(
            bool nullRg,
            bool nullEventBus,
            bool nullGamePlay,
            bool nullGameHistory,
            bool nullRuntime,
            bool nullProperties)
        {
            _target = CreateController(nullRg, nullEventBus, nullGamePlay, nullGameHistory, nullRuntime, nullProperties);
        }

        [DataRow(PrinterWarningTypes.PaperInChute, true, false, true)]
        [DataRow(PrinterWarningTypes.PaperInChute, false, false, false)]
        [DataRow(PrinterWarningTypes.PaperInChute, true, true, false)]
        [DataRow(PrinterWarningTypes.PaperLow, true, false, false)]
        [DataTestMethod]
        public void WhenInPaperInChuteWarningShouldRaiseNotifyEvent(
            PrinterWarningTypes warning,
            bool gameIdle,
            bool recoveryNeeded,
            bool eventPosted)
        {
            _gamePlayState.Setup(x => x.Idle).Returns(gameIdle);
            _gameHistory.Setup(x => x.IsRecoveryNeeded).Returns(recoveryNeeded);
            _hardwareWarningAction.Invoke(new HardwareWarningEvent(warning));
            _eventBusMock.Verify(
                bus => bus.Publish(It.Is<CashoutNotificationEvent>(ev => ev.PaperIsInChute)),
                eventPosted ? Times.Once() : Times.Never());
        }

        [DataRow(PrinterWarningTypes.PaperInChute, true, true, true, true, true)]
        [DataRow(PrinterWarningTypes.PaperInChute, false, true, true, false, false)]
        [DataRow(PrinterWarningTypes.PaperInChute, true, false, true, true, false)]
        [DataRow(PrinterWarningTypes.PaperInChute, true, true, false, true, false)]
        [DataRow(PrinterWarningTypes.PaperLow, true, true, true, false, false)]
        [DataTestMethod]
        public void WhenPaperInChuteIsClearedShouldRaiseNotifyEvent(
            PrinterWarningTypes warning,
            bool notificationActive,
            bool gameIdle,
            bool runtimeConnected,
            bool eventPosted,
            bool gameRoundAllow)
        {
            _gamePlayState.Setup(x => x.Idle).Returns(gameIdle);
            _target.PaperInChuteNotificationActive = notificationActive;
            _target.PaperIsInChute = true;
            _runtime.Setup(x => x.Connected).Returns(runtimeConnected);
            _hardwareWarningClearedAction.Invoke(new HardwareWarningClearEvent(warning));
            _eventBusMock.Verify(
                bus => bus.Publish(It.Is<CashoutNotificationEvent>(evt => !evt.PaperIsInChute)),
                eventPosted ? Times.Once() : Times.Never());
            _runtime.Verify(
                x => x.UpdateFlag(RuntimeCondition.AllowGameRound, true),
                gameRoundAllow ? Times.Once() : Times.Never());
        }

        [DataRow(true, true, true, true, true)]
        [DataRow(false, true, true, false, false)]
        [DataRow(true, false, true, true, false)]
        [DataRow(true, true, false, true, false)]
        [DataTestMethod]
        public void OnButtonPressed(
            bool notificationActive,
            bool gameIdle,
            bool runtimeConnected,
            bool eventPosted,
            bool gameRoundAllow)
        {
            _gamePlayState.Setup(x => x.Idle).Returns(gameIdle);
            _target.PaperInChuteNotificationActive = notificationActive;
            _target.PaperIsInChute = true;
            _runtime.Setup(x => x.Connected).Returns(runtimeConnected);
            _downAction.Invoke(new SystemDownEvent());
            _eventBusMock.Verify(
                bus => bus.Publish(It.Is<CashoutNotificationEvent>(evt => !evt.PaperIsInChute)),
                eventPosted ? Times.Once() : Times.Never());
            _runtime.Verify(
                x => x.UpdateFlag(RuntimeCondition.AllowGameRound, true),
                gameRoundAllow ? Times.Once() : Times.Never());
        }

        [DataRow(PrinterWarningTypes.PaperInChute, true, false, true)]
        [DataRow(PrinterWarningTypes.PaperInChute, false, false, false)]
        [DataRow(PrinterWarningTypes.PaperInChute, true, true, false)]
        [DataRow(PrinterWarningTypes.PaperLow, true, false, false)]
        [DataTestMethod]
        public void WhenMissingStartupEventIsRaisedWithWarningShouldRaiseNotifyEvent(
            PrinterWarningTypes warning,
            bool gameIdle,
            bool recoveryNeeded,
            bool eventPosted)
        {
            _gamePlayState.Setup(x => x.Idle).Returns(gameIdle);
            _gameHistory.Setup(x => x.IsRecoveryNeeded).Returns(recoveryNeeded);
            _missedStartupEventAction.Invoke(new MissedStartupEvent(new HardwareWarningEvent(warning)));
            _eventBusMock.Verify(
                bus => bus.Publish(It.Is<CashoutNotificationEvent>(ev => ev.PaperIsInChute)),
                eventPosted ? Times.Once() : Times.Never());
        }

        [DataRow(true, true)]
        [DataRow(true, false)]
        [DataRow(false, true)]
        [DataRow(false, false)]
        [DataTestMethod]
        public void WhenGameRequestCashoutRaisedWithPaperInChuteShouldNotRaiseCashOutButtonEvent(bool paperInChute, bool paperInChuteBlocksCashout)
        {
            _gamePlayState.Setup(x => x.Idle).Returns(true);
            _gameHistory.Setup(x => x.IsRecoveryNeeded).Returns(false);
            _properties.Setup(p => p.GetProperty(ApplicationConstants.PaperInChuteBlocksCashout, true)).Returns(paperInChuteBlocksCashout);

            _target.PaperIsInChute = paperInChute;
            _target.GameRequestedCashout();

            if (paperInChuteBlocksCashout)
            {
                // If PaperInChute, publish only CashOutNotificationEvent
                // If No PaperInChute, publish only CashOutButtonPressedEvent
                _eventBusMock.Verify(
                    bus => bus.Publish(It.Is<CashOutButtonPressedEvent>(_ => true)),
                    paperInChute ? Times.Never() : Times.Once());
                _eventBusMock.Verify(
                    bus => bus.Publish(It.Is<CashoutNotificationEvent>(_ => true)),
                    paperInChute ? Times.Once() : Times.Never());
            }
            else
            {
                // Publish only CashOutButtonPressedEvent no matter if PaperInChute
                _eventBusMock.Verify(
                    bus => bus.Publish(It.Is<CashOutButtonPressedEvent>(_ => true)),
                    Times.Once());
                _eventBusMock.Verify(
                    bus => bus.Publish(It.Is<CashoutNotificationEvent>(_ => true)),
                    Times.Never());
            }
        }

        [DataRow(true)]
        [DataRow(false)]
        [DataTestMethod]
        public void NotificationOnDisabledTest(bool notificationActive)
        {
            _target.PaperInChuteNotificationActive = notificationActive;
            _target.PaperIsInChute = true;
            _disabledAction.Invoke(new SystemDisabledEvent());
            _eventBusMock.Verify(
                bus => bus.Publish(It.Is<CashoutNotificationEvent>(evt => !evt.PaperIsInChute)),
                notificationActive ? Times.Once() : Times.Never());
        }

        [DataRow(true, true, false, true)]
        [DataRow(false, true, false, false)]
        [DataRow(true, false, false, false)]
        [DataRow(true, true, true, false)]
        [DataTestMethod]
        public void NotificationOnEnabledTest(bool paperInChute, bool gameIdle, bool recoveryNeeded, bool eventPosted)
        {
            _target.PaperIsInChute = paperInChute;
            _gamePlayState.Setup(x => x.Idle).Returns(gameIdle);
            _gameHistory.Setup(x => x.IsRecoveryNeeded).Returns(recoveryNeeded);
            _enabledAction.Invoke(new SystemEnabledEvent());
            _eventBusMock.Verify(
                bus => bus.Publish(It.Is<CashoutNotificationEvent>(evt => evt.PaperIsInChute)),
                eventPosted ? Times.Once() : Times.Never());
        }

        [DataRow(true, true, false, true)]
        [DataRow(false, true, false, false)]
        [DataRow(true, false, false, false)]
        [DataRow(true, true, true, false)]
        [DataTestMethod]
        public void NotificationOnIdleTest(bool paperInChute, bool gameIdle, bool recoveryNeeded, bool eventPosted)
        {
            _target.PaperIsInChute = paperInChute;
            _gamePlayState.Setup(x => x.Idle).Returns(gameIdle);
            _gameHistory.Setup(x => x.IsRecoveryNeeded).Returns(recoveryNeeded);
            _idleAction.Invoke(new GameIdleEvent(0, 0, string.Empty, new GameHistoryLog(0)));
            _eventBusMock.Verify(
                bus => bus.Publish(It.Is<CashoutNotificationEvent>(evt => evt.PaperIsInChute)),
                eventPosted ? Times.Once() : Times.Never());
        }

        [TestMethod]
        public void NotificationOnGameStartedTest()
        {
            _target.PaperIsInChute = false;
            _gamePlayState.Setup(x => x.Idle).Returns(true);
            _gameHistory.Setup(x => x.IsRecoveryNeeded).Returns(false);
            _gameStartedAction.Invoke(new PrimaryGameStartedEvent(0, 0, string.Empty, new GameHistoryLog(0)));
            _eventBusMock.Verify(
                bus => bus.Publish(It.Is<CashoutNotificationEvent>(evt => evt.PaperIsInChute)),
                Times.Never());
        }

        private CashoutController CreateController(
            bool nullRg = false,
            bool nullEventBus = false,
            bool nullGamePlay = false,
            bool nullGameHistory = false,
            bool nullRuntime = false,
            bool nullProperties = false)
        {
            return new CashoutController(
                nullRg ? null : _responsibleGaming.Object,
                nullEventBus ? null : _eventBusMock.Object,
                nullGamePlay ? null : _gamePlayState.Object,
                nullGameHistory ? null : _gameHistory.Object,
                nullRuntime ? null : _runtime.Object,
                nullProperties ? null : _properties.Object);
        }

        private void InitializeController()
        {
            _eventBusMock.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<HardwareWarningEvent>>()))
                .Callback<object, Action<HardwareWarningEvent>>((o, a) => _hardwareWarningAction = a);
            _eventBusMock.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<HardwareWarningClearEvent>>()))
                .Callback<object, Action<HardwareWarningClearEvent>>((o, a) => _hardwareWarningClearedAction = a);
            _eventBusMock.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<MissedStartupEvent>>()))
                .Callback<object, Action<MissedStartupEvent>>((o, a) => _missedStartupEventAction = a);
            _eventBusMock.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<SystemDisabledEvent>>()))
                .Callback<object, Action<SystemDisabledEvent>>((o, a) => _disabledAction = a);
            _eventBusMock.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<SystemEnabledEvent>>()))
                .Callback<object, Action<SystemEnabledEvent>>((o, a) => _enabledAction = a);
            _eventBusMock.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<GameIdleEvent>>()))
                .Callback<object, Action<GameIdleEvent>>((o, a) => _idleAction = a);
            _eventBusMock.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<PrimaryGameStartedEvent>>()))
                .Callback<object, Action<PrimaryGameStartedEvent>>((o, a) => _gameStartedAction = a);
            _eventBusMock.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<SystemDownEvent>>()))
                .Callback<object, Action<SystemDownEvent>>((o, a) => _downAction = a);

            _properties.Setup(p => p.GetProperty(ApplicationConstants.PaperInChuteBlocksCashout, true)).Returns(true);

            _target.Initialize();
        }
    }
}