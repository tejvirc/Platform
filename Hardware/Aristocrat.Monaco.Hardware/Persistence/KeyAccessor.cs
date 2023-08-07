namespace Aristocrat.Monaco.Hardware.Persistence
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts.Persistence;
    using log4net;

    /// <summary> A key accessor. </summary>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Persistence.KeyValueAccessor" />
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.Contracts.Persistence.IKeyLevelAccessor" />
    public class KeyAccessor : IKeyAccessor
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly ConcurrentDictionary<string, PersistenceLevel> _cache =
            new ConcurrentDictionary<string, PersistenceLevel>();

        private readonly IPersistentStore _store;

        public KeyAccessor(IPersistentStore store)
        {
            _store = store;
            InitializeCache();
        }

        /// <inheritdoc />
        public bool GetValue<T>(out T value)
        {
            return GetValueInternal(KeyCreator.Key(typeof(T)), out value);
        }

        /// <inheritdoc />
        public bool GetValue<T>(string key, out T value)
        {
            return GetValueInternal(KeyCreator.Key(key, typeof(T)), out value);
        }

        /// <inheritdoc />
        public bool GetValue<T>(int index, out T value)
        {
            return GetValueInternal(KeyCreator.IndexedKey(index, typeof(T)), out value);
        }

        /// <inheritdoc />
        public bool GetValue<T>(string key, int index, out T value)
        {
            return GetValueInternal(KeyCreator.IndexedKey(key, index, typeof(T)), out value);
        }

        /// <inheritdoc />
        public T GetOrCreateValue<T>() where T : new()
        {
            var exists = GetValue(out T result);
            if (exists)
            {
                return result;
            }

            result = new T();
            SetValue(result);

            return result;
        }

        /// <inheritdoc />
        public T GetOrCreateValue<T>(string key) where T : new()
        {
            var exists = GetValue(key, out T result);
            if (exists)
            {
                return result;
            }

            result = new T();
            SetValue(key, result);

            return result;
        }

        /// <inheritdoc />
        public bool SetValue<T>(T value)
        {
            var generatedKey = KeyCreator.Key(typeof(T));
            if (!PersistenceLevelExists(generatedKey))
            {
                Logger.Error($"No persistence level set for the key {generatedKey}");
                return false;
            }

            return SetValueInternal(generatedKey, value);
        }

        /// <inheritdoc />
        public bool SetValue<T>(string key, T value)
        {
            var prefix = KeyCreator.KeyPrefix(key);
            if (!string.IsNullOrEmpty(prefix))
            {
                if (!PersistenceLevelExists(prefix))
                {
                    Logger.Error($"No persistence level set for the key {prefix}");
                    return false;
                }
            }
            else
            {
                if (!PersistenceLevelExists(key))
                {
                    Logger.Error($"No persistence level set for the key {key}");
                    return false;
                }
            }

            return SetValueInternal(KeyCreator.Key(key, typeof(T)), value);
        }

        /// <inheritdoc />
        public bool SetValue<T>(int index, T value)
        {
            var key = KeyCreator.Key(typeof(T));
            if (!PersistenceLevelExists(key))
            {
                Logger.Error($"No persistence level set for key {key}");
                return false;
            }

            return SetValueInternal(KeyCreator.IndexedKey(index, typeof(T)), value);
        }

        /// <inheritdoc />
        public bool SetValue<T>(string key, int index, T value)
        {
            var prefix = KeyCreator.KeyPrefix(key);
            if (!string.IsNullOrEmpty(prefix))
            {
                if (!PersistenceLevelExists(prefix))
                {
                    Logger.Error($"No persistence level set for the key {prefix}");
                    return false;
                }
            }
            else
            {
                if (!PersistenceLevelExists(key))
                {
                    Logger.Error($"No persistence level set for the key {key}");
                    return false;
                }
            }

            return SetValueInternal(KeyCreator.IndexedKey(key, index, typeof(T)), value);
        }

        /// <inheritdoc />
        public bool Commit(IEnumerable<((string prefix, int? index) key, object value)> values)
        {
            if (values == null)
            {
                return false;
            }

            var collection = values.ToList();
            foreach (var ((key, _), _) in collection)
            {
                if (PersistenceLevelExists(key))
                {
                    continue;
                }

                var prefix = KeyCreator.KeyPrefix(key);
                if (PersistenceLevelExists(prefix))
                {
                    continue;
                }

                Logger.Error($"No persistence level set for the key {key} or {prefix}");
                return false;
            }

            var updates = new List<(string key, object value)>();
            foreach (var ((prefix, index), value) in collection)
            {
                updates.Add(
                    index != null
                        ? (KeyCreator.IndexedKey(prefix, index.Value, value.GetType()), value)
                        : (KeyCreator.Key(prefix, value.GetType()), value));
            }

            return _store.AddOrUpdateValue(updates);
        }

        /// <inheritdoc />
        public bool Clear(PersistenceLevel level)
        {
            var snapshot = _cache.ToArray();
            var levelKeys = new List<string>();
            var valueKeys = new List<string>();

            foreach (var item in snapshot)
            {
                if (item.Value == level)
                {
                    levelKeys.Add(item.Key);
                }
            }

            if (levelKeys.Count <= 0)
            {
                return true;
            }

            foreach (var key in _store.ValueKeys())
            {
                if (levelKeys.Contains(key) || levelKeys.Contains(KeyCreator.KeyPrefix(key)))
                {
                    valueKeys.Add(key);
                }
            }

            if (!_store.TryRemoveValue(valueKeys))
            {
                return false;
            }

            InitializeCache();
            return true;
        }

        /// <inheritdoc />
        public bool Verify(bool full)
        {
            return _store.Verify(full);
        }

        /// <inheritdoc />
        public bool GetPersistenceLevel(string key, out PersistenceLevel level)
        {
            if (_cache.TryGetValue(key, out level))
            {
                return true;
            }

            level = default(PersistenceLevel);
            return false;
        }

        /// <inheritdoc />
        public bool SetPersistenceLevel(string key, PersistenceLevel level)
        {
            var result = _store.AddOrUpdateLevel(key, level);
            if (result)
            {
                _cache[key] = level;
            }

            return result;
        }

        private void InitializeCache()
        {
            _cache.Clear();
            foreach (var item in _store.LevelData())
            {
                _cache[item.Key] = item.Value;
            }
        }

        private bool GetValueInternal<T>(string key, out T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                value = default(T);
                return false;
            }

            return _store.TryGetValue(key, out value);
        }

        private bool SetValueInternal<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            return _store.AddOrUpdateValue(key, value);
        }

        private bool PersistenceLevelExists(string key)
        {
            return _cache.TryGetValue(key, out _);
        }
    }
}