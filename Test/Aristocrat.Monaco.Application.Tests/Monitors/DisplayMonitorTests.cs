namespace Aristocrat.Monaco.Application.Tests.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Monitors;
    using Aristocrat.Cabinet;
    using Aristocrat.Monaco.Hardware.SerialTouch;
    using Aristocrat.Monaco.Hardware.Services;
    using Cabinet.Contracts;
    using Contracts;
    using Hardware.Contracts.ButtonDeck;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Display;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.Touch;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class DisplayMonitorTests
    {
        private const string LcdButtonDeckDescription = "USBD480";
        private const string BlockName = "Aristocrat.Monaco.Application.Monitors.DisplayMonitor";

        private static readonly string[] TouchScreenMeter =
        {
            ApplicationMeters.MainTouchScreenDisconnectCount,
            ApplicationMeters.TopTouchScreenDisconnectCount,
            ApplicationMeters.VbdTouchScreenDisconnectCount
        };

        private static readonly string[] VideoDisplayMeter =
        {
            ApplicationMeters.MainVideoDisplayDisconnectCount,
            ApplicationMeters.TopVideoDisplayDisconnectCount,
            ApplicationMeters.VbdVideoDisplayDisconnectCount,
            ApplicationMeters.TopperVideoDisplayDisconnectCount
        };

        private readonly List<Mock<IDisplayDevice>> _displayDeviceMocks = new List<Mock<IDisplayDevice>>();
        private readonly List<Mock<ITouchDevice>> _touchDeviceMocks = new List<Mock<ITouchDevice>>();
        private readonly List<Guid> _disabledDevices = new List<Guid> { ApplicationConstants.TouchDisplayDisconnectedLockupKey };

        private Mock<IEventBus> _eventBus;
        private Mock<ISystemDisableManager> _disableManager;
        private Mock<IMeterManager> _meterManager;
        private Mock<IPersistentStorageManager> _persistentStorage;
        private Mock<ICabinetDetectionService> _cabinetDetectionService;
        private Mock<IButtonDeckDisplay> _buttonDeckDisplay;
        private Mock<IPersistentStorageAccessor> _storageAccessorMock;
        private Mock<IPersistentStorageTransaction> _transactionMock;
        private Mock<ISerialTouchService> _serialTouchService;
        private MockRepository _mockRepository;
        private Action<DeviceConnectedEvent> _connectedHandler;
        private Action<DeviceDisconnectedEvent> _disconnectedHandler;
        private Mock<IMeter> _meterMock;

        private IReadOnlyCollection<IDevice> Devices => _displayDeviceMocks.Select(x => x.Object as IDevice)
            .Concat(_touchDeviceMocks.Select(t => t.Object as IDevice)).ToList();

        [TestInitialize]
        public void Initialize()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _eventBus = _mockRepository.Create<IEventBus>();
            _disableManager = _mockRepository.Create<ISystemDisableManager>();
            _meterManager = _mockRepository.Create<IMeterManager>();
            _persistentStorage = _mockRepository.Create<IPersistentStorageManager>();
            _cabinetDetectionService = _mockRepository.Create<ICabinetDetectionService>();
            _buttonDeckDisplay = _mockRepository.Create<IButtonDeckDisplay>();
            _storageAccessorMock = _mockRepository.Create<IPersistentStorageAccessor>();
            _transactionMock = _mockRepository.Create<IPersistentStorageTransaction>(MockBehavior.Loose);
            _serialTouchService = _mockRepository.Create<ISerialTouchService>();
            _meterMock = _mockRepository.Create<IMeter>();
            _displayDeviceMocks.Clear();
            _touchDeviceMocks.Clear();
        }

        [TestCleanup]
        public void Cleanup()
        {
            //_mockRepository.VerifyAll();
        }

        [TestMethod]
        public void DefaultConstructor()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _disableManager = MoqServiceManager.CreateAndAddService<ISystemDisableManager>(MockBehavior.Default);
            _meterManager = MoqServiceManager.CreateAndAddService<IMeterManager>(MockBehavior.Default);
            _persistentStorage = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Default);
            _buttonDeckDisplay = MoqServiceManager.CreateAndAddService<IButtonDeckDisplay>(MockBehavior.Default);
            _cabinetDetectionService =
                MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Default);
            SetCabinetDetectionMock();
            SetupPersistence();
            _buttonDeckDisplay.Setup(x => x.DisplayCount).Returns(2);
            var dm = new DisplayMonitor();
            SetupEventBusSubscription(dm);
            dm.Initialize();
            dm.Dispose();
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void DisplayMonitorNullParameterTest()
        {
            DisplayMonitor dm = null;
            AssertHelper.Throws<ArgumentNullException>(
                () => dm =
                    new DisplayMonitor(
                        null,
                        _disableManager.Object,
                        _meterManager.Object,
                        _persistentStorage.Object,
                        _cabinetDetectionService.Object,
                        _buttonDeckDisplay.Object));
            AssertHelper.Throws<ArgumentNullException>(
                () => dm =
                    new DisplayMonitor(
                        _eventBus.Object,
                        null,
                        _meterManager.Object,
                        _persistentStorage.Object,
                        _cabinetDetectionService.Object,
                        _buttonDeckDisplay.Object));
            AssertHelper.Throws<ArgumentNullException>(
                () => dm =
                    new DisplayMonitor(
                        _eventBus.Object,
                        _disableManager.Object,
                        null,
                        _persistentStorage.Object,
                        _cabinetDetectionService.Object,
                        _buttonDeckDisplay.Object));
            AssertHelper.Throws<ArgumentNullException>(
                () => dm =
                    new DisplayMonitor(
                        _eventBus.Object,
                        _disableManager.Object,
                        _meterManager.Object,
                        null,
                        _cabinetDetectionService.Object,
                        _buttonDeckDisplay.Object));
            AssertHelper.Throws<ArgumentNullException>(
                () => dm =
                    new DisplayMonitor(
                        _eventBus.Object,
                        _disableManager.Object,
                        _meterManager.Object,
                        _persistentStorage.Object,
                        null,
                        _buttonDeckDisplay.Object));
            AssertHelper.Throws<ArgumentNullException>(
                () => dm =
                    new DisplayMonitor(
                        _eventBus.Object,
                        _disableManager.Object,
                        _meterManager.Object,
                        _persistentStorage.Object,
                        _cabinetDetectionService.Object,
                        null));
            Assert.IsNull(dm);
        }

        [TestMethod]
        public void DisplayMonitorTestPersistenceExists()
        {
            SetCabinetDetectionMock(false);
            SetupPersistence();
            _buttonDeckDisplay.Setup(x => x.DisplayCount).Returns(2);
            var dm = new DisplayMonitor(
                _eventBus.Object,
                _disableManager.Object,
                _meterManager.Object,
                _persistentStorage.Object,
                _cabinetDetectionService.Object,
                _buttonDeckDisplay.Object);
            Assert.AreEqual(typeof(DisplayMonitor).Name, dm.Name);
            Assert.IsTrue(dm.ServiceTypes.SequenceEqual(new[] { typeof(DisplayMonitor) }));
            SetupEventBusSubscription(dm);
            dm.Initialize();
            dm.Dispose();
            dm.Dispose(); // No exception after double dispose.
        }

        [TestMethod]
        public void DisplayMonitorTestPersistenceNotExistsWithLcd()
        {
            SetCabinetDetectionMock(false);
            SetupPersistence(false);
            _cabinetDetectionService.Setup(x => x.GetDisplayDeviceByItsRole(DisplayRole.VBD))
                .Returns(null as IDisplayDevice);
            _buttonDeckDisplay.Setup(x => x.DisplayCount).Returns(2);
            var dm = new DisplayMonitor(
                _eventBus.Object,
                _disableManager.Object,
                _meterManager.Object,
                _persistentStorage.Object,
                _cabinetDetectionService.Object,
                _buttonDeckDisplay.Object);
            SetupEventBusSubscription(dm);
            dm.Initialize();
            dm.Dispose();
        }

        [TestMethod]
        public void DisplayMonitorTestPersistenceNotExistsWithVbd()
        {
            SetCabinetDetectionMock(false);
            SetupPersistence(false, false);
            _cabinetDetectionService.Setup(x => x.GetDisplayDeviceByItsRole(DisplayRole.VBD))
                .Returns(_displayDeviceMocks.Last().Object);
            var dm = new DisplayMonitor(
                _eventBus.Object,
                _disableManager.Object,
                _meterManager.Object,
                _persistentStorage.Object,
                _cabinetDetectionService.Object,
                _buttonDeckDisplay.Object);
            SetupEventBusSubscription(dm);
            dm.Initialize();
            dm.Dispose();
        }

        [TestMethod]
        public void DisplayMonitorTestPersistenceNotExistsWithVbdAndLcd()
        {
            SetCabinetDetectionMock(false);
            SetupPersistence(false, false);
            _cabinetDetectionService.Setup(x => x.GetDisplayDeviceByItsRole(DisplayRole.VBD))
                .Returns(_displayDeviceMocks.Last().Object);
            var dm = new DisplayMonitor(
                _eventBus.Object,
                _disableManager.Object,
                _meterManager.Object,
                _persistentStorage.Object,
                _cabinetDetectionService.Object,
                _buttonDeckDisplay.Object);
            SetupEventBusSubscription(dm);
            dm.Initialize();
            dm.Dispose();
        }

        [TestMethod]
        public void DisplayMonitorTestPersistenceNotExistsWithNoVbdNoLcd()
        {
            SetCabinetDetectionMock(false);
            SetupPersistence(false, false, false);
            _cabinetDetectionService.Setup(x => x.GetDisplayDeviceByItsRole(DisplayRole.VBD))
                .Returns(null as IDisplayDevice);
            _buttonDeckDisplay.Setup(x => x.DisplayCount).Returns(1);
            var dm = new DisplayMonitor(
                _eventBus.Object,
                _disableManager.Object,
                _meterManager.Object,
                _persistentStorage.Object,
                _cabinetDetectionService.Object,
                _buttonDeckDisplay.Object);
            SetupEventBusSubscription(dm);
            dm.Initialize();
            dm.Dispose();
        }

        [TestMethod]
        public void BootWithLcdDeviceDisconnected()
        {
            SetCabinetDetectionMock(false);
            SetupPersistence();
            TestBootUpDeviceDisconnect(
                () =>
                {
                    _buttonDeckDisplay.Setup(x => x.DisplayCount).Returns(2);
                    SetupDeviceConnect<ButtonDeckConnectedEvent, IDisplayDevice>(
                        null,
                        ApplicationMeters.PlayerButtonErrorCount,
                        ApplicationConstants.LcdButtonDeckDisconnectedLockupKey);
                    _connectedHandler.Invoke(new DeviceConnectedEvent(new Dictionary<string, object>()));
                },
                () =>
                {
                    _buttonDeckDisplay.Setup(x => x.DisplayCount).Returns(1);
                    SetupDeviceDisconnect<ButtonDeckDisconnectedEvent, IDisplayDevice>(
                        null,
                        ApplicationMeters.PlayerButtonErrorCount,
                        ApplicationConstants.LcdButtonDeckDisconnectedLockupKey);
                }
            );
            _meterMock.Verify(x => x.Increment(1), Times.Exactly(1));
        }

        [TestMethod]
        public void BootWithMainDisplayDisconnected()
        {
            SetCabinetDetectionMock();
            SetupPersistence();
            const int display = 0;
            var device = _displayDeviceMocks.Skip(display).First();
            //device.Setup(x => x.Role).Returns(DisplayRole.Main);
            _buttonDeckDisplay.Setup(x => x.DisplayCount).Returns(2);
            TestBootUpDeviceDisconnect(
                () =>
                {
                    SetupDeviceConnect<DisplayConnectedEvent, IDisplayDevice>(
                        device,
                        VideoDisplayMeter[display],
                        ApplicationConstants.DisplayDisconnectedLockupKey);

                    _connectedHandler.Invoke(new DeviceConnectedEvent(new Dictionary<string, object>()));
                },
                () =>
                {
                    SetupDeviceDisconnect<DisplayDisconnectedEvent, IDisplayDevice>(
                        device,
                        VideoDisplayMeter[display],
                        ApplicationConstants.DisplayDisconnectedLockupKey);
                }
            );
            _meterMock.Verify(x => x.Increment(1), Times.Exactly(1));
        }

        [TestMethod]
        public void BootWithTopTouchDisconnected()
        {
            SetCabinetDetectionMock(false);
            SetupPersistence();
            const int deviceIndex = 1;
            var device = _touchDeviceMocks.Skip(deviceIndex).First();
            _disableManager.Setup(m => m.CurrentDisableKeys).Returns(_disabledDevices);
            _disableManager.Setup(m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null));
            _buttonDeckDisplay.Setup(x => x.DisplayCount).Returns(2);
            TestBootUpDeviceDisconnect(
                () =>
                {
                    SetupDeviceConnect<TouchDisplayConnectedEvent, ITouchDevice>(
                        device,
                        TouchScreenMeter[deviceIndex],
                        ApplicationConstants.TouchDisplayDisconnectedLockupKey);

                    _connectedHandler.Invoke(new DeviceConnectedEvent(new Dictionary<string, object>()));
                },
                () =>
                {
                    SetupDeviceDisconnect<TouchDisplayDisconnectedEvent, ITouchDevice>(
                        device,
                        TouchScreenMeter[deviceIndex],
                        ApplicationConstants.TouchDisplayDisconnectedLockupKey);
                }
            );
            _meterMock.Verify(x => x.Increment(1), Times.Exactly(1));
        }

        [TestMethod]
        public void BootWithVbdDisplayDisconnected()
        {
            SetCabinetDetectionMock();
            SetupPersistence(true, false);
            const int display = 2;
            var device = _displayDeviceMocks.Skip(display).First();
            TestBootUpDeviceDisconnect(
                () =>
                {
                    SetupDeviceConnect<DisplayConnectedEvent, IDisplayDevice>(
                        device,
                        VideoDisplayMeter[display],
                        ApplicationConstants.DisplayDisconnectedLockupKey);
                    SetupDeviceConnect<ButtonDeckConnectedEvent, IDisplayDevice>(
                        null,
                        ApplicationMeters.PlayerButtonErrorCount,
                        ApplicationConstants.LcdButtonDeckDisconnectedLockupKey);
                    _connectedHandler.Invoke(new DeviceConnectedEvent(new Dictionary<string, object>()));
                },
                () =>
                {
                    SetupDeviceDisconnect<DisplayDisconnectedEvent, IDisplayDevice>(
                        device,
                        VideoDisplayMeter[display],
                        ApplicationConstants.DisplayDisconnectedLockupKey);
                    SetupDeviceDisconnect<ButtonDeckDisconnectedEvent, IDisplayDevice>(
                        null,
                        ApplicationMeters.PlayerButtonErrorCount,
                        ApplicationConstants.LcdButtonDeckDisconnectedLockupKey);
                }
            );
            _meterMock.Verify(x => x.Increment(1), Times.Exactly(2));
        }

        [TestMethod]
        public void BootWithLcdDeviceAlreadyDisconnected()
        {
            SetCabinetDetectionMock(false);
            SetupPersistence();
            TestBootUpDeviceDisconnect(
                () =>
                {
                    _buttonDeckDisplay.Setup(x => x.DisplayCount).Returns(2);
                    SetupDeviceConnect<ButtonDeckConnectedEvent, IDisplayDevice>(
                        null,
                        ApplicationMeters.PlayerButtonErrorCount,
                        ApplicationConstants.LcdButtonDeckDisconnectedLockupKey);
                    _connectedHandler.Invoke(new DeviceConnectedEvent(new Dictionary<string, object>()));
                },
                () =>
                {
                    _buttonDeckDisplay.Setup(x => x.DisplayCount).Returns(1);
                    SetupDeviceDisconnect<ButtonDeckDisconnectedEvent, IDisplayDevice>(
                        null,
                        ApplicationMeters.PlayerButtonErrorCount,
                        ApplicationConstants.LcdButtonDeckDisconnectedLockupKey,
                        false);
                }
            );
        }

        [TestMethod]
        public void MultipleDisplayDeviceDisconnect()
        {
            SetCabinetDetectionMock();
            SetupPersistence();
            _buttonDeckDisplay.Setup(x => x.DisplayCount).Returns(2);

            var dm = new DisplayMonitor(
                _eventBus.Object,
                _disableManager.Object,
                _meterManager.Object,
                _persistentStorage.Object,
                _cabinetDetectionService.Object,
                _buttonDeckDisplay.Object);
            SetupEventBusSubscription(dm);
            dm.Initialize();

            VerifyDisconnectDevices<DisplayDisconnectedEvent, IDisplayDevice>(
                _displayDeviceMocks,
                ApplicationConstants.DisplayDisconnectedLockupKey,
                VideoDisplayMeter);

            VerifyConnectDevices<DisplayConnectedEvent, IDisplayDevice>(
                _displayDeviceMocks,
                ApplicationConstants.DisplayDisconnectedLockupKey,
                VideoDisplayMeter);
            _cabinetDetectionService.Verify(x => x.ApplyDisplaySettings(), Times.Exactly(1));
            _cabinetDetectionService.Verify(x => x.MapTouchscreens(false), Times.Exactly(2));
            _meterMock.Verify(x => x.Increment(1), Times.Exactly(_displayDeviceMocks.Count));
            dm.Dispose();
        }

        [TestMethod]
        public void MultipleDevicesDisconnect()
        {
            SetCabinetDetectionMock();
            SetupPersistence();
            _buttonDeckDisplay.Setup(x => x.DisplayCount).Returns(2);
            _disableManager.Setup(m => m.CurrentDisableKeys).Returns(_disabledDevices);
            _disableManager.Setup(m => m.Disable(It.IsAny<Guid>(), SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null));

            var dm = new DisplayMonitor(
                _eventBus.Object,
                _disableManager.Object,
                _meterManager.Object,
                _persistentStorage.Object,
                _cabinetDetectionService.Object,
                _buttonDeckDisplay.Object);
            SetupEventBusSubscription(dm);
            dm.Initialize();
            VerifyDisconnectDevices<DisplayDisconnectedEvent, IDisplayDevice>(
                _displayDeviceMocks,
                ApplicationConstants.DisplayDisconnectedLockupKey,
                VideoDisplayMeter);
            VerifyDisconnectDevices<TouchDisplayDisconnectedEvent, ITouchDevice>(
                _touchDeviceMocks,
                ApplicationConstants.TouchDisplayDisconnectedLockupKey,
                TouchScreenMeter);

            VerifyConnectDevices<DisplayConnectedEvent, IDisplayDevice>(
                _displayDeviceMocks,
                ApplicationConstants.DisplayDisconnectedLockupKey,
                VideoDisplayMeter);
            VerifyConnectDevices<TouchDisplayConnectedEvent, ITouchDevice>(
                _touchDeviceMocks,
                ApplicationConstants.TouchDisplayDisconnectedLockupKey,
                TouchScreenMeter);

            _cabinetDetectionService.Verify(x => x.ApplyDisplaySettings(), Times.Exactly(1));
            _cabinetDetectionService.Verify(x => x.MapTouchscreens(false), Times.Exactly(2));
            _meterMock.Verify(x => x.Increment(1), Times.Exactly(_displayDeviceMocks.Count + _touchDeviceMocks.Count));
            dm.Dispose();
        }

        [TestMethod]
        public void SerialTouchDeviceDisconnect()
        {
            SetupCabinetDetectionMockForSerialTouch();
            SetupPersistence();
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _serialTouchService = MoqServiceManager.CreateAndAddService<ISerialTouchService>(MockBehavior.Default);

            _buttonDeckDisplay.Setup(x => x.DisplayCount).Returns(2);
            _serialTouchService.Setup(x => x.IsDisconnected).Returns(true);
            _disableManager.Setup(m => m.CurrentDisableKeys).Returns(_disabledDevices);

            var dm = new DisplayMonitor(
                _eventBus.Object,
                _disableManager.Object,
                _meterManager.Object,
                _persistentStorage.Object,
                _cabinetDetectionService.Object,
                _buttonDeckDisplay.Object);
            SetupEventBusSubscription(dm);
            dm.Initialize();

            VerifyDisconnectDevices<TouchDisplayDisconnectedEvent, ITouchDevice>(
                _touchDeviceMocks,
                ApplicationConstants.TouchDisplayDisconnectedLockupKey,
                TouchScreenMeter);

            _cabinetDetectionService.Verify(x => x.MapTouchscreens(false), Times.Exactly(1));
            _meterMock.Verify(x => x.Increment(1), Times.Exactly(_touchDeviceMocks.Count));
            dm.Dispose();
        }

        private void VerifyConnectDevices<TEvent, TDevice>(
            List<Mock<TDevice>> deviceMocks,
            Guid lockupKey,
            IReadOnlyList<string> meters) where TDevice : class, IDevice where TEvent : IEvent
        {
            _disableManager.Setup(
                y => y.Enable(lockupKey));
            var index = 0;
            deviceMocks.ForEach(
                x =>
                {
                    if (x is Mock<IDisplayDevice> displayMock)
                    {
                        displayMock.Setup(y => y.Role).Returns(DisplayRole.Main);
                    }

                    SetupDeviceConnectNoEnable<TEvent, TDevice>(
                        x,
                        meters[index++]);
                    _connectedHandler.Invoke(new DeviceConnectedEvent(new Dictionary<string, object>()));
                });
            _disableManager.Verify(
                y => y.Enable(lockupKey),
                Times.Exactly(1));
        }

        private void VerifyDisconnectDevices<TEvent, TDevice>(
            List<Mock<TDevice>> deviceMocks,
            Guid lockupKey,
            IReadOnlyList<string> meters) where TDevice : class, IDevice where TEvent : IEvent
        {
            var displayIndex = 0;
            _disableManager.Setup(
                y => y.Disable(
                    lockupKey,
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    null));
            deviceMocks.ForEach(
                x =>
                {
                    if (x is Mock<IDisplayDevice> displayMock)
                    {
                        displayMock.Setup(y => y.Role).Returns(DisplayRole.Main);
                    }

                    SetupDeviceDisconnectNoDisable<TEvent, TDevice>(
                        x,
                        meters[displayIndex++]);
                    _disconnectedHandler.Invoke(new DeviceDisconnectedEvent(new Dictionary<string, object>()));
                });
            _disableManager.Verify(
                y => y.Disable(
                    lockupKey,
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    null),
                Times.Exactly(1));
        }

        private void TestBootUpDeviceDisconnect(Action connectSetup, Action disconnectSetup)
        {
            var dm = new DisplayMonitor(
                _eventBus.Object,
                _disableManager.Object,
                _meterManager.Object,
                _persistentStorage.Object,
                _cabinetDetectionService.Object,
                _buttonDeckDisplay.Object);
            SetupEventBusSubscription(dm);
            disconnectSetup?.Invoke();
            dm.Initialize();
            connectSetup?.Invoke();
            dm.Dispose();
        }

        private void SetupDeviceDisconnect<TEvent, TDevice>(
            Mock<TDevice> deviceMock,
            string meter,
            Guid disableKey,
            bool previouslyConnected = true)
            where TEvent : IEvent where TDevice : class, IDevice
        {
            SetupDeviceDisconnectNoDisable<TEvent, TDevice>(deviceMock, meter, previouslyConnected);
            _disableManager.Setup(
                x => x.Disable(
                    disableKey,
                    SystemDisablePriority.Immediate,
                    It.IsAny<Func<string>>(),
                    null));
        }

        private void SetupDeviceDisconnectNoDisable<TEvent, TDevice>(
            Mock<TDevice> deviceMock,
            string meter,
            bool previouslyConnected = true)
            where TEvent : IEvent where TDevice : class, IDevice
        {
            deviceMock?.Setup(x => x.Status).Returns(DeviceStatus.Disconnected);
            SetupPersistDeviceStatus(meter, false, previouslyConnected);
            if (previouslyConnected)
            {
                _meterMock.Setup(x => x.Increment(1));
                _meterManager.Setup(x => x.GetMeter(meter)).Returns(_meterMock.Object);
            }

            _eventBus.Setup(x => x.Publish(It.IsAny<TEvent>()));
        }

        private void SetupDeviceConnect<TEvent, TDevice>(Mock<TDevice> deviceMock, string meter, Guid enableKey)
            where TEvent : IEvent where TDevice : class, IDevice
        {
            SetupDeviceConnectNoEnable<TEvent, TDevice>(deviceMock, meter);
            _disableManager.Setup(
                x => x.Enable(enableKey));
        }

        private void SetupDeviceConnectNoEnable<TEvent, TDevice>(Mock<TDevice> deviceMock, string meter)
            where TEvent : IEvent where TDevice : class, IDevice
        {
            SetupPersistDeviceStatus(meter, true, false);
            deviceMock?.Setup(x => x.Status).Returns(DeviceStatus.Connected);
            _eventBus.Setup(x => x.Publish(It.IsAny<TEvent>()));
        }

        private void SetupPersistDeviceStatus(string meter, bool newStatus, bool oldStatus)
        {
            _storageAccessorMock.Setup(x => x.StartTransaction()).Returns(_transactionMock.Object);
            _transactionMock.SetupGet(y => y[meter]).Returns(oldStatus);
            _transactionMock.SetupSet(y => y[meter] = newStatus);
            _transactionMock.Setup(x => x.Commit());
            _transactionMock.Setup(x => x.Dispose());
        }

        private void SetupEventBusSubscription(DisplayMonitor displayMonitor)
        {
            _eventBus.Setup(
                    x => x.Subscribe(
                        displayMonitor,
                        It.IsAny<Action<DeviceConnectedEvent>>(),
                        It.IsAny<Predicate<DeviceConnectedEvent>>()))
                .Callback<object, Action<DeviceConnectedEvent>, Predicate<DeviceConnectedEvent>>(
                    (dm, handler, filter) =>
                    {
                        _connectedHandler = handler;
                        ValidateFilter(filter, x => new DeviceConnectedEvent(x));
                    });
            _eventBus.Setup(
                    x => x.Subscribe(
                        displayMonitor,
                        It.IsAny<Action<DeviceDisconnectedEvent>>(),
                        It.IsAny<Predicate<DeviceDisconnectedEvent>>()))
                .Callback<object, Action<DeviceDisconnectedEvent>, Predicate<DeviceDisconnectedEvent>>(
                    (dm, handler, filter) =>
                    {
                        _disconnectedHandler = handler;
                        ValidateFilter(filter, x => new DeviceDisconnectedEvent(x));
                    });
            _eventBus.Setup(x => x.Subscribe(displayMonitor, It.IsAny<Action<ClearDisplayDisconnectedLockupEvent>>()));
            _eventBus.Setup(x => x.Subscribe(displayMonitor, It.IsAny<Action<SetDisplayDisconnectedLockupEvent>>()));
            _eventBus.Setup(x => x.UnsubscribeAll(displayMonitor));

            void ValidateFilter<T>(Predicate<T> filter, Func<IDictionary<string, object>, T> factory)
                where T : BaseDeviceEvent
            {
                var descriptions = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { { "DeviceCategory", "DISPLAY" } },
                    new Dictionary<string, object> { { "DeviceCategory", "HID" } },
                    new Dictionary<string, object>
                    {
                        { "DeviceCategory", "USB" }, { "DeviceDesc", LcdButtonDeckDescription }
                    },
                    new Dictionary<string, object> { { "DeviceCategory", "USB" }, { "DeviceDesc", "Invalid" } },
                    new Dictionary<string, object> { { "DeviceCategory", "USB3" }, { "DeviceDesc", "Invalid" } }
                };
                descriptions.ForEach(
                    x =>
                    {
                        var mock = factory(x);
                        Assert.AreEqual(
                            mock.DeviceCategory != "USB3" && mock.Description != "Invalid",
                            filter.Invoke(mock));
                    });
            }
        }

        private void SetCabinetDetectionMock(bool applyDisplaySetting = true)
        {
            CreateDeviceMocks();
            _cabinetDetectionService.Setup(x => x.CabinetExpectedDevices).Returns(Devices);
            _cabinetDetectionService.Setup(x => x.RefreshCabinetDeviceStatus());

            if (applyDisplaySetting)
                _cabinetDetectionService.Setup(x => x.ApplyDisplaySettings());
            _cabinetDetectionService.Setup(x => x.MapTouchscreens(false)).Returns(true);

            _eventBus.Setup(x => x.Publish(It.IsAny<DisplayMonitorStatusChangeEvent>()));
        }

        private void SetupCabinetDetectionMockForSerialTouch()
        {
            var mainVideo = CreateDeviceMock<IDisplayDevice>(DeviceType.Display);
            mainVideo.SetupGet(v => v.Role).Returns(DisplayRole.Main);
            mainVideo.SetupGet(v => v.Name).Returns("mane");
            mainVideo.SetupGet(v => v.Id).Returns(103);
            _displayDeviceMocks.Add(mainVideo);

            var mainTouch = CreateDeviceMock<ITouchDevice>(DeviceType.Touch);
            mainTouch.SetupGet(v => v.Name).Returns("mane");
            mainTouch.SetupGet(v => v.Id).Returns(0);
            mainTouch.SetupSet(v => v.Status = DeviceStatus.Disconnected);
            mainTouch.SetupGet(v => v.CommunicationType).Returns(CommunicationTypes.Serial);
            _touchDeviceMocks.Add(mainTouch);

            _cabinetDetectionService.Setup(x => x.CabinetExpectedDevices).Returns(Devices);
            _cabinetDetectionService.Setup(x => x.RefreshCabinetDeviceStatus());

            _cabinetDetectionService.Setup(x => x.MapTouchscreens(false)).Returns(true);

            _cabinetDetectionService.Setup(x => x.GetDisplayMappedToTouchDevice(It.IsAny<ITouchDevice>())).Returns(mainVideo.Object);
            _cabinetDetectionService.Setup(x => x.GetDisplayRoleMappedToTouchDevice(It.IsAny<ITouchDevice>())).Returns(mainVideo.Object.Role);

            _eventBus.Setup(x => x.Publish(It.IsAny<DisplayMonitorStatusChangeEvent>()));
        }

        private void SetupPersistence(bool exists = true, bool lcdExpected = true, bool vbdExpected = true)
        {
            _persistentStorage.Setup(x => x.BlockExists(BlockName)).Returns(exists);
            if (exists)
            {
                _persistentStorage.Setup(x => x.GetBlock(BlockName)).Returns(_storageAccessorMock.Object);
            }
            else
            {
                _persistentStorage.Setup(x => x.CreateBlock(PersistenceLevel.Transient, BlockName, 1))
                    .Returns(_storageAccessorMock.Object);
                _storageAccessorMock.Setup(x => x.StartTransaction()).Returns(_transactionMock.Object);
                VideoDisplayMeter.Skip(1).Concat(TouchScreenMeter.Skip(1)).ToList()
                    .ForEach(x => _transactionMock.SetupSet(y => y[x] = false));
                SetupConnectedDeviceMeters(_displayDeviceMocks.Count, VideoDisplayMeter);
                SetupConnectedDeviceMeters(_touchDeviceMocks.Count, TouchScreenMeter);
                _transactionMock.SetupSet(
                    y => y[ApplicationMeters.PlayerButtonErrorCount] = lcdExpected || vbdExpected);
                _transactionMock.SetupSet(y => y[ApplicationConstants.LcdPlayerButtonExpected] = lcdExpected);
                _transactionMock.Setup(x => x.Commit());
                _transactionMock.Setup(x => x.Dispose());
                _transactionMock.SetupGet(y => y[It.IsAny<string>()]).Returns(false);
                _transactionMock.SetupSet(y => y[It.IsAny<string>()] = true);

            }

            _storageAccessorMock.Setup(x => x[ApplicationConstants.LcdPlayerButtonExpected]).Returns(lcdExpected);

            void SetupConnectedDeviceMeters(int deviceCount, string[] meters)
            {
                for (var i = 0; i < deviceCount; i++)
                {
                    var index = i;
                    _transactionMock.SetupSet(y => y[meters[index]] = true);
                }
            }
        }

        private Mock<T> CreateDeviceMock<T>(DeviceType type, DeviceStatus status = DeviceStatus.Unknown)
            where T : class, IDevice
        {
            var mock = _mockRepository.Create<T>();
            mock.Setup(x => x.DeviceType).Returns(type);
            mock.Setup(x => x.Status).Returns(status);
            return mock;
        }

        private void CreateDeviceMocks()
        {
            // Note that these devices have to be in the same order as the meters in DisplayMonitor.cs
            var mainVideo = CreateDeviceMock<IDisplayDevice>(DeviceType.Display);
            mainVideo.SetupGet(v => v.Role).Returns(DisplayRole.Main);
            mainVideo.SetupGet(v => v.Name).Returns("mane");
            mainVideo.SetupGet(v => v.Id).Returns(103);
            _displayDeviceMocks.Add(mainVideo);
            var topVideo = CreateDeviceMock<IDisplayDevice>(DeviceType.Display);
            topVideo.SetupGet(v => v.Role).Returns(DisplayRole.Top);
            topVideo.SetupGet(v => v.Name).Returns("topp");
            topVideo.SetupGet(v => v.Id).Returns(102);
            _displayDeviceMocks.Add(topVideo);
            var vbdVideo = CreateDeviceMock<IDisplayDevice>(DeviceType.Display);
            vbdVideo.SetupGet(v => v.Role).Returns(DisplayRole.VBD);
            vbdVideo.SetupGet(v => v.Name).Returns("v'beed");
            vbdVideo.SetupGet(v => v.Id).Returns(104);
            _displayDeviceMocks.Add(vbdVideo);
            var topperVideo = CreateDeviceMock<IDisplayDevice>(DeviceType.Display);
            topperVideo.SetupGet(v => v.Role).Returns(DisplayRole.Topper);
            topperVideo.SetupGet(v => v.Name).Returns("toppa");
            topperVideo.SetupGet(v => v.Id).Returns(101);
            _displayDeviceMocks.Add(topperVideo);

            var mainTouch = CreateDeviceMock<ITouchDevice>(DeviceType.Touch);
            mainTouch.SetupGet(v => v.Name).Returns("mane");
            mainTouch.SetupGet(v => v.Id).Returns(103);
            mainTouch.SetupGet(v => v.CommunicationType).Returns(CommunicationTypes.HID);
            _touchDeviceMocks.Add(mainTouch);
            var topTouch = CreateDeviceMock<ITouchDevice>(DeviceType.Touch);
            topTouch.SetupGet(v => v.Name).Returns("topp");
            topTouch.SetupGet(v => v.Id).Returns(102);
            topTouch.SetupGet(v => v.CommunicationType).Returns(CommunicationTypes.HID);
            _touchDeviceMocks.Add(topTouch);
            var vbdTouch = CreateDeviceMock<ITouchDevice>(DeviceType.Touch);
            vbdTouch.SetupGet(v => v.Name).Returns("v'beed");
            vbdTouch.SetupGet(v => v.Id).Returns(104);
            vbdTouch.SetupGet(v => v.CommunicationType).Returns(CommunicationTypes.HID);
            _touchDeviceMocks.Add(vbdTouch);

            _cabinetDetectionService.Setup(s => s.GetDisplayRoleMappedToTouchDevice(It.Is<ITouchDevice>(d => d.Id == 103))).Returns(mainVideo.Object.Role);
            _cabinetDetectionService.Setup(s => s.GetDisplayRoleMappedToTouchDevice(It.Is<ITouchDevice>(d => d.Id == 102))).Returns(topVideo.Object.Role);
            _cabinetDetectionService.Setup(s => s.GetDisplayRoleMappedToTouchDevice(It.Is<ITouchDevice>(d => d.Id == 104))).Returns(vbdVideo.Object.Role);
            _cabinetDetectionService.Setup(s => s.GetDisplayRoleMappedToTouchDevice(It.Is<ITouchDevice>(d => d.Id == 101))).Returns((DisplayRole?) null);
        }
    }
}