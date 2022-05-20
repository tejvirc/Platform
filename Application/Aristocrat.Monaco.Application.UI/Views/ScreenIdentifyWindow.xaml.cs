namespace Aristocrat.Monaco.Application.UI.Views
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows;

    /// <summary>
    ///     A simple blank screen with a large number at the center representing the Id number of a screen. This is used to
    ///     help identify screen mapping/order.
    /// </summary>
    [CLSCompliant(false)]
    public partial class ScreenIdentifyWindow
    {
        public static readonly DependencyProperty FlashWindowProperty = DependencyProperty.Register(
            nameof(FlashWindow),
            typeof(bool),
            typeof(ScreenIdentifyWindow),
            new PropertyMetadata(default(bool)));

        private readonly int _deviceNumber;
        private readonly bool _flashWindow;
        private readonly Rectangle _screenBounds;
        private readonly Rectangle _viewableAreaWithinWindow;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ScreenIdentifyWindow"/> class.
        /// </summary>
        /// <param name="deviceNumber">The screen device number.</param>
        /// <param name="flashWindow">if set to <c>true</c> [flash window].</param>
        /// <param name="screenBounds">The screen bounds.</param>
        /// <param name="viewableAreaWithinWindow">Viewable area rectangle with origin point at top left of window.</param>
        public ScreenIdentifyWindow(
            int deviceNumber,
            bool flashWindow,
            Rectangle screenBounds,
            Rectangle viewableAreaWithinWindow)
        {
            InitializeComponent();

            _deviceNumber = deviceNumber;
            _flashWindow = flashWindow;
            _screenBounds = screenBounds;
            _viewableAreaWithinWindow = viewableAreaWithinWindow;

            Topmost = true;
            ContentView.Visibility = Visibility.Visible;

            Loaded += OnLoaded;
        }

        public bool FlashWindow
        {
            get => (bool)GetValue(FlashWindowProperty);
            set => SetValue(FlashWindowProperty, value);
        }

        /// <inheritdoc />
        protected override void OnClosing(CancelEventArgs e)
        {
            Loaded -= OnLoaded;
            base.OnClosing(e);
        }

        protected void OnLoaded(object sender, RoutedEventArgs e)
        {
            DeviceNumberLabel.Content = _deviceNumber;
            FlashWindow = _flashWindow;

            WindowRoot.Width = _screenBounds.Width;
            WindowRoot.Height = _screenBounds.Height;
            WindowRoot.Left = _screenBounds.X;
            WindowRoot.Top = _screenBounds.Y;
            LabelContainer.Width = _viewableAreaWithinWindow.Width;
            LabelContainer.Height = _viewableAreaWithinWindow.Height;
            LabelLeftSpacer.Width = _viewableAreaWithinWindow.X;
            LabelTopSpacer.Height = _viewableAreaWithinWindow.Y;
        }
    }
}