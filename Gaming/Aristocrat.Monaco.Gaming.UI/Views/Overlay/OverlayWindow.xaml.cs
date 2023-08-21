namespace Aristocrat.Monaco.Gaming.UI.Views.Overlay
{
    using System;
    using System.Windows;
    using MahApps.Metro.Controls;
    using ViewModels;

    /// <summary>
    ///     Interaction logic for OverlayWindow.xaml
    /// </summary>
    public partial class OverlayWindow
    {
        private readonly MetroWindow _parent;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OverlayWindow" /> class.
        /// </summary>
        /// <param name="parent">Parent window</param>
        public OverlayWindow(MetroWindow parent)
        {
            InitializeComponent();

            // MetroApps issue--need to set in code behind after InitializeComponent.
            AllowsTransparency = true;

            _parent = parent;

            IsVisibleChanged += MessageOverlay_IsVisibleChanged;
        }

        /// <summary>
        /// Dependency property for <see cref="IsDialogFadingOut"/>.
        /// </summary>
        public static readonly DependencyProperty IsDialogFadingOutProperty =
            DependencyProperty.Register(nameof(IsDialogFadingOut), typeof(bool), typeof(OverlayWindow),
                new PropertyMetadata(false));

        /// <summary>
        /// Is this dialog in its fade-out animation?
        /// </summary>
        public bool IsDialogFadingOut
        {
            get => (bool)GetValue(IsDialogFadingOutProperty);
            private set => SetValue(IsDialogFadingOutProperty, value);
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

        private void MessageOverlay_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var isVisible = (bool)e.NewValue;
            if (isVisible)
            {
                Top = _parent.Top;
                Left = _parent.Left;

                Width = _parent.Width;
                Height = _parent.Height;

                WindowState = _parent.WindowState;
            }
        }

        private void FadeOutStoryboard_OnCompleted(object sender, EventArgs e)
        {
            if (!ViewModel.MessageOverlayDisplay.IsOverlayWindowVisible)
            {
                Hide();
            }

            IsDialogFadingOut = false;
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