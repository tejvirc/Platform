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
        private const PersistenceLevel BlockGameOrderDataLevel = PersistenceLevel.Critical;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IEventBus _eventBus;
        private readonly IPersistentStorageAccessor _gameOrderAccessor;

        private readonly object _sync = new object();

        private IList<string> _gameOrder = new List<string>();

        public GameOrderSettings(IPersistentStorageManager storageManager, IEventBus eventBus)
        {
            _eventBus = eventBus;

            var dataBlockName = GetType() + ".Data";

            _gameOrderAccessor = storageManager.BlockExists(dataBlockName)
                ? storageManager.GetBlock(dataBlockName)
                : storageManager.CreateBlock(BlockGameOrderDataLevel, dataBlockName, 1);
            
            
              LoadGameOrder();
        } 

        public IList<string> Order
        {
            get
            {
                lock (_sync)
                {
                    return _gameOrder;
                }
            }
            internal set
            {
                lock (_sync)
                {
                    _gameOrder = value;
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

        public void SetGameOrderFromConfig(IList<IGameInfo> games, IList<string> gameOrderConfig)
        {
            // Load from saved game order if it exists
            if (Order.Any() || games == null || !games.Any())
            {
                return;
            }

            // Load from config order list the first time
            if (gameOrderConfig.Any())
            {
                var gamesInConfig = games.Where(o => gameOrderConfig.Contains(o.ThemeId)).ToList();
                var gamesInConfigWithSort = new List<KeyValuePair<IGameInfo, int>>();

                // put the config file index into a key value pair to sort
                foreach (var game in gamesInConfig)
                {
                    var index = gameOrderConfig.IndexOf(game.ThemeId);
                    gamesInConfigWithSort.Add(new KeyValuePair<IGameInfo, int>(game, index));
                }

                var gamesInConfigSorted = gamesInConfigWithSort.OrderBy(o => o.Value).Select(o => o.Key.ThemeId);

                var gamesNotInConfig = games.Where(o => !gamesInConfig.Contains(o)).ToList();
                var gamesNotInConfigWithSort = new List<KeyValuePair<IGameInfo, int>>();

                // put the initial list order into a key value pair as a secondary sort
                foreach (var game in gamesNotInConfig)
                {
                    var index = games.IndexOf(game);
                    gamesNotInConfigWithSort.Add(new KeyValuePair<IGameInfo, int>(game, index));
                }

                // Sort First by newest install, then if install date is ==, sort by the initial game order
                // which I believe is determined by filename
                var gamesNotInConfigSorted = gamesNotInConfigWithSort
                    .OrderByDescending(o => o.Key.InstallDateTime)
                    .ThenBy(o => o.Value)
                    .Select(o => o.Key.ThemeId);

                // We assume anything not in the config is a game added to the system that should be displayed first.
                // Ordered by newest install date first.
                var gameIdList = new List<string>(gamesNotInConfigSorted);

                // Then we order everything else by the jurisdictional config file
                gameIdList.AddRange(gamesInConfigSorted);

                SetGameOrder(gameIdList, false);
            }
        }

        /// <inheritdoc />
        public void SetGameOrder(IEnumerable<string> gameOrder, bool operatorChanged)
        {
            lock (_sync)
            {
                _gameOrder = new List<string>(gameOrder);
                SaveGameOrder(operatorChanged);
            }
        }

        /// <inheritdoc />
        public int GetPositionPriority(string gameId)
        {
            // Add 1 to this because the operator expects order to begin with 1, not 0
            return (Order?.IndexOf(gameId) ?? -1) + 1; 
        }


        /// <inheritdoc />
        public void UpdatePositionPriority(string gameId, int newPosition)
        {
            // Assume the new position order is based on a starting value of 1
            newPosition--;

            var order = new List<string>(Order);
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

            SetGameOrder(order, false);

            Logger.Debug($"Updated position for Theme Id - {gameId} to {newPosition}");
        }

        /// <inheritdoc />
        public void OnGameAdded(string themeId)
        {
            lock (_sync)
            {
                if (!_gameOrder.Contains(themeId))
                {
                    _gameOrder.Insert(0, themeId);
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
                if (_gameOrder.Contains(themeId))
                {
                    _gameOrder.Remove(themeId);
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
                return _gameOrder.Contains(themeId);
            }
        }

        private void SaveGameOrder(bool operatorChanged)
        {
            lock (_sync)
            {
                byte[] byteArray;
                if (_gameOrder.Count == 0)
                {
                    // This can happen if all games removed during testing.  Write the
                    // special hack for empty/default byte blob to persistent storage.
                    byteArray = new byte[] { 0, 0 };
                }
                else
                {
                    // serialize the Theme Ids into JSON & convert to a Byte array.
                    var gameOrderString = JsonConvert.SerializeObject(_gameOrder, Formatting.None);
                    byteArray = Encoding.UTF8.GetBytes(gameOrderString);
                }

                using (var transaction = _gameOrderAccessor.StartTransaction())
                {
                    transaction[GameOrderBlobField] = byteArray;
                    transaction.Commit();
                }

                _eventBus.Publish(new GameOrderChangedEvent(Order, operatorChanged));

                Logger.Debug("Game order saved");
            }
        }

        private void LoadGameOrder()
        {
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
                    _gameOrder = JsonConvert.DeserializeObject<List<string>>(jsonText);
                }
            }

            if (_gameOrder == null)
            {
                _gameOrder = new List<string>();
            }
        }
    }
}
