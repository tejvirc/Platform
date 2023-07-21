namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using Application.Contracts.Media;
    using MediaDisplay;
    using Kernel;
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using Aristocrat.Toolkit.Mvvm.Extensions;

    public class LayoutTemplateViewModel : BaseObservableObject, IDisposable
    {
        private readonly IMediaProvider _provider;
        private readonly DisplayType _displayType;
        private readonly bool _isPlaceholder;

        private IEventBus _eventBus;
        private ScreenType? _screenType;
        private double _windowWidth;
        private double _windowHeight; 

        public event EventHandler<int> BrowserProcessTerminated;

        private bool _disposed;

        #region Properties

        public ScreenType? ScreenType
        {
            get => _screenType;
            set
            {
                if (_screenType != value)
                {
                    _screenType = value;
                    if (_screenType != null) LoadProvider();
                }
            }
        }

        public ObservableCollection<MediaPlayerViewModelBase> MediaPlayers { get; set; }

        public double WindowWidth
        {
            get => _windowWidth;

            set
            {
                if (!_windowWidth.Equals(value))
                {
                    _windowWidth = value;
                    OnPropertyChanged(nameof(WindowWidth));
                }
            }
        }

        public double WindowHeight
        {
            get => _windowHeight;

            set
            {
                if (!_windowHeight.Equals(value))
                {
                    _windowHeight = value;
                    OnPropertyChanged(nameof(WindowHeight));
                }
            }
        }

        #endregion Properties

        public LayoutTemplateViewModel(ScreenType? screenType, DisplayType displayType, bool placeholder = false)
        {
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            _provider = ServiceManager.GetInstance().GetService<IMediaProvider>();

            _isPlaceholder = placeholder;
            _displayType = displayType;
            ScreenType = screenType;

            _eventBus.Subscribe<BrowserProcessTerminatedEvent>(this, e => BrowserProcessTerminated?.Invoke(this, e.MediaPlayerId));
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == "WindowWidth" || propertyName == "WindowHeight")
            {
                SetContentSize();
            }
        }

        private void LoadProvider()
        {
            // Only create the media player view models that are appropriate to the current display screen
            if (_isPlaceholder)
            {
                var mediaPlayers = _provider.GetPlaceholders();
                MediaPlayers =
                    new ObservableCollection<MediaPlayerViewModelBase>(
                        mediaPlayers.Where(p => p.DisplayType == _displayType && p.ScreenType == _screenType)
                            .Select(m => new MediaPlayerPlaceholderViewModel(m)));
            }
            else
            {
                var mediaPlayers = _provider.GetMediaPlayers();
                MediaPlayers =
                    new ObservableCollection<MediaPlayerViewModelBase>(
                        mediaPlayers.Where(p => p.DisplayType == _displayType && p.ScreenType == _screenType)
                            .Select(m => new MediaPlayerViewModel(m)));
            }

            // We won't need to adjust sizes of overlay windows on visibility change, so only subscribe for scaled players
            if (_displayType == DisplayType.Scale)
            {
                foreach (var player in MediaPlayers)
                {
                    // Instead of using Show/Hide Media Player Events, subscribe to visibility property change
                    // so we don't get the event here before it's processed to set visibility in MediaPlayerViewModel
                    player.PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName == "IsVisible")
                            Application.Current.Dispatcher.Invoke(SetContentSize);
                    };
                }
            }
        }

        private void SetMediaPlayerSize(MediaPlayerViewModelBase player, ref double availableWidth, ref double availableHeight)
        {
            // Keep full screen players full size and only change opacity/visibility instead of height/width
            if (player.DisplayPosition == DisplayPosition.FullScreen)
            {
                player.ActualWidth = availableWidth;
                player.ActualHeight = availableHeight;
            }
            else if (player.IsVisible)
            {
                SetScaledSize(player.DisplayWidth, player.DisplayHeight, player.DisplayPosition, ref availableWidth, ref availableHeight, out var actualWidth, out var actualHeight);

                player.ActualWidth = actualWidth;
                player.ActualHeight = actualHeight;
            }
            else
            {
                player.ActualWidth = 0;
                player.ActualHeight = 0;
            }
        }

        private void SetContentSize()
        {
            // Use the window size as the basis for aspect ratio because this is the size the game will be scaled to match
            if (WindowHeight.Equals(0) || WindowWidth.Equals(0)) return;

            var availableWidth = WindowWidth;
            var availableHeight = WindowHeight;

            // Set width/height to 0 for invisible scalable players
            MediaPlayers
                .Where(p => !p.IsVisible)
                .OrderBy(p => p.Priority)
                .ToList()
                .ForEach(p => { SetMediaPlayerSize(p, ref availableWidth, ref availableHeight); });

            // Highest priority visible players should be scaled proportionately
            var visiblePlayers = MediaPlayers.Where(p => p.IsVisible).OrderBy(p => p.Priority).ToList();
            
            // Scale all but the last in the list
            MediaPlayerViewModelBase lastVisible = null;
            if (visiblePlayers.Count > 1)
            {
                lastVisible = visiblePlayers.Last();
                visiblePlayers.Remove(lastVisible);
            }

            foreach (var player in visiblePlayers)
            {
                SetMediaPlayerSize(player, ref availableWidth, ref availableHeight);
            }

            // Now scale content proportionately with available space
            SetScaledSize(WindowWidth, WindowHeight, DisplayPosition.FullScreen, ref availableWidth, ref availableHeight, out _, out _);

            // Lowest priority visible should NOT be scaled and should fill remaining space after players and content have been scaled proportionately
            // This is so we don't have blank space when there are equal numbers of banner and service window media displays
            if (lastVisible != null)
            {
                lastVisible.ActualWidth = availableWidth;
                lastVisible.ActualHeight = availableHeight;
            }

            // TODO MediaDisplay: This is probably also where we would add any filler graphics from the studio
        }

        private void SetScaledSize(double displayWidth, double displayHeight, DisplayPosition position, ref double availableWidth, ref double availableHeight, out double actualWidth, out double actualHeight)
        {
            var aspectRatio = displayWidth / displayHeight;

            // Figure out which is the limiting factor
            if (availableWidth / aspectRatio < availableHeight)
            {
                actualWidth = availableWidth;
                actualHeight = availableWidth / aspectRatio;
            }
            else
            {
                actualWidth = availableHeight * aspectRatio;
                actualHeight = availableHeight;
            }

            switch (position)
            {
                case DisplayPosition.Left:
                case DisplayPosition.Right:
                    availableWidth -= actualWidth;
                    break;
                case DisplayPosition.Top:
                case DisplayPosition.Bottom:
                    availableHeight -= actualHeight;
                    break;
                default:
                    // This case is for the main content and should only occur once
                    if (availableWidth > actualWidth)
                    {
                        availableWidth -= actualWidth;
                    }

                    if (availableHeight > actualHeight)
                    {
                        availableHeight -= actualHeight;
                    }
                    break;
            }
        }

        /// <summary>
        ///     Disposes the <see cref="MediaPlayerViewModel"/>.
        /// </summary>
        /// <param name="disposing">True if the call is disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);

                foreach (var player in MediaPlayers)
                {
                    player.Dispose();
                }

                MediaPlayers.Clear();
            }

            _eventBus = null;
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
