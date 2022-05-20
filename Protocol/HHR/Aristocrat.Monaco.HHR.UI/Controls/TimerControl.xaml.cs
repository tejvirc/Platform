namespace Aristocrat.Monaco.Hhr.UI.Controls
{
    using System;
    using System.Timers;
    using System.Windows;
    using System.Windows.Input;
    using Kernel;
    using ManagedBink;
    using Gaming.Contracts;

    /// <summary>
    /// Interaction logic for TimerControl.xaml
    /// </summary>
    public partial class TimerControl : IDisposable
    {
        private bool _disposed;
        private readonly IEventBus _eventBus;

        /// <summary>
        ///     Constructor
        /// </summary>
        public TimerControl()
        {
            InitializeComponent();

            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            if (_eventBus != null)
            {
                _eventBus.Subscribe<GamePlayDisabledEvent>(this, OnGamePlayDisabled);
                _eventBus.Subscribe<GamePlayEnabledEvent>(this, OnGamePlayEnabled);
            }

            SetupBinkTimer();
        }

        /// <summary>
        ///     Timeout --Maximum time, after that timer would expire
        /// </summary>
        public int Timeout
        {
            get => (int)GetValue(TimeoutProperty);
            set => SetValue(TimeoutProperty, value);
        }

        /// <summary>
        ///     Dependency property for Time out.
        /// </summary>
        public static readonly DependencyProperty TimeoutProperty =
            DependencyProperty.Register(
                nameof(Timeout),
                typeof(int),
                typeof(TimerControl),
                new PropertyMetadata(1, OnTimeoutChanged));


        /// <summary>
        ///     IsQuickPickTextVisible --To make title of timer visible/hidden for Quick-Pick
        /// </summary>
        public bool IsQuickPickTextVisible
        {
            get => (bool)GetValue(IsQuickPickTextVisibleProperty);
            set => SetValue(IsQuickPickTextVisibleProperty, value);
        }

        /// <summary>
        ///     Dependency property for IsQuickPickTextVisible
        /// </summary>
        public static readonly DependencyProperty IsQuickPickTextVisibleProperty =
            DependencyProperty.Register(
                nameof(IsQuickPickTextVisible),
                typeof(bool),
                typeof(TimerControl),
                new PropertyMetadata(false));

        /// <summary>
        ///     IsAutoPickTextVisible --To make title of timer visible/hidden for Auto-Pick
        /// </summary>
        public bool IsAutoPickTextVisible
        {
            get => (bool)GetValue(IsAutoPickTextVisibleProperty);
            set => SetValue(IsAutoPickTextVisibleProperty, value);
        }

        /// <summary>
        ///     Dependency property for IsAutoPickTextVisible
        /// </summary>
        public static readonly DependencyProperty IsAutoPickTextVisibleProperty =
            DependencyProperty.Register(
                nameof(IsAutoPickTextVisible),
                typeof(bool),
                typeof(TimerControl),
                new PropertyMetadata(false));

        /// <summary>
        ///     Enable/disable the timer
        /// </summary>
        public bool TimerEnabled
        {
            get => (bool)GetValue(TimerEnabledProperty);
            set => SetValue(TimerEnabledProperty, value);
        }

        /// <summary>
        ///     Dependency property for TimerEnabled
        /// </summary>
        public static readonly DependencyProperty TimerEnabledProperty =
            DependencyProperty.Register(
                nameof(TimerEnabled),
                typeof(bool),
                typeof(TimerControl),
                new PropertyMetadata(false));

        /// <summary>
        ///     Dependency property for OnTimerElapsedHandler
        /// </summary>
        public static readonly DependencyProperty OnTimerElapsedHandlerProperty =
            DependencyProperty.Register(
                nameof(OnTimerElapsedHandler),
                typeof(ICommand),
                typeof(TimerControl));

        /// <summary>
        ///     TimerElapsedCommand --Command to execute after timer expires
        /// </summary>
        public ICommand OnTimerElapsedHandler
        {
            get => (ICommand)GetValue(OnTimerElapsedHandlerProperty);

            set => SetValue(OnTimerElapsedHandlerProperty, value);
        }

        /// <summary>
        ///     Dependency property for OnUnitTimeElapsedHandle
        /// </summary>
        public static readonly DependencyProperty OnTimerTickHandlerProperty =
            DependencyProperty.Register(
                nameof(OnTimerTickHandler),
                typeof(ICommand),
                typeof(TimerControl));

        /// <summary>
        ///     The handler to invoke when unit time is elapsed
        /// </summary>
        public ICommand OnTimerTickHandler
        {
            get => (ICommand)GetValue(OnTimerTickHandlerProperty);

            set => SetValue(OnTimerTickHandlerProperty, value);
        }

        /// <summary>
        ///     Dependency property for Counter
        /// </summary>
        public static readonly DependencyProperty CounterProperty =
            DependencyProperty.Register(
                "Counter",
                typeof(int),
                typeof(TimerControl),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        ///     Counter -- represent the counter inside the timer which would decrease as the time passes
        /// </summary>
        public int Counter
        {
            get => (int)GetValue(CounterProperty);
            set => SetValue(CounterProperty, value);
        }

        /// <summary>
        ///     Timer to start/stop
        /// </summary>
        private static Timer _timer;

        private BinkGpuControl TimerVideo { get; set; }

        private void SetupBinkTimer()
        {
            BinkTimer.Children.Clear();

            TimerVideo = new BinkGpuControl
            {
                Name = "TimerVideo",
                Height = 150,
                Width = 130,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Filename = "./Resources/Timer.bk2",
                LoopVideo = true,
                ShowFirstFrameOnLoad = true
            };

            BinkTimer.Children.Add(TimerVideo);
        }

        /// <summary>
        ///     OnTimeoutChange -- api to execute whenever property Timeout Changes while switching b/w different pages.
        /// </summary>
        private static void OnTimeoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var timerControl = (TimerControl)d;

            if (timerControl.Timeout == 0)
            {
                return;
            }

            timerControl.SetupBinkTimer();
            timerControl.Counter = timerControl.Timeout;

            _timer?.Stop();
            _timer = null;
            _timer = new Timer(1000);
            _timer.Elapsed += (sender, x) =>
            {
                Application.Current.Dispatcher.Invoke(
                    () =>
                    {
                        timerControl.OnTimerTickHandler?.Execute(null);

                        if (timerControl.Counter == 1)
                        {
                            if (_timer != null)
                            {
                                _timer.Stop();
                                _timer = null;
                            }

                            timerControl.OnTimerElapsedHandler?.Execute(null);
                        }
                        else
                        {
                            timerControl.Counter--;
                        }
                    });
            };
            _timer.AutoReset = true;
            _timer.Enabled = timerControl.TimerEnabled;
        }

        private void OnGamePlayDisabled(GamePlayDisabledEvent theEvent)
        {
            _timer?.Stop();

            Dispatcher.Invoke(() =>
            {
                TimerVideo.VideoState = BinkVideoState.Paused;
            });
        }

        private void OnGamePlayEnabled(GamePlayEnabledEvent theEvent)
        {
            _timer?.Start();

            Dispatcher.Invoke(() =>
            {
                TimerVideo.VideoState = BinkVideoState.Playing;
            });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _timer?.Dispose();
                _eventBus.UnsubscribeAll(this);
            }

            _timer = null;
            _disposed = true;
        }
    }

}