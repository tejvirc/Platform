namespace Aristocrat.Monaco.Hardware.Tests.Reel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Hardware.Contracts.Communicator;
    using Aristocrat.Monaco.Hardware.Contracts.Dfu;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Aristocrat.Monaco.Hardware.Contracts.SharedDevice;
    using Aristocrat.Monaco.Hardware.Reel;
    using Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    [TestClass]
    public class ReelControllerAdapterTests
    {
        private const string ReelBrightnessOption = "ReelBrightness";
        private const string OptionsBlock = "Aristocrat.Monaco.Hardware.MechanicalReels.ReelControllerAdapter.Options";
        private const int reelCount = 3;

        private ReelControllerAdapter _target;
        private dynamic _privateAccessor;
        private Mock<IReelControllerImplementation> _controller;
        private Mock<IGdsCommunicator> _communicator;
        private Mock<IDfuProvider> _dfuProvider;
        private Mock<IPersistentStorageManager> _persistence;
        private Mock<IEventBus> _eventBus;
        private readonly ManualResetEvent _waitEvent = new ManualResetEvent(false);
        private Mock<IAddinFactory> _factory;
        private Mock<IPersistentStorageAccessor> _accessor = new Mock<IPersistentStorageAccessor>(MockBehavior.Default);
        private Mock<IPersistentStorageTransaction> _transaction;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new ReelControllerAdapter();

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _persistence = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);

            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            _privateAccessor = new DynamicPrivateObject(_target);

            _controller = new Mock<IReelControllerImplementation>(MockBehavior.Default);
            _privateAccessor._reelController = _controller.Object;
            _controller.Setup(c => c.SetBrightness(It.IsAny<Dictionary<int, int>>())).Returns(Task.FromResult(true)).Verifiable();

            for (var i = 0; i < reelCount; i++)
            {
                var args = new ReelEventArgs(i);
                _privateAccessor.ReelControllerOnReelConnected(new object(), args);
            }
        }

        [TestMethod]
        public void CheckInitialStateTest()
        {
            Assert.AreEqual(ReelControllerState.Uninitialized, _target.LogicalState);
        }

        [TestMethod]
        public async Task SetLightsTest()
        {
            var reelLampData = new ReelLampData[]
            {
                new ReelLampData(System.Drawing.Color.White, true, 5),
                new ReelLampData(System.Drawing.Color.White, false, 2),
                new ReelLampData(System.Drawing.Color.White, true, 3),
            };

            _controller.Setup(c => c.SetLights(It.IsAny<ReelLampData[]>())).Returns(Task.FromResult(true)).Verifiable();

            await _target.SetLights(reelLampData);
        }

        [TestMethod]
        public async Task GetReelLightIdentifiersTest()
        {
            IList<int> result = new List<int>();
            _controller.Setup(c => c.GetReelLightIdentifiers()).Returns(Task.FromResult(result)).Verifiable();

            await _target.GetReelLightIdentifiers();
        }

        [TestMethod]
        public async Task SetSpecificReelBrightnessTest()
        {
            var idToBrightnessDictionary = new Dictionary<int, int>
            {
                {3, 5},
                {2, 5},
                {1, 5},
            };

            _controller.Setup(c => c.SetBrightness(It.IsAny<Dictionary<int, int>>())).Returns(Task.FromResult(true)).Verifiable();

            await _target.SetReelBrightness(idToBrightnessDictionary);
        }

        [TestMethod]
        public async Task SetReelBrightnessTest()
        {
            _controller.Setup(c => c.SetBrightness(It.IsAny<int>())).Returns(Task.FromResult(true)).Verifiable();

            await _target.SetReelBrightness(5);
        }

        [TestMethod]
        public async Task SetReelSpeedTest()
        {
            Initialize();

            _controller.Setup(c => c.SetReelSpeed(It.IsAny<ReelSpeedData>())).Returns(Task.FromResult(true)).Verifiable();

            await _target.SetReelSpeed(new ReelSpeedData(1, 10));
        }

        [TestMethod]
        public void DisabledWhileUnitializedTest()
        {
            Initialize();
            _target.Disable(DisabledReasons.Service);

            Assert.AreEqual(ReelControllerState.Uninitialized, _target.LogicalState);
        }

        [TestMethod]
        public void EnableTest()
        {
            _controller.Setup(c => c.Enable()).Verifiable();

            Initialize();
            _target.Disable(DisabledReasons.Error);

            _controller.Raise(f => f.Initialized += null, EventArgs.Empty);
            _controller.Raise(f => f.FaultCleared += null, new ReelFaultedEventArgs(ReelFaults.Disconnected, 2));

            Assert.AreEqual(ReelControllerState.IdleUnknown, _target.LogicalState);
        }

        [TestMethod]
        public void DisableTest()
        {
            _controller.Setup(c => c.UpdateConfiguration(It.IsAny<IDeviceConfiguration>())).Verifiable();
            _controller.Setup(c => c.Enable()).Verifiable();

            Initialize();

            _controller.Raise(f => f.Initialized += null, EventArgs.Empty);
            _controller.Raise(f => f.Disabled += null, EventArgs.Empty);

            Assert.AreEqual(ReelControllerState.Disabled, _target.LogicalState);
        }

        [TestMethod]
        public void TiltTest()
        {
            Initialize();

            _controller.Raise(f => f.Initialized += null, EventArgs.Empty);
            _controller.Raise(f => f.ReelSlowSpinning += null, new ReelEventArgs(1));

            var result =_target.ReelStates[1] == ReelLogicalState.Tilted;

            Assert.IsTrue(result);
            Assert.AreEqual(ReelControllerState.Tilted, _target.LogicalState);
        }

        [TestMethod]
        public void DisconnectAndReconnectReelTest()
        {
            Initialize();
            Inspect();

            foreach (var reelId in _target.ConnectedReels)
            {
                _controller.Raise(f => f.ReelDisconnected += null, new ReelEventArgs(reelId));
            }

            var disconnectedResult = true;
            foreach(var reel in _target.ReelStates)
            {
                disconnectedResult &= reel.Value == ReelLogicalState.IdleUnknown;
            }

            var disconnectControllerResult = _target.LogicalState;

            foreach (var reel in _target.ReelStates)
            {
                _controller.Raise(f => f.ReelConnected += null, new ReelEventArgs(reel.Key));
            }

            var connectedResult = true;
            foreach (var reel in _target.ReelStates)
            {
                connectedResult &= reel.Value == ReelLogicalState.IdleUnknown;
            }

            Assert.IsTrue(disconnectedResult);
            Assert.IsTrue(connectedResult);
            Assert.AreEqual(ReelControllerState.Inspecting, disconnectControllerResult);
            Assert.AreEqual(ReelControllerState.Inspecting, _target.LogicalState);
        }

        [TestMethod]
        public void DisconnectAndReconnectControllerTest()
        {
            Initialize();
            Inspect();

            _controller.Raise(f => f.Disconnected += null, EventArgs.Empty);

            var disconnectControllerResult = _target.LogicalState;

            _controller.Raise(f => f.Connected += null, EventArgs.Empty);

            Assert.AreEqual(ReelControllerState.Disconnected, disconnectControllerResult);
            Assert.AreEqual(ReelControllerState.Inspecting, _target.LogicalState);
        }

        [TestMethod]
        public void InspectionFailedTest()
        {
            Initialize();
            _controller.Raise(f => f.InitializationFailed += null, EventArgs.Empty);

            Assert.AreEqual(ReelControllerState.Uninitialized, _target.LogicalState);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InitializeFailTest()
        {
            _target.Initialize();
        }

        [TestMethod]
        public void InitializeTest()
        {
            Initialize();
        }

        [TestMethod]
        public void BlockTest()
        {
            var defaultBrightness = 100;
            var blockIndex = 1;

            var accessor = new Mock<IPersistentStorageAccessor>(MockBehavior.Default);
            var transaction = new Mock<IPersistentStorageTransaction>(MockBehavior.Default);

            _accessor.Setup(m => m[blockIndex, ReelBrightnessOption]).Returns(defaultBrightness).Verifiable();
            _accessor.Setup(m => m.StartTransaction()).Returns(transaction.Object).Verifiable();
            transaction.Setup(t => t.Commit()).Verifiable();
            transaction.SetupSet(m => m[blockIndex, ReelBrightnessOption] = defaultBrightness).Verifiable();

            Assert.AreEqual(true, _target.TryAddBlock(_accessor.Object, 1, out ReelControllerOptions initialBlock));
            Assert.AreEqual(true, _target.TryGetBlock(_accessor.Object, 1, out ReelControllerOptions resultBlock));
            Assert.AreEqual(defaultBrightness, resultBlock.ReelBrightness);

            transaction.Verify();
        }

        [TestMethod]
        public async Task HomingAllTest()
        {
            Initialize();
            var args = new EventArgs();

            _controller.Setup(c => c.UpdateConfiguration(It.IsAny<IDeviceConfiguration>())).Verifiable();
            _controller.Setup(c => c.Enable()).Verifiable();
            _controller.Setup(c => c.HomeReel(It.IsAny<int>(), It.IsAny<int>(), true)).Returns(Task.FromResult(true)).Verifiable();

            _controller.Raise(f => f.Initialized += null, EventArgs.Empty);

            var result = await _target.HomeReels();

            Assert.IsTrue(result);
            Assert.IsTrue(_target.ReelStates.Count != 0);
            Assert.IsTrue(_target.ReelStates.All(reel => reel.Value == ReelLogicalState.Homing));
            Assert.AreEqual(ReelControllerState.Homing, _target.LogicalState);
        }

        [TestMethod]
        public async Task HomingFailedTest()
        {
            Initialize();
            var args = new EventArgs();

            _controller.Setup(c => c.UpdateConfiguration(It.IsAny<IDeviceConfiguration>())).Verifiable();
            _controller.Setup(c => c.Enable()).Verifiable();
            _controller.Setup(c => c.HomeReel(It.IsAny<int>(), It.IsAny<int>(), true)).Returns(Task.FromResult(false)).Verifiable();

            _controller.Raise(f => f.Initialized += null, EventArgs.Empty);

            await _target.HomeReels();

            Assert.AreEqual(ReelControllerState.Tilted, _target.LogicalState);
        }

        [TestMethod]
        public async Task HomingReelTest()
        {
            Initialize();
            var args = new EventArgs();

            _controller.Setup(c => c.UpdateConfiguration(It.IsAny<IDeviceConfiguration>())).Verifiable();
            _controller.Setup(c => c.Enable()).Verifiable();
            _controller.Setup(c => c.HomeReel(It.IsAny<int>(), It.IsAny<int>(), true)).Returns(Task.FromResult(true)).Verifiable();

            _controller.Raise(f => f.Initialized += null, EventArgs.Empty);

            await _target.HomeReels();
            foreach (var reel in _target.ReelStates)
            {
                _controller.Raise(f => f.ReelStopped += null, new ReelEventArgs(reel.Key, 0));
            }

            var result = await _target.HomeReels(_target.Steps);

            Assert.IsTrue(result);
            Assert.IsTrue(_target.ReelStates.Count != 0);
            Assert.IsTrue(_target.ReelStates.All(reel => reel.Value == ReelLogicalState.Homing));
            Assert.AreEqual(ReelControllerState.Homing, _target.LogicalState);
        }

        [TestMethod]
        public async Task NudgeTest()
        {
            Initialize();
            var args = new EventArgs();

            _controller.Setup(c => c.UpdateConfiguration(It.IsAny<IDeviceConfiguration>())).Verifiable();
            _controller.Setup(c => c.Enable()).Verifiable();
            _controller.Setup(c => c.HomeReel(It.IsAny<int>(), It.IsAny<int>(), true)).Returns(Task.FromResult(true)).Verifiable();
            _controller.Setup(c => c.NudgeReels(It.IsAny<NudgeReelData>())).Returns(Task.FromResult(true)).Verifiable();

            _controller.Raise(f => f.Initialized += null, EventArgs.Empty);

            await _target.HomeReels();
            foreach(var reel in _target.ReelStates)
            {
                _controller.Raise(f => f.ReelStopped += null, new ReelEventArgs(reel.Key, 0));
            }

            var result = true;

            foreach (var reel in _target.ReelStates)
            {
                result &= await _target.NudgeReel(new NudgeReelData(reel.Key, SpinDirection.Forward, 1));
            }

            Assert.IsTrue(result);
            Assert.IsTrue(_target.ReelStates.Count != 0);
            Assert.IsTrue(_target.ReelStates.All(reel => reel.Value == ReelLogicalState.SpinningForward));
            Assert.AreEqual(_target.LogicalState, ReelControllerState.Spinning);
        }

        [TestMethod]
        public async Task SpinTest()
        {
            Initialize();
            var args = new EventArgs();

            _controller.Setup(c => c.UpdateConfiguration(It.IsAny<IDeviceConfiguration>())).Verifiable();
            _controller.Setup(c => c.Enable()).Verifiable();
            _controller.Setup(c => c.HomeReel(It.IsAny<int>(), It.IsAny<int>(), true)).Returns(Task.FromResult(true)).Verifiable();
            _controller.Setup(c => c.SpinReels(It.IsAny<ReelSpinData>())).Returns(Task.FromResult(true)).Verifiable();

            _controller.Raise(f => f.Initialized += null, EventArgs.Empty);

            await _target.HomeReels();
            foreach (var reel in _target.ReelStates)
            {
                _controller.Raise(f => f.ReelStopped += null, new ReelEventArgs(reel.Key, 0));
            }

            var reelStoppedState = _target.LogicalState;
            var result = true;

            foreach (var reel in _target.ReelStates)
            {
                result &= await _target.SpinReels(new ReelSpinData(reel.Key, SpinDirection.Forward, 5));
            }

            Assert.AreEqual(reelStoppedState, ReelControllerState.IdleAtStops);
            Assert.IsTrue(result);
            Assert.IsTrue(_target.ReelStates.Count != 0);
            Assert.IsTrue(_target.ReelStates.All(reel => reel.Value == ReelLogicalState.SpinningForward));
            Assert.AreEqual(_target.LogicalState, ReelControllerState.Spinning);
        }

        [TestMethod]
        public async Task IdleAtStopsDisableTest()
        {
            Initialize();
            var args = new EventArgs();

            _controller.Setup(c => c.UpdateConfiguration(It.IsAny<IDeviceConfiguration>())).Verifiable();
            _controller.Setup(c => c.Enable()).Verifiable();
            _controller.Setup(c => c.HomeReel(It.IsAny<int>(), It.IsAny<int>(), true)).Returns(Task.FromResult(true)).Verifiable();

            _controller.Raise(f => f.Initialized += null, EventArgs.Empty);

            await _target.HomeReels();
            foreach (var reel in _target.ReelStates)
            {
                _controller.Raise(f => f.ReelStopped += null, new ReelEventArgs(reel.Key, 0));
            }

            _controller.Raise(f => f.Disabled += null, EventArgs.Empty);

            Assert.AreEqual(ReelControllerState.Disabled, _target.LogicalState);
        }

        [TestMethod]
        public void ReelControllerFaultOccurredTest()
        {
            ReelControllerFaults faults = ReelControllerFaults.HardwareError;
            var args = new ReelControllerFaultedEventArgs(faults);
            var postedEvent = new HardwareFaultEvent(faults);

            _eventBus.Setup(e => e.Publish(postedEvent)).Verifiable();

            Initialize();

            _controller.Raise(f => f.ControllerFaultOccurred += null, args);

            Assert.IsFalse(_target.Enabled);
        }

        [TestMethod]
        public void ReelControllerFaultOccurredMultipleFaultsTest()
        {
            ReelControllerFaults faults = ReelControllerFaults.HardwareError | ReelControllerFaults.CommunicationError;
            var args = new ReelControllerFaultedEventArgs(faults);
            var postedEvent1 = new HardwareFaultEvent(ReelControllerFaults.HardwareError);
            var postedEvent2 = new HardwareFaultEvent(ReelControllerFaults.CommunicationError);

            _eventBus.Setup(e => e.Publish(postedEvent1)).Verifiable();
            _eventBus.Setup(e => e.Publish(postedEvent2)).Verifiable();

            Initialize();

            _controller.Raise(f => f.ControllerFaultOccurred += null, args);

            Assert.IsFalse(_target.Enabled);
        }

        [TestMethod]
        public void ReelControllerFaultClearedTest()
        {
            ReelControllerFaults faults = ReelControllerFaults.HardwareError;
            var setupArgs = new ReelControllerFaultedEventArgs(faults);
            var args = new ReelControllerFaultedEventArgs(faults);
            var postedEvent = new HardwareFaultClearEvent(faults);

            _eventBus.Setup(e => e.Publish(postedEvent)).Verifiable();

            Initialize();

            _controller.Raise(f => f.ControllerFaultOccurred += null, args);
            _controller.Raise(f => f.ControllerFaultCleared += null, args);

            Assert.IsTrue(_target.Enabled);
        }

        [TestMethod]
        public void ReelControllerFaultClearedMultipleFaultsTest()
        {
            ReelControllerFaults faults = ReelControllerFaults.HardwareError | ReelControllerFaults.CommunicationError;
            var setupArgs = new ReelControllerFaultedEventArgs(faults);
            var args = new ReelControllerFaultedEventArgs(faults);
            var postedEvent1 = new HardwareFaultClearEvent(ReelControllerFaults.HardwareError);
            var postedEvent2 = new HardwareFaultClearEvent(ReelControllerFaults.CommunicationError);

            _eventBus.Setup(e => e.Publish(postedEvent1)).Verifiable();
            _eventBus.Setup(e => e.Publish(postedEvent2)).Verifiable();

            Initialize();

            _controller.Raise(f => f.ControllerFaultOccurred += null, setupArgs);
            _controller.Raise(f => f.ControllerFaultCleared += null, args);

            Assert.IsTrue(_target.Enabled);
        }

        [TestMethod]
        public void ReelControllerReelFaultOccurredTest()
        {
            ReelFaults faults = ReelFaults.ReelStall;
            var reelId = 2;
            var reelControllerId = 1;
            var args = new ReelFaultedEventArgs(faults, reelId);
            var postedEvent = new HardwareReelFaultEvent(reelControllerId, faults, reelId);

            _eventBus.Setup(e => e.Publish(postedEvent)).Verifiable();

            Initialize();

            _controller.Raise(f => f.FaultOccurred += null, args);

            Assert.IsFalse(_target.Enabled);
        }

        [TestMethod]
        public void ReelControllerReelFaultOccurredMultipleFaultsTest()
        {
            ReelFaults faults = ReelFaults.ReelStall | ReelFaults.ReelTamper;
            var reelId = 2;
            var reelControllerId = 1;
            var args = new ReelFaultedEventArgs(faults, reelId);
            var postedEvent1 = new HardwareReelFaultEvent(reelControllerId, ReelFaults.ReelStall, reelId);
            var postedEvent2 = new HardwareReelFaultEvent(reelControllerId, ReelFaults.ReelTamper, reelId);

            _eventBus.Setup(e => e.Publish(postedEvent1)).Verifiable();
            _eventBus.Setup(e => e.Publish(postedEvent2)).Verifiable();

            Initialize();

            _controller.Raise(f => f.FaultOccurred += null, args);

            Assert.IsFalse(_target.Enabled);
        }

        [TestMethod]
        public void ReelControllerReelFaultClearedTest()
        {
            ReelFaults faults = ReelFaults.ReelStall;
            var reelId = 2;
            var reelControllerId = 1;
            var setupArgs = new ReelFaultedEventArgs(faults, reelId);
            var args = new ReelFaultedEventArgs(faults, reelId);
            var postedEvent = new HardwareReelFaultClearEvent(reelControllerId, faults);

            _eventBus.Setup(e => e.Publish(postedEvent)).Verifiable();

            Initialize();

            _controller.Raise(f => f.FaultOccurred += null, setupArgs);
            _controller.Raise(f => f.FaultCleared += null, args);

            Assert.IsTrue(_target.Enabled);
        }

        [TestMethod]
        public void ReelControllerReelFaultClearedMultipleFaultsTest()
        {
            ReelFaults faults = ReelFaults.ReelStall | ReelFaults.ReelTamper;
            var reelId = 2;
            var reelControllerId = 1;
            var setupArgs = new ReelFaultedEventArgs(faults, reelId);
            var args = new ReelFaultedEventArgs(faults, reelId);
            var postedEvent1 = new HardwareReelFaultClearEvent(reelControllerId, ReelFaults.ReelStall);
            var postedEvent2 = new HardwareReelFaultClearEvent(reelControllerId, ReelFaults.ReelTamper);

            _eventBus.Setup(e => e.Publish(postedEvent1)).Verifiable();
            _eventBus.Setup(e => e.Publish(postedEvent2)).Verifiable();

            Initialize();

            _controller.Raise(f => f.FaultOccurred += null, setupArgs);
            _controller.Raise(f => f.FaultCleared += null, args);

            Assert.IsTrue(_target.Enabled);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _accessor?.Verify();
            _controller?.Verify();
            _factory?.Verify();
            _persistence?.Verify();
            _transaction?.Verify();

            MoqServiceManager.RemoveInstance();
            AddinManager.Shutdown();
        }

        private void Initialize()
        {
            _factory = new Mock<IAddinFactory>(MockBehavior.Default);
            _accessor = new Mock<IPersistentStorageAccessor>(MockBehavior.Default);
            _transaction = new Mock<IPersistentStorageTransaction>(MockBehavior.Default);

            var defaultBrightness = 100;
            var blockIndex = 0;

            _transaction = new Mock<IPersistentStorageTransaction>(MockBehavior.Default);

            _factory.Setup(f => f.CreateAddin<IReelControllerImplementation>(It.IsAny<string>(), It.IsAny<string>())).Returns(_controller.Object).Verifiable();
            _privateAccessor.AddinFactory = _factory.Object;
            _persistence.Setup(s => s.BlockExists(It.IsAny<string>())).Returns(false).Verifiable();
            _persistence.Setup(s => s.CreateBlock(PersistenceLevel.Transient, OptionsBlock, It.IsAny<int>())).Returns(_accessor.Object).Verifiable();
            _accessor.Setup(m => m.StartTransaction()).Returns(_transaction.Object).Verifiable();
            _transaction.Setup(t => t.Commit()).Verifiable();
            _transaction.SetupSet(m => m[blockIndex, ReelBrightnessOption] = defaultBrightness).Verifiable();

            _target.Initialize();
        }

        private void Inspect()
        {
            _communicator = new Mock<IGdsCommunicator>(MockBehavior.Default);
            var comConfiguration = new ComConfiguration();
            _communicator.Setup(m => m.Configure(comConfiguration)).Returns(true).Verifiable();
            _dfuProvider = MoqServiceManager.CreateAndAddService<IDfuProvider>(MockBehavior.Default);

            _factory.Setup(f => f.CreateAddin<IGdsCommunicator>(It.IsAny<string>(), It.IsAny<string>())).Returns(_communicator.Object).Verifiable();

            _target.Inspect(comConfiguration, 10);
        }
    }
}
