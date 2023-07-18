namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Windows.Input;
    using Contracts;
    using log4net;
    using Contracts.PlayerInfoDisplay;
    using Monaco.UI.Common;
    using PlayerInfoDisplay;

    /// <summary>
    ///     Player Information Display Main Page (menu)
    /// </summary>
    public sealed class PlayerInfoDisplayMenuViewModel : BaseObservableObject, IPlayerInfoDisplayViewModel, IDisposable
    {
        private ITimer _closeTimer;
        private readonly IPlayerInfoDisplayFeatureProvider _playerInfoDisplayFeatureProvider;
        private new static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool _isVisible;
        private bool _disposedValue;

        public ICommand ExitClickedCommand { get; }

        public ICommand GameInfoClickedCommand { get; }

        public ICommand GameRulesClickedCommand { get; }

        public PlayerInfoDisplayMenuViewModel(IPlayerInfoDisplayFeatureProvider playerInfoDisplayFeatureProvider)
            : this(playerInfoDisplayFeatureProvider, new DispatcherTimerAdapter())
        {
        }

        public PlayerInfoDisplayMenuViewModel(IPlayerInfoDisplayFeatureProvider playerInfoDisplayFeatureProvider, ITimer timeoutTimer)
        {
            _playerInfoDisplayFeatureProvider = playerInfoDisplayFeatureProvider ?? throw new ArgumentNullException(nameof(playerInfoDisplayFeatureProvider));

            IsVisible = false;

            if (IsDisabled)
            {
                return;
            }

            ExitClickedCommand = new RelayCommand<object>(_ => ExitRequested());

            if (_playerInfoDisplayFeatureProvider.IsGameInfoSupported)
            {
                GameInfoClickedCommand = new RelayCommand<object>(GameInfoRequested);
            }

            if (_playerInfoDisplayFeatureProvider.IsGameRulesSupported)
            {
                GameRulesClickedCommand = new RelayCommand<object>(GameRulesRequested);
            }

            _closeTimer = timeoutTimer ?? new DispatcherTimerAdapter();
            _closeTimer.Interval = TimeSpan.FromMilliseconds(_playerInfoDisplayFeatureProvider.TimeoutMilliseconds);
            _closeTimer.Tick += (_, __) => ExitRequested();
        }

        private EventHandler<CommandArgs> _onButton;

        public void ExitRequested()
        {
            Logger.Debug("Exit from Player Info Display Menu");
            _onButton?.Invoke(this, new CommandArgs(CommandType.Exit));
        }

        private void GameInfoRequested(object obj)
        {
            Logger.Debug("Game Info from Player Info Display Menu");
            _onButton?.Invoke(this, new CommandArgs(CommandType.GameInfo));
        }

        private void GameRulesRequested(object obj)
        {
            Logger.Debug("Game Rules from Player Info Display Menu");
            _onButton?.Invoke(this, new CommandArgs(CommandType.GameRules));
        }

        public string BackgroundImageLandscapePath { get; private set; }

        public string BackgroundImagePortraitPath { get; private set; }

        public bool IsVisible
        {
            get => _isVisible;
            private set
            {
                if (_isVisible != value)
                {
                    Logger.Debug(_isVisible ? "Showing--->Hiding" : "Hiding--->Showing");
                    SetProperty(ref _isVisible, value, nameof(IsVisible));
                }
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _closeTimer?.Stop();
                    _closeTimer = null;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        /// <inheritdoc />
        public PageType PageType => PageType.Menu;

        public bool IsGameInfoButtonVisible => _playerInfoDisplayFeatureProvider.IsGameInfoSupported;

        public bool IsGameRulesButtonVisible => _playerInfoDisplayFeatureProvider.IsGameRulesSupported;

        /// <inheritdoc />
        public void Show()
        {
            IsVisible = true;
            _closeTimer.Start();
        }

        /// <inheritdoc />
        public void Hide()
        {
            IsVisible = false;
            _closeTimer.Stop();
        }

        /// <inheritdoc />
        public event EventHandler<CommandArgs> ButtonClicked
        {
            add => _onButton += value;
            remove => _onButton -= value;
        }

        /// <inheritdoc />
        public void SetupResources(IPlayInfoDisplayResourcesModel model)
        {
            BackgroundImageLandscapePath = model.GetScreenBackground(new HashSet<string> { GameAssetTags.LandscapeTag, GameAssetTags.ScreenTag, GameAssetTags.PlayerInformationDisplayMenuTag });
            BackgroundImagePortraitPath = model.GetScreenBackground(new HashSet<string> { GameAssetTags.PortraitTag, GameAssetTags.ScreenTag, GameAssetTags.PlayerInformationDisplayMenuTag });

            ExitButtonPath = model.GetButton(new HashSet<string>() { GameAssetTags.ExitTag, GameAssetTags.PlayerInformationDisplayMenuTag, GameAssetTags.NormalTag });
            ExitButtonPressedPath = model.GetButton(new HashSet<string>() { GameAssetTags.ExitTag, GameAssetTags.PlayerInformationDisplayMenuTag, GameAssetTags.PressedTag });

            GameInfoButtonPath = model.GetButton(new HashSet<string>() { GameAssetTags.GameInfoTag, GameAssetTags.PlayerInformationDisplayMenuTag, GameAssetTags.NormalTag });
            GameInfoButtonPressedPath = model.GetButton(new HashSet<string>() { GameAssetTags.GameInfoTag, GameAssetTags.PlayerInformationDisplayMenuTag, GameAssetTags.PressedTag });

            GameRulesButtonPath = model.GetButton(new HashSet<string>() { GameAssetTags.GameRulesTag, GameAssetTags.PlayerInformationDisplayMenuTag, GameAssetTags.NormalTag });
            GameRulesButtonPressedPath = model.GetButton(new HashSet<string>() { GameAssetTags.GameRulesTag, GameAssetTags.PlayerInformationDisplayMenuTag, GameAssetTags.PressedTag });

            if (string.IsNullOrWhiteSpace(GameInfoButtonPath))
            {
                Logger.Warn("No image found for Game Info button. Default image will be used.");
                GameInfoButtonPath = GameInfoButtonDefaultPath;
            }

            if (string.IsNullOrWhiteSpace(GameRulesButtonPath))
            {
                Logger.Warn("No image found for Game Rules button. Default image will be used.");
                GameRulesButtonPath = GameRulesButtonDefaultPath;
            }

            if (string.IsNullOrWhiteSpace(ExitButtonPath))
            {
                Logger.Warn("No image found for Exit button. Default image will be used.");
                ExitButtonPath = ExitButtonDefaultPath;
            }

            if (string.IsNullOrWhiteSpace(GameInfoButtonPressedPath))
            {
                Logger.Warn("No image found for Game Info button Pressed. Default image will be used.");
                GameInfoButtonPressedPath = GameInfoButtonPressedDefaultPath;
            }

            if (string.IsNullOrWhiteSpace(GameRulesButtonPressedPath))
            {
                Logger.Warn("No image found for Game Rules button Pressed. Default image will be used.");
                GameRulesButtonPressedPath = GameRulesButtonPressedDefaultPath;
            }

            if (string.IsNullOrWhiteSpace(ExitButtonPressedPath))
            {
                Logger.Warn("No image found for Exit button Pressed. Default image will be used.");
                ExitButtonPressedPath = ExitButtonPressedDefaultPath;
            }

            OnPropertyChanged(ObservablePropertyNames.GameAsset);
        }

        private string _gameInfoButtonPath;
        public string GameInfoButtonPath
        {
            get => _gameInfoButtonPath;
            set
            {
                _gameInfoButtonPath = value;
                OnPropertyChanged(nameof(GameInfoButtonPath));
            }
        }

        private string _gameRulesButtonPath;
        public string GameRulesButtonPath
        {
            get => _gameRulesButtonPath;
            set
            {
                _gameRulesButtonPath = value;
                OnPropertyChanged(nameof(GameRulesButtonPath));
            }
        }

        private string _exitButtonPath;
        public string ExitButtonPath
        {
            get => _exitButtonPath;
            set
            {
                _exitButtonPath = value;
                OnPropertyChanged(nameof(ExitButtonPath));
            }
        }

        private string _gameInfoButtonPressedPath;
        public string GameInfoButtonPressedPath
        {
            get => _gameInfoButtonPressedPath;
            set
            {
                _gameInfoButtonPressedPath = value;
                OnPropertyChanged(nameof(GameInfoButtonPressedPath));
            }
        }

        private string _gameRulesButtonPressedPath;
        public string GameRulesButtonPressedPath
        {
            get => _gameRulesButtonPressedPath;
            set
            {
                _gameRulesButtonPressedPath = value;
                OnPropertyChanged(nameof(GameRulesButtonPressedPath));
            }
        }

        private string _exitButtonPressedPath;
        public string ExitButtonPressedPath
        {
            get => _exitButtonPressedPath;
            set
            {
                _exitButtonPressedPath = value;
                OnPropertyChanged(nameof(ExitButtonPressedPath));
            }
        }

        public bool IsDisabled => !_playerInfoDisplayFeatureProvider.IsPlayerInfoDisplaySupported;

        private const string GameInfoButtonDefaultPath =
            "pack://siteOfOrigin:,,,/../jurisdiction/DefaultAssets/ui/Images/PlayerInfoDisplay/GameInfo_Button.png";

        private const string GameRulesButtonDefaultPath =
            "pack://siteOfOrigin:,,,/../jurisdiction/DefaultAssets/ui/Images/PlayerInfoDisplay/GameRules_Button.png";

        private const string ExitButtonDefaultPath =
            "pack://siteOfOrigin:,,,/../jurisdiction/DefaultAssets/ui/Images/PlayerInfoDisplay/Exit_Button.png";

        private const string GameInfoButtonPressedDefaultPath =
            "pack://siteOfOrigin:,,,/../jurisdiction/DefaultAssets/ui/Images/PlayerInfoDisplay/GameInfo_Button_Pressed.png";

        private const string GameRulesButtonPressedDefaultPath =
            "pack://siteOfOrigin:,,,/../jurisdiction/DefaultAssets/ui/Images/PlayerInfoDisplay/GameRules_Button_Pressed.png";

        private const string ExitButtonPressedDefaultPath =
            "pack://siteOfOrigin:,,,/../jurisdiction/DefaultAssets/ui/Images/PlayerInfoDisplay/Exit_Button_Pressed.png";
    }
}
