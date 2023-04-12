namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System;
    using System.Linq;
    using Accounting.HandCount;
    using Aristocrat.Monaco.Accounting.Contracts.HandCount;
    using Aristocrat.Monaco.Gaming.UI.Events;
    using Kernel;
    using Monaco.UI.Common;
    using MVVM.ViewModel;

    /// <summary>
    ///  Defines the HandCountTimerDialogViewModel class
    /// </summary>
    public class HandCountTimerDialogViewModel : BaseViewModel, IDisposable
    {
        private const double initialTimeSeconds = 45.0;
        private const double resetTimerIntervalSeconds = 1.0;
        private TimeSpan oneSecondElapsed = TimeSpan.FromSeconds(resetTimerIntervalSeconds);
        private readonly IEventBus _eventBus;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly DispatcherTimerAdapter _resetTimer;
        private IHandCountService _handCountService;
        private bool _showDialog;
        private TimeSpan _timeLeft;
        private bool _disposed = false;

        /// <summary>
        /// HandCount timer dialog will be shown if true
        /// </summary>
        public bool ShowDialog
        {
            get
            {
                return _showDialog;
            }
            set
            {
                _showDialog = value;
                _eventBus.Publish(new HandCountTimerOverlayVisibilityChangedEvent(_showDialog));
                RaisePropertyChanged(nameof(ShowDialog));
            }
        }


        /// <summary>
        /// Displays time left in handcount timer dialog
        /// </summary>
        public TimeSpan TimeLeft
        {
            get
            {
                return _timeLeft;
            }
            set
            {
                _timeLeft = value;
                RaisePropertyChanged(nameof(TimeLeft));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandCountTimerDialogViewModel" /> class.
        /// </summary>
        public HandCountTimerDialogViewModel() : this(ServiceManager.GetInstance().TryGetService<IEventBus>(),
                                                      ServiceManager.GetInstance().TryGetService<ISystemDisableManager>(),
                                                      ServiceManager.GetInstance().TryGetService<IHandCountService>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandCountTimerDialogViewModel" /> class.
        /// </summary>
        /// <param name="eventBus">Event bus</param>
        /// <param name="systemDisableManager">System disable Manager</param>
        /// <param name="handCountService"> HandCount service</param>
        public HandCountTimerDialogViewModel(IEventBus eventBus, ISystemDisableManager systemDisableManager, IHandCountService handCountService)
        {
            _eventBus = eventBus;
            _systemDisableManager = systemDisableManager;
            _handCountService = handCountService;
            _resetTimer = new DispatcherTimerAdapter() { Interval = oneSecondElapsed };
            TimeLeft = TimeSpan.FromSeconds(initialTimeSeconds);
            _resetTimer.Tick += resetTimer_Tick;
            SetupTimerCallbacks();
            //_eventBus.Subscribe<HandCountResetTimerStartedEvent>(this, Handle);
        }

        //private void Handle(HandCountResetTimerStartedEvent obj)
        //{
        //    OnHandCountTimerStarted();
        //}

        private void SetupTimerCallbacks()
        {
            _handCountService.OnResetTimerStarted += OnHandCountTimerStarted;
            _handCountService.OnResetTimerCancelled += OnHandCountTimerCancelled;
        }

        private void OnHandCountTimerCancelled()
        {
            HideTimerDialog();
        }

        private void OnHandCountTimerStarted()
        {
            ShowDialog = true;
            // Start Timer
            TimeLeft = TimeSpan.FromSeconds(initialTimeSeconds);
            _resetTimer.Start();
            _resetTimer.IsEnabled = true;
        }

        private void resetTimer_Tick(object sender, EventArgs e)
        {
            TimeLeft = TimeLeft.Subtract(oneSecondElapsed);
            if (OtherLockupsExist())
            {
                HideTimerDialog();
                return;
            }

            if (TimeLeft.Seconds == 0 && TimeLeft.Minutes == 0)
            {
                HideTimerDialog();
                _handCountService.HandCountResetTimerElapsed();
            }
        }

        private void HideTimerDialog()
        {
            if (_resetTimer.IsEnabled)
            {
                ResetAndDisableTimer();
                ShowDialog = false;
            }
        }

        private void ResetAndDisableTimer()
        {
            _resetTimer.Stop();
            _resetTimer.IsEnabled = false;
        }

        private bool OtherLockupsExist()
        {
            return _systemDisableManager.CurrentDisableKeys.Any();
        }

        /// <summary>
        ///  Dispose
        /// </summary>
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Cleanup.
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
                _eventBus?.UnsubscribeAll(this);
                _resetTimer?.Stop();
                ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);
            }
            _disposed = true;
        }
    }
}
