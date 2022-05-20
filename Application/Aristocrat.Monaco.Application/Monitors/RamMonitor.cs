namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Contracts;
    using Contracts.Localization;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Monaco.Localization.Properties;
    using Util;

    /// <summary>
    ///     The RamMonitor class shall subscribe to and monitor for events related to a RAM error.
    ///     Upon such a detection, game play shall be disabled until the problem is dealt with.
    /// </summary>
    public sealed class RamMonitor : IService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly Guid StorageFaultGuid = ApplicationConstants.StorageFaultDisableKey;

        private readonly IEventBus _bus;
        private readonly IPersistentStorageManager _storageManager;
        private readonly IPropertiesManager _properties;
        private readonly ISystemDisableManager _disableManager;
        private readonly IMeterManager _meterManager;
        private readonly IAudio _audioService;

        private TimeSpan _integrityCheckInterval;
        private Timer _integrityCheckTimer;

        private bool _disposed;
        private string _criticalMemoryCheckFailedErrorSoundFilePath;

        public RamMonitor()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IPersistentStorageManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().GetService<IMeterManager>(),
                ServiceManager.GetInstance().TryGetService<IAudio>())
        {
        }

        public RamMonitor(
            IEventBus bus,
            IPersistentStorageManager storageManager,
            IPropertiesManager properties,
            ISystemDisableManager disableManager,
            IMeterManager meterManager,
            IAudio audioService)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _storageManager = storageManager ?? throw new ArgumentNullException(nameof(storageManager));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _bus.UnsubscribeAll(this);

            if (_integrityCheckTimer != null)
            {
                _integrityCheckTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                _integrityCheckTimer.Dispose();
                _integrityCheckTimer = null;
            }

            _disposed = true;
        }

        /// <inheritdoc />
        public string Name => typeof(RamMonitor).Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(RamMonitor) };

        /// <inheritdoc />
        public void Initialize()
        {
            Logger.Info("Initializing RamMonitor...");

            LoadSounds();

            _bus.Subscribe<PersistentStorageClearStartedEvent>(this, Handle);
            _bus.Subscribe<PersistentStorageIntegrityCheckFailedEvent>(this, Handle);
            _bus.Subscribe<StorageErrorEvent>(this, Handle);
            _bus.Subscribe<SecondaryStorageErrorEvent>(this, Handle);

            _storageManager.VerifyIntegrity(true);

            // Check if the interval needs to be checked
            if (_properties.GetValue(ApplicationConstants.PeriodicCriticalMemoryIntegrityCheckEnabled, false))
            {
                _integrityCheckInterval = TimeSpan.FromSeconds(
                    _properties.GetValue(
                        ApplicationConstants.PeriodicCriticalMemoryIntegrityCheckValue,
                        (int)TimeSpan.FromHours(24).TotalSeconds));

                _integrityCheckTimer = new Timer(
                    OnIntegrityCheck,
                    null,
                    _integrityCheckInterval,
                    Timeout.InfiniteTimeSpan);
            }

            Logger.Info("Initializing RamMonitor...complete!");
        }

        private void OnIntegrityCheck(object state)
        {
            if (_storageManager.VerifyIntegrity(true))
            {
                _integrityCheckTimer.Change(_integrityCheckInterval, Timeout.InfiniteTimeSpan);
            }
        }

        private void Handle(PersistentStorageIntegrityCheckFailedEvent evt)
        {
            _disableManager.Disable(
                StorageFaultGuid,
                SystemDisablePriority.Immediate,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.IntegrityCheckFailed));

            PlayErrorSound();
        }

        private void Handle(PersistentStorageClearStartedEvent theEvent)
        {
            _disableManager.Enable(StorageFaultGuid);
        }

        private void Handle(StorageErrorEvent theEvent)
        {
            if (_disableManager.CurrentDisableKeys.Contains(StorageFaultGuid))
            {
                return;
            }

            Logger.Error($"Handling storage error event: {theEvent.Id}");

            _disableManager.Disable(
                StorageFaultGuid,
                SystemDisablePriority.Immediate,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.StorageFault));

            PlayErrorSound();

            // NOTE: It's likely that this will fail if the database is in a bad state
            _meterManager.GetMeter(ApplicationMeters.MemoryErrorCount).Increment(1);
        }

        private void Handle(SecondaryStorageErrorEvent theEvent)
        {
            Logger.Error($"Handling secondary storage error event: {theEvent.Id}");

            switch (theEvent.Id)
            {
                case SecondaryStorageError.ExpectedButNotConnected:
                    // secondary storage required and missing.
                    _disableManager.Disable(
                        ApplicationConstants.SecondaryStorageMediaNotConnectedKey,
                        SystemDisablePriority.Immediate,
                        () => Localizer.For(CultureFor.Operator)
                            .GetString(ResourceKeys.ErrorInfoSecondaryStorageMediaNotConnected));
                    PlayErrorSound();

                    break;
                case SecondaryStorageError.NotExpectedButConnected:

                    if (!_properties.GetValue(ApplicationConstants.ReadOnlyMediaRequired, false))
                    {
                        // Secondary storage not supported, but connected
                        _disableManager.Disable(
                            ApplicationConstants.SecondaryStorageMediaConnectedKey,
                            SystemDisablePriority.Immediate,
                            () => Localizer.For(CultureFor.Operator)
                                .GetString(ResourceKeys.ErrorInfoSecondaryStorageMediaConnected));
                        PlayErrorSound();
                    }

                    break;
            }
        }

        /// <summary>
        /// Load sound if configured for DB Integrity check failed.
        /// </summary>
        private void LoadSounds()
        {
            _criticalMemoryCheckFailedErrorSoundFilePath = _properties?.GetValue(
                ApplicationConstants.PeriodicCriticalMemoryIntegrityCheckSoundFilePath,
                string.Empty);
            _audioService.LoadSound(_criticalMemoryCheckFailedErrorSoundFilePath);
            }

        /// <summary>
        /// Plays the sound defined in the Application Config for DB Integrity check failed.
        /// </summary>
        private void PlayErrorSound()
        {
            _audioService.PlaySound(
                _properties,
                _criticalMemoryCheckFailedErrorSoundFilePath);
        }
    }
}