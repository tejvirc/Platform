namespace Aristocrat.Monaco.Asp.Tests.Client.DataSources
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Application.Contracts.OperatorMenu;
    using Application.Contracts.HardwareDiagnostics;
    using Asp.Client.DataSources;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using Aristocrat.Monaco.Kernel.Contracts.Events;
    using Asp.Client.Contracts;
    using Events;
    using Gaming.Diagnostics;
    using Hardware.Contracts.Persistence;

    [TestClass]
    public class CurrentMachineModeStateManagerTests
    {
        private Action<GamePlayStateChangedEvent> _gameGamePlayStateChangedCallback;
        private Action<GameDiagnosticsStartedEvent> _gameReplayStartedCallback;
        private Action<GameDiagnosticsCompletedEvent> _gameReplayCompletedCallback;
        private Action<OperatorMenuEnteredEvent> _operatorMenuEnteredCallback;
        private Action<OperatorMenuExitedEvent> _operatorMenuExitedCallback;
        private Action<HardwareDiagnosticTestStartedEvent> _hardwareDiagnosticTestStartedCallback;
        private Action<HardwareDiagnosticTestFinishedEvent> _hardwareDiagnosticTestFinishedCallback;

        private Mock<IEventBus> _eventBus;
        private Mock<ISystemDisableManager> _systemDisableManager;
        private Mock<IDisableByOperatorManager> _disableByOperatorManager;
        private Mock<IGamePlayState> _gamePlayState;
        private Mock<IGameHistory> _gamePlayHistory;

        private CurrentMachineModeStateManager _source;
        private Action<AspClientStartedEvent> _aspClientStartedEvent;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _systemDisableManager = new Mock<ISystemDisableManager>(MockBehavior.Strict);
            _eventBus = new Mock<IEventBus>(MockBehavior.Strict);
            _disableByOperatorManager = new Mock<IDisableByOperatorManager>(MockBehavior.Strict);
            _gamePlayState = new Mock<IGamePlayState>(MockBehavior.Strict);
            _gamePlayHistory = new Mock<IGameHistory>(MockBehavior.Strict);
            _gamePlayHistory.Setup(x => x.IsRecoveryNeeded).Returns(false);

            SetupEventMocks();

            _source = new CurrentMachineModeStateManager(
                _eventBus.Object,
                _gamePlayState.Object,
                _systemDisableManager.Object,
                _gamePlayHistory.Object,
                _disableByOperatorManager.Object);
        }

        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_source);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullEventBusTest()
        {
            var _ = new CurrentMachineModeStateManager(
                null,
                _gamePlayState.Object,
                _systemDisableManager.Object,
                _gamePlayHistory.Object,
                _disableByOperatorManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullSystemDisableManagerTest()
        {
            var _ = new CurrentMachineModeStateManager(
                _eventBus.Object,
                _gamePlayState.Object,
                null,
                _gamePlayHistory.Object,
                _disableByOperatorManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullGamePlayStateTest()
        {
            var _ = new CurrentMachineModeStateManager(
                _eventBus.Object,
                null,
                _systemDisableManager.Object,
                _gamePlayHistory.Object,
                _disableByOperatorManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullDisableByOperatorManagerTest()
        {
            var _ = new CurrentMachineModeStateManager(
                _eventBus.Object,
                _gamePlayState.Object,
                _systemDisableManager.Object,
                _gamePlayHistory.Object,
                null);
        }

        [TestMethod]
        public void GivenNotInOperationWhenInitializationCompletedEventFatalErrorThenFatalError()
        {
            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>()
            {
                ApplicationConstants.StorageFaultDisableKey
            });

            var memberValue = _source.GetCurrentMode();
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            var eventCount = 0;
            var eventCountExpected = 0;
            _source.MachineModeChanged += (sender, s) =>
            {
                memberValue = _source.GetCurrentMode();
                eventCount++;
            };

            _aspClientStartedEvent(new AspClientStartedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after AspClientStartedEvent");
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            _gamePlayState.Setup(x => x.InGameRound).Returns(() => false);

            _source.HandleEvent(new InitializationCompletedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after InitializationCompletedEvent");
            Assert.AreEqual(MachineMode.FatalError, memberValue);


            _operatorMenuEnteredCallback(new OperatorMenuEnteredEvent());
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged is not expected after OperatorMenuEnteredEvent");
            Assert.AreEqual(MachineMode.FatalError, memberValue);
        }

        [TestMethod]
        public void GivenNotInOperationWhenPersistentStorageIntegrityCheckFailedEventFirstInitializationCompletedEventThenFatalError()
        {
            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>()
            {
                ApplicationConstants.StorageFaultDisableKey
            });

            var memberValue = _source.GetCurrentMode();
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            var eventCount = 0;
            var eventCountExpected = 0;
            _source.MachineModeChanged += (sender, s) =>
            {
                memberValue = _source.GetCurrentMode();
                eventCount++;
            };

            _aspClientStartedEvent(new AspClientStartedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after AspClientStartedEvent");
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            _gamePlayState.Setup(x => x.InGameRound).Returns(() => false);

            _source.HandleEvent(new PersistentStorageIntegrityCheckFailedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after PersistentStorageIntegrityCheckFailedEvent");
            Assert.AreEqual(MachineMode.FatalError, memberValue);

            _source.HandleEvent(new InitializationCompletedEvent());
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged not expected after InitializationCompletedEvent");
            Assert.AreEqual(MachineMode.FatalError, memberValue);


            _operatorMenuEnteredCallback(new OperatorMenuEnteredEvent());
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged is not expected after OperatorMenuEnteredEvent");
            Assert.AreEqual(MachineMode.FatalError, memberValue);
        }


        [TestMethod]
        public void GivenNotInOperationWhenPersistentStorageIntegrityCheckFailedEventFirstNoAspClientStartedEventThenFatalError()
        {
            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>()
            {
                ApplicationConstants.StorageFaultDisableKey
            });

            var memberValue = _source.GetCurrentMode();
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            var eventCount = 0;
            var eventCountExpected = 0;
            _source.MachineModeChanged += (sender, s) =>
            {
                memberValue = _source.GetCurrentMode();
                eventCount++;
            };

            _source.HandleEvent(new PersistentStorageIntegrityCheckFailedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after PersistentStorageIntegrityCheckFailedEvent");
            Assert.AreEqual(MachineMode.FatalError, memberValue);

            _source.HandleEvent(new InitializationCompletedEvent());
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged not expected after InitializationCompletedEvent");
            Assert.AreEqual(MachineMode.FatalError, memberValue);


            _operatorMenuEnteredCallback(new OperatorMenuEnteredEvent());
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged is not expected after OperatorMenuEnteredEvent");
            Assert.AreEqual(MachineMode.FatalError, memberValue);
        }


        [TestMethod]
        public void GivenNotInOperationWhenInitializationCompletedEventOutOfServiceThenEgmOutOfService()
        {
            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>());
            _disableByOperatorManager.Setup(x => x.DisabledByOperator).Returns(true);

            var memberValue = _source.GetCurrentMode();
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            var eventCount = 0;
            var eventCountExpected = 0;
            _source.MachineModeChanged += (sender, s) =>
            {
                memberValue = _source.GetCurrentMode();
                eventCount++;
            };

            _aspClientStartedEvent(new AspClientStartedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after AspClientStartedEvent");
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            _gamePlayState.Setup(x => x.InGameRound).Returns(() => false);

            _source.HandleEvent(new InitializationCompletedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after InitializationCompletedEvent");
            Assert.AreEqual(MachineMode.EgmOutOfService, memberValue);

        }

        [TestMethod]
        public void GivenNotInOperationWhenSystemDisabledByOperatorEventFirstInitializationCompletedEventOutOfServiceThenEgmOutOfService()
        {
            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>());
            _disableByOperatorManager.Setup(x => x.DisabledByOperator).Returns(true);

            var memberValue = _source.GetCurrentMode();
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            var eventCount = 0;
            var eventCountExpected = 0;
            _source.MachineModeChanged += (sender, s) =>
            {
                memberValue = _source.GetCurrentMode();
                eventCount++;
            };

            _aspClientStartedEvent(new AspClientStartedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after AspClientStartedEvent");
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            _gamePlayState.Setup(x => x.InGameRound).Returns(() => false);

            _source.HandleEvent(new SystemDisabledByOperatorEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after SystemDisabledByOperatorEvent");
            Assert.AreEqual(MachineMode.EgmOutOfService, memberValue);

            _source.HandleEvent(new InitializationCompletedEvent());
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged not expected after InitializationCompletedEvent");
            Assert.AreEqual(MachineMode.EgmOutOfService, memberValue);

        }

        [TestMethod]
        public void GivenNotInOperationWhenSystemDisabledByOperatorEventFirstAspClientStartedEventThenEgmOutOfService()
        {
            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>());
            _disableByOperatorManager.Setup(x => x.DisabledByOperator).Returns(true);

            var memberValue = _source.GetCurrentMode();
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            var eventCount = 0;
            var eventCountExpected = 0;
            _source.MachineModeChanged += (sender, s) =>
            {
                memberValue = _source.GetCurrentMode();
                eventCount++;
            };

            _source.HandleEvent(new SystemDisabledByOperatorEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after SystemDisabledByOperatorEvent");
            Assert.AreEqual(MachineMode.EgmOutOfService, memberValue);

            _source.HandleEvent(new InitializationCompletedEvent());
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged not expected after InitializationCompletedEvent");
            Assert.AreEqual(MachineMode.EgmOutOfService, memberValue);

        }

        [TestMethod]
        public void GivenNotInOperationWhenInitializationCompletedEventInGameRoundThenGamePlayInitiated()
        {
            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>());
            _disableByOperatorManager.Setup(x => x.DisabledByOperator).Returns(false);

            _gamePlayState.Setup(x => x.InGameRound).Returns(() => true);

            var memberValue = _source.GetCurrentMode();
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            var eventCount = 0;
            var eventCountExpected = 0;
            _source.MachineModeChanged += (sender, s) =>
            {
                memberValue = _source.GetCurrentMode();
                eventCount++;
            };

            _aspClientStartedEvent(new AspClientStartedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after AspClientStartedEvent");
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);

            _source.HandleEvent(new InitializationCompletedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after InitializationCompletedEvent");
            Assert.AreEqual(MachineMode.GameInProgress, memberValue);

        }

        [TestMethod]
        public void GivenNotInOperationWhenInitializationCompletedEventRecoveryNeedThenGamePlayInitiated()
        {
            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>());
            _disableByOperatorManager.Setup(x => x.DisabledByOperator).Returns(false);

            _gamePlayState.Setup(x => x.InGameRound).Returns(() => false);
            _gamePlayHistory.Setup(x => x.IsRecoveryNeeded).Returns(() => true);

            var memberValue = _source.GetCurrentMode();
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            var eventCount = 0;
            var eventCountExpected = 0;
            _source.MachineModeChanged += (sender, s) =>
            {
                memberValue = _source.GetCurrentMode();
                eventCount++;
            };

            _aspClientStartedEvent(new AspClientStartedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after AspClientStartedEvent");
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);

            _source.HandleEvent(new InitializationCompletedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after InitializationCompletedEvent");
            Assert.AreEqual(MachineMode.GameInProgress, memberValue);

        }

        [TestMethod]
        public void RandomTest()
        {

            _systemDisableManager.SetupGet(s => s.CurrentDisableKeys).Returns(() => new List<Guid>());

            var memberValue = _source.GetCurrentMode();
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            var eventCount = 0;
            var eventCountExpected = 0;
            _source.MachineModeChanged += (sender, s) =>
            {
                memberValue = _source.GetCurrentMode();
                eventCount++;
            };

            _aspClientStartedEvent(new AspClientStartedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after AspClientStartedEvent");
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            _gamePlayState.Setup(x => x.InGameRound).Returns(() => false);
            _disableByOperatorManager.SetupGet(s => s.DisabledByOperator).Returns(() => true);

            _source.HandleEvent(new InitializationCompletedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after InitializationCompletedEvent");
            Assert.AreEqual(MachineMode.EgmOutOfService, memberValue);

            _operatorMenuEnteredCallback(new OperatorMenuEnteredEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after OperatorMenuEnteredEvent");
            Assert.AreEqual(MachineMode.AuditMode, memberValue);

            _operatorMenuExitedCallback(new OperatorMenuExitedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after OperatorMenuExitedEvent");
            Assert.AreEqual(MachineMode.EgmOutOfService, memberValue);

            _operatorMenuEnteredCallback(new OperatorMenuEnteredEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after OperatorMenuEnteredEvent");
            Assert.AreEqual(MachineMode.AuditMode, memberValue);

            _disableByOperatorManager.SetupGet(s => s.DisabledByOperator).Returns(() => false);
            _source.HandleEvent(new SystemEnabledByOperatorEvent());

            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged is not expected after SystemEnabledByOperatorEvent due to last state was AuditMode");
            Assert.AreEqual(MachineMode.AuditMode, memberValue);


            _gamePlayState.Setup(x => x.Idle).Returns(true);
            _operatorMenuExitedCallback(new OperatorMenuExitedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after OperatorMenuExitedEvent");
            Assert.AreEqual(MachineMode.Idle, memberValue);

            _operatorMenuEnteredCallback(new OperatorMenuEnteredEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after OperatorMenuEnteredEvent");
            Assert.AreEqual(MachineMode.AuditMode, memberValue);

            _disableByOperatorManager.SetupGet(s => s.DisabledByOperator).Returns(() => true);
            _source.HandleEvent(new SystemDisabledByOperatorEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after SystemDisabledByOperatorEvent");
            Assert.AreEqual(MachineMode.EgmOutOfService, memberValue);

            _hardwareDiagnosticTestStartedCallback(new HardwareDiagnosticTestStartedEvent(HardwareDiagnosticDeviceCategory.Buttons));
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after HardwareDiagnosticTestStartedEvent Buttons");
            Assert.AreEqual(MachineMode.DiagnosticTest, memberValue);

            _hardwareDiagnosticTestFinishedCallback(new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.Buttons));
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after HardwareDiagnosticTestFinishedEvent Buttons");
            Assert.AreEqual(MachineMode.EgmOutOfService, memberValue);

            _hardwareDiagnosticTestStartedCallback(new HardwareDiagnosticTestStartedEvent(HardwareDiagnosticDeviceCategory.Buttons));
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after HardwareDiagnosticTestStartedEvent Buttons");
            Assert.AreEqual(MachineMode.DiagnosticTest, memberValue);

            _disableByOperatorManager.SetupGet(s => s.DisabledByOperator).Returns(() => false);
            _hardwareDiagnosticTestFinishedCallback(new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.Displays));
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after HardwareDiagnosticTestFinishedEvent Displays");
            Assert.AreEqual(MachineMode.AuditMode, memberValue);

            _disableByOperatorManager.SetupGet(s => s.DisabledByOperator).Returns(() => true);
            _source.HandleEvent(new SystemDisabledByOperatorEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after SystemDisabledByOperatorEvent");
            Assert.AreEqual(MachineMode.EgmOutOfService, memberValue);


            _gameReplayStartedCallback(new GameDiagnosticsStartedEvent(1, 1, "a", Mock.Of<IDiagnosticContext>()));
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after GameDiagnosticsStartedEvent");
            Assert.AreEqual(MachineMode.DiagnosticTest, memberValue);

            _gameReplayCompletedCallback(new GameDiagnosticsCompletedEvent(Mock.Of<IDiagnosticContext>()));
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after GameDiagnosticsCompletedEvent");
            Assert.AreEqual(MachineMode.EgmOutOfService, memberValue);

            _gameReplayStartedCallback(new GameDiagnosticsStartedEvent(1, 1, "a", Mock.Of<IDiagnosticContext>()));
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after GameDiagnosticsStartedEvent");
            Assert.AreEqual(MachineMode.DiagnosticTest, memberValue);

            _operatorMenuExitedCallback(new OperatorMenuExitedEvent());
            eventCountExpected++; // one for GameDiagnosticsStartedEvent
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected 2 after OperatorMenuExitedEvent");
            Assert.AreEqual(MachineMode.EgmOutOfService, memberValue);

            _operatorMenuEnteredCallback(new OperatorMenuEnteredEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after OperatorMenuEnteredEvent");
            Assert.AreEqual(MachineMode.AuditMode, memberValue);

            _operatorMenuExitedCallback(new OperatorMenuExitedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after OperatorMenuExitedEvent");
            Assert.AreEqual(MachineMode.EgmOutOfService, memberValue);

            _operatorMenuEnteredCallback(new OperatorMenuEnteredEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after OperatorMenuEnteredEvent");
            Assert.AreEqual(MachineMode.AuditMode, memberValue);

            _disableByOperatorManager.SetupGet(s => s.DisabledByOperator).Returns(() => false);
            _source.HandleEvent(new SystemEnabledByOperatorEvent());
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after SystemEnabledByOperatorEvent");
            Assert.AreEqual(MachineMode.AuditMode, memberValue);


            _gameReplayStartedCallback(new GameDiagnosticsStartedEvent(1, 1, "a", new ReplayContext(Mock.Of<IGameHistoryLog>(), 1)));
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after GameDiagnosticsStartedEvent replay");
            Assert.AreEqual(MachineMode.GameReplayActive, memberValue);

            _gameReplayCompletedCallback(new GameDiagnosticsCompletedEvent(new ReplayContext(Mock.Of<IGameHistoryLog>(), 1)));
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after GameDiagnosticsCompletedEvent replay");
            Assert.AreEqual(MachineMode.AuditMode, memberValue);

            _gameReplayStartedCallback(new GameDiagnosticsStartedEvent(1, 1, "a", new ReplayContext(Mock.Of<IGameHistoryLog>(), 1)));
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after GameDiagnosticsStartedEvent replay");
            Assert.AreEqual(MachineMode.GameReplayActive, memberValue);

            _operatorMenuExitedCallback(new OperatorMenuExitedEvent());
            eventCountExpected++; // one for GameDiagnosticsStartedEvent
            eventCountExpected++; // one for OperatorMenuExitedEvent
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after OperatorMenuExitedEvent");
            Assert.AreEqual(MachineMode.Idle, memberValue);

            _gamePlayState.Setup(x => x.InGameRound).Returns(true);
            _gameGamePlayStateChangedCallback(new GamePlayStateChangedEvent(PlayState.Idle, PlayState.GameEnded));
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after GamePlayStateChangedEvent");
            Assert.AreEqual(MachineMode.GameInProgress, memberValue);

            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>()
            {
                ApplicationConstants.StorageFaultDisableKey
            });
            _source.HandleEvent(new PersistentStorageIntegrityCheckFailedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after PersistentStorageIntegrityCheckFailedEvent");
            Assert.AreEqual(MachineMode.FatalError, memberValue);

            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>());

            _gamePlayState.Setup(x => x.InGameRound).Returns(false);
            _gameGamePlayStateChangedCallback(new GamePlayStateChangedEvent(PlayState.Idle, PlayState.Idle));
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged not expected after GamePlayStateChangedEvent");
            Assert.AreEqual(MachineMode.FatalError, memberValue);

            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>()
            {
                ApplicationConstants.StorageFaultDisableKey
            });
            _source.HandleEvent(new StorageErrorEvent(new StorageError ()));
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged not expected after StorageErrorEvent");
            Assert.AreEqual(MachineMode.FatalError, memberValue);

        }


        [TestMethod]
        public void GivenOperationMenuOpenHardwareDiagnosticStartedWhenExitOperationMenuOpenThenMachineModeChangedToIdle()
        {
            _disableByOperatorManager.SetupGet(s => s.DisabledByOperator).Returns(() => false);
            _systemDisableManager.SetupGet(s => s.CurrentDisableKeys).Returns(() => new List<Guid>());

            var memberValue = _source.GetCurrentMode();
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            var eventCount = 0;
            var eventCountExpected = 0;
            _source.MachineModeChanged += (sender, s) =>
            {
                memberValue = _source.GetCurrentMode();
                eventCount++;
            };

            _aspClientStartedEvent(new AspClientStartedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after AspClientStartedEvent");
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            _gamePlayState.Setup(x => x.InGameRound).Returns(() => false);

            _source.HandleEvent(new InitializationCompletedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after InitializationCompletedEvent");
            Assert.AreEqual(MachineMode.Idle, memberValue);


            _operatorMenuEnteredCallback(new OperatorMenuEnteredEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after OperatorMenuEnteredEvent");
            Assert.AreEqual(MachineMode.AuditMode, memberValue);

            _hardwareDiagnosticTestStartedCallback(new HardwareDiagnosticTestStartedEvent(HardwareDiagnosticDeviceCategory.Printer));
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after HardwareDiagnosticTestStartedEvent");
            Assert.AreEqual(MachineMode.DiagnosticTest, memberValue);

            _hardwareDiagnosticTestStartedCallback(new HardwareDiagnosticTestStartedEvent(HardwareDiagnosticDeviceCategory.Printer));
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged is not expected after second HardwareDiagnosticTestStartedEvent");

            _gamePlayState.Setup(x => x.Idle).Returns(true);
            _operatorMenuExitedCallback(new OperatorMenuExitedEvent());
            eventCountExpected++; // one for diagnostic exit
            eventCountExpected++; // one for audit exit

            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after OperatorMenuExitedEvent");
            Assert.AreEqual(MachineMode.Idle, memberValue);

            _hardwareDiagnosticTestFinishedCallback(new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.Printer));
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged is not expected after HardwareDiagnosticTestFinishedEvent when Operator Menu Exited");
            Assert.AreEqual(MachineMode.Idle, memberValue);
        }


        [TestMethod]
        public void GivenOperationMenuOpenHardwareDiagnosticStartedWhenHardwareDiagnosticFinishedExitOperationMenuOpenThenMachineModeChangedToIdle()
        {
            _disableByOperatorManager.SetupGet(s => s.DisabledByOperator).Returns(() => false);
            _systemDisableManager.SetupGet(s => s.CurrentDisableKeys).Returns(() => new List<Guid>());

            var memberValue = _source.GetCurrentMode();
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            var eventCount = 0;
            var eventCountExpected = 0;
            _source.MachineModeChanged += (sender, s) =>
            {
                memberValue = _source.GetCurrentMode();
                eventCount++;
            };

            _aspClientStartedEvent(new AspClientStartedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after AspClientStartedEvent");
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            _gamePlayState.Setup(x => x.InGameRound).Returns(() => false);

            _source.HandleEvent(new InitializationCompletedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after InitializationCompletedEvent");
            Assert.AreEqual(MachineMode.Idle, memberValue);

            _operatorMenuEnteredCallback(new OperatorMenuEnteredEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after OperatorMenuEnteredEvent");
            Assert.AreEqual(MachineMode.AuditMode, memberValue);

            _hardwareDiagnosticTestStartedCallback(new HardwareDiagnosticTestStartedEvent(HardwareDiagnosticDeviceCategory.Printer));
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after HardwareDiagnosticTestStartedEvent");
            Assert.AreEqual(MachineMode.DiagnosticTest, memberValue);

            _hardwareDiagnosticTestFinishedCallback(new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.Printer));
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after HardwareDiagnosticTestFinishedEvent");
            Assert.AreEqual(MachineMode.AuditMode, memberValue);

            _gamePlayState.Setup(x => x.Idle).Returns(true);
            _operatorMenuExitedCallback(new OperatorMenuExitedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after OperatorMenuExitedEvent");
            Assert.AreEqual(MachineMode.Idle, memberValue);

            _gameGamePlayStateChangedCallback(new GamePlayStateChangedEvent(PlayState.Idle, PlayState.Idle));
            _gamePlayState.Setup(x => x.InGameRound).Returns(false);
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged not expected after GameIdleEvent");
            Assert.AreEqual(MachineMode.Idle, memberValue);
        }

        [TestMethod]
        public void GivenPausedGameOperationMenuOpenWhenExitOperationMenuOpenThenMachineModeChangedToGameInProgress()
        {
            _disableByOperatorManager.SetupGet(s => s.DisabledByOperator).Returns(() => false);
            _systemDisableManager.SetupGet(s => s.CurrentDisableKeys).Returns(() => new List<Guid>());

            var memberValue = _source.GetCurrentMode();
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            var eventCount = 0;
            var eventCountExpected = 0;
            _source.MachineModeChanged += (sender, s) =>
            {
                memberValue = _source.GetCurrentMode();
                eventCount++;
            };

            _aspClientStartedEvent(new AspClientStartedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after AspClientStartedEvent");
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            _gamePlayState.Setup(x => x.InGameRound).Returns(() => true);

            _source.HandleEvent(new InitializationCompletedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after InitializationCompletedEvent");
            Assert.AreEqual(MachineMode.GameInProgress, memberValue);

            _operatorMenuEnteredCallback(new OperatorMenuEnteredEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after OperatorMenuEnteredEvent");
            Assert.AreEqual(MachineMode.AuditMode, memberValue);

            _gamePlayState.Setup(x => x.Idle).Returns(false);
            _gamePlayState.Setup(x => x.InGameRound).Returns(true);
            _operatorMenuExitedCallback(new OperatorMenuExitedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after OperatorMenuExitedEvent");
            Assert.AreEqual(MachineMode.GameInProgress, memberValue);

            _gamePlayState.Setup(x => x.Idle).Returns(true);
            _gamePlayState.Setup(x => x.InGameRound).Returns(false);
            _gameGamePlayStateChangedCallback(new GamePlayStateChangedEvent(PlayState.Idle, PlayState.Idle));
            _gamePlayState.Setup(x => x.InGameRound).Returns(false);
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged not expected after GameIdleEvent");
            Assert.AreEqual(MachineMode.Idle, memberValue);
        }

        [TestMethod]
        public void GivenOperationMenuOpenGameDiagnosticStartedWhenExitOperationMenuOpenThenMachineModeChangedToIdle()
        {
            _disableByOperatorManager.SetupGet(s => s.DisabledByOperator).Returns(() => false);
            _systemDisableManager.SetupGet(s => s.CurrentDisableKeys).Returns(() => new List<Guid>());

            var memberValue = _source.GetCurrentMode();
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            var eventCount = 0;
            var eventCountExpected = 0;
            _source.MachineModeChanged += (sender, s) =>
            {
                memberValue = _source.GetCurrentMode();
                eventCount++;
            };

            _aspClientStartedEvent(new AspClientStartedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after AspClientStartedEvent");
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            _gamePlayState.Setup(x => x.InGameRound).Returns(() => false);

            _source.HandleEvent(new InitializationCompletedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after InitializationCompletedEvent");
            Assert.AreEqual(MachineMode.Idle, memberValue);


            _gameGamePlayStateChangedCallback(new GamePlayStateChangedEvent(PlayState.Idle, PlayState.Idle));
            _gamePlayState.Setup(x => x.InGameRound).Returns(false);
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged is not expected after GameIdleEvent");
            Assert.AreEqual(MachineMode.Idle, memberValue);


            _operatorMenuEnteredCallback(new OperatorMenuEnteredEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after OperatorMenuEnteredEvent");
            Assert.AreEqual(MachineMode.AuditMode, memberValue);

            _gameReplayStartedCallback(new GameDiagnosticsStartedEvent(1,1,"a", new Mock<IDiagnosticContext>().Object));
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after GameDiagnosticsStartedEvent");
            Assert.AreEqual(MachineMode.DiagnosticTest, memberValue);

            _gamePlayState.Setup(x => x.Idle).Returns(true);
            _operatorMenuExitedCallback(new OperatorMenuExitedEvent());
            eventCountExpected++; // one for the game replay started event
            eventCountExpected++; // one for exit operator menu event
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after OperatorMenuExitedEvent");
            Assert.AreEqual(MachineMode.Idle, memberValue);

            _gameReplayCompletedCallback(new GameDiagnosticsCompletedEvent(Mock.Of<IDiagnosticContext>()));
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged is not expected after GameDiagnosticsCompletedEvent when Operator Menu Exited");
            Assert.AreEqual(MachineMode.Idle, memberValue);
        }


        [TestMethod]
        public void GivenOperationMenuOpenGameDiagnosticStartedWhenGameDiagnosticFinishedExitOperationMenuOpenThenMachineModeChangedToIdle()
        {
            _disableByOperatorManager.SetupGet(s => s.DisabledByOperator).Returns(() => false);
            _systemDisableManager.SetupGet(s => s.CurrentDisableKeys).Returns(() => new List<Guid>());

            var memberValue = _source.GetCurrentMode();
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            var eventCount = 0;
            var eventCountExpected = 0;
            _source.MachineModeChanged += (sender, s) =>
            {
                memberValue = _source.GetCurrentMode();
                eventCount++;
            };

            _aspClientStartedEvent(new AspClientStartedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after AspClientStartedEvent");
            Assert.AreEqual(MachineMode.NotInOperation, memberValue);
            _gamePlayState.Setup(x => x.InGameRound).Returns(() => false);

            _source.HandleEvent(new InitializationCompletedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after InitializationCompletedEvent");
            Assert.AreEqual(MachineMode.Idle, memberValue);

            _operatorMenuEnteredCallback(new OperatorMenuEnteredEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after OperatorMenuEnteredEvent");
            Assert.AreEqual(MachineMode.AuditMode, memberValue);

            _gameReplayStartedCallback(new GameDiagnosticsStartedEvent(1,1,"", Mock.Of<IDiagnosticContext>()));
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after GameDiagnosticsStartedEvent");
            Assert.AreEqual(MachineMode.DiagnosticTest, memberValue);

            _gameReplayCompletedCallback(new GameDiagnosticsCompletedEvent(Mock.Of<IDiagnosticContext>()));
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after GameDiagnosticsCompletedEvent");
            Assert.AreEqual(MachineMode.AuditMode, memberValue);

            _gamePlayState.Setup(x => x.Idle).Returns(true);
            _operatorMenuExitedCallback(new OperatorMenuExitedEvent());
            eventCountExpected++;
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged expected after OperatorMenuExitedEvent");
            Assert.AreEqual(MachineMode.Idle, memberValue);

            _gameGamePlayStateChangedCallback(new GamePlayStateChangedEvent(PlayState.Idle, PlayState.Idle));
            _gamePlayState.Setup(x => x.InGameRound).Returns(false);
            Assert.AreEqual(eventCountExpected, eventCount, "MachineModeChanged not expected after GameIdleEvent");
            Assert.AreEqual(MachineMode.Idle, memberValue);
        }


        [TestMethod]
        public void Dispose_ShouldUnsubscribeAll()
        {
            //Call dispose twice - should only unsubscribe/deregister from events once
            _source.Dispose();
            _source.Dispose();

            _eventBus.Verify(v => v.UnsubscribeAll(It.IsAny<object>()), Times.Once);
        }

        private void SetupEventMocks()
        {
            _eventBus.Setup(s => s.UnsubscribeAll(It.IsAny<object>())).Verifiable();
            _eventBus.Setup(s => s.Unsubscribe<InitializationCompletedEvent>(It.IsAny<object>()));
            _eventBus.Setup(s => s.Unsubscribe<GamePlayStateInitializedEvent>(It.IsAny<object>()));
            _eventBus.Setup(s => s.Unsubscribe<AspClientStartedEvent>(It.IsAny<object>()));

            _eventBus
                .Setup(
                    m => m.Subscribe(
                        It.IsAny<CurrentMachineModeStateManager>(),
                        It.IsAny<Action<AspClientStartedEvent>>()))
                .Callback<object, Action<AspClientStartedEvent>>(
                    (subscriber, callback) => _aspClientStartedEvent = callback);
            
            _eventBus
                .Setup(
                    m => m.Subscribe(
                        It.IsAny<CurrentMachineModeStateManager>(),
                        It.IsAny<Action<GamePlayStateChangedEvent>>()))
                .Callback<object, Action<GamePlayStateChangedEvent>>(
                    (subscriber, callback) => _gameGamePlayStateChangedCallback = callback);

            _eventBus
                .Setup(
                    m => m.Subscribe(
                        It.IsAny<CurrentMachineModeStateManager>(),
                        It.IsAny<Action<GameDiagnosticsStartedEvent>>()))
                .Callback<object, Action<GameDiagnosticsStartedEvent>>(
                    (subscriber, callback) => _gameReplayStartedCallback = callback);

            _eventBus
                .Setup(
                    m => m.Subscribe(
                        It.IsAny<CurrentMachineModeStateManager>(),
                        It.IsAny<Action<GameDiagnosticsCompletedEvent>>()))
                .Callback<object, Action<GameDiagnosticsCompletedEvent>>(
                    (subscriber, callback) => _gameReplayCompletedCallback = callback);

            _eventBus
                .Setup(
                    m => m.Subscribe(
                        It.IsAny<CurrentMachineModeStateManager>(),
                        It.IsAny<Action<OperatorMenuEnteredEvent>>()))
                .Callback<object, Action<OperatorMenuEnteredEvent>>(
                    (subscriber, callback) => _operatorMenuEnteredCallback = callback);

            _eventBus
                .Setup(
                    m => m.Subscribe(
                        It.IsAny<CurrentMachineModeStateManager>(),
                        It.IsAny<Action<OperatorMenuExitedEvent>>()))
                .Callback<object, Action<OperatorMenuExitedEvent>>(
                    (subscriber, callback) => _operatorMenuExitedCallback = callback);

            _eventBus
                .Setup(
                    m => m.Subscribe(
                        It.IsAny<CurrentMachineModeStateManager>(),
                        It.IsAny<Action<HardwareDiagnosticTestStartedEvent>>()))
                .Callback<object, Action<HardwareDiagnosticTestStartedEvent>>(
                    (subscriber, callback) => _hardwareDiagnosticTestStartedCallback = callback);

            _eventBus
                .Setup(
                    m => m.Subscribe(
                        It.IsAny<CurrentMachineModeStateManager>(),
                        It.IsAny<Action<HardwareDiagnosticTestFinishedEvent>>()))
                .Callback<object, Action<HardwareDiagnosticTestFinishedEvent>>(
                    (subscriber, callback) => _hardwareDiagnosticTestFinishedCallback = callback);
        }
    }
}