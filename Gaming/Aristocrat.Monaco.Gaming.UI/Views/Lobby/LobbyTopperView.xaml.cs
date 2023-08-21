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
    ///     Interaction logic for LobbyTopperView.xaml
    /// </summary>
    public partial class LobbyTopperView
    {
        private LayoutOverlayWindow _topperMediaDisplayWindow;
        private readonly WindowToScreenMapper _windowToScreenMapper = new WindowToScreenMapper(DisplayRole.Topper);

        /// <summary>
        ///     Initializes a new instance of the <see cref="LobbyTopperView" /> class.
        /// </summary>
        public LobbyTopperView()
        {
            InitializeComponent();

            var enabled = ServiceManager.GetInstance().GetService<IPropertiesManager>()
                .GetValue(ApplicationConstants.MediaDisplayEnabled, false);

            if (!enabled)
            {
                return;
            }

            SetupMediaDisplayWindow();
        }

        /// <summary>
        ///     Gets or sets the view model
        /// </summary>
        public LobbyViewModel ViewModel
        {
            get => DataContext as LobbyViewModel;
            set => DataContext = value;
        }

        public LayoutOverlayWindow GetMediaDisplayWindow()
        {
            return _topperMediaDisplayWindow;
        }

        public void SetupMediaDisplayWindow()
        {
            _topperMediaDisplayWindow = new LayoutOverlayWindow(ScreenType.Glass) { Title = "Topper Glass Overlay" };
            _topperMediaDisplayWindow.Loaded += TopperLayoutOverlayWindow_OnLoaded;

            var topWidthBinding = new Binding("Width") { Source = this, Mode = BindingMode.OneWay };

            _topperMediaDisplayWindow.SetBinding(WidthProperty, topWidthBinding);

            var topHeightBinding = new Binding("Height") { Source = this, Mode = BindingMode.OneWay };

            _topperMediaDisplayWindow.SetBinding(HeightProperty, topHeightBinding);

            _topperMediaDisplayWindow.Show();
        }

        private void LobbyTopperView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!(sender is FrameworkElement element))
            {
                return;
            }

            // Seems to be some interop issue with winforms control not being in sync with parent window.
            // So manually sync them.  We can't data bind with winform control.
            GameTopperWindowCtrl.Width = (int)element.ActualWidth;
            GameTopperWindowCtrl.Height = (int)element.ActualHeight;

            if (_topperMediaDisplayWindow != null)
            {
                _topperMediaDisplayWindow.Width = ActualWidth;
                _topperMediaDisplayWindow.Height = ActualHeight;
            }
        }

        private void WinHostCtrl_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.GameTopperHwnd = GameTopperWindowCtrl.Handle;
        }

        private void LobbyTopperView_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.DisplayChanged += ViewModel_OnDisplayChanged;
            _windowToScreenMapper.MapWindow(this);

            ShowTitleBar = !_windowToScreenMapper.IsFullscreen;
            ShowCloseButton = !_windowToScreenMapper.IsFullscreen;
            ShowMinButton = !_windowToScreenMapper.IsFullscreen;
            ShowMaxRestoreButton = !_windowToScreenMapper.IsFullscreen;
        }

        private void TopperLayoutOverlayWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _windowToScreenMapper.MapWindow(_topperMediaDisplayWindow);
        }

        private void ViewModel_OnDisplayChanged(object sender, EventArgs e)
        {
            Dispatcher?.BeginInvoke(
                new Action(
                    () =>
                    {
                        _windowToScreenMapper.MapWindow(this);
                        if (_topperMediaDisplayWindow != null)
                        {
                            _windowToScreenMapper.MapWindow(_topperMediaDisplayWindow);
                        }
                    }));
        }
        private void LobbyTopperView_OnClosed(object sender, EventArgs e)
        {
            SizeChanged -= LobbyTopperView_SizeChanged;
            ViewModel.DisplayChanged -= ViewModel_OnDisplayChanged;

            // This is explicit to ensure the D3D and Bink layer is/can be destroyed
            LobbyTopperVideo?.Dispose();

            _topperMediaDisplayWindow?.Close();
        }
    }
}