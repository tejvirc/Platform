namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using Application.Contracts.Media;
    using Kernel;
    using log4net;
    using Monaco.UI.Common;
    using System;
    using System.ComponentModel;
    using System.Reflection;

    /// <summary>
    /// View model class to support the <see cref="IMediaPlayer"/>.
    /// </summary>
    public abstract class MediaPlayerViewModelBase : BaseObservableObject, IDisposable, IMediaPlayerViewModel
    {
        public event PropertyChangedEventHandler WidthChanged;
        public event PropertyChangedEventHandler HeightChanged;
        protected new static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IEventBus _eventBus;
        private readonly object _visStateLock = new object();
        protected readonly object DisplayLock = new object();
        private double _actualWidth;
        private double _actualHeight;
        private double _actualX;
        private double _actualY;
        private bool _isVisible;
        private bool _isAnimating;
        private bool _disposed;
        private bool? _latestVisibleState;

        #region Properties

        /// <summary>
        ///     Gets or sets if the media player should be visible.
        /// </summary>
        public virtual bool IsVisible
        {
            get => _isVisible;

            protected set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    Logger.Debug($"MediaPlayer {Id} IsVisible={value}");
                    OnPropertyChanged(nameof(IsVisible)); 
                }
            }
        }

        public virtual bool IsAnimating
        {
            get => _isAnimating;
            set
            {
                if (_isAnimating == value) return;

                _isAnimating = value;
                Logger.Debug($"MediaPlayer {Id} IsAnimating={value}");

                if (DisplayType == DisplayType.Scale)
                {
                    // Disabling game play here is only used during non-overlay media player open or close for less than 1s each time
                    if (_isAnimating)
                    {
                        _eventBus.Publish(new MediaPlayerResizeStartEvent(Id));
                    }
                    else
                    {
                        _eventBus.Publish(new MediaPlayerResizeStopEvent(Id));
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the unique ID of the media player.
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     Gets or sets the actual width of the content adjusted for scaling.
        /// </summary>
        public double ActualWidth
        {
            get => _actualWidth;

            set
            {
                if (!_actualWidth.Equals(value))
                {
                    var oldValue = _actualWidth;
                    _actualWidth = value;

                    RaiseWidthChanged(oldValue, _actualWidth);
                    OnPropertyChanged(nameof(ActualWidth));
                }
            }
        }

        /// <summary>
        ///     Gets the original width of the media display on the screen.
        /// </summary>
        public double DisplayWidth => Model.DisplayWidth;

        /// <summary>
        ///     Gets or sets the actual height of the content adjusted for scaling.
        /// </summary>
        public double ActualHeight
        {
            get => _actualHeight;

            set
            {
                if (!_actualHeight.Equals(value))
                {
                    var oldValue = _actualHeight;
                    _actualHeight = value;

                    RaiseHeightChanged(oldValue, _actualHeight);
                    OnPropertyChanged(nameof(ActualHeight));
                }
            }
        }

        /// <summary>
        ///     Gets the original height of the media player on the screen.
        /// </summary>
        public double DisplayHeight => Model.DisplayHeight;

        /// <summary>
        ///     Gets the <see cref="DisplayPosition"/> of the media player on the screen.
        /// </summary>
        public DisplayPosition DisplayPosition => Model.DisplayPosition;

        /// <summary>
        ///     Gets the <see cref="DisplayType"/> of the media player on the screen.
        /// </summary>
        public DisplayType DisplayType => Model.DisplayType;

        /// <summary>
        ///     Gets the <see cref="ScreenType"/> of the media player on the screen.
        /// </summary>
        public ScreenType ScreenType => Model.ScreenType;

        /// <summary>
        ///     Gets the priority of the media player on the screen.
        /// </summary>
        public int Priority => Model.Priority;

        /// <summary>
        ///     Gets or sets the actual X position of the content adjusted for scaling.
        /// </summary>
        public double ActualX
        {
            get => _actualX;

            set
            {
                _actualX = value;
                OnPropertyChanged(nameof(ActualX));
            }
        }

        /// <summary>
        ///     Gets the original X position of the media player on the screen.
        /// </summary>
        public double X => Model.XPosition;

        /// <summary>
        ///     Gets or sets the actual Y position of the content adjusted for scaling.
        /// </summary>
        public double ActualY
        {
            get => _actualY;

            set
            {
                _actualY = value;
                OnPropertyChanged(nameof(ActualY));
            }
        }

        /// <summary>
        ///     Gets the original Y position of the media player on the screen.
        /// </summary>
        public double Y => Model.YPosition;

        /// <summary>
        ///     Gets or sets the media player.
        /// </summary>
        public IMediaPlayer Model { get; set; }

        protected bool? LatestVisibleState
        {
            get
            {
                lock (_visStateLock)
                {
                    return _latestVisibleState;
                }
            }
            set
            {
                lock (_visStateLock)
                {
                    _latestVisibleState = value;
                }
                SetLatestVisibility(value);
            }
        }

        protected IEventBus EventBus => _eventBus;

        #endregion Properties

        protected MediaPlayerViewModelBase(IMediaPlayer model) : this()
        {
            Logger.Debug($"Creating VM for Media Player ID {model.Id}");
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            Model = model;
            Id = model.Id;

            Model.ShowRequested += (o, e) => OnVisChangeRequested(true);
            Model.HideRequested += (o, e) => OnVisChangeRequested(false);
        }

        protected MediaPlayerViewModelBase()
        {
            _disposed = false;
        }

        public virtual void ClearBrowser() {}

        protected abstract void OnVisChangeRequested(bool visibleState);

        protected abstract void SetLatestVisibility(bool? latestVisibleState);

        public abstract void SetVisibility(bool? visible = null);

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
            }

            _eventBus = null;
            _disposed = true;
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void RaiseHeightChanged(double oldValue, double newValue)
        {
            HeightChanged?.Invoke(this, new ExtendedPropertyChangedEventArgs<double>("ActualHeight", oldValue, newValue));
        }

        private void RaiseWidthChanged(double oldValue, double newValue)
        {
            WidthChanged?.Invoke(this, new ExtendedPropertyChangedEventArgs<double>("ActualWidth", oldValue, newValue));
        }
    }
}

