namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Application.Contracts;
    using Contracts.Meters;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;
    using Contracts.Progressives.SharedSap;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Newtonsoft.Json;

    /// <summary>
    ///     An object that manages per-progressive meters.
    /// </summary>
    public class ProgressiveMeterManager : IProgressiveMeterManager
    {
        private const string DeviceIdKey = "DeviceId";
        private const string DeviceBlockIndexKey = "BlockIndex";
        private const string DeviceLevelsKey = "Levels";
        private const int MaxLinkedLevelsSize = 1024;
        private const int MaxSharedLevelsSize = 1024;

        private const string MeterNameSuffix = "Progressive";
        private const string ProgressiveLevelNameSuffix = "AtLevel";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _deviceDataBlockName;
        private readonly string _linkedLevelsBlockName;
        private readonly string _sharedLevelsBlockName;

        private readonly ConcurrentDictionary<int, ProgressiveIndices> _deviceIdMap = new();
        private readonly ConcurrentDictionary<string, int> _linkedLevelMap = new();
        private readonly ConcurrentDictionary<Guid, int> _sharedLevelMap = new();

        private readonly IMeterManager _meterManager;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly IPropertiesManager _properties;

        private int _savedLinkedLevelCount;
        private int _savedSharedLevelCount;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveMeterManager" /> class.
        /// </summary>
        public ProgressiveMeterManager(
            IMeterManager meterManager,
            IPropertiesManager properties,
            IPersistentStorageManager persistentStorage)
        {
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));

            var blockName = typeof(ProgressiveMeterManager).ToString();

            _deviceDataBlockName = $"{blockName}.Data";
            _linkedLevelsBlockName = $"{blockName}.LinkedLevels";
            _sharedLevelsBlockName = $"{blockName}.SharedLevels";

            Initialize();
        }

        private PersistenceLevel StorageLevel =>
            _properties.GetValue(ApplicationConstants.DemonstrationMode, false)
                ? PersistenceLevel.Critical
                : PersistenceLevel.Static;

        /// <inheritdoc />
        public event EventHandler<ProgressiveAddedEventArgs> ProgressiveAdded;

        /// <inheritdoc />
        public event EventHandler<LinkedProgressiveAddedEventArgs> LinkedProgressiveAdded;

        /// <inheritdoc />
        public event EventHandler<LPCompositeMetersCanUpdateEventArgs> LPCompositeMetersCanUpdate;

        /// <inheritdoc />
        public int ProgressiveCount => _deviceIdMap.Count;

        /// <inheritdoc />
        public int LevelCount
        {
            get { return _deviceIdMap.Sum(device => device.Value.Levels.Count); }
        }

        public int LinkedLevelCount => _linkedLevelMap.Count;

        public int SharedLevelCount => _sharedLevelMap.Count;

        /// <inheritdoc />
        public string Name => typeof(ProgressiveMeterManager).FullName;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IProgressiveMeterManager) };

        public void UpdateLPCompositeMeters()
        {
            LPCompositeMetersCanUpdate?.Invoke(this, new LPCompositeMetersCanUpdateEventArgs());
        }

        public void AddProgressives(IEnumerable<IViewableProgressiveLevel> progressives)
        {
            var progressiveList = progressives.ToList();

            foreach (var progressive in progressiveList)
            {
                InternalAddProgressive(progressive);
            }

            Save();

            OnProgressiveAdded(new ProgressiveAddedEventArgs(progressiveList));
        }

        public void AddProgressives(IEnumerable<IViewableSharedSapLevel> progressives)
        {
            var progressiveList = progressives.ToList();

            foreach (var progressive in progressiveList)
            {
                InternalAddProgressive(progressive);
            }

            Save();

            OnProgressiveAdded(new ProgressiveAddedEventArgs(null));
        }

        public void AddLinkedProgressives(IEnumerable<IViewableLinkedProgressiveLevel> progressives)
        {
            var progressiveList = progressives.ToList();

            foreach (var progressive in progressiveList)
            {
                InternalAddProgressive(progressive);
            }

            Save();

            OnLinkedProgressiveAdded(new LinkedProgressiveAddedEventArgs(progressiveList));
        }

        public IEnumerable<(int deviceId, int blockIndex)> GetProgressiveBlocks()
        {
            return _deviceIdMap.Select(m => (m.Key, m.Value.BlockIndex));
        }

        public IEnumerable<(int deviceId, int levelId, int blockIndex)> GetProgressiveLevelBlocks()
        {
            return _deviceIdMap.SelectMany(m => m.Value.Levels, (p, l) => (p.Key, l.Key, l.Value));
        }

        public IEnumerable<(string linkedLevelName, int blockIndex)> GetLinkedLevelBlocks()
        {
            return _linkedLevelMap.Select(m => (m.Key, m.Value));
        }

        public IEnumerable<(Guid sharedLevelId, int blockIndex)> GetSharedLevelBlocks()
        {
            return _sharedLevelMap.Select(m => (m.Key, m.Value));
        }

        /// <inheritdoc />
        public IMeter GetMeter(int deviceId, string meterName)
        {
            return _meterManager.GetMeter(GetMeterName(deviceId, meterName));
        }

        /// <inheritdoc />
        public IMeter GetMeter(string meterName)
        {
            return _meterManager.GetMeter(meterName);
        }

        /// <inheritdoc />
        public IMeter GetMeter(int deviceId, int levelId, string meterName)
        {
            return _meterManager.GetMeter(GetMeterName(deviceId, levelId, meterName));
        }

        public IMeter GetMeter(string linkedLevelName, string meterName)
        {
            return _meterManager.GetMeter(GetMeterName(linkedLevelName, meterName));
        }

        public IMeter GetMeter(Guid sharedLevelId, string meterName)
        {
            return _meterManager.GetMeter(GetMeterName(sharedLevelId, meterName));
        }

        /// <inheritdoc />
        public string GetMeterName(int deviceId, string meterName)
        {
            if (_deviceIdMap.TryGetValue(deviceId, out _))
            {
                return $"{meterName}{MeterNameSuffix}{deviceId}";
            }

            var error = $"GetMeter() failed to retrieve meter: {meterName}";
            Logger.Fatal(error);
            throw new MeterNotFoundException(error);
        }

        /// <inheritdoc />
        public string GetMeterName(int deviceId, int levelId, string meterName)
        {
            var baseMeterName = GetMeterName(deviceId, meterName);

            return $"{baseMeterName}{ProgressiveLevelNameSuffix}{levelId}";
        }

        public string GetMeterName(string linkedLevelName, string meterName)
        {
            if (_linkedLevelMap.TryGetValue(linkedLevelName, out var linkedLevelIndex))
            {
                return $"{meterName}{linkedLevelIndex}";
            }

            var error = $"GetMeter() failed to retrieve meter: {meterName}";
            Logger.Fatal(error);
            throw new MeterNotFoundException(error);
        }

        public string GetMeterName(Guid sharedLevelId, string meterName)
        {
            if (_sharedLevelMap.TryGetValue(sharedLevelId, out _))
            {
                return $"{meterName}{MeterNameSuffix}{sharedLevelId}";
            }

            var error = $"GetMeter() failed to retrieve meter: {meterName}";
            Logger.Fatal(error);
            throw new MeterNotFoundException(error);
        }

        /// <inheritdoc />
        public bool IsMeterProvided(int deviceId, string meterName)
        {
            return _deviceIdMap.TryGetValue(deviceId, out _) &&
                   _meterManager.IsMeterProvided(GetMeterName(deviceId, meterName));
        }

        /// <inheritdoc />
        public bool IsMeterProvided(string meterName)
        {
            return _meterManager.IsMeterProvided(meterName);
        }

        /// <inheritdoc />
        public bool IsMeterProvided(int deviceId, int levelId, string meterName)
        {
            return _deviceIdMap.TryGetValue(deviceId, out _) &&
                   _meterManager.IsMeterProvided(GetMeterName(deviceId, levelId, meterName));
        }

        public bool IsMeterProvided(string linkedLevelName, string meterName)
        {
            return _linkedLevelMap.TryGetValue(linkedLevelName, out _) &&
                   _meterManager.IsMeterProvided(GetMeterName(linkedLevelName, meterName));
        }

        public bool IsMeterProvided(Guid sharedLevelId, string meterName)
        {
            return _sharedLevelMap.TryGetValue(sharedLevelId, out _) &&
                   _meterManager.IsMeterProvided(GetMeterName(sharedLevelId, meterName));
        }

        /// <inheritdoc />
        public void Initialize()
        {
            InitializeDataBlock(_deviceDataBlockName, LoadDeviceData);
            InitializeDataBlock(_linkedLevelsBlockName, LoadLinkedLevels);
            InitializeDataBlock(_sharedLevelsBlockName, LoadSharedLevels);
        }

        private void InitializeDataBlock(string blockName, Action loadDataCallback)
        {
            var dataBlockExists = _persistentStorage.BlockExists(blockName);
            if (dataBlockExists)
            {
                loadDataCallback();
            }
            else
            {
                _persistentStorage.CreateBlock(StorageLevel, blockName, 1);
            }
        }

        private static Dictionary<int, int> ToLevelMap(string data)
        {
            return !string.IsNullOrEmpty(data)
                ? JsonConvert.DeserializeObject<Dictionary<int, int>>(data)
                : new Dictionary<int, int>();
        }

        private void InternalAddProgressive(IViewableProgressiveLevel progressive)
        {
            _deviceIdMap.AddOrUpdate(
                progressive.DeviceId,
                new ProgressiveIndices
                {
                    BlockIndex = ProgressiveCount,
                    Levels = new Dictionary<int, int> { { progressive.LevelId, LevelCount } }
                },
                (key, data) =>
                {
                    data.Levels.Add(progressive.LevelId, LevelCount);
                    return data;
                });
        }

        private void InternalAddProgressive(IViewableLinkedProgressiveLevel linkedLevel)
        {
            _linkedLevelMap.TryAdd(linkedLevel.LevelName, LinkedLevelCount);
        }

        private void InternalAddProgressive(IViewableSharedSapLevel sharedLevel)
        {
            _sharedLevelMap.TryAdd(sharedLevel.Id, SharedLevelCount);
        }

        private void OnProgressiveAdded(ProgressiveAddedEventArgs e)
        {
            ProgressiveAdded?.Invoke(this, e);
        }

        private void OnLinkedProgressiveAdded(LinkedProgressiveAddedEventArgs e)
        {
            LinkedProgressiveAdded?.Invoke(this, e);
        }

        private void LoadDeviceData()
        {
            var data = _persistentStorage.GetBlock(_deviceDataBlockName).GetAll();

            foreach (var result in data)
            {
                var meterInfo = result.Value;

                var deviceId = (int)meterInfo[DeviceIdKey];

                if (deviceId == 0)
                {
                    continue;
                }

                _deviceIdMap.TryAdd(
                    deviceId,
                    new ProgressiveIndices
                    {
                        BlockIndex = (int)meterInfo[DeviceBlockIndexKey],
                        Levels = ToLevelMap((string)meterInfo[DeviceLevelsKey])
                    });
            }
        }

        private void LoadLinkedLevels()
        {
            var blocks = _persistentStorage.GetBlock(_linkedLevelsBlockName).GetAll();
            foreach (var block in blocks)
            {
                var blockInfo = block.Value;
                var blockLevels = (string)blockInfo[DeviceLevelsKey];
                if (!string.IsNullOrEmpty(blockLevels))
                {
                    var deserializedBlockLevels = JsonConvert.DeserializeObject<Dictionary<string, int>>(blockLevels);
                    foreach (var level in deserializedBlockLevels)
                    {
                        _linkedLevelMap.TryAdd(level.Key, level.Value);
                    }
                }
            }
            _savedLinkedLevelCount = _linkedLevelMap.Count;
        }

        private void LoadSharedLevels()
        {
            var blocks = _persistentStorage.GetBlock(_sharedLevelsBlockName).GetAll();
            foreach (var block in blocks)
            {
                var blockInfo = block.Value;
                var blockLevels = (string)blockInfo[DeviceLevelsKey];
                if (!string.IsNullOrEmpty(blockLevels))
                {
                    var deserializedBlockLevels = JsonConvert.DeserializeObject<Dictionary<Guid, int>>(blockLevels);
                    foreach (var level in deserializedBlockLevels)
                    {
                        _sharedLevelMap.TryAdd(level.Key, level.Value);
                    }
                }
            }
            _savedSharedLevelCount = _sharedLevelMap.Count;
        }

        private void Save()
        {
            SaveDeviceData();
            SaveLinkedData();
            SaveSharedData();
        }

        private void SaveDeviceData()
        {
            var blockDeviceData = _persistentStorage.GetBlock(_deviceDataBlockName);

            if (ProgressiveCount > blockDeviceData.Count)
            {
                Logger.Debug($"Resizing {_deviceDataBlockName} from {blockDeviceData.Count} to {ProgressiveCount} blocks");
                _persistentStorage.ResizeBlock(_deviceDataBlockName, ProgressiveCount);
            }

            using var transaction = blockDeviceData.StartTransaction();
            foreach (var item in _deviceIdMap)
            {
                var blockIndex = item.Value.BlockIndex;
                transaction[blockIndex, DeviceIdKey] = item.Key;
                transaction[blockIndex, DeviceBlockIndexKey] = blockIndex;
                transaction[blockIndex, DeviceLevelsKey] =
                    JsonConvert.SerializeObject(item.Value.Levels, Formatting.None);
            }

            transaction.Commit();
        }

        private void SaveLinkedData()
        {
            if (_linkedLevelMap.Count <= 0 || _linkedLevelMap.Count == _savedLinkedLevelCount)
            {
                return;
            }

            var blockLinkedLevels = _persistentStorage.GetBlock(_linkedLevelsBlockName);
            var blockIndex = blockLinkedLevels.Count;
            var serializedLinkedLevels = new StringBuilder();

            foreach (var linkedLevel in _linkedLevelMap)
            {
                var serializedLinkedLevel = JsonConvert.SerializeObject(linkedLevel, Formatting.None);
                if (serializedLinkedLevels.Length + serializedLinkedLevel.Length > MaxLinkedLevelsSize)
                {
                    serializedLinkedLevels.Clear();
                    serializedLinkedLevels.Append(serializedLinkedLevel);
                    if (blockIndex + 1 > blockLinkedLevels.Count)
                    {
                        Logger.Debug($"Resizing {_linkedLevelsBlockName} from {blockIndex} to {blockIndex + 1} blocks");
                        _persistentStorage.ResizeBlock(_linkedLevelsBlockName, blockIndex + 1);
                    }
                }
                else
                {
                    serializedLinkedLevels.Append(serializedLinkedLevel);
                }

                blockIndex++;
            }

            serializedLinkedLevels.Clear();
            using (var transaction = blockLinkedLevels.StartTransaction())
            {
                blockIndex = 0;
                var linkedLevelMap = new ConcurrentDictionary<string, int>();
                foreach (var linkedLevel in _linkedLevelMap)
                {
                    var serializedLinkedLevel = JsonConvert.SerializeObject(linkedLevel, Formatting.None);
                    if (serializedLinkedLevels.Length + serializedLinkedLevel.Length > MaxLinkedLevelsSize)
                    {
                        transaction[blockIndex, DeviceBlockIndexKey] = blockIndex;
                        transaction[blockIndex, DeviceLevelsKey] =
                            JsonConvert.SerializeObject(linkedLevelMap, Formatting.None);
                        linkedLevelMap.Clear();
                        serializedLinkedLevels.Clear();
                        serializedLinkedLevels.Append(serializedLinkedLevel);
                        blockIndex++;
                    }
                    else
                    {
                        linkedLevelMap.TryAdd(linkedLevel.Key, linkedLevel.Value);
                        serializedLinkedLevels.Append(serializedLinkedLevel);
                    }
                }

                if (linkedLevelMap.Count > 0)
                {
                    transaction[blockIndex, DeviceBlockIndexKey] = blockIndex;
                    transaction[blockIndex, DeviceLevelsKey] =
                        JsonConvert.SerializeObject(linkedLevelMap, Formatting.None);
                }

                transaction.Commit();
            }

            _savedLinkedLevelCount = _linkedLevelMap.Count;
        }

        private void SaveSharedData()
        {
            if (_sharedLevelMap.Count <= 0 || _sharedLevelMap.Count == _savedSharedLevelCount)
            {
                return;
            }

            var blockSharedLevels = _persistentStorage.GetBlock(_sharedLevelsBlockName);
            var blockIndex = blockSharedLevels.Count;
            var serializedSharedLevels = new StringBuilder();

            foreach (var sharedLevel in _sharedLevelMap)
            {
                var serializedSharedLevel = JsonConvert.SerializeObject(sharedLevel, Formatting.None);
                if (serializedSharedLevels.Length + serializedSharedLevel.Length > MaxSharedLevelsSize)
                {
                    serializedSharedLevels.Clear();
                    serializedSharedLevels.Append(serializedSharedLevel);
                    if (blockIndex + 1 > blockSharedLevels.Count)
                    {
                        Logger.Debug($"Resizing {_sharedLevelsBlockName} from {blockIndex} to {blockIndex + 1} blocks");
                        _persistentStorage.ResizeBlock(_sharedLevelsBlockName, blockIndex + 1);
                    }
                }
                else
                {
                    serializedSharedLevels.Append(serializedSharedLevel);
                }

                blockIndex++;
            }

            serializedSharedLevels.Clear();
            using (var transaction = blockSharedLevels.StartTransaction())
            {
                blockIndex = 0;
                var sharedLevelMap = new ConcurrentDictionary<Guid, int>();
                foreach (var sharedLevel in _sharedLevelMap)
                {
                    var serializedSharedLevel = JsonConvert.SerializeObject(sharedLevel, Formatting.None);
                    if (serializedSharedLevels.Length + serializedSharedLevel.Length > MaxSharedLevelsSize)
                    {
                        transaction[blockIndex, DeviceBlockIndexKey] = blockIndex;
                        transaction[blockIndex, DeviceLevelsKey] =
                            JsonConvert.SerializeObject(sharedLevelMap, Formatting.None);
                        sharedLevelMap.Clear();
                        serializedSharedLevels.Clear();
                        serializedSharedLevels.Append(serializedSharedLevel);
                        blockIndex++;
                    }
                    else
                    {
                        sharedLevelMap.TryAdd(sharedLevel.Key, sharedLevel.Value);
                        serializedSharedLevels.Append(serializedSharedLevel);
                    }
                }

                if (sharedLevelMap.Count > 0)
                {
                    transaction[blockIndex, DeviceBlockIndexKey] = blockIndex;
                    transaction[blockIndex, DeviceLevelsKey] =
                        JsonConvert.SerializeObject(sharedLevelMap, Formatting.None);
                }

                transaction.Commit();
            }

            _savedSharedLevelCount = _sharedLevelMap.Count;
        }

        private class ProgressiveIndices
        {
            public int BlockIndex { get; set; }

            public Dictionary<int, int> Levels { get; set; }
        }
    }
}