namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Common.PerformanceCounters;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Runtime.Client;

    public interface ILocalStorageProvider
    {
        void ClearActiveGame();

        void ActivateGame();

        void SetStorage(StorageType storageType, IDictionary<string, string> values);

        void SetStorage(IEnumerable<KeyValuePair<StorageType, IDictionary<string, string>>> updateValues);

        IDictionary<string, string> GetStorage(StorageType storageType);

        void ClearLocalData(StorageType storageType, int gameId, long denomId);

        void ClearLocalData(StorageType storageType);
    }

    public class LocalStorageProvider : ILocalStorageProvider
    {
        private static readonly StorageType[] StorageTypes =
        {
            StorageType.GameLocalSession, StorageType.GamePlayerSession
        };

        private readonly IGameStorage _gameStorage;
        private readonly IPropertiesManager _properties;

        private readonly object _sync = new();
        private readonly Dictionary<StorageType, Dictionary<string, string>> _gameCache;
        private int _cachedGameId;
        private long _cachedDenomId;

        public LocalStorageProvider(IGameStorage gameStorage, IPropertiesManager properties)
        {
            _gameStorage = gameStorage;
            _properties = properties;
            _gameCache = new Dictionary<StorageType, Dictionary<string, string>>();
            LoadCommonSettings();
        }

        public void ClearActiveGame()
        {
            lock (_sync)
            {
                _cachedDenomId = 0;
                _cachedGameId = 0;
                foreach (var storageType in StorageTypes)
                {
                    _gameCache[storageType] = new Dictionary<string, string>();
                }
            }
        }

        public void ActivateGame()
        {
            var gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
            var denomId = _properties.GetValue(GamingConstants.SelectedDenom, 0L);

            lock (_sync)
            {
                if (gameId == _cachedGameId && denomId == _cachedDenomId)
                {
                    return;
                }

                foreach (var storageType in StorageTypes)
                {
                    var perGame = _gameStorage.GetValue<Dictionary<string, string>>(
                        gameId,
                        denomId,
                        storageType.ToString()) ?? new Dictionary<string, string>();

                    _gameCache[storageType] = perGame;
                }

                _cachedDenomId = denomId;
                _cachedGameId = gameId;
            }
        }

        public void SetStorage(StorageType storageType, IDictionary<string, string> values)
        {
            var gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
            var denomId = _properties.GetValue(GamingConstants.SelectedDenom, 0L);

            lock (_sync)
            {
                if (gameId != _cachedGameId || denomId != _cachedDenomId)
                {
                    ActivateGame();
                }

                var perGame = _gameCache[storageType];
                var updated = false;
                values.ToList().ForEach(
                    x =>
                    {
                        if (perGame.ContainsKey(x.Key) && perGame[x.Key] == x.Value)
                        {
                            return;
                        }

                        perGame[x.Key] = x.Value;
                        updated = true;
                    });

                if (!updated)
                {
                    return;
                }

                if (StorageTypes.Contains(storageType))
                {
                    _gameStorage.SetValue(gameId, denomId, storageType.ToString(), perGame);
                }
                else
                {
                    _gameStorage.SetValue(storageType.ToString(), perGame);
                }
            }
        }

        public void SetStorage(IEnumerable<KeyValuePair<StorageType, IDictionary<string, string>>> updateValues)
        {
            lock (_sync)
            {
                foreach (var value in updateValues)
                {
                    SetStorage(value.Key, value.Value);
                }
            }
        }

        public IDictionary<string, string> GetStorage(StorageType storageType)
        {
            lock (_sync)
            {
                var gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
                var denomId = _properties.GetValue(GamingConstants.SelectedDenom, 0L);
                if (gameId != _cachedGameId || denomId != _cachedDenomId)
                {
                    ActivateGame();
                }

                return _gameCache[storageType];
            }
        }

        public void ClearLocalData(StorageType storageType, int gameId, long denomId)
        {
            if (!StorageTypes.Contains(storageType))
            {
                throw new InvalidOperationException();
            }

            lock (_sync)
            {
                var storage = new Dictionary<string, string>();
                if (gameId == _cachedGameId && denomId == _cachedDenomId)
                {
                    _gameCache[storageType] = storage;
                }

                _gameStorage.SetValue(gameId, denomId, storageType.ToString(), storage);
            }
        }

        public void ClearLocalData(StorageType storageType)
        {
            if (StorageTypes.Contains(storageType))
            {
                throw new InvalidOperationException();
            }

            lock (_sync)
            {
                _gameCache[storageType] = new Dictionary<string, string>();
                _gameStorage.SetValue(storageType.ToString(), _gameCache[storageType]);
            }
        }

        private void LoadCommonSettings()
        {
            lock (_sync)
            {
                var localSession =
                    _gameStorage.GetValue<Dictionary<string, string>>(StorageType.LocalSession.ToString()) ??
                    new Dictionary<string, string>();

                var playerSession =
                    _gameStorage.GetValue<Dictionary<string, string>>(StorageType.PlayerSession.ToString()) ??
                    new Dictionary<string, string>();
                _gameCache[StorageType.LocalSession] = localSession;
                _gameCache[StorageType.PlayerSession] = playerSession;
            }
        }
    }

    [CounterDescription("Set Local Storage", PerformanceCounterType.AverageTimer32)]
    public class SetLocalStorageCommandHandler : ICommandHandler<SetLocalStorage>
    {
        private readonly ILocalStorageProvider _localStorageProvider;
        private readonly IPersistentStorageManager _persistentStorage;

        public SetLocalStorageCommandHandler(
            ILocalStorageProvider localStorageProvider,
            IPersistentStorageManager persistentStorage)
        {
            _localStorageProvider = localStorageProvider ?? throw new ArgumentNullException(nameof(localStorageProvider));
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
        }

        public void Handle(SetLocalStorage command)
        {
            if (command.Values.Count <= 0)
            {
                return;
            }

            using var scope = _persistentStorage.ScopedTransaction();
            _localStorageProvider.SetStorage(command.Values);
            scope.Complete();
        }
    }
}