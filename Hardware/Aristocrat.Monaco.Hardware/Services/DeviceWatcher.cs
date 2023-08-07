namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Interop;
    using Contracts.Cabinet;
    using Kernel;
    using log4net;
    using Microsoft.Win32;

    internal sealed class DeviceWatcher : IDisposable, IService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IEventBus _eventBus;
        private readonly Monitor _monitor = new Monitor();
        private bool _disposed;

        public DeviceWatcher()
            : this(ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        public DeviceWatcher(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _monitor.PlugInAction = HandlePluggedInEvent;
            _monitor.UnPlugAction = HandleUnPluggedEvent;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _monitor.UnRegister();
            _monitor.Dispose();
            _disposed = true;
            Logger.Debug("Device watcher stopped.");
        }

        public string Name => nameof(DeviceWatcher);

        public ICollection<Type> ServiceTypes => new List<Type> { typeof(DeviceWatcher) };

        public void Initialize()
        {
            _monitor.Register();
        }

        private void HandleUnPluggedEvent(IDictionary<string, object> deviceProperties)
        {
            var eventData = new DeviceDisconnectedEvent(deviceProperties);
            _eventBus.Publish(eventData);
        }

        private void HandlePluggedInEvent(IDictionary<string, object> deviceProperties)
        {
            var eventData = new DeviceConnectedEvent(deviceProperties);
            _eventBus.Publish(eventData);
        }

        private sealed class Monitor : IDisposable
        {
            private readonly IReadOnlyCollection<Guid> _deviceClassGuidList = new[]
            {
                new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED"), // USB device interface
                new Guid("E6F07B5F-EE97-4a90-B076-33F57BF4EAA7"), // Monitor device interface
                new Guid("4D1E55B2-F16F-11CF-88CB-001111000030"), // HID device interface
                new Guid("53F5630D-B6BF-11D0-94F2-00A0C91EFB8B"), // STORAGE device interface
                new Guid("50dd5230-ba8a-11d1-bf5d-0000f805f530")  // Smart Card Readers
            };

            private HwndSource _windowSource;
            private List<IntPtr> _hNotifyDevNodes;
            private bool _disposed;

            public Action<IDictionary<string, object>> PlugInAction { get; set; }

            public Action<IDictionary<string, object>> UnPlugAction { get; set; }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                UnRegister();
                _disposed = true;
            }

            public void Register()
            {
                Application.Current?.Dispatcher?.Invoke(
                    () =>
                    {
                        UnRegister();
                        _windowSource = new HwndSource(0, 0, 0, 0, 0, "DeviceWatcher", IntPtr.Zero);
                        _windowSource.AddHook(Hook);
                        _hNotifyDevNodes = _deviceClassGuidList
                            .Select(x => RegisterNotification(_windowSource.Handle, x)).ToList();
                    });
            }

            public void UnRegister()
            {
                if (_windowSource == null)
                {
                    return;
                }

                _hNotifyDevNodes.ForEach(x => NativeMethods.UnregisterDeviceNotification(x));

                var windowSource = _windowSource;
                Application.Current?.Dispatcher?.Invoke(
                    () =>
                    {
                        windowSource.RemoveHook(Hook);
                        windowSource.Dispose();
                        windowSource = null;
                    });
                if (windowSource != null)
                {
                    _windowSource.Dispose();
                }

                _windowSource = null;
            }

            private static IntPtr RegisterNotification(IntPtr handle, Guid guid)
            {
                var devIf = new NativeMethods.DEV_BROADCAST_DEVICEINTERFACE();

                // Set to HID GUID
                devIf.dbcc_size = Marshal.SizeOf(devIf);
                devIf.dbcc_devicetype = NativeMethods.DBT_DEVTYP_DEVICEINTERFACE;
                devIf.dbcc_reserved = 0;
                devIf.dbcc_classguid = guid;

                // Allocate a buffer for DLL call
                var devIfBuffer = Marshal.AllocHGlobal(devIf.dbcc_size);

                // Copy devIF to buffer
                Marshal.StructureToPtr(devIf, devIfBuffer, true);

                // Register for HID device notifications
                var hNotifyDevNode = NativeMethods.RegisterDeviceNotification(
                    handle,
                    devIfBuffer,
                    NativeMethods.DEVICE_NOTIFY_WINDOW_HANDLE);
                if (hNotifyDevNode == IntPtr.Zero)
                {
                    var err = Marshal.GetLastWin32Error();
                    throw new Win32Exception(err);
                }

                // Copy buffer to devIF
                Marshal.PtrToStructure(devIfBuffer, devIf);

                // Free buffer
                Marshal.FreeHGlobal(devIfBuffer);
                return hNotifyDevNode;
            }

            private static IDictionary<string, object> GetDeviceDetails(string deviceName)
            {
                var deviceDetails = new Dictionary<string, object>
                {
                    { nameof(BaseDeviceEvent.DeviceId), deviceName },
                    { nameof(BaseDeviceEvent.DeviceCategory), deviceName.Split('\\').FirstOrDefault() }
                };

                var regPath = $@"SYSTEM\CurrentControlSet\Enum\{deviceName}";
                var key = Registry.LocalMachine.OpenSubKey(regPath);
                if (key == null)
                {
                    return deviceDetails;
                }

                foreach (var valueName in key.GetValueNames())
                {
                    var value = key.GetValue(valueName);
                    if (value is string stringVal)
                    {
                        value = stringVal.Split(';').LastOrDefault();
                    }

                    deviceDetails.Add(valueName, value);
                }

                return deviceDetails;
            }

            private static string DevBroadcastNameToDeviceName(string name)
            {
                return string.Join("\\", name.Split('#', '\\', '?').SkipWhile(string.IsNullOrEmpty).Take(3));
            }

            private IntPtr Hook(IntPtr handleWindow, int msg, IntPtr writeParam, IntPtr leftParam, ref bool handled)
            {
                if (msg != NativeMethods.WM_DEVICECHANGE)
                {
                    return IntPtr.Zero;
                }

                var eventType = writeParam.ToInt32();
                Action<IDictionary<string, object>> eventAction;
                string eventName;
                switch (eventType)
                {
                    case NativeMethods.DBT_DEVICEARRIVAL:
                        eventName = "Connected";
                        eventAction = PlugInAction;
                        break;
                    case NativeMethods.DBT_DEVICEREMOVECOMPLETE:
                        eventName = "Disconnected";
                        eventAction = UnPlugAction;
                        break;
                    default:
                        return IntPtr.Zero;
                }

                var hdr = new NativeMethods.DEV_BROADCAST_HDR();

                // Convert leftParam to DEV_BROADCAST_HDR structure
                Marshal.PtrToStructure(leftParam, hdr);
                if (hdr.dbch_devicetype != NativeMethods.DBT_DEVTYP_DEVICEINTERFACE)
                {
                    return IntPtr.Zero;
                }

                var devIf = new NativeMethods.DEV_BROADCAST_DEVICEINTERFACE_1();
                Marshal.PtrToStructure(leftParam, devIf);
                var name = DevBroadcastNameToDeviceName(new string(devIf.dbcc_name.TakeWhile(x => x != 0).ToArray()));
                Logger.Debug($"Device {eventName}: {name}");
                var deviceDetails = GetDeviceDetails(name);
                eventAction(deviceDetails);
                return IntPtr.Zero;
            }
        }
    }
}