namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Windows.Input;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Contracts;
    using log4net;
    using Contracts.PlayerInfoDisplay;
    using Monaco.UI.Common;
    using PlayerInfoDisplay;

    /// <summary>
    ///     Player Information Display Main Page (menu)
    /// </summary>
    public sealed class PlayerInfoDisplayMenuViewModel : ObservableObject, IPlayerInfoDisplayViewModel, IDisposable
    {
        private ITimer _closeTimer;
        private readonly IPlayerInfoDisplayFeatureProvider _playerInfoDisplayFeatureProvider;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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

            OnPropertyChanged(ObservablePropertyNames.GameAsset);
        }

        public string GameRulesButtonPressedPath { get; private set; }

        public string GameRulesButtonPath { get; private set; }

        public string GameInfoButtonPressedPath { get; private set; }

        public string GameInfoButtonPath { get; private set; }

        public string ExitButtonPressedPath { get; private set; }

        public string ExitButtonPath { get; private set; }

        public bool IsDisabled => !_playerInfoDisplayFeatureProvider.IsPlayerInfoDisplaySupported;
    }
}