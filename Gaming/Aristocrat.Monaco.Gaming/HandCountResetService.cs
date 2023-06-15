namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Accounting.Contracts;
    using System.Reflection;
    using System.Timers;
    using Contracts;
    using Kernel;
    using log4net;
    using Aristocrat.Monaco.Hardware.Contracts.Button;
    using Hardware.Contracts.Touch;
    using Aristocrat.Monaco.Accounting.Contracts.HandCount;
    using Aristocrat.Monaco.Kernel.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Events;

    /// <summary>
    ///     An implementation of <see cref="IHandCountResetService" />
    /// </summary>
    public class HandCountResetService : IHandCountResetService
    {
        private const double IdleTimerInterval = 1000;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;
        private readonly IHandCountService _handCountService;
        private readonly IGamePlayState _gameState;

        private readonly long _minimumRequiredCredits;
        private readonly TimeSpan _currentIdleTimePeriod;
        private bool _disposed;
        private bool _idle;
        private bool _isPlayable;
        private DateTime _lastAction = DateTime.UtcNow;

        private Timer _timer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HandCountResetService" /> class.
        /// </summary>
        /// <param name="systemDisableManager">An instance of <see cref="ISystemDisableManager" />.</param>
        /// <param name="properties">An instance of <see cref="IPropertiesManager" />.</param>
        /// <param name="eventBus">An instance of <see cref="IEventBus" />.</param>
        /// <param name="handCountService">An instance of <see cref="IHandCountService" />.</param>
        /// <param name="gameState">An instance of <see cref="IGamePlayState" />.</param>
        public HandCountResetService(
            ISystemDisableManager systemDisableManager,
            IPropertiesManager properties,
            IEventBus eventBus,
            IHandCountService handCountService,
            IGamePlayState gameState)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _handCountService = handCountService ?? throw new ArgumentNullException(nameof(handCountService));
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));

            _isPlayable = !systemDisableManager.IsDisabled || !systemDisableManager.IsIdleStateAffected;

            _timer = new Timer(IdleTimerInterval);
            _timer.Elapsed += IdleTimerTicker;

            _minimumRequiredCredits = (long)_properties.GetProperty(AccountingConstants.HandCountMinimumRequiredCredits,
                AccountingConstants.HandCountDefaultRequiredCredits);

            _currentIdleTimePeriod = TimeSpan.FromMilliseconds(
                _properties.GetValue(
                    AccountingConstants.HandCountResetIdleTimePeriod,
                    AccountingConstants.DefaultHandCountResetIdleTimeoutPeriod));

            Initialize();
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IHandCountResetService) };

        /// <inheritdoc />
        public TimeSpan IdleTime => DateTime.UtcNow - _lastAction;

        public bool TimerDialogVisible { get; set; }

        /// <inheritdoc />
        public bool Idle
        {
            get => _idle;

            private set
            {
                if (_idle == value)
                {
                    return;
                }

                _idle = value;

                if (_idle && !TimerDialogVisible)
                {
                    Logger.Info("Hand Count reset idle period elapsed.");
                    TimerDialogVisible = true;
                    _eventBus.Publish(new HandCountResetTimerStartedEvent());
                }

                if (!_idle && TimerDialogVisible)
                {
                    Logger.Info("Hand Count reset dialog cancelled.");
                    TimerDialogVisible = false;
                    _eventBus.Publish(new HandCountResetTimerCancelledEvent());
                }
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases allocated resources.
        /// </summary>
        /// <param name="disposing">
        ///     true to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
                _timer.Dispose();
            }

            _timer = null;

            _disposed = true;
        }

        private void CheckForIdle()
        {
            if (NeedsReset())
            {
                if (!_timer.Enabled)
                {
                    _timer.Start();

                    Logger.Info($"Hand count reset is needed. Idle timer started. Expires in {_currentIdleTimePeriod - IdleTime}");
                }
            }
            else
            {
                Idle = false;

                if (_timer.Enabled)
                {
                    _timer.Stop();
                }

                Logger.Info("Hand count reset not needed.");
            }
        }

        private void HandleActivity()
        {
            if (TimerDialogVisible)
            {
                if (!NeedsReset())
                {
                    _lastAction = DateTime.UtcNow;
                    Idle = false;
                }
            }
            else
            {
                _lastAction = DateTime.UtcNow;
                Idle = false;
                Logger.Debug($"Hand count reset is not idle: activity @ {_lastAction}.");

                CheckForIdle();
            }
        }

        private bool NeedsReset()
        {
            if ((!_gameState.Idle && !_gameState.InPresentationIdle) || !_isPlayable)
            {
                return false;
            }

            var balance = (long)_properties.GetProperty(PropertyKey.CurrentBalance, 0L);
            return (balance > 0 && balance < _minimumRequiredCredits) || (balance == 0 && _handCountService.HandCount > 0);
        }

        private bool HasIdleTimeElapsed()
        {
            return IdleTime >= _currentIdleTimePeriod;
        }

        private void IdleTimerTicker(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (!NeedsReset())
            {
                Logger.Debug("IdleTimerTicker: No need to reset. Stop timer.");

                _timer.Stop();
                return;
            }

            Logger.Debug($"IdleTimerTicker: Idle time {IdleTime}");
            if (HasIdleTimeElapsed())
            {
                Logger.Debug("Idle time has elapsed. Setting to idle");

                SetIdle();
            }
        }

        private void SetIdle()
        {
            _timer.Stop();

            Idle = true;
        }

        public void Initialize()
        {
            Logger.Debug($"Hand count reset idle time period is {_currentIdleTimePeriod}");

            _eventBus.Subscribe<BankBalanceChangedEvent>(this, _ => HandleActivity());
            _eventBus.Subscribe<GameIdleEvent>(this, _ => HandleActivity());
            _eventBus.Subscribe<GameSelectedEvent>(this, _ => HandleActivity());
            _eventBus.Subscribe<GameProcessExitedEvent>(this, _ => HandleActivity());
            
            _eventBus.Subscribe<UserInteractionEvent>(this, _ => _lastAction = DateTime.UtcNow);
            _eventBus.Subscribe<DownEvent>(this, _ => _lastAction = DateTime.UtcNow);

            _eventBus.Subscribe<HandCountResetTimerElapsedEvent>(this, _ => TimerDialogVisible = false);

            _eventBus.Subscribe<SystemDisableRemovedEvent>(
                this,
                evt =>
                {
                    _isPlayable = !evt.SystemDisabled || !evt.SystemIdleStateAffected;
                    Idle = false;
                    _lastAction = DateTime.UtcNow;
                    CheckForIdle();
                });
            _eventBus.Subscribe<SystemDisableAddedEvent>(
                this,
                evt =>
                {
                    _isPlayable = !evt.SystemIdleStateAffected;
                    Idle = false;
                    _lastAction = DateTime.UtcNow;
                    CheckForIdle();
                });

            CheckForIdle();
        }
    }
}
