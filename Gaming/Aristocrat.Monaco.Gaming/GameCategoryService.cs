namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Contracts.Models;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Definition of the GameCategoryService class.
    /// </summary>
    public class GameCategoryService : IGameCategoryService, IService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private const string AutoPlayField = @"AutoPlay";
        private const string AutoHoldField = @"AutoHold";
        private const string ShowPlayerSpeedButtonField = @"ShowPlayerSpeedButton";
        private const string VolumeScalarField = @"VolumeScalar";
        private const string PlaySpeedField = @"PlaySpeed";
        private const string DealSpeedField = @"DealSpeed";
        private const string BackgroundColorField = @"BackgroundColor";
        private const string PokerBackgroundColor = "Blue";

        private readonly IPropertiesManager _properties;
        private readonly IGameProvider _gameProvider;
        private readonly string _blockName = typeof(GameCategoryService).ToString();
        private readonly IEventBus _eventBus;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly Dictionary<GameType, GameCategorySetting> _gameCategorySettings = new Dictionary<GameType, GameCategorySetting>();
        private IPersistentStorageAccessor _dataAccessor;

        public GameCategoryService(
            IEventBus eventBus,
            IGameProvider gameProvider,
            IPropertiesManager properties,
            IPersistentStorageManager persistentStorageManager)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _persistentStorage = persistentStorageManager ?? throw new ArgumentNullException(nameof(persistentStorageManager));
        }

        public GameCategorySetting this[GameType gameType] => _gameCategorySettings[gameType];

        public string Name => typeof(GameCategoryService).FullName;

        public ICollection<Type> ServiceTypes => new[] { typeof(IGameCategoryService) };

        public GameCategorySetting SelectedGameCategorySetting =>
            _gameCategorySettings[((int)_properties.GetProperty(GamingConstants.SelectedGameId, -1) != -1 ?
                _gameProvider.GetGame((int)_properties.GetProperty(GamingConstants.SelectedGameId, -1) ).GameType :
                GameType.Undefined)];

        public void Initialize()
        {
            CreatePersistence();
        }

        public void UpdateGameCategory(GameType gameType, GameCategorySetting settings)
        {
            _gameCategorySettings[gameType] = settings;
            if ((int)gameType >= 0)
            {
                using (var transaction = _dataAccessor.StartTransaction())
                {
                    UpdateSettings(transaction, gameType, settings);
                    transaction.Commit();
                }
            }

            _eventBus.Publish(new GameCategoryChangedEvent(gameType));

            Logger.Info($"Updating Game Category Setting type:{gameType} {settings}");
        }

        public GameCategorySetting GetGameCategorySetting(GameType gameType)
        {
            return _gameCategorySettings[gameType];
        }

        private  GameCategorySetting GetDefaultGameCategorySetting(GameType gameType)
        {
            return new GameCategorySetting
            {
                AutoPlay = _properties.GetValue(GamingConstants.AutoPlayAllowed, false),
                AutoHold = _properties.GetValue(GamingConstants.AutoHoldEnable, false),
                ShowPlayerSpeedButton = _properties.GetValue(GamingConstants.ShowPlayerSpeedButtonEnabled, true),
                VolumeScalar =
                    gameType == GameType.Keno ? VolumeScalar.Scale60 :
                    gameType == GameType.Slot ? VolumeScalar.Scale80 : VolumeScalar.Scale40,
                PlayerSpeed = 2,
                DealSpeed = gameType == GameType.Poker ? 8 : 5,
                BackgroundColor = (gameType == GameType.Poker || gameType == GameType.Slot) ? PokerBackgroundColor : string.Empty
            };
        }

        private void CreatePersistence()
        {
            if (_persistentStorage.BlockExists(_blockName))
            {
                _dataAccessor = _persistentStorage.GetBlock(_blockName);

                foreach (var index in Enum.GetValues(typeof(GameType)).Cast<GameType>())
                {
                    if ((int)index < 0)
                    {
                        _gameCategorySettings[index] = GetDefaultGameCategorySetting(index);
                    }
                    else
                    {
                        _gameCategorySettings[index] = new GameCategorySetting
                        {
                            AutoPlay = (bool)_dataAccessor[(int)index, AutoPlayField],
                            AutoHold = (bool)_dataAccessor[(int)index, AutoHoldField],
                            ShowPlayerSpeedButton = (bool)_dataAccessor[(int)index, ShowPlayerSpeedButtonField],
                            VolumeScalar = (VolumeScalar)((byte)_dataAccessor[(int)index, VolumeScalarField]),
                            PlayerSpeed = (int)_dataAccessor[(int)index, PlaySpeedField],
                            DealSpeed = (int)_dataAccessor[(int)index, DealSpeedField],
                            BackgroundColor = (string)_dataAccessor[(int)index, BackgroundColorField]
                        };
                    }
                }
            }
            else
            {
                var size = Enum.GetValues(typeof(GameType)).Cast<GameType>().Count(a => (int)a >= 0);

                using (var scope = _persistentStorage.ScopedTransaction())
                {
                    _dataAccessor = _persistentStorage.CreateBlock(
                        PersistenceLevel.Critical,
                        _blockName,
                        size);

                    using (var transaction = _dataAccessor.StartTransaction())
                    {
                        foreach (var index in Enum.GetValues(typeof(GameType)).Cast<GameType>())
                        {
                            var defaultSetting = GetDefaultGameCategorySetting(index);
                            _gameCategorySettings[index] = defaultSetting;

                            if ((int)index >= 0)
                            {
                                UpdateSettings(transaction, index, defaultSetting);
                            }
                        }

                        transaction.Commit();
                    }

                    scope.Complete();
                }
            }
        }

        private void UpdateSettings(
            IPersistentStorageTransaction transaction,
            GameType index,
            GameCategorySetting setting)
        {
            transaction[(int)index, AutoPlayField] = setting.AutoPlay;
            transaction[(int)index, AutoHoldField] = setting.AutoHold;
            transaction[ (int)index, ShowPlayerSpeedButtonField] = setting.ShowPlayerSpeedButton;
            transaction[(int)index, VolumeScalarField] = setting.VolumeScalar;
            transaction[(int)index, PlaySpeedField] = setting.PlayerSpeed;
            transaction[(int)index, DealSpeedField] = setting.DealSpeed;
            transaction[(int)index, BackgroundColorField] = setting.BackgroundColor;
        }
    }
}