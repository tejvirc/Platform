namespace Aristocrat.Monaco.Bingo.UI
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Xml.Serialization;
    using Application.Contracts.Localization;
    using Common;
    using Events;
    using Gaming.Contracts;
    using Kernel;
    using Localization.Properties;
    using Models;
    using OverlayServer.Data.Bingo;
    using PresentationOverrideMessageFormat = OverlayServer.Data.Bingo.BingoDisplayConfigurationPresentationOverrideMessageFormat;

    public class BingoDisplayConfigurationProvider : IBingoDisplayConfigurationProvider, IService, IDisposable
    {
        private const string DisplayConfigurationFile = @"BingoDisplayConfiguration.xml";

        private readonly BingoDisplayConfigurationHelpAppearance _defaultHelpAppearance =
            new()
            {
                HelpBox = new BingoDisplayConfigurationHelpAppearanceHelpBox
                {
                    Left = 0.2, Top = 0.2, Right = 0.2, Bottom = 0.3
                }
            };

        private readonly object _sync = new();

        private readonly ConcurrentDictionary<string, BingoDisplayConfiguration> _displayConfigurations =
            new();

        private readonly IDispatcher _dispatcher;
        private readonly IEventBus _eventBus;
        private readonly IGameProvider _gameProvider;
        private readonly Dictionary<BingoWindow, Window> _windowMap = new();

        private readonly Dictionary<BingoWindow, BingoDisplayConfigurationBingoWindowSettings> _windowSettings =
            new();

        private BingoDisplayConfigurationHelpAppearance _helpAppearance;
        private BingoDisplayConfigurationBingoAttractSettings _attractSettings = new();
        private List<PresentationOverrideMessageFormat> _presentationOverrideMessageFormats = new();
        private BingoWindow _currentWindow;
        private bool _disposed;
        private int _version;
        private int _selectedGameId;

        public BingoDisplayConfigurationProvider(
            IDispatcher dispatcher,
            IEventBus eventBus,
            IGameProvider gameProvider)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));

            Initialize();

            _eventBus.Subscribe<GameSelectedEvent>(this, Handle);
            _eventBus.Subscribe<GameAddedEvent>(this, Handle);
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IBingoDisplayConfigurationProvider) };

        /// <inheritdoc />
        public void Initialize()
        {
            LoadSettings(BingoWindow.Main);

            ScanForGameFiles();
        }

        public List<PresentationOverrideMessageFormat> GetPresentationOverrideMessageFormats() => _presentationOverrideMessageFormats;

        public BingoDisplayConfigurationHelpAppearance GetHelpAppearance() => _helpAppearance;

        public BingoDisplayConfigurationBingoAttractSettings GetAttractSettings() => _attractSettings;

        /// <inheritdoc />
        public BingoDisplayConfigurationBingoWindowSettings GetSettings(BingoWindow window)
        {
            if (!_windowSettings.ContainsKey(window))
            {
                throw new ArgumentException(nameof(window));
            }

            return _windowSettings[window];
        }

        /// <inheritdoc />
        public Window GetWindow(BingoWindow window)
        {
            if (!_windowMap.ContainsKey(window))
            {
                throw new ArgumentException(nameof(window));
            }

            return _windowMap[window];
        }

        public int GetVersion() => _version;

        /// <inheritdoc />
        public BingoWindow CurrentWindow
        {
            get => _currentWindow;
            set
            {
                if (value == _currentWindow)
                {
                    return;
                }

                _currentWindow = value;
                RaiseChangeEvent(_currentWindow);
            }
        }

        /// <inheritdoc />
        public void LoadFromFile(string path)
        {
            var settingsArray = CreateSettingsFromFile(path);
            LoadFromSettings(settingsArray);
        }

#if !RETAIL
        public void OverrideHelpAppearance(BingoDisplayConfigurationHelpAppearance helpAppearance)
        {
            if (helpAppearance is null)
            {
                return;
            }

            _helpAppearance = helpAppearance;
            RaiseChangeEvent(_currentWindow);
        }

        public void OverrideSettings(BingoWindow window, BingoDisplayConfigurationBingoWindowSettings settings)
        {
            if (settings is null || !_windowSettings.ContainsKey(window))
            {
                return;
            }

            _windowSettings[window] = settings;
            RaiseChangeEvent(window);
        }

        public void RestoreSettings(BingoWindow window)
        {
            RestoreSettingsForWindow(window);
        }

        /// <inheritdoc />
        public void SaveToFile(string path)
        {
            var config = new BingoDisplayConfiguration
            {
                BingoAttractSettings = _attractSettings,
                BingoInfoWindowSettings = _windowSettings.Values.ToArray(),
                HelpAppearance = _helpAppearance,
                Version = _version,
                PresentationOverrideMessageFormats = _presentationOverrideMessageFormats.ToArray()
            };

            var serializer = new XmlSerializer(config.GetType());
            var writer = new StreamWriter(path);
            serializer.Serialize(writer, config);
            writer.Close();
        }
#endif

        public void LobbyInitialized()
        {
            LoadWindowMap();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        private void LoadWindowMap()
        {
            _dispatcher.ExecuteOnUIThread(
                () =>
                {
                    foreach (Window window in Application.Current.Windows)
                    {
                        switch (window.Title)
                        {
                            case GamingConstants.MainWindowTitle:
                                _windowMap[BingoWindow.Main] = window;
                                break;
                            default:
                                continue;
                        }
                    }

                    _currentWindow = BingoWindow.Main;
                    if (_windowMap.Keys.Contains(BingoWindow.Vbd))
                    {
                        _currentWindow = BingoWindow.Vbd;
                    }
                    else if (_windowMap.Keys.Contains(BingoWindow.Top))
                    {
                        _currentWindow = BingoWindow.Top;
                    }
                });
        }

        private void LoadSettings(BingoWindow window)
        {
            _attractSettings = new BingoDisplayConfigurationBingoAttractSettings();
            _helpAppearance = _defaultHelpAppearance;
            _presentationOverrideMessageFormats = new List<PresentationOverrideMessageFormat>();

            var appearance = new BingoDisplayConfigurationBingoWindowSettings
            {
                Allow0PaddingBingoCard = false,
                Allow0PaddingBallCall = false,
                BallCallTitle = BingoConstants.DefaultBallCallTitle,
                CardTitle = BingoConstants.DefaultCardTitle,
                CssPath = BingoConstants.DefaultOverlayCssPath,
                FreeSpaceCharacter = BingoConstants.DefaultFreeSpaceCharacter,
                InitialScene = BingoConstants.DefaultInitialOverlayScene,
                PatternCyclePeriod = BingoConstants.DefaultPatternCycleTime,
                WaitingForGameMessage = Localizer.For(CultureFor.Player).GetString(ResourceKeys.WaitingForPlayersText),
                WaitingForGameTimeoutMessage = Localizer.For(CultureFor.Player).GetString(ResourceKeys.NoPlayersFoundText),
                WaitingForGameDelaySeconds = BingoConstants.DefaultDelayStartWaitingForPlayersSeconds,
                WaitingForGameTimeoutDisplaySeconds = BingoConstants.DefaultNoPlayersFoundSeconds,
                DisclaimerText = new List<string>
                {
                    Localizer.For(CultureFor.Player).GetString(ResourceKeys.MalfunctionVoids).ToUpper(),
                    Localizer.For(CultureFor.Player).GetString(ResourceKeys.DisclaimerAllPrizes).ToUpper(),
                    Localizer.For(CultureFor.Player).GetString(ResourceKeys.DisclaimerReelsAre).ToUpper()
                }.ToArray()
            };

            _windowSettings[window] = appearance;
        }

        private void ScanForGameFiles()
        {
            var allGames = _gameProvider.GetAllGames().Where(
                game => game?.Folder is not null && !_displayConfigurations.ContainsKey(game.Folder));
            foreach (var game in allGames)
            {
                var file = Path.Combine(game.Folder, DisplayConfigurationFile);
                if (!File.Exists(file))
                {
                    continue;
                }

                var bingoDisplayConfiguration = CreateSettingsFromFile(file);
                _displayConfigurations.AddOrUpdate(
                    game.Folder,
                    _ => bingoDisplayConfiguration,
                    (_, _) => bingoDisplayConfiguration);
            }
        }

        private BingoDisplayConfiguration CreateSettingsFromFile(string path)
        {
            var config = ConfigurationUtilities.SafeDeserialize<BingoDisplayConfiguration>(path) ??
                         new BingoDisplayConfiguration();

            _version = config.Version;
            foreach (var windowSettings in config.BingoInfoWindowSettings)
            {
                windowSettings.DisclaimerText ??= new List<string>
                {
                    Localizer.For(CultureFor.Player).GetString(ResourceKeys.MalfunctionVoids).ToUpper(),
                    Localizer.For(CultureFor.Player).GetString(ResourceKeys.DisclaimerAllPrizes).ToUpper(),
                    Localizer.For(CultureFor.Player).GetString(ResourceKeys.DisclaimerReelsAre).ToUpper()
                }.ToArray();
            }

            config.PresentationOverrideMessageFormats ??= Array.Empty<PresentationOverrideMessageFormat>();
            return config;
        }

        private void LoadFromSettings(BingoDisplayConfiguration config)
        {
            lock (_sync)
            {
                _windowSettings[BingoWindow.Main] = config.BingoInfoWindowSettings.First();

                _helpAppearance = config.HelpAppearance;
                _attractSettings = config.BingoAttractSettings ?? _attractSettings;
                _presentationOverrideMessageFormats = config.PresentationOverrideMessageFormats?.ToList();
                _version = config.Version;

                RaiseChangeEvent(BingoWindow.Main);
            }
        }

        private void RestoreSettingsForWindow(BingoWindow window)
        {
            LoadSettings(window);

            RaiseChangeEvent(window);
        }

        private void Handle(GameSelectedEvent evt)
        {
            if (evt.GameId == _selectedGameId)
            {
                return;
            }

            _selectedGameId = evt.GameId;
            var currentGame = _gameProvider.GetGame(_selectedGameId);
            if (currentGame != null && _displayConfigurations.TryGetValue(currentGame.Folder, out var configuration))
            {
                LoadFromSettings(configuration);
            }
        }

        private void Handle(GameAddedEvent evt)
        {
            ScanForGameFiles();
        }

        private void RaiseChangeEvent(BingoWindow changedWindow)
        {
            if (_windowMap.ContainsKey(changedWindow))
            {
                _eventBus.Publish(
                    new BingoDisplayConfigurationChangedEvent(
                        _windowMap[changedWindow],
                        _windowSettings[changedWindow]));
            }

            _eventBus.Publish(new BingoDisplayHelpAppearanceChangedEvent(_helpAppearance));
        }
    }
}
