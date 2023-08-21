namespace Aristocrat.Monaco.Gaming.VideoLottery.ScreenSaver
{
    using System;
    using System.Reflection;
    using System.Threading;
    using Accounting.Contracts;
    using Application.Contracts.Operations;
    using Application.Contracts.OperatorMenu;
    using Contracts;
    using Kernel;
    using log4net;
    using Vgt.Client12.Application.OperatorMenu;

    public class ScreenSaverMonitor : IDisposable
    {
        private const int DelayToDisplayScreensaver = 5 * 60; //delay 5 minutes
        private const int RebootDelayToDisplayScreensaver = 45; //45 seconds

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IPlayerBank _bank;
        private readonly IEventBus _eventBus;
        private readonly IGamePlayState _gamePlayState;
        private readonly IGameRecovery _gameRecovery;
        private readonly object _lock = new object();
        private readonly IOperatingHoursMonitor _operatingHours;

        private readonly IOperatorMenuLauncher _operatorMenu;
        private bool _active;

        private Timer _delay;

        private bool _disposed;
        private bool _starting;

        public ScreenSaverMonitor(
            IEventBus eventBus,
            IGamePlayState gamePlayState,
            IPlayerBank bank,
            IGameRecovery gameRecovery,
            IOperatingHoursMonitor operatingHours,
            IOperatorMenuLauncher operatorMenu)
        {
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _gameRecovery = gameRecovery ?? throw new ArgumentNullException(nameof(gameRecovery));
            _operatingHours = operatingHours ?? throw new ArgumentNullException(nameof(operatingHours));
            _operatorMenu = operatorMenu ?? throw new ArgumentNullException(nameof(operatorMenu));

            _eventBus.Subscribe<BankBalanceChangedEvent>(this, _ => TryStartCountdown());
            _eventBus.Subscribe<OperatingHoursEnabledEvent>(this, _ => Hide());
            _eventBus.Subscribe<OperatingHoursExpiredEvent>(this, _ => TryStartCountdown());
            _eventBus.Subscribe<OperatorMenuEnteredEvent>(this, _ => Hide());
            _eventBus.Subscribe<OperatorMenuExitedEvent>(this, _ => TryStartCountdown());
            _eventBus.Subscribe<GamePlayEnabledEvent>(this, _ => Hide());
            _eventBus.Subscribe<GamePlayDisabledEvent>(this, _ => TryStartCountdown());

            Logger.Info("Screensaver Monitor Initialized");

            _delay = new Timer(OnDelayExpired, null, Timeout.Infinite, Timeout.Infinite);

            if (CanShow())
            {
                TryStartCountdown(RebootDelayToDisplayScreensaver);
            }
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
                _delay.Dispose();
                _eventBus.UnsubscribeAll(this);
            }

            _delay = null;

            _disposed = true;
        }

        private void TryStartCountdown(int delaySecond = DelayToDisplayScreensaver)
        {
            lock (_lock)
            {
                if (_starting || _active || !CanShow())
                {
                    return;
                }

                _starting = true;
                _delay.Change(TimeSpan.FromSeconds(delaySecond), Timeout.InfiniteTimeSpan);
            }
        }

        private void OnDelayExpired(object state)
        {
            lock (_lock)
            {
                _starting = false;
                Show();
            }
        }

        private void Show()
        {
            Logger.Debug("Show method called");
            lock (_lock)
            {
                _active = true;
                _eventBus.Publish(new ScreenSaverVisibilityEvent(true));
            }
        }

        private void Hide()
        {
            Logger.Debug("Hide method called");
            lock (_lock)
            {
                _delay.Change(Timeout.Infinite, Timeout.Infinite);
                _starting = false;

                if (!_active)
                {
                    return;
                }

                _active = false;
                _eventBus.Publish(new ScreenSaverVisibilityEvent(false));
            }
        }

        private bool CanShow()
        {
            return !IsPlaying() && !OperatorMenuActive() && !IsPlayable() && OutsideOperatingHours();
        }

        private bool IsPlayable()
        {
            return _bank.Balance > 0 || _gamePlayState.Enabled;
        }

        private bool IsPlaying()
        {
            return _gameRecovery.IsRecovering || _gamePlayState.InGameRound;
        }

        private bool OutsideOperatingHours()
        {
            return _operatingHours.OutsideOperatingHours;
        }

        private bool OperatorMenuActive()
        {
            return _operatorMenu.IsShowing;
        }
    }
}