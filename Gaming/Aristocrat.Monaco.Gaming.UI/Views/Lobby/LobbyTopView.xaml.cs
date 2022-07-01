namespace Aristocrat.Monaco.Gaming.UI.Views.Lobby
{
    using System;
    using System.Windows;
    using System.Windows.Data;
    using Application.Contracts;
    using Application.Contracts.Media;
    using Application.UI.Views;
    using Cabinet.Contracts;
    using Kernel;
    using MediaDisplay;
    using ViewModels;

    /// <summary>
    ///     Interaction logic for LobbyTopView.xaml
    /// </summary>
    public partial class LobbyTopView
    {
        private LayoutOverlayWindow _topMediaDisplayWindow;
        private readonly WindowToScreenMapper _windowToScreenMapper = new WindowToScreenMapper(DisplayRole.Top);

        /// <summary>
        ///     Initializes a new instance of the <see cref="LobbyTopView" /> class.
        /// </summary>
        public LobbyTopView()
        {
            InitializeComponent();

            var enabled = ServiceManager.GetInstance().GetService<IPropertiesManager>()
                .GetValue(ApplicationConstants.MediaDisplayEnabled, false);
            SizeChanged += LobbyTopView_SizeChanged;

            if (!enabled)
            {
                return;
            }

            SetupMediaDisplayWindow();
        }

        /// <summary>
        /// Initializes the top screen overlay
        /// </summary>
        public void SetupMediaDisplayWindow()
        {
            _topMediaDisplayWindow = new LayoutOverlayWindow(ScreenType.Glass) { Title = "Top Glass Overlay" };
            _topMediaDisplayWindow.Loaded += TopLayoutOverlayWindow_OnLoaded;

            var topWidthBinding = new Binding("Width") { Source = this, Mode = BindingMode.OneWay };

            _topMediaDisplayWindow.SetBinding(WidthProperty, topWidthBinding);

            var topHeightBinding = new Binding("Height") { Source = this, Mode = BindingMode.OneWay };

            _topMediaDisplayWindow.SetBinding(HeightProperty, topHeightBinding);

            _topMediaDisplayWindow.Show();
        }

        /// <summary>
        ///     Gets or sets the view model
        /// </summary>
        public LobbyViewModel ViewModel
        {
            get => DataContext as LobbyViewModel;
            set => DataContext = value;
        }

        /// <summary>
        /// Returns the top screen overlay window
        /// </summary>
        /// <returns></returns>
        public LayoutOverlayWindow GetMediaDisplayWindow()
        {
            return _topMediaDisplayWindow;
        }

        private void LobbyTopView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Seems to be some interop issue with winforms control not being in sync with parent window.
            // So manually sync them.  We can't data bind with winform control.
            GameTopWindowCtrl.Width = (int)ActualWidth;
            GameTopWindowCtrl.Height = (int)ActualHeight;

            if (_topMediaDisplayWindow != null)
            {
                _topMediaDisplayWindow.Width = ActualWidth;
                _topMediaDisplayWindow.Height = ActualHeight;
            }
        }

        private void WinHostCtrl_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.GameTopHwnd = GameTopWindowCtrl.Handle;
        }

        private void LobbyTopView_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.DisplayChanged += ViewModel_OnDisplayChanged;
            _windowToScreenMapper.MapWindow(this);

            ShowTitleBar = !_windowToScreenMapper.IsFullscreen;
            ShowCloseButton = !_windowToScreenMapper.IsFullscreen;
            ShowMinButton = !_windowToScreenMapper.IsFullscreen;
            ShowMaxRestoreButton = !_windowToScreenMapper.IsFullscreen;
        }

        private void TopLayoutOverlayWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _windowToScreenMapper.MapWindow(_topMediaDisplayWindow);
        }

        private void ViewModel_OnDisplayChanged(object sender, EventArgs e)
        {
            Dispatcher?.BeginInvoke(
                new Action(
                    () =>
                    {
                        _windowToScreenMapper.MapWindow(this);
                        if (_topMediaDisplayWindow != null)
                        {
                            _windowToScreenMapper.MapWindow(_topMediaDisplayWindow);
                        }
                    }));
        }

        private void GameAttract_VideoCompleted(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.Config.BottomAttractVideoEnabled)
            {
                ViewModel.OnGameAttractVideoCompleted();
            }
        }

        private void LobbyTopView_OnClosed(object sender, EventArgs e)
        {
            SizeChanged -= LobbyTopView_SizeChanged;
            ViewModel.DisplayChanged -= ViewModel_OnDisplayChanged;

            // This is explicit to ensure the D3D and Bink layer is/can be destroyed
            GameAttract.Dispose();

            _topMediaDisplayWindow?.Close();
        }
    }
}