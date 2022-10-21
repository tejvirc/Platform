namespace Aristocrat.Monaco.Gaming.UI.Views.Overlay
{
    using System;
    using System.Windows;
    using ViewModels;

    /// <summary>
    ///     Interaction logic for OverlayWindow.xaml
    /// </summary>
    public partial class OverlayWindow
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OverlayWindow" /> class.
        /// </summary>
        public OverlayWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Dependency property for <see cref="ReplayNavigationBarHeight"/>.
        /// </summary>
        public static readonly DependencyProperty ReplayNavigationBarHeightProperty =
            DependencyProperty.Register(nameof(ReplayNavigationBarHeight), typeof(double), typeof(OverlayWindow),
                new PropertyMetadata(0d));

        /// <summary>
        /// The replay navigation bar's current height (including DPI scaling).
        /// </summary>
        public double ReplayNavigationBarHeight
        {
            get => (double)GetValue(ReplayNavigationBarHeightProperty);
            private set => SetValue(ReplayNavigationBarHeightProperty, value);
        }

        /// <summary>
        ///     Gets or sets the view model
        /// </summary>
        public LobbyViewModel ViewModel
        {
            get => DataContext as LobbyViewModel;

            set => DataContext = value;
        }

        //private void MessageOverlay_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    var isVisible = (bool)e.NewValue;
        //    if (isVisible)
        //    {
        //        Top = _parent.Top;
        //        Left = _parent.Left;

        //        Width = _parent.Width;
        //        Height = _parent.Height;

        //        WindowState = _parent.WindowState;
        //    }
        //}

        private void FadeOutStoryboard_OnCompleted(object sender, EventArgs e)
        {
            ViewModel.IsMessageOverlayDlgFadingOut = false;
        }

        /// <summary>
        /// Reacts to the replay navigation bar's size.
        /// </summary>
        private void OnReplayNavigationBarSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ReplayNavigationBarHeight = Math.Ceiling(e.NewSize.Height * HighDpiBehavior.Scale);
        }
    }
}