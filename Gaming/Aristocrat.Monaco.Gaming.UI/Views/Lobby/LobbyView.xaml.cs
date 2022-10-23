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
        private readonly WindowToScreenMapper _windowToScreenMapper = new(DisplayRole.Main);

        private readonly ICabinetDetectionService _cabinetDetectionService =
            ServiceManager.GetInstance().GetService<ICabinetDetectionService>();
        private readonly IEventBus _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
        private readonly IPropertiesManager _properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();

        private TestToolView _testToolView;

        private readonly OverlayManager _overlayManager;

        private readonly LobbyTopView _topView;
        private readonly LobbyTopperView _topperView;
        private VirtualButtonDeckView _vbdView;
        private ButtonDeckSimulatorView _buttonView;

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
            _overlayManager = new OverlayManager(ViewModel, this, _topView, _topperView);
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
                        // TODO: 
                        //_overlayManager.
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
            }
        }

        /// <summary>
        ///     Shows the DisableCountdown window.
        /// </summary>
        public void ShowDisableCountdownWindow()
        {
            _overlayManager.ShowDisableCountdownWindow();
        }

        /// <summary>
        ///     Closes the DisableCountdown window.
        /// </summary>
        public void HideDisableCountdownWindow()
        {
            _overlayManager.HideDisableCountdownWindow();
        }

        public void SetOverlayWindowTransparent(bool transparent)
        {
            //_overlayManager.SetOverlayWindowTransparent(transparent);
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
                    _buttonView = new ButtonDeckSimulatorView();
                    _buttonView.Show();
                }
            }

            var simulateVirtualButtonDeck = _properties.GetValue(
                HardwareConstants.SimulateVirtualButtonDeck,
                Constants.False);
            Debug.Assert(
                simulateLcdButtonDeck != Constants.True ||
                simulateVirtualButtonDeck.ToUpperInvariant() != Constants.True,
                $"{nameof(simulateLcdButtonDeck)} and {nameof(simulateVirtualButtonDeck)} are mutually exclusive.");

            if (HostMachineHasVbd())
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

            OnLanguageChanged(null, null);

            ViewModel.OnLoaded();

            // now show the Lobby View Window after all loading is done.
            // This addresses defect VLT-2584.  We do not want to display black window
            // to user when the window is loading.  Only display when done.
            Show();

            // now show the Lobby TopView window here to address defect VLT-2584.
            _topView?.Show();
            _topperView?.Show();

            _overlayManager.ShowAndPositionOverlays();
        }

        private bool HostMachineHasVbd()
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
            _vbdView = new VirtualButtonDeckView { ViewModel = ViewModel };
            SetStylusSettings(_vbdView);
            ShowWithTouch(_vbdView);
            _overlayManager.LoadVbdOverlay(_vbdView);
        }

        private void ViewModel_OnDisplayChanged(object sender, EventArgs e)
        {
            //Dispatcher?.Invoke(() =>
            //{
            //    _windowToScreenMapper.MapWindow(this);
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

            Logger.Debug("Closing VirtualButtonDeckView");
            _vbdView?.Close();
            Logger.Debug("Closing ButtonDeckSimulatorView");
            _buttonView?.Close();
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
            var primaryLanguageSelected = ViewModel.IsPrimaryLanguageSelected;

            MvvmHelper.ExecuteOnUI(() =>
                {
                    _overlayManager.ChangeLanguageSkin(primaryLanguageSelected);
                });
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