namespace Aristocrat.Monaco.Gaming.VideoLottery
{
    using Common;
    using Contracts;
    using Contracts.Lobby;
    using Kernel;
    using log4net;
    using Monaco.UI.Common;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Implements the IResponsibleGaming interface.
    /// </summary>
    public class ResponsibleGaming : IResponsibleGaming
    {
        private const int TimerLengthInSeconds = 60;
        private const int TimerAlcDialogDismissalInSeconds = 60;
        private const int TimerPlayBreakDismissalInSeconds = 5;
        private const int SpinGuardLengthInMilliseconds = 250;
        private const string AlcDialogResourceKey = "AlcTimeLimit{0}Prompt";
        private const string StartDialogResourceKey = "TimeLimitInitialPrompt";
        private const string TimeOutDialogResourceKey = "TimeLimitExpiredPrompt";
        private const string FinalDialogResourceKey = "TimeLimitFinalPrompt";
        private const string PlayBreak1ResourceKey = "TimeLimitPlayBreak1";
        private const string PlayBreak2ResourceKey = "TimeLimitPlayBreak2";
        private const string Expired15ResourceKey = "TimeLimitExpiredPrompt15";
        private const string Expired30ResourceKey = "TimeLimitExpiredPrompt30";
        private const string Expired45ResourceKey = "TimeLimitExpiredPrompt45";
        private const string Expired60ResourceKey = "TimeLimitExpiredPrompt60";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly object _eventLock = new object();
        private readonly IGameRecovery _gameRecovery;
        private readonly IGamePlayState _gameState;

        private readonly IPropertiesManager _properties;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly object _timerLock = new object();
        private readonly object _stateLock = new object();
        private readonly object _spinGuardLock = new object();

        private LobbyConfiguration _config;
        private bool _dialogResetDueToOperatorMenu;
        private DateTime? _dialogShownTime;
        private bool _disabledOnStartup;
        private bool _disposed;
        private bool _isTimeLimitDlgVisible;
        private DateTime _lastTimerTick = DateTime.MinValue;

        private ITimer _playLimitTimer;
        private CancellationTokenSource _spinGuardCancellationSource;
        private bool _responsibleGamingEnabled;
        private int _sessionCount;
        private TimeSpan _sessionElapsedTime;
        private TimeSpan _sessionLength = TimeSpan.Zero;
        private bool _sessionPlayBreakHit;
        private TimeSpan _sessionPlayBreakTime = TimeSpan.Zero;
        private bool _showTimeLimitDlgPending;
        private ResponsibleGamingSessionState _state = ResponsibleGamingSessionState.Stopped;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResponsibleGaming" /> class.
        /// </summary>
        /// <param name="propertiesManager">The IPropertiesManager.</param>
        /// <param name="gameRecovery"> The IGameRecovery.</param>
        /// <param name="systemDisableManager">The ISystemDisableManager</param>
        /// <param name="gamePlayState">The IGamePlayState</param>
        public ResponsibleGaming(
            IPropertiesManager propertiesManager,
            IGameRecovery gameRecovery,
            ISystemDisableManager systemDisableManager,
            IGamePlayState gamePlayState)
        {
            _properties = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _gameRecovery = gameRecovery ?? throw new ArgumentNullException(nameof(gameRecovery));
            _systemDisableManager = systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _gameState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            Logger.Debug("ResponsibleGaming constructed.");
        }

        public bool IsLobbyLoadingGame { get; set; }

        private TimeSpan SessionPlayBreakTime
        {
            get => _sessionPlayBreakTime;
            set
            {
                if (_sessionPlayBreakTime != value)
                {
                    _sessionPlayBreakTime = value;
                    SaveSessionPlayBreakTime();
                }
            }
        }

        private bool SessionPlayBreakHit
        {
            get => _sessionPlayBreakHit;
            set
            {
                if (_sessionPlayBreakHit != value)
                {
                    _sessionPlayBreakHit = value;
                    SaveSessionPlayBreakHit();
                }
            }
        }

        public event EventHandler ForceCashOut;

        /// <summary>
        ///     Raised when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc />
        public int SessionLimit { get; private set; }

        /// <inheritdoc />
        public bool HasSessionLimits => SessionLimit > 0;

