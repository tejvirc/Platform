namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Contracts.Events;
    using Contracts;
    using Kernel;
    using Localization.Properties;
    using log4net;

    /// <summary>
    ///     Implements <see cref="IReserveService" /> interface
    /// </summary>
    public sealed class ReserveService : IReserveService, IDisposable
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _propertiesManager;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly IGamePlayState _gamePlay;
        private readonly IPlayerBank _bank;
        private bool _disposed;
        private bool _reserveServiceAllowed;
        private bool _reserveServiceSupported;
        private Timer _reserveServiceLockupTimer;
        private int _remainingSeconds;
        private int _timeoutInSeconds;
        private bool _isGambleFeatureActive;
        private const int DefaultTimeOutInMinutes = 5;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReserveService" /> class.
        /// </summary>
        /// <param name="eventBus">The event bus service.</param>
        /// <param name="propertiesManager">The properties manager.</param>
        /// <param name="disableManager">The system disable manager.</param>
        /// <param name="gamePlay">The game play service.</param>
        /// <param name="bank">The bank service.</param>
        public ReserveService(
            IEventBus eventBus,
            IPropertiesManager propertiesManager,
            ISystemDisableManager disableManager,
            IGamePlayState gamePlay,
            IPlayerBank bank)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _systemDisableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _gamePlay = gamePlay ?? throw new ArgumentNullException(nameof(gamePlay));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));

            Initialize();
        }

        private bool IsReserveCondition =>
            (_gamePlay.CurrentState == PlayState.Idle || _isGambleFeatureActive) &&
        _bank.Balance > 0 &&
            (!_systemDisableManager.CurrentDisableKeys.Any() || _systemDisableManager.CurrentDisableKeys.Count == 1 && _systemDisableManager.CurrentDisableKeys.Contains(ApplicationConstants.WaitingForInputDisableKey));

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IReserveService) };

        /// <inheritdoc />
        public bool IsMachineReserved { get; private set; }

        public bool ReserveMachine()
        {
            if (!_reserveServiceSupported || !_reserveServiceAllowed)
            {
                return false;
            }

            return CanReserveMachine && CreateLockup();
        }

        public bool ExitReserveMachine()
        {
            if (!IsReserveServiceLockupPresent)
            {
                return false;
            }

            RemoveLockup();
            IsMachineReserved = false;
            _reserveServiceLockupTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            return true;
        }

        public bool CanReserveMachine => IsReserveCondition &&
                                         (bool)_propertiesManager.GetProperty(
                                             ApplicationConstants.ReserveServiceAllowed,
                                             true) &&
                                         (bool)_propertiesManager.GetProperty(
                                             ApplicationConstants.ReserveServiceEnabled,
                                             true);

        /// <inheritdoc />
        public void Initialize()
        {
            Logger.Debug("Initialize.");

            _reserveServiceAllowed = (bool)_propertiesManager.GetProperty(
                ApplicationConstants.ReserveServiceAllowed,
                true);

            _reserveServiceSupported =
                (bool)_propertiesManager.GetProperty(ApplicationConstants.ReserveServiceEnabled, true);

            _reserveServiceLockupTimer = new Timer(
                OnReserveLockupTimerTick,
                null,
                Timeout.InfiniteTimeSpan,
                Timeout.InfiniteTimeSpan);

            _eventBus?.Subscribe<ExitReserveButtonPressedEvent>(this, HandleEvent);
            _eventBus?.Subscribe<PropertyChangedEvent>(this, HandleEvent);
            _eventBus?.Subscribe<GambleFeatureActiveEvent>(this, HandleEvent);

            SetupReserveServiceTimer();

            // If we have a lockup and the reserve is not allowed or enabled, clear up lockup.
            // This should not happen in the first place
            if ((!_reserveServiceSupported || !_reserveServiceAllowed) && IsReserveServiceLockupPresent)
            {
                RemoveLockup();
            }

            //If the reserve is not allowed, enabled or there's no lockup on start, no need to do anything else.
            if (!IsReserveServiceLockupPresent || !_reserveServiceSupported || !_reserveServiceAllowed)
            {
                return;
            }

            CreateLockup(true);
            _eventBus?.Subscribe<OverlayWindowVisibilityChangedEvent>(this, HandleEvent);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);

                // ReSharper disable once UseNullPropagation
                if (_reserveServiceLockupTimer != null)
                {
                    _reserveServiceLockupTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                    _reserveServiceLockupTimer.Dispose();
                    _reserveServiceLockupTimer = null;
                }
            }

            _disposed = true;
        }

        private void SetupReserveServiceTimer()
        {
            _timeoutInSeconds = _propertiesManager.GetValue(
                ApplicationConstants.ReserveServiceTimeoutInSeconds,
                (int)TimeSpan.FromMinutes(DefaultTimeOutInMinutes).TotalSeconds);
        }

        // Callback when the reserve timer expires.
        private void OnReserveLockupTimerTick(object state)
        {
            if (!IsMachineReserved)
            {
                return;
            }

            if (--_remainingSeconds > 0)
            {
                _propertiesManager.SetProperty(ApplicationConstants.ReserveServiceLockupRemainingSeconds, _remainingSeconds);
                return;
            }

            IsMachineReserved = false;
            RemoveLockup();
        }

        private void RemoveLockup()
        {
            _systemDisableManager.Enable(ApplicationConstants.ReserveDisableKey);
            _propertiesManager.SetProperty(ApplicationConstants.ReserveServiceLockupPresent, false);
            _propertiesManager.SetProperty(ApplicationConstants.ReserveServiceLockupRemainingSeconds, 0);
        }

        private void ShowLockup()
        {
            _systemDisableManager.Disable(
                ApplicationConstants.ReserveDisableKey,
                SystemDisablePriority.Immediate,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReservedMachine));

            _propertiesManager.SetProperty(ApplicationConstants.ReserveServiceLockupPresent, true);
        }

        private void HandleEvent(ExitReserveButtonPressedEvent evt)
        {
            ExitReserveMachine();
        }

        private void HandleEvent(GambleFeatureActiveEvent evt)
        {
            _isGambleFeatureActive = evt.Active;
        }

        private void HandleEvent(PropertyChangedEvent evt)
        {
            switch (evt.PropertyName)
            {
                case ApplicationConstants.ReserveServiceEnabled:
                    _reserveServiceSupported =
                        (bool)_propertiesManager.GetProperty(ApplicationConstants.ReserveServiceEnabled, true);
                    break;

                case ApplicationConstants.ReserveServiceTimeoutInSeconds:
                    SetupReserveServiceTimer();
                    break;
            }
        }


        private void HandleEvent(OverlayWindowVisibilityChangedEvent evt)
        {
            if (!evt.IsVisible)
                return;

            _eventBus.Unsubscribe<OverlayWindowVisibilityChangedEvent>(this);

            if (IsReserveServiceLockupPresent)
            {
                _reserveServiceLockupTimer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            }
        }

        private bool IsReserveServiceLockupPresent => (bool)_propertiesManager.GetProperty(ApplicationConstants.ReserveServiceLockupPresent, false);
        
        private bool CreateLockup(bool lockupOnStartup = false)
        {
            ShowLockup();
            IsMachineReserved = true;

            _remainingSeconds = (int)_propertiesManager.GetProperty(
                ApplicationConstants.ReserveServiceLockupRemainingSeconds,
                0);

            if (_remainingSeconds == 0)
            {
                _remainingSeconds = _timeoutInSeconds;
            }

            //while reserving the machine on startup, don't start timer immediately as game will take
            //some time to initialize , will do that when initialization is completed.
            if (!lockupOnStartup)
            {
                _reserveServiceLockupTimer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            }

            return true;
        }
    }
}