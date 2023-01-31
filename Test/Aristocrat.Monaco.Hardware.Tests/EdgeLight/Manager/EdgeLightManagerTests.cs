namespace Aristocrat.Monaco.Hardware.Tests.EdgeLight.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
	using Aristocrat.Monaco.Hardware.Services;
    using Contracts.Cabinet;
    using Contracts.EdgeLighting;
    using Hardware.EdgeLight.Contracts;
    using Hardware.EdgeLight.Manager;
    using Hardware.EdgeLight.Strips;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class EdgeLightManagerTests
    {
        private Mock<ICabinetDetectionService> _cabinetDetectionService;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IEdgeLightDevice> _edgeLightDevice;
        private EdgeLightManager _edgeLightManager;
        private Mock<IEventBus> _eventBus;
        private Mock<ILogicalStripFactory> _logicalStripFactoryMoq;
        private List<Mock<IStrip>> _logicalStripMocks;
        private List<IStrip> _logicalStrips;
        private MockRepository _mockRepository;

        private List<Mock<IStrip>> _physicalStripMocks;

        private List<IStrip> _physicalStrips;
        private PriorityComparer _priorityComparer;
        private StripDataRenderer _stripDataRenderer;
        private Action<DeviceConnectedEvent> _pluginAction;
        private Action<DeviceDisconnectedEvent> _unplugAction;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _physicalStripMocks = new List<Mock<IStrip>>
            {
                _mockRepository.Create<IStrip>(), _mockRepository.Create<IStrip>()
            };
            _logicalStripMocks = new List<Mock<IStrip>>
            {
                _mockRepository.Create<IStrip>(), _mockRepository.Create<IStrip>()
            };

            _priorityComparer = new PriorityComparer();
            _logicalStripFactoryMoq = _mockRepository.Create<ILogicalStripFactory>();
            _edgeLightDevice = _mockRepository.Create<IEdgeLightDevice>();
            _edgeLightDevice.Setup(x => x.SetSystemBrightness(It.IsAny<int>()));
            _stripDataRenderer = new StripDataRenderer(_priorityComparer);

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _cabinetDetectionService = MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Default);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);

            _eventBus.Setup(x => x.Publish(It.IsAny<EdgeLightingConnectedEvent>()));
            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<object>(),
                        It.IsAny<Action<DeviceConnectedEvent>>(),
                        It.IsAny<Predicate<DeviceConnectedEvent>>()))
                .Callback<object, Action<DeviceConnectedEvent>, Predicate<DeviceConnectedEvent>>(
                    (o, action, p) => { _pluginAction = action; });
            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<object>(),
                        It.IsAny<Action<DeviceDisconnectedEvent>>(),
                        It.IsAny<Predicate<DeviceDisconnectedEvent>>()))
                .Callback<object, Action<DeviceDisconnectedEvent>, Predicate<DeviceDisconnectedEvent>>(
                    (o, action, p) => { _unplugAction = action; });
            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<object>(),
                        It.IsAny<Action<PropertyChangedEvent>>(),
                        It.IsAny<Predicate<PropertyChangedEvent>>()))
                .Callback<object, Action<PropertyChangedEvent>, Predicate<PropertyChangedEvent>>(
                    (o, action, p) => { });
        }

        [TestCleanup]
        public void Cleanup()
        {
            _edgeLightManager?.Dispose();
            _eventBus.VerifyAll();
            _mockRepository.VerifyAll();
        }

        [TestMethod]
        public void EdgeLightManagerTest()
        {
            SetupManager(false);
            _edgeLightDevice.Setup(x => x.DevicesInfo)
                .Returns(new[] { new EdgeLightDeviceInfo { DeviceType = ElDeviceType.Cabinet } });
            Assert.IsNotNull(_edgeLightManager);
            Assert.IsFalse(_edgeLightManager.LogicalStrips.Any());
            Assert.IsTrue(_edgeLightManager.DevicesInfo.Any(x=>x.DeviceType == ElDeviceType.Cabinet));
            Assert.AreEqual(
                new EdgeLightingBrightnessLimits
                {
                    MinimumAllowed = EdgeLightingBrightnessLimits.MinimumBrightness,
                    MaximumAllowed = EdgeLightingBrightnessLimits.MaximumBrightness
                },
                _edgeLightManager.GetBrightnessLimits(StripPriority.LowPriority));

            Helper.AssertThrow<ArgumentNullException>(
                () =>
                {
                    using (var unused = new EdgeLightManager(null, null, null, null, null, null, null))
                    {
                    }
                });
            Helper.AssertThrow<ArgumentNullException>(
                () =>
                {
                    using (var unused = new EdgeLightManager(_logicalStripFactoryMoq.Object, null, null, null, null, null, null))
                    {
                    }
                });
            Helper.AssertThrow<ArgumentNullException>(
                () =>
                {
                    using (var unused = new EdgeLightManager(
                        _logicalStripFactoryMoq.Object,
                        _edgeLightDevice.Object,
                        null,
                        null,
                        null,
                        null,
                        null))
                    {
                    }
                });
            Helper.AssertThrow<ArgumentNullException>(
                () =>
                {
                    using (var unused = new EdgeLightManager(
                        _logicalStripFactoryMoq.Object,
                        _edgeLightDevice.Object,
                        _stripDataRenderer,
                        null,
                        null,
                        null,
                        null))
                    {
                    }
                });
            Helper.AssertThrow<ArgumentNullException>(
                () =>
                {
                    using (var unused = new EdgeLightManager(
                        _logicalStripFactoryMoq.Object,
                        _edgeLightDevice.Object,
                        _stripDataRenderer,
                        _priorityComparer,
                        null,
                        null,
                        null))
                    {
                    }
                });
        }

        [TestMethod]
        public void EdgeLightManagerWithLogicalStripsTest()
        {
            SetupManager(true);
            _edgeLightDevice.Setup(x => x.DevicesInfo)
                .Returns(new[] { new EdgeLightDeviceInfo { DeviceType = ElDeviceType.Cabinet } });
            Assert.IsNotNull(_edgeLightManager);
            Assert.IsTrue(
                _edgeLightManager.LogicalStrips.Select(x => (x.StripId, x.LedCount))
                    .SequenceEqual(_logicalStrips.Select(x => (x.StripId, x.LedCount))));

            Assert.IsFalse(_edgeLightManager.DevicesInfo.Any(x => x.DeviceType != ElDeviceType.Cabinet));
            Assert.AreEqual(
                new EdgeLightingBrightnessLimits
                {
                    MinimumAllowed = EdgeLightingBrightnessLimits.MinimumBrightness,
                    MaximumAllowed = EdgeLightingBrightnessLimits.MaximumBrightness
                },
                _edgeLightManager.GetBrightnessLimits(StripPriority.LowPriority));
        }

        [TestMethod]
        public void SetStripColorTest()
        {
            SetupManager(true);
            Assert.IsTrue(
                _edgeLightManager.SetStripColor(_logicalStrips.First().StripId, Color.Aqua, StripPriority.LowPriority));
            var stripColors = new[] { (Strip: _logicalStrips.First().StripId, Color: Color.Aqua) }.Concat(
                _logicalStrips.Select(x => (Strip: x.StripId, Color: Color.Blue)).Skip(1)).ToList();
            VerifyRenderedColor(stripColors);

            // Set different strip different priority
            Assert.IsTrue(
                _edgeLightManager.SetStripColor(_logicalStrips.Last().StripId, Color.Red, StripPriority.GamePriority));
            var stripColors2 = new[]
                {
                    (Strip: _logicalStrips.First().StripId, Color: Color.Aqua),
                    (Strip: _logicalStrips.Last().StripId, Color: Color.Red)
                }.Concat(
                    _logicalStrips.Select(x => (Strip: x.StripId, Color: Color.Blue)).Skip(1)
                        .Take(_logicalStrips.Count - 2))
                .ToList();
            VerifyRenderedColor(stripColors2);

            // Set same strip lower priority
            Assert.IsTrue(
                _edgeLightManager.SetStripColor(
                    _logicalStrips.Last().StripId,
                    Color.Yellow,
                    StripPriority.LowPriority));
            VerifyRenderedColor(stripColors2);

            // Set same strip higher priority
            Assert.IsTrue(
                _edgeLightManager.SetStripColor(_logicalStrips.Last().StripId, Color.Yellow, StripPriority.AuditMenu));
            var stripColors3 = new[]
                {
                    (Strip: _logicalStrips.First().StripId, Color: Color.Aqua),
                    (Strip: _logicalStrips.Last().StripId, Color: Color.Yellow)
                }.Concat(
                    _logicalStrips.Select(x => (Strip: x.StripId, Color: Color.Blue)).Skip(1)
                        .Take(_logicalStrips.Count - 2))
                .ToList();
            VerifyRenderedColor(stripColors3);

            // clear higher priority.
            Assert.IsTrue(
                _edgeLightManager.ClearStripForPriority(_logicalStrips.Last().StripId, StripPriority.AuditMenu));

            // Validate invalid strip
            Assert.IsFalse(_edgeLightManager.SetStripColor(122000, Color.Green, StripPriority.Absolute));
            stripColors2.Append((122000, Color.FromArgb(0, 0, 0, 0)));
            VerifyRenderedColor(stripColors2);
            Assert.IsFalse(_edgeLightManager.ClearStripForPriority(122000, StripPriority.Absolute));
        }

        [TestMethod]
        public void SetGlobalBrightnessTest()
        {
            void Verify(
                StripPriority priority,
                int brightness,
                int expectedBrightness)
            {
                SetupAllStripBrightness(expectedBrightness);

                _edgeLightManager.SetBrightnessForPriority(brightness, priority);
                _physicalStripMocks.ForEach(x => x.VerifyAll());
            }

            SetupManager(true);
            Verify(StripPriority.GamePriority, 10, 10);
            Verify(StripPriority.LowPriority, 20, 10);
            Verify(StripPriority.AuditMenu, 200, 100);

            SetupAllStripBrightness(10);
            _edgeLightManager.ClearBrightnessForPriority(StripPriority.AuditMenu);
        }

        [TestMethod]
        public void SetBrightnessLimitsChangeTest()
        {
            void Verify(
                EdgeLightingBrightnessLimits limit,
                StripPriority priority,
                int brightness,
                int expectedBrightnessBefore,
                int expectedBrightnessAfter)
            {
                SetupAllStripBrightness(expectedBrightnessBefore);
                _edgeLightManager.SetBrightnessForPriority(brightness, priority);
                _physicalStripMocks.ForEach(x => x.VerifyAll());
                SetupAllStripBrightness(expectedBrightnessAfter);

                _edgeLightManager.SetBrightnessLimits(limit, priority);
                _physicalStripMocks.ForEach(x => x.VerifyAll());
            }

            SetupManager(true);
            // Change limits and check if already set brightness changes.
            var newLimit = new EdgeLightingBrightnessLimits { MinimumAllowed = 10, MaximumAllowed = 50 };
            Verify(newLimit, StripPriority.GamePriority, 100, 100, newLimit.MaximumAllowed);

            var oldLimit = newLimit;
            newLimit = new EdgeLightingBrightnessLimits { MinimumAllowed = 10, MaximumAllowed = 80 };
            Verify(
                newLimit,
                StripPriority.GamePriority,
                60,
                ChangeBrightnessRange(60, oldLimit),
                ChangeBrightnessRange(60, newLimit));

            oldLimit = newLimit;
            newLimit = new EdgeLightingBrightnessLimits { MinimumAllowed = 70, MaximumAllowed = 80 };
            Verify(newLimit, StripPriority.GamePriority, 0, oldLimit.MinimumAllowed, newLimit.MinimumAllowed);
            Verify(
                newLimit,
                StripPriority.GamePriority,
                50,
                ChangeBrightnessRange(50, newLimit),
                ChangeBrightnessRange(50, newLimit));
        }

        [TestMethod]
        public void SetBrightnessLimitsTest()
        {
            void Verify(
                EdgeLightingBrightnessLimits limit,
                StripPriority priority,
                int brightness,
                int expectedBrightness,
                bool setLimit = true)
            {
                if (setLimit)
                {
                    _edgeLightManager.SetBrightnessLimits(limit, priority);
                }

                SetupAllStripBrightness(expectedBrightness);

                _edgeLightManager.SetBrightnessForPriority(brightness, priority);
                _physicalStripMocks.ForEach(x => x.VerifyAll());
            }

            SetupManager(true);
            _physicalStripMocks.ForEach(x => x.VerifyAll());
            var newLimit = new EdgeLightingBrightnessLimits { MinimumAllowed = 50, MaximumAllowed = 80 };
            Verify(newLimit, StripPriority.GamePriority, 0, newLimit.MinimumAllowed);
            Verify(newLimit, StripPriority.GamePriority, 12, ChangeBrightnessRange(12, newLimit));

            Verify(newLimit, StripPriority.GamePriority, 300, newLimit.MaximumAllowed);
            _edgeLightManager.SetBrightnessForPriority(12, StripPriority.LowPriority);

            Verify(newLimit, StripPriority.AuditMenu, 33, 33, false);

            var brightnessLimits = _edgeLightManager.GetBrightnessLimits(StripPriority.AuditMenu);
            Assert.AreEqual(100, brightnessLimits.MaximumAllowed);
            Assert.AreEqual(0, brightnessLimits.MinimumAllowed);
            brightnessLimits = _edgeLightManager.GetBrightnessLimits(StripPriority.GamePriority);
            Assert.AreEqual(newLimit.MaximumAllowed, brightnessLimits.MaximumAllowed);
            Assert.AreEqual(newLimit.MinimumAllowed, brightnessLimits.MinimumAllowed);
        }

        [TestMethod]
        public void SetStripColorsTest()
        {
            SetupManager(true);
            var strip = _logicalStrips.First();
            var buffer = new LedColorBuffer(strip.LedCount);
            buffer.SetColor(0, Color.Green, buffer.Count);
            Assert.IsTrue(_edgeLightManager.SetStripColors(strip.StripId, buffer, 0, StripPriority.GamePriority));
            VerifyRenderedColor(
                new List<(int strip, IEnumerable<Color> color)>
                {
                    (strip.StripId, Enumerable.Repeat(Color.Green, strip.LedCount))
                });

            buffer.SetColor(0, Color.Red, buffer.Count);
            Assert.IsTrue(_edgeLightManager.SetStripColors(strip.StripId, buffer, 20, StripPriority.GamePriority));
            VerifyRenderedColor(
                new List<(int strip, IEnumerable<Color> color)>
                {
                    (strip.StripId,
                        Enumerable.Repeat(Color.Green, 20)
                            .Concat(Enumerable.Repeat(Color.Red, strip.LedCount - 20)))
                });

            buffer.SetColor(0, Color.Blue, buffer.Count);
            Assert.IsTrue(
                _edgeLightManager.SetStripColors(strip.StripId, buffer, strip.LedCount, StripPriority.GamePriority));
            VerifyRenderedColor(
                new List<(int strip, IEnumerable<Color> color)>
                {
                    (strip.StripId,
                        Enumerable.Repeat(Color.Green, 20)
                            .Concat(Enumerable.Repeat(Color.Red, strip.LedCount - 20)))
                });

            buffer.SetColor(0, Color.Red, buffer.Count);
            buffer.SetColor(20, Color.Blue, buffer.Count - 20);
            Assert.IsTrue(_edgeLightManager.SetStripColors(strip.StripId, buffer, 0, StripPriority.GamePriority));
            VerifyRenderedColor(new List<(int strip, IEnumerable<Color> color)> { (strip.StripId, buffer) });

            //Validate invalid strip id.
            buffer.SetColor(0, Color.Green, buffer.Count);
            Assert.IsFalse(_edgeLightManager.SetStripColors(123000, buffer, 0, StripPriority.GamePriority));
            VerifyRenderedColor(
                new List<(int strip, IEnumerable<Color> color)>
                {
                    (123000, Enumerable.Repeat(Color.FromArgb(0, 0, 0, 0), strip.LedCount))
                });
        }

        [TestMethod]
        public void RenderAllStripDataTest()
        {
            SetupManager(true);
            _logicalStripMocks.ForEach(
                x => x.Setup(y => y.SetColors(It.IsAny<LedColorBuffer>(), 0, 24, 0)));
            _edgeLightDevice.Setup(x => x.RenderAllStripData());
            _edgeLightManager.RenderAllStripData();
            _edgeLightManager.RenderAllStripData();
        }

        [TestMethod]
        public void SetPriorityComparerTest()
        {
            SetupManager(true);
            SetupAllStripBrightness(20);
            _edgeLightManager.SetBrightnessForPriority(20, StripPriority.AuditMenu);
            _edgeLightManager.SetBrightnessForPriority(10, StripPriority.GamePriority);

            var strip = _logicalStrips.First();
            _edgeLightManager.SetStripColor(strip.StripId, Color.Green, StripPriority.AuditMenu);
            _edgeLightManager.SetStripColor(strip.StripId, Color.Red, StripPriority.GamePriority);
            VerifyRenderedColor(new List<(int strip, Color color)> { (strip.StripId, Color.Green) });
            SetupAllStripBrightness(10);
            var comparer = new TestPriorityComparer(
                new List<StripPriority> { StripPriority.GamePriority, StripPriority.AuditMenu });
            _edgeLightManager.SetPriorityComparer(comparer);
            VerifyRenderedColor(new List<(int strip, Color color)> { (strip.StripId, Color.Red) });
        }

        [TestMethod]
        public void SetPowerModeTest()
        {
            SetupManager(true);
            _edgeLightDevice.SetupSet(x => x.LowPowerMode = true);
            _edgeLightManager.PowerMode = true;
            _edgeLightDevice.SetupSet(x => x.LowPowerMode = false);
            _edgeLightManager.PowerMode = false;
        }

        [TestMethod]
        public void SetStripBrightnessForPriorityTest()
        {
            SetupManager(true);
            var mock = _logicalStripMocks.First();
            var stripId = mock.Object.StripId;
            _edgeLightManager.SetStripBrightnessForPriority(12322, 33, StripPriority.AuditMenu); // Invalid strip
            _edgeLightManager.ClearStripBrightnessForPriority(12322, StripPriority.AuditMenu); // Invalid strip
            mock.SetupSet(x => x.Brightness = 10);
            _edgeLightManager.SetStripBrightnessForPriority(stripId, 10, StripPriority.GamePriority);
            mock.VerifySet(x => x.Brightness = 10, Times.AtLeastOnce);
            mock.SetupSet(x => x.Brightness = 33);
            _edgeLightManager.SetStripBrightnessForPriority(stripId, 33, StripPriority.AuditMenu);
            mock.VerifySet(x => x.Brightness = 33, Times.AtLeastOnce);
            mock.SetupSet(x => x.Brightness = 10);
            _edgeLightManager.ClearStripBrightnessForPriority(stripId, StripPriority.AuditMenu);
            mock.VerifySet(x => x.Brightness = 10, Times.AtLeastOnce);
        }

        [TestMethod]
        public void PlugUnplugTest()
        {
            SetupManager(true);
            _edgeLightDevice.Setup(x => x.CheckForConnection()).Returns(false);
            _eventBus.Setup(x => x.Publish(It.IsAny<EdgeLightingDisconnectedEvent>()));
            _pluginAction.Invoke(new DeviceConnectedEvent(null));
            _eventBus.Verify(x => x.Publish(It.IsAny<EdgeLightingDisconnectedEvent>()), Times.Once);
            _unplugAction.Invoke(new DeviceDisconnectedEvent(null));
            _unplugAction.Invoke(new DeviceDisconnectedEvent(null));
            _eventBus.Verify(x => x.Publish(It.IsAny<EdgeLightingConnectedEvent>()), Times.Once);
            _eventBus.Setup(x => x.Publish(It.IsAny<EdgeLightingConnectedEvent>()));
            _edgeLightDevice.Setup(x => x.CheckForConnection()).Returns(true);
            _pluginAction.Invoke(new DeviceConnectedEvent(null));
            _pluginAction.Invoke(new DeviceConnectedEvent(null));
            _eventBus.Verify(x => x.Publish(It.IsAny<EdgeLightingConnectedEvent>()), Times.Exactly(2));
        }

        [TestMethod]
        public void ClearColorsOnStripChangedTest()
        {
            SetupManager(true);

            var strip = _logicalStrips.First();
            var buffer = new LedColorBuffer(strip.LedCount);
            buffer.SetColor(0, Color.Green, buffer.Count);
            Assert.IsTrue(_edgeLightManager.SetStripColors(strip.StripId, buffer, 0, StripPriority.GamePriority));
            _edgeLightDevice.Raise(x => x.StripsChanged += null, EventArgs.Empty);
            Assert.IsTrue(_stripDataRenderer.RenderedData(strip.StripId).All(x => x.ToArgb() == Color.Blue.ToArgb()));
        }

        private static int ChangeBrightnessRange(int oldValue, EdgeLightingBrightnessLimits newLimit)
        {
            var newValue = (int)((newLimit.MaximumAllowed - newLimit.MinimumAllowed) /
                                 100.0 * oldValue + newLimit.MinimumAllowed);
            return newValue;
        }

        private void SetupLogicalStrips()
        {
            _logicalStrips = _logicalStripMocks.Select(x => x.Object).ToList();
            _logicalStripFactoryMoq.Setup(x => x.GetLogicalStrips(_physicalStrips)).Returns(_logicalStrips);
            var stripId = 0;
            foreach (var logicalStripMock in _logicalStripMocks)
            {
                var id = stripId++;
                logicalStripMock.SetupGet(x => x.StripId).Returns(id);
                logicalStripMock.SetupGet(x => x.LedCount).Returns(24);
            }
        }

        private void SetupManager(bool setupLogicalStrips)
        {
            SetupDevice(setupLogicalStrips);
            if (setupLogicalStrips)
            {
                SetupAllStripBrightness(100);
                SetupLogicalStrips();
            }

            _edgeLightManager = new EdgeLightManager(
                _logicalStripFactoryMoq.Object,
                _edgeLightDevice.Object,
                _stripDataRenderer,
                _priorityComparer,
                _eventBus.Object,
                _cabinetDetectionService.Object,
                _propertiesManager.Object);
            _physicalStripMocks.ForEach(x => x.VerifyAll());
            _eventBus.Setup(x => x.UnsubscribeAll(_edgeLightManager));
        }

        private void SetupDevice(bool raiseStripChanged)
        {
            _physicalStrips = _physicalStripMocks.Select(x => x.Object).ToList();
            _edgeLightDevice.Setup(x => x.CheckForConnection()).Returns(
                () =>
                {
                    if (!raiseStripChanged)
                    {
                        return true;
                    }

                    _edgeLightDevice.SetupGet(x => x.PhysicalStrips)
                        .Returns(_physicalStrips);
                    _eventBus.Setup(x => x.Publish(It.IsAny<EdgeLightingStripsChangedEvent>()));
                    _edgeLightDevice.Raise(x => x.StripsChanged += null, EventArgs.Empty);
                    return true;
                });
            _edgeLightDevice.Setup(x => x.Dispose());
        }

        private void VerifyRenderedColor(List<(int strip, Color color)> stripColors)
        {
            stripColors.ForEach(
                x => Assert.IsTrue(
                    _stripDataRenderer.RenderedData(x.strip).All(color => color.ToArgb() == x.color.ToArgb())));
        }

        private void VerifyRenderedColor(List<(int strip, IEnumerable<Color> colors)> stripColors)
        {
            var index = 0;
            stripColors.ForEach(
                x => Assert.IsTrue(
                    _stripDataRenderer.RenderedData(x.strip).Take(stripColors.Count).All(
                        color => color.ToArgb() == x.colors.ElementAt(index++).ToArgb())));
        }

        private void SetupAllStripBrightness(int expectedBrightness)
        {
            _logicalStripMocks.ForEach(
                x =>
                {
                    x.SetupGet(y => y.Brightness).Returns(0).Verifiable();
                    x.SetupSet(y => y.Brightness = expectedBrightness).Verifiable();
                });
        }
    }
}