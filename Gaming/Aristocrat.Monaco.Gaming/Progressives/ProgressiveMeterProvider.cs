namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Contracts.Meters;
    using Contracts.Progressives;
    using Hardware.Contracts.Persistence;
    using Kernel;

    /// <summary>
    ///     Definition of the ProgressiveMeterProvider class.
    /// </summary>
    /// <seealso cref="T:Aristocrat.Monaco.Application.Contracts.BaseMeterProvider" />
    /// <seealso cref="T:System.IDisposable" />
    public class ProgressiveMeterProvider : BaseMeterProvider, IDisposable
    {
        private const PersistenceLevel ProviderPersistenceLevel = PersistenceLevel.Critical;

        private const string ProgressiveMeterProviderExtensionPoint = "/Gaming/Metering/ProgressiveMeterProvider";

        private readonly object _lock = new();
        private readonly IProgressiveMeterManager _meterManager;
        private readonly IProgressiveLevelProvider _levelProvider;
        private readonly IPersistentStorageManager _persistentStorage;

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveMeterProvider" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="persistentStorage">The persistent storage manager.</param>
        /// <param name="meterManager">The meter manager.</param>
        /// <param name="levelProvider">Progressive Level Provider.</param>
        public ProgressiveMeterProvider(
            IPersistentStorageManager persistentStorage,
            IProgressiveMeterManager meterManager,
            IProgressiveLevelProvider levelProvider)
            : base(ProviderName)
        {
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _levelProvider = levelProvider ?? throw new ArgumentNullException(nameof(levelProvider));

            _meterManager.ProgressiveAdded += OnProgressiveAdded;

            _meterManager.LPCompositeMetersCanUpdate += UpdateLPCompositeMeters;

            Initialize();
        }

        /// <summary>
        ///     Gets the name of the provider.
        /// </summary>
        /// <value>The name of the provider.</value>
        public static string ProviderName => typeof(ProgressiveMeterProvider).ToString();

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases the unmanaged resources used by the Aristocrat.Monaco.Gaming.ProgressiveMeterProvider and optionally
        ///     releases the managed
        ///     resources.
        /// </summary>
        /// <param name="disposing">
        ///     True to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _meterManager.ProgressiveAdded -= OnProgressiveAdded;
            }

            _disposed = true;
        }

        private void OnProgressiveAdded(object sender, ProgressiveAddedEventArgs progressiveAddedEventArgs)
        {
            Initialize();

            // Refresh the published meters
            Invalidate();
        }

        private void Initialize()
        {
            var atomicMeters =
                MonoAddinsHelper.GetSelectedNodes<ProgressiveAtomicMeterNode>(ProgressiveMeterProviderExtensionPoint);

            lock (_lock)
            {
                // Allocate storage for the atomic meters
                Allocate(atomicMeters);

                // Add the two types of supported progressive meters
                AddAtomicMeters(atomicMeters);
            }
        }

        private void Allocate(IEnumerable<ProgressiveAtomicMeterNode> meters)
        {
            var blockName = GetType().ToString();

            foreach (var meter in meters)
            {
                var meterBlockName = $"{blockName}.{meter.Name}";
                var blockSize = 0;

                if (meter.Group.Equals("progressive", StringComparison.InvariantCultureIgnoreCase))
                {
                    blockSize = _meterManager.ProgressiveCount;
                }
                else if (meter.Group.Equals("progressiveLevel", StringComparison.InvariantCultureIgnoreCase))
                {
                    blockSize = _meterManager.LevelCount;
                }
                else if (meter.Group.Equals("sharedLevel", StringComparison.InvariantCultureIgnoreCase))
                {
                    blockSize = _meterManager.SharedLevelCount;
                }

                if (_persistentStorage.BlockExists(meterBlockName))
                {
                    var block = _persistentStorage.GetBlock(meterBlockName);
                    if (block.Count < blockSize)
                    {
                        _persistentStorage.ResizeBlock(meterBlockName, blockSize);
                    }
                }
                else
                {
                    var blockFormat = new BlockFormat();

                    blockFormat.AddFieldDescription(new FieldDescription(FieldType.Int64, 0, "Lifetime"));
                    blockFormat.AddFieldDescription(new FieldDescription(FieldType.Int64, 0, "Period"));

                    _persistentStorage.CreateDynamicBlock(
                        ProviderPersistenceLevel,
                        meterBlockName,
                        blockSize == 0 ? 1 : blockSize,
                        blockFormat);
                }
            }
        }

        private void AddAtomicMeters(IEnumerable<ProgressiveAtomicMeterNode> meters)
        {
            var blockName = GetType().ToString();

            foreach (var meter in meters)
            {
                var meterBlockName = $"{blockName}.{meter.Name}";

                var block = _persistentStorage.GetBlock(meterBlockName);

                if (meter.Group.Equals("progressive", StringComparison.InvariantCultureIgnoreCase))
                {
                    AddAtomicProgressiveMeter(meter, block);
                }
                else if (meter.Group.Equals("progressiveLevel", StringComparison.InvariantCultureIgnoreCase))
                {
                    AddAtomicLevelMeter(meter, block);
                }
                else if (meter.Group.Equals("sharedLevel", StringComparison.InvariantCultureIgnoreCase))
                {
                    AddAtomicSharedLevelMeter(meter, block);
                }
            }
        }
        
        private void UpdateLPCompositeMeters(object sender, EventArgs e)
        {
            var activeLinkedLevels = _levelProvider.GetProgressiveLevels().Where(
                level => level.LevelType == ProgressiveLevelType.LP
                         && _meterManager.IsMeterProvided(
                             level.DeviceId,
                             level.LevelId,
                             ProgressiveMeters.ProgressiveLevelWinOccurrence)).ToList();
            var activeLevelIds = activeLinkedLevels.Select(level => level.LevelId).Distinct().ToList();
            var levelIdIndex = 0;
            foreach (var activeLevelId in activeLevelIds)
            {
                var meterName = $"{ProgressiveMeters.LinkedProgressiveWinOccurrence}AtLevel{levelIdIndex}";
                AddMeter(
                    new CompositeMeter(
                        meterName,
                        _ =>
                        {
                            long sum = 0;
                            var sameLevelIdActiveLevels =
                                activeLinkedLevels.Where(level => level.LevelId == activeLevelId).ToList();
                            foreach (var level in sameLevelIdActiveLevels)
                            {
                                sum += _meterManager.GetMeter(
                                    level.DeviceId,
                                    level.LevelId,
                                    ProgressiveMeters.ProgressiveLevelWinOccurrence).Lifetime;
                            }

                            return sum;
                        },
                        new List<string>(),
                        new OccurrenceMeterClassification()));
                levelIdIndex++;
            }

            Invalidate();
        }

        private void AddAtomicProgressiveMeter(
            ProgressiveAtomicMeterNode meter,
            IPersistentStorageAccessor block)
        {
            var currentValues = block.GetAll();

            foreach (var (deviceId, blockIndex) in _meterManager.GetProgressiveBlocks())
            {
                var meterName = _meterManager.GetMeterName(deviceId, meter.Name);
                if (Contains(meterName))
                {
                    continue;
                }

                var lifetime = 0L;
                var period = 0L;

                if (currentValues.TryGetValue(blockIndex, out var current))
                {
                    lifetime = (long)current["Lifetime"];
                    period = (long)current["Period"];
                }

                AddMeter(new AtomicMeter(meterName, block, blockIndex, meter.Classification, this, lifetime, period));
            }
        }

        private void AddAtomicLevelMeter(ProgressiveAtomicMeterNode meter, IPersistentStorageAccessor block)
        {
            var currentValues = block.GetAll();

            foreach (var (deviceId, levelId, blockIndex) in _meterManager.GetProgressiveLevelBlocks())
            {
                var meterName = _meterManager.GetMeterName(deviceId, levelId, meter.Name);
                if (Contains(meterName))
                {
                    continue;
                }

                var lifetime = 0L;
                var period = 0L;

                if (currentValues.TryGetValue(blockIndex, out var current))
                {
                    lifetime = (long)current["Lifetime"];
                    period = (long)current["Period"];
                }

                AddMeter(new AtomicMeter(meterName, block, blockIndex, meter.Classification, this, lifetime, period));
            }
        }

        private void AddAtomicSharedLevelMeter(ProgressiveAtomicMeterNode meter, IPersistentStorageAccessor block)
        {
            var currentValues = block.GetAll();

            foreach (var (level, blockIndex) in _meterManager.GetSharedLevelBlocks())
            {
                var meterName = _meterManager.GetMeterName(level, meter.Name);
                if (Contains(meterName))
                {
                    continue;
                }

                var lifetime = 0L;
                var period = 0L;

                if (currentValues.TryGetValue(blockIndex, out var current))
                {
                    lifetime = (long)current["Lifetime"];
                    period = (long)current["Period"];
                }

                AddMeter(new AtomicMeter(meterName, block, blockIndex, meter.Classification, this, lifetime, period));
            }
        }
    }
}