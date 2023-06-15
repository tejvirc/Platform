﻿namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Management;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Interop;
    using Common;
    using Contracts.Cabinet;
    using JetBrains.Annotations;
    using Kernel;
    using log4net;
    using Microsoft.Win32;
    using NativeUsb.DeviceWatcher;

    internal sealed class DeviceWatcher : IDisposable, IService
    {
        private readonly IEventBus _eventBus;
        private readonly IDeviceWatcher _deviceWatcher;
        private bool _disposed;

        public DeviceWatcher()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                DeviceWatcherFactory.CreateDeviceWatcher())
        {
        }

        public DeviceWatcher(IEventBus eventBus, IDeviceWatcher deviceWatcher)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _deviceWatcher = deviceWatcher ?? throw new ArgumentNullException(nameof(deviceWatcher));
            _deviceWatcher.DeviceUnplugged += HandleUnPluggedEvent;
            _deviceWatcher.DevicePluggedIn += HandlePluggedInEvent;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _deviceWatcher.DeviceUnplugged -= HandleUnPluggedEvent;
            _deviceWatcher.DevicePluggedIn -= HandlePluggedInEvent;
            _deviceWatcher.Dispose();
            _disposed = true;
        }

        public string Name => nameof(DeviceWatcher);

        public ICollection<Type> ServiceTypes => new List<Type> { typeof(DeviceWatcher) };

        public void Initialize()
        {
            _deviceWatcher.Initialize(CancellationToken.None).WaitForCompletion();
        }

        private void HandleUnPluggedEvent(object sender, DeviceUnpluggedEventArgs eventArgs)
        {
            var eventData = new DeviceDisconnectedEvent(eventArgs.DeviceProperties);
            _eventBus.Publish(eventData);
        }

        private void HandlePluggedInEvent(object sender, DevicePluggedInEventArgs eventArgs)
        {
            var eventData = new DeviceConnectedEvent(eventArgs.DeviceProperties);
            _eventBus.Publish(eventData);
        }
    }
}