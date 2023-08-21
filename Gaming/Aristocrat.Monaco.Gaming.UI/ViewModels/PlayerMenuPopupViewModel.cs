namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System;
    using System.Reflection;
    using System.Timers;
    using System.Windows.Input;
    using Application.Contracts;
    using Application.Contracts.Input;
    using Aristocrat.Monaco.Hardware.Contracts.Audio;
    using Common;
    using Contracts;
    using Contracts.Events;
    using Kernel;
    using log4net;
    using MVVM.Command;
    using MVVM.ViewModel;

    /// <summary>
    ///     Configurable states for the player menu popup display
    /// </summary>
    public enum PlayerMenuPopupBackground
    {
        /// <summary>
        ///     Show the full menu - all 3 sections
        /// </summary>
        FullMenu,
        /// <summary>
        ///     Show the session tracking section and the buttons
        /// </summary>
        SessionTrackingAndButtons,
        /// <summary>
        ///     Show the reserve section and the buttons
        /// </summary>
        ReserveMachineAndButtons,
        /// <summary>
        ///     Show only the button section
        /// </summary>
        ButtonsOnly
    }

    public class PlayerMenuPopupViewModel : BaseViewModel, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;
        private readonly IRuntimeFlagHandler _runtimeFlagHandler;
        private readonly IOnScreenKeyboardService _keyboardService;
        private readonly IAudio _audioService;

        private readonly string PasswordChar = "*";

        private bool _isMenuVisible;
        private bool _disposedValue;
        private PlayerMenuPopupBackground _menuBackgroundOption;
        private bool _isSessionTrackingSectionVisible;
        private bool _isVolumeButtonVisible;
        private bool _isReserveMachineSectionVisible;
        private bool _isReserveButtonEnabled;
        private string _trackingStartTime = string.Empty;
        private string _trackingGamesPlayed = string.Empty;
        private string _trackingAmountPlayed = string.Empty;
        private string _trackingAmountWon = string.Empty;
        private string _pin = string.Empty;
        private readonly string _touchSoundFile = string.Empty;

        private readonly Timer _closeDelayTimer = new Timer(GamingConstants.PlayerMenuPopupTimeoutMilliseconds);

        public ICommand ReserveDigitClickedCommand { get; }

        public ICommand ReserveClickedCommand { get; }

        public ICommand ReserveBackspaceClickedCommand { get; }

        public ICommand StartNewSessionClickedCommand { get; }

        public void SendButtonPressToExit() => _eventBus.Publish(new PlayerMenuButtonPressedEvent(false));

        public PlayerMenuPopupViewModel()
            : this(
                ServiceManager.GetInstance().TryGetService<IEventBus>(),
                ServiceManager.GetInstance().TryGetService<IPropertiesManager>(),
                ServiceManager.GetInstance().TryGetService<IContainerService>().Container.GetInstance<IRuntimeFlagHandler>(),
                ServiceManager.GetInstance().TryGetService<IOnScreenKeyboardService>(),
                ServiceManager.GetInstance().TryGetService<IAudio>())
        {
        }

        public PlayerMenuPopupViewModel(
            IEventBus eventBus,
            IPropertiesManager propertiesManager,
            IRuntimeFlagHandler runtimeFlagHandler,
            IOnScreenKeyboardService keyboardService,
            IAudio audioService)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _properties = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _runtimeFlagHandler = runtimeFlagHandler ?? throw new ArgumentException(nameof(runtimeFlagHandler));
            _keyboardService = keyboardService ?? throw new ArgumentException(nameof(keyboardService));
            _audioService = audioService ?? throw new ArgumentException(nameof(audioService));

            _eventBus.Subscribe<GamePlayStateChangedEvent>(this, eventArgs => Handler(eventArgs.CurrentState));
            _eventBus.Subscribe<PropertyChangedEvent>(this, eventArgs => SetVolumeControlVisible(), property => property.PropertyName == ApplicationConstants.VolumeControlLocationKey);
            _closeDelayTimer.Elapsed += (sender, args) => SendButtonPressToExit();
            _touchSoundFile = _properties.GetValue(ApplicationConstants.TouchSoundKey, string.Empty);

            ReserveDigitClickedCommand = new ActionCommand<string>(ConcatenateReservePin);
            ReserveClickedCommand = new ActionCommand<object>(StartMachineReservation);
            ReserveBackspaceClickedCommand = new ActionCommand<object>(BackspaceOnReservePin);
            StartNewSessionClickedCommand = new ActionCommand<object>(StartNewTrackingSession);

            IsMenuVisible = false;

            SetupMenu();
        }

        private void SetupMenu()
        {
            if (!_properties.GetValue(GamingConstants.ShowPlayerMenuPopup, true))
            {
                return;
            }

            // Session tracking implementation was postponed, so disabling its menu section here
            IsSessionTrackingSectionVisible = false;

            IsReserveMachineSectionVisible = (bool)_properties.GetProperty(
                ApplicationConstants.ReserveServiceEnabled,
                true);

            SetVolumeControlVisible();

            Pin = string.Empty;

            UpdateReserveButtonEnabled();

            if (IsSessionTrackingSectionVisible && IsReserveMachineSectionVisible)
            {
                MenuBackgroundOption = PlayerMenuPopupBackground.FullMenu;
            }
            else if (IsSessionTrackingSectionVisible && !IsReserveMachineSectionVisible)
            {
                MenuBackgroundOption = PlayerMenuPopupBackground.SessionTrackingAndButtons;
            }
            else if (!IsSessionTrackingSectionVisible && IsReserveMachineSectionVisible)
            {
                MenuBackgroundOption = PlayerMenuPopupBackground.ReserveMachineAndButtons;
            }
            else
            {
                MenuBackgroundOption = PlayerMenuPopupBackground.ButtonsOnly;
            }

            Logger.Debug($"Setting up menu type: {MenuBackgroundOption}");
        }

        private void Handler(PlayState playState)
        {
            // If the menu button is pressed moments before the play button is pressed,
            // force the menu to close right away
            if (playState == PlayState.Initiated && IsMenuVisible)
            {
                IsMenuVisible = false;
            }
        }

        /// <summary>
        ///     The current background image of the menu
        /// </summary>
        public PlayerMenuPopupBackground MenuBackgroundOption
        {
            get => _menuBackgroundOption;
            set => SetProperty(ref _menuBackgroundOption, value, nameof(MenuBackgroundOption));
        }

        /// <summary>
        ///     Controls the visibility of the session tracking panel
        /// </summary>
        public bool IsSessionTrackingSectionVisible
        {
            get => _isSessionTrackingSectionVisible;
            set => SetProperty(ref _isSessionTrackingSectionVisible, value, nameof(IsSessionTrackingSectionVisible));
        }

        /// <summary>
        ///     Controls the visibility of the reserve machine panel
        /// </summary>
        public bool IsReserveMachineSectionVisible
        {
            get => _isReserveMachineSectionVisible;
            set => SetProperty(ref _isReserveMachineSectionVisible, value, nameof(IsReserveMachineSectionVisible));
        }

        /// <summary>
        ///     Controls the visibility of the volume button
        /// </summary>
        public bool IsVolumeButtonVisible
        {
            get => _isVolumeButtonVisible;
            set => SetProperty(ref _isVolumeButtonVisible, value, nameof(IsVolumeButtonVisible));
        }

        /// <summary>
        ///     Controls the enabled state of the reserve button
        /// </summary>
        public bool ReserveButtonEnabled
        {
            get => _isReserveButtonEnabled;
            set => SetProperty(ref _isReserveButtonEnabled, value, nameof(ReserveButtonEnabled));
        }

        /// <summary>
        ///     The session tracking start time
        /// </summary>
        public string TrackingStartTime
        {
            get => _trackingStartTime;
            set => SetProperty(ref _trackingStartTime, value, nameof(TrackingStartTime));
        }

        /// <summary>
        ///     The session tracking games played count
        /// </summary>
        public string TrackingGamesPlayed
        {
            get => _trackingGamesPlayed;
            set => SetProperty(ref _trackingGamesPlayed, value, nameof(TrackingGamesPlayed));
        }

        /// <summary>
        ///     The session tracking amount played
        /// </summary>
        public string TrackingAmountPlayed
        {
            get => _trackingAmountPlayed;
            set => SetProperty(ref _trackingAmountPlayed, value, nameof(TrackingAmountPlayed));
        }

        /// <summary>
        ///     The session tracking amount won
        /// </summary>
        public string TrackingAmountWon
        {
            get => _trackingAmountWon;
            set => SetProperty(ref _trackingAmountWon, value, nameof(TrackingAmountWon));
        }

        /// <summary>
        ///     The reserve pin that will be persisted
        /// </summary>
        private string Pin
        {
            get => _pin;
            set => SetProperty(ref _pin, value, nameof(PinBoxContent));
        }

        /// <summary>
        ///     The *** reserve code string
        /// </summary>
        public string PinBoxContent => PasswordChar.Repeat(Pin.Length);

        private void ConcatenateReservePin(string digitPressed)
        {
            ResetCloseDelay();

            if (Pin.Length < GamingConstants.ReserveMachinePinLength)
            {
                Pin += digitPressed;
                UpdateReserveButtonEnabled();
            }
        }

        private void BackspaceOnReservePin(object obj)
        {
            ResetCloseDelay();

            if (Pin.Length > 0)
            {
                Pin = Pin.Substring(0, Pin.Length - 1);
                UpdateReserveButtonEnabled();
            }
        }

        private void UpdateReserveButtonEnabled()
        {
            ReserveButtonEnabled = Pin.Length == GamingConstants.ReserveMachinePinLength;
        }

        private void StartMachineReservation(object obj)
        {
            PlayClickSound();
            if (Pin.Length == GamingConstants.ReserveMachinePinLength)
            {
                var storedPin = (string)_properties.GetProperty(ApplicationConstants.ReserveServicePin, string.Empty);
                _properties.SetProperty(ApplicationConstants.ReserveServicePin, Pin);

                Logger.Debug($"Starting reserve machine. Pin changed from {storedPin} to {Pin}");

                IsMenuVisible = false;

                _eventBus.Publish(new ReserveButtonPressedEvent());
            }
        }

        private void StartNewTrackingSession(object obj)
        {
            Logger.Debug("Starting new tracking session");
            IsMenuVisible = false;
        }

        public bool IsMenuVisible
        {
            get => _isMenuVisible;
            set
            {
                if (!SetProperty(ref _isMenuVisible, value, nameof(IsMenuVisible)))
                {
                    return;
                }
                SetupMenu();

                _runtimeFlagHandler.SetInPlayerMenu(_isMenuVisible);
                _keyboardService.DisableKeyboard = _isMenuVisible;

                if (_isMenuVisible)
                {
                    Pin = string.Empty;
                    UpdateReserveButtonEnabled();
                    ResetCloseDelay();
                }
                else
                {
                    CancelCloseDelay();
                }
            }
        }

        /// <summary>
        ///     Cancels and starts the close delay timer
        /// </summary>
        public void ResetCloseDelay()
        {
            _eventBus.Publish(new UserInteractionEvent());

            CancelCloseDelay();
            SetCloseDelay();
        }

        /// <summary>
        ///     Cancels the close delay timer
        /// </summary>
        public void CancelCloseDelay()
        {
            if (_closeDelayTimer.Enabled)
            {
                _closeDelayTimer.Stop();
            }
        }

        private void SetCloseDelay()
        {
            if (!_closeDelayTimer.Enabled)
            {
                _closeDelayTimer.Start();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _closeDelayTimer.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void SetVolumeControlVisible()
        {
            var volumeControlLocation = (VolumeControlLocation)_properties.GetValue(
                ApplicationConstants.VolumeControlLocationKey,
                ApplicationConstants.VolumeControlLocationDefault);

            IsVolumeButtonVisible = volumeControlLocation == VolumeControlLocation.Game ||
                                    volumeControlLocation == VolumeControlLocation.LobbyAndGame;
        }

        public void PlayClickSound()
        {
            if (!string.IsNullOrWhiteSpace(_touchSoundFile))
            {
                var soundVolume = (byte)_properties.GetProperty(ApplicationConstants.PlayerVolumeScalarKey, ApplicationConstants.DefaultVolumeLevel);
                _audioService.Play(_touchSoundFile, soundVolume);
            }
        }
    }
}