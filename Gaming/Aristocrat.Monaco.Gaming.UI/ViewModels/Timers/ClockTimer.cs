namespace Aristocrat.Monaco.Gaming.UI.ViewModels.Timers
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using Application.Contracts;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using Common;
    using Contracts;
    using Contracts.Lobby;
    using Contracts.Models;
    using Kernel;
    using log4net;
    using Monaco.UI.Common;
    using Monaco.UI.Common.Models;

    public class ClockTimer : BaseObservableObject
    {
        private const int ClockStateTimeoutInSeconds = 30;
        private const double DayTimerIntervalSeconds = 1.0;
        private const string TimeResourceKey = "TimeLabel";
        private const string TimeLeftResourceKey = "TimeLeftLabel";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly object _clockStateLock = new object();

        private readonly LobbyConfiguration _config;
        private readonly ILobbyStateManager _lobbyStateManager;
        private readonly IResponsibleGaming _responsibleGaming;
        private readonly ITime _timeService;
        private readonly IRuntimeFlagHandler _runtime;

        private LobbyClockState _clockState = LobbyClockState.Clock;
        private TimeSpan _clockTimerElapsedTime;
        private DateTime _clockTimerStartTime;
        private string _currentTime;
        private DateTime? _lastClockStateSet;
        private bool? _lastDisplayingTimeRemainingValue;
        private string _lastTimeRemainingValue;
        private string _sessionTimeText;

        public ClockTimer(
            LobbyConfiguration config,
            IResponsibleGaming responsible,
            IRuntimeFlagHandler runtime,
            ILobbyStateManager stateManager)
        {
            _config = config;
            _lobbyStateManager = stateManager;
            _responsibleGaming = responsible;
            _runtime = runtime;

            ClockStateTimer =
                new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(ClockStateTimeoutInSeconds) };
            ClockStateTimer.Tick += ClockStateTimer_Tick;

            DayTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(DayTimerIntervalSeconds) };
            DayTimer.Tick += DayTimer_Tick;
            DayTimer.Start();

            _timeService = ServiceManager.GetInstance().GetService<ITime>();
            _timeService.SetDateTimeForCurrentCulture();

            if (_config.DisplaySessionTimeInClock)
            {
                StartClockTimer();
                UpdateSessionTimeText();
            }

            UpdateTime();
        }

        /// <summary>
        ///     Property for determining how the clock is displayed.  Read from the config file
        /// </summary>
        public ClockMode ClockMode => _config.ClockMode;

        public LobbyClockState ClockState
        {
            get => _clockState;

            set
            {
                if (_clockState != value)
                {
                    _clockState = value;
                    _lastClockStateSet = DateTime.UtcNow;
                    Logger.Debug($"Setting Lobby Clock State: {_clockState}");
                    OnPropertyChanged(nameof(ClockState));
                    UpdateTime();
                }
            }
        }

        public ITimer ClockStateTimer { get; }

        public ITimer DayTimer { get; private set; }

        /// <summary>
        ///     Gets or sets the current time
        /// </summary>
        public string CurrentTime
        {
            get => _currentTime;

            set
            {
                if (_currentTime != value)
                {
                    _currentTime = value;
                    OnPropertyChanged(nameof(CurrentTime));
                }
            }
        }

        public bool IsInLobby { get; set; } = true;

        public bool IsPrimaryLanguageSelected { get; set; } = true;

        public bool IsResponsibleGamingSessionOver => _responsibleGaming.RemainingSessionTime.TotalSeconds <= 0;

        public bool IsSessionOverLabelVisible => ClockState == LobbyClockState.ResponsibleGamingSessionTime &&
                                                 IsResponsibleGamingSessionOver;

        public ResponsibleGamingSessionState ResponsibleGamingSessionState { get; set; }

        public string SessionTimeText
        {
            get => _sessionTimeText;

            set
            {
                if (_sessionTimeText != value)
                {
                    _sessionTimeText = value;
                    UpdateTime();
                }
            }
        }

        private string ActiveLocaleCode => IsPrimaryLanguageSelected ? _config.LocaleCodes[0] : _config.LocaleCodes[1];

        public string TimeLabelResourceKey =>
            ClockState == LobbyClockState.Clock ? TimeResourceKey : TimeLeftResourceKey;

        public void ChangeClockState(
            LobbyClockState? newClockState = null,
            bool calledFromTimer = false,
            bool calledFromFlagChangeEvent = false)
        {
            if (_config.DisplaySessionTimeInClock)
            {
                lock (_clockStateLock)
                {
                    if (calledFromTimer && _lastClockStateSet.HasValue && _lastClockStateSet.Value >
                        DateTime.UtcNow - TimeSpan.FromSeconds(ClockStateTimeoutInSeconds - 1)
                    ) // -1 because event seems to sometimes fire a few milliseconds earlier than expected so need buffer.
                    {
                        // Timestamp the clock flip and ignore the timer if it comes in earlier than it should.  
                        // This should prevent double flips caused by lags in communication with the runtime.
                        Logger.Debug(
                            $"Ignoring ChangeClockState: LastClockStateSet:{_lastClockStateSet.Value.ToString("hh: mm:ss.fff", CultureInfo.InvariantCulture)}");
                        return;
                    }

                    if (!newClockState.HasValue)
                    {
                        newClockState = ClockState == LobbyClockState.Clock
                            ? LobbyClockState.ResponsibleGamingSessionTime
                            : LobbyClockState.Clock;
                    }

                    if (ClockState != newClockState)
                    {
                        ClockStateTimer?.Stop();
                        ClockState = newClockState.Value;
                        StartClockTimer();

                        if (!calledFromFlagChangeEvent)
                        {
                            // don't cause an infinite loop of flag change events
                            SetDisplayingTimeRemainingFlag();
                        }
                        else
                        {
                            // Set value properly even though we didn't actually send it.  
                            _lastDisplayingTimeRemainingValue = ClockState == LobbyClockState.ResponsibleGamingSessionTime;
                        }
                    }
                }
            }
        }

        public void SetDisplayingTimeRemainingFlag()
        {
            _lastTimeRemainingValue = null;
            _lastDisplayingTimeRemainingValue = null;
            UpdateSessionTimeText();

            var showingTimeRemaining = ClockState == LobbyClockState.ResponsibleGamingSessionTime;

            if ((!_lastDisplayingTimeRemainingValue.HasValue ||
                 _lastDisplayingTimeRemainingValue.Value != showingTimeRemaining) &&
                (!IsInLobby || _lobbyStateManager.CurrentState == LobbyState.GameLoading))
            {
                Logger.Debug($"DisplayTimeRemaining Runtime Flag: {showingTimeRemaining}");
                _lastDisplayingTimeRemainingValue = showingTimeRemaining;
                _runtime.SetDisplayingTimeRemaining(showingTimeRemaining);
            }
        }

        public void UpdateClockTimer()
        {
            if (_config.DisplaySessionTimeInClock)
            {
                lock (ClockStateTimer)
                {
                    switch (ResponsibleGamingSessionState)
                    {
                        case ResponsibleGamingSessionState.Started:
                            if (!ClockStateTimer.IsEnabled)
                            {
                                StartClockTimer(_clockTimerElapsedTime);
                            }

                            break;
                        case ResponsibleGamingSessionState.Paused:
                            if (ClockStateTimer.IsEnabled)
                            {
                                _clockTimerElapsedTime += _clockTimerStartTime.GetUtcElapsedTime(out var _);
                                ClockStateTimer?.Stop();
                            }

                            break;
                        case ResponsibleGamingSessionState.Stopped:
                        case ResponsibleGamingSessionState.Disabled:
                            break;
                    }
                }

                UpdateSessionTimeText();
            }
        }

        public void RestartClockTimer()
        {
            //This should only happen if we had a forced cashout while disabled and now we are re-enabling 
            if (_config.DisplaySessionTimeInClock &&
                ResponsibleGamingSessionState == ResponsibleGamingSessionState.Stopped &&
                !ClockStateTimer.IsEnabled)
            {
                Logger.Debug(
                    "Restarting clock timer because EGM is coming out of lockup, responsible gaming is stopped, but clock timer is not running");
                StartClockTimer(_clockTimerElapsedTime);
            }
        }

        public void UpdateSessionTimeText()
        {
            //we want to round the time up to the last whole minute.  So if you have 59 min and 1 seconds, we should show 1 hour.
            var time = _responsibleGaming.RemainingSessionTime;
            var fractionalMinutes = time.TotalMinutes % 1;

            if (!fractionalMinutes.Equals(0))
            {
                var timeToAdd = TimeSpan.FromMinutes(1) - TimeSpan.FromMinutes(fractionalMinutes) +
                                TimeSpan.FromSeconds(1);
                time += timeToAdd;
            }

            SessionTimeText = time.ToString(@"h\:mm");
            // VLT-6699 : have the time left clock not be blank when RG times out
            //  this will display 0:00
            SetTimeRemaining(time.ToString(@"h\:mm"));
        }

        public void UpdateTime()
        {
            OnPropertyChanged(nameof(IsSessionOverLabelVisible));
            if (_clockState == LobbyClockState.Clock)
            {
                var culture = new CultureInfo(ActiveLocaleCode);
                var format = culture.DateTimeFormat.ShortTimePattern;

                switch (ClockMode)
                {
                    case ClockMode.Locale:
                        if (ActiveLocaleCode == GamingConstants.FrenchCultureCode)
                        {
                            culture = CultureInfo.CreateSpecificCulture(GamingConstants.FrenchCultureCode);
                            var dtfi = culture.DateTimeFormat;
                            dtfi.TimeSeparator = "h";
                            format = "HH:mm";
                        }

                        break;
                    case ClockMode.Military:
                        //we want 24-hour always for Quebec, which is FR-CA culture    
                        culture = CultureInfo.CreateSpecificCulture(GamingConstants.FrenchCultureCode);
                        format = culture.DateTimeFormat.ShortTimePattern;
                        break;
                }

                CurrentTime = _timeService.GetLocationTime().ToString(format, culture);
            }
            else //_clockState == LobbyClockState.ResponsibleGamingSessionTime
            {
                CurrentTime = SessionTimeText;
            }

            OnPropertyChanged(nameof(TimeLabelResourceKey));
        }

        private void DayTimer_Tick(object sender, EventArgs e)
        {
            if (_config.DisplaySessionTimeInClock &&
                ResponsibleGamingSessionState != ResponsibleGamingSessionState.Stopped)
            {
                UpdateSessionTimeText();
            }

            UpdateTime();
        }

        private void ClockStateTimer_Tick(object sender, EventArgs e)
        {
            ChangeClockState(null, true);
        }

        private void SetTimeRemaining(string timeRemainingText)
        {
            if (_config.DisplaySessionTimeInClock)
            {
                if ((_lastTimeRemainingValue == null || _lastTimeRemainingValue != timeRemainingText) &&
                    (!IsInLobby || _lobbyStateManager.CurrentState == LobbyState.GameLoading))
                {
                    Logger.Debug($"SetTimeRemaining: {timeRemainingText}");
                    _lastTimeRemainingValue = timeRemainingText;
                    _runtime.SetTimeRemaining(timeRemainingText);
                }
            }
        }

        private void StartClockTimer(TimeSpan? elapsedTime = null)
        {
            Logger.Debug($"StartClockTimer.  Elapsed Time: {elapsedTime}");

            if (ClockStateTimer != null)
            {
                var clockStateTimeSpan = TimeSpan.FromSeconds(ClockStateTimeoutInSeconds);
                lock (_clockStateLock)
                {
                    if (elapsedTime.HasValue && elapsedTime.Value > TimeSpan.Zero &&
                        elapsedTime.Value < clockStateTimeSpan)
                    {
                        ClockStateTimer.Interval = TimeSpan.FromSeconds(ClockStateTimeoutInSeconds) - elapsedTime.Value;
                    }
                    else
                    {
                        Logger.Debug(
                            $"Elapsed Time not present or not valid.  Using Default Clock State Timeout of {ClockStateTimeoutInSeconds}");
                        ClockStateTimer.Interval = TimeSpan.FromSeconds(ClockStateTimeoutInSeconds);
                        _clockTimerElapsedTime = TimeSpan.Zero;
                    }

                    _clockTimerStartTime = DateTime.UtcNow;
                    Logger.Debug(
                        $"ClockState timer StartTime: {_clockTimerStartTime}.  Interval: {ClockStateTimer.Interval}");
                    ClockStateTimer.Start();
                }
            }
        }

        public void Dispose()
        {
            DayTimer?.Stop();
            DayTimer = null;
        }
    }
}