        /// <inheritdoc />
        public int SessionCount
        {
            get => _sessionCount;

            private set
            {
                if (_sessionCount != value)
                {
                    if (SessionLimit > 0 && value > SessionLimit)
                    {
                        value = SessionLimit;
                    }

                    if (value < 0)
                    {
                        value = 0;
                    }

                    if (_sessionCount == value)
                    {
                        return;
                    }

                    _sessionCount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SessionCount)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSessionLimitHit)));
                    _properties.SetProperty(LobbyConstants.LobbyPlayTimeSessionCount, _sessionCount);
                }
            }
        }

        /// <inheritdoc />
        public bool IsTimeLimitDialogVisible
        {
            get => _isTimeLimitDlgVisible;

            private set
            {
                if (_isTimeLimitDlgVisible != value)
                {
                    _isTimeLimitDlgVisible = value;

                    Logger.Debug($"IsTimeLimitDialogVisible changed to {value}.");

                    // Need to persist this.
                    _properties.SetProperty(LobbyConstants.LobbyIsTimeLimitDlgVisible, value);

                    if (value)
                    {
                        // Update TimeLimitDialogState property when the dialog is about to become visible
                        TimeLimitDialogState = FindTimeLimitDialogState();

                        PropertyChanged?.Invoke(
                            this,
                            new PropertyChangedEventArgs(nameof(ResponsibleGamingDialogResourceKey)));
                    }

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsTimeLimitDialogVisible)));

                    if (!value)
                    {
                        _dialogShownTime = null;
                    }
                }
            }
        }

        public bool IsSessionLimitHit
        {
            get
            {
                if (!HasSessionLimits)
                    return false;

                bool isSessionLimitHit = SessionCount >= SessionLimit;

                if (ResponsibleGamingMode == ResponsibleGamingMode.Segmented)
                {
                    // we don't increment SessionCount until AFTER dialog is dismissed
                    // so until SessionCount == SessionLimit for entire session.  Have to check time too.
                    isSessionLimitHit &= _sessionElapsedTime >= _sessionLength;
                }

                return isSessionLimitHit;
            }
        }

        /// <summary>
        ///     Public property indicating that Responsible Gaming
        ///     wants to show a dialog
        /// </summary>
        public bool ShowTimeLimitDlgPending
        {
            get => _showTimeLimitDlgPending;

            private set
            {
                if (_showTimeLimitDlgPending != value)
                {
                    _showTimeLimitDlgPending = value;

                    Logger.Debug($"ShowTimeLimitDlgPending changed to {value}.");

                    // Need to persist this.
                    _properties.SetProperty(LobbyConstants.LobbyShowTimeLimitDlgPending, value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowTimeLimitDlgPending)));
                }
            }
        }

        public ResponsibleGamingDialogState TimeLimitDialogState { get; private set; }

        public string ResponsibleGamingDialogResourceKey
        {
            get
            {
                //NOTE:  we reset the session state right before displaying the dialog
                string resourceKey;
                if (ResponsibleGamingMode == ResponsibleGamingMode.Continuous)
                {
                    var time = 60;
                    var sessionCount = SessionCount - 1;
                    if (sessionCount < 0)
                    {
                        sessionCount = 0;
                    }
                    else if (sessionCount >= SessionLimit)
                    {
                        sessionCount = SessionLimit - 1;
                    }

                    switch (sessionCount)
                    {
                        case 0:
                            time = 60;
                            break;
                        case 1:
                            time = 90;
                            break;
                        case 2:
                            time = 120;
                            break;
                        case 3:
                            time = 145;
                            break;
                        case 4:
                            time = 150;
                            break;
                    }

                    resourceKey = string.Format(AlcDialogResourceKey, time);
                }
                else //Standard
                {
                    switch (TimeLimitDialogState)
                    {
                        case ResponsibleGamingDialogState.Initial:
                            resourceKey = StartDialogResourceKey;
                            break;
                        case ResponsibleGamingDialogState.ForceCashOut:
                            resourceKey = FinalDialogResourceKey;
                            break;
                        case ResponsibleGamingDialogState.PlayBreak1:
                            resourceKey = PlayBreak1ResourceKey;
                            break;
                        case ResponsibleGamingDialogState.PlayBreak2:
                            resourceKey = PlayBreak2ResourceKey;
                            break;
                        case ResponsibleGamingDialogState.ChooseTime:
                            if (_sessionLength.TotalMinutes <= 15)
                            {
                                resourceKey = Expired15ResourceKey;
                            }
                            else if (_sessionLength.TotalMinutes <= 30)
                            {
                                resourceKey = Expired30ResourceKey;
                            }
                            else if (_sessionLength.TotalMinutes <= 45)
                            {
                                resourceKey = Expired45ResourceKey;
                            }
                            else if (_sessionLength.TotalMinutes <= 60)
                            {
                                resourceKey = Expired60ResourceKey;
                            }
                            else
                            {
                                resourceKey = TimeOutDialogResourceKey;
                            }
                            break;
                        default:
                            resourceKey = TimeOutDialogResourceKey;
                            break;
                    }
                }

                return resourceKey;
            }
        }

        public ResponsibleGamingMode ResponsibleGamingMode { get; private set; }

        public TimeSpan RemainingSessionTime
        {
            get
            {
                var time = _sessionLength - _sessionElapsedTime;
                if (_state == ResponsibleGamingSessionState.Started && _lastTimerTick != DateTime.MinValue)
                {
                    // Responsible Gaming only ticks once a minute.
                    // Time has passed since the last tick.  Factor it in.
                    time -= DateTime.UtcNow - _lastTimerTick;
                }

                if (time < TimeSpan.Zero)
                {
                    time = TimeSpan.Zero;
                }

                return time;
            }
        }

        public bool SpinGuard { get; private set; }

        public bool Enabled => _responsibleGamingEnabled;

        /// <inheritdoc />
        public void Initialize()
        {
            _config = (LobbyConfiguration)_properties.GetProperty(GamingConstants.LobbyConfig, null);
            ResponsibleGamingMode = _config.ResponsibleGamingMode;
            _responsibleGamingEnabled = _config.ResponsibleGamingTimeLimitEnabled;
            if (_responsibleGamingEnabled)
            {
                _playLimitTimer = new DispatcherTimerAdapter();
                _playLimitTimer.Tick += PlayLimitTimerTick;

                _sessionElapsedTime = TimeSpan.Zero;

                // Set the property now so that the test tool can override it while running to shorten test times
                _properties.SetProperty(
                    LobbyConstants.RGTimeLimitsInMinutes,
                    NormalizeTimeLimits(_config.ResponsibleGamingTimeLimits));
                _properties.SetProperty(
                    LobbyConstants.RGPlayBreaksInMinutes,
                    NormalizeTimeLimits(_config.ResponsibleGamingPlayBreaks));

                var timeout = ResponsibleGamingMode == ResponsibleGamingMode.Continuous
                    ? TimerAlcDialogDismissalInSeconds
                    : TimerPlayBreakDismissalInSeconds;
                _properties.SetProperty(LobbyConstants.LobbyPlayTimeDialogTimeoutInSeconds, timeout);
            }

            SessionLimit = _config.ResponsibleGamingSessionLimit;
            TimeLimitDialogState = ResponsibleGamingDialogState.Initial;

            Logger.Debug("ResponsibleGaming Initialized.");
        }

        public void ShowDialog(bool allowDialogWhileDisabled = false)
        {
            UpdateElapsedTimeFromOverride();

            if (_gameState.Idle && (!_systemDisableManager.IsDisabled || allowDialogWhileDisabled))
            {
                lock (_timerLock)
                {
                    if (!_systemDisableManager.IsDisabled && !_dialogResetDueToOperatorMenu)
                    {
                        // VLT-4453: We need to add in elapsed time before showing the dialog for ALC mode
                        UpdateElapsedTimeSinceTick();
                    }

                    if (ResponsibleGamingMode == ResponsibleGamingMode.Continuous)
                    {
                        //Determine Dialog to show based on elapsed time
                        SetSessionCountBasedOnElapsedTime();
                    }

                    IsTimeLimitDialogVisible = true;
                    ShowTimeLimitDlgPending = false;

                    if (UpdateDialogShownTime)
                    {
                        //ALC makes us dismiss the dialog in 1 minute if the user doesn't do anything
                        _dialogShownTime = DateTime.UtcNow;

                        // VLT-4474:  If we are re-displaying the RG dialog while disabled or due to the Operator Menu coming up, then don't start the timer.
                        // it will be started when we come out of lockup.
                        if (!_systemDisableManager.IsDisabled && !_dialogResetDueToOperatorMenu)
                        {
                            StartPlayTimer();
                        }
                    }
                }

                SaveSessionElapsedTime();

                //See comments in VLT-4474:  If we exit Operator Menu with a lockup, we don't want to reset this flag
                if (!_systemDisableManager.IsDisabled)
                {
                    _dialogResetDueToOperatorMenu = false;
                }
            }
        }

        /// <inheritdoc />
        public void LoadPropertiesFromPersistentStorage()
        {
            //if a dialog was showing when we shut down, we want to show it when we come back, but not until after we have recovered.
            //if a dialog is PENDING, then we want to treat it like a normal pending event.
            // VLT-4530:  Adding IsRecovering to _disabledOnStartup so that we don't tick time if we are recovering on boot
            _disabledOnStartup = _systemDisableManager.IsDisabled || _gameRecovery.IsRecovering;
            var isTimeLimitDlgVisible = _properties.GetValue(LobbyConstants.LobbyIsTimeLimitDlgVisible, false);
            var showTimeLimitDlgPending = _properties.GetValue(LobbyConstants.LobbyShowTimeLimitDlgPending, false);
            var playTimeSessionCount = _properties.GetValue(LobbyConstants.LobbyPlayTimeSessionCount, -1);
            var elapsedTimeInSeconds = _properties.GetValue(LobbyConstants.LobbyPlayTimeElapsedInSeconds, 0);
            _sessionElapsedTime = TimeSpan.FromSeconds(elapsedTimeInSeconds);
            var playBreakTimeInSeconds = _properties.GetValue(LobbyConstants.ResponsibleGamingPlayBreakTimeoutInSeconds, 0);

            if (playBreakTimeInSeconds > 0)
            {
                _sessionPlayBreakTime = TimeSpan.FromSeconds(playBreakTimeInSeconds);
                _sessionPlayBreakHit = _properties.GetValue(LobbyConstants.ResponsibleGamingPlayBreakHit, false);
            }

            Logger.Debug(
                $"Restarting Responsible Gaming with Elapsed Time: {_sessionElapsedTime.TotalMinutes} Minutes.");

            if (ResponsibleGamingMode == ResponsibleGamingMode.Continuous)
            {
                SetSessionCountBasedOnElapsedTime();
            }
            else if (playTimeSessionCount > 0)
            {
                SessionCount = playTimeSessionCount;
            }

            Logger.Debug($"Restarting Responsible Gaming with Session Count: {SessionCount}.");
            Logger.Debug(
                $"Restarting Responsible Gaming with Play Break Time: {SessionPlayBreakTime.TotalMinutes} Minutes.  Play Break Hit: {_sessionPlayBreakHit}");

            // Because we now only set the Dialog state once, we have to set the session time before we set the dialog state.

            var sessionLength = _properties.GetValue(LobbyConstants.LobbyPlayTimeRemainingInSeconds, -1);
            if (sessionLength > 0)
            {
                _sessionLength = TimeSpan.FromSeconds(sessionLength);
            }

            if (isTimeLimitDlgVisible)
            {
                if (_gameRecovery.IsRecovering)
                {
                    //if we are recovering, wait for recovery to be finished before displaying the dialog
                    IsTimeLimitDialogVisible = false;
                    ShowTimeLimitDlgPending = true;
                }
                else
                {
                    IsTimeLimitDialogVisible = true;
                    if (UpdateDialogShownTime)
                    {
                        _dialogShownTime = DateTime.UtcNow;
                    }
                }
            }
            else if (showTimeLimitDlgPending)
            {
                ShowTimeLimitDlgPending = true;
            }

            // Because of ALC We check the timer values even if the dialogs are up

            if (sessionLength > 0)
            {
                if (_disabledOnStartup)
                {
                    //don't start timer, but timer WAS running
                    _lastTimerTick = DateTime.UtcNow;
                    FireStateChange(ResponsibleGamingSessionState.Paused);
                }
                else
                {
                    StartSession(sessionLength / 60.0, false);
                }
            }
        }

        /// <inheritdoc />
        public void AcceptTimeLimit(int timeLimitIndex)
        {
            Logger.Debug($"AcceptTimeLimit with index: {timeLimitIndex}.");

            if (!IsTimeLimitDialogVisible && !ShowTimeLimitDlgPending)
            {
                Logger.Debug(
                    "IsTimeLimitDialogVisible is false && ShowTimeLimitDlgPending is false.  Ignore Button Press Event");
                return;
            }

            ShowTimeLimitDlgPending = false;
            IsTimeLimitDialogVisible = false;

            if (timeLimitIndex != -1) //-1 means we are shutting down Responsible Gaming due to a cash out
            {
                if (ResponsibleGamingMode == ResponsibleGamingMode.Continuous)
                {
                    if (IsSessionLimitHit || timeLimitIndex == (int)Buttons.No)
                    {
                        SignalForceCashOutEvent();
                    }
                    else
                    {
                        ContinueSession();
                    }
                }
                else
                {
                    //Standard
                    if (timeLimitIndex == (int)Buttons.CashOut)
                    {
                        SignalForceCashOutEvent();
                    }
                    else
                    {
                        var timeLimits = NormalizeTimeLimits(
                            _properties.GetProperty(LobbyConstants.RGTimeLimitsInMinutes, null) as double[]);
                        var timeLimitInMinutes = 15.0;
                        var playBreakInMinutes = 0.0;

                        if (timeLimits != null)
                        {
                            timeLimitInMinutes = timeLimits[timeLimitIndex];
                            var playBreaks = NormalizeTimeLimits(
                                _properties.GetProperty(LobbyConstants.RGPlayBreaksInMinutes, null) as double[]);
                            if (playBreaks != null)
                            {
                                playBreakInMinutes = playBreaks[timeLimitIndex];
                            }
                        }

                        Logger.Debug($"Time {timeLimitInMinutes} minutes.");
                        Logger.Debug($"PlayBreak At: {playBreakInMinutes} minutes.");
                        SessionCount++; //ALC advances SessionCount when the dialog shows
                        SessionPlayBreakTime = TimeSpan.FromMinutes(playBreakInMinutes);
                        StartSession(timeLimitInMinutes);
                    }
                }
            }
        }

        /// <inheritdoc />
        public void OnInitialCurrencyIn()
        {
            Logger.Debug("OnInitialCurrencyIn.");
            // Start the responsible gaming session only if responsible gaming is enabled from market configuration
            if (!_responsibleGamingEnabled)
            {
                return;
            }

            SessionCount = 0;
            if (ResponsibleGamingMode == ResponsibleGamingMode.Continuous)
            {
                StartSession();
            }
            else //Standard
            {
                InitiateShowTimeLimitDialog();
            }
        }

        /// <inheritdoc />
        public void EndResponsibleGamingSession()
        {
            if (!_responsibleGamingEnabled)
            {
                return;
            }

            Logger.Debug("EndResponsibleGamingSession.");

            //// Need to do this stuff for any cash out event, button or auto.
            lock (_stateLock)
            {
                _playLimitTimer?.Stop();
                _properties.SetProperty(LobbyConstants.LobbyPlayTimeRemainingInSeconds, -1);
                _properties.SetProperty(LobbyConstants.LobbyPlayTimeElapsedInSeconds, 0);
                _sessionElapsedTime = TimeSpan.Zero;
                _sessionLength = TimeSpan.Zero;

                ShowTimeLimitDlgPending = false; // If we had something pending but cashed out, we no longer need to show it.
                IsTimeLimitDialogVisible = false;
                SessionPlayBreakHit = false;
                SessionCount = 0;
                FireStateChange(ResponsibleGamingSessionState.Stopped);
            }
        }

        /// <inheritdoc />
        public void OnGamePlayDisabled()
        {
            Logger.Debug("OnGamePlayDisabled.");
            // Do not enter if we already stopped.
            if (_playLimitTimer != null)
            {
                lock (_stateLock)
                {
                    if (_state == ResponsibleGamingSessionState.Started)
                    {
                        UpdateElapsedTimeFromOverride();
                        _playLimitTimer.Stop();

                        // We are pausing the timer, add how much time has passed to elapsed time.
                        // DispatchTimer does not support pause.  Stopping/disabling resets the timer.
                        var elapsed = UpdateElapsedTimeSinceTick();

                        if (_dialogShownTime.HasValue)
                        {
                            //adjust the timer so that when we enable the dialog will time out in less than a minute
                            var newInterval = _playLimitTimer.Interval - elapsed;
                            if (newInterval.TotalSeconds < 1)
                            {
                                //protect against bad stuff
                                newInterval = TimeSpan.FromSeconds(1);
                            }

                            _playLimitTimer.Interval = newInterval;
                        }

                        FireStateChange(ResponsibleGamingSessionState.Paused);
                    }
                    else if (_state == ResponsibleGamingSessionState.Stopped)
                    {
                        FireStateChange(ResponsibleGamingSessionState.Disabled);
                    }

                    // if already Paused or Disabled, we do nothing
                }
            }
        }

        /// <inheritdoc />
        public void OnGamePlayEnabled()
        {
            Logger.Debug("OnGamePlayEnabled.");

            if (_playLimitTimer != null)
            {
                lock (_stateLock)
                {
                    // Un-pause timer (if it was running).
                    if (_state == ResponsibleGamingSessionState.Paused)
                    {
                        UpdateElapsedTimeFromOverride();

                        if (_dialogResetDueToOperatorMenu && UpdateDialogShownTime)
                        {
                            // we were already running a timer before the operator menu
                            // this allows us to keep that elapsed time and continue the timer
                            _dialogShownTime = DateTime.UtcNow;
                        }

                        if (_disabledOnStartup && IsTimeLimitDialogVisible && UpdateDialogShownTime)
                        {
                            _dialogShownTime = DateTime.UtcNow;
                            StartPlayTimer();
                        }
                        else if (_dialogShownTime.HasValue)
                        {
                            // a dialog is up and we are timing it to see if we should dismiss it.
                            _playLimitTimer.Start();
                            _lastTimerTick = DateTime.UtcNow;
                        }
                        else
                        {
                            StartPlayTimer();
                        }

                        FireStateChange(ResponsibleGamingSessionState.Started);
                    }
                    else if (_state == ResponsibleGamingSessionState.Disabled)
                    {
                        // no longer disabled
                        FireStateChange(ResponsibleGamingSessionState.Stopped);
                    }
                }

                if (IsTimeLimitDialogVisible)
                {
                    // if we are enabling the system after the dialog is already up, then we should
                    // clear this flag, since we are no longer doing the reset.
                    _dialogResetDueToOperatorMenu = false;
                }
            }
        }

        public void ResetDialog(bool resetDueToOperatorMenu)
        {
            lock (_timerLock)
            {
                _dialogResetDueToOperatorMenu = resetDueToOperatorMenu;
                IsTimeLimitDialogVisible = false;
                ShowTimeLimitDlgPending = true;

                if (ResponsibleGamingMode == ResponsibleGamingMode.Continuous)
                {
                    //SessionCount should ALWAYS be > 0 when this is called, but protecting for error cases so we don't crash.
                    if (SessionCount > 0)
                    {
                        SessionCount--; //ALC advances the SessionCount when the dialog Shows, so decrease it.
                    }
                }
            }
        }
        //*** ResponsibleGaming FireStateChange ResponsibleGamingSessionState.Started
        public event ResponsibleGamingStateChangeEventHandler OnStateChange
        {
            add
            {
                lock (_eventLock)
                {
                    StateChangeEvent += value;
                }
            }
            remove
            {
                lock (_eventLock)
                {
                    StateChangeEvent -= value;
                }
            }
        }

        public event EventHandler<EventArgs> OnForcePendingCheck;

        public bool CanSpinReels()
        {
            var canSpin = !IsTimeLimitDialogVisible && !ShowTimeLimitDlgPending;
            Logger.Debug($"CanSpinReels: {canSpin}");
            return canSpin;
        }

        /// <summary>
        ///     Dispose.
        /// </summary>
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private event ResponsibleGamingStateChangeEventHandler StateChangeEvent;

        public void DismissPlayBreak()
        {
            Logger.Debug("Dismissing Play Break Dialog.");
            Debug.Assert(IsTimeLimitDialogVisible);

            ShowTimeLimitDlgPending = false;
            IsTimeLimitDialogVisible = false;
            SessionPlayBreakHit = true;
            ContinueSession();
        }

        public void EngageSpinGuard()
        {
            if (!_responsibleGamingEnabled)
            {
                return;
            }

            Logger.Debug("Engaging Spin Guard");
            CancellationToken token;
            lock (_spinGuardLock)
            {
                ClearSpinGuardCancellationSource();
                SpinGuard = true;
                _spinGuardCancellationSource = new CancellationTokenSource();
                token = _spinGuardCancellationSource.Token;
            }

            Task.Run(
                () =>
                {
                    CancellationTokenSource delay = null;
                    lock (_spinGuardLock)
                    {
                        if (!token.IsCancellationRequested)
                        {
                            delay = CancellationTokenSource.CreateLinkedTokenSource(token);
                        }
                    }

                    bool disengaged = false;
                    if (delay != null)
                    {
                        try
                        {
                            delay.Token.WaitHandle.WaitOne(SpinGuardLengthInMilliseconds);

                            lock (_spinGuardLock)
                            {
                                if (!delay.Token.IsCancellationRequested)
                                {
                                    disengaged = true;
                                    Logger.Debug("Spin Guard Disengaged");
                                    SpinGuard = false;
                                    if (ShowTimeLimitDlgPending)
                                    {
                                        Task.Run(() => OnForcePendingCheck?.Invoke(this, new EventArgs()), CancellationToken.None);
                                    }
                                }
                            }
                        }
                        finally
                        {
                            delay.Dispose();
                        }
                    }

                    if (!disengaged)
                    {
                        Logger.Debug("Resetting Spin Guard Timer");
                    }
                }, CancellationToken.None);
        }

        private void ClearSpinGuardCancellationSource()
        {
            if (_spinGuardCancellationSource != null)
            {
                _spinGuardCancellationSource.Cancel(false);
                _spinGuardCancellationSource.Dispose();
                _spinGuardCancellationSource = null;
            }
        }

        /// <summary>
        ///     Dispose.
        /// </summary>
        /// <param name="disposing">True if disposing; false if finalizing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_playLimitTimer != null)
                {
                    _playLimitTimer.Stop();
                    _playLimitTimer = null;
                }

                lock (_spinGuardLock)
                {
                    ClearSpinGuardCancellationSource();
                }
            }

            _disposed = true;
        }

        private bool UpdateDialogShownTime => ResponsibleGamingMode == ResponsibleGamingMode.Continuous || IsTimeForPlayBreak();

        private TimeSpan RemainingSessionTimeAbsolute
        {
            get
            {
                var time = _sessionLength - _sessionElapsedTime;
                if (time < TimeSpan.Zero)
                {
                    time = TimeSpan.Zero;
                }

                return time;
            }
        }

        private void StartSession(double? minutes = null, bool clearElapsedTime = true)
        {
            if (!minutes.HasValue && ResponsibleGamingMode == ResponsibleGamingMode.Continuous)
            {
                //Session Length is really not relevant for ALC
                var timeLimits =
                    NormalizeTimeLimits(_properties.GetProperty(LobbyConstants.RGTimeLimitsInMinutes, null) as double[]);
                minutes = timeLimits[SessionLimit - 1];
            }
            else
            {
                Debug.Assert(minutes.HasValue, "Responsible Gaming session needs minutes value");
            }

            Logger.Debug(
                $"Start Responsible Gaming Session.  Session Length: {minutes} minutes.  Clear Elapsed Time: {clearElapsedTime}");

            _sessionLength = TimeSpan.FromSeconds((int)Math.Round(minutes.Value * 60, MidpointRounding.AwayFromZero));
            _properties.SetProperty(LobbyConstants.LobbyPlayTimeRemainingInSeconds, (int)_sessionLength.TotalSeconds);

            if (clearElapsedTime) //if clearElapsed time is false, we are rebooting in mid-timer.
            {
                _sessionElapsedTime = TimeSpan.Zero;
                UpdateElapsedTimeFromOverride();
                _properties.SetProperty(LobbyConstants.LobbyPlayTimeElapsedInSeconds, (int)_sessionElapsedTime.TotalSeconds);
            }

            SessionPlayBreakHit = false;

            lock (_stateLock)
            {
                if (_state == ResponsibleGamingSessionState.Stopped || _state == ResponsibleGamingSessionState.Started)
                {
                    //*** ResponsibleGaming FireStateChange ResponsibleGamingSessionState.Started
                    StartPlayTimer();
                    FireStateChange(ResponsibleGamingSessionState.Started);
                }
                else if (_state == ResponsibleGamingSessionState.Disabled || _state == ResponsibleGamingSessionState.Paused)
                {
                    _lastTimerTick = DateTime.MinValue;
                    FireStateChange(ResponsibleGamingSessionState.Paused);
                }
            }
        }

        private void ContinueSession()
        {
            Logger.Debug("Continue Responsible Gaming Session");
            UpdateElapsedTimeFromOverride();
            UpdateElapsedTimeSinceTick();
            StartPlayTimer();
            SaveSessionElapsedTime();
        }

        private void InitiateShowTimeLimitDialog()
        {
            Logger.Debug("InitiateShowTimeLimitDialog.");
            ShowTimeLimitDlgPending = true;
        }

        private void PlayLimitTimerTick(object sender, EventArgs e)
        {
            Logger.Debug($"Play Limit Timer Tick.  Dialog Visible: {IsTimeLimitDialogVisible} SessionPlayBreakHit: {SessionPlayBreakHit} DialogShown (timed): {_dialogShownTime.HasValue}");
            lock (_timerLock)
            {
                _lastTimerTick = DateTime.UtcNow;

                if (!UpdateElapsedTimeFromOverride())
                {
                    _sessionElapsedTime += _playLimitTimer.Interval;
                }

                if (ResponsibleGamingMode == ResponsibleGamingMode.Continuous && IsTimeLimitDialogVisible)
                {
                    AcceptTimeLimit((int)Buttons.YesOk); //we are dismissing the dialog for the user after a timeout
                }
                else if (ResponsibleGamingMode == ResponsibleGamingMode.Segmented && IsTimeLimitDialogVisible &&
                         !SessionPlayBreakHit && _dialogShownTime.HasValue)
                {
                    DismissPlayBreak();
                }
                else
                {
                    if (!ShowTimeLimitDlgPending && !IsTimeLimitDialogVisible)
                    {
                        //don't do this if we are already pending or visible
                        if (ResponsibleGamingMode == ResponsibleGamingMode.Segmented &&
                            (_sessionElapsedTime >= _sessionLength || IsTimeForPlayBreak()) ||
                            ResponsibleGamingMode == ResponsibleGamingMode.Continuous && IsAlcTimeToShowDialog())
                        {
                            InitiateShowTimeLimitDialog();
                        }
                    }

                    var newInterval = GetTimerInterval();
                    if (newInterval != _playLimitTimer.Interval)
                    {
                        StartPlayTimer(); // set new timer interval
                    }

                    SaveSessionElapsedTime();
                }
            }
        }

        private TimeSpan GetTimerInterval()
        {
            TimeSpan timeInterval;

            if (_dialogShownTime.HasValue)
            {
                var defaultValue = ResponsibleGamingMode == ResponsibleGamingMode.Continuous
                    ? TimerAlcDialogDismissalInSeconds
                    : TimerPlayBreakDismissalInSeconds;

                var timeoutInSeconds = (int)_properties.GetProperty(
                    LobbyConstants.LobbyPlayTimeDialogTimeoutInSeconds,
                    defaultValue);

                timeInterval = TimeSpan.FromSeconds(timeoutInSeconds);
            }
            else if (IsTimeLimitDialogVisible || ShowTimeLimitDlgPending)
            {
                timeInterval = TimeSpan.FromSeconds(TimerLengthInSeconds);
            }
            else
            {
                if (ResponsibleGamingMode == ResponsibleGamingMode.Continuous)
                {
                    var currentLimit = GetCurrentAlcTimeOut();
                    timeInterval = currentLimit - _sessionElapsedTime;
                }
                else //Standard
                {
                    timeInterval = RemainingSessionTimeAbsolute;
                    if (SessionPlayBreakTime != TimeSpan.Zero)
                    {
                        // we have a play break time to look for
                        var timeInterval2 = SessionPlayBreakTime - _sessionElapsedTime;
                        if (timeInterval2.TotalSeconds >= 0 && timeInterval2 < timeInterval)
                        {
                            timeInterval = timeInterval2;
                        }
                    }
                }

                if (timeInterval.TotalSeconds <= 0)
                {
                    timeInterval = TimeSpan.FromSeconds(1);
                }

                if (ResponsibleGamingMode == ResponsibleGamingMode.Continuous)
                {
                    if (TimerLengthInSeconds < timeInterval.TotalSeconds)
                    {
                        timeInterval = TimeSpan.FromSeconds(TimerLengthInSeconds);
                    }
                }
                else
                {
                    // VLT-5421: Align the timer with Remaining Time Display changes so that we persist updates right after the display changes
                    var remainingTime = RemainingSessionTimeAbsolute;
                    var minutesToTimeLeftDisplayChange = remainingTime.TotalMinutes - Math.Truncate(remainingTime.TotalMinutes);

                    if (minutesToTimeLeftDisplayChange.Equals(0))
                    {
                        minutesToTimeLeftDisplayChange = 1;
                    }
                    else
                    {
                        //round up to closest 1000th of a minute so we don't end up with .00001 minute left when timer ticks
                        minutesToTimeLeftDisplayChange = Math.Ceiling(minutesToTimeLeftDisplayChange * 1000) / 1000.0;
                    }

                    if (minutesToTimeLeftDisplayChange < timeInterval.TotalMinutes)
                    {
                        timeInterval = TimeSpan.FromMinutes(minutesToTimeLeftDisplayChange);
                    }
                }
            }

            return timeInterval;
        }

        private void StartPlayTimer()
        {
            var interval = GetTimerInterval();
            Logger.Debug($"Starting Play Timer: {interval.TotalSeconds} Seconds");
            _playLimitTimer.Interval = interval;
            _lastTimerTick = DateTime.UtcNow;
            _playLimitTimer.Stop();
            _playLimitTimer.Start();
        }

        private void SaveSessionElapsedTime()
        {
            Logger.Debug($"Session Elapsed Time: {_sessionElapsedTime.TotalMinutes} Minutes");
            Task.Run(
                () =>
                {
                    _properties.SetProperty(
                        LobbyConstants.LobbyPlayTimeElapsedInSeconds,
                        (int)_sessionElapsedTime.TotalSeconds);
                });
        }

        private void SaveSessionPlayBreakTime()
        {
            Logger.Debug($"Session Play Break Time: {SessionPlayBreakTime.TotalMinutes} Minutes");
            Task.Run(
                () =>
                {
                    Logger.Debug($"Session Play Break Time: {SessionPlayBreakTime.TotalMinutes} Minutes");
                    _properties.SetProperty(
                        LobbyConstants.ResponsibleGamingPlayBreakTimeoutInSeconds,
                        (int)SessionPlayBreakTime.TotalSeconds);
                });
        }

        private void SaveSessionPlayBreakHit()
        {
            Logger.Debug($"Session Play Break Hit: {SessionPlayBreakHit}");
            Task.Run(
                () => { _properties.SetProperty(LobbyConstants.ResponsibleGamingPlayBreakHit, SessionPlayBreakHit); });
        }

        private bool IsAlcTimeToShowDialog()
        {
            return _sessionElapsedTime >= GetCurrentAlcTimeOut();
        }

        private TimeSpan GetCurrentAlcTimeOut()
        {
            var timeLimits =
                NormalizeTimeLimits(_properties.GetProperty(LobbyConstants.RGTimeLimitsInMinutes, null) as double[]);
            if (ResponsibleGamingMode == ResponsibleGamingMode.Continuous && SessionCount >= SessionLimit)
            {
                //We probably had a situation where cash out failed and we didn't shut down Responsible Gaming
                //to avoid crashing, take us back to the final session and display the dialog again.
                SessionCount = SessionLimit - 1;
            }

            // VLT-4506 prevent index out of bounds
            if (SessionCount >= timeLimits.Length)
            {
                SessionCount = timeLimits.Length - 1;
            }

            var seconds = (int)Math.Round(timeLimits[SessionCount] * 60, MidpointRounding.AwayFromZero);
            var timeOut = TimeSpan.FromSeconds(seconds);
            Logger.Debug(
                $"Next Responsible Gaming Timeout (Session Count: {SessionCount}): {timeOut.TotalMinutes} Minutes");
            return timeOut;
        }

        private void SetSessionCountBasedOnElapsedTime()
        {
            int? sessionCount = null;
            var timeLimits =
                NormalizeTimeLimits(_properties.GetProperty(LobbyConstants.RGTimeLimitsInMinutes, null) as double[]);
            var elapsedTimeInMinutes = _sessionElapsedTime.TotalSeconds / 60.0;
            for (var i = 0; i < timeLimits.Length && !sessionCount.HasValue; i++)
            {
                if (elapsedTimeInMinutes < timeLimits[i])
                {
                    sessionCount = i;
                }
            }

            if (!sessionCount.HasValue)
            {
                sessionCount = SessionLimit;
            }

            Logger.Debug($"Session Count Based on Elapsed Time is: {sessionCount.Value}");

            SessionCount = sessionCount.Value;
        }

        private void SignalForceCashOutEvent()
        {
            Logger.Debug("Forcing a Cash Out");
            ForceCashOut?.Invoke(this, new EventArgs());
        }

        //Responsible Gaming was written assuming the Time Limit values would be at least whole seconds.
        //If the values are fractional seconds, we run into issues.  This will normalize the values to
        //the nearest whole second.
        private double[] NormalizeTimeLimits(double[] timeLimits)
        {
            if (timeLimits == null)
            {
                return null;
            }

            for (var i = 0; i < timeLimits.Length; i++)
            {
                timeLimits[i] = Math.Round(timeLimits[i] * 60, MidpointRounding.AwayFromZero) / 60.0;
            }

            return timeLimits;
        }

        private TimeSpan UpdateElapsedTimeSinceTick()
        {
            if (_lastTimerTick == DateTime.MinValue)
                return TimeSpan.Zero;

            var elapsed = _lastTimerTick.GetUtcElapsedTime(out DateTime now);
            _lastTimerTick = now;
            _sessionElapsedTime += elapsed;
            SaveSessionElapsedTime();
            return elapsed;
        }

        private bool IsTimeForPlayBreak()
        {
            return SessionPlayBreakTime != TimeSpan.Zero && !SessionPlayBreakHit &&
                   _sessionElapsedTime >= SessionPlayBreakTime
                   && _sessionElapsedTime < _sessionLength;
        }

        private void FireStateChange(ResponsibleGamingSessionState state)
        {
            if (_state != state)
            {
                var handler = StateChangeEvent;
                var remainingTime = state == ResponsibleGamingSessionState.Stopped
                    ? TimeSpan.Zero
                    : RemainingSessionTime;

                _state = state;
                Logger.Debug($"Setting State: {state}");
                handler?.Invoke(this, new ResponsibleGamingSessionStateEventArgs(state, remainingTime));
            }
        }

        private ResponsibleGamingDialogState FindTimeLimitDialogState()
        {
            ResponsibleGamingDialogState state;

            if (ResponsibleGamingMode == ResponsibleGamingMode.Continuous)
            {
                Debug.Assert(SessionCount <= 5);
                //mode is ALC
                if (SessionCount <= 3)
                {
                    state = ResponsibleGamingDialogState.TimeInfo;
                }
                else if (SessionCount == 4)
                {
                    state = ResponsibleGamingDialogState.TimeInfoLastWarning;
                }
                else
                {
                    Debug.Assert(IsSessionLimitHit);
                    state = ResponsibleGamingDialogState.SeeionEndForceCashOut;
                }
            }
            else //Standard
            {
                if (SessionPlayBreakTime != TimeSpan.Zero &&
                    _sessionElapsedTime < _sessionLength &&
                    _sessionElapsedTime >= SessionPlayBreakTime)
                {
                    state = SessionCount == 1
                        ? ResponsibleGamingDialogState.PlayBreak1
                        : ResponsibleGamingDialogState.PlayBreak2;
                }
                else if (SessionCount == 0)
                {
                    state = ResponsibleGamingDialogState.Initial;
                }
                else if (IsSessionLimitHit)
                {
                    state = ResponsibleGamingDialogState.ForceCashOut;
                }
                else
                {
                    state = ResponsibleGamingDialogState.ChooseTime;
                }
            }

            return state;
        }

        private bool UpdateElapsedTimeFromOverride()
        {
            bool sessionElapsedTimeUpdated = false;
#if !(RETAIL)
            if (_properties.GetProperty(LobbyConstants.LobbyPlayTimeElapsedInSecondsOverride, null) is int elapsedTimeOverride)
            {
                if (_sessionElapsedTime < _sessionLength) // VLT-6035 don't reset the elapsed time if the session has already expired
                {
                    _sessionElapsedTime = TimeSpan.FromSeconds(elapsedTimeOverride);
                    Logger.Debug($"Overriding Session Elapsed Time to {elapsedTimeOverride} seconds");
                    if (ResponsibleGamingMode == ResponsibleGamingMode.Continuous)
                    {
                        SetSessionCountBasedOnElapsedTime();
                    }

                    _properties.SetProperty(LobbyConstants.LobbyPlayTimeElapsedInSecondsOverride, null);
                    sessionElapsedTimeUpdated = true;
                }
                else
                {
                    Logger.Debug($"Skipping Elapsed Time Override because Session Elapsed Time of {_sessionElapsedTime} exceeds Session Length of {_sessionLength}");
                }
            }

            if (ResponsibleGamingMode == ResponsibleGamingMode.Segmented)
            {
                if (_properties.GetProperty(LobbyConstants.LobbyPlayTimeSessionCountOverride, null) is int sessionCountOverride)
                {
                    if (!IsTimeLimitDialogVisible) // VLT-6035 don't reset the SessionCount if the dialog is already up
                    {
                        SessionCount = sessionCountOverride;
                        Logger.Debug($"Overriding Session Count to {sessionCountOverride}");
                        _properties.SetProperty(LobbyConstants.LobbyPlayTimeSessionCountOverride, null);
                    }
                    else
                    {
                        Logger.Debug("Skipping Session Count Override because Responsible Gaming Dialog was up.");
                    }
                }
            }
#endif
            return sessionElapsedTimeUpdated;
        }

        private enum Buttons
        {
            YesOk = 0,
            No = 1,
            CashOut = 4
        }
    }
}
