namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Newtonsoft.Json;

    /// <summary>
    ///     VLT implementation for IGameOrderSettings.
    /// </summary>
    public class GameOrderSettings : IGameOrderSettings
    {
        private const string GameOrderBlobField = @"GameOrderBlob";
        private const string WasOperatorChangedField = @"WasOperatorChanged";
        private const PersistenceLevel BlockGameOrderDataLevel = PersistenceLevel.Critical;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IEventBus _eventBus;
        private readonly IPersistentStorageAccessor _gameOrderAccessor;

        private readonly object _sync = new object();

        private IList<string> _iconOrder = new List<string>();
        private IList<string> _attractOrder = new List<string>();
        private IList<string> _iconOrderConfig = new List<string>();
        private IList<string> _attractOrderConfig = new List<string>();
        private bool _wasOperatorChanged;

        public GameOrderSettings(IPersistentStorageManager storageManager, IEventBus eventBus)
        {
            _eventBus = eventBus;

            var dataBlockName = GetType() + ".Data";

            _gameOrderAccessor = storageManager.BlockExists(dataBlockName)
                ? storageManager.GetBlock(dataBlockName)
                : storageManager.CreateBlock(BlockGameOrderDataLevel, dataBlockName, 1);
            
            
              LoadGameOrder();
        } 

        public IList<string> IconOrder
        {
            get
            {
                lock (_sync)
                {
                    return _iconOrder;
                }
            }
            internal set
            {
                lock (_sync)
                {
                    _iconOrder = value;
                }
            }
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IGameOrderSettings) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        public void SetAttractOrderFromConfig(IList<IGameInfo> games, IList<string> gameOrderConfig)
        {
            if (games == null || !games.Any() || !gameOrderConfig.Any())
            {
                return;
            }

            if (_attractOrderConfig.SequenceEqual(gameOrderConfig) && _attractOrder.Any())
            {
                return;
            }

            var gamesInConfig = games.Where(o => gameOrderConfig.Contains(o.ThemeId)).ToList();
            var gamesInConfigWithSort = new List<(IGameInfo game, int priority)>();

            foreach (var game in gamesInConfig)
            {
                var index = gameOrderConfig.IndexOf(game.ThemeId);
                gamesInConfigWithSort.Add((game, index));
            }

            var gamesInConfigSorted = gamesInConfigWithSort.OrderBy(o => o.priority).Select(o => o.game.ThemeId);
            var gamesNotInConfig = games.Where(o => !gamesInConfig.Contains(o)).ToList();
            var gamesNotInConfigWithSort = new List<(IGameInfo game, int priority)>();

            foreach (var game in gamesNotInConfig)
            {
                var index = games.IndexOf(game);
                gamesNotInConfigWithSort.Add((game, index));
            }

            var gamesNotInConfigSorted = gamesNotInConfigWithSort
                .OrderByDescending(o => o.game.InstallDateTime)
                .ThenBy(o => o.priority)
                .Select(o => o.game.ThemeId);

            var gameIdList = new List<string>(gamesNotInConfigSorted);

            //adds known games below unknown games as they might be newer for attract
            gameIdList.AddRange(gamesInConfigSorted);

            _attractOrderConfig = gameOrderConfig;

            _attractOrder = new List<string>(gameIdList);
        }

        public void SetIconOrderFromConfig(IList<IGameInfo> games, IList<string> gameOrderConfig)
        {
            if (_wasOperatorChanged || games == null || !games.Any())
            {
                return;
            }

            // No need to update the order if we're already using it
            if (_iconOrderConfig.SequenceEqual(gameOrderConfig) && IconOrder.Any())
            {
                return;
            }

            // Load from config order list
            if (gameOrderConfig.Any())
            {
                var gamesInConfig = games.Where(o => gameOrderConfig.Contains(o.ThemeId)).ToList();
                var gamesInConfigWithSort = new List<(IGameInfo game, int priority)>();

                // put the config file index into a key value pair to sort
                foreach (var game in gamesInConfig)
                {
                    var index = gameOrderConfig.IndexOf(game.ThemeId);
                    gamesInConfigWithSort.Add((game, index));
                }

                var gamesInConfigSorted = gamesInConfigWithSort.OrderBy(o => o.priority).Select(o => o.game.ThemeId);
                var gamesNotInConfig = games.Where(o => !gamesInConfig.Contains(o)).ToList();
                var gamesNotInConfigWithSort = new List<(IGameInfo game, int priority)>();

                // put the initial list order into a key value pair as a secondary sort
                foreach (var game in gamesNotInConfig)
                {
                    var index = games.IndexOf(game);
                    gamesNotInConfigWithSort.Add((game, index));
                }

                // Sort First by newest install, then if install date is ==, sort by the initial game order
                // which I believe is determined by filename
                var gamesNotInConfigSorted = gamesNotInConfigWithSort
                    .OrderByDescending(o => o.game.InstallDateTime)
                    .ThenBy(o => o.priority)
                    .Select(o => o.game.ThemeId);
                var gameIdList = new List<string>(gamesInConfigSorted);

                //adds unknown games to the bottom of the icon list.
                gameIdList.AddRange(gamesNotInConfigSorted);

                // Finally, keep track of which order we're using so we can skip this if we've already loaded it
                _iconOrderConfig = gameOrderConfig;

                SetIconOrder(gameIdList, false);
            }
        }

        /// <inheritdoc />
        public void SetIconOrder(IEnumerable<string> gameOrder, bool operatorChanged)
        {
            lock (_sync)
            {
                _iconOrder = new List<string>(gameOrder);
                SaveGameOrder(operatorChanged);
            }
        }

        /// <inheritdoc />
        public int GetAttractPositionPriority(string gameId)
        {
            return (_attractOrder?.IndexOf(gameId) ?? -1) + 1;
        }

        /// <inheritdoc />
        public int GetIconPositionPriority(string gameId)
        {
            // Add 1 to this because the operator expects order to begin with 1, not 0
            return (IconOrder?.IndexOf(gameId) ?? -1) + 1; 
        }


        /// <inheritdoc />
        public void UpdateIconPositionPriority(string gameId, int newPosition)
        {
            // Assume the new position order is based on a starting value of 1
            newPosition--;

            var order = new List<string>(IconOrder);
            if (order.Contains(gameId))
            {
                order.Remove(gameId);
            }

            if (newPosition <= 0)
            {
                order.Insert(0, gameId);
            }
            else if (newPosition >= order.Count)
            {
                order.Add(gameId);
            }
            else
            {
                order.Insert(newPosition, gameId);
            }

            SetIconOrder(order, false);

            Logger.Debug($"Updated position for Theme Id - {gameId} to {newPosition}");
        }

        /// <inheritdoc />
        public void OnGameAdded(string themeId)
        {
            lock (_sync)
            {
                if (!_iconOrder.Contains(themeId))
                {
                    _iconOrder.Insert(0, themeId);
                    SaveGameOrder(false);

                    Logger.Debug($"Added Theme Id - {themeId}");
                }
            }
        }

        /// <inheritdoc />
        public void RemoveGame(string themeId)
        {
            lock (_sync)
            {
                if (_iconOrder.Contains(themeId))
                {
                    _iconOrder.Remove(themeId);
                    SaveGameOrder(false);

                    Logger.Debug($"Removed Theme Id - {themeId}");
                }
            }
        }

        /// <inheritdoc />
        public bool Exists(string themeId)
        {
            lock (_sync)
            {
                return _iconOrder.Contains(themeId);
            }
        }

        private void SaveGameOrder(bool operatorChanged)
        {
            lock (_sync)
            {
                byte[] byteArray;
                if (_iconOrder.Count == 0)
                {
                    // This can happen if all games removed during testing.  Write the
                    // special hack for empty/default byte blob to persistent storage.
                    byteArray = new byte[] { 0, 0 };
                }
                else
                {
                    // serialize the Theme Ids into JSON & convert to a Byte array.
                    var gameOrderString = JsonConvert.SerializeObject(_iconOrder, Formatting.None);
                    byteArray = Encoding.UTF8.GetBytes(gameOrderString);
                }

                using (var transaction = _gameOrderAccessor.StartTransaction())
                {
                    transaction[GameOrderBlobField] = byteArray;
                    transaction[WasOperatorChangedField] = operatorChanged;
                    transaction.Commit();
                }

                _eventBus.Publish(new GameIconOrderChangedEvent(IconOrder, operatorChanged));

                Logger.Debug("Game icon order saved");
            }
        }

        private void LoadGameOrder()
        {
            _wasOperatorChanged = (bool)_gameOrderAccessor[WasOperatorChangedField];
            var byteArray = (byte[])_gameOrderAccessor[GameOrderBlobField];
            if (byteArray != null)
            {
                // Hack to handle the hack to store a dynamic array in the database.
                // This is equivalent to not having an order set yet.
                if (byteArray.Length == 2 && byteArray[0] == 0 && byteArray[1] == 0)
                {
                    return; // new List<string>();
                }

                var jsonText = Encoding.UTF8.GetString(byteArray);

                Logger.Debug($"Game order loaded {jsonText}");

                lock (_sync)
                {
                    _iconOrder = JsonConvert.DeserializeObject<List<string>>(jsonText);
                }
            }

            if (_iconOrder == null)
            {
                _iconOrder = new List<string>();
            }
        }
    }
}
