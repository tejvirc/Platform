namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using Application.UI.OperatorMenu;
    using Cabinet.Contracts;
    using Contracts;
    using Hardware.Contracts.Cabinet;
    using Kernel;
    using Models;
    using Monaco.UI.Common.Extensions;

    /// <summary>
    ///     Defines the AttractPreviewViewModel class
    /// </summary>
    [CLSCompliant(false)]
    public class AttractPreviewViewModel : OperatorMenuSaveViewModelBase
    {
        private const double DefaultHeight = 350;
        private const double DefaultWidth = 600;
        private const double PaddingSize = 10;

        private readonly IAttractConfigurationProvider _attractConfigurationProvider;
        private readonly object _attractLock = new object();

        private string _topperAttractVideoPath;
        private string _topAttractVideoPath;
        private string _bottomAttractVideoPath;

        private bool _isTopAttractFeaturePlaying;
        private bool _isTopperAttractFeaturePlaying;
        private bool _isBottomAttractFeaturePlaying;
        private bool _isBottomAttractVisible;

        private int _currentAttractIndex;
        private bool _nextAttractModeLanguageIsPrimary = true;
        private bool _lastInitialAttractModeLanguageIsPrimary = true;

        private List<IAttractDetails> _attractSequence;

        private readonly LobbyConfiguration _lobbyConfig;
        private double _attractVideoWidth = DefaultWidth;
        private double _attractVideoHeight = DefaultHeight;

        public AttractPreviewViewModel()
        {
            _attractConfigurationProvider = ServiceManager.GetInstance().GetService<IAttractConfigurationProvider>();
            _lobbyConfig = (LobbyConfiguration)PropertiesManager.GetProperty(GamingConstants.LobbyConfig, null);

            var cabinetDetectionService = ServiceManager.GetInstance().GetService<ICabinetDetectionService>();
            var expectedDisplays = cabinetDetectionService.ExpectedDisplayDevices.ToList();

            IsTopperAttractVisible = expectedDisplays.Any(d => d.Role == DisplayRole.Topper);
            IsTopAttractVisible = expectedDisplays.Any(d => d.Role == DisplayRole.Top);

            OnTopperGameAttractCompleteHandler = OnTopperGameAttractVideoCompleted;
            OnTopGameAttractCompleteHandler = OnTopGameAttractVideoCompleted;
            OnBottomGameAttractCompleteHandler = OnBottomGameAttractVideoCompleted;

            TopAttractVideoPath = null;
            TopperAttractVideoPath = null;
            BottomAttractVideoPath = null;
        }

        public RoutedEventHandler OnTopperGameAttractCompleteHandler { get; }

        public RoutedEventHandler OnTopGameAttractCompleteHandler { get; }

        public RoutedEventHandler OnBottomGameAttractCompleteHandler { get; }

        public double MaxHeight { get; } = 850;

        public double AttractVideoHeight
        {
            get => _attractVideoHeight;
            set => SetProperty(ref _attractVideoHeight, value, nameof(AttractVideoHeight));
        }

        public double AttractVideoWidth
        {
            get => _attractVideoWidth;
            set => SetProperty(ref _attractVideoWidth, value, nameof(AttractVideoWidth));
        }

        /// <summary>
        ///     Gets or sets the topper attract video path
        /// </summary>
        public string TopperAttractVideoPath
        {
            get => _topperAttractVideoPath;
            set
            {
                _topperAttractVideoPath = value;
                RaisePropertyChanged(nameof(TopperAttractVideoPath));
            }
        }

        /// <summary>
        ///     Gets or sets the top attract video path
        /// </summary>
        public string TopAttractVideoPath
        {
            get => _topAttractVideoPath;
            set
            {
                _topAttractVideoPath = value;
                RaisePropertyChanged(nameof(TopAttractVideoPath));
            }
        }

        /// <summary>
        ///     Gets or sets the bottom attract video path
        /// </summary>
        public string BottomAttractVideoPath
        {
            get => _bottomAttractVideoPath;
            set
            {
                _bottomAttractVideoPath = value;
                RaisePropertyChanged(nameof(BottomAttractVideoPath));
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the topper attract feature is playing or not
        /// </summary>
        public bool IsTopperAttractFeaturePlaying
        {
            get => _isTopperAttractFeaturePlaying;
            set => SetProperty(
                ref _isTopperAttractFeaturePlaying,
                value,
                nameof(IsTopperAttractFeaturePlaying));
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the top attract feature is playing or not
        /// </summary>
        public bool IsTopAttractFeaturePlaying
        {
            get => _isTopAttractFeaturePlaying;
            set => SetProperty(ref _isTopAttractFeaturePlaying, value, nameof(IsTopAttractFeaturePlaying));
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the bottom attract feature is playing or not
        /// </summary>
        public bool IsBottomAttractFeaturePlaying
        {
            get => _isBottomAttractFeaturePlaying;
            set => SetProperty(ref _isBottomAttractFeaturePlaying, value, nameof(IsBottomAttractFeaturePlaying));
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the bottom attract feature is visible or not
        /// </summary>
        public bool IsBottomAttractVisible
        {
            get => _isBottomAttractVisible;
            set => SetProperty(ref _isBottomAttractVisible, value, nameof(IsBottomAttractVisible));
        }

        public bool IsTopAttractVisible { get; }

        public bool IsTopperAttractVisible { get; }

        /// <summary>
        ///     Gets or sets the current attract index
        /// </summary>
        public int CurrentAttractIndex
        {
            get => _currentAttractIndex;
            set => SetProperty(ref _currentAttractIndex, value, nameof(CurrentAttractIndex));
        }

        /// <summary>
        ///     Gets the active locale code.
        /// </summary>
        public string ActiveLocaleCode => _lobbyConfig.LocaleCodes[0];

        private void LoadAttractSequence()
        {
            var configuredAttract =
                _attractConfigurationProvider.GetAttractSequence().Where(ai => ai.IsSelected).ToList();

            var gameProvider = ServiceManager.GetInstance().GetService<IGameProvider>();
            var attractSequence = new List<IAttractDetails>();
            var enabledGames = gameProvider.GetEnabledGames();
            if (_lobbyConfig.HasAttractIntroVideo)
            {
                attractSequence.Add(new AttractVideoDetails
                {
                    BottomAttractVideoPath = _lobbyConfig.BottomAttractIntroVideoFilename,
                    TopAttractVideoPath = _lobbyConfig.TopAttractIntroVideoFilename,
                    TopperAttractVideoPath = _lobbyConfig.TopperAttractIntroVideoFilename
                });
            }

            var gameAttracts = configuredAttract.Join(
                enabledGames,
                attractInfo => new { attractInfo.GameType, attractInfo.ThemeId },
                gameDetail => new { gameDetail.GameType, gameDetail.ThemeId },
                (attractInfo, gameDetail) => new GameInfo
                {
                    GameId = gameDetail.Id,
                    Name = gameDetail.ThemeName,
                    InstallDateTime = gameDetail.InstallDate,
                    TopAttractVideoPath = gameDetail.LocaleGraphics[ActiveLocaleCode].TopAttractVideo,
                    TopperAttractVideoPath = gameDetail.LocaleGraphics[ActiveLocaleCode].TopperAttractVideo,
                    BottomAttractVideoPath = gameDetail.LocaleGraphics[ActiveLocaleCode].BottomAttractVideo,
                    GameType = gameDetail.GameType,
                    GameSubtype = gameDetail.GameSubtype,
                    Enabled = gameDetail.Enabled,
                    AttractHighlightVideoPath =
                        !string.IsNullOrEmpty(gameDetail.DisplayMeterName)
                            ? _lobbyConfig.AttractVideoWithBonusFilename
                            : _lobbyConfig.AttractVideoNoBonusFilename,
                    LocaleGraphics = gameDetail.LocaleGraphics,
                    ThemeId = gameDetail.ThemeId
                });

            attractSequence.AddRange(gameAttracts);
            _attractSequence = attractSequence;
        }

        private void SetAttractVideos()
        {
            IAttractDetails attractDetails = null;

            if (_attractSequence.Count > 0)
            {
                attractDetails = _attractSequence[CurrentAttractIndex];
            }

            if (_lobbyConfig.AlternateAttractModeLanguage)
            {
                var languageIndex = _nextAttractModeLanguageIsPrimary ? 0 : 1;

                TopAttractVideoPath =
                    attractDetails?.GetTopAttractVideoPathByLocaleCode(_lobbyConfig.LocaleCodes[languageIndex]).NullIfEmpty() ??
                    _lobbyConfig.DefaultTopAttractVideoFilename;

                TopperAttractVideoPath =
                    attractDetails?.GetTopperAttractVideoPathByLocaleCode(_lobbyConfig.LocaleCodes[languageIndex])
                        .NullIfEmpty() ??
                    _lobbyConfig.DefaultTopperAttractVideoFilename;

                if (_lobbyConfig.BottomAttractVideoEnabled)
                {
                    BottomAttractVideoPath =
                        attractDetails?.GetBottomAttractVideoPathByLocaleCode(_lobbyConfig.LocaleCodes[languageIndex])
                            .NullIfEmpty() ??
                        _lobbyConfig.DefaultTopAttractVideoFilename;
                }

                return;
            }

            TopAttractVideoPath = attractDetails?.TopAttractVideoPath.NullIfEmpty() ??
                                  _lobbyConfig.DefaultTopAttractVideoFilename;

            TopperAttractVideoPath =
                attractDetails?.TopperAttractVideoPath.NullIfEmpty() ??
                _lobbyConfig.DefaultTopperAttractVideoFilename;

            if (_lobbyConfig.BottomAttractVideoEnabled)
            {
                BottomAttractVideoPath =
                    attractDetails?.BottomAttractVideoPath.NullIfEmpty() ?? _lobbyConfig.DefaultTopAttractVideoFilename;
            }
        }

        private void StartAttract()
        {
            UpdateView();
            SetAttractVideos();
        }

        private void UpdateView()
        {
            IsTopAttractFeaturePlaying = IsTopAttractVisible;
            IsTopperAttractFeaturePlaying = IsTopperAttractVisible;
            IsBottomAttractVisible = _lobbyConfig.BottomAttractVideoEnabled;
            IsBottomAttractFeaturePlaying = _lobbyConfig.BottomAttractVideoEnabled;
            SetVideoSize();
        }

        private void SetVideoSize()
        {
            var monitorCount = 0;
            var maxVideoHeight = MaxHeight - PaddingSize;
            if (IsTopAttractFeaturePlaying)
            {
                ++monitorCount;
            }

            if (IsTopperAttractFeaturePlaying)
            {
                ++monitorCount;
            }

            if (IsBottomAttractFeaturePlaying)
            {
                ++monitorCount;
            }

            AttractVideoHeight = Math.Min(DefaultHeight, maxVideoHeight / monitorCount);
            AttractVideoWidth = Math.Min(DefaultWidth, (AttractVideoHeight / DefaultHeight) * DefaultWidth);
        }

        private void StartNextAttract()
        {
            if (_attractSequence.Count <= 1)
            {
                StopAndUnloadAttractVideo();
            }

            // Increment to next attract mode video.
            var wrap = AdvanceAttractIndex();

            if (_lobbyConfig.AlternateAttractModeLanguage)
            {
                // Use alternate language for alternate attract videos
                _nextAttractModeLanguageIsPrimary = !_nextAttractModeLanguageIsPrimary;
            }

            if (wrap && _lobbyConfig.AlternateAttractModeLanguage)
            {
                _nextAttractModeLanguageIsPrimary = !_lastInitialAttractModeLanguageIsPrimary;
                _lastInitialAttractModeLanguageIsPrimary = _nextAttractModeLanguageIsPrimary;
            }

            UpdateView();

            SetAttractVideos();
        }

        private void OnAttractExit()
        {
            StopAndUnloadAttractVideo();
        }

        // returns true if we wrap around to index 0
        private bool AdvanceAttractIndex()
        {
            lock (_attractLock)
            {
                CurrentAttractIndex++;

                return CheckAndResetAttractIndex();
            }
        }

        private bool CheckAndResetAttractIndex()
        {
            lock (_attractLock)
            {
                if (CurrentAttractIndex >= _attractSequence.Count)
                {
                    ResetAttractIndex();

                    return true;
                }

                return false;
            }
        }

        private void ResetAttractIndex()
        {
            lock (_attractLock)
            {
                CurrentAttractIndex = 0;
            }
        }

        private void OnTopperGameAttractVideoCompleted(object sender, RoutedEventArgs e)
        {
            if (IsTopperAttractFeaturePlaying && !IsBottomAttractFeaturePlaying && !IsTopAttractFeaturePlaying)
            {
                Task.Run(() => StartNextAttract());
            }
        }

        private void OnTopGameAttractVideoCompleted(object sender, RoutedEventArgs e)
        {
            if (IsTopAttractFeaturePlaying)
            {
                Task.Run(() => StartNextAttract());
            }
        }

        private void OnBottomGameAttractVideoCompleted(object sender, RoutedEventArgs e)
        {
            if (IsBottomAttractFeaturePlaying && !IsTopAttractFeaturePlaying)
            {
                Task.Run(() => StartNextAttract());
            }
        }

        private void StopAndUnloadAttractVideo()
        {
            TopAttractVideoPath = null;
            TopperAttractVideoPath = null;
            BottomAttractVideoPath = null;
            IsBottomAttractFeaturePlaying = false;
            IsTopAttractFeaturePlaying = false;
            IsTopperAttractFeaturePlaying = false;
        }

        protected override void OnLoaded()
        {
            LoadAttractSequence();
            StartAttract();

            base.OnLoaded();
        }

        protected override void OnUnloaded()
        {
            OnAttractExit();

            base.OnUnloaded();
        }
    }
}
