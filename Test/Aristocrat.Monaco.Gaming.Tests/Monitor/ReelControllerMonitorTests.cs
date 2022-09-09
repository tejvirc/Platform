namespace Aristocrat.Monaco.Gaming.Tests.Monitor
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.OperatorMenu;
    using Common;
    using Contracts;
    using Gaming.Monitor;
    using Hardware.Contracts;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.EdgeLighting;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Kernel.Contracts.Events;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using Vgt.Client12.Application.OperatorMenu;
    using DisabledEvent = Hardware.Contracts.Reel.DisabledEvent;
    using EnabledEvent = Hardware.Contracts.Reel.EnabledEvent;

    [TestClass]
    public class ReelControllerMonitorTests
    {
        private const int WaitTIme = 1000;

        private ReelControllerMonitor _target;
        private Mock<IEventBus> _eventBus;
        private Mock<IReelController> _reelController;
        private Mock<IMessageDisplay> _messageDisplay;
        private Mock<ISystemDisableManager> _disable;
        private Mock<IGameProvider> _gameProvider;
        private Mock<IGamePlayState> _gamePlayState;
        private Mock<IEdgeLightingController> _edgeLightingController;
        private Mock<IGameService> _gameService;
        private Mock<IOperatorMenuLauncher> _operatorMenuLauncher;

        private Func<ClosedEvent, CancellationToken, Task> _doorClosedAction;
        private Func<ConnectedEvent, CancellationToken, Task> _connectedAction;
        private Func<DisconnectedEvent, CancellationToken, Task> _disconnectedAction;
        private Action<DisabledEvent> _disabledAction;
        private Action<EnabledEvent> _enabledAction;
        private Func<HardwareFaultClearEvent, CancellationToken, Task> _reelControllerClearAction;
        private Func<HardwareFaultEvent, CancellationToken, Task> _reelControllerFaultedAction;
        private Func<HardwareReelFaultClearEvent, CancellationToken, Task> _clearAction;
        private Func<HardwareReelFaultEvent, CancellationToken, Task> _faultedAction;
        private Func<InitializationCompletedEvent, CancellationToken, Task> _initCompleteAction;
        private Func<InspectedEvent, CancellationToken, Task> _inspectedAction;
        private Func<InspectionFailedEvent, CancellationToken, Task> _inspectionFailedAction;
        private Action<OperatorMenuEnteredEvent> _operatorMenuEnteredAction;
        private Func<OperatorMenuExitedEvent, CancellationToken, Task> _operatorMenuExitedAction;
        private Func<SystemDisableAddedEvent, CancellationToken, Task> _systemDisabledAction;
        private Func<SystemDisableRemovedEvent, CancellationToken, Task> _systemDisableRemovedAction;
        private Action<ReelStoppedEvent> _reelStoppedEventAction;
        private Action<GameAddedEvent> _gameAddedEventAction;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _eventBus = new Mock<IEventBus>(MockBehavior.Default);
            _reelController = MoqServiceManager.CreateAndAddService<IReelController>(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Strict);
            MockLocalization.Localizer.Setup(m => m.GetString(It.IsAny<string>(), It.IsAny<Action<Exception>>())).Returns(string.Empty);
            _messageDisplay = new Mock<IMessageDisplay>(MockBehavior.Default);
            _disable = new Mock<ISystemDisableManager>(MockBehavior.Default);
            _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);
            _gameService = new Mock<IGameService>(MockBehavior.Default);
            _operatorMenuLauncher = new Mock<IOperatorMenuLauncher>(MockBehavior.Default);
            _gamePlayState = new Mock<IGamePlayState>(MockBehavior.Default);
            _edgeLightingController = new Mock<IEdgeLightingController>(MockBehavior.Default);

            var games = new List<IGameDetail>
            {
                new GameDetail()
                {
                    Id = 100, MechanicalReels = 3, MechanicalReelHomeSteps = new int[] { 118, 56, 132 }
                }
            };

            _gameProvider.Setup(x => x.GetGames()).Returns(games);
            _gameProvider.Setup(x => x.GetMinimumNumberOfMechanicalReels()).Returns(3);
            _gamePlayState.Setup(x => x.Enabled).Returns(true);

            _gameService.Setup(x => x.Running).Returns(true);
            _gameService.Setup(x => x.ShutdownBegin()).Verifiable();

            _operatorMenuLauncher.Setup(x => x.IsShowing).Returns(false);

            _reelController.Setup(x => x.Enabled).Returns(true);
            _reelController.Setup(x => x.Connected).Returns(false);
            _reelController.Setup(x => x.ConnectedReels).Returns(new List<int>() { 1, 2, 3 });
            _reelController.Setup(x => x.ReelControllerFaults).Returns(ReelControllerFaults.None);
            _reelController.Setup(x => x.Faults).Returns(new Dictionary<int, ReelFaults>() { { 1, ReelFaults.None } });
            _reelController.Setup(x => x.LogicalState).Returns(ReelControllerState.Uninitialized);
            _reelController.Setup(x => x.HomeReels()).Returns(Task.FromResult(true));
            IList<int> ids = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            _reelController.Setup(x => x.GetReelLightIdentifiers()).Returns(Task.FromResult(ids));
            _reelController.Setup(x => x.SetLights(It.IsAny<ReelLampData[]>())).Returns(Task.FromResult(true));
            _reelController.Setup(x => x.TiltReels()).Returns(Task.FromResult(true));

            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Func<InitializationCompletedEvent, CancellationToken, Task>>()))
                .Callback<object, Func<InitializationCompletedEvent, CancellationToken, Task>>((o, c) => _initCompleteAction = c);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Func<HardwareReelFaultEvent, CancellationToken, Task>>()))
                .Callback<object, Func<HardwareReelFaultEvent, CancellationToken, Task>>((o, c) => _faultedAction = c);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Func<HardwareReelFaultClearEvent, CancellationToken, Task>>()))
                .Callback<object, Func<HardwareReelFaultClearEvent, CancellationToken, Task>>((o, c) => _clearAction = c);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Func<HardwareFaultEvent, CancellationToken, Task>>()))
                .Callback<object, Func<HardwareFaultEvent, CancellationToken, Task>>((o, c) => _reelControllerFaultedAction = c);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Func<HardwareFaultClearEvent, CancellationToken, Task>>()))
                .Callback<object, Func<HardwareFaultClearEvent, CancellationToken, Task>>((o, c) => _reelControllerClearAction = c);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Func<SystemDisableAddedEvent, CancellationToken, Task>>()))
                .Callback<object, Func<SystemDisableAddedEvent, CancellationToken, Task>>((o, c) => _systemDisabledAction = c);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Func<SystemDisableRemovedEvent, CancellationToken, Task>>()))
                .Callback<object, Func<SystemDisableRemovedEvent, CancellationToken, Task>>((o, c) => _systemDisableRemovedAction = c);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Func<ConnectedEvent, CancellationToken, Task>>()))
                .Callback<object, Func<ConnectedEvent, CancellationToken, Task>>((o, c) => _connectedAction = c);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Func<DisconnectedEvent, CancellationToken, Task>>()))
                .Callback<object, Func<DisconnectedEvent, CancellationToken, Task>>((o, c) => _disconnectedAction = c);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<EnabledEvent>>()))
                .Callback<object, Action<EnabledEvent>>((o, c) => _enabledAction = c);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<DisabledEvent>>()))
                .Callback<object, Action<DisabledEvent>>((o, c) => _disabledAction = c);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Func<InspectionFailedEvent, CancellationToken, Task>>()))
                .Callback<object, Func<InspectionFailedEvent, CancellationToken, Task>>((o, c) => _inspectionFailedAction = c);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Func<InspectedEvent, CancellationToken, Task>>()))
                .Callback<object, Func<InspectedEvent, CancellationToken, Task>>((o, c) => _inspectedAction = c);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<OperatorMenuEnteredEvent>>()))
                .Callback<object, Action<OperatorMenuEnteredEvent>>((o, c) => _operatorMenuEnteredAction = c);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Func<OperatorMenuExitedEvent, CancellationToken, Task>>()))
                .Callback<object, Func<OperatorMenuExitedEvent, CancellationToken, Task>>((o, c) => _operatorMenuExitedAction = c);
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Func<ClosedEvent, CancellationToken, Task>>(), It.IsAny<Predicate<ClosedEvent>>()))
                .Callback<object, Func<ClosedEvent, CancellationToken, Task>, Predicate<ClosedEvent>>((_, c, _) => _doorClosedAction = c);
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<ReelStoppedEvent>>()))
                .Callback<object, Action<ReelStoppedEvent>>((o, c) => _reelStoppedEventAction = c);
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<GameAddedEvent>>()))
                .Callback<object, Action<GameAddedEvent>>((o, c) => _gameAddedEventAction = c);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataRow(true, false, false, false, false, false, false, DisplayName = "null Event bus")]
        [DataRow(false, true, false, false, false, false, false, DisplayName = "null Message")]
        [DataRow(false, false, true, false, false, false, false, DisplayName = "null Disable manager")]
        [DataRow(false, false, false, true, false, false, false, DisplayName = "null Game play")]
        [DataRow(false, false, false, false, true, false, false, DisplayName = "null GameProvider")]
        [DataRow(false, false, false, false, false, true, false, DisplayName = "null EdgeLightController")]
        [DataRow(false, false, false, false, false, false, true, DisplayName = "null GameService")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorArguments(
            bool nullEvent,
            bool nullMessage,
            bool nullDisable,
            bool nullGamePlay,
            bool nullGameProvider,
            bool nullEdgeLightController,
            bool nullGameService)
        {
            _target = CreateTarget(
                nullEvent,
                nullMessage,
                nullDisable,
                nullGamePlay,
                nullGameProvider,
                nullEdgeLightController,
                nullGameService);
        }

        [TestMethod]
        public async Task ExitOperatorMenuWithFaultsTest()
        {
            InitializeClient(true);
            _disable.Reset();
            _reelController.Reset();
            _reelController.Setup(x => x.LogicalState).Returns(ReelControllerState.IdleAtStops);
            _reelController.Setup(x => x.TiltReels()).Returns(Task.FromResult(true));

            var disableKeys = new List<Guid> { ReelFaults.Disconnected.GetAttribute<ErrorGuidAttribute>().Id };
            _disable.Setup(x => x.CurrentDisableKeys).Returns(disableKeys);

            Assert.IsNotNull(_operatorMenuExitedAction);
            await _operatorMenuExitedAction(new OperatorMenuExitedEvent(), CancellationToken.None);

            _reelController.Verify();
            _reelController.Verify(x => x.TiltReels(), Times.Once());
        }

        [TestMethod]
        public async Task ExitOperatorMenuWithoutFaultsTest()
        {
            InitializeClient(true);
            _disable.Reset();
            _reelController.Reset();
            _reelController.Setup(x => x.LogicalState).Returns(ReelControllerState.IdleAtStops);
            _reelController.Setup(x => x.TiltReels()).Returns(Task.FromResult(true));

            var disableKeys = new List<Guid>();
            _disable.Setup(x => x.CurrentDisableKeys).Returns(disableKeys);

            Assert.IsNotNull(_operatorMenuExitedAction);
            await _operatorMenuExitedAction(new OperatorMenuExitedEvent(), CancellationToken.None);

            _reelController.Verify();
            _reelController.Verify(x => x.TiltReels(), Times.Never());
        }

        [TestMethod]
        public void HandleReelStoppedEvent_AllStoppedTest()
        {
            InitializeClient(true);
            _disable.Reset();
            _reelController.Reset();

            _reelController.Setup(x => x.GetReelLightIdentifiers())
                .Returns(Task.FromResult<IList<int>>(new List<int>()));
            _reelController.Setup(x => x.LogicalState).Returns(ReelControllerState.IdleUnknown);
            var connectedReels = new List<int>() { 1 };
            _reelController.Setup(x => x.ConnectedReels).Returns(connectedReels);
            var reelState = new Dictionary<int, ReelLogicalState>();
            reelState.Add(1, ReelLogicalState.IdleAtStop);
            _reelController.Setup(x => x.ReelStates).Returns(reelState);

            Assert.IsNotNull(_reelStoppedEventAction);
            _reelStoppedEventAction(new ReelStoppedEvent(1, 100, true));

            _disable.Verify(x => x.Enable(new Guid("{AD46A871-616A-4034-9FB5-962F8DE15E79}")), Times.Once());
        }

        [TestMethod]
        public void HandleReelStoppedEvent_NotAllStoppedTest()
        {
            InitializeClient(false);
            _disable.Reset();
            _reelController.Setup(x => x.LogicalState).Returns(ReelControllerState.IdleUnknown);
            var connectedReels = new List<int>() { 1, 2 };
            _reelController.Setup(x => x.ConnectedReels).Returns(connectedReels);
            var reelState = new Dictionary<int, ReelLogicalState>();
            reelState.Add(1, ReelLogicalState.IdleAtStop);
            reelState.Add(2, ReelLogicalState.Homing);
            _reelController.Setup(x => x.ReelStates).Returns(reelState);

            Assert.IsNotNull(_reelStoppedEventAction);
            _reelStoppedEventAction(new ReelStoppedEvent(1, 100, true));

            _disable.Verify(x => x.Enable(new Guid("{AD46A871-616A-4034-9FB5-962F8DE15E79}")), Times.Never());
        }

        [DataRow(ReelControllerState.Uninitialized, false, false)]
        [DataRow(ReelControllerState.Inspecting, false, false)]
        [DataRow(ReelControllerState.IdleUnknown, true, true)]
        [DataRow(ReelControllerState.IdleAtStops, true, true)]
        [DataRow(ReelControllerState.Homing, false, true)]
        [DataRow(ReelControllerState.Spinning, false, true)]
        [DataRow(ReelControllerState.Tilted, true, true)]
        [DataRow(ReelControllerState.Disabled, false, true)]
        [DataRow(ReelControllerState.Disconnected, false, false)]
        [DataTestMethod]
        public async Task HandleMainDoorClosedEventTest(ReelControllerState state, bool callHomeReels, bool callSelfTest)
        {
            InitializeClient(false);
            _disable.Reset();
            _reelController.Reset();
            _reelController.Setup(x => x.LogicalState).Returns(state);
            var disableKeys = new List<Guid> { ReelFaults.Disconnected.GetAttribute<ErrorGuidAttribute>().Id };
            _disable.Setup(x => x.CurrentDisableKeys).Returns(disableKeys);

            Assert.IsNotNull(_doorClosedAction);
            await _doorClosedAction(new ClosedEvent((int)DoorLogicalId.Main, string.Empty), CancellationToken.None);

            _reelController.Verify();
            _reelController.Verify(x => x.HomeReels(), callHomeReels ? Times.Once() : Times.Never());
            _reelController.Verify(x => x.SelfTest(false), callSelfTest ? Times.Once() : Times.Never());
        }

        [DataRow(ReelControllerState.Uninitialized, false)]
        [DataRow(ReelControllerState.Inspecting, false)]
        [DataRow(ReelControllerState.IdleUnknown, true)]
        [DataRow(ReelControllerState.IdleAtStops, true)]
        [DataRow(ReelControllerState.Homing, true)]
        [DataRow(ReelControllerState.Spinning, true)]
        [DataRow(ReelControllerState.Tilted, true)]
        [DataRow(ReelControllerState.Disabled, true)]
        [DataRow(ReelControllerState.Disconnected, false)]
        [DataTestMethod]
        public async Task HandleMainDoorClosedEventWithSystemDisableTest(ReelControllerState state, bool callSelfTest)
        {
            InitializeClient(false);
            _disable.Reset();
            _reelController.Reset();
            _reelController.Setup(x => x.LogicalState).Returns(state);
            if (callSelfTest)
            {
                _reelController.Setup(x => x.SelfTest(false)).Returns(Task.FromResult(true)).Verifiable();
            }

            var disableKeys = new List<Guid>() { ApplicationConstants.LogicDoorGuid };
            _disable.Setup(x => x.CurrentDisableKeys).Returns(disableKeys);

            Assert.IsNotNull(_doorClosedAction);
            await _doorClosedAction(new ClosedEvent((int)DoorLogicalId.Main, string.Empty), CancellationToken.None);

            _reelController.Verify();
            _reelController.Verify(x => x.HomeReels(), Times.Never());
        }

        [DataRow(ReelControllerState.Uninitialized, false, false)]
        [DataRow(ReelControllerState.Inspecting, false, false)]
        [DataRow(ReelControllerState.IdleUnknown, true, true)]
        [DataRow(ReelControllerState.IdleAtStops, true, true)]
        [DataRow(ReelControllerState.Homing, false, true)]
        [DataRow(ReelControllerState.Spinning, false, true)]
        [DataRow(ReelControllerState.Tilted, true, true)]
        [DataRow(ReelControllerState.Disabled, false, true)]
        [DataRow(ReelControllerState.Disconnected, false, false)]
        [DataTestMethod]
        public async Task HandleMainDoorClosedEventWithLiveAuthDisableTest(ReelControllerState state, bool callHomeReels, bool callSelfTest)
        {
            InitializeClient(false);
            _disable.Reset();
            _reelController.Reset();
            _reelController.Setup(x => x.GetReelLightIdentifiers())
                .Returns(Task.FromResult<IList<int>>(new List<int>()));
            _reelController.Setup(x => x.LogicalState).Returns(state);
            if (callSelfTest)
            {
                _reelController.Setup(x => x.SelfTest(false)).Returns(Task.FromResult(true)).Verifiable();
            }
            var disableKeys = new List<Guid>() { ApplicationConstants.LiveAuthenticationDisableKey };
            _disable.Setup(x => x.CurrentDisableKeys).Returns(disableKeys);

            Assert.IsNotNull(_doorClosedAction);
            await _doorClosedAction(new ClosedEvent((int)DoorLogicalId.Main, string.Empty), CancellationToken.None);

            _reelController.Verify(x => x.HomeReels(), callHomeReels ? Times.Once() : Times.Never());
        }

        [DataRow(ReelControllerState.Uninitialized, false, false)]
        [DataRow(ReelControllerState.Inspecting, false, false)]
        [DataRow(ReelControllerState.IdleUnknown, true, true)]
        [DataRow(ReelControllerState.IdleAtStops, true, true)]
        [DataRow(ReelControllerState.Homing, false, true)]
        [DataRow(ReelControllerState.Spinning, false, true)]
        [DataRow(ReelControllerState.Tilted, true, true)]
        [DataRow(ReelControllerState.Disabled, false, true)]
        [DataRow(ReelControllerState.Disconnected, false, false)]
        [DataTestMethod]
        public async Task HandleMainDoorClosedEventWithReelTiltTest(ReelControllerState state, bool callHomeReels, bool callSelfTest)
        {
            InitializeClient(false);
            _disable.Reset();
            _reelController.Reset();
            _reelController.Setup(x => x.GetReelLightIdentifiers())
                .Returns(Task.FromResult<IList<int>>(new List<int>()));
            _reelController.Setup(x => x.LogicalState).Returns(state);
            if (callSelfTest)
            {
                _reelController.Setup(x => x.SelfTest(false)).Returns(Task.FromResult(true)).Verifiable();
            }

            var disableKeys = new List<Guid>() { new Guid("{AD46A871-616A-4034-9FB5-962F8DE15E79}") };
            _disable.Setup(x => x.CurrentDisableKeys).Returns(disableKeys);

            Assert.IsNotNull(_doorClosedAction);
            await _doorClosedAction(new ClosedEvent((int)DoorLogicalId.Main, string.Empty), CancellationToken.None);

            _reelController.Verify(x => x.HomeReels(), callHomeReels ? Times.Once() : Times.Never());
        }

        [TestMethod]
        public async Task SystemDisabledNotInOperatorMenuTest()
        {
            InitializeClient(false);
            _disable.Reset();
            _reelController.Reset();
            _gamePlayState.Reset();

            _gamePlayState.Setup(x => x.Enabled).Returns(false);
            _gamePlayState.Setup(x => x.InGameRound).Returns(false);
            _disable.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid> { ApplicationConstants.SystemDisableGuid });

            _reelController.Setup(x => x.LogicalState).Returns(ReelControllerState.IdleUnknown);
            _reelController.Setup(x => x.Connected).Returns(true);

            _disable.Setup(x => x.IsDisabled).Returns(true);
            _reelController.Setup(x => x.TiltReels()).Returns(Task.FromResult(true));

            Assert.IsNotNull(_systemDisabledAction);
            await _systemDisabledAction(new SystemDisableAddedEvent(SystemDisablePriority.Immediate, Guid.Empty, string.Empty, false), CancellationToken.None);

            _reelController.Verify(x => x.TiltReels(), Times.Once());
        }

        [TestMethod]
        public async Task SystemDisabledInOperatorMenuOnlyFaultTest()
        {
            InitializeClient(false);
            _disable.Reset();
            _disable.Setup(x => x.IsDisabled).Returns(false);
            _disable.Setup(x => x.CurrentDisableKeys.Count).Returns(1);
            _disable.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>() { ApplicationConstants.OperatorKeyNotRemovedDisableKey });

            Assert.IsNotNull(_systemDisabledAction);
            await _systemDisabledAction(new SystemDisableAddedEvent(SystemDisablePriority.Immediate, Guid.Empty, string.Empty, false), CancellationToken.None);
        }

        [TestMethod]
        public async Task SystemDisabledInOperatorMenuOnlyFault2Test()
        {
            InitializeClient(false);
            _disable.Reset();
            _disable.Setup(x => x.IsDisabled).Returns(false);
            _disable.Setup(x => x.CurrentDisableKeys.Count).Returns(1);
            _disable.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>() { ApplicationConstants.OperatorMenuLauncherDisableGuid });

            Assert.IsNotNull(_systemDisabledAction);
            await _systemDisabledAction(new SystemDisableAddedEvent(SystemDisablePriority.Immediate, Guid.Empty, string.Empty, false), CancellationToken.None);
        }

        [TestMethod]
        public async Task SystemDisabledInOperatorMenuOnlyFault3Test()
        {
            InitializeClient(false);
            _disable.Reset();
            _disable.Setup(x => x.IsDisabled).Returns(false);
            _disable.Setup(x => x.CurrentDisableKeys.Count).Returns(2);
            _disable.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>() { ApplicationConstants.OperatorKeyNotRemovedDisableKey, ApplicationConstants.OperatorMenuLauncherDisableGuid });

            Assert.IsNotNull(_systemDisabledAction);
            await _systemDisabledAction(new SystemDisableAddedEvent(SystemDisablePriority.Immediate, Guid.Empty, string.Empty, false), CancellationToken.None);
        }

        [TestMethod]
        public async Task SystemDisableRemovedNoSystemDisableTest()
        {
            InitializeClient(false);
            _disable.Reset();
            _reelController.Reset();

            _reelController.Setup(x => x.GetReelLightIdentifiers())
                .Returns(Task.FromResult<IList<int>>(new List<int>()));
            _reelController.Setup(x => x.LogicalState).Returns(ReelControllerState.Tilted);

            var disableKeys = new List<Guid>();
            _disable.Setup(x => x.CurrentDisableKeys).Returns(disableKeys);

            Assert.IsNotNull(_systemDisableRemovedAction);
            await _systemDisableRemovedAction(new SystemDisableRemovedEvent(SystemDisablePriority.Immediate, ApplicationConstants.LiveAuthenticationDisableKey, string.Empty, true, true), CancellationToken.None);

            _reelController.Verify(x => x.HomeReels(), Times.Once());
        }

        [TestMethod]
        public async Task SystemDisableRemovedWithSystemDisableTest()
        {
            InitializeClient(true);
            var disableKeys = new List<Guid>() { ApplicationConstants.LiveAuthenticationDisableKey };
            _disable.Setup(x => x.CurrentDisableKeys).Returns(disableKeys);

            _reelController.Setup(x => x.LogicalState)
                .Returns(ReelControllerState.Tilted);

            Assert.IsNotNull(_systemDisableRemovedAction);
            await _systemDisableRemovedAction(
                new SystemDisableRemovedEvent(
                    SystemDisablePriority.Immediate,
                    ApplicationConstants.LiveAuthenticationDisableKey,
                    string.Empty,
                    true,
                    true),
                CancellationToken.None);

            _reelController.Verify(x => x.HomeReels(), Times.Once());
        }

        [TestMethod]
        public async Task SystemDisableRemovedWithLiveAuthTest()
        {
            InitializeClient(true);
            var disableKeys = new List<Guid>() { ApplicationConstants.LiveAuthenticationDisableKey };
            _disable.Setup(x => x.CurrentDisableKeys).Returns(disableKeys);
            _reelController.Setup(x => x.LogicalState)
                .Returns(ReelControllerState.Tilted);

            Assert.IsNotNull(_systemDisableRemovedAction);
            await _systemDisableRemovedAction(
                new SystemDisableRemovedEvent(
                    SystemDisablePriority.Immediate,
                    ApplicationConstants.MainDoorGuid,
                    string.Empty,
                    true,
                    true),
                CancellationToken.None);

            _reelController.Verify(x => x.HomeReels(), Times.Once());
        }

        [TestMethod]
        public async Task SystemDisableRemovedWithReelTiltTest()
        {
            InitializeClient(true);
            var disableKeys = new List<Guid>() { new Guid("{AD46A871-616A-4034-9FB5-962F8DE15E79}") };
            _disable.Setup(x => x.CurrentDisableKeys).Returns(disableKeys);
            _reelController.Setup(x => x.LogicalState)
                .Returns(ReelControllerState.Tilted);

            Assert.IsNotNull(_systemDisableRemovedAction);
            await _systemDisableRemovedAction(
                new SystemDisableRemovedEvent(
                    SystemDisablePriority.Immediate,
                    ApplicationConstants.MainDoorGuid,
                    string.Empty,
                    true,
                    true),
                CancellationToken.None);

            _reelController.Verify(x => x.HomeReels(), Times.Once());
        }

        [TestMethod]
        public async Task ConnectedNoOtherSystemFaultsTest()
        {
            InitializeClient(false);

            CreateTarget();

            Assert.IsNotNull(_connectedAction);
            await _connectedAction(new ConnectedEvent(), CancellationToken.None);

            _disable.Verify(x => x.Enable(ApplicationConstants.ReelControllerDisconnectedGuid));
        }

        [TestMethod]
        public void NoReelControllerSelectedFaultTest()
        {
            _reelController = null;
            MoqServiceManager.RemoveService<IReelController>();
            using var waiter = new ManualResetEvent(false);
            _disable.Setup(
                x => x.Disable(
                    ApplicationConstants.ReelCountMismatchDisableKey,
                    It.IsAny<SystemDisablePriority>(),
                    It.IsAny<Func<string>>(),
                    It.IsAny<bool>(),
                    It.IsAny<Func<string>>(),
                    null))
                .Callback(() => waiter.Set());

            CreateTarget();
            Assert.IsTrue(waiter.WaitOne(WaitTIme));
            _disable.Verify(
                x => x.Disable(
                ApplicationConstants.ReelCountMismatchDisableKey,
                SystemDisablePriority.Immediate,
                It.IsAny<Func<string>>(),
                true,
                It.IsAny<Func<string>>(),
                null));
        }

        [TestMethod]
        public async Task ConnectedOtherSystemFaultsTest()
        {
            InitializeClient(true);
            Assert.IsNotNull(_connectedAction);
            await _connectedAction(new ConnectedEvent(), CancellationToken.None);

            _disable.Verify(x => x.Enable(ApplicationConstants.ReelControllerDisconnectedGuid));
            _reelController.Verify(x => x.HomeReels(), Times.Never);
        }

        [TestMethod]
        public async Task DisconnectedTest()
        {
            InitializeClient(false);
            _reelController.Setup(x => x.LogicalState).Returns(ReelControllerState.IdleAtStops);

            Assert.IsNotNull(_disconnectedAction);
            await _disconnectedAction(new DisconnectedEvent(), CancellationToken.None);

            _disable.Verify(
                x => x.Disable(
                    ApplicationConstants.ReelControllerDisconnectedGuid,
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    null));
        }

        [DataRow(ReelControllerFaults.None, false)]
        [DataTestMethod]
        public async Task HardwareFaultNoneOccuredTest(ReelControllerFaults fault, bool isDisabled)
        {
            // Don't allow the disable to be set on initialize
            InitializeClient(false);

            _disable.Reset();
            _reelController.Setup(x => x.ReelControllerFaults).Returns(ReelControllerFaults.HardwareError);
            _reelController.Setup(x => x.LogicalState).Returns(ReelControllerState.IdleAtStops);
            _reelController.Setup(x => x.Connected).Returns(true);
            _disable.Setup(x => x.IsDisabled).Returns(true);

            Assert.IsNotNull(_reelControllerFaultedAction);
            await _reelControllerFaultedAction(new HardwareFaultEvent(fault), CancellationToken.None);

            _disable.Verify(
                x => x.Disable(
                    It.IsAny<Guid>(),
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    It.IsAny<Type>()),
                isDisabled ? Times.Exactly(1) : Times.Exactly(0));  // ReelTilt fault will also be set
            _reelController.Verify(x => x.TiltReels(), Times.Exactly(1));
        }

        [DataRow(ReelControllerFaults.Disconnected, true)]
        [DataRow(ReelControllerFaults.FirmwareFault, true)]
        [DataRow(ReelControllerFaults.HardwareError, true)]
        [DataRow(ReelControllerFaults.LightError, true)]
        [DataRow(ReelControllerFaults.LowVoltage, true)]
        [DataRow(ReelControllerFaults.CommunicationError, true)]
        [DataTestMethod]
        public async Task HardwareFaultOccuredTest(ReelControllerFaults fault, bool isDisabled)
        {
            // Don't allow the disable to be set on initialize
            InitializeClient(false);

            _disable.Reset();
            _reelController.Reset();
            _reelController.Setup(x => x.ReelControllerFaults).Returns(ReelControllerFaults.HardwareError);
            _reelController.Setup(x => x.LogicalState).Returns(ReelControllerState.IdleAtStops);
            _reelController.Setup(x => x.Connected).Returns(true);
            _disable.Setup(x => x.IsDisabled).Returns(true);

            Assert.IsNotNull(_reelControllerFaultedAction);
            await _reelControllerFaultedAction(new HardwareFaultEvent(fault), CancellationToken.None);

            _disable.Verify(
                x => x.Disable(
                    It.IsAny<Guid>(),
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    It.IsAny<Type>()),
                isDisabled ? Times.Exactly(1) : Times.Exactly(0));  // ReelTilt fault will also be set
            _reelController.Verify(x => x.TiltReels(), Times.Once());
        }

        [DataRow(ReelControllerFaults.None, false)]
        [DataRow(ReelControllerFaults.Disconnected, true)]
        [DataRow(ReelControllerFaults.FirmwareFault, true)]
        [DataRow(ReelControllerFaults.HardwareError, true)]
        [DataRow(ReelControllerFaults.LightError, true)]
        [DataRow(ReelControllerFaults.LowVoltage, true)]
        [DataRow(ReelControllerFaults.CommunicationError, true)]
        [DataTestMethod]
        public async Task HardwareFaultClearedTest(ReelControllerFaults fault, bool isCleared)
        {
            InitializeClient(false);
            _disable.Reset();

            _disable.Setup(x => x.IsDisabled).Returns(true);

            Assert.IsNotNull(_reelControllerFaultedAction);
            Assert.IsNotNull(_reelControllerClearAction);

            await _reelControllerFaultedAction(new HardwareFaultEvent(fault), CancellationToken.None); // Set the error
            await _reelControllerClearAction(new HardwareFaultClearEvent(fault), CancellationToken.None);
            _disable.Verify(x => x.Enable(It.IsAny<Guid>()), isCleared ? Times.Once() : Times.Never());
        }

        [DataRow(ReelControllerFaults.None, false)]
        [DataRow(ReelControllerFaults.Disconnected, true)]
        [DataRow(ReelControllerFaults.FirmwareFault, true)]
        [DataRow(ReelControllerFaults.HardwareError, true)]
        [DataRow(ReelControllerFaults.LightError, true)]
        [DataRow(ReelControllerFaults.LowVoltage, true)]
        [DataRow(ReelControllerFaults.CommunicationError, true)]
        [DataTestMethod]
        public async Task HardwareFaultClearedNoOtherFaultsTest(ReelControllerFaults fault, bool isCleared)
        {
            InitializeClient(false);
            _disable.Reset();
            _reelController.Reset();

            _reelController.Setup(x => x.GetReelLightIdentifiers())
                .Returns(Task.FromResult<IList<int>>(new List<int>()));
            _reelController.Setup(x => x.LogicalState).Returns(ReelControllerState.Tilted);

            _disable.Setup(x => x.IsDisabled).Returns(false);

            Assert.IsNotNull(_reelControllerFaultedAction);
            Assert.IsNotNull(_reelControllerClearAction);

            await _reelControllerFaultedAction(new HardwareFaultEvent(fault), CancellationToken.None); // Set the error
            await _reelControllerClearAction(new HardwareFaultClearEvent(fault), CancellationToken.None);

            _disable.Verify(x => x.Enable(It.IsAny<Guid>()), isCleared ? Times.Once() : Times.Never());
            _reelController.Verify(x => x.HomeReels(), Times.Once);
        }

        [DataRow(ReelFaults.None, false)]
        [DataTestMethod]
        public async Task ReelFaultNoneOccuredTest(ReelFaults fault, bool isDisabled)
        {
            InitializeClient(false);
            _disable.Reset();
            _reelController.Reset();

            // Don't allow the disable to be set on initialize
            _reelController.Setup(x => x.ReelControllerFaults).Returns(ReelControllerFaults.HardwareError);
            _reelController.Setup(x => x.LogicalState).Returns(ReelControllerState.IdleAtStops);
            _reelController.Setup(x => x.Connected).Returns(true);
            _disable.Setup(x => x.IsDisabled).Returns(true);

            Assert.IsNotNull(_faultedAction);
            await _faultedAction(new HardwareReelFaultEvent(fault, 1), CancellationToken.None);

            _disable.Verify(
                x => x.Disable(
                    It.IsAny<Guid>(),
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    It.IsAny<Type>()),
                isDisabled ? Times.Exactly(1) : Times.Exactly(0));   // ReelTilt fault will also be set
            _reelController.Verify(x => x.TiltReels(), isDisabled ? Times.Once() : Times.Never());
        }

        [DataRow(ReelFaults.ReelStall, true)]
        [DataRow(ReelFaults.ReelTamper, true)]
        [DataRow(ReelFaults.RequestError, true)]
        [DataRow(ReelFaults.LowVoltage, true)]
        [DataRow(ReelFaults.Disconnected, true)]
        [DataTestMethod]
        public async Task ReelFaultOccuredTest(ReelFaults fault, bool isDisabled)
        {
            InitializeClient(false);
            _disable.Reset();
            _reelController.Reset();

            // Don't allow the disable to be set on initialize
            _reelController.Setup(x => x.ReelControllerFaults).Returns(ReelControllerFaults.HardwareError);
            _reelController.Setup(x => x.LogicalState).Returns(ReelControllerState.IdleAtStops);
            _reelController.Setup(x => x.Connected).Returns(true);
            _disable.Setup(x => x.IsDisabled).Returns(true);

            Assert.IsNotNull(_faultedAction);
            await _faultedAction(new HardwareReelFaultEvent(fault, 1), CancellationToken.None);

            _disable.Verify(
                x => x.Disable(
                    It.IsAny<Guid>(),
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    It.IsAny<Type>()),
                isDisabled ? Times.Exactly(1) : Times.Exactly(0));   // ReelTilt fault will also be set
            _reelController.Verify(x => x.TiltReels(), isDisabled ? Times.Once() : Times.Never());
        }

        [DataRow(ReelFaults.None, false)]
        [DataRow(ReelFaults.ReelStall, true)]
        [DataRow(ReelFaults.ReelTamper, true)]
        [DataRow(ReelFaults.RequestError, true)]
        [DataRow(ReelFaults.LowVoltage, true)]
        [DataRow(ReelFaults.Disconnected, true)]
        [DataTestMethod]
        public async Task ReelFaultClearedTest(ReelFaults fault, bool isCleared)
        {
            InitializeClient(false);
            _disable.Reset();

            _disable.Setup(x => x.IsDisabled).Returns(true);

            Assert.IsNotNull(_faultedAction);
            Assert.IsNotNull(_clearAction);

            await _faultedAction(new HardwareReelFaultEvent(fault, 1), CancellationToken.None); // Set the error
            await _clearAction(new HardwareReelFaultClearEvent(fault), CancellationToken.None);
            _disable.Verify(x => x.Enable(It.IsAny<Guid>()), isCleared ? Times.Once() : Times.Never());
        }

        [DataRow(ReelFaults.None, false)]
        [DataRow(ReelFaults.ReelStall, true)]
        [DataRow(ReelFaults.ReelTamper, true)]
        [DataRow(ReelFaults.RequestError, true)]
        [DataRow(ReelFaults.LowVoltage, true)]
        [DataRow(ReelFaults.Disconnected, true)]
        [DataTestMethod]
        public async Task ReelFaultClearedNoOtherFaultsTest(ReelFaults fault, bool isCleared)
        {
            InitializeClient(false);
            _disable.Reset();
            _reelController.Reset();

            _reelController.Setup(x => x.GetReelLightIdentifiers())
                .Returns(Task.FromResult<IList<int>>(new List<int>()));
            _reelController.Setup(x => x.LogicalState).Returns(ReelControllerState.Tilted);

            _disable.Setup(x => x.IsDisabled).Returns(false);

            Assert.IsNotNull(_faultedAction);
            Assert.IsNotNull(_clearAction);

            await _faultedAction(new HardwareReelFaultEvent(fault, 1), CancellationToken.None); // Set the error
            await _clearAction(new HardwareReelFaultClearEvent(fault), CancellationToken.None);

            _disable.Verify(x => x.Enable(It.IsAny<Guid>()), isCleared ? Times.Once() : Times.Never());
            _reelController.Verify(x => x.HomeReels(), Times.Once());
        }

        [TestMethod]
        public async Task InspectedTest()
        {
            InitializeClient(false);
            _disable.Reset();

            _reelController.Setup(x => x.GetReelLightIdentifiers())
                .Returns(Task.FromResult<IList<int>>(new List<int>()));
            _reelController.Setup(x => x.LogicalState).Returns(ReelControllerState.IdleUnknown);

            // Disable the fault first before having it cleared.
            Assert.IsNotNull(_inspectionFailedAction);
            await _inspectionFailedAction(new InspectionFailedEvent(), CancellationToken.None);

            _disable.Setup(x => x.IsDisabled).Returns(false);

            Assert.IsNotNull(_inspectedAction);
            await _inspectedAction(new InspectedEvent(), CancellationToken.None);

            _disable.Verify(x => x.Enable(ApplicationConstants.ReelControllerDisconnectedGuid));
        }

        [TestMethod]
        public async Task InspectedWithOtherFaultsTest()
        {
            InitializeClient(false);
            _disable.Reset();
            _disable.Setup(x => x.IsDisabled).Returns(true);

            Assert.IsNotNull(_inspectedAction);
            await _inspectedAction(new InspectedEvent(), CancellationToken.None);

            _disable.Verify(x => x.Enable(ApplicationConstants.ReelControllerDisconnectedGuid));
        }

        [TestMethod]
        public async Task InspectionFailedTest()
        {
            _reelController.Setup(x => x.Connected).Returns(true);
            InitializeClient(false);
            _disable.Reset();
            Assert.IsNotNull(_inspectionFailedAction);
            await _inspectionFailedAction(new InspectionFailedEvent(), CancellationToken.None);

            _disable.Verify(x => x.Disable(ApplicationConstants.ReelControllerDisconnectedGuid, SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null));
        }

        [TestMethod]
        public void EnabledTest()
        {
            InitializeClient(false);
            _disable.Reset();
            // Set the disabled fault first so it can be cleared
            Assert.IsNotNull(_disabledAction);
            _disabledAction(new DisabledEvent(DisabledReasons.Service));

            Assert.IsNotNull(_enabledAction);
            _enabledAction(new EnabledEvent(EnabledReasons.Service));

            _disable.Verify(x => x.Enable(new Guid("{B9029021-106D-419B-961F-1B2799817916}")));
        }

        [TestMethod]
        public void DisabledTest()
        {
            InitializeClient(false);
            _disable.Reset();

            // Don't allow the disable to be set on initialize
            _reelController.Setup(x => x.ReelControllerFaults).Returns(ReelControllerFaults.HardwareError);

            Assert.IsNotNull(_disabledAction);
            _disabledAction(new DisabledEvent(DisabledReasons.Service));

            _disable.Verify(x => x.Disable(new Guid("{B9029021-106D-419B-961F-1B2799817916}"), SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null));
        }

        [TestMethod]
        public void GameAddedEventTest()
        {
            InitializeClient(false);

            var expectedHomeSteps = new Dictionary<int, int>() { { 1, 118 }, { 2, 56 }, { 3, 132 } };
            _reelController.SetupSet(x => x.ReelHomeSteps = expectedHomeSteps).Verifiable();

            Assert.IsNotNull(_gameAddedEventAction);
            _gameAddedEventAction(new GameAddedEvent(1, "theme"));

            _disable.Verify(x => x.Enable(ApplicationConstants.ReelCountMismatchDisableKey));
        }

        private void InitializeClient(bool areReelsTilted)
        {
            using var waiter = new ManualResetEvent(false);
            if (areReelsTilted)
            {
                _reelController.SetupSequence(x => x.LogicalState)
                    .Returns(ReelControllerState.Tilted)
                    .Returns(ReelControllerState.IdleUnknown);
            }
            else
            {
                _reelController.Setup(x => x.LogicalState)
                    .Returns(ReelControllerState.Tilted);
            }

            _gamePlayState.Setup(x => x.Enabled).Returns(!areReelsTilted);
            _gamePlayState.Setup(x => x.InGameRound).Returns(false);
            _disable.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid> { ApplicationConstants.SystemDisableGuid });
            if (areReelsTilted)
            {
                var count = 0;
                _reelController.Setup(x => x.TiltReels())
                    .Returns(Task.FromResult(true))
                    .Callback(() =>
                    {
                        if (count++ == 1)
                        {
                            waiter.Set();
                        }
                    });
            }
            else
            {
                _reelController.Setup(x => x.HomeReels()).Returns(Task.FromResult(true))
                    .Callback(() => waiter.Set());
            }

            CreateTarget();
            Assert.IsTrue(waiter.WaitOne(WaitTIme));
        }

        private ReelControllerMonitor CreateTarget(
            bool nullEvent = false,
            bool nullMessage = false,
            bool nullDisable = false,
            bool nullGamePlay = false,
            bool nullGameProvider = false,
            bool nullEdgeLightController = false,
            bool nullGameService = false,
            bool nullOperatorMenuLauncher = false)
        {
            return new ReelControllerMonitor(
                nullEvent ? null : _eventBus.Object,
                nullMessage ? null : _messageDisplay.Object,
                nullDisable ? null : _disable.Object,
                nullGamePlay ? null : _gamePlayState.Object,
                nullGameProvider ? null : _gameProvider.Object,
                nullEdgeLightController ? null : _edgeLightingController.Object,
                nullGameService ? null : _gameService.Object,
                nullOperatorMenuLauncher ? null : _operatorMenuLauncher.Object);
        }
    }
}