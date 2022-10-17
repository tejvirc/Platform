namespace Aristocrat.Monaco.Gaming.UI.Views.Lobby
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Interop;
    using Application.Contracts;
    using Application.Contracts.Media;
    using Application.UI.Views;
    using ButtonDeck;
    using Cabinet.Contracts;
    using Common;
    using Contracts;
    using Contracts.Events;
    using Hardware.Contracts;
    using Hardware.Contracts.ButtonDeck;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.Touch;
    using Kernel;
    using log4net;
    using ManagedBink;
    using MediaDisplay;
    using Monaco.UI.Common;
    using MVVM;
    using Overlay;
    using Utils;
    using ViewModels;
    using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
    using TouchAction = Hardware.Contracts.Touch.TouchAction;

    /// <summary>
    ///     Interaction logic for LobbyView.xaml
    /// </summary>
    public partial class LobbyView
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly WindowToScreenMapper _windowToScreenMapper = new WindowToScreenMapper(DisplayRole.Main);

        private readonly ICabinetDetectionService _cabinetDetectionService =
            ServiceManager.GetInstance().GetService<ICabinetDetectionService>();
        private readonly IEventBus _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
        private readonly IPropertiesManager _properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();

        private TestToolView _testToolView;

        private readonly OverlayManager _overlayManager;

        ////private readonly TimeLimitDialog _timeLimitDlg;
        ////private readonly MessageOverlay _msgOverlay;
        private readonly LobbyTopView _topView;
        private readonly LobbyTopperView _topperView;


        private VirtualButtonDeckOverlayView _vbdOverlay;
        private ButtonDeckSimulatorView _buttonDeckSimulator;

        private VirtualButtonDeckView _vbd;
        private bool _vbdLoaded;

        //private Dictionary<DisplayRole, (Action<UIElement> entryAction, Action<UIElement> exitAction)> _customOverlays;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LobbyView" /> class
        /// </summary>
        public LobbyView()
        {
            Logger.Debug("Initializing D3D");
            D3D.Init();

            Logger.Debug("Checking CEF");
            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var mediaEnabled = properties.GetValue(ApplicationConstants.MediaDisplayEnabled, false);
            if (mediaEnabled)
            {
                Logger.Debug("Initializing CEF");
                CefHelper.Initialize();
            }

            Logger.Debug("Initializing GUI");
            InitializeComponent();


            ////_timeLimitDlg.IsVisibleChanged += OnChildWindowIsVisibleChanged;
            ////_msgOverlay.IsVisibleChanged += OnChildWindowIsVisibleChanged;

            GameBottomWindowCtrl.MouseDown += GameBottomWindowCtrl_MouseDown;
            GameBottomWindowCtrl.MouseUp += GameBottomWindowCtrl_MouseUp;

            SizeChanged += LobbyView_SizeChanged;

            if (_cabinetDetectionService.Family == HardwareFamily.Unknown ||
                _cabinetDetectionService.GetDisplayDeviceByItsRole(DisplayRole.Top) != null)
            {
                Logger.Debug("Creating top view");
                _topView = new LobbyTopView();
            }

            if (_cabinetDetectionService.Family == HardwareFamily.Unknown &&
                _properties.GetValue("display", string.Empty) == "windowed" ||
                _cabinetDetectionService.GetDisplayDeviceByItsRole(DisplayRole.Topper) != null)
            {
                Logger.Debug("Creating topper view");
                _topperView = new LobbyTopperView();
            }

            Logger.Debug("Creating view model");
            ViewModel = new LobbyViewModel();
            _overlayManager = new OverlayManager(ViewModel)
            {
                MainView = this,
                TopView = _topView,
                TopperView = _topperView
            };
            //ViewModel.PropertyChanged += ViewModel_OnPropertyChanged;

            // BinkGpuControl can't deal with having its size set by binding to anything other than Root height/width initially (timing issue maybe)
            // Bind to Root in xaml and then update it to the proper size here when the LayoutTemplate size is set later
            LayoutTemplate.SizeChanged += (o, args) =>
            {
                GameAttract.Height = args.NewSize.Height;
                GameAttract.Width = args.NewSize.Width;
            };

            _eventBus.Subscribe<OverlayWindowVisibilityChangedEvent>(this, HandleOverlayWindowVisibilityChanged);
        }

        private void HandleOverlayWindowVisibilityChanged(OverlayWindowVisibilityChangedEvent e)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                try
                {
                    if (e.IsVisible)
                    {
                        //ShowOverlayWindow();
                    }

                    SetOverlayWindowTransparent(!e.IsVisible);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            });
        }

        /// <summary>
        ///     Gets or sets the view model
        /// </summary>
        public LobbyViewModel ViewModel
        {
            get => DataContext as LobbyViewModel;

            set
            {
                value.LobbyView = this;
                DataContext = value;
                if (_topView != null)
                {
                    _topView.DataContext = value;
                }
                if (_topperView != null)
                {
                    _topperView.DataContext = value;
                }

                ////_timeLimitDlg.DataContext = value;
                ////_msgOverlay.DataContext = value;
            }
        }

        /// <summary>
        /// Binds miscellaneous properties from the overlay window to its associated lobby viewmodel.
        /// </summary>
        //private static void AddOverlayBindings(OverlayWindow view, LobbyViewModel vm)
        //{
        //    WpfUtil.Bind(view, OverlayWindow.ReplayNavigationBarHeightProperty, vm, nameof(vm.ReplayNavigationBarHeight), BindingMode.OneWayToSource);
        //    WpfUtil.Bind(view, OverlayWindow.IsDialogFadingOutProperty, vm.MessageOverlayDisplay.MessageOverlayData, nameof(vm.MessageOverlayDisplay.MessageOverlayData.IsDialogFadingOut), BindingMode.OneWayToSource);
        //}

        /// <summary>
        ///     Creates and shows the DisableCountdown window.
        /// </summary>
        public void CreateAndShowDisableCountdownWindow()
        {
            _overlayManager.CreateAndShowDisableCountdownWindow();
        }

        /// <summary>
        ///     Closes the DisableCountdown window.
        /// </summary>
        public void CloseDisableCountdownWindow()
        {
            _overlayManager.CloseDisableCountdownWindow();
        }

        public void SetOverlayWindowTransparent(bool transparent)
        {
            //_overlayManager.SetOverlayWindowTransparent(transparent);
        }

        private void OnChildWindowIsVisibleChanged(
            object sender,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            // This is a fix for VLT-1450.  When hiding a child window, the parent window seems to lose
            // focus and another window comes to the foreground.
            Activate();
        }

        private void GameBottomWindowCtrl_MouseUp(object sender, MouseEventArgs e)
        {
            var ti = new TouchInfo
            {
                X = e.X,
                Y = e.Y,
                State = TouchState.Up,
                Action = e.Button == MouseButtons.Left ? TouchAction.LeftMouse : TouchAction.None
            };

            /* only accept left mouse clicks */
            ViewModel.PostTouchEvent(ti);
        }

        private void GameBottomWindowCtrl_MouseDown(object sender, MouseEventArgs e)
        {
            var ti = new TouchInfo
            {
                X = e.X,
                Y = e.Y,
                State = TouchState.Down,
                Action = e.Button == MouseButtons.Left ? TouchAction.LeftMouse : TouchAction.None
            };

            /* only accept left mouse clicks */
            ViewModel.PostTouchEvent(ti);
        }

        private void LobbyView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Seems to be some interop issue with WinForms control not being in sync with parent window.
            // So manually sync them.  We can't data bind with WinForm control.
            GameBottomWindowCtrl.Width = (int)GameLayout.ActualWidth;
            GameBottomWindowCtrl.Height = (int)GameLayout.ActualHeight;

            //if (_mediaDisplayWindow != null)
            //{
            //    _mediaDisplayWindow.Width = ActualWidth;
            //    _mediaDisplayWindow.Height = ActualHeight;
            //}
        }

        private void WinHostCtrl_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.GameBottomHwnd = GameBottomWindowCtrl.Handle;

            ViewModel.OnHwndLoaded();
        }

        private void LobbyView_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Hide the LobbyView window while data is being loaded on this screen.
            // This way the user will not see a black screen which using Stopwatch
            // can take up to around 2 seconds. This Fixes Defect VLT-2584.
            Hide();

            ViewModel.LanguageChanged += OnLanguageChanged;
            ViewModel.DisplayChanged += ViewModel_OnDisplayChanged;

            //if (_responsibleGamingWindow != null)
            //{
            //    _responsibleGamingWindow.Owner = this;
            //    _responsibleGamingWindow.IsVisibleChanged += OnChildWindowIsVisibleChanged;
            //    SetStylusSettings(_responsibleGamingWindow);
            //}

            _windowToScreenMapper.MapWindow(this);

            ShowTitleBar = !_windowToScreenMapper.IsFullscreen;
            ShowCloseButton = !_windowToScreenMapper.IsFullscreen;
            ShowMinButton = !_windowToScreenMapper.IsFullscreen;
            ShowMaxRestoreButton = !_windowToScreenMapper.IsFullscreen;

            var simulateLcdButtonDeck = _properties.GetValue(
                HardwareConstants.SimulateLcdButtonDeck,
                Constants.False);
            simulateLcdButtonDeck = simulateLcdButtonDeck.ToUpperInvariant();
            if (simulateLcdButtonDeck == Constants.True)
            {
                var buttonDeckService = ServiceManager.GetInstance().TryGetService<IButtonDeckDisplay>();

                // Do we not have a hardware button deck?
                if (buttonDeckService != null && buttonDeckService.DisplayCount == 0)
                {
                    _buttonDeckSimulator = new ButtonDeckSimulatorView();
                    _buttonDeckSimulator.Show();
                }
            }

            var simulateVirtualButtonDeck = _properties.GetValue(
                HardwareConstants.SimulateVirtualButtonDeck,
                Constants.False);
            Debug.Assert(
                simulateLcdButtonDeck != Constants.True ||
                simulateVirtualButtonDeck.ToUpperInvariant() != Constants.True,
                $"{nameof(simulateLcdButtonDeck)} and {nameof(simulateVirtualButtonDeck)} are mutually exclusive.");

            if (HostMachineIsNotAnEGM())
            {
                LoadVbd();
            }

            var showTestTool = (string)_properties.GetProperty(
                Constants.ShowTestTool,
                Constants.False);
            showTestTool = showTestTool.ToUpperInvariant();
            if (showTestTool == Constants.True)
            {
                _testToolView = new TestToolView();
            }

            // Set owners so _customOverlays appear on top of parent window (we have to do this
            // in OnHwndLoaded, not constructor).  This means we do not have to use Topmost
            // which caused other issues (audit screen appearing behind overlay).
            ////_timeLimitDlg.Owner = this;
            ////_msgOverlay.Owner = this;

            ShowWithTouch(_testToolView);

            _overlayManager.ShowAndPositionOverlays();

            _overlayManager.ChangeLanguageSkin(ViewModel.IsPrimaryLanguageSelected);

            ViewModel.OnLoaded();

            // now show the Lobby View Window after all loading is done.
            // This addresses defect VLT-2584.  We do not want to display black window
            // to user when the window is loading.  Only display when done.
            Show();

            // now show the Lobby TopView window here to address defect VLT-2584.
            _topView?.Show();
            _topperView?.Show();
        }

        private bool HostMachineIsNotAnEGM()
        {
            var ioService = ServiceManager.GetInstance().TryGetService<IIO>();  // used to check for laptop usage
            if (HardwareHelpers.CheckForVirtualButtonDeckHardware() || ioService?.LogicalState == IOLogicalState.Disabled)
            {
                return true;
            }

            return false;
        }

        private void LoadVbd()
        {
            if (!_vbdLoaded)
            {
                _vbd = new VirtualButtonDeckView { ViewModel = ViewModel };
                SetStylusSettings(_vbd);
                //_lobbyWindows.Add(_vbd);
                ShowWithTouch(_vbd);

                _vbdOverlay = new VirtualButtonDeckOverlayView(_vbd) { ViewModel = ViewModel };
                SetStylusSettings(_vbdOverlay);
                //_lobbyWindows.Add(_vbdOverlay);
                ShowWithTouch(_vbdOverlay);
            }

            _vbdLoaded = true;
        }

        private void ViewModel_OnDisplayChanged(object sender, EventArgs e)
        {
            //Dispatcher?.Invoke(() =>
            //{
            //    _windowToScreenMapper.MapWindow(this);
            //    _windowToScreenMapper.MapWindow(_mediaDisplayWindow);
            //    _windowToScreenMapper.MapWindow(_responsibleGamingWindow);
            //    _windowToScreenMapper.MapWindow(_overlayWindow);
            //});

            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    _overlayManager.ShowAndPositionOverlays();
                });
        }

        private void MediaDisplayWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var isFullscreen = WindowToScreenMapper.GetFullscreen(propertiesManager);
            WindowTools.AssignWindowToPrimaryScreen(sender as Window, isFullscreen);
        }


        //private void ViewModel_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == "IsOverlayWindowVisible")
        //    {
        //        if (ViewModel.MessageOverlayDisplay.IsOverlayWindowVisible)
        //        {
        //            ShowOverlayWindow();
        //        }
        //        else
        //        {
        //            HideOverlayWindow();
        //        }
        //    }
        //}

        private void LobbyView_OnClosed(object sender, EventArgs e)
        {
            Logger.Debug("Closing LobbyView");

            ViewModel.LanguageChanged -= OnLanguageChanged;
            ViewModel.DisplayChanged -= ViewModel_OnDisplayChanged;

            Logger.Debug("Closing LobbyTopView");
            _topView?.Close();
            _topperView?.Close();

            _overlayManager.CloseAllOverlays();

            /*
            if (_timeLimitDlg != null)
            {
                _timeLimitDlg.IsVisibleChanged -= OnChildWindowIsVisibleChanged;
                _timeLimitDlg.Close();
            }
      
            if (_msgOverlay != null)
            {
                _msgOverlay.IsVisibleChanged -= OnChildWindowIsVisibleChanged;
                _msgOverlay.Close();
            }
            */

            Logger.Debug("Closing VirtualButtonDeckView");
            _vbd?.Close();
            _vbdOverlay?.Close();
            Logger.Debug("Closing ButtonDeckSimulatorView");
            _buttonDeckSimulator?.Close();
            Logger.Debug("Closing TestToolView");
            _testToolView?.Close();

            Logger.Debug("Disposing LobbyViewModel");
            ViewModel?.Dispose();

            ServiceManager.GetInstance().TryGetService<IBrowserProcessManager>()?.Dispose();
            ServiceManager.GetInstance().TryGetService<IEventBus>()?.UnsubscribeAll(this);

            // In a RAM clear, everything gets shut down except the lower layers.  We
            // need to recreate all the Bink stuff since the lobby windows will be launched on
            // a new thread.  Otherwise we get error about other thread accessing WPF object.
            Logger.Debug("Clearing DecoderCache");
            BinkViewerControl.ClearDecoderCache();

            // This is explicit to ensure the D3D and Bink layer is/can be destroyed
            Logger.Debug("Disposing BinkGpuControl");
            GameAttract.Dispose();
            Logger.Debug("Destroy D3D");
            D3D.Destroy();

            LayoutTemplate?.Dispose();

            //_lobbyWindows.Clear();

            GameBottomWindowCtrl.MouseDown -= GameBottomWindowCtrl_MouseDown;
            GameBottomWindowCtrl.MouseUp -= GameBottomWindowCtrl_MouseUp;

            SizeChanged -= LobbyView_SizeChanged;
        }

        private void SetStylusSettings(Window window)
        {
            Stylus.SetIsTapFeedbackEnabled(window, false);
            Stylus.SetIsTouchFeedbackEnabled(window, false);
            Stylus.SetIsPressAndHoldEnabled(window, false);
            Stylus.SetIsFlicksEnabled(window, false);
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            MvvmHelper.ExecuteOnUI(() => _overlayManager.ChangeLanguageSkin(ViewModel.IsPrimaryLanguageSelected));
        }

        private void LobbyRoot_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.OnUserInteraction();
        }

        private void LobbyRoot_OnGotFocus(object sender, RoutedEventArgs e)
        {
        }

        private void LobbyView_OnGotFocus(object sender, RoutedEventArgs e)
        {
            WindowTools.RemoveTouchCursor();
        }

        private void LobbyView_OnActivated(object sender, EventArgs e)
        {
            WindowTools.RemoveTouchCursor();
        }

        private void LobbyView_OnContentRendered(object sender, EventArgs e)
        {
            Logger.Debug("LobbyView_OnContentRendered");
            Logger.Debug($"Original ViewBox Main Size Width:{GameLayout.ActualWidth} Height:{GameLayout.ActualHeight}");
            //if (_responsibleGamingWindow != null)
            //{
            //    _windowToScreenMapper.MapWindow(_responsibleGamingWindow);
            //}
        }

        private void GameLayout_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewModel.ResizeGameWindow(e.NewSize, LayoutTemplate.RenderSize);
        }

        private void ShowWithTouch(Window window)
        {
            if (window == null)
            {
                return;
            }

            window.Show();
            WpfWindowLauncher.DisableStylus(window);
        }

        private void GameAttract_OnVideoCompleted(object sender, RoutedEventArgs e)
        {
            ViewModel.OnGameAttractVideoCompleted();
        }
    }
}