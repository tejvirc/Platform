namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Reflection;
    using System.Timers;
    using Accounting.Contracts;
    using Contracts;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.Touch;
    using Kernel;
    using log4net;

    /// <summary>
    ///     An implementation of <see cref="ICabinetState" />
    /// </summary>
    public class CabinetState : ICabinetState, IDisposable
    {
        private const double IdleTimerInterval = 1000;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IBank _bank;
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;

        private bool _disposed;
        private bool _idle;
        private bool _isPlayable;
        private DateTime _lastAction;

        private Timer _timer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CabinetState" /> class.
        /// </summary>
        /// <param name="bank">An instance of <see cref="IBank" />.</param>
        /// <param name="systemDisableManager">An instance of <see cref="ISystemDisableManager" />.</param>
        /// <param name="properties">An instance of <see cref="IPropertiesManager" />.</param>
        /// <param name="eventBus">An instance of <see cref="IEventBus" />.</param>
        public CabinetState(
            IBank bank,
            ISystemDisableManager systemDisableManager,
            IPropertiesManager properties,
            IEventBus eventBus)
        {
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _isPlayable = !systemDisableManager.IsDisabled || !systemDisableManager.IsIdleStateAffected;

            _timer = new Timer(IdleTimerInterval);
            _timer.Elapsed += IdleTimerElapsed;

            Initialize();
        }

        private TimeSpan CurrentIdleTimePeriod => TimeSpan.FromMilliseconds(
            _properties.GetValue(
                GamingConstants.IdleTimePeriod,
                (int)GamingConstants.DefaultIdleTimeoutPeriod.TotalMilliseconds));

        /// <inheritdoc />
        public TimeSpan IdleTime => DateTime.UtcNow - _lastAction;

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

                if (_idle)
                {
                    Logger.Info("Cabinet state changed to idle.");

                    _eventBus.Publish(new CabinetIdleEvent());
                }
                else
                {
                    _eventBus.Publish(new CabinetNotIdleEvent());
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
            if (IsIdle())
            {
                SetIdle();
            }
            else if (HasZeroCredits() && _isPlayable)
            {
                if (!_timer.Enabled)
                {
                    _timer.Start();

                    Logger.Info(
                        $"Cabinet has zero credits and is playable.  Idle timer started.  Expires in {CurrentIdleTimePeriod - IdleTime}");
                }
            }
            else
            {
                Idle = false;

                _timer.Stop();

                if (!HasZeroCredits())
                {
                    Logger.Info("Cabinet is not idle: has credits.");
                }

                if (!_isPlayable)
                {
                    Logger.Info("Cabinet is not playable.");
                }
            }
        }

        private void HandleActivity(bool updateTime = true)
        {
            if (updateTime)
            {
                _lastAction = DateTime.UtcNow;
            }

            Idle = false;

            Logger.Debug($"Cabinet is not idle: activity @ {_lastAction}.");

            CheckForIdle();
        }

        private bool HasZeroCredits()
        {
            return _bank.QueryBalance() == 0;
        }

        private bool HasIdleTimeElapsed()
        {
            return IdleTime >= CurrentIdleTimePeriod;
        }

        private bool IsIdle()
        {
            return _isPlayable && HasZeroCredits() && HasIdleTimeElapsed();
        }

        private void IdleTimerElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (HasIdleTimeElapsed())
            {
                Logger.Debug("Idle timer has lapsed. Setting cabinet state to idle");

                SetIdle();
            }
        }

        private void SetIdle()
        {
            _timer.Stop();

            Idle = true;
        }

        private void Initialize()
        {
            Logger.Debug($"Cabinet state idle time period is {CurrentIdleTimePeriod}");

            _eventBus.Subscribe<BankBalanceChangedEvent>(this, _ => HandleActivity());
            _eventBus.Subscribe<GameIdleEvent>(this, _ => HandleActivity());
            _eventBus.Subscribe<GameSelectedEvent>(this, _ => HandleActivity());
            _eventBus.Subscribe<TouchEvent>(this, _ => HandleActivity());
            _eventBus.Subscribe<DownEvent>(this, _ => HandleActivity());

            // The following events only check for idle without resetting the idle timer
            _eventBus.Subscribe<SystemDisableRemovedEvent>(
                this,
                evt =>
                {
                    _isPlayable = !evt.SystemDisabled || !evt.SystemIdleStateAffected;
                    CheckForIdle();
                });
            _eventBus.Subscribe<SystemDisableAddedEvent>(
                this,
                evt =>
                {
                    _isPlayable = !evt.SystemIdleStateAffected;
                    CheckForIdle();
                });

            CheckForIdle();
        }
    }
}