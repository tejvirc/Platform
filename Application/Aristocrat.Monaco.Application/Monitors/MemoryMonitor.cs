namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using Contracts;
    using Contracts.Localization;
    using Kernel;
    using log4net;
    using Monaco.Localization.Properties;
    using NativeOS.Services.OS;

    public sealed class MemoryMonitor : IService, IDisposable
    {
        private bool _disposed;
        private ulong _memoryLeftThreshold;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private static readonly Guid LockupId = ApplicationConstants.MemoryBelowThresholdDisableKey;
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(15);

        private readonly ISystemDisableManager _disableManager;
        private readonly IPropertiesManager _properties;

#if !RETAIL
        private readonly Timer _checkMemoryStatusTimer;
        private bool _disabled;
#endif

        public MemoryMonitor()
            : this(
                ServiceManager.GetInstance().GetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        public MemoryMonitor(
            ISystemDisableManager disableManager,
            IPropertiesManager propertiesManager)
        {
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _properties = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
#if !RETAIL
            _checkMemoryStatusTimer = new Timer(MemoryCheck, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
#endif
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

#if !RETAIL
            _checkMemoryStatusTimer.Dispose();
#endif
            _disposed = true;
        }

        /// <inheritdoc />
        public string Name => nameof(MemoryMonitor);

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(MemoryMonitor) };

        /// <inheritdoc />
        public void Initialize()
        {
            _memoryLeftThreshold = (ulong)_properties.GetValue(ApplicationConstants.LowMemoryThreshold, ApplicationConstants.LowMemoryThresholdDefault); //Default of 200MB
#if !RETAIL
            _checkMemoryStatusTimer.Change(TimeSpan.Zero, Interval);
#endif
            Logger.Info("Initialized");
        }

#if !RETAIL
        /// <summary>
        /// Check memory to see if system is running low on memory.
        /// </summary>
        /// <param name="state">Not used, but required in signature</param>
        /// <remarks>This will lockup the game and request an attendant.</remarks>
        private void MemoryCheck(object state)
        {
            var info = SystemPerformanceProvider.GetSystemPerformance();
            if (info.PhysicalAvailableBytes > _memoryLeftThreshold || _disabled)
            {
                return;
            }

            _checkMemoryStatusTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            Logger.Error($"Computer Memory is full. Locking up system for reboot. Available Memory: {info.PhysicalAvailable}. Total Memory: {info.PhysicalTotal}. Threshold: {_memoryLeftThreshold}.");
            _disabled = true;
            _disableManager.Disable(LockupId, SystemDisablePriority.Immediate,
                () => Localizer.ForLockup().GetString(ResourceKeys.OutOfMemoryMessage));
        }
#endif
    }
}