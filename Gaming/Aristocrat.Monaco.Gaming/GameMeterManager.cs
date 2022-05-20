namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Application.Contracts;
    using Contracts;
    using Contracts.Meters;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Newtonsoft.Json;

    /// <summary>
    ///     An object that manages per-game meters.
    /// </summary>
    public class GameMeterManager : IGameMeterManager
    {
        private const string DeviceCountKey = "DeviceCount";
        private const string GameIdKey = "GameId";
        private const string BlockIndexKey = "BlockIndex";
        private const string DenomMap = "DenomMap";

        private const string MeterNameSuffix = "Game";
        private const string BetLevelNameSuffix = "AtBetLevel";
        private const string WagerCategoryNameSuffix = "AtWagerCategory";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _blockDeviceCountName;
        private readonly string _blockGameDataName;

        private readonly Dictionary<int, GameIndices> _gameIdMap = new Dictionary<int, GameIndices>();

        private readonly IMeterManager _meterManager;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameMeterManager" /> class.
        /// </summary>
        /// <param name="meterManager">The meter manager</param>
        /// <param name="properties">The property manager</param>
        /// <param name="persistentStorage">The storage manager</param>
        public GameMeterManager(
            IMeterManager meterManager,
            IPropertiesManager properties,
            IPersistentStorageManager persistentStorage)
        {
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));

            var blockName = GetType().ToString();
            _blockDeviceCountName = blockName + ".DeviceCount";
            _blockGameDataName = blockName + ".Data";

            Initialize();
        }

        private PersistenceLevel PersistenceLevel => _properties.GetValue(ApplicationConstants.DemonstrationMode, false)
            ? PersistenceLevel.Critical
            : PersistenceLevel.Static;

        public int GameCount { get; private set; }

        public int GameIdCount => _gameIdMap.Count;

        public int DenominationCount { get; private set; }

        public int WagerCategoryCount { get; private set; }

        /// <inheritdoc />
        public event EventHandler<GameAddedEventArgs> GameAdded;

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IGameMeterManager) };

        /// <inheritdoc />
        public bool AddGame(IGameDetail game)
        {
            if (_gameIdMap.ContainsKey(game.Id))
            {
                return false;
            }

            var gameIndices = ToGameIndices(game);
            _gameIdMap.Add(game.Id, gameIndices);

            Save();

            OnGameAdded(new GameAddedEventArgs(new List<IGameProfile> { game }));

            return true;
        }

        /// <inheritdoc />
        public void AddGames(IReadOnlyCollection<IGameDetail> games)
        {
            if (games.Count == 0)
            {
                return;
            }

            foreach (var game in games)
            {
                if (_gameIdMap.ContainsKey(game.Id))
                {
                    continue;
                }

                var gameIndices = ToGameIndices(game);
                _gameIdMap.Add(game.Id, gameIndices);
            }

            Save();

            OnGameAdded(new GameAddedEventArgs(games));
        }

        /// <inheritdoc />
        public int GetBlockIndex(int gameId)
        {
            if (!_gameIdMap.TryGetValue(gameId, out var gameIndices))
            {
                return -1;
            }

            return gameIndices.BlockIndex;
        }

        public int GetBlockIndex(int gameId, long betAmount)
        {
            if (!_gameIdMap.TryGetValue(gameId, out var gameIndices))
            {
                return -1;
            }

            if (!gameIndices.Denominations.TryGetValue(betAmount, out var denomIndices))
            {
                return -1;
            }

            return denomIndices.BlockIndex;
        }

        public int GetBlockIndex(int gameId, long betAmount, string wagerCategory)
        {
            if (!_gameIdMap.TryGetValue(gameId, out var gameIndices))
            {
                return -1;
            }

            if (!gameIndices.Denominations.TryGetValue(betAmount, out var denomIndex))
            {
                return -1;
            }

            if (!denomIndex.WagerCategories.TryGetValue(wagerCategory, out var blockIndex))
            {
                return -1;
            }

            return blockIndex;
        }

        /// <inheritdoc />
        public IMeter GetMeter(string meterName)
        {
            return _meterManager.GetMeter(meterName);
        }

        /// <inheritdoc />
        public IMeter GetMeter(int gameId, string meterName)
        {
            return _meterManager.GetMeter(GetMeterName(gameId, meterName));
        }

        /// <inheritdoc />
        public IMeter GetMeter(int gameId, long betAmount, string meterName)
        {
            return _meterManager.GetMeter(GetMeterName(gameId, betAmount, meterName));
        }

        /// <inheritdoc />
        public IMeter GetMeter(int gameId, string wagerCategory, string meterName)
        {
            return _meterManager.GetMeter(GetMeterName(gameId, wagerCategory, meterName));
        }

        public IMeter GetMeter(int gameId, long betAmount, string wagerCategory, string meterName)
        {
            return _meterManager.GetMeter(GetMeterName(gameId, betAmount, wagerCategory, meterName));
        }

        /// <inheritdoc />
        public IMeter GetMeter(long betAmount, string meterName)
        {
            return _meterManager.GetMeter(GetMeterName(betAmount, meterName));
        }

        /// <inheritdoc />
        public string GetMeterName(int gameId, string meterName)
        {
            if (!_gameIdMap.TryGetValue(gameId, out _))
            {
                var error = $"GetMeter() failed to retrieve meter: {meterName}";
                Logger.Fatal(error);
                throw new MeterNotFoundException(error);
            }

            return $"{meterName}{MeterNameSuffix}{gameId}";
        }

        /// <inheritdoc />
        public string GetMeterName(long betAmount, string meterName)
        {
            return $"{meterName}{BetLevelNameSuffix}{betAmount}";
        }

        /// <inheritdoc />
        public string GetMeterName(int gameId, long betAmount, string meterName)
        {
            var baseMeterName = GetMeterName(gameId, meterName);

            return $"{baseMeterName}{BetLevelNameSuffix}{betAmount}";
        }

        /// <inheritdoc />
        public string GetMeterName(int gameId, string wagerCategory, string meterName)
        {
            var baseMeterName = GetMeterName(gameId, meterName);

            return $"{baseMeterName}{WagerCategoryNameSuffix}{wagerCategory}";
        }

        public string GetMeterName(int gameId, long betAmount, string wagerCategory, string meterName)
        {
            var baseMeterName = GetMeterName(gameId, meterName);

            return $"{baseMeterName}{BetLevelNameSuffix}{betAmount}{WagerCategoryNameSuffix}{wagerCategory}";
        }

        /// <inheritdoc />
        public bool IsMeterProvided(string meterName)
        {
            return _meterManager.IsMeterProvided(meterName);
        }

        /// <inheritdoc />
        public bool IsMeterProvided(int gameId, string meterName)
        {
            return _meterManager.IsMeterProvided(GetMeterName(gameId, meterName));
        }

        /// <inheritdoc />
        public bool IsMeterProvided(int gameId, long betAmount, string meterName)
        {
            return _meterManager.IsMeterProvided(GetMeterName(gameId, betAmount, meterName));
        }

        /// <inheritdoc />
        public bool IsMeterProvided(int gameId, string wagerCategory, string meterName)
        {
            return _meterManager.IsMeterProvided(GetMeterName(gameId, wagerCategory, meterName));
        }

        public bool IsMeterProvided(int gameId, long betAmount, string wagerCategory, string meterName)
        {
            return _meterManager.IsMeterProvided(GetMeterName(gameId, betAmount, wagerCategory, meterName));
        }

        /// <inheritdoc />
        public bool IsMeterProvided(long betAmount, string meterName)
        {
            return _meterManager.IsMeterProvided(GetMeterName(betAmount, meterName));
        }

        /// <inheritdoc />
        public void Initialize()
        {
            var countBlockExists = _persistentStorage.BlockExists(_blockDeviceCountName);
            var dataBlockExists = _persistentStorage.BlockExists(_blockGameDataName);

            if (countBlockExists && dataBlockExists)
            {
                LoadData();
            }
            else
            {
                if (!countBlockExists)
                {
                    _persistentStorage.CreateBlock(PersistenceLevel, _blockDeviceCountName, 1);
                }

                if (!dataBlockExists)
                {
                    _persistentStorage.CreateBlock(PersistenceLevel, _blockGameDataName, 1);
                }

                AddGames(_properties.GetValues<IGameDetail>(GamingConstants.AllGames).ToList());
            }
        }

        private static Dictionary<long, DenomIndices> ToDenomMap(string data)
        {
            return !string.IsNullOrEmpty(data)
                ? JsonConvert.DeserializeObject<Dictionary<long, DenomIndices>>(data)
                : new Dictionary<long, DenomIndices>();
        }

        private void OnGameAdded(GameAddedEventArgs e)
        {
            GameAdded?.Invoke(this, e);
        }

        private void LoadData()
        {
            var blockGameCount = _persistentStorage.GetBlock(_blockDeviceCountName);

            GameCount = (int)blockGameCount[DeviceCountKey];

            var blockData = _persistentStorage.GetBlock(_blockGameDataName);

            var games = blockData.GetAll();

            for (var blockIndex = 0; blockIndex < GameCount; blockIndex++)
            {
                if (games.TryGetValue(blockIndex, out var game))
                {
                    var gameId = (int)game[GameIdKey];

                    _gameIdMap.Add(
                        gameId,
                        new GameIndices
                        {
                            BlockIndex = (int)game[BlockIndexKey],
                            Denominations = ToDenomMap((string)game[DenomMap])
                        });
                }
            }

            if (!_gameIdMap.IsNullOrEmpty())
            {
                DenominationCount = _gameIdMap.Max(g => g.Value.Denominations.Values.Max(x => x.BlockIndex)) + 1;
                WagerCategoryCount = _gameIdMap.Max(g => g.Value.Denominations.Values.Max(d => d.WagerCategories.Values.Max())) + 1;
            }
        }

        private void Save()
        {
            var blockDeviceCount = _persistentStorage.GetBlock(_blockDeviceCountName);
            var blockData = _persistentStorage.GetBlock(_blockGameDataName);

            if (GameCount > blockData.Count)
            {
                Logger.Debug($"Resizing the game data storage block to {GameCount} blocks");
                _persistentStorage.ResizeBlock(_blockGameDataName, GameCount);
            }

            blockDeviceCount[DeviceCountKey] = GameCount;

            using (var transaction = blockData.StartTransaction())
            {
                foreach (var item in _gameIdMap)
                {
                    var blockIndex = item.Value.BlockIndex;

                    transaction[blockIndex, GameIdKey] = item.Key;
                    transaction[blockIndex, BlockIndexKey] = blockIndex;
                    transaction[blockIndex, DenomMap] = JsonConvert.SerializeObject(item.Value.Denominations, Formatting.None);
                }

                transaction.Commit();
            }
        }

        private GameIndices ToGameIndices(IGameDetail game)
        {
            return new GameIndices
            {
                BlockIndex = GameCount++,
                Denominations = ToDenomMap(game)
            };
        }

        private Dictionary<long, DenomIndices> ToDenomMap(IGameDetail game)
        {
            return game.SupportedDenominations.ToDictionary(
                denom => denom,
                _ => new DenomIndices { BlockIndex = DenominationCount++, WagerCategories = ToWagerCategoryMap(game) });
        }

        private Dictionary<string, int> ToWagerCategoryMap(IGameDetail game)
        {
            return game.WagerCategories.ToDictionary(
                wagerCategory => wagerCategory?.Id,
                _ => WagerCategoryCount++);
        }

        private class GameIndices
        {
            public int BlockIndex { get; set; }

            public Dictionary<long, DenomIndices> Denominations { get; set; }
        }

        private class DenomIndices
        {
            public int BlockIndex { get; set; }

            public Dictionary<string, int> WagerCategories { get; set; }
        }
    }
}