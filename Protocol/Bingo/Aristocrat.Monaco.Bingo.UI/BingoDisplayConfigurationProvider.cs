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
    using Common.Events;
    using Events;
    using Gaming.Contracts;
    using Kernel;
    using Localization.Properties;
    using Models;

    public class BingoDisplayConfigurationProvider : IBingoDisplayConfigurationProvider, IService, IDisposable
    {
        private const string DisplayConfigurationFile = @"BingoDisplayConfiguration.xml";

        private readonly BingoHelpAppearance _defaultHelpAppearance = new() { HelpBox = new(0.2, 0.2, 0.2, 0.3) };
        private readonly object _sync = new();

        private readonly ConcurrentDictionary<string, BingoDisplayConfiguration> _displayConfigurations =
            new();

        private readonly IDispatcher _dispatcher;
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IGameProvider _gameProvider;

        private readonly Dictionary<BingoWindow, Window> _windowMap = new();

        private readonly Dictionary<BingoWindow, BingoWindowSettings> _windowSettings =
            new();

        private BingoHelpAppearance _helpAppearance;
        private BingoAttractSettings _attractSettings = new();
        private BingoWindow _currentWindow;
        private bool _disposed;

        public BingoDisplayConfigurationProvider(
            IDispatcher dispatcher,
            IEventBus eventBus,
            IPropertiesManager propertiesManager,
            IGameProvider gameProvider)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));

            Initialize();

            _eventBus.Subscribe<GameConnectedEvent>(this, Handle);
            _eventBus.Subscribe<HostConnectedEvent>(this, Handle);
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

        public BingoHelpAppearance GetHelpAppearance() => _helpAppearance;

        public BingoAttractSettings GetAttractSettings() => _attractSettings;

        /// <inheritdoc />
        public BingoWindowSettings GetSettings(BingoWindow window)
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

        /// <inheritdoc />
        public BingoWindow CurrentWindow
        {
            get => _currentWindow;
            set
            {
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
        public void OverrideHelpAppearance(BingoHelpAppearance helpAppearance)
        {
            if (helpAppearance is not null)
            {
                _helpAppearance = helpAppearance;

                RaiseChangeEvent(_currentWindow);
            }
        }

        public void OverrideSettings(BingoWindow window, BingoWindowSettings settings)
        {
            if (settings is not null && _windowSettings.ContainsKey(window))
            {
                _windowSettings[window] = settings;

                RaiseChangeEvent(window);
            }
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
                BingoInfoWindowSettings = _windowSettings.Values.ToList(),
                HelpAppearance = _helpAppearance
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
            _attractSettings = new BingoAttractSettings();
            _helpAppearance = _defaultHelpAppearance;

            var appearance = new BingoWindowSettings
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
                }
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
            var config = new BingoDisplayConfiguration();
            var serializer = new XmlSerializer(config.GetType());

            var reader = new StreamReader(path);
            config = (BingoDisplayConfiguration)serializer.Deserialize(reader);
            reader.Close();

            return config;
        }

        private void LoadFromSettings(BingoDisplayConfiguration config)
        {
            _windowSettings[BingoWindow.Main] = config.BingoInfoWindowSettings[0];
            RaiseChangeEvent(BingoWindow.Main);

            _helpAppearance = config.HelpAppearance;
            _attractSettings = config.BingoAttractSettings ?? _attractSettings;

            _eventBus.Publish(new BingoDisplayHelpAppearanceChangedEvent(_helpAppearance));
        }

        private void RestoreSettings()
        {
            var windowArray = _windowSettings.Keys.ToArray();
            foreach (var window in windowArray)
            {
                RestoreSettingsForWindow(window);
            }
        }

        private void RestoreSettingsForWindow(BingoWindow window)
        {
            LoadSettings(window);

            RaiseChangeEvent(window);
        }

        private void Handle(GameConnectedEvent evt)
        {
            lock (_sync)
            {
                RestoreSettings();

                var currentGame = _gameProvider.GetGame(_propertiesManager.GetValue(GamingConstants.SelectedGameId, 0));
                if (currentGame != null && _displayConfigurations.TryGetValue(currentGame.Folder, out var configuration))
                {
                    LoadFromSettings(configuration);
                }
            }
        }

        private void Handle(GameAddedEvent evt)
        {
            ScanForGameFiles();
        }

        private void Handle(HostConnectedEvent evt)
        {
            RestoreSettings();
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
