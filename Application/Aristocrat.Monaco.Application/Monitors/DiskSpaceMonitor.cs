namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Contracts;
    using Contracts.Localization;
    using Hardware.Contracts.Audio;
    using Kernel;
    using log4net;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     The DiskSpaceMonitor class shall monitor the disk space to ensure that it does not fall
    ///     below a certain threshold (i.e. 100MB) if it does then it will disable the cabinet until
    ///     the disk space is above the threshold and cabinet will be enable again
    /// </summary>
    public sealed class DiskSpaceMonitor : IService, IDisposable
    {
        private const long AvailableDiskSpaceThreshold = 104857600; // 100MB
        private const string DataPath = @"/Data";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly Guid LockupId = ApplicationConstants.DiskSpaceBelowThresholdDisableKey;
        private static readonly TimeSpan LogIntervalHours = TimeSpan.FromHours(1);
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(15);

        private readonly IEventBus _bus;
        private readonly ISystemDisableManager _disableManager;
        private readonly IPathMapper _pathMapper;
        private readonly IPropertiesManager _properties;
        private readonly IAudio _audioService;

        private Timer _checkDiskSpaceTimer;
        private bool _disabled;
        private DateTime _lastLogTime = DateTime.MinValue;

        private bool _disposed;

        public DiskSpaceMonitor()
            : this(
                ServiceManager.GetInstance().GetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().GetService<IPathMapper>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().TryGetService<IAudio>())
        {
        }

        public DiskSpaceMonitor(
            ISystemDisableManager disableManager,
            IPathMapper pathMapper,
            IEventBus bus,
            IPropertiesManager propertiesManager,
            IAudio audio)
        {
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _properties = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _audioService = audio ?? throw new ArgumentNullException(nameof(audio));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _checkDiskSpaceTimer.Dispose();
            _checkDiskSpaceTimer = null;

            _disposed = true;
        }

        /// <inheritdoc />
        public string Name => nameof(DiskSpaceMonitor);

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(DiskSpaceMonitor) };

        /// <inheritdoc />
        public void Initialize()
        {
            if (_checkDiskSpaceTimer != null)
            {
                return;
            }

            var dirInfoRoot = _pathMapper.GetDirectory(DataPath);

            if (dirInfoRoot == null)
            {
                Logger.Info("Path not found");
                return;
            }

            _checkDiskSpaceTimer = new Timer(CheckDiskSpace, dirInfoRoot.Root.Name, TimeSpan.Zero, Interval);
            
            Logger.Info("Initialized");
        }

        private void CheckDiskSpace(object state)
        {
            var drive = (string)state;

            if (!NativeMethods.GetDiskFreeSpaceEx(drive, out var free, out _, out _))
            {
                Logger.Info($"Failed to get drive info: {drive}");
                return;
            }

            if (DateTime.UtcNow - _lastLogTime >= LogIntervalHours)
            {
                _lastLogTime = DateTime.UtcNow;
                Logger.Info($"Drive {drive} Available Free Space {free}");
            }

            var belowThreshold = free < AvailableDiskSpaceThreshold;

            if (belowThreshold && !_disabled)
            {
                _disabled = true;

                _disableManager.Disable(LockupId, SystemDisablePriority.Normal,
                    () => Localizer.ForLockup().GetString(ResourceKeys.DiskSpaceBelowThresholdMessage));

                PlayErrorSound();

                _bus.Publish(new DiskSpaceEvent());
            }
            else if (!belowThreshold && _disabled)
            {
                _disabled = false;

                _disableManager.Enable(LockupId);

                _bus.Publish(new DiskSpaceClearEvent());
            }
        }
        
        /// <summary>
        /// Plays the sound defined in the Application Config for DiskSpace monitor check failed.
        /// </summary>
        private void PlayErrorSound()
        {
            var alertVolume = _properties.GetValue(ApplicationConstants.AlertVolumeKey, _audioService.DefaultAlertVolume);
            _audioService.PlayAlert(SoundName.DiskSpaceMonitorError, alertVolume);
        }

        private static class NativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);
        }
    }
}