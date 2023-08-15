namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Interop;
    using System.Windows.Media;
    using Cabinet.Contracts;
    using Contracts;
    using Contracts.Handlers;
    using Contracts.Localization;
    using Handlers;
    using Hardware.Contracts.ButtonDeck;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Display;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.Touch;
    using Kernel;
    using log4net;
    using Monaco.Localization.Properties;

    public class DisplayMonitor : IService, IDisposable
    {
        private const string LcdButtonDeckDescription = "USBD480";
        private const string BlockName = "Aristocrat.Monaco.Application.Monitors.DisplayMonitor";
        private const int ExpectedButtonDeckDisplayCount = 2;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private static readonly Dictionary<DisplayRole, string> TouchScreenMeters = new()
        {
            { DisplayRole.Top, ApplicationMeters.TopTouchScreenDisconnectCount },
            { DisplayRole.Main, ApplicationMeters.MainTouchScreenDisconnectCount },
            { DisplayRole.VBD, ApplicationMeters.VbdTouchScreenDisconnectCount },
        };

        private static readonly Dictionary<DisplayRole, string> VideoDisplayMeters = new()
        {
            { DisplayRole.Topper, ApplicationMeters.TopperVideoDisplayDisconnectCount },
            { DisplayRole.Top, ApplicationMeters.TopVideoDisplayDisconnectCount },
            { DisplayRole.Main, ApplicationMeters.MainVideoDisplayDisconnectCount },
            { DisplayRole.VBD, ApplicationMeters.VbdVideoDisplayDisconnectCount },
        };

        private readonly IReadOnlyDictionary<string, string> _monitoredDeviceCategories =
            new Dictionary<string, string>
            {
                { "DISPLAY", null },
                { "HID", null },
                { "USB", LcdButtonDeckDescription },
                { "SERIAL", null }
            };

        private readonly ISystemDisableManager _disableManager;
        private readonly IEventBus _eventBus;
        private readonly IMeterManager _meterManager;
        private readonly ICabinetDetectionService _cabinetDetectionService;
        private readonly IButtonDeckDisplay _buttonDeckDisplay;
        private readonly object _lock = new();
        private readonly IPersistentStorageAccessor _persistentBlock;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly List<IDeviceStatusHandler> _deviceStatusHandlers = new();
        private readonly bool _lcdButtonDeckExpected;
        private bool _lcdButtonDeckConnected = true;

        private bool _disposed;

        public DisplayMonitor()
            :
            this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().GetService<IMeterManager>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<ICabinetDetectionService>(),
                ServiceManager.GetInstance().GetService<IButtonDeckDisplay>())
        {
        }

        public DisplayMonitor(
            IEventBus eventBus,
            ISystemDisableManager disableManager,
            IMeterManager meterManager,
            IPersistentStorageManager persistentStorage,
            ICabinetDetectionService cabinetDetectionService,
            IButtonDeckDisplay buttonDeckDisplay)
        {
            _eventBus = eventBus
                ?? throw new ArgumentNullException(nameof(eventBus));
            _disableManager = disableManager
                ?? throw new ArgumentNullException(nameof(disableManager));
            _meterManager = meterManager
                ?? throw new ArgumentNullException(nameof(meterManager));
            _cabinetDetectionService = cabinetDetectionService
                ?? throw new ArgumentNullException(nameof(cabinetDetectionService));
            _buttonDeckDisplay = buttonDeckDisplay
                ?? throw new ArgumentNullException(nameof(buttonDeckDisplay));
            _persistentStorage = persistentStorage
                ?? throw new ArgumentNullException(nameof(persistentStorage));

            AddDeviceHandlers<IDisplayDevice>(
                VideoDisplayMeters,
                x => OnDeviceStatusChanged<DisplayConnectedEvent>(x, true),
                x => OnDeviceStatusChanged<DisplayDisconnectedEvent>(x, false));

            AddDeviceHandlers<ITouchDevice>(
                TouchScreenMeters,
                x => OnDeviceStatusChanged<TouchDisplayConnectedEvent>(x, true),
                x => OnDeviceStatusChanged<TouchDisplayDisconnectedEvent>(x, false));

            _persistentBlock = GetPersistence();
            _lcdButtonDeckExpected = (bool)_persistentBlock[ApplicationConstants.LcdPlayerButtonExpected];
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Name => nameof(DisplayMonitor);

        public ICollection<Type> ServiceTypes => new[] { typeof(DisplayMonitor) };

        public void Initialize()
        {
            lock (_lock)
            {
                _eventBus.Subscribe<DeviceConnectedEvent>(this,
                    _ => CheckDevicesCount(),
                    FilterDeviceEvent);

                _eventBus.Subscribe<DeviceDisconnectedEvent>(this,
                    _ => CheckDevicesCount(),
                    FilterDeviceEvent);

                _eventBus.Subscribe<ClearDisplayDisconnectedLockupEvent>(this,
                    _ =>
                    {
                        _disableManager.Enable(ApplicationConstants.DisplayDisconnectedLockupKey);
                        SetGraphicsSafeMode(false);
                    });

                _eventBus.Subscribe<SetDisplayDisconnectedLockupEvent>(this,
                    _ => _disableManager.Disable(
                        ApplicationConstants.DisplayDisconnectedLockupKey,
                        SystemDisablePriority.Immediate,
                        () => Localizer.ForLockup().GetString(ResourceKeys.DisplayDisconnected))
                );

                if (CheckDevicesCount())
                {
                    _disableManager.Enable(ApplicationConstants.DisplayConnectedLockupKey);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        /// <summary>
        ///     Enabled/disables safe mode for graphics rendering
        /// </summary>
        /// <remarks>
        ///     When displays are disconnected and reconnected, the Windows Composition thread will often crash.
        ///     This is an issue common with various NVidia GPU driver versions. Upon display [re]connection, windows are
        ///     being moved to different displays and Bink videos are currently playing. Often this will cause the
        ///     Composition thread will block as it waits for GPU commands to be flushed. It seems Bink videos play a big
        ///     part in the low-level graphics commands halting. By enabling software rendering, it bypasses use of the
        ///     GPU and its possible issues, making things a lot more stable. When displays have been reconnected and
        ///     windows have been positioned, GPU hardware rendering is stable and can be re-enabled.
        /// </remarks>
        /// <param name="enabled">True to enable software rendering, false to enable hardware rendering</param>
        private static void SetGraphicsSafeMode(bool enabled)
        {
            RenderOptions.ProcessRenderMode = enabled ? RenderMode.SoftwareOnly : RenderMode.Default;
        }

        private bool FilterDeviceEvent<T>(T deviceEvent) where T : BaseDeviceEvent
        {
            if (!_monitoredDeviceCategories.TryGetValue(deviceEvent.DeviceCategory, out var description))
            {
                return false;
            }

            return string.IsNullOrEmpty(description) || description == deviceEvent.Description;
        }

        private void AddDeviceHandlers<TDevice>(
            IReadOnlyDictionary<DisplayRole, string> meters,
            Action<IDeviceStatusHandler> connectAction,
            Action<IDeviceStatusHandler> disconnectAction)
            where TDevice : class, IDevice
        {
            var devices = _cabinetDetectionService.CabinetExpectedDevices.OfType<TDevice>();

            foreach (var device in devices)
            {
                var role = DisplayRole.Unknown;

                switch (device)
                {
                    case IDisplayDevice displayDevice:
                        role = displayDevice.Role;
                        break;

                    case ITouchDevice touchDevice:
                        role = _cabinetDetectionService.GetDisplayRoleMappedToTouchDevice(touchDevice) ?? DisplayRole.Unknown;
                        break;
                }

                if (role == DisplayRole.Unknown)
                {
                    continue;
                }

                if (device is ITouchDevice touch && touch.CommunicationType == Cabinet.CommunicationTypes.Serial)
                {
                    var serialTouchDeviceStatusHandler = new SerialTouchDeviceStatusHandler()
                    {
                        Device = device,
                        Meter = meters[role],
                        ConnectAction = connectAction,
                        DisconnectAction = disconnectAction
                    };

                    _deviceStatusHandlers.Add(serialTouchDeviceStatusHandler);
                }
                else
                {
                    var deviceStatusHandler = new DefaultDeviceStatusHandler
                    {
                        Device = device,
                        Meter = meters[role],
                        ConnectAction = connectAction,
                        DisconnectAction = disconnectAction
                    };

                    _deviceStatusHandlers.Add(deviceStatusHandler);
                }
            }

            foreach (var handler in _deviceStatusHandlers)
            {
                Logger.Debug($"AddDeviceHandlers - device name={handler.Device.Name}, type={handler.Device.DeviceType}, id={handler.Device.Id}, meter={handler.Meter}");
            }
        }

        private bool PersistDeviceStatus(string meter, bool connected)
        {
            lock (_lock)
            {
                using (var transaction = _persistentBlock.StartTransaction())
                {
                    var oldStatus = (bool)transaction[meter];
                    transaction[meter] = connected;
                    transaction.Commit();
                    return oldStatus;
                }
            }
        }

        private IPersistentStorageAccessor GetPersistence()
        {
            return _persistentStorage.BlockExists(BlockName)
                ? _persistentStorage.GetBlock(BlockName)
                : CreatePersistence();

            IPersistentStorageAccessor CreatePersistence()
            {
                var block = _persistentStorage.CreateBlock(PersistenceLevel.Transient, BlockName, 1);
                using (var transaction = block.StartTransaction())
                {
                    foreach (var meter in VideoDisplayMeters.Values)
                    {
                        transaction[meter] = false;
                    }

                    foreach (var meter in TouchScreenMeters.Values)
                    {
                        transaction[meter] = false;
                    }

                    foreach (var x in _deviceStatusHandlers)
                    {
                        transaction[x.Meter] = true;
                    }

                    transaction[ApplicationMeters.PlayerButtonErrorCount] =
                        _cabinetDetectionService.GetDisplayDeviceByItsRole(DisplayRole.VBD) != null ||
                        _buttonDeckDisplay.DisplayCount == ExpectedButtonDeckDisplayCount;
                    transaction[ApplicationConstants.LcdPlayerButtonExpected] =
                        _cabinetDetectionService.GetDisplayDeviceByItsRole(DisplayRole.VBD) == null &&
                        _buttonDeckDisplay.DisplayCount == ExpectedButtonDeckDisplayCount;
                    transaction.Commit();
                }

                return block;
            }
        }

        private void OnDeviceStatusChanged<TEvent>(string meter, bool connected) where TEvent : IEvent, new()
        {
            Logger.Error($"{meter} connected = {connected}");
            var oldStatus = PersistDeviceStatus(meter, connected);
            if (oldStatus != connected && connected == false)
            {
                _meterManager.GetMeter(meter).Increment(1);
            }

            _eventBus.Publish(new TEvent());
        }

        private void OnDeviceStatusChanged<TEvent>(IDeviceStatusHandler sender, bool connected)
            where TEvent : IEvent, new()
        {
            // Handle VBD Display status changed
            if (sender.Device is IDisplayDevice displayDevice && displayDevice.Role == DisplayRole.VBD)
            {
                OnButtonDeckStatusChanged(connected);
            }

            if (sender.Device is IDisplayDevice)
            {
                var allConnected = _deviceStatusHandlers.Where(x => x.Device.DeviceType == DeviceType.Display)
                    .All(x => x.Status != DeviceStatus.Disconnected);

                if (_disableManager.CurrentDisableKeys.Contains(ApplicationConstants.DisplayDisconnectedLockupKey) && allConnected)
                {
                    _disableManager.Disable(
                        ApplicationConstants.DisplayConnectedLockupKey,
                        SystemDisablePriority.Immediate,
                        () => Localizer.ForLockup().GetString(ResourceKeys.DisplayConnected));
                }
                else if (_disableManager.CurrentDisableKeys.Contains(ApplicationConstants.DisplayConnectedLockupKey) && !allConnected)
                {
                    _disableManager.Enable(ApplicationConstants.DisplayConnectedLockupKey);
                }
            }

            // Handle Touch Devices status changed
            if (sender.Device is ITouchDevice)
            {
                var allConnected = _deviceStatusHandlers.Where(x => x.Device.DeviceType == DeviceType.Touch)
                    .All(x => x.Status != DeviceStatus.Disconnected);

                // TouchDisplayDisconnected lockup is cleared when All touch devices are connected.
                if (_disableManager.CurrentDisableKeys.Contains(ApplicationConstants.TouchDisplayDisconnectedLockupKey) && allConnected)
                {
                    _disableManager.Disable(
                        ApplicationConstants.TouchDisplayReconnectedLockupKey,
                        SystemDisablePriority.Immediate,
                        () => Localizer.ForLockup().GetString(ResourceKeys.TouchDisplayReconnected));
                }
                // TouchDisplayDisconnected lockup is enabled when any touch displays are disconnected.
                else if (_disableManager.CurrentDisableKeys.Contains(ApplicationConstants.TouchDisplayReconnectedLockupKey) && !allConnected)
                {
                    _disableManager.Enable(ApplicationConstants.TouchDisplayReconnectedLockupKey);
                }
            }

            // Update meter for device
            OnDeviceStatusChanged<TEvent>(sender.Meter, connected);
        }

        private void OnButtonDeckStatusChanged(bool connected)
        {
            if (connected)
            {
                OnDeviceStatusChanged<ButtonDeckConnectedEvent>(ApplicationMeters.PlayerButtonErrorCount, true);
            }
            else
            {
                OnDeviceStatusChanged<ButtonDeckDisconnectedEvent>(ApplicationMeters.PlayerButtonErrorCount, false);
            }
        }

        private bool CheckDevicesCount()
        {
            lock (_lock)
            {
                CheckLcdButtonDeck();
                _cabinetDetectionService.RefreshCabinetDeviceStatus();
                var displayStatus = CheckDisplayStatus();

                var touchStatus = CheckStatus(
                    _deviceStatusHandlers.Where(x => x.Device.DeviceType == DeviceType.Touch).ToList(),
                    ApplicationConstants.TouchDisplayDisconnectedLockupKey,
                    ResourceKeys.TouchDisplayDisconnected);

                if (displayStatus && touchStatus)
                {
                    _cabinetDetectionService.MapTouchscreens();
                    return true;
                }

                return false;
            }

            bool CheckDisplayStatus()
            {
                var displayStatusHandlers =
                    _deviceStatusHandlers.Where(x => x.Device.DeviceType == DeviceType.Display).ToList();
                // Apply display settings if all displays are connected.
                if (displayStatusHandlers.All(x => x.Device.Status == DeviceStatus.Connected)
                    && displayStatusHandlers.Any(x => x.Device.Status != x.Status))
                {
                    _cabinetDetectionService.ApplyDisplaySettings();
                }

                var displayStatus = CheckStatus(
                    displayStatusHandlers,
                    ApplicationConstants.DisplayDisconnectedLockupKey,
                    ResourceKeys.DisplayDisconnected);
                SetGraphicsSafeMode(!displayStatus);

                return displayStatus;
            }

            bool CheckStatus(List<IDeviceStatusHandler> handlers, Guid disableKey, string resource)
            {
                var previouslyConnected = handlers.All(x => x.Status != DeviceStatus.Disconnected);
                handlers.ForEach(x => x.Refresh());

                var nowConnected = handlers.All(x => x.Status != DeviceStatus.Disconnected);
                HandleStatusChange(previouslyConnected, nowConnected, disableKey, resource);
                return nowConnected;
            }
        }

        private void HandleStatusChange(bool oldStatus, bool newStatus, Guid disableKey, string resource)
        {
            _eventBus.Publish(new DisplayMonitorStatusChangeEvent());

            if (oldStatus == newStatus)
            {
                return;
            }

            if (newStatus)
            {
                Logger.Debug($"Enabling {disableKey} reason key {resource}.");
                _disableManager.Enable(disableKey);
            }
            else
            {
                Logger.Debug($"Disabling {disableKey} reason key {resource}.");
                _disableManager.Disable(
                    disableKey,
                    SystemDisablePriority.Immediate,
                    () => Localizer.ForLockup().GetString(resource));
            }
        }

        private void CheckLcdButtonDeck()
        {
            if (!_lcdButtonDeckExpected)
            {
                return;
            }

            var newStatus = _buttonDeckDisplay.DisplayCount == ExpectedButtonDeckDisplayCount;
            if (_lcdButtonDeckConnected == newStatus)
            {
                return;
            }

            _lcdButtonDeckConnected = newStatus;
            OnButtonDeckStatusChanged(newStatus);
        }
    }
}