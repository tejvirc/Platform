namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Drm;
    using Application.Contracts.Localization;
    using Aristocrat.PackageManifest.Extension.v100;
    using Cabinet.Contracts;
    using Common;
    using Contracts;
    using Contracts.Configuration;
    using Contracts.GameSpecificOptions;
    using Contracts.Meters;
    using Contracts.Models;
    using Contracts.Progressives;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using Newtonsoft.Json;
    using PackageManifest;
    using PackageManifest.Ati;
    using PackageManifest.Models;
    using Runtime;
    using Rectangle = System.Drawing.Rectangle;
    using WagerCategory = Contracts.WagerCategory;

    /// <summary>
    ///     An <see cref="T:Aristocrat.Monaco.Gaming.Contracts.IGameProvider" />
    /// </summary>
    public sealed class GameProvider : IGameProvider, IPropertyProvider, IService, IServerPaytableInstaller
    {
        private const string ManifestFilter = @"*.gsaManifest";
        private const string AtiPrefix = @"ATI_";

        private const string DataBlock = @"Data";
        private const string GameIdField = @"Game.Id";
        private const string GameThemeIdField = @"Game.ThemeId";
        private const string GameThemeNameField = @"Game.ThemeName";
        private const string GamePayTableIdField = @"Game.PaytableId";
        private const string GameVersionField = @"Game.Version";
        private const string GameStatusField = @"Game.Status";
        private const string GameDenominationsField = @"Game.Denominations";
        private const string GameWagerCategoriesField = @"Game.WagerCategories";
        private const string CdsGameInfosField = @"Game.CdsGameInfos";
        private const string GameInstallDateField = @"Game.InstallDate";
        private const string GameTagsField = @"Game.Tags";
        private const string GameUpgradedField = @"Game.Upgraded";
        private const string GameMaximumWagerCreditsField = @"Game.MaximumWagerCredits";
        private const string GameCategoryField = @"Game.Category";
        private const string GameSubCategoryField = @"Game.SubCategory";
        private const string GameFeaturesField = @"Game.Features";
        private const string GameMinimumPaybackPercentField = @"Game.MinimumPaybackPercent";
        private const string GameMaximumPaybackPercentField = @"Game.MaximumPaybackPercent";
        private const string GameSubGameDetailsField = @"Game.SubGameDetails";

        private const string ProgressivesConfigFilename = @"progressives.xml";
        private const string GameSpecificOptionsConfigFilename = @"gamespecificoptions.xml";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEventBus _bus;
        private readonly ISystemDisableManager _disableManager;
        private readonly IGameOrderSettings _gameOrder;
        private readonly List<GameDetail> _games = new();
        private readonly List<(GameDetail, List<ProgressiveDetail>)> _availableGames = new();
        private readonly IManifest<GameContent> _manifest;
        private readonly IGameMeterManager _meters;
        private readonly IPathMapper _pathMapper;
        private readonly IManifest<IEnumerable<ProgressiveDetail>> _progressiveManifest;
        private readonly IProgressiveLevelProvider _progressiveProvider;
        private readonly IIdProvider _idProvider;
        private readonly IPropertiesManager _properties;
        private readonly IRuntimeProvider _runtimeProvider;
        private readonly IPersistentStorageManager _storageManager;
        private readonly IDigitalRights _digitalRights;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly ICabinetDetectionService _cabinetDetectionService;
        private readonly IManifest<GameSpecificOptionConfig> _gameSpecificOptionManifest;
        private readonly IGameSpecificOptionProvider _gameSpecificOptionProvider;
       
        private readonly double _multiplier;
        private readonly object _sync = new();

        private bool _initialized;

        public GameProvider(
            IPathMapper pathMapper,
            IPersistentStorageManager storageManager,
            IManifest<GameContent> manifest,
            IGameMeterManager meters,
            ISystemDisableManager disableManager,
            IGameOrderSettings gameOrder,
            IEventBus bus,
            IPropertiesManager properties,
            IRuntimeProvider runtimeProvider,
            IManifest<IEnumerable<ProgressiveDetail>> progressiveManifest,
            IProgressiveLevelProvider progressiveProvider,
            IManifest<GameSpecificOptionConfig> gameSpecificOptionManifest,
            IGameSpecificOptionProvider gameSpecificOptionProvider,
            IIdProvider idProvider,
            IDigitalRights digitalRights,
            IConfigurationProvider configurationProvider,
            ICabinetDetectionService cabinetDetectionService)
        {
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));
            _storageManager = storageManager ?? throw new ArgumentNullException(nameof(storageManager));
            _manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _gameOrder = gameOrder ?? throw new ArgumentNullException(nameof(gameOrder));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _runtimeProvider = runtimeProvider ?? throw new ArgumentNullException(nameof(runtimeProvider));
            _progressiveManifest = progressiveManifest ?? throw new ArgumentNullException(nameof(progressiveManifest));
            _progressiveProvider = progressiveProvider ?? throw new ArgumentNullException(nameof(progressiveProvider));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
            _digitalRights = digitalRights ?? throw new ArgumentNullException(nameof(digitalRights));
            _configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            _cabinetDetectionService = cabinetDetectionService ?? throw new ArgumentNullException(nameof(cabinetDetectionService));

            _gameSpecificOptionManifest = gameSpecificOptionManifest ?? throw new ArgumentNullException(nameof(gameSpecificOptionManifest));
            _gameSpecificOptionProvider = gameSpecificOptionProvider ?? throw new ArgumentNullException(nameof(gameSpecificOptionProvider));
            _multiplier = properties.GetValue(ApplicationConstants.CurrencyMultiplierKey, 1d);

            _initialized = HasGame();

            BuildCache();
        }

        private PersistenceLevel PersistenceLevel => _properties.GetValue(ApplicationConstants.DemonstrationMode, false)
            ? PersistenceLevel.Critical
            : PersistenceLevel.Static;

        /// <inheritdoc />
        public IGameDetail GetGame(int gameId)
        {
            lock (_sync)
            {
                return _games.SingleOrDefault(g => g.Id == gameId);
            }
        }

        public (IGameDetail game, IDenomination denomination) GetGame(int gameId, long denomination)
        {
            lock (_sync)
            {
                var game = GetGame(gameId);

                return game == null ? (null, null) : (game, game.Denominations.Single(d => d.Value == denomination));
            }
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IGameDetail> GetGames()
        {
            lock (_sync)
            {
                return _games.Where(g => g.Active).ToList();
            }
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IGameDetail> GetEnabledGames()
        {
            lock (_sync)
            {
                return _games.Where(g => g.EgmEnabled && g.Enabled).ToList();
            }
        }
        
        /// <inheritdoc />
        public IReadOnlyCollection<IGameDetail> GetAllGames()
        {
            lock (_sync)
            {
                return _games.ToList();
            }
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IGameCombo> GetGameCombos()
        {
            lock (_sync)
            {
                return _games.Where(g => g.Active).SelectMany(
                    d => d.Denominations,
                    (game, denom) => new GameCombo(
                        denom.Id,
                        game.Id,
                        game.ThemeId,
                        game.PaytableId,
                        denom.Value,
                        denom.BetOption)).ToList();
            }
        }

        public IReadOnlyCollection<ISubGameDetails> GetEnabledSubGames(IGameDetail currentGame)
        {
            lock (_sync)
            {
                return currentGame.SupportedSubGames.ToList();
            }
        }

        /// <inheritdoc />
        public bool ValidateConfiguration(IGameDetail gameProfile)
        {
            return ValidateConfiguration(gameProfile, gameProfile.ActiveDenominations);
        }

        /// <inheritdoc />
        public bool ValidateConfiguration(IGameDetail gameProfile, IEnumerable<long> denominations)
        {
            lock (_sync)
            {
                var games = _games.Where(g => g.GameType == gameProfile.GameType);
                var denominationsList = denominations.ToList();
                if (!gameProfile.GameSubtype.IsNullOrEmpty())
                {
                    games = games.Where(g => g.GameSubtype == gameProfile.GameSubtype);
                }

                var activeDenomsLists = games.Select(g => g.ActiveDenominations);
                var activeDenomList = new List<long>();
                foreach (var list in activeDenomsLists)
                {
                    activeDenomList.AddRange(list);
                }

                activeDenomList.AddRange(denominationsList);
                var uniqueDenomList = activeDenomList.ToHashSet();

                if (uniqueDenomList.Count > ApplicationConstants.NumSelectableDenomsPerGameTypeInLobby)
                {
                    return false;
                }

                var collision = _games.Any(g =>
                    g.Id != gameProfile.Id &&
                    g.Enabled &&
                    g.ThemeId == gameProfile.ThemeId &&
                    denominationsList.Intersect(g.ActiveDenominations).Any());

                return !collision;
            }
        }

        /// <inheritdoc />
        public void EnableGame(int gameId, GameStatus status)
        {
            lock (_sync)
            {
                var game = _games.Single(g => g.Id == gameId);

                if (!ValidateConfiguration(game))
                {
                    throw new GamePlayCollisionException();
                }

                if ((game.Status & status) == 0)
                {
                    return;
                }

                game.Status &= ~status;
                UpdateGame(game);

                if (game.Enabled)
                {
                    HandleUpgrade(game);

                    _bus.Publish(new GameEnabledEvent(gameId, status, game.ThemeId));

                    CheckState();
                }
            }
        }

        /// <inheritdoc />
        public void DisableGame(int gameId, GameStatus status)
        {
            lock (_sync)
            {
                var game = _games.Single(g => g.Id == gameId);

                if ((game.Status & status) == 0)
                {
                    game.Status |= status;
                    UpdateGame(game);

                    _bus.Publish(new GameDisabledEvent(gameId, status, game.ThemeId));

                    CheckState();
                }
            }
        }

        /// <inheritdoc />
        public void SetActiveDenominations(int gameId, IEnumerable<long> denominations)
        {
            lock (_sync)
            {
                var game = _games.Single(g => g.Id == gameId);

                var activeDenominations = game.Denominations.Where(d => denominations.Contains(d.Value))
                    .Cast<Denomination>()
                    .Select(
                        denom =>
                        {
                            var clone = denom.ShallowCopy();
                            clone.Active = true;
                            return clone;
                        });

                SetActiveDenominations(gameId, activeDenominations);
            }
        }

        /// <inheritdoc />
        public void SetActiveDenominations(int gameId, IEnumerable<IDenomination> denominations)
        {
            var activeDenominations = denominations.ToList();

            lock (_sync)
            {
                var game = _games.Single(g => g.Id == gameId);

                if (!ValidateConfiguration(game, activeDenominations.Select(d => d.Value)))
                {
                    throw new GamePlayCollisionException();
                }

                if (game.Denominations.Where(d => d.Active).SequenceEqual(activeDenominations.Where(d => d.Active)))
                {
                    return;
                }

                var enabled = game.EgmEnabled;

                foreach (var denomination in game.Denominations.Cast<Denomination>())
                {
                    var active = activeDenominations.FirstOrDefault(d => d.Id == denomination.Id);
                    if (active == null || !active.Active)
                    {
                        if (denomination.Active)
                        {
                            if (DateTime.UtcNow > denomination.ActiveDate)
                            {
                                denomination.PreviousActiveTime += DateTime.UtcNow - denomination.ActiveDate;
                            }
                        }

                        denomination.Active = false;

                    }
                    else
                    {
                        denomination.BetOption = active.BetOption;
                        denomination.LineOption = active.LineOption;
                        denomination.BonusBet = active.BonusBet;
                        denomination.MinimumWagerCredits = active.MinimumWagerCredits;
                        denomination.MaximumWagerCredits = active.MaximumWagerCredits;
                        denomination.MaximumWagerOutsideCredits = active.MaximumWagerOutsideCredits;
                        denomination.SecondaryAllowed = active.SecondaryAllowed;
                        denomination.LetItRideAllowed = active.LetItRideAllowed;
                        if (!denomination.Active)
                        {
                            denomination.ActiveDate = DateTime.UtcNow;
                        }

                        denomination.Active = true;
                    }
                }

                UpdateGame(game);

                if (!enabled && game.EgmEnabled)
                {
                    _bus.Publish(new GameEnabledEvent(gameId, GameStatus.DisabledBySystem, game.ThemeId));
                }
                else if (enabled && !game.EgmEnabled)
                {
                    _bus.Publish(new GameDisabledEvent(gameId, GameStatus.DisabledBySystem, game.ThemeId));

                }

                _bus.Publish(new GameDenomChangedEvent(gameId, game, game.EgmEnabled, _multiplier));

                CheckState();
            }
        }

        /// <inheritdoc />
        public int GetMinimumNumberOfMechanicalReels()
        {
            lock (_sync)
            {
                return _games.MaxOrDefault(g => g.Active ? g.MechanicalReels : 0, 0);
            }
        }

        /// <inheritdoc />
        public bool Add(string path) => Add(path, false);

        /// <inheritdoc />
        public bool Remove(string path)
        {
            var manifest = GetManifest(path);
            if (string.IsNullOrEmpty(manifest))
            {
                return false;
            }

            RemoveFromManifest(manifest);

            CheckState();

            return true;
        }

        /// <inheritdoc />
        public bool Replace(string path, IGameDetail game)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            var current = FindGame(game.ThemeId, game.PaytableId);
            if (current != null && current.Active)
            {
                current.Folder = null;
                current.GameDll = null;
            }

            return Add(path, true);
        }

        /// <inheritdoc />
        public bool Register(string path)
        {
            var manifest = GetManifest(path);
            return !string.IsNullOrEmpty(manifest) && LoadFromManifest(manifest, false, false);
        }

        /// <inheritdoc />
        public IGameDetail Exists(string path)
        {
            var manifest = GetManifest(path);
            if (string.IsNullOrEmpty(manifest))
            {
                return null;
            }

            try
            {
                var gameContent = _manifest.Read(manifest);

                return FindGame(gameContent);
            }
            catch (InvalidManifestException exception)
            {
                Logger.Error($"Error encountered while parsing the manifest: {path}", exception);
            }

            return null;
        }

        /// <inheritdoc />
        public void Configure(int gameId, GameOptionConfigValues options)
        {
            lock (_sync)
            {
                var game = _games.Single(g => g.Id == gameId);

                if (options.MaximumWagerCredits.HasValue)
                {
                    //game.MaximumWagerCredits = options.MaximumWagerCredits.Value;
                }

                UpdateGame(game);
            }
        }

        /// <inheritdoc />
        public void SetGameTags(int gameId, IEnumerable<string> tags)
        {
            lock (_sync)
            {
                var game = _games.Single(g => g.Id == gameId);

                // Replace current tags with new tags
                game.GameTags = tags.ToList();

                UpdateGame(game);

                _bus.Publish(new GameTagsChangedEvent(game));
            }
        }

        /// <inheritdoc />
        public void UpdateGameRuntimeTargets()
        {
            lock (_sync)
            {
                foreach (var gameDetail in _games)
                {
                    if (!string.IsNullOrEmpty(gameDetail.Folder))
                    {
                        var manifest = GetManifest(gameDetail.Folder);
                        if (string.IsNullOrEmpty(manifest))
                        {
                            continue;
                        }

                        var gameContent = _manifest.Read(manifest);

                        gameDetail.TargetRuntime = GetTargetRuntime(gameContent);
                    }
                }
            }
        }

        /// <inheritdoc />
        public ICollection<KeyValuePair<string, object>> GetCollection => new List<KeyValuePair<string, object>>
        {
            new(GamingConstants.Games, GetGames()),
            new(GamingConstants.AllGames, GetAllGames()),
            new(GamingConstants.GameCombos, GetGameCombos())
        };

        /// <inheritdoc />
        public object GetProperty(string propertyName)
        {
            switch (propertyName)
            {
                case GamingConstants.Games:
                    return GetGames();
                case GamingConstants.AllGames:
                    return GetAllGames();
                case GamingConstants.GameCombos:
                    return GetGameCombos();
                default:
                    var errorMessage = "Unknown game property: " + propertyName;
                    Logger.Error(errorMessage);
                    throw new UnknownPropertyException(errorMessage);
            }
        }

        /// <inheritdoc />
        public void SetProperty(string propertyName, object propertyValue)
        {
            // No external sets for this provider...
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IGameProvider), typeof(IServerPaytableInstaller) };

        /// <inheritdoc />
        public void Initialize()
        {
#if !(RETAIL)
            Scan();
#endif
            CheckState();

            if (!_initialized || _meters.GameIdCount == 0)
            {
                _meters.AddGames(_games);
            }

            _initialized = true;
        }

        IGameDetail IServerPaytableInstaller.GetGame(int gameId)
        {
            if (!_properties.GetValue(GamingConstants.ServerControlledPaytables, false))
            {
                return null;
            }

            lock (_sync)
            {
                return _availableGames.Select(x => x.Item1).SingleOrDefault(g => g.Id == gameId);
            }
        }

        IGameDetail IServerPaytableInstaller.InstallGame(int gameId, ServerPaytableConfiguration paytableConfiguration)
        {
            if (!_properties.GetValue(GamingConstants.ServerControlledPaytables, false))
            {
                return null;
            }

            GameDetail game;
            List<ProgressiveDetail> progressiveDetails;
            lock (_sync)
            {
                (game, progressiveDetails) = _availableGames.SingleOrDefault(g => g.Item1.Id == gameId);
                game = game?.ShallowClone();
            }

            if (game is null)
            {
                return null;
            }

            using var scope = _storageManager.ScopedTransaction();
            var result = InstallNewGame(game, progressiveDetails, paytableConfiguration);
            scope.Complete();

            return result;
        }

        void IServerPaytableInstaller.InstallSubGames(int gameId, IReadOnlyCollection<SubGameConfiguration> subGameConfiguration)
        {
            var game = _games.Single(g => g.Id == gameId);
            var manifestSubGames = game.SupportedSubGames.ToList();

            foreach (var subGame in subGameConfiguration)
            {
                var foundSubGame =
                    manifestSubGames.FirstOrDefault(x => long.Parse(x.CdsTitleId) == subGame.GameTitleId) as
                        SubGameDetails;

                if (foundSubGame?.Denominations.FirstOrDefault(x => x.Value == subGame.Denomination) is not null)
                {
                    foundSubGame.SupportedDenoms = new List<long> { subGame.Denomination };
                }
            }

            game.SupportedSubGames = manifestSubGames.Where(subGame => !subGame.SupportedDenoms.IsNullOrEmpty()).ToList();

            lock (_sync)
            {
                UpdatePersistence(game);
            }
        }

        IReadOnlyCollection<IGameDetail> IServerPaytableInstaller.GetAvailableGames()
        {
            lock (_sync)
            {
                return _availableGames.Select(x => x.Item1).ToList();
            }
        }

        private static string GetManifest(string path)
        {
            return Directory.GetFiles(path, ManifestFilter, SearchOption.AllDirectories).FirstOrDefault();
        }

        private bool Add(string path, bool upgradeExisting)
        {
            var manifest = GetManifest(path);
            if (string.IsNullOrEmpty(manifest))
            {
                return false;
            }

            using (var scope = _storageManager.ScopedTransaction())
            {
                if (!LoadFromManifest(manifest, true, upgradeExisting))
                {
                    return false;
                }

                scope.Complete();
            }

            CheckState();

            return true;
        }

        private Dictionary<string, ILocaleGameGraphics> BuildLocaleGraphics(Product gameContent, string gameFolder)
        {
            var localeGraphics = new Dictionary<string, ILocaleGameGraphics>();

            foreach (var graphic in gameContent.Graphics)
            {
                var graphicInfoList = graphic.Value.ToList();
                var icons = graphicInfoList.FindAll(g => g.GraphicType == GraphicType.Icon);
                var denomButtonIcons = graphicInfoList.FindAll(g => g.GraphicType == GraphicType.DenomButton);
                var denomButtonPanels = graphicInfoList.FindAll(g => g.GraphicType == GraphicType.DenomPanel);
                var attractVideos = graphicInfoList.FindAll(g => g.GraphicType == GraphicType.AttractVideo);
                var loadingScreens = graphicInfoList.FindAll(g => g.GraphicType == GraphicType.LoadingScreen);
                var backgroundPreviews = graphicInfoList.FindAll(g => g.GraphicType == GraphicType.BackgroundPreview);
                var pidResources = graphicInfoList.FindAll(g => g.GraphicType == GraphicType.Other && GameAssetTags.PlayerInformationDisplayScreensTags.IsSubsetOf(g.Tags));

                var normalIcons = icons?.Where(icon => !icon.Tags.Contains(GameAssetTags.TopPickTag)).ToList();

                var topPickIcons = icons?.Where(icon => icon.Tags.Contains(GameAssetTags.TopPickTag)).ToList();

                var largeIcon = normalIcons?.Count > 0 ? normalIcons[0] : null;
                var smallIcon = normalIcons?.Count > 1 ? normalIcons[1] : null;

                var largeTopPickIcon = topPickIcons?.Count > 0 ? topPickIcons[0] : null;
                var smallTopPickIcon = topPickIcons?.Count > 1 ? topPickIcons[1] : null;

                var denomButtonIcon = denomButtonIcons.FirstOrDefault();
                var denomButtonPanel = denomButtonPanels.FirstOrDefault();

                var mainDisplay = _cabinetDetectionService.GetDisplayDeviceByItsRole(DisplayRole.Main);
                if (mainDisplay == null)
                {
                    throw new InvalidDataException("Missing Main Display. Cabinet Detection Service could not register Main Display.");
                }

                var mainDisplayRect = Rectangle.Empty;
                if (mainDisplay.Bounds != Rectangle.Empty)
                {
                    mainDisplayRect = mainDisplay.Bounds;
                }
                else if (mainDisplay.WorkingArea != Rectangle.Empty)
                {
                    mainDisplayRect = mainDisplay.WorkingArea;
                }

                var isPortraitEgm = false;
                if (mainDisplayRect != Rectangle.Empty)
                {
                    isPortraitEgm = mainDisplayRect.Width < mainDisplayRect.Height;
                }

                var tagMatchingEgmOrientation = isPortraitEgm ? GameAssetTags.PortraitTag : GameAssetTags.LandscapeTag;

                Graphic topperAttractVideo, topAttractVideo, bottomAttractVideo;
                // Map attract videos using attached metadata Tags
                if (attractVideos.Any(v => v.Tags.Overlaps(GameAssetTags.DisplayAndOrientationTags)))
                {
                    var topperAttractVideos = attractVideos.Where(v => v.Tags.Contains(GameAssetTags.TopperTag)).ToList();
                    var topAttractVideos = attractVideos.Where(v => v.Tags.Contains(GameAssetTags.TopTag)).ToList();
                    var bottomAttractVideos = attractVideos.Where(v => v.Tags.Contains(GameAssetTags.BottomTag)).ToList();

                    topperAttractVideo = ResolveGraphicByTags(topperAttractVideos, GameAssetTags.TopperTag, tagMatchingEgmOrientation);
                    topAttractVideo = ResolveGraphicByTags(topAttractVideos, GameAssetTags.TopTag, tagMatchingEgmOrientation);
                    bottomAttractVideo = ResolveGraphicByTags(bottomAttractVideos, GameAssetTags.BottomTag, tagMatchingEgmOrientation);
                }
                else
                {
                    // IMPORTANT: This is the Legacy method for mapping attract videos to screens.
                    // We must keep this for backwards compatibility with older games.
                    var attractVideoIndex = 0;
                    topperAttractVideo = attractVideos.Count > 2 ? attractVideos[attractVideoIndex++] : null;
                    topAttractVideo = attractVideos.Count > 1 ? attractVideos[attractVideoIndex++] : null;
                    bottomAttractVideo = attractVideos.Count > 0 ? attractVideos[attractVideoIndex] : null;
                }

                // If orientation Tags are included, find the correct image based on the Cabinet's Main Screen orientation
                var loadingScreen =
                    loadingScreens.FirstOrDefault(g => g.Tags?.Contains(tagMatchingEgmOrientation) ?? false) ??
                    loadingScreens.FirstOrDefault();

                var gameGraphics = new LocaleGameGraphics
                {
                    LocaleCode = graphic.Key.Replace(@"_", "-").ToUpperInvariant(),
                    LargeIcon = GetGraphicPath(largeIcon, gameFolder, true),
                    SmallIcon = GetGraphicPath(smallIcon, gameFolder, true),
                    LargeTopPickIcon = largeTopPickIcon != null ? GetGraphicPath(largeTopPickIcon, gameFolder, true) : null,
                    SmallTopPickIcon = smallTopPickIcon != null ? GetGraphicPath(smallTopPickIcon, gameFolder, true) : null,
                    DenomButtonIcon = GetGraphicPath(denomButtonIcon, gameFolder),
                    DenomPanel = GetGraphicPath(denomButtonPanel, gameFolder),
                    TopperAttractVideo = GetGraphicPath(topperAttractVideo, gameFolder),
                    TopAttractVideo = GetGraphicPath(topAttractVideo, gameFolder),
                    BottomAttractVideo = GetGraphicPath(bottomAttractVideo, gameFolder),
                    LoadingScreen = GetGraphicPath(loadingScreen, gameFolder),
                    PlayerInfoDisplayResources = pidResources.Select(x => (x.Tags, GetGraphicPath(x, gameFolder))).ToList(),
                    BackgroundPreviewImages = backgroundPreviews.Where(x => x.Tags.Any())
                        .DistinctBy(x => x.Tags)
                        .Select(x => (x.Tags.FirstOrDefault() ?? string.Empty, GetGraphicPath(x, gameFolder)))
                        .ToList()
                };

                // if one of the icons is left out, use the same icon for both schemes.
                if (string.IsNullOrEmpty(gameGraphics.SmallIcon))
                {
                    gameGraphics.SmallIcon = gameGraphics.LargeIcon;
                }

                if (string.IsNullOrEmpty(gameGraphics.LargeIcon))
                {
                    gameGraphics.LargeIcon = gameGraphics.SmallIcon;
                }

                localeGraphics.Add(gameGraphics.LocaleCode, gameGraphics);
            }

            return localeGraphics;
        }

        private static Graphic ResolveGraphicByTags(IReadOnlyList<Graphic> graphics, params string[] filterByTagsInOrder)
        {
            // [Base Case 1]
            if (graphics.Count == 0)
            {
                return null;
            }

            // [Base Case 2]
            if (graphics.Count == 1)
            {
                return graphics.Single();
            }

            // [Base Case 3]
            if (filterByTagsInOrder == null || filterByTagsInOrder.Length == 0)
            {
                return graphics.FirstOrDefault();
            }

            // [Working towards base cases]
            // Grab the first tag from the tag array and filter out graphics that don't have that tag.
            var nextTag = filterByTagsInOrder.First();
            var remainingTags = filterByTagsInOrder.Skip(1).ToArray();
            var filteredGraphics = graphics.Where(g => g.Tags.Contains(nextTag)).ToArray();

            // [Recursive call]
            // Try to resolve the graphic again, with the remaining tags and with the smaller filtered graphics list.
            return ResolveGraphicByTags(filteredGraphics, remainingTags);
        }

        private static IEnumerable<UpgradeAction> GetUpgradeActions(Product gameContent, string paytableId)
        {
            return gameContent.UpgradeActions?.Where(u => u.ToPaytableId == paytableId);
        }

        private static string GetGraphicPath(Graphic graphic, string gamePath, bool throwIfNotExists = false)
        {
            var path = string.Empty;

            if (graphic != null)
            {
                path = Path.Combine(gamePath, graphic.FileName);
                if (!File.Exists(path))
                {
                    if (throwIfNotExists)
                    {
                        throw new FileNotFoundException($"{path} not found.");
                    }

                    // Indicate file does not exist to use default graphic.
                    path = string.Empty;
                }
            }

            return path;
        }

        private static decimal ConvertToRtp(long value)
        {
            // Older versions of the manifest contained a truncated Rtp (precision of 2), represented as 9821 or 98.21%
            // Newer manifests have a precision of 3, represented as 98212 or 98.212%
            return value > 10000 ? new decimal(value / 1000.0) : new decimal(value / 100.0);
        }

        private static string GetPaytableName(string paytableId)
        {
            var prefixIndex = paytableId.IndexOf(AtiPrefix, StringComparison.Ordinal);
            return prefixIndex < 0
                ? paytableId
                : paytableId.Remove(prefixIndex, AtiPrefix.Length);
        }

        private void Scan()
        {
            Logger.Debug("Initiating game discovery");

            lock (_sync)
            {
                var dir = _pathMapper.GetDirectory(GamingConstants.GamesPath);

                var gameDirs = Directory.GetDirectories(dir.FullName);

                foreach (var gameDir in gameDirs)
                {
                    string[] files;
                    try
                    {
                        files = Directory.GetFiles(gameDir, ManifestFilter, SearchOption.TopDirectoryOnly);
                    }
                    catch (DirectoryNotFoundException e) // Catch this in-case the mount didn't get cleaned up
                    {
                        Logger.Warn("Failed to get the files.  The folder appears to be a left over ISO mount", e);
                        continue;
                    }

                    if (files.Length > 1)
                    {
                        throw new InvalidOperationException("Only one manifest file should be present for each game.");
                    }

                    if (files.Length == 1)
                    {
                        // Attempt to parse the manifest file.
                        var gameContent = _manifest.Read(files[0]);

                        // Add any discovered files that haven't already been processed
                        var current = gameContent.GameAttributes.Select(
                                game => FindGame(game.ThemeId, game.PaytableId, gameContent.ReleaseNumber))
                            .FirstOrDefault(existing => existing != null);

                        if (current == null)
                        {
                            Add(gameDir);
                            Logger.Debug($"Added new game at {gameDir}");
                        }
                        else if (!current.Active)
                        {
                            Register(gameDir);
                            Logger.Debug($"Registered game at {gameDir}");
                        }
                    }
                }
            }

            Logger.Debug("Completed game discovery scan");
        }

        private IEnumerable<(GameDetail, List<ProgressiveDetail>)> GetGameDetailFromManifest(GameContent gameContent, string file, bool upgrade, bool addIfNotExists)
        {
            const string binFolder = @"bin";

            var gameFolder = Path.GetDirectoryName(file);
            if (gameFolder == null)
            {
                Logger.Error($"Unable to get the directory for {file}");
                yield break;
            }

            var gameDll = @"bin\" + gameContent.GameDll;
            var fullGameDllPath = Path.Combine(gameFolder, gameDll);

            var definedGames = gameContent.GameAttributes.ToList();

            if (!_gameSpecificOptionProvider.HasThemeId(definedGames[0].ThemeId))
            {
                var config = LoadGameSpecificOptions(Path.Combine(gameFolder, binFolder));

                _gameSpecificOptionProvider.InitGameSpecificOptionsCache(definedGames[0].ThemeId, GetGameSpecificOptionsFromConfig(config).ToList());
            }

            var progressives = LoadProgressiveDetails(Path.Combine(gameFolder, binFolder)).ToList();

            var isComplex = definedGames.Count > 1;

            var denomLimit = _digitalRights.License.Configuration == Application.Contracts.Drm.Configuration.Vip
                ? long.MaxValue
                : AccountingConstants.DefaultDenominationLimit;

            foreach (var game in definedGames)
            {
                var (gameDetail, progressiveDetails) = CreateGameDetail(progressives, game, denomLimit, gameContent, fullGameDllPath, gameFolder, isComplex, upgrade, addIfNotExists);
                if (gameDetail is null)
                {
                    continue;
                }

                yield return (gameDetail, progressiveDetails);
            }
        }

        private (GameDetail, List<ProgressiveDetail>) CreateGameDetail(
            IEnumerable<ProgressiveDetail> progressives,
            GameAttributes game,
            long denomLimit,
            GameContent gameContent,
            string fullGameDllPath,
            string gameFolder,
            bool isComplex,
            bool upgrade,
            bool addIfNotExists)
        {
            var progressiveDetails =
                progressives.Where(
                    p => p.Variation == "ALL" || p.Variation.Split(',')
                        .Any(v => Convert.ToInt32(v) == Convert.ToInt32(game.VariationId))).ToList();

            if (!IsTypeAllowed(game))
            {
                Logger.Info(
                    $"{game.ThemeId}:{game.PaytableId}'s {game.GameType} type is not allowed in Jurisdiction");
                return (null, null);
            }

            if (!IsValidRtp(game, progressiveDetails))
            {
                Logger.Info($"{game.ThemeId}:{game.PaytableId}'s RTP is not allowed in Jurisdiction");
                return (null, null);
            }

            var wagerCategories = game.WagerCategories.Select(
                w => new WagerCategory(
                    w.Id,
                    ConvertToRtp(w.TheoPaybackPercent),
                    w.MinWagerCredits,
                    w.MaxWagerCredits,
                    w.MaxWinAmount)).ToList();
            var centralAllowed = false;

            if (game.CentralInfo.Any())
            {
                centralAllowed = true;
                _properties.SetProperty(ApplicationConstants.CentralAllowed, true);
            }

            var cdsGameInfos = game.CentralInfo?.GroupBy(
                c => c.Id,
                c => c.Bet,
                (id, bet) =>
                {
                    var betList = bet.ToList();
                    return new CdsGameInfo(
                        id.ToString(),
                        betList.Min(),
                        betList.Max());
                }).ToList() ?? new List<CdsGameInfo>();

            var validDenoms = GetValidDenoms(game, denomLimit).ToList();
            if (!validDenoms.Any())
            {
                Logger.Info($"{game.ThemeId}:{game.PaytableId} has no valid denominations for this jurisdiction");
                return (null, null);
            }

            var features = game?.Features?.Select(
                x => new Feature
                {
                    Name = x.Name,
                    Enable = x.Enable,
                    Editable = x.Editable,
                    StatInfo = x.StatInfo.Select(
                            statInfo => new StatInfo { Name = statInfo.Name, DisplayName = statInfo.DisplayName })
                        .ToList()
                }).ToList();

            isComplex = isComplex || game.Denominations.Count() > 1;
            var shouldAutoEnableGame = !isComplex && _properties.GetValue(GamingConstants.AutoEnableSimpleGames, true);

            var gameDetail = FindGame(game.ThemeId, game.PaytableId, gameContent.ReleaseNumber);
            if (gameDetail is null)
            {
                if (!addIfNotExists)
                {
                    return (null, null);
                }

                var installDate = GetInstallDate();
                gameDetail = new GameDetail
                {
                    ThemeId = game.ThemeId,
                    ThemeName = gameContent.Name,
                    PaytableId = game.PaytableId,
                    Version = gameContent.ReleaseNumber,
                    Status = GameStatus.DisabledByBackend,
                    Denominations =
                        FromValueBasedDenominations(
                            game,
                            validDenoms,
                            shouldAutoEnableGame ? game.Denominations.Single() : 0),
                    WagerCategories = wagerCategories,
                    New = true,
                    InstallDate = installDate,
                    Upgraded = installDate == DateTime.MinValue,
                    GameTags = new List<string>(),
                    SecondaryAllowed = game.SecondaryAllowed,
                    Category = game.Category != null ? (GameCategory)game.Category : GameCategory.Undefined,
                    SubCategory = game.SubCategory != null ? (GameSubCategory)game.SubCategory : GameSubCategory.Undefined,
                    Features = features,
                    CdsGameInfos = cdsGameInfos,
                    MaximumPaybackPercent = ConvertToRtp(game.MaxPaybackPercent),
                    MinimumPaybackPercent = ConvertToRtp(game.MinPaybackPercent)
                };
            }

            gameDetail.Active = true;
            if (!File.Exists(fullGameDllPath))
            {
                gameDetail.Status |= GameStatus.GameFilesNotFound;
            }

            gameDetail.PaytableName = GetPaytableName(game.PaytableId);
            gameDetail.VariationId = game.VariationId;
            if (upgrade)
            {
                gameDetail.MaximumPaybackPercent = ConvertToRtp(game.MaxPaybackPercent);
                gameDetail.MinimumPaybackPercent = ConvertToRtp(game.MinPaybackPercent);
            }

            gameDetail.CentralAllowed = centralAllowed;
            gameDetail.CdsThemeId = game.CdsThemeId;
            gameDetail.CdsTitleId = game.CdsTitleId;
            gameDetail.ProductCode = game.CentralInfo?.FirstOrDefault()?.Upc;
            gameDetail.WinLevels = Enumerable.Empty<IWinLevel>();
            gameDetail.BetOptionList = game.BetOptionList;
            gameDetail.ActiveBetOption = game.ActiveBetOption;
            gameDetail.LineOptionList = game.LineOptionList;
            gameDetail.ActiveLineOption = game.ActiveLineOption;
            gameDetail.BetLinePresetList = game.BetLinePresetList;
            gameDetail.WinThreshold = game.WinThreshold;
            gameDetail.MaximumProgressivePerDenom = game.MaximumProgressivePerDenom;
            gameDetail.ReleaseDate = gameContent.ReleaseDate;
            gameDetail.MechanicalReels = gameContent.MechanicalReels;
            gameDetail.MechanicalReelHomeSteps = gameContent.MechanicalReelHomeSteps;
            gameDetail.Folder = gameFolder;
            gameDetail.GameDll = fullGameDllPath;
            gameDetail.GameIconType = (GameIconType)gameContent.IconType;
            gameDetail.InitialValue = game.InitialValue;
            gameDetail.DisplayMeterName = game.DisplayMeterName;
            gameDetail.AssociatedSapDisplayMeterName = !string.IsNullOrEmpty(game.AssociatedSapDisplayMeterName)
                ? Array.ConvertAll(
                        game.AssociatedSapDisplayMeterName.Split(','),
                        meterName => meterName.Trim())
                    ?.ToList()
                : default(IEnumerable<string>);

            gameDetail.GameType = (GameType)game.GameType;
            gameDetail.GameSubtype = game.GameSubtype;

            gameDetail.LocaleGraphics = BuildLocaleGraphics(gameContent, gameFolder);
            gameDetail.UpgradeActions = GetUpgradeActions(gameContent, gameDetail.PaytableId);
            gameDetail.TargetRuntime = GetTargetRuntime(gameContent);

            gameDetail.ReferenceId = game.ReferenceId;
            gameDetail.Category = game.Category != null ? (GameCategory)game.Category : GameCategory.Undefined;
            gameDetail.SubCategory = game.SubCategory != null
                ? (GameSubCategory)game.SubCategory
                : GameSubCategory.Undefined;
            gameDetail.Features = features;

            gameDetail.MaximumWagerInsideCredits = game.MaxWagerInsideCredits;
            gameDetail.MaximumWagerOutsideCredits = game.MaxWagerOutsideCredits;
            gameDetail.NextToMaxBetTopAwardMultiplier = game.NextToMaxBetTopAwardMultiplier;

            gameDetail.SupportedSubGames ??= GetSubGames(game.SubGames, gameDetail.Denominations.ToList());

            gameDetail.PreloadedAnimationFiles = GetPreloadedAnimationFiles(gameContent, gameFolder);
            gameDetail.UniqueGameId = game.UniqueGameId;

            return (gameDetail, progressiveDetails);
        }

        private IGameDetail InstallNewGame(
            GameDetail gameDetail,
            List<ProgressiveDetail> progressiveDetails,
            ServerPaytableConfiguration paytableConfiguration = null)
        {
            if (!gameDetail.New)
            {
                return gameDetail;
            }

            gameDetail.Id = _idProvider.GetNextDeviceId<GameDetail>();
            if (paytableConfiguration is not null)
            {
                var categoryMap = paytableConfiguration.WagerCategoryOptions.ToDictionary(x => x.Id, x => x);
                gameDetail.MaximumPaybackPercent = paytableConfiguration.MaximumPaybackPercent;
                gameDetail.MinimumPaybackPercent = paytableConfiguration.MinimumPaybackPercent;
                gameDetail.WagerCategories = gameDetail.WagerCategories.Select(x =>
                {
                    if (!categoryMap.TryGetValue(x.Id, out var configuration))
                    {
                        return x;
                    }

                    return new WagerCategory(
                        x.Id,
                        configuration.TheoPaybackPercentRtp,
                        x.MinWagerCredits,
                        x.MinWagerCredits,
                        x.MaxWinAmount);
                });
            }

            gameDetail.Denominations = gameDetail.Denominations.Where(
                x => paytableConfiguration is null ||
                     paytableConfiguration.DenominationConfigurations.Any(d => d.Value == x.Value))
                .Select(
                x => new Denomination(_idProvider.GetNextDeviceId<IDenomination>(), x.Value, x.Active)
                {
                    ActiveDate = x.ActiveDate,
                    BetOption = x.BetOption,
                    BonusBet = x.BonusBet,
                    LetItRideAllowed = x.LetItRideAllowed,
                    LetItRideEnabled = x.LetItRideEnabled,
                    LineOption = x.LineOption,
                    MaximumWagerCredits = x.MaximumWagerCredits,
                    MaximumWagerOutsideCredits = x.MaximumWagerOutsideCredits,
                    MinimumWagerCredits = x.MinimumWagerCredits,
                    SecondaryAllowed = x.SecondaryAllowed,
                    SecondaryEnabled = x.SecondaryEnabled,
                    PreviousActiveTime = x.PreviousActiveTime
                }).ToList();

            lock (_sync)
            {
                _storageManager.ResizeBlock(GetBlockName(DataBlock), gameDetail.Id);
                UpdatePersistence(gameDetail);
                _games.Add(gameDetail);

                if (_initialized)
                {
                    _meters.AddGame(gameDetail);
                }

                // This is only used to track whether or not the game was added in GetOrCreateGame. Reset to avoid reentry
                gameDetail.New = false;
                _progressiveProvider.LoadProgressiveLevels(gameDetail, progressiveDetails);
                _bus.Publish(new GameAddedEvent(gameDetail.Id, gameDetail.ThemeId));
            }

            return gameDetail;
        }

        private bool LoadFromManifest(string file, bool addIfNotExists, bool upgradeExisting)
        {
            var success = false;

            try
            {
                var gameContent = _manifest.Read(file);
                var gameFolder = Path.GetDirectoryName(file);
                if (gameFolder == null)
                {
                    Logger.Error($"Unable to get the directory for {file}");
                    return false;
                }

                string gameThemeId = null;
                var serverControlledPaytables = _properties.GetValue(GamingConstants.ServerControlledPaytables, false);
                foreach (var (gameDetail, progressiveDetails) in GetGameDetailFromManifest(gameContent, file, upgradeExisting, addIfNotExists))
                {
                    if (gameDetail is null)
                    {
                        continue;
                    }

                    if (gameDetail.Id is not default(int))
                    {
                        UpdatePersistence(gameDetail);
                        _progressiveProvider.LoadProgressiveLevels(gameDetail, progressiveDetails);
                    }
                    else if (!serverControlledPaytables)
                    {
                        InstallNewGame(gameDetail, progressiveDetails);
                    }
                    else
                    {
                        lock (_sync)
                        {
                            gameDetail.Id = _availableGames.MaxOrDefault(x => x.Item1.Id, 0) + 1;
                        }
                    }

                    lock (_sync)
                    {
                        _availableGames.Add((gameDetail, progressiveDetails));
                        gameThemeId = gameDetail.ThemeId;
                    }
                }

                if (gameContent.Configurations != null)
                {
                    var games = GetGames();

                    foreach (var config in gameContent.Configurations)
                    {
                        foreach (var mapping in config.GameConfiguration.ConfigurationMapping)
                        {
                            mapping.Active = games.Any(g => g.VariationId == mapping.Variation);
                            Logger.Debug($"{config.Name} variation {mapping.Variation} set to Active={mapping.Active}");
                        }
                    }

                    if (gameContent.Configurations.Any() && gameThemeId != null)
                    {
                        _configurationProvider.Load(
                            gameThemeId,
                            gameContent.Configurations,
                            gameContent.DefaultConfiguration);
                        Logger.Debug(
                            $"Finished loading {gameContent.Configurations.Count()} game restriction configurations for {gameThemeId}");
                    }
                }

                success = true;
            }
            catch (InvalidManifestException manifestException)
            {
                Logger.Error($"Error encountered while parsing the manifest: {file}", manifestException);
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                Logger.Error($"Game icon asset missing: {fileNotFoundException.Message}", fileNotFoundException);
            }

            return success;
        }

        private void RemoveFromManifest(string file)
        {
            var gameContent = _manifest.Read(file);

            foreach (var game in gameContent.GameAttributes)
            {
                lock (_sync)
                {
                    var gameDetail = FindGame(game.ThemeId, game.PaytableId);

                    if (gameDetail != null)
                    {
                        gameDetail.Active = false;
                        gameDetail.GameDll = null;
                        gameDetail.Folder = null;
                        _bus.Publish(new GameRemovedEvent(gameDetail.Id));
                    }
                }
            }
        }

        private void BuildCache()
        {
            var dataBlock = GetAccessor(DataBlock);

            var results = dataBlock.GetAll();

            lock (_sync)
            {
                foreach (var result in results)
                {
                    var gameInfo = result.Value;

                    if ((int)gameInfo[GameIdField] == 0)
                    {
                        continue;
                    }

                    var paytableId = (string)gameInfo[GamePayTableIdField];
                    var paytableName = GetPaytableName(paytableId);
                    var gameDetail = new GameDetail
                    {
                        Id = (int)gameInfo[GameIdField],
                        ThemeId = (string)gameInfo[GameThemeIdField],
                        ThemeName = (string)gameInfo[GameThemeNameField],
                        PaytableId = paytableId,
                        PaytableName = paytableName,
                        Version = (string)gameInfo[GameVersionField],
                        Status = (GameStatus)gameInfo[GameStatusField],
                        Denominations =
                            JsonConvert.DeserializeObject<List<Denomination>>(
                                (string)gameInfo[GameDenominationsField]),
                        WagerCategories =
                            JsonConvert.DeserializeObject<List<WagerCategory>>(
                                (string)gameInfo[GameWagerCategoriesField]),
                        CdsGameInfos =
                            JsonConvert.DeserializeObject<List<CdsGameInfo>>(
                                (string)gameInfo[CdsGameInfosField]),
                        Active = false,
                        InstallDate =
                            DateTime.TryParse(gameInfo[GameInstallDateField].ToString(), out var dt)
                                ? dt
                                : DateTime.MinValue,
                        GameTags =
                            JsonConvert.DeserializeObject<List<string>>((string)gameInfo[GameTagsField]) ??
                            new List<string>(),
                        Upgraded = (bool)gameInfo[GameUpgradedField],
                        Category = (GameCategory)gameInfo[GameCategoryField],
                        SubCategory = (GameSubCategory)gameInfo[GameSubCategoryField],
                        Features =
                            JsonConvert.DeserializeObject<List<Feature>>(
                                (string)gameInfo[GameFeaturesField]),
                        MinimumPaybackPercent =
                            decimal.Parse(
                                (string)gameInfo[GameMinimumPaybackPercentField],
                                NumberStyles.Any,
                                CultureInfo.InvariantCulture),
                        MaximumPaybackPercent = decimal.Parse(
                            (string)gameInfo[GameMaximumPaybackPercentField],
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture),
                        SupportedSubGames =
                            JsonConvert.DeserializeObject<List<SubGameDetails>>(
                                (string)gameInfo[GameSubGameDetailsField])
                    };

                    _games.Add(gameDetail);
                }
            }
        }

        private void UpdatePersistence(IGameDetail game)
        {
            var dataBlock = GetAccessor(DataBlock);

            var blockIndex = game.Id - 1;

            using var transaction = dataBlock.StartTransaction();
            transaction[blockIndex, GameIdField] = game.Id;
            transaction[blockIndex, GameThemeIdField] = game.ThemeId;
            transaction[blockIndex, GameThemeNameField] = game.ThemeName;
            transaction[blockIndex, GamePayTableIdField] = game.PaytableId;
            transaction[blockIndex, GameVersionField] = game.Version;
            transaction[blockIndex, GameStatusField] = game.Status;
            transaction[blockIndex, GameDenominationsField] =
                JsonConvert.SerializeObject(game.Denominations, Formatting.None);
            transaction[blockIndex, GameWagerCategoriesField] =
                JsonConvert.SerializeObject(game.WagerCategories, Formatting.None);
            transaction[blockIndex, CdsGameInfosField] =
                JsonConvert.SerializeObject(game.CdsGameInfos, Formatting.None);
            transaction[blockIndex, GameInstallDateField] = $"{game.InstallDate:g}";
            transaction[blockIndex, GameUpgradedField] = game.Upgraded;
            transaction[blockIndex, GameMaximumWagerCreditsField] = game.MaximumWagerCredits;
            transaction[blockIndex, GameCategoryField] = game.Category;
            transaction[blockIndex, GameSubCategoryField] = game.SubCategory;
            transaction[blockIndex, GameFeaturesField] =
                JsonConvert.SerializeObject(game.Features, Formatting.None);
            transaction[blockIndex, GameMinimumPaybackPercentField] =
                game.MinimumPaybackPercent.ToString(CultureInfo.InvariantCulture);
            transaction[blockIndex, GameMaximumPaybackPercentField] =
                game.MaximumPaybackPercent.ToString(CultureInfo.InvariantCulture);
            transaction[blockIndex, GameSubGameDetailsField] =
                JsonConvert.SerializeObject(game.SupportedSubGames, Formatting.None);

            transaction.Commit();
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

        private void UpdateGame(IGameDetail game)
        {
            var dataBlock = GetAccessor(DataBlock);
            var blockIndex = game.Id - 1;
            using (var transaction = dataBlock.StartTransaction())
            {
                transaction[blockIndex, GameStatusField] = game.Status;
                transaction[blockIndex, GameDenominationsField] =
                    JsonConvert.SerializeObject(game.Denominations, Formatting.None);
                transaction[blockIndex, GameTagsField] = JsonConvert.SerializeObject(game.GameTags, Formatting.None);
                transaction[blockIndex, GameUpgradedField] = game.Upgraded;
                transaction[blockIndex, GameInstallDateField] = $"{game.InstallDate:g}";
                transaction[blockIndex, GameMaximumWagerCreditsField] = game.MaximumWagerCredits;

                transaction.Commit();
            }
        }

        private GameDetail FindGame(string themeId, string paytableId)
        {
            lock (_sync)
            {
                return _games.SingleOrDefault(
                    g => g.Active &&
                         string.Equals(g.ThemeId, themeId, StringComparison.InvariantCultureIgnoreCase) &&
                         string.Equals(g.PaytableId, paytableId, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        private GameDetail FindGame(string themeId, string paytableId, string version)
        {
            lock (_sync)
            {
                return _games.LastOrDefault(
                    g => string.Equals(g.ThemeId, themeId, StringComparison.InvariantCultureIgnoreCase) &&
                         string.Equals(g.PaytableId, paytableId, StringComparison.InvariantCultureIgnoreCase) &&
                         string.Equals(g.Version, version, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        private GameDetail FindGame(GameContent gameContent)
        {
            // TODO: This should be by ProductId, but the studio needs to start providing unique values for that attribute in the manifest
            return gameContent.GameAttributes.Select(game => FindGame(game.ThemeId, game.PaytableId))
                .FirstOrDefault(existing => existing != null);
        }

        private IEnumerable<ProgressiveDetail> LoadProgressiveDetails(string path)
        {
            try
            {
                return _progressiveManifest.Read(Path.Combine(path, ProgressivesConfigFilename));
            }
            catch (Exception exception)
            {
                Logger.Error($"Failed to parse the game's progressive.xml file. {exception.Message}");
                return Enumerable.Empty<ProgressiveDetail>();
            }
        }

        private GameSpecificOptionConfig LoadGameSpecificOptions(string path)
        {
            try
            {
                return _gameSpecificOptionManifest.Read(Path.Combine(path, GameSpecificOptionsConfigFilename));
            }
            catch (Exception exception)
            {
                Logger.Error($"Failed to parse the game's gamespecificoptions.xml file. {exception.Message}");
                return null;
            }
        }

        private IEnumerable<GameSpecificOption> GetGameSpecificOptionsFromConfig(GameSpecificOptionConfig config)
        {
            if (config == null)
                return Enumerable.Empty<GameSpecificOption>();

            var gameSpecificOptions = config.GameToggleOptions.GameToggleOption.
                Select(x => new GameSpecificOption()
            {
                Name = x.name,
                Value = x.value,
                OptionType = OptionType.Toggle,
                ValueSet = new List<string> { ToggleOptions.On.ToString(), ToggleOptions.Off.ToString() }
            });

            gameSpecificOptions = gameSpecificOptions.Concat(config.GameListOptions.GameListOption.
                Select(x => new GameSpecificOption()
                    {
                        Name = x.name,
                        Value = x.value,
                        OptionType = OptionType.List,
                        ValueSet = new List<string>(x.List.Select(z => z.name))
                    }));

            gameSpecificOptions = gameSpecificOptions.Concat(config.GameNumberOptions.GameNumberOption.
                Select(x => new GameSpecificOption()
                    {
                        Name = x.name,
                        Value = x.value.ToString(),
                        OptionType = OptionType.Number,
                        ValueSet = new List<string>(),
                        MinValue = x.minValue,
                        MaxValue = x.maxValue
                    }));

            return gameSpecificOptions.DistinctBy(x => x.Name);
        }

        private DateTime GetInstallDate()
        {
            return _initialized && HasGame() ? DateTime.UtcNow : DateTime.MinValue;
        }

        private bool HasGame()
        {
            return _idProvider.GetCurrentDeviceId<GameDetail>() > 0;
        }

        private void CheckState()
        {
            lock (_sync)
            {
                if (_games.All(g => !g.Enabled))
                {
                    if (!_disableManager.CurrentDisableKeys.Contains(ApplicationConstants.NoGamesEnabledDisableKey))
                    {
                        _disableManager.Disable(
                            ApplicationConstants.NoGamesEnabledDisableKey,
                            SystemDisablePriority.Normal,
                            () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AllGamesDisabled),
                            false);
                    }
                }
                else if (_disableManager.CurrentDisableKeys.Contains(ApplicationConstants.NoGamesEnabledDisableKey))
                {
                    _disableManager.Enable(ApplicationConstants.NoGamesEnabledDisableKey);
                }
            }
        }

        private void HandleUpgrade(GameDetail game)
        {
            if (game.Upgraded || !game.Enabled)
            {
                return;
            }

            if (!(game.UpgradeActions?.Any() ?? false))
            {
                _gameOrder.UpdateIconPositionPriority(game.ThemeId, 1);

                Logger.Info($"No Upgrade Actions: Game Id {game.Id} moved to position 1");
            }
            else
            {
                foreach (var denom in game.ActiveDenominations)
                {
                    var (from, action) = FindMostRelevantUpgradePath(game, denom);
                    if (action == null || from.Version == game.Version)
                    {
                        continue;
                    }

                    Logger.Info(
                        $"Upgrade: Found upgrade action for game Id {game.Id}.  Upgrading from {from.Version} to {game.Version}");

                    if (action.MigrateJackpots) //** && game.HasPools())
                    {
                        //** TODO:  Add support for pool migration
                        //_poolMapper.MigratePools(from.Id, game.Id);
                        Logger.Info($"Upgrade: Migrated for jackpots for game Id {game.Id}");
                    }

                    if (!action.MaintainPosition)
                    {
                        _gameOrder.UpdateIconPositionPriority(game.ThemeId, 1);

                        Logger.Info($"Upgrade: Game Id {game.Id} moved to position 1");
                    }
                    else
                    {
                        game.InstallDate = from.InstallDate;

                        Logger.Info($"Upgrade: Applied {from.InstallDate} install date to game Id {game.Id}");
                    }
                }
            }

            game.Upgraded = true;
            UpdateGame(game);

            Logger.Info($"Upgrade: Completed for game Id {game.Id}");
        }

        private IEnumerable<IDenomination> FromValueBasedDenominations(
            GameAttributes game,
            IReadOnlyCollection<long> supportedDenoms,
            long activeDenom)
        {
            var activeBetOption = game.ActiveBetOption ?? game.BetOptionList?.FirstOrDefault();
            var activeLineOption = game.ActiveLineOption ?? game.LineOptionList?.FirstOrDefault();

            var minWagerCredits = activeLineOption == null || activeBetOption == null
                ? game.WagerCategories.Min(wc => wc.MinWagerCredits)
                : activeBetOption.Bets.Min(b => b.Multiplier) * activeLineOption.Lines.Min(l => l.Cost);

            var maxWagerCredits = activeLineOption == null || activeBetOption == null
                ? game.WagerCategories.Max(wc => wc.MinWagerCredits)
                : activeBetOption.Bets.Max(b => b.Multiplier) * activeLineOption.Lines.Max(l => l.Cost);

            var maxWagerOutsideCredits = maxWagerCredits;

            if (game.GameType == t_gameType.Roulette)
            {
                if (game.MaxWagerInsideCredits > 0)
                {
                    maxWagerCredits = game.MaxWagerInsideCredits;
                }

                if (game.MaxWagerOutsideCredits > 0)
                {
                    maxWagerOutsideCredits = game.MaxWagerOutsideCredits;
                }
            }

            var denoms = supportedDenoms.Select(
                denomination => new Denomination
                {
                    Value = denomination,
                    BetOption = activeBetOption?.Name,
                    LineOption = activeLineOption?.Name,
                    MinimumWagerCredits = minWagerCredits,
                    MaximumWagerCredits = maxWagerCredits,
                    MaximumWagerOutsideCredits = maxWagerOutsideCredits,
                    SecondaryAllowed = game.SecondaryAllowed || game.SecondaryEnabled,
                    SecondaryEnabled = game.SecondaryEnabled, // default value
                    LetItRideAllowed = game.LetItRideAllowed || game.LetItRideEnabled,
                    LetItRideEnabled = game.LetItRideEnabled, // default value
                    Active = denomination == activeDenom
                }).ToList();

            if (game.GameType != t_gameType.Poker || activeBetOption == null)
            {
                return denoms;
            }

            var bonusBets = new ObservableCollection<int>(activeBetOption.BonusBets);

            if (bonusBets.Count > 0)
            {
                denoms.ForEach(d => d.BonusBet = bonusBets.First());
            }

            return denoms;
        }

        private string GetTargetRuntime(GameContent game)
        {
            if (game.Package?.Dependency?.Dependencies == null)
            {
                return _runtimeProvider.DefaultInstance;
            }

            foreach (var dependency in game.Package.Dependency.Dependencies)
            {
                if (dependency is ModuleDependency moduleDependency)
                {
                    var pattern = new Regex(moduleDependency.Pattern);

                    var runtime = _runtimeProvider.FindTargetRuntime(pattern);
                    if (!string.IsNullOrEmpty(runtime))
                    {
                        return runtime;
                    }
                }
            }

            return _runtimeProvider.DefaultInstance;
        }

        private (GameDetail from, UpgradeAction action) FindMostRelevantUpgradePath(GameDetail game, long denom)
        {
            lock (_sync)
            {
                if (game.UpgradeActions != null)
                {
                    // This seems like the most reasonable way to get the most current version
                    foreach (var action in game.UpgradeActions.OrderByDescending(u => u.FromVersion))
                    {
                        var from = _games.FirstOrDefault(
                            g => g.PaytableId == action.FromPaytableId && g.Version == action.FromVersion &&
                                 denom == action.DenomId);

                        if (from != null)
                        {
                            return (from, action);
                        }
                    }
                }
            }

            return (null, null);
        }

        private static int MaximumWagerCredits(GameAttributes game)
        {
            if (game.BetOptionList is null)
            {
                return game.WagerCategories.MaxOrDefault(x => x.MaxWagerCredits, 1);
            }

            var maxInitialBets = game.BetOptionList.Where(x => x.MaxInitialBet.HasValue).ToList();
            if (maxInitialBets.Any())
            {
                return maxInitialBets.Max(x => x.MaxInitialBet).Value;
            }

            var maxBetMultiplier = game.BetOptionList.MaxOrDefault(b => b.Bets.MaxOrDefault(x => x.Multiplier, 1), 1);
            return maxBetMultiplier * BaseMaxWagerCredits(game);
        }

        private static int BaseMaxWagerCredits(GameAttributes game)
        {
            var lineCost = game.LineOptionList?.MaxOrDefault(l => l.Lines.MaxOrDefault(x => x.TotalCost, 0), 0) ?? 0;
            if (lineCost > 0)
            {
                return lineCost;
            }

            var maxWagerCredits = game.WagerCategories.MaxOrDefault(x => x.MaxWagerCredits, 1);
            var maxBetMultiplier =
                game.BetOptionList?.MaxOrDefault(o => o.Bets.MaxOrDefault(b => b.Multiplier, 0), 0) ?? 0;
            return maxBetMultiplier <= 1 ? maxWagerCredits : maxWagerCredits / maxBetMultiplier;
        }

        private IEnumerable<long> GetValidDenoms(GameAttributes game, long denomLimit)
        {
            var maxBetLimit = _properties.GetValue(
                AccountingConstants.MaxBetLimit,
                AccountingConstants.DefaultMaxBetLimit);

            return game.Denominations.Where(d => MaximumWagerCredits(game) * d <= maxBetLimit && d <= denomLimit);
        }

        private bool IsValidRtp(GameAttributes game, IReadOnlyCollection<ProgressiveDetail> progressiveDetails)
        {
            string includeLinkIncrementKey;
            string includeStandaloneIncrementKey;
            string minRtpKey;
            string maxRtpKey;

            // Default to slot for games that are not tagged
            switch (game.GameType)
            {
                case t_gameType.Blackjack:
                    includeLinkIncrementKey = GamingConstants.BlackjackIncludeLinkProgressiveIncrementRtp;
                    includeStandaloneIncrementKey = GamingConstants.BlackjackIncludeStandaloneProgressiveIncrementRtp;
                    minRtpKey = GamingConstants.BlackjackMinimumReturnToPlayer;
                    maxRtpKey = GamingConstants.BlackjackMaximumReturnToPlayer;
                    break;
                case t_gameType.Poker:
                    includeLinkIncrementKey = GamingConstants.PokerIncludeLinkProgressiveIncrementRtp;
                    includeStandaloneIncrementKey = GamingConstants.PokerIncludeStandaloneProgressiveIncrementRtp;
                    minRtpKey = GamingConstants.PokerMinimumReturnToPlayer;
                    maxRtpKey = GamingConstants.PokerMaximumReturnToPlayer;
                    break;
                case t_gameType.Keno:
                    includeLinkIncrementKey = GamingConstants.KenoIncludeLinkProgressiveIncrementRtp;
                    includeStandaloneIncrementKey = GamingConstants.KenoIncludeStandaloneProgressiveIncrementRtp;
                    minRtpKey = GamingConstants.KenoMinimumReturnToPlayer;
                    maxRtpKey = GamingConstants.KenoMaximumReturnToPlayer;
                    break;
                case t_gameType.Roulette:
                    includeLinkIncrementKey = GamingConstants.RouletteIncludeLinkProgressiveIncrementRtp;
                    includeStandaloneIncrementKey = GamingConstants.RouletteIncludeStandaloneProgressiveIncrementRtp;
                    minRtpKey = GamingConstants.RouletteMinimumReturnToPlayer;
                    maxRtpKey = GamingConstants.RouletteMaximumReturnToPlayer;
                    break;
                default:
                    includeLinkIncrementKey = GamingConstants.SlotsIncludeLinkProgressiveIncrementRtp;
                    includeStandaloneIncrementKey = GamingConstants.SlotsIncludeStandaloneProgressiveIncrementRtp;
                    minRtpKey = GamingConstants.SlotMinimumReturnToPlayer;
                    maxRtpKey = GamingConstants.SlotMaximumReturnToPlayer;
                    break;
            }

            return IsValidRtp(
                progressiveDetails,
                includeLinkIncrementKey,
                includeStandaloneIncrementKey,
                minRtpKey,
                maxRtpKey,
                ConvertToRtp(game.MinPaybackPercent),
                ConvertToRtp(game.MaxPaybackPercent));
        }

        private bool IsValidRtp(
            IReadOnlyCollection<ProgressiveDetail> progressiveDetails,
            string includeLinkProgressiveIncrementRtpKey,
            string includeStandaloneProgressiveIncrementKey,
            string minRtpKey,
            string maxRtpKey,
            decimal minPayback,
            decimal maxPayback)
        {
            var includeIncrement = _properties.GetValue(includeLinkProgressiveIncrementRtpKey, false) ||
                                    _properties.GetValue(includeStandaloneProgressiveIncrementKey, false);

            var returnToPlayer = progressiveDetails?.FirstOrDefault()?.ReturnToPlayer;

            var totalRtpMin = (includeIncrement
                ? returnToPlayer?.BaseRtpAndResetRtpAndIncRtpMin
                : returnToPlayer?.BaseRtpAndResetRtpMin) ?? minPayback;
            var totalRtpMax = (includeIncrement
                ? returnToPlayer?.BaseRtpAndResetRtpAndIncRtpMax
                : returnToPlayer?.BaseRtpAndResetRtpMax) ?? maxPayback;

            return totalRtpMax >= totalRtpMin
                   && totalRtpMax > 0
                   && totalRtpMin >= 0
                   && IsValidMinimumRtp(minRtpKey, totalRtpMin)
                   && IsValidMaximumRtp(maxRtpKey, totalRtpMax);
        }

        private bool IsTypeAllowed(GameAttributes game)
        {
            switch (game.GameType)
            {
                case t_gameType.Blackjack:
                    return _properties.GetValue(GamingConstants.AllowBlackjackGames, true);
                case t_gameType.Poker:
                    return _properties.GetValue(GamingConstants.AllowPokerGames, true);
                case t_gameType.Keno:
                    return _properties.GetValue(GamingConstants.AllowKenoGames, true);
                case t_gameType.Roulette:
                    return _properties.GetValue(GamingConstants.AllowRouletteGames, true);
                default:
                    return _properties.GetValue(GamingConstants.AllowSlotGames, true);
            }
        }

        private bool IsValidMinimumRtp(string key, decimal rtp)
        {
            return rtp >= ConvertToRtp(_properties.GetValue(key, int.MinValue));
        }

        private bool IsValidMaximumRtp(string key, decimal rtp)
        {
            return rtp <= ConvertToRtp(_properties.GetValue(key, int.MaxValue));
        }

        private IEnumerable<ISubGameDetails> GetSubGames(IEnumerable<SubGame> subGames, IList<IDenomination> gameDenominations)
        {
            var subGameList = new List<SubGameDetails>();
            foreach (var subGame in subGames)
            {
                var denomList = new List<Denomination>();
                foreach (var denom in subGame.Denominations)
                {
                    var existingDenom = gameDenominations.FirstOrDefault(x => x.Value == denom);
                    if (existingDenom is not null)
                    {
                        denomList.Add(existingDenom as Denomination);
                    }
                    else
                    {
                        var newDenom = new Denomination(_idProvider.GetNextDeviceId<IDenomination>(), denom, false);
                        denomList.Add(newDenom);
                    }
                }

                var cdsGameInfos = subGame.CentralInfo?.GroupBy(
                    c => c.Id,
                    c => c.Bet,
                    (id, bet) =>
                    {
                        var betList = bet.ToList();
                        return new CdsGameInfo(
                            id.ToString(),
                            betList.Min(),
                            betList.Max());
                    }).ToList() ?? new List<CdsGameInfo>();

                var newSubGame = new SubGameDetails(
                    (int)subGame.UniqueGameId,
                    subGame.TitleId.ToString(),
                    denomList,
                    cdsGameInfos);

                subGameList.Add(newSubGame);
            }

            return subGameList;
        }

        private static IEnumerable<AnimationFile> GetPreloadedAnimationFiles(GameContent gameContent, string gameFolder)
        {
            var animationFiles = new List<AnimationFile>();
            if (gameContent.PreloadedAnimationFiles?.stepperAnimationFile == null)
            {
                return animationFiles;
            }

            foreach (var t in gameContent.PreloadedAnimationFiles.stepperAnimationFile)
            {
                animationFiles.Add(
                    new AnimationFile
                    {
                        FilePath = Path.Combine(gameFolder, t.filePath),
                        FileIdentifier = t.identifier
                    });
            }

            return animationFiles;
        }
    }
}