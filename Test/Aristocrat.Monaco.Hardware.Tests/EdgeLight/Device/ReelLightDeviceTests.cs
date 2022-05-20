namespace Aristocrat.Monaco.Hardware.Tests.EdgeLight.Device
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Hardware.Contracts.Reel;
    using Hardware.EdgeLight.Device;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using Vgt.Client12.Hardware.HidLibrary;

    [TestClass]
    public class ReelLightDeviceTests
    {
        private ReelLightDevice _device;
        private Mock<IEventBus> _eventBus;
        private Mock<IReelController> _reelController;
        private Func<ConnectedEvent, CancellationToken, Task> _connectedAction;
        private Func<DisconnectedEvent, CancellationToken, Task> _disconnectedAction;
        private Func<ReelConnectedEvent, CancellationToken, Task> _reelConnectedAction;
        private Func<ReelDisconnectedEvent, CancellationToken, Task> _reelDisconnectedAction;
        private bool _connectionChangedCallbackCalled = false;
        private bool _stripsChangedCallbackCalled = false;

        [TestInitialize]
        public void Setup()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _reelController = MoqServiceManager.CreateAndAddService<IReelController>(MockBehavior.Default);

            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Func<ConnectedEvent, CancellationToken, Task>>()))
                .Callback<object, Func<ConnectedEvent, CancellationToken, Task>>((o, c) => _connectedAction = c);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Func<DisconnectedEvent, CancellationToken, Task>>()))
                .Callback<object, Func<DisconnectedEvent, CancellationToken, Task>>((o, c) => _disconnectedAction = c);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Func<ReelConnectedEvent, CancellationToken, Task>>()))
                .Callback<object, Func<ReelConnectedEvent, CancellationToken, Task>>((o, c) => _reelConnectedAction = c);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Func<ReelDisconnectedEvent, CancellationToken, Task>>()))
                .Callback<object, Func<ReelDisconnectedEvent, CancellationToken, Task>>((o, c) => _reelDisconnectedAction = c);

            _device = new ReelLightDevice(_eventBus.Object);

            IList<int> ids = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            _reelController.Setup(x => x.GetReelLightIdentifiers()).Returns(Task.FromResult(ids));
            _reelController.Setup(x => x.ConnectedReels).Returns(new List<int>() { 1, 2, 3, 4, 5 });
            _reelController.Setup(x => x.Connected).Returns(true);
            _reelController.Setup(x => x.Enabled).Returns(true);
            _reelController.Setup(x => x.Initialized).Returns(true);

            _connectionChangedCallbackCalled = false;
            _stripsChangedCallbackCalled = false;
            _device.ConnectionChanged += EdgeLightDevice_ConnectionChanged;
            _device.StripsChanged += EdgeLightDevice_StripsChanged;
        }

        [TestCleanup]
        public void Cleanup()
        {
            MoqServiceManager.RemoveInstance();
            _device.Dispose();
        }

        [TestMethod]
        public void DiposeTest()
        {
            // Method being tested
            _device.Dispose();

            _eventBus.Verify(x => x.UnsubscribeAll(_device), Times.Once());
        }

        [TestMethod]
        public async Task HandleConnectionChangedConnectedTest()
        {
            int reelControllerId = 1;

            // Method being tested
            Assert.IsNotNull(_connectedAction);
            await _connectedAction(new ConnectedEvent(reelControllerId), CancellationToken.None);

            Assert.IsTrue(_connectionChangedCallbackCalled);
            Assert.IsTrue(_stripsChangedCallbackCalled);
            Assert.AreEqual(5, _device.PhysicalStrips.Count);
            for (var i = 0; i < _device.PhysicalStrips.Count; ++i)
            {
                var strip = _device.PhysicalStrips[i];
                Assert.AreEqual(0x50 + i, strip.StripId);
                Assert.AreEqual(3, strip.LedCount);
                Assert.AreEqual(100, strip.Brightness);
            }
        }

        [TestMethod]
        public async Task HandleConnectionChangedDisconnectedTest()
        {
            int reelControllerId = 1;

            // Method being tested
            Assert.IsNotNull(_connectedAction);
            await _disconnectedAction(new DisconnectedEvent(reelControllerId), CancellationToken.None);

            Assert.IsTrue(_connectionChangedCallbackCalled);
            Assert.IsTrue(_stripsChangedCallbackCalled);
            Assert.AreEqual(5, _device.PhysicalStrips.Count);
            for (var i = 0; i < _device.PhysicalStrips.Count; ++i)
            {
                var strip = _device.PhysicalStrips[i];
                Assert.AreEqual(0x50 + i, strip.StripId);
                Assert.AreEqual(3, strip.LedCount);
                Assert.AreEqual(100, strip.Brightness);
            }
        }

        [TestMethod]
        public async Task HandledStripChangedReelConnectedTest()
        {
            int reelId = 1;

            // Method being tested
            Assert.IsNotNull(_connectedAction);
            await _reelConnectedAction(new ReelConnectedEvent(reelId), CancellationToken.None);

            Assert.IsFalse(_connectionChangedCallbackCalled);
            Assert.IsTrue(_stripsChangedCallbackCalled);
            Assert.AreEqual(5, _device.PhysicalStrips.Count);
            for (var i = 0; i < _device.PhysicalStrips.Count; ++i)
            {
                var strip = _device.PhysicalStrips[i];
                Assert.AreEqual(0x50 + i, strip.StripId);
                Assert.AreEqual(3, strip.LedCount);
                Assert.AreEqual(100, strip.Brightness);
            }
        }

        [TestMethod]
        public async Task HandledStripChangedReelDisconnectedTest()
        {
            int reelId = 1;

            // Method being tested
            Assert.IsNotNull(_connectedAction);
            await _reelDisconnectedAction(new ReelDisconnectedEvent(reelId), CancellationToken.None);

            Assert.IsFalse(_connectionChangedCallbackCalled);
            Assert.IsTrue(_stripsChangedCallbackCalled);
            Assert.AreEqual(5, _device.PhysicalStrips.Count);
            for (var i = 0; i < _device.PhysicalStrips.Count; ++i)
            {
                var strip = _device.PhysicalStrips[i];
                Assert.AreEqual(0x50 + i, strip.StripId);
                Assert.AreEqual(3, strip.LedCount);
                Assert.AreEqual(100, strip.Brightness);
            }
        }

        [TestMethod]
        public async Task RenderAllStripDataTest()
        {
            // Setup the physical strips
            Assert.IsNotNull(_connectedAction);
            await _reelConnectedAction(new ReelConnectedEvent(1), CancellationToken.None);
            Assert.IsFalse(_connectionChangedCallbackCalled);
            Assert.IsTrue(_stripsChangedCallbackCalled);
            Assert.AreEqual(5, _device.PhysicalStrips.Count);
            for (var i = 0; i < _device.PhysicalStrips.Count; ++i)
            {
                var strip = _device.PhysicalStrips[i];
                Assert.AreEqual(0x50 + i, strip.StripId);
                Assert.AreEqual(3, strip.LedCount);
                Assert.AreEqual(100, strip.Brightness);
            }

            var expectedReelLampData = new List<ReelLampData>();
            var expectedReelLampBrightness = new Dictionary<int, int>();
            for (var i = 0; i < 15; ++i)
            {
                expectedReelLampData.Add(new ReelLampData(System.Drawing.Color.White, false, i + 1));
                expectedReelLampBrightness.Add(i + 1, 100);
            }

            _reelController.Setup(x => x.SetLights(expectedReelLampData.ToArray())).Returns(Task.FromResult(true)).Verifiable();

            // Method being tested
            _device.RenderAllStripData();

            // Default is white color, off state, 100 brightness
            _reelController.Verify(x => x.SetReelBrightness(expectedReelLampBrightness), Times.Once());
        }

        [TestMethod]
        public void CheckForConnectionTest()
        {
            Assert.IsTrue(_device.CheckForConnection());
        }

        [TestMethod]
        public void CheckForConnectionNoReelControllerTest()
        {
            MoqServiceManager.RemoveService<IReelController>();
            _reelController.Setup(x => x.Connected).Returns(true);

            // Method being tested
            Assert.IsFalse(_device.CheckForConnection());
        }

        [TestMethod]
        public void CheckForConnectionNotConnectedTest()
        {
            _reelController.Setup(x => x.Connected).Returns(false);

            // Method being tested
            Assert.IsFalse(_device.CheckForConnection());
        }

        [TestMethod]
        public void SetSystemBrightness100Test()
        {
            // Brightness defaults to -1
            int brightness = 100;

            // Method being tested
            _device.SetSystemBrightness(brightness);

            _reelController.Verify(x => x.SetReelBrightness(brightness), Times.Once());
        }

        [TestMethod]
        public void SetSystemBrightnessTwiceSameValueTest()
        {
            int brightness = 100;

            // Method being tested
            _device.SetSystemBrightness(brightness);
            _device.SetSystemBrightness(brightness);

            _reelController.Verify(x => x.SetReelBrightness(brightness), Times.Once());
        }

        [TestMethod]
        public void SetSystemBrightnessTwiceDifferentValueTest()
        {
            // Method being tested
            _device.SetSystemBrightness(100);
            _device.SetSystemBrightness(50);

            _reelController.Verify(x => x.SetReelBrightness(100), Times.Once());
            _reelController.Verify(x => x.SetReelBrightness(50), Times.Once());
        }

        private void EdgeLightDevice_ConnectionChanged(object sender, EventArgs e)
        {
            _connectionChangedCallbackCalled = true;
        }

        private void EdgeLightDevice_StripsChanged(object sender, EventArgs e)
        {
            _stripsChangedCallbackCalled = true;
        }
    }
}