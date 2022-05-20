namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using Application.Contracts.Media;
    using Contracts;
    using Contracts.Events;
    using Kernel;

    /// <summary>
    /// View model class to support the <see cref="IMediaPlayer"/>.
    /// </summary>
    public class MediaPlayerViewModel : MediaPlayerViewModelBase
    {
        public const string DummyUrl = "dummy:";

        private readonly IGamePlayState _gameState;
        private string _address;
        private bool _isGamePlaying;

        #region Properties

        /// <summary>
        ///     Gets or sets if the media player should be visible.
        /// </summary>
        public override bool IsVisible
        {
            get => base.IsVisible;

            protected set
            {   
                if (IsVisible != value)
                {
                    base.IsVisible = value;

                    if (Model.IsPrimaryOverlay())
                    {
                        EventBus.Publish(new PrimaryOverlayMediaPlayerEvent(IsVisible));
                    }

                    ServiceManager.GetInstance().GetService<IMediaProvider>().ShowHidePlaceholders(Id, value);
                }
            }
        }

        public override bool IsAnimating
        {
            get => base.IsAnimating;
            set
            {
                if (IsAnimating == value) return;

                base.IsAnimating = value;

                if (!IsAnimating)
                {
                    SetLatestVisibility(LatestVisibleState);
                }
            }
        }

        /// <summary>
        ///     Gets or sets the address of the media player content.
        /// </summary>
        public string Address
        {
            get => _address;

            set
            {
                _address = value;
                if (_address == null)
                {
                    // If the address is set to null, hide the player
                    if (IsVisible)
                    {
                        ServiceManager.GetInstance().GetService<IMediaProvider>().Hide(Model.Id);
                    }
                    else
                    {
                        ClearBrowser();
                    }
                }
                else
                {
                    RaisePropertyChanged(nameof(Address));
                }
            }
        }

        #endregion Properties

        public MediaPlayerViewModel(IMediaPlayer model) : base(model)
        {
            Logger.Debug($"Creating VM for Media Player ID {model.Id}");

            EventBus.Subscribe<GamePlayStateChangedEvent>(this, Handler);
            EventBus.Subscribe<SetContentMediaPlayerEvent>(this, Handler);
            EventBus.Subscribe<ReleaseContentMediaPlayerEvent>(this, Handler);
            EventBus.Subscribe<ToggleMediaPlayerTestEvent>(this, Handler);
            EventBus.Subscribe<MediaPlayerContentReadyEvent>(this, Handler);
            EventBus.Subscribe<GamePlayDisabledEvent>(this, Handler);

            var containerService = ServiceManager.GetInstance().TryGetService<IContainerService>();
            _gameState = containerService.Container.GetInstance<IGamePlayState>();

            EventBus.Publish(new MediaPlayerSetAudioMutedEvent(true));
            _isGamePlaying = false;
        }

        public override void ClearBrowser()
        {
            // If the address is set to null, clear the browser
            if (_address == null)
            {
                Address = DummyUrl;
            }
        }

        /// <summary>
        ///     Set media player actual visibility using current states
        /// </summary>
        /// <param name="visible">Pass in default null to use Model.Visible or a value to override Model.Visible</param>
        public override void SetVisibility(bool? visible = null)
        {
            if (visible == false)
            {
                // We can set IsVisible to false immediately, but more checks are required for true in case states have changed
                IsVisible = false;
            }
            else
            {
                lock (DisplayLock)
                {
                    // Scaled viewers don't need to check game state before showing or hiding.
                    // If we have reached the appropriate state then we can show/hide the overlay window.
                    var state = _gameState.CurrentState;
                    IsVisible = Model.Visible && Model.Enabled &&
                                (Model.DisplayType == DisplayType.Scale || state == PlayState.Idle || state == PlayState.GameEnded);
                }
            }
        }

        public void SetMuted()
        {
            EventBus.Publish(new MediaPlayerSetAudioMutedEvent(!IsVisible || _isGamePlaying));
        }

        private void Handler(GamePlayStateChangedEvent e)
        {
            SetVisibility();

            if (e.CurrentState == PlayState.GameEnded)
            {
                _isGamePlaying = false;
            }
            else if (e.CurrentState == PlayState.Initiated)
            {
                _isGamePlaying = true;
            }

            SetMuted();
        }

        private void Handler(SetContentMediaPlayerEvent e)
        {
            if (e.MediaPlayer.Id == Model.Id)
            {
                // Assuming here that the ActiveMedia has been set before the SetContent event was fired
                Address = Model.ActiveMedia.Address.AbsoluteUri;
                SetMuted();
            }
        }

        private void Handler(ReleaseContentMediaPlayerEvent e)
        {
            if (e.MediaPlayer == null || e.Media == null) return;

            if (e.MediaPlayer.Id == Model.Id && (Model.ActiveMedia == null || Model.ActiveMedia.Id == e.Media?.Id))
            {
                // Set to null to hide the media display when the active content is released
                Address = null;
                SetMuted();
            }
        }

        private void Handler(ToggleMediaPlayerTestEvent obj)
        {
            // For testing, this behaves more or less like the SetContent/ReleaseContent events
            if (obj.PlayerId != Id) return;

            if (Address == null || Address == DummyUrl)
            {
                Address = DisplayPosition.IsMenu()
                    ? "http://aws.aristocrat.tech/pui/menu"
                    : DisplayPosition.IsBanner()
                        ? "http://aws.aristocrat.tech/pui/banner"
                        : "www.dailyoverview.com";
            }

            if (IsVisible)
            {
                ServiceManager.GetInstance().GetService<IMediaProvider>().Hide(Model.Id);
            }
            else
            {
                ServiceManager.GetInstance().GetService<IMediaProvider>().Show(Model.Id);
            }

            SetMuted();

            /* Other URLs for testing
             * http://24hoursofhappy.com/ for flash video
             * http://jkorpela.fi/forms/file.html#example for file dialog & text boxes
             * www.dailyoverview.com for popups/javascript
             */
        }

        private void Handler(MediaPlayerContentReadyEvent e)
        {
            if (e.MediaPlayerId == Id)
            {
                SetMuted();
            }
        }

        private void Handler(GamePlayDisabledEvent evt)
        {
            ServiceManager.GetInstance().GetService<IMediaProvider>().Hide(Model.Id);
        }

        protected override void OnVisChangeRequested(bool visibleState)
        {
            LatestVisibleState = visibleState;
        }

        protected override void SetLatestVisibility(bool? latestVisibleState)
        {
            if (latestVisibleState == null || !Model.Enabled) return;

            if (latestVisibleState == IsVisible)
            {
                Logger.Debug($"MediaPlayer {Id} SetLatestVisibility return -- IsVisible already set to LatestVisibleState ({IsVisible})");
                return;
            }

            if (IsAnimating)
            {
                Logger.Debug($"MediaPlayer {Id} SetLatestVisibility return -- IsAnimating=True");
                return;
            }

            if (latestVisibleState == true)
            {
                Logger.Debug($"MediaPlayer {Id} SetLatestVisibility to True");
                ServiceManager.GetInstance().GetService<IMediaProvider>().UpdatePlayer(Model, true);

                EventBus.Publish(new ShowMediaPlayerEvent(Model));
            }
            else
            {
                Logger.Debug($"MediaPlayer {Id} SetLatestVisibility to False");
                ServiceManager.GetInstance().GetService<IMediaProvider>().UpdatePlayer(Model, false);

                EventBus.Publish(new HideMediaPlayerEvent(Model));
            }

            SetVisibility();
            SetMuted();
            LatestVisibleState = null;
        }
    }
}
