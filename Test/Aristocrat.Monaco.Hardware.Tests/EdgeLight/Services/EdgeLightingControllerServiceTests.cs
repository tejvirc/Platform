namespace Aristocrat.Monaco.Hardware.Tests.EdgeLight.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Contracts;
    using Contracts.EdgeLighting;
    using Hardware.EdgeLight.Contracts;
    using Hardware.EdgeLight.Services;
    using Kernel;
    using Manager;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using NativeUsb.Hid;
    using Test.Common;

    internal class TestEdgeLightRendererFactory : IEdgeLightRendererFactory
    {
        public TestEdgeLightRendererFactory()
        {
            RenderMock.Setup(x => x.Setup(new Mock<IEdgeLightManager>(MockBehavior.Strict).Object));
            RenderMock.Setup(x => x.Clear());
            RenderMock.Setup(x => x.Update());
        }

        public Mock<IEdgeLightRenderer> RenderMock { get; } = new(MockBehavior.Default);

        public IEdgeLightRenderer CreateRenderer<TParametersType>(TParametersType parameters)
        {
            return RenderMock.Object;
        }
    }

    [TestClass]
    public class EdgeLightingControllerServiceTests
    {
        private readonly List<Mock<IEdgeLightRenderer>> _rendererList = new();
        private readonly TestEdgeLightRendererFactory _rendererFactory = new();
        private EdgeLightingControllerService _controllerService;
        private Mock<IEdgeLightManager> _edgeLightManagerMock;
        private Mock<IEventBus> _eventBus;
        private Mock<IPropertiesManager> _propertiesManager;
        private Action<EdgeLightingStripsChangedEvent> _stripChangedHandler;
        private int _timerTicks;

        [TestInitialize]
        public void Setup()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _edgeLightManagerMock = new Mock<IEdgeLightManager>(MockBehavior.Strict);

            _propertiesManager.Setup(x => x.GetProperty(HardwareConstants.SimulateEdgeLighting, It.IsAny<object>()))
                .Returns(false);

            _rendererList.Add(_rendererFactory.RenderMock);
            _controllerService = new EdgeLightingControllerService(
                _eventBus.Object,
                _propertiesManager.Object,
                _edgeLightManagerMock.Object,
                _rendererFactory,
                EnumerateDevices(),
                _rendererList.Select(x => x.Object)
            );
            _edgeLightManagerMock.Setup(x => x.Dispose());
            _timerTicks = 0;
            _edgeLightManagerMock.SetupGet(x => x.LogicalStrips).Returns(new List<StripData>());
            _edgeLightManagerMock.Setup(x => x.RenderAllStripData()).Callback(() => _timerTicks++);
            _eventBus.Setup(
                    x => x.Subscribe(
                        _controllerService,
                        It.IsAny<Action<EdgeLightingStripsChangedEvent>>()))
                .Callback<object, Action<EdgeLightingStripsChangedEvent>>((_, x) => _stripChangedHandler = x);
            _eventBus.Setup(x => x.UnsubscribeAll(_controllerService));
            _controllerService.Initialize();
        }

        [TestCleanup]
        public void CleanUp()
        {
            var maxTries = 20;
            while (_timerTicks == 0 && maxTries > 0)
            {
                Thread.Sleep(10);
                maxTries--;
            }

            _controllerService.Dispose();
        }

        [TestMethod]
        public void EdgeLightingControllerServiceTest()
        {
            Assert.AreEqual(nameof(EdgeLightingControllerService), _controllerService.Name);
            Assert.AreEqual(typeof(IEdgeLightingController), _controllerService.ServiceTypes.First());
            _edgeLightManagerMock.Setup(x => x.DevicesInfo)
                .Returns(new List<EdgeLightDeviceInfo> { new() { DeviceType = ElDeviceType.Cabinet } });
            var stripDataList = new List<StripData> { new() { LedCount = 40, StripId = 1 } };
            _edgeLightManagerMock.SetupGet(x => x.LogicalStrips)
                .Returns(stripDataList);
            Assert.AreEqual(ElDeviceType.Cabinet, _controllerService.Devices.Select(x => x.DeviceType).First());
            Assert.IsTrue(_controllerService.StripIds.SequenceEqual(stripDataList.Select(x => x.StripId)));
        }

        [TestMethod]
        public void DisposeTest()
        {
            Helper.AssertNoThrow<Exception>(() => _controllerService.Dispose());
        }

        [TestMethod]
        public void SetBrightnessLimitsTest()
        {
            var testLimit = new EdgeLightingBrightnessLimits();
            _edgeLightManagerMock.Setup(x => x.SetBrightnessLimits(testLimit, StripPriority.AuditMenu));
            _controllerService.SetBrightnessLimits(testLimit, StripPriority.AuditMenu);
        }

        [TestMethod]
        public void GetBrightnessLimitsTest()
        {
            var testLimit = new EdgeLightingBrightnessLimits();
            _edgeLightManagerMock.Setup(x => x.GetBrightnessLimits(StripPriority.AuditMenu)).Returns(testLimit);
            Assert.AreEqual(testLimit, _controllerService.GetBrightnessLimits(StripPriority.AuditMenu));
        }

        [TestMethod]
        public void GetStripLedCountTest()
        {
            var stripDataList = new List<StripData>
            {
                new() { LedCount = 40, StripId = 1 }, new() { LedCount = 140, StripId = 11 }
            };
            _edgeLightManagerMock.SetupGet(x => x.LogicalStrips)
                .Returns(stripDataList);
            Assert.AreEqual(140, _controllerService.GetStripLedCount(11));
            Assert.AreEqual(40, _controllerService.GetStripLedCount(1));
            Assert.AreEqual(0, _controllerService.GetStripLedCount(-1));
            Assert.AreEqual(0, _controllerService.GetStripLedCount(0));
            Assert.AreEqual(0, _controllerService.GetStripLedCount(1000));
        }

        [TestMethod]
        public void SetBrightnessForPriorityTest()
        {
            _edgeLightManagerMock.Setup(x => x.SetBrightnessForPriority(200, StripPriority.AuditMenu));
            _controllerService.SetBrightnessForPriority(200, StripPriority.AuditMenu);
        }

        [TestMethod]
        public void ClearBrightnessForPriorityTest()
        {
            _edgeLightManagerMock.Setup(x => x.ClearBrightnessForPriority(StripPriority.AuditMenu));
            _controllerService.ClearBrightnessForPriority(StripPriority.AuditMenu);
        }

        [TestMethod]
        public void AddEdgeLightRendererTest()
        {
            Assert.AreSame(
                _controllerService.AddEdgeLightRenderer(new BlinkPatternParameters()),
                _rendererList.Last().Object);
            _rendererList.Last().Verify(x => x.Setup(_edgeLightManagerMock.Object), Times.AtLeast(1));
            _controllerService.RemoveEdgeLightRenderer(_rendererList.Last().Object);
            _rendererList.Last().Verify(x => x.Clear(), Times.Exactly(1));
        }

        [TestMethod]
        public void SetPriorityComparerTest()
        {
            var comparer = new TestPriorityComparer(
                new List<StripPriority> { StripPriority.AuditMenu, StripPriority.Absolute });
            _edgeLightManagerMock.Setup(x => x.SetPriorityComparer(comparer));
            _controllerService.SetPriorityComparer(comparer);
        }

        [TestMethod]
        public void StripsChangedEvent()
        {
            Assert.AreSame(
                _controllerService.AddEdgeLightRenderer(new BlinkPatternParameters()),
                _rendererList.Last().Object);
            _stripChangedHandler?.Invoke(new EdgeLightingStripsChangedEvent());
            _rendererList.Last().Verify(x => x.Setup(_edgeLightManagerMock.Object), Times.AtLeast(2));
        }

        [TestMethod]
        public void SetStripBrightnessForPriorityTest()
        {
            _edgeLightManagerMock.Setup(x => x.SetStripBrightnessForPriority(10, 11, StripPriority.AuditMenu))
                .Verifiable();
            _edgeLightManagerMock.Setup(x => x.ClearStripBrightnessForPriority(10, StripPriority.AuditMenu))
                .Verifiable();
            _controllerService.SetStripBrightnessForPriority(10, 11, StripPriority.AuditMenu);
            _controllerService.ClearStripBrightnessForPriority(10, StripPriority.AuditMenu);
        }

        private IEnumerable<IHidDevice> EnumerateDevices()
        {
            return new List<IHidDevice>();
        }
    }
}