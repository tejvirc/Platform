namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System;
    using Accounting.Contracts;
    using Accounting.Contracts.HandCount;
    using CommunityToolkit.Mvvm.ComponentModel;
    using Contracts.HandCount;
    using Kernel;
    using Monaco.UI.Common;

    /// <summary>
    ///     Defines the HandCountTimerDialogViewModel class
    /// </summary>
    public class HandCountTimerDialogViewModel : ObservableObject, IDisposable
    {
        private const double InitialTimeSeconds = 45.0;
        private const double ResetTimerIntervalSeconds = 1.0;
        private readonly TimeSpan _oneSecondElapsed = TimeSpan.FromSeconds(ResetTimerIntervalSeconds);
        private readonly IEventBus _eventBus;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly DispatcherTimerAdapter _resetTimer;
        private IHandCountService _handCountService;
        private readonly IBank _bank;
        private TimeSpan _timeLeft;
        private bool _disposed = false;
        private long _residualAmount = 0;

        /// <summary>
        /// Displays time left in handcount timer dialog
        /// </summary>
        public TimeSpan TimeLeft
        {
            get => _timeLeft;
            set
            {
                _timeLeft = value;
                OnPropertyChanged(nameof(TimeLeft));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandCountTimerDialogViewModel" /> class.
        /// </summary>
        public HandCountTimerDialogViewModel()
            : this(ServiceManager.GetInstance().TryGetService<IEventBus>(),
                   ServiceManager.GetInstance().TryGetService<ISystemDisableManager>(),
                   ServiceManager.GetInstance().TryGetService<IHandCountService>(),
                   ServiceManager.GetInstance().TryGetService<IBank>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandCountTimerDialogViewModel" /> class.
        /// </summary>
        /// <param name="eventBus">Event bus</param>
        /// <param name="systemDisableManager">System disable Manager</param>
        /// <param name="handCountService"> HandCount service</param>
        /// <param name="bank"> Bank</param>
        public HandCountTimerDialogViewModel(IEventBus eventBus,
                                             ISystemDisableManager systemDisableManager,
                                             IHandCountService handCountService,
                                             IBank bank)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _systemDisableManager = systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _handCountService = handCountService ?? throw new ArgumentNullException(nameof(handCountService));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));

            _resetTimer = new DispatcherTimerAdapter() { Interval = _oneSecondElapsed };
            TimeLeft = TimeSpan.FromSeconds(InitialTimeSeconds);
            _resetTimer.Tick += resetTimer_Tick;
        }

        public void OnHandCountTimerCancelled()
        {
            HideTimerDialog();
        }

        public void OnHandCountTimerStarted()
        {
            // Start Timer
            TimeLeft = TimeSpan.FromSeconds(InitialTimeSeconds);
            _resetTimer.Start();
            _resetTimer.IsEnabled = true;

            _residualAmount = _bank.QueryBalance();
        }

        /// <summary>
        ///     Dispose of unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Will unsubscribe from all event bus events when disposing, 
        ///     stop timer and unsubscribe tick event
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
                _eventBus.UnsubscribeAll(this);
                _resetTimer.Tick -= resetTimer_Tick;
                _resetTimer.Stop();
            }

            _disposed = true;
        }

        private void resetTimer_Tick(object sender, EventArgs e)
        {
            TimeLeft = TimeLeft.Subtract(_oneSecondElapsed);

            if (TimeLeft.Seconds == 0 && TimeLeft.Minutes == 0)
            {
                HideTimerDialog();
                _eventBus.Publish(new HandCountResetTimerElapsedEvent(_residualAmount));
            }
        }

        private void HideTimerDialog()
        {
            if (_resetTimer.IsEnabled)
            {
                ResetAndDisableTimer();
            }
        }

        private void ResetAndDisableTimer()
        {
            _resetTimer.Stop();
            _resetTimer.IsEnabled = false;
        }
    }
}
