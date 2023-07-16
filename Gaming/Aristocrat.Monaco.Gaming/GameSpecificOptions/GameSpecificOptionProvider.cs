namespace Aristocrat.Monaco.Gaming.GameSpecificOptions
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Contracts.GameSpecificOptions;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Newtonsoft.Json;

    public class GameSpecificOptionProvider : IGameSpecificOptionProvider, IService, IDisposable
    {
        private const int BlockIndex = 0;
        private const string PersistentKey = nameof(GameSpecificOptionProvider);
        
        private readonly IPersistentStorageManager _storageManager;
        private readonly IPersistenceProvider _persistenceProvider;

        private PersistenceLevel _persistenceLevel;
        private ConcurrentDictionary<string, string> _persistenceCache = new ConcurrentDictionary<string, string>();

        private readonly ConcurrentDictionary<string, IList<GameSpecificOption>> _gameSpecificOptionsCache =
            new ConcurrentDictionary<string, IList<GameSpecificOption>>();

        private bool _blockExists;

        public GameSpecificOptionProvider(
            IPersistentStorageManager storageManager,
            IPersistenceProvider persistenceProvider,
            IPropertiesManager propertiesManager)
        {
            _storageManager = storageManager ?? throw new ArgumentNullException(nameof(storageManager));
            _persistenceProvider = persistenceProvider ?? throw new ArgumentNullException(nameof(persistenceProvider));

            if(propertiesManager == null)
                throw new ArgumentNullException(nameof(propertiesManager));

            _persistenceLevel = propertiesManager.GetValue(ApplicationConstants.DemonstrationMode, false)
                    ? PersistenceLevel.Transient
                    : PersistenceLevel.Static;

            _blockExists = false;
            if (_persistenceProvider.GetBlock(PersistentKey) != null)
            {
                _blockExists = true;
            }
            
            InitPersistenceCacheFromStorage();
        }

        public void Initialize()
        {
        }

        public string Name { get; set; }

        public ICollection<Type> ServiceTypes => new[] { typeof(IGameSpecificOptionProvider) };

        public bool HasThemeId(string themeId)
        {
            return _gameSpecificOptionsCache.ContainsKey(themeId);
        }

        public string GetCurrentOptionsForGDK(string themeId)
        {
            var options = GetGameSpecificOptions(themeId);
            if (!options.Any())
            {
                return string.Empty;
            }

            return JsonConvert.SerializeObject(new
            {
                GameSpecificOptions = options.Select(g => new
                {
                    g.Name,
                    g.Value
                })
            }, Formatting.None);
        }

        public IEnumerable<GameSpecificOption> GetGameSpecificOptions(string themeId)
        {
            return _gameSpecificOptionsCache.TryGetValue(themeId, out var options)
                ? options
                : Enumerable.Empty<GameSpecificOption>();
        }

        public void InitGameSpecificOptionsCache(string themeId, IList<GameSpecificOption> options)
        {
            if(options == null) return;

            using (var scope = _storageManager.ScopedTransaction())
            {
                _gameSpecificOptionsCache.AddOrUpdate(themeId, options, (key, oldValue) => options);

                if (!_blockExists) // First time
                {
                    UpdatePersistenceCache();
                }
                else // After reboot
                {
                    InitFromPersistenceCache();
                }

                scope.Complete();
            }
        }

        public void UpdateGameSpecificOptionsCache(string themeId, IList<GameSpecificOption> options)
        {
            using (var scope = _storageManager.ScopedTransaction())
            {
                _gameSpecificOptionsCache.AddOrUpdate(themeId, options, (key, oldValue) => options);
                UpdatePersistenceCache();

                scope.Complete();
            }
        }

        private void UpdatePersistenceCache()
        {
            foreach (var id in _gameSpecificOptionsCache.Keys)
            {
                foreach (var item in _gameSpecificOptionsCache[id])
                {
                    _persistenceCache.AddOrUpdate(id + "_" + item.Name, item.Value, (key, oldValue) => item.Value);
                }
            }
            SaveToPersistenceStorage();
        }

        private void InitFromPersistenceCache()
        {
            foreach (var id in _gameSpecificOptionsCache.Keys)
            {
                foreach (var option in _gameSpecificOptionsCache[id])
                {
                    string result;
                    if (_persistenceCache.TryGetValue(GetKeyFromName(option.Name, id), out result))
                    {
                        option.Value = result;
                    }
                }
            }
        }

        private string GetKeyFromName(string name, string prefix)
        {
            return prefix + "_" + name;
        }

        public void SaveToPersistenceStorage()
        {
            using (var persistentBlock = _persistenceProvider.GetOrCreateBlock(PersistentKey, _persistenceLevel))
            {
                var transaction = persistentBlock.Transaction();
                transaction.SetValue(BlockIndex, _persistenceCache);
                transaction.Commit();

                persistentBlock?.Dispose();
            }
        }

        private void InitPersistenceCacheFromStorage()
        {
            using (var persistentBlock = _persistenceProvider.GetOrCreateBlock(PersistentKey, _persistenceLevel))
            {
                if (!persistentBlock.GetValue(BlockIndex, out _persistenceCache))
                {
                    _persistenceCache = new ConcurrentDictionary<string, string>();
                }

                persistentBlock?.Dispose();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}