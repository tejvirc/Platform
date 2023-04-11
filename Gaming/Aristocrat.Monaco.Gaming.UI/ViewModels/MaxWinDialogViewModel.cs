namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using Aristocrat.Monaco.Accounting.Contracts.HandCount;
    using Aristocrat.Monaco.Gaming.UI.Events;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.UI.Common;
    using Aristocrat.MVVM.ViewModel;
    using System;
    using System.Linq;

    public class MaxWinDialogViewModel : BaseViewModel
    {
        private const double initialTimeSeconds = 5.0;
        private const double resetTimerIntervalSeconds = 1.0;

        private TimeSpan oneSecondElapsed = TimeSpan.FromSeconds(resetTimerIntervalSeconds);
        private readonly IEventBus _eventBus;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly DispatcherTimerAdapter _resetTimer;
        private bool _showDialog;
        private long _maxWinAmount;
        private bool _disposed = false;

        /// <summary>
        /// MaxWin reached dialog will be shown if true
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
                _eventBus.Publish(new MaxWinOverlayVisibilityChangedEvent(_showDialog));
                RaisePropertyChanged(nameof(ShowDialog));
            }
        }

        public long MaxWinAmount
        {
            get
            {
                return _maxWinAmount;
            }
            set
            {
                _maxWinAmount = value;
                RaisePropertyChanged(nameof(MaxWinAmount));
            }
        }
        /// <summary>
        /// Holds the time left to show the max win reached pop up
        /// </summary>
        public TimeSpan TimeLeft { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxWinDialogViewModel" /> class.
        /// </summary>
        public MaxWinDialogViewModel() : this(ServiceManager.GetInstance().TryGetService<IEventBus>(),
                                                      ServiceManager.GetInstance().TryGetService<ISystemDisableManager>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxWinDialogViewModel" /> class.
        /// </summary>
        /// <param name="eventBus">Event bus</param>
        /// <param name="systemDisableManager">System disable Manager</param>
        public MaxWinDialogViewModel(IEventBus eventBus, ISystemDisableManager systemDisableManager)
        {
            _eventBus = eventBus;
            _systemDisableManager = systemDisableManager;
            _resetTimer = new DispatcherTimerAdapter() { Interval = oneSecondElapsed };
            TimeLeft = TimeSpan.FromSeconds(initialTimeSeconds);
            _resetTimer.Tick += resetTimer_Tick;
            MaxWinAmount = 100;
            // _eventBus.Subscribe<MaxWinReachedEvent>(this, Handle);

            _eventBus.Subscribe<HandCountResetTimerStartedEvent>(this, Handle);
        }

        private void Handle(HandCountResetTimerStartedEvent evt)
        {
            ShowDialog = true;
            // Start Timer
            TimeLeft = TimeSpan.FromSeconds(initialTimeSeconds);
            _resetTimer.Start();
            _resetTimer.IsEnabled = true;
        }
        //private void Handle(MaxWinReachedEvent evt)
        //{
        //    ShowDialog = true;
        //    // Start Timer
        //    TimeLeft = TimeSpan.FromSeconds(initialTimeSeconds);
        //    _resetTimer.Start();
        //    _resetTimer.IsEnabled = true;
        //}

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
