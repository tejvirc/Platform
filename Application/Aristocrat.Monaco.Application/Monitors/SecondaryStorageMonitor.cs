namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    /// <summary>
    ///     This class monitors the secondary storage when enabled for a jurisdiction
    /// </summary>
    public class SecondaryStorageMonitor : IService, IDisposable
    {
        // ReSharper disable once PossibleNullReferenceException
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _propertiesManager;

        private bool _storageDeviceRemoved;
        private bool _disposed;

        public SecondaryStorageMonitor()
            : this(
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        public SecondaryStorageMonitor(IPropertiesManager propertiesManager, IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Name => typeof(SecondaryStorageMonitor).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(SecondaryStorageMonitor) };

        public void Initialize()
        {
            VerifySecondaryStorageRequired();

            if (_propertiesManager.GetValue(SecondaryStorageConstants.SecondaryStorageRequired, false))
            {
                _eventBus.Subscribe<DeviceDisconnectedEvent>(
                    this,
                    _ => StorageDeviceRemoved(),
                    FilterStorageDeviceEvent);
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

        private static void VerifySecondaryStorageRequired()
        {
            var secondaryStorageManager = ServiceManager.GetInstance().GetService<ISecondaryStorageManager>();

            secondaryStorageManager.VerifyConfiguration();
        }

        private static bool FilterStorageDeviceEvent<T>(T deviceEvent) where T : BaseDeviceEvent
        {
            return deviceEvent.DeviceCategory.Equals("STORAGE");
        }

        private void StorageDeviceRemoved()
        {
            if (_storageDeviceRemoved)
            {
                return;
            }

            _storageDeviceRemoved = true;

            Logger.Debug("Storage Device Removed");

            VerifySecondaryStorageRequired();
        }
    }
}