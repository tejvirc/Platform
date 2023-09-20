namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Windows.Input;
    using Application.Contracts;
    using Application.Contracts.Input;
    using Application.Contracts.OperatorMenu;
    using Contracts;
    using Kernel;
    using log4net;
    using Aristocrat.Monaco.Hardware.Contracts.Audio;
    using CommunityToolkit.Mvvm.Input;
    using CommunityToolkit.Mvvm.ComponentModel;
    using Aristocrat.Extensions.CommunityToolkit;

    /// <summary>
    ///     Reserve machine GUI states
    /// </summary>
    public enum ReserveMachineDisplayState
    {
        /// <summary>
        ///     Confirming the pin entered in the player menu
        /// </summary>
        Confirm = 0,
        /// <summary>
        ///     The machine is locked and the countdown timer is on the screen
        /// </summary>
        Countdown,
        /// <summary>
        ///     Enter the saved pin to unlock the reserved machine
        /// </summary>
        Exit,
        /// <summary>
        ///     Too many incorrect pins were entered, a message for this will be displayed
        /// </summary>
        IncorrectPin,
        /// <summary>
        ///     Idle state with no reserve displays
        /// </summary>
        None
    }

    /// <summary>
    ///     Class to store data for the Message Overlay
    /// </summary>
    public class ReserveOverlayViewModel : ObservableObject, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPropertiesManager _propertiesManager;
        private readonly IReserveService _reserveService;
        private readonly ISystemDisableManager _disableManager;
        private readonly IEventBus _eventBus;
        private readonly IOnScreenKeyboardService _keyboardService;
        private readonly IAudio _audioService;

        private const string TimeFormat = "m\\:ss";
        private const int DefaultTimeOutInMinutes = 5;
        private readonly string PasswordChar = "*";
        private readonly string _touchSoundFile;

        private int FullLockupTimeSeconds => _propertiesManager.GetValue(
            ApplicationConstants.ReserveServiceTimeoutInSeconds,
            (int)TimeSpan.FromMinutes(DefaultTimeOutInMinutes).TotalSeconds);

        private bool _disposed;
        private ReserveMachineDisplayState _state;

        /// <summary>
        ///     Timespan the keeps track of the countdown time being displayed
        /// </summary>
        private TimeSpan _countDownTime;

        /// <summary>
        ///     This timer is for when there are too many incorrect PIN entry attempts,
        ///     the player can try again after this timer expires
        /// </summary>
        private Timer _incorrectPinWaitTimer;
        private TimeSpan _incorrectPinWaitTimeSpan = TimeSpan.FromMinutes(1);

        /// <summary>
        ///     This timer controls how long the pin entry display remains on screen. It gets reset
        ///     when a button is pressed. For the PIN confirm display, the timer elapsing will
        ///     cancel the reserve request. For the PIN unlock display, the timer elapsing will
        ///     return to the countdown display.
        /// </summary>
        private readonly System.Timers.Timer _pinEntryCloseTimer =
            new System.Timers.Timer(GamingConstants.ReserveMachinePinDisplayTimeoutMilliseconds);

        public ICommand BackspaceButtonClickedCommand { get; }

        public ICommand ReserveButtonClickedCommand { get; }

        public ICommand CancelButtonClickedCommand { get; }

        public ICommand UnlockButtonClickedCommand { get; }

        public ICommand DigitClickedCommand { get; }

        public ICommand ExitReserveButtonClickedCommand { get; }

        public ReserveOverlayViewModel()
            :
            this(
                ServiceManager.GetInstance().TryGetService<IEventBus>(),
                ServiceManager.GetInstance().TryGetService<IPropertiesManager>(),
                ServiceManager.GetInstance().TryGetService<IContainerService>().Container.GetInstance<IReserveService>(),
                ServiceManager.GetInstance().TryGetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().TryGetService<IOnScreenKeyboardService>(),
                ServiceManager.GetInstance().TryGetService<IAudio>())
        {
        }

        public ReserveOverlayViewModel(
            IEventBus eventBus,
            IPropertiesManager propertiesManager,
            IReserveService reserveService,
            ISystemDisableManager disableManager,
            IOnScreenKeyboardService keyboardService,
            IAudio audioService)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _reserveService = reserveService ?? throw new ArgumentNullException(nameof(reserveService));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _keyboardService = keyboardService ?? throw new ArgumentNullException(nameof(keyboardService));
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            _touchSoundFile = _propertiesManager.GetValue(ApplicationConstants.TouchSoundKey, string.Empty);

            DigitClickedCommand = new RelayCommand<object>(ConcatenateReservePin);

            BackspaceButtonClickedCommand = new RelayCommand<object>(_ => BackspaceButtonPressed());

            ReserveButtonClickedCommand = new RelayCommand<object>(_ => ReserveTheMachine());

            CancelButtonClickedCommand = new RelayCommand<object>(_ => CancelButtonPressed());

            UnlockButtonClickedCommand = new RelayCommand<object>(_ => UnlockReserve());

            ExitReserveButtonClickedCommand = new RelayCommand<object>(_ => ExitReserveButtonPressed());

            _incorrectPinWaitTimer = new Timer(
                IncorrectPinWaitTimerCallback,
                null,
                Timeout.InfiniteTimeSpan,
                Timeout.InfiniteTimeSpan);

            _pinEntryCloseTimer.Elapsed += PinEntryCloseTimerCallback;

            _eventBus.Subscribe<PropertyChangedEvent>(this, HandleEvent);
            _eventBus.Subscribe<OperatorMenuEnteredEvent>(this, evt => CancelButtonPressed());

            ResetFields();

            SetCountdownText();

            var lockupPresent = (bool)_propertiesManager.GetProperty(ApplicationConstants.ReserveServiceLockupPresent, false);
            State = lockupPresent ? ReserveMachineDisplayState.Countdown : ReserveMachineDisplayState.None;

            IsDialogVisible = false;
        }

        public string TimeLengthMachineWillBeReserved => TimeSpan.FromSeconds(
            FullLockupTimeSeconds).TotalMinutes.ToString(CultureInfo.CurrentUICulture);

        public string CountdownTimerText => _countDownTime.ToString(TimeFormat);

        public string IncorrectPinWaitTimeLeft => _incorrectPinWaitTimeSpan.ToString(TimeFormat);

        private void SetCountdownText()
        {
            var remainingSeconds = (int)_propertiesManager.GetProperty(ApplicationConstants.ReserveServiceLockupRemainingSeconds, 0);

            _countDownTime = remainingSeconds == 0
                ? TimeSpan.FromSeconds(FullLockupTimeSeconds)
                : TimeSpan.FromSeconds(remainingSeconds);

            OnPropertyChanged(nameof(CountdownTimerText));
        }

        private void ResetFields()
        {
            Pin = string.Empty;
            PinBoxContent = string.Empty;
            UpdateConfirmButtonEnabled();
        }

        /// <summary>
        ///     The state of the reserve GUI
        /// </summary>
        public ReserveMachineDisplayState State
        {
            get => _state;
            set
            {
                if (!SetProperty(ref _state, value, nameof(State)))
                {
                    return;
                }

                Logger.Debug($"Reserve State: {_state}");

                ResetFields();
                StopPinEntryTimer();
                ShowIncorrectPinWarning = false;

                switch (_state)
                {
                    case ReserveMachineDisplayState.Confirm:
                        ShowPinEntryPanel = true;
                        ShowCountDownTimer = false;
                        ShowLockupBackground = false;
                        ShowIncorrectUnlockPinDisplay = false;
                        RestartPinEntryCloseTimer();
                        ShowPreReserveLockup();
                        break;
                    case ReserveMachineDisplayState.Countdown:
                        ShowPinEntryPanel = false;
                        ShowCountDownTimer = true;
                        ShowLockupBackground = true;
                        ShowIncorrectUnlockPinDisplay = false;
                        SetCountdownText();
                        break;
                    case ReserveMachineDisplayState.Exit:
                        ShowPinEntryPanel = true;
                        ShowCountDownTimer = false;
                        ShowLockupBackground = true;
                        ShowIncorrectUnlockPinDisplay = false;
                        RestartPinEntryCloseTimer();
                        break;
                    case ReserveMachineDisplayState.IncorrectPin:
                        ShowPinEntryPanel = false;
                        ShowCountDownTimer = false;
                        ShowLockupBackground = true;
                        ShowIncorrectUnlockPinDisplay = true;
                        break;
                    case ReserveMachineDisplayState.None:
                        ShowPinEntryPanel = false;
                        ShowCountDownTimer = false;
                        ShowLockupBackground = false;
                        ShowIncorrectUnlockPinDisplay = false;
                        CancelButtonPressed();
                        break;
                }
            }
        }

        private bool _showLockupBackground;

        /// <summary>
        ///     Controls the showing of the solid color background that covers the entire screen
        /// </summary>
        public bool ShowLockupBackground
        {
            get => _showLockupBackground;
            private set => SetProperty(ref _showLockupBackground, value, nameof(ShowLockupBackground));
        }

        private bool _showIncorrectUnlockPinDisplay;

        /// <summary>
        ///     Controls the showing of the display that tells the player they've entered the
        ///     reserve unlock pin wrong too many times and must wait 1 minute
        /// </summary>
        public bool ShowIncorrectUnlockPinDisplay
        {
            get => _showIncorrectUnlockPinDisplay;
            private set => SetProperty(ref _showIncorrectUnlockPinDisplay, value, nameof(ShowIncorrectUnlockPinDisplay));
        }

        private bool _showIncorrectPinWarning;

        /// <summary>
        ///     Controls the showing of the small incorrect PIN warning message below the entry box
        /// </summary>
        public bool ShowIncorrectPinWarning
        {
            get => _showIncorrectPinWarning;
            private set => SetProperty(ref _showIncorrectPinWarning, value, nameof(ShowIncorrectPinWarning));
        }

        private bool _showCountdownTimer;

        /// <summary>
        ///     Controls the showing of the countdown timer
        /// </summary>
        public bool ShowCountDownTimer
        {
            get => _showCountdownTimer;
            private set => SetProperty(ref _showCountdownTimer, value, nameof(ShowCountDownTimer));
        }

        private string _pinBoxContent = string.Empty;

        /// <summary>
        ///     The *** reserve code string
        /// </summary>
        public string PinBoxContent
        {
            get => _pinBoxContent;
            set => SetProperty(ref _pinBoxContent, value, nameof(PinBoxContent));
        }

        private string _pin = string.Empty;

        /// <summary>
        ///     The Pin that is being entered either in the Confirm or Exit state
        /// </summary>
        public string Pin
        {
            get => _pin;
            private set
            {
                if (value.Length > GamingConstants.ReserveMachinePinLength)
                {
                    return;
                }

                SetProperty(ref _pin, value, nameof(Pin));
            }
        }

        private bool _showPinEntryPanel;

        /// <summary>
        ///     Controls the showing or hiding of the pin entry panel
        /// </summary>
        public bool ShowPinEntryPanel
        {
            get => _showPinEntryPanel;
            set => SetProperty(ref _showPinEntryPanel, value, nameof(ShowPinEntryPanel));
        }

        private bool _isDialogVisible;

        /// <summary>
        ///     The entry point to showing the reserve overlay
        /// </summary>
        public bool IsDialogVisible
        {
            get => _isDialogVisible;

            set
            {
                if (!SetProperty(ref _isDialogVisible, value, nameof(IsDialogVisible)))
                {
                    return;
                }

                // Is the machine already reserved and we need to let them enter their pin to unlock the machine
                ShowLockupBackground = (bool)_propertiesManager.GetProperty(ApplicationConstants.ReserveServiceLockupPresent, false);
                State = ShowLockupBackground
                    ? ReserveMachineDisplayState.Countdown
                    : _isDialogVisible
                        ? ReserveMachineDisplayState.Confirm
                        : ReserveMachineDisplayState.None;

                _keyboardService.DisableKeyboard = _isDialogVisible;
                if (!_isDialogVisible)
                {
                    IsDialogFadingOut = true;
                }
            }
        }

        private int _pinEntryAttempts;

        /// <summary>
        ///     Counts the number of times the wrong pin was entered
        /// </summary>
        public int PinEntryAttempts
        {
            get => _pinEntryAttempts;
            set => SetProperty(ref _pinEntryAttempts, value, nameof(PinEntryAttempts));
        }

        private bool _isDialogFadingOut;

        public bool IsDialogFadingOut
        {
            get => _isDialogFadingOut;
            set => SetProperty(ref _isDialogFadingOut, value, nameof(IsDialogFadingOut));
        }

        private bool _isConfirmButtonEnabled;

        /// <summary>
        ///     Controls the enabled state of the Reserve and Unlock buttons
        /// </summary>
        public bool ConfirmButtonEnabled
        {
            get => _isConfirmButtonEnabled;
            set => SetProperty(ref _isConfirmButtonEnabled, value, nameof(ConfirmButtonEnabled));
        }

        private void UpdateConfirmButtonEnabled()
        {
            ConfirmButtonEnabled = _pin.Length == GamingConstants.ReserveMachinePinLength;
        }

        private void IncorrectPinWaitTimerCallback(object state)
        {
            if (_incorrectPinWaitTimeSpan > TimeSpan.Zero)
            {
                OnPropertyChanged(nameof(IncorrectPinWaitTimeLeft));
                _incorrectPinWaitTimeSpan = _incorrectPinWaitTimeSpan.Subtract(TimeSpan.FromSeconds(1));
                _incorrectPinWaitTimer.Change(TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);
            }
            else
            {
                PinEntryAttempts = 0;
                State = ReserveMachineDisplayState.Exit;
                _incorrectPinWaitTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            }
        }

        private void PinEntryCloseTimerCallback(object sender, System.Timers.ElapsedEventArgs args)
        {
            if (State == ReserveMachineDisplayState.Confirm)
            {
                CancelButtonPressed();
            }
            else if (State == ReserveMachineDisplayState.Exit)
            {
                State = ReserveMachineDisplayState.Countdown;
            }
        }

        private void StopPinEntryTimer()
        {
            PinEntryAttempts = 0;

            if (_pinEntryCloseTimer.Enabled)
            {
                _pinEntryCloseTimer.Stop();
            }
            _incorrectPinWaitTimeSpan = TimeSpan.FromSeconds(GamingConstants.ReserveMachineIncorrectPinWaitTimeSeconds);
            _incorrectPinWaitTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        private void RestartPinEntryCloseTimer()
        {
            if (_pinEntryCloseTimer.Enabled)
            {
                _pinEntryCloseTimer.Stop();
            }

            _pinEntryCloseTimer.Start();
        }

        private void HandleEvent(PropertyChangedEvent evt)
        {
            Execute.OnUIThread(() =>
            {
                switch (evt.PropertyName)
                {
                    case ApplicationConstants.ReserveServiceLockupPresent:
                        var lockup = (bool)_propertiesManager.GetProperty(ApplicationConstants.ReserveServiceLockupPresent, false);

                        State = lockup ? ReserveMachineDisplayState.Countdown : ReserveMachineDisplayState.None;

                        // If there's no lockup then we don't need to display anything
                        if (!lockup)
                        {
                            IsDialogVisible = false;
                        }
                        break;

                    case ApplicationConstants.ReserveServiceTimeoutInSeconds:
                        OnPropertyChanged(nameof(TimeLengthMachineWillBeReserved));
                        break;

                    case ApplicationConstants.ReserveServiceLockupRemainingSeconds:
                        SetCountdownText();
                        break;
                }
            });
        }

        /// <summary>
        ///     When the exit button (below the countdown timer) is pressed, keep showing the solid background,
        ///     stop showing the countdown timer, and show the pin entry panel
        /// </summary>
        private void ExitReserveButtonPressed()
        {
            PlayButtonClickSound();
            Execute.OnUIThread(() => State = ReserveMachineDisplayState.Exit);
        }

        /// <summary>
        ///     When the cancel button is pressed, change the state back to Countdown if previously in the
        ///     Exit state, or cancel the reserve altogether if the state was Confirm
        /// </summary>
        private void CancelButtonPressed()
        {
            if (State == ReserveMachineDisplayState.Confirm)
            {
                PlayButtonClickSound();
            }

            if (State == ReserveMachineDisplayState.Confirm || State == ReserveMachineDisplayState.None)
            {
                IsDialogVisible = false;
                _propertiesManager.SetProperty(ApplicationConstants.ReserveServicePin, string.Empty);
                RemovePreReserveLockup();
            }
            else if (State == ReserveMachineDisplayState.Exit)
            {
                PlayButtonClickSound();
                Execute.OnUIThread(() => State = ReserveMachineDisplayState.Countdown);
            }
        }

        private void BackspaceButtonPressed()
        {
            PlayButtonClickSound();
            if (Pin.Length > 0)
            {
                Execute.OnUIThread(() =>
                {
                    Pin = Pin.Substring(0, Pin.Length - 1);
                    PinBoxContent = PinBoxContent.Substring(0, PinBoxContent.Length - 1);
                    UpdateConfirmButtonEnabled();
                    RestartPinEntryCloseTimer();
                });
            }
        }

        private void ConcatenateReservePin(object obj)
        {
            PlayButtonClickSound();
            if (Pin.Length < GamingConstants.ReserveMachinePinLength)
            {
                Execute.OnUIThread(() =>
                {
                    Pin += (string)obj;
                    PinBoxContent += PasswordChar;
                    UpdateConfirmButtonEnabled();
                    ShowIncorrectPinWarning = false;
                    RestartPinEntryCloseTimer();
                });
            }
        }

        private void ReserveTheMachine()
        {
            PlayButtonClickSound();
            var storedPin = (string)_propertiesManager.GetProperty(ApplicationConstants.ReserveServicePin, string.Empty);

            Execute.OnUIThread(() =>
            {
                // The pin was confirmed, reserve the machine
                if (string.Compare(Pin, storedPin, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    _reserveService.ReserveMachine();
                    State = ReserveMachineDisplayState.Countdown;
                    RemovePreReserveLockup();
                }
                // The pin does not match what was entered on the player menu
                else
                {
                    ShowIncorrectPinWarning = true;
                }

                ResetFields();
            });
        }

        private void UnlockReserve()
        {
            var storedPin = (string)_propertiesManager.GetProperty(ApplicationConstants.ReserveServicePin, string.Empty);
            PlayButtonClickSound();
            Execute.OnUIThread(() =>
            {
                // The entered pin matches the saved pin, unlock the machine and exit reserve
                if (string.Compare(storedPin, Pin, StringComparison.CurrentCultureIgnoreCase) == 0)
                {
                    _reserveService?.ExitReserveMachine();
                    StopPinEntryTimer();
                    _propertiesManager.SetProperty(ApplicationConstants.ReserveServicePin, string.Empty);
                    IsDialogVisible = false;
                    RemovePreReserveLockup();
                }
                // The entered pin does not match, up the retry count
                else
                {
                    if (++PinEntryAttempts < GamingConstants.ReserveMachineMaxPinEntryAttempts)
                    {
                        ShowIncorrectPinWarning = true;
                    }
                    else
                    {
                        State = ReserveMachineDisplayState.IncorrectPin;

                        // Start the incorrect PIN attempts timer of 1 minute
                        _incorrectPinWaitTimeSpan = TimeSpan.FromSeconds(GamingConstants.ReserveMachineIncorrectPinWaitTimeSeconds);
                        _incorrectPinWaitTimer.Change(TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);
                        OnPropertyChanged(nameof(IncorrectPinWaitTimeLeft));
                    }
                }

                ResetFields();
            });
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

                if (_incorrectPinWaitTimer != null)
                {
                    _incorrectPinWaitTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                    _incorrectPinWaitTimer.Dispose();
                    _incorrectPinWaitTimer = null;
                }

                _pinEntryCloseTimer.Dispose();
            }

            _disposed = true;
        }

        private void ShowPreReserveLockup()
        {
            // Don't display any message, just lockup so that the player can't change bet levels, cashout, or start
            // start a game while confirming the pin
            _disableManager.Disable(
                ApplicationConstants.WaitingForInputDisableKey,
                SystemDisablePriority.Normal,
                () => string.Empty);
        }

        private void RemovePreReserveLockup()
        {
            if (_disableManager.CurrentDisableKeys.Contains(ApplicationConstants.WaitingForInputDisableKey))
            {
                _disableManager.Enable(ApplicationConstants.WaitingForInputDisableKey);
            }
        }

        private void PlayButtonClickSound()
        {
            if (!string.IsNullOrWhiteSpace(_touchSoundFile))
            {
                var soundVolume = (byte)_propertiesManager.GetProperty(ApplicationConstants.PlayerVolumeScalarKey, ApplicationConstants.DefaultVolumeLevel);
                _audioService.Play(_touchSoundFile, soundVolume);
            }
        }
    }
}

