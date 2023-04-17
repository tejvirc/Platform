namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Common;
    using Contracts;
    using Contracts.Models;
    using Hardware.Contracts.Persistence;
    using Kernel;

    /// <summary>
    ///     An <see cref="T:Aristocrat.Monaco.Gaming.Contracts.IAttractConfigurationProvider" />
    /// </summary>
    public class AttractConfigurationProvider : IAttractConfigurationProvider, IService, IDisposable
    {
        private const string DataBlock = @"Data";
        private const string GameSequenceNumberField = @"AttractGame.SequenceNumber";
        private const string GameTypeNameField = @"AttractGame.GameTypeName";
        private const string GameThemeIdField = @"AttractGame.GameThemeId";
        private const string GameIsSelectedField = @"AttractGame.GameIsSelected";
        private const int BlockStartIndex = 1;

        private readonly IPropertiesManager _properties;
        private readonly IPersistentStorageManager _storageManager;
        private readonly IGameProvider _gameProvider;
        private readonly IGameOrderSettings _gameOrder;
        private readonly IEventBus _eventBus;
        private readonly object _sync = new object();
        private readonly List<AttractInfo> _attractInfo = new List<AttractInfo>();

        private bool _disposed;

        private readonly List<string> _gamesEnabledInConfiguration = new List<string>();

        private LobbyConfiguration _lobbyConfiguration;

        public AttractConfigurationProvider(
            IPersistentStorageManager storageManager,
            IPropertiesManager properties,
            IGameProvider gameProvider,
            IGameOrderSettings gameOrder,
            IEventBus eventBus)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _storageManager = storageManager ?? throw new ArgumentNullException(nameof(storageManager));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _gameOrder = gameOrder ?? throw new ArgumentNullException(nameof(gameOrder));

            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _eventBus.Subscribe<GameDisabledEvent>(this, evt=>  UpdateAttractSequence(evt.GameThemeId, false));
            _eventBus.Subscribe<GameEnabledEvent>(this, evt => UpdateAttractSequence(evt.GameThemeId, true));

            _lobbyConfiguration = _properties.GetValue<LobbyConfiguration>(GamingConstants.LobbyConfig, null);
        }

        public bool IsAttractEnabled => _properties.GetValue(GamingConstants.AttractModeEnabled, true);

        public string Name => GetType().ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(IAttractConfigurationProvider) };

        public IEnumerable<IAttractInfo> GetAttractSequence()
        {
            lock (_sync)
            {
                if (!_properties.GetValue(
                    GamingConstants.DefaultAttractSequenceOverridden,
                    false))
                {
                    // Here, we return the default attract sequence
                    return GetDefaultSequence();
                }

                var enabledGames = _gameProvider.GetEnabledGames().DistinctBy(g => g.ThemeId)
                    .OrderBy(g => _gameOrder.GetAttractPositionPriority(g.ThemeId)).ToList();

                var enabledAttract = new List<IAttractInfo>();

                // Select saved attract sequence, excluding games which are disabled now
                var configuredAttractSequence = enabledGames.Join(
                    _attractInfo,
                    detail => new { detail.GameType, detail.ThemeId },
                    stored => new { stored.GameType, stored.ThemeId },
                    (gameDetail, stored) => new AttractInfo
                    {
                        ThemeId = gameDetail.ThemeId,
                        GameType = gameDetail.GameType,
                        ThemeNameDisplayText =
                            AttractGameThemeDisplayText(gameDetail.GameType, gameDetail.ThemeName),
                        SequenceNumber = stored.SequenceNumber,
                        IsSelected = stored.IsSelected
                    }).ToList();

                var configuredOrderedAttract = configuredAttractSequence.OrderBy(ai => ai.SequenceNumber).ToList();

                // Next, some new games may have been enabled, since previous run.
                // Add them to attract sequence here(for display for selection)
                var enabledAttractGameDetail = configuredOrderedAttract
                    .SelectMany(ai => enabledGames.Where(eg => eg.GameType == ai.GameType && eg.ThemeId == ai.ThemeId))
                    .ToList();
                var newEnabledGameDetails = enabledGames.Except(enabledAttractGameDetail).ToList();

                var selectedAttracts = configuredOrderedAttract.Where(ai => ai.IsSelected).ToList();

                // Since last saved, some of these unselected attracts may have to be selected
                // as the corresponding games may have been disabled and enabled again.
                // As a rule, any game that is enabled must be added to attract sequence even if the last unselected in attract configuration.
                var unselectedAttracts = configuredOrderedAttract.Where(ai => !ai.IsSelected).ToList();

                if (_gamesEnabledInConfiguration.Any() &&
                    unselectedAttracts.Any(ai => _gamesEnabledInConfiguration.Any(g => g == ai.ThemeId)))
                {
                    var allEnabledGames = _gameProvider.GetEnabledGames().DistinctBy(g => g.ThemeId)
                        .OrderBy(g => _gameOrder.GetAttractPositionPriority(g.ThemeId)).ToList();

                    var newGamesToSelect = allEnabledGames
                        .Where(g => _gamesEnabledInConfiguration.Any(themeId => themeId == g.ThemeId)).ToList();

                    // These are the games which were previously unselected, but must be selected now as they were
                    // disabled and enabled in game configuration after attract sequence was saved.
                    var gamesToSelectFromPreviouslyUnselected =
                        newGamesToSelect.Where(g => unselectedAttracts.Any(ai => ai.ThemeId == g.ThemeId)).ToList();

                    newEnabledGameDetails.AddRange(gamesToSelectFromPreviouslyUnselected);

                    // Reorder the games.
                    newEnabledGameDetails = newEnabledGameDetails.DistinctBy(g => g.ThemeId)
                        .OrderBy(m => _gameOrder.GetAttractPositionPriority(m.ThemeId)).ToList();

                    // Remaining unselected attract items.
                    unselectedAttracts = unselectedAttracts.Where(
                        ai => gamesToSelectFromPreviouslyUnselected.All(g => g.ThemeId != ai.ThemeId)).ToList();
                }

                _gamesEnabledInConfiguration.Clear();

                // Add configured and selected attracts
                enabledAttract.AddRange(selectedAttracts);

                // Add newly enabled games
                if (newEnabledGameDetails.Any())
                {
                    UpdateAttractSequence(enabledAttract, newEnabledGameDetails, null, enabledAttract.Count + 1);
                }

                if (unselectedAttracts.Any())
                {
                    // Add configured but un-selected attracts
                    enabledAttract.AddRange(unselectedAttracts);
                }

                ResetAttractSequenceNumbers(enabledAttract);

                if (newEnabledGameDetails.Any())
                {
                    SaveAttractSequence(enabledAttract);
                }

                return enabledAttract;
            }
        }

        /// <inheritdoc />
        public void SaveAttractSequence(ICollection<IAttractInfo> attractSequence)
        {
            if (attractSequence == null)
            {
                return;
            }

            var selectedAttractSequence = attractSequence.ToList();

            lock (_sync)
            {
                var block = GetAccessor(DataBlock);

                var newCount = selectedAttractSequence.Count + 1;

                if (block.Count != newCount)
                {
                    _storageManager.ResizeBlock(GetBlockName(DataBlock), newCount);
                }

                var blockIndex = BlockStartIndex;

                using (var transaction = block.StartTransaction())
                {
                    foreach (var attractInfo in selectedAttractSequence)
                    {
                        transaction[blockIndex, GameTypeNameField] = (int)attractInfo.GameType;
                        transaction[blockIndex, GameThemeIdField] = attractInfo.ThemeId;
                        transaction[blockIndex, GameIsSelectedField] = attractInfo.IsSelected;
                        transaction[blockIndex, GameSequenceNumberField] = blockIndex;

                        blockIndex++;
                    }

                    transaction.Commit();
                }

                LoadAttractInfo();
            }
        }

        public IEnumerable<IAttractInfo> GetDefaultSequence()
        {
            // lobby config loads after attract config provider, might need a refresh
            _lobbyConfiguration ??= _properties.GetValue<LobbyConfiguration>(GamingConstants.LobbyConfig, null);

            var enabledGames = _gameProvider.GetEnabledGames().DistinctBy(g => g.ThemeId).ToList();
            List<AttractInfo> configuredAttractSequence;

            if (_lobbyConfiguration == null)
            {
                // if no configuration loaded at this point, follow the Game Order Priority
                var priorityOrderedGames = enabledGames.OrderBy(g => _gameOrder.GetAttractPositionPriority(g.ThemeId)).ToList();

                configuredAttractSequence = priorityOrderedGames.Select(
                                                                    game => new AttractInfo
                                                                    {
                                                                        ThemeId = game.ThemeId,
                                                                        GameType = game.GameType,
                                                                        ThemeNameDisplayText = AttractGameThemeDisplayText(game.GameType, game.ThemeName),
                                                                        IsSelected = AttractGameTypeEnabled(game.GameType)
                                                                    }
                                                                )
                                                                .DistinctBy(a => a.ThemeId)
                                                                .ToList();

                ResetAttractSequenceNumbers(configuredAttractSequence);
                return configuredAttractSequence;
            }

            // Try to follow the Lightning Link game order
            // Otherwise, follow the ThemeID order
            var lightningLinkOrder = enabledGames.Any(g => g.Category == GameCategory.LightningLink)
                                         ? _lobbyConfiguration.DefaultGameOrderLightningLinkEnabled
                                         : _lobbyConfiguration.DefaultGameOrderLightningLinkDisabled;

            var defaultGameOrderList = lightningLinkOrder ?? _lobbyConfiguration.DefaultGameDisplayOrderByThemeId;

            var gamesInConfig = defaultGameOrderList
                                .Select(game => enabledGames.FirstOrDefault(g2 => g2.ThemeId == game))
                                .Where(game => game != null);

            var gamesNotInConfig = enabledGames
                                   .Where(g => !defaultGameOrderList.Contains(g.ThemeId))
                                   .OrderBy(g => g.InstallDate);

            var attractList = gamesNotInConfig.ToList();
            attractList = attractList.Concat(gamesInConfig).ToList();

            configuredAttractSequence = attractList.Select(
                                                       game => new AttractInfo
                                                       {
                                                           ThemeId = game.ThemeId,
                                                           GameType = game.GameType,
                                                           ThemeNameDisplayText = AttractGameThemeDisplayText(game.GameType,game.ThemeName),
                                                           IsSelected = AttractGameTypeEnabled(game.GameType)
                                                       }
                                                   ).ToList();

            ResetAttractSequenceNumbers(configuredAttractSequence);

            return configuredAttractSequence;
        }

        public void Initialize()
        {
            if (!_storageManager.BlockExists(GetBlockName(DataBlock)))
            {
                return;
            }

            LoadAttractInfo();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private PersistenceLevel PersistenceLevel => _properties.GetValue(ApplicationConstants.DemonstrationMode, false)
            ? PersistenceLevel.Critical
            : PersistenceLevel.Static;

        private void LoadAttractInfo()
        {
            var dataBlock = GetAccessor(DataBlock);
            var results = dataBlock.GetAll();

            lock (_sync)
            {
                _attractInfo.Clear();

                for (var i = BlockStartIndex; i < dataBlock.Count + BlockStartIndex; i++)
                {
                    if (results.TryGetValue(i, out var gameAttractInfo))
                    {
                        _attractInfo.Add(
                            new AttractInfo
                            {
                                GameType = (GameType)gameAttractInfo[GameTypeNameField],
                                ThemeId = (string)gameAttractInfo[GameThemeIdField],
                                SequenceNumber = (int)gameAttractInfo[GameSequenceNumberField],
                                IsSelected = (bool)gameAttractInfo[GameIsSelectedField]
                            });
                    }
                }
            }
        }

        private void UpdateAttractSequence(
            List<IAttractInfo> enabledAttract,
            IEnumerable<IGameDetail> newGames,
            bool? selected = null,
            int? startIndex = 1)
        {
            var index = startIndex ?? 1;
            enabledAttract.AddRange(
                from game in newGames
                select new AttractInfo
                {
                    ThemeId = game.ThemeId,
                    GameType = game.GameType,
                    ThemeNameDisplayText = AttractGameThemeDisplayText(game.GameType, game.ThemeName),
                    IsSelected = selected ?? AttractGameTypeEnabled(game.GameType),
                    SequenceNumber = index++
                });
        }

        private void ResetAttractSequenceNumbers(IEnumerable<IAttractInfo> attractSequence)
        {
            var count = 0;
            foreach (var ai in attractSequence)
            {
                ai.SequenceNumber = ++count;
            }
        }

        private bool AttractGameTypeEnabled(GameType type)
        {
            switch (type)
            {
                case GameType.Slot:
                    return _properties.GetValue(GamingConstants.SlotAttractSelected, true);
                case GameType.Keno:
                    return _properties.GetValue(GamingConstants.KenoAttractSelected, true);
                case GameType.Poker:
                    return _properties.GetValue(GamingConstants.PokerAttractSelected, true);
                case GameType.Blackjack:
                    return _properties.GetValue(GamingConstants.BlackjackAttractSelected, true);
                case GameType.Roulette:
                    return _properties.GetValue(GamingConstants.RouletteAttractSelected, true);
                default:
                    return false;
            }
        }

        private string AttractGameThemeDisplayText(GameType type, string text)
        {
            switch (type)
            {
                case GameType.Slot:
                    var displayText = _properties.GetValue(GamingConstants.OverridenSlotGameTypeText, string.Empty);
                    return string.Join(
                        " - ",
                        string.IsNullOrEmpty(displayText)
                            ? GameType.Slot.GetDescription(typeof(GameType))
                            : displayText,
                        text);
                default:
                    return string.Join(
                        " - ",
                        type.GetDescription(typeof(GameType)),
                        text);
            }
        }

        private void UpdateAttractSequence(string themeId, bool enabled)
        {
            lock (_sync)
            {
                var defaultSequenceOverriden = _properties.GetValue(
                    GamingConstants.DefaultAttractSequenceOverridden,
                    false);
                if (!defaultSequenceOverriden)
                {
                    return;
                }

                if (enabled)
                {
                    if (_gamesEnabledInConfiguration.Contains(themeId))
                    {
                        return;
                    }

                    _gamesEnabledInConfiguration.Add(themeId);
                }
                else
                {
                    if (!_gamesEnabledInConfiguration.Contains(themeId))
                    {
                        return;
                    }

                    _gamesEnabledInConfiguration.Remove(themeId);
                }

                _eventBus.Publish(new AttractConfigurationChangedEvent());
            }
        }

        private IPersistentStorageAccessor GetAccessor(string name = null, int blockSize = 1)
        {
            var blockName = GetBlockName(name);

            return _storageManager.BlockExists(blockName)
                ? _storageManager.GetBlock(blockName)
                : _storageManager.CreateBlock(PersistenceLevel, blockName, blockSize);
        }

        private string GetBlockName(string name = null)
        {
            var baseName = GetType().ToString();

            return string.IsNullOrEmpty(name) ? baseName : $"{baseName}.{name}";
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }
    }
}