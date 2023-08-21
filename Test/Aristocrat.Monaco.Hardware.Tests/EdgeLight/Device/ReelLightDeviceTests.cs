namespace Aristocrat.Monaco.Hardware.Tests.EdgeLight.Device
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Capabilities;
    using Hardware.Contracts.Reel.ControlData;
    using Hardware.Contracts.Reel.Events;
    using Hardware.EdgeLight.Device;
    using Hardware.EdgeLight.Strips;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

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
        private bool _connectionChangedCallbackCalled;
        private bool _stripsChangedCallbackCalled;

        [TestInitialize]
        public void Setup()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _reelController = MoqServiceManager.CreateAndAddService<IReelController>(MockBehavior.Default);

            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Func<ConnectedEvent, CancellationToken, Task>>()))
                .Callback<object, Func<ConnectedEvent, CancellationToken, Task>>((_, c) => _connectedAction = c);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Func<DisconnectedEvent, CancellationToken, Task>>()))
                .Callback<object, Func<DisconnectedEvent, CancellationToken, Task>>((_, c) => _disconnectedAction = c);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Func<ReelConnectedEvent, CancellationToken, Task>>()))
                .Callback<object, Func<ReelConnectedEvent, CancellationToken, Task>>((_, c) => _reelConnectedAction = c);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Func<ReelDisconnectedEvent, CancellationToken, Task>>()))
                .Callback<object, Func<ReelDisconnectedEvent, CancellationToken, Task>>((_, c) => _reelDisconnectedAction = c);

            _device = new ReelLightDevice(_eventBus.Object);

            IList<int> ids = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

            _reelController.Setup(x => x.HasCapability<IReelLightingCapabilities>()).Returns(true);
            _reelController.Setup(x => x.HasCapability<IReelBrightnessCapabilities>()).Returns(true);
            _reelController.Setup(x => x.GetCapability<IReelLightingCapabilities>().GetReelLightIdentifiers()).Returns(Task.FromResult(ids));
            _reelController.Setup(x => x.GetCapability<IReelBrightnessCapabilities>().DefaultReelBrightness).Returns(100);
            _reelController.Setup(x => x.ConnectedReels).Returns(new List<int> { 1, 2, 3, 4, 5 });
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
        public void DisposeTest()
        {
            // Method being tested
            _device.Dispose();

            _eventBus.Verify(x => x.UnsubscribeAll(_device), Times.Once());
        }

        [TestMethod]
        public async Task HandleConnectionChangedConnectedTest()
        {
            const int reelControllerId = 1;

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
            const int reelControllerId = 1;

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
            const int reelId = 1;

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
            const int reelId = 1;

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
                _device.PhysicalStrips[i].ColorBuffer.SetColors(
                    new LedColorBuffer(new[] { Color.White, Color.White, Color.White }),
                    0,
                    3,
                    0);
            }

            var expectedReelLampData = new List<ReelLampData>();
            var expectedReelLampBrightness = new Dictionary<int, int>();
            for (var i = 0; i < 15; ++i)
            {
                expectedReelLampData.Add(new ReelLampData(Color.White, true, i + 1));
                expectedReelLampBrightness.Add(i + 1, 100);
            }

            _reelController.Setup(
                    x => x.GetCapability<IReelLightingCapabilities>().SetLights(
                        It.Is<ReelLampData[]>(l => expectedReelLampData.SequenceEqual(l, new ReelLampDataComparer()))))
                .ReturnsAsync(true);

            // Method being tested
            _device.RenderAllStripData();

            // Default is white color, off state, 100 brightness
            _reelController.Verify(x => x.GetCapability<IReelBrightnessCapabilities>().SetBrightness(expectedReelLampBrightness), Times.Once());
        }

        [TestMethod]
        public async Task CheckForConnectionTest()
        {
            Assert.IsNotNull(_connectedAction);
            await _reelConnectedAction(new ReelConnectedEvent(1), CancellationToken.None);
            Assert.IsFalse(_connectionChangedCallbackCalled);
            Assert.IsTrue(_stripsChangedCallbackCalled);
            Assert.AreEqual(5, _device.PhysicalStrips.Count);

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
        public async Task SetSystemBrightness100Test()
        {
            Assert.IsNotNull(_connectedAction);
            await _reelConnectedAction(new ReelConnectedEvent(1), CancellationToken.None);
            Assert.IsFalse(_connectionChangedCallbackCalled);
            Assert.IsTrue(_stripsChangedCallbackCalled);
            Assert.AreEqual(5, _device.PhysicalStrips.Count);
            _reelController.Setup(x => x.GetCapability<IReelBrightnessCapabilities>().SetBrightness(It.IsAny<int>())).ReturnsAsync(true);

            // Brightness defaults to -1
            const int brightness = 100;

            // Method being tested
            _device.SetSystemBrightness(brightness);
            _device.RenderAllStripData();

            _reelController.Verify(x => x.GetCapability<IReelBrightnessCapabilities>().SetBrightness(brightness), Times.Once());
        }

        [TestMethod]
        public async Task SetSystemBrightnessTwiceSameValueTest()
        {
            Assert.IsNotNull(_connectedAction);
            await _reelConnectedAction(new ReelConnectedEvent(1), CancellationToken.None);
            Assert.IsFalse(_connectionChangedCallbackCalled);
            Assert.IsTrue(_stripsChangedCallbackCalled);
            Assert.AreEqual(5, _device.PhysicalStrips.Count);

            const int brightness = 100;
            _reelController.Setup(x => x.GetCapability<IReelBrightnessCapabilities>().SetBrightness(It.IsAny<int>())).ReturnsAsync(true);

            // Method being tested
            _device.SetSystemBrightness(brightness);
            _device.RenderAllStripData();

            _device.SetSystemBrightness(brightness);
            _device.RenderAllStripData();

            _reelController.Verify(x => x.GetCapability<IReelBrightnessCapabilities>().SetBrightness(brightness), Times.Once());
        }

        private void EdgeLightDevice_ConnectionChanged(object sender, EventArgs e)
        {
            _connectionChangedCallbackCalled = true;
        }

        private void EdgeLightDevice_StripsChanged(object sender, EventArgs e)
        {
            _stripsChangedCallbackCalled = true;
        }

        private class ReelLampDataComparer : IEqualityComparer<ReelLampData>
        {
            public bool Equals(ReelLampData x, ReelLampData y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (ReferenceEquals(x, null))
                {
                    return false;
                }

                if (ReferenceEquals(y, null))
                {
                    return false;
                }

                if (x.GetType() != y.GetType())
                {
                    return false;
                }

                return x.Color.ToArgb() == y.Color.ToArgb() && x.IsLampOn == y.IsLampOn && x.Id == y.Id;
            }

            public int GetHashCode(ReelLampData obj)
            {
                unchecked
                {
                    var hashCode = obj.Color.ToArgb().GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.IsLampOn.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.Id;
                    return hashCode;
                }
            }
        }
    }
}