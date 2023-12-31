﻿namespace Aristocrat.Monaco.Gaming.UI.Views.Lobby
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
    using EdgeLight;
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
        private readonly List<Window> _lobbyWindows = new List<Window>();
        private readonly List<ResourceDictionary> _skins = new List<ResourceDictionary>();

        private TestToolView _testToolView;

        ////private readonly TimeLimitDialog _timeLimitDlg;
        ////private readonly MessageOverlay _msgOverlay;
        private readonly LobbyTopView _topView;
        private readonly LobbyTopperView _topperView;
        private LayoutOverlayWindow _mediaDisplayWindow;
        private LayoutOverlayWindow _topMediaDisplayWindow;
        private LayoutOverlayWindow _topperMediaDisplayWindow;

        private int _activeSkinIndex;

        private VirtualButtonDeckOverlayView _vbdOverlay;
        private ButtonDeckSimulatorView _buttonDeckSimulator;

        private DisableCountdownWindow _disableCountdownWindow;
        private InfoWindow _infoWindow;
        private OverlayWindow _overlayWindow;
        private ResponsibleGamingWindow _responsibleGamingWindow;
        private VirtualButtonDeckView _vbd;
        private bool _vbdLoaded;

        private Dictionary<DisplayRole, (Action<UIElement> entryAction, Action<UIElement> exitAction)> _customOverlays;

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

            // Pre-load all the skins and cache them to support changing at runtime.
            Logger.Debug("Caching skins");
            var config = (LobbyConfiguration)properties.GetProperty(GamingConstants.LobbyConfig, null);
            foreach (var skinFilename in config.SkinFilenames)
            {
                _skins.Add(SkinLoader.Load(skinFilename));
            }

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

            Logger.Debug("Creating overlay view");
            _overlayWindow = new OverlayWindow(this);

            Logger.Debug("Creating view model");
            ViewModel = new LobbyViewModel();
            ViewModel.CustomEventViewChangedEvent += ViewModelOnCustomEventViewChangedEvent;
            ViewModel.PropertyChanged += ViewModel_OnPropertyChanged;

            if (ViewModel.Config.ResponsibleGamingTimeLimitEnabled)
            {
                Logger.Debug("Creating RG window");
                _responsibleGamingWindow = new ResponsibleGamingWindow { ViewModel = ViewModel };
            }

            _lobbyWindows.Add(this);
            if (_topView != null)
            {
                _lobbyWindows.Add(_topView);
            }

            if (_topperView != null)
            {
                _lobbyWindows.Add(_topperView);
            }

            _lobbyWindows.Add(_overlayWindow);

            if (_responsibleGamingWindow != null)
            {
                _lobbyWindows.Add(_responsibleGamingWindow);
            }

            if (mediaEnabled)
            {
                Logger.Debug("Creating media display");
                _mediaDisplayWindow = new LayoutOverlayWindow(ScreenType.Primary) { Title = "Primary Overlay" };
                _mediaDisplayWindow.Loaded += MediaDisplayWindow_OnLoaded;
                ShowWithTouch(_mediaDisplayWindow);

                _lobbyWindows.Add(_mediaDisplayWindow);
                if (_topView != null)
                {
                    _topMediaDisplayWindow = _topView.GetMediaDisplayWindow();
                    _lobbyWindows.Add(_topMediaDisplayWindow);
                }

                if (_topperView != null)
                {
                    _topperMediaDisplayWindow = _topperView.GetMediaDisplayWindow();
                    _lobbyWindows.Add(_topperMediaDisplayWindow);
                }
            }

            // BinkGpuControl can't deal with having its size set by binding to anything other than Root height/width initially (timing issue maybe)
            // Bind to Root in xaml and then update it to the proper size here when the LayoutTemplate size is set later
            LayoutTemplate.SizeChanged += (o, args) =>
            {
                GameAttract.Height = args.NewSize.Height;
                GameAttract.Width = args.NewSize.Width;
            };

            InitializeCustomOverlays();

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
                        ShowOverlayWindow();
                    }

                    SetOverlayWindowTransparent(!e.IsVisible);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            });
        }

        private void InitializeCustomOverlays()
        {
            _customOverlays = new Dictionary<DisplayRole, (Action<UIElement> entryAction, Action<UIElement> exitAction)>();

            if (_cabinetDetectionService.Family == HardwareFamily.Unknown ||
                _cabinetDetectionService.IsDisplayExpected(DisplayRole.Top))
            {
                Logger.Debug("Initializing top overlay");
                _customOverlays.Add(DisplayRole.Top, (
                    (element) =>
                    {
                        if (_topView == null)
                        {
                            Logger.Debug("Top view is null");
                            return;
                        }

                        if (_topMediaDisplayWindow == null)
                        {
                            _topView.SetupMediaDisplayWindow();
                            _topMediaDisplayWindow = _topView.GetMediaDisplayWindow();
                        }

                        // Insert the control under a grid instead of the canvas, this let the
                        // child ViewBox (if present) expand as needed for the screen resolution
                        var grid = _topMediaDisplayWindow.Content as Grid ?? new Grid();
                        if (grid.Children.Contains(element))
                        {
                            Logger.Warn(
                                $"Trying to add twice the custom view {element.GetType().FullName} to Top Screen, hash: {element.GetHashCode()}");
                        }
                        else
                        {
                            grid.Children.Add(element);
                            _topMediaDisplayWindow.Content = grid;
                            _topMediaDisplayWindow.Show();
                            Logger.Debug(
                                $"Added the custom view {element.GetType().FullName} to Top Screen, hash: {element.GetHashCode()}");
                        }
                    },
                    (element) =>
                    {
                        if (_topMediaDisplayWindow != null)
                        {
                            var grid = _topMediaDisplayWindow.Content as Grid;
                            if (grid?.Children.Count > 0)
                            {
                                if (element != null)
                                {
                                    grid.Children.Remove(element);
                                    Logger.Debug($"Cleared the custom view {element.GetType().FullName} from Top Screen, hash: {element.GetHashCode()}");
                                }
                                else
                                {
                                    grid.Children.Clear();
                                    _topMediaDisplayWindow.Content = null;
                                    Logger.Debug("Cleared all custom views from Top Screen");
                                }
                            }
                        }
                    }
                ));
            }

            if (_cabinetDetectionService.Family == HardwareFamily.Unknown ||
                _cabinetDetectionService.IsDisplayExpected(DisplayRole.Topper))
            {
                Logger.Debug("Initializing topper overlay");
                _customOverlays.Add(DisplayRole.Topper, (
                    (element) =>
                    {
                        if (_topperView == null)
                        {
                            Logger.Debug("Topper view is null");
                            return;
                        }

                        if (_topperMediaDisplayWindow == null)
                        {
                            _topperView.SetupMediaDisplayWindow();
                            _topperMediaDisplayWindow = _topperView.GetMediaDisplayWindow();
                        }

                        // Insert the control under a grid instead of the canvas, this let the
                        // child ViewBox (if present) expand as needed for the screen resolution
                        var grid = _topperMediaDisplayWindow.Content as Grid ?? new Grid();
                        if (grid.Children.Contains(element))
                        {
                            Logger.Warn(
                                $"Trying to add twice the custom view {element.GetType().FullName} to Topper Screen, hash: {element.GetHashCode()}");
                        }
                        else
                        {
                            grid.Children.Add(element);
                            _topperMediaDisplayWindow.Content = grid;
                            _topperMediaDisplayWindow.Show();
                            Logger.Debug($"Added the custom view {element.GetType().FullName} to Topper Screen, hash: {element.GetHashCode()}");
                        }
                    },
                    (element) =>
                    {
                        if (_topperMediaDisplayWindow != null)
                        {
                            var grid = _topperMediaDisplayWindow.Content as Grid;
                            if (grid?.Children.Count > 0)
                            {
                                if (element != null)
                                {
                                    grid.Children.Remove(element);
                                    Logger.Debug($"Cleared the custom view {element.GetType().FullName} from Topper Screen, hash: {element.GetHashCode()}");
                                }
                                else
                                {
                                    grid.Children.Clear();
                                    _topperMediaDisplayWindow.Content = null;
                                    Logger.Debug("Cleared all custom views from Topper Screen");
                                }
                            }
                        }
                    }
                ));
            }

            Logger.Debug("Initializing main overlay");
            _customOverlays.Add(DisplayRole.Main, (
                (element) =>
                {
                    Grid grid = _overlayWindow.FindName("ViewInjectionRoot") as Grid;
                    if (grid != null && grid.Children.Contains(element))
                    {
                        Logger.Warn(
                            $"Trying to add twice the custom view {element.GetType().FullName} to Main Screen, hash: {element.GetHashCode()}");
                    }
                    else
                    {
                        grid?.Children.Add(element);
                        Logger.Debug(
                            $"Added the custom view {element.GetType().FullName} to Main Screen, hash: {element.GetHashCode()}");
                    }
                },
                (element) =>
                {
                    Grid grid = _overlayWindow.FindName("ViewInjectionRoot") as Grid;
                    if (grid?.Children.Count > 0)
                    {
                        if (element != null)
                        {
                            grid.Children.Remove(element);
                            Logger.Debug(
                                $"Removing the custom view model {element.GetType().FullName} from Main Screen, hash: {element.GetHashCode()}");
                        }
                        else
                        {
                            grid.Children.Clear();
                            Logger.Debug($"Cleared all custom views from Main Screen");
                        }
                    }
                }
            ));
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
                _overlayWindow.ViewModel = ViewModel;
                AddOverlayBindings(_overlayWindow, ViewModel);

                if (_responsibleGamingWindow != null)
                {
                    _responsibleGamingWindow.ViewModel = ViewModel;
                }
                ////_timeLimitDlg.DataContext = value;
                ////_msgOverlay.DataContext = value;
            }
        }

        /// <summary>
        /// Binds miscellaneous properties from the overlay window to its associated lobby viewmodel.
        /// </summary>
        private static void AddOverlayBindings(OverlayWindow view, LobbyViewModel vm)
        {
            WpfUtil.Bind(view, OverlayWindow.ReplayNavigationBarHeightProperty, vm, nameof(vm.ReplayNavigationBarHeight), BindingMode.OneWayToSource);
            WpfUtil.Bind(view, OverlayWindow.IsDialogFadingOutProperty, vm.MessageOverlayDisplay.MessageOverlayData, nameof(vm.MessageOverlayDisplay.MessageOverlayData.IsDialogFadingOut), BindingMode.OneWayToSource);
        }

        /// <summary>
        ///     Creates and shows the overlay window.
        /// </summary>
        public void CreateAndShowInfoWindow()
        {
            Dispatcher?.Invoke(
                () =>
                {
                    if (_infoWindow != null)
                    {
                        ShowWithTouch(_infoWindow);
                    }
                    else
                    {
                        _infoWindow = new InfoWindow(this) { Owner = this };
                        _infoWindow.IsVisibleChanged += OnChildWindowIsVisibleChanged;

                        SetStylusSettings(_infoWindow);
                        _lobbyWindows.Add(_infoWindow);

                        ShowWithTouch(_infoWindow);
                    }
                });
        }

        /// <summary>
        ///     Hides the info window.
        /// </summary>
        public void HideInfoWindow()
        {
            Dispatcher?.Invoke(() => _infoWindow?.Hide());
        }

        /// <summary>
        ///     Closes the info window.
        /// </summary>
        public void CloseInfoWindow()
        {
            Dispatcher?.Invoke(
                () =>
                {
                    if (_infoWindow != null)
                    {
                        _lobbyWindows.Remove(_infoWindow);
                        _infoWindow.IsVisibleChanged -= OnChildWindowIsVisibleChanged;
                        _infoWindow.Close();
                        _infoWindow = null;
                    }
                });
        }

        /// <summary>
        ///     Creates and shows the overlay window.
        /// </summary>
        public void ShowOverlayWindow()
        {
            Dispatcher?.Invoke(() => ShowWithTouch(_overlayWindow));
        }

        /// <summary>
        ///     Hides the overlay window.
        /// </summary>
        public void HideOverlayWindow()
        {
            Dispatcher?.Invoke(() => _overlayWindow?.Hide());
        }

        /// <summary>
        ///     Closes the overlay window.
        /// </summary>
        public void CloseOverlayWindow()
        {
            Dispatcher?.Invoke(
                () =>
                {
                    if (_overlayWindow != null)
                    {
                        _lobbyWindows.Remove(_overlayWindow);
                        _overlayWindow.IsVisibleChanged -= OnChildWindowIsVisibleChanged;
                        _overlayWindow.Close();
                        _overlayWindow = null;
                    }
                });
        }

        /// <summary>
        /// Closes the layout overlay window.
        /// </summary>
        public void CloseLayoutOverlayWindow()
        {
            Dispatcher?.Invoke(
                () =>
                {
                    if (_mediaDisplayWindow != null)
                    {
                        _lobbyWindows.Remove(_mediaDisplayWindow);
                        _mediaDisplayWindow.Close();
                        _mediaDisplayWindow = null;
                    }
                });
        }

        /// <summary>
        /// Closes the top layout overlay window.
        /// </summary>
        private void CloseTopLayoutOverlayWindow()
        {
            Dispatcher?.Invoke(
                () =>
                {
                    if (_topMediaDisplayWindow != null)
                    {
                        _lobbyWindows.Remove(_topMediaDisplayWindow);
                        _topMediaDisplayWindow.Close();
                        _topMediaDisplayWindow = null;
                    }
                });
        }

        public void CloseResponsibleGamingWindow()
        {
            Dispatcher?.Invoke(
                () =>
                {
                    if (_responsibleGamingWindow != null)
                    {
                        _lobbyWindows.Remove(_responsibleGamingWindow);
                        _responsibleGamingWindow.IsVisibleChanged -= OnChildWindowIsVisibleChanged;
                        _responsibleGamingWindow.Close();
                        _responsibleGamingWindow = null;
                    }
                });
        }

        /// <summary>
        ///     Creates and shows the DisableCountdown window.
        /// </summary>
        public void CreateAndShowDisableCountdownWindow()
        {
            Logger.Debug("Showing DisabledCountdownWindow");
            Dispatcher?.Invoke(
                () =>
                {
                    if (_disableCountdownWindow != null)
                    {
                        _disableCountdownWindow.Show();
                    }
                    else
                    {
                        _disableCountdownWindow = new DisableCountdownWindow
                        {
                            Owner = this,
                            DataContext = DataContext,
                            Top = Top,
                            Left = Left,
                            Width = Width,
                            Height = Height
                        };

                        _lobbyWindows.Add(_disableCountdownWindow);

                        _disableCountdownWindow.Show();
                    }
                });
        }

        /// <summary>
        ///     Closes the DisableCountdown window.
        /// </summary>
        public void CloseDisableCountdownWindow()
        {
            Logger.Debug("Closing DisabledCountdownWindow");
            Dispatcher?.Invoke(
                () =>
                {
                    if (_disableCountdownWindow != null)
                    {
                        _lobbyWindows.Remove(_disableCountdownWindow);
                        _disableCountdownWindow.Close();
                        _disableCountdownWindow = null;
                    }
                });
        }

        public void SetOverlayWindowTransparent(bool transparent)
        {
            var hWnd = new WindowInteropHelper(_overlayWindow).Handle;
            WindowsServices.SetWindowExTransparent(hWnd, transparent);
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

            if (_mediaDisplayWindow != null)
            {
                _mediaDisplayWindow.Width = ActualWidth;
                _mediaDisplayWindow.Height = ActualHeight;
            }
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

            if (_responsibleGamingWindow != null)
            {
                _responsibleGamingWindow.Owner = this;
                _responsibleGamingWindow.IsVisibleChanged += OnChildWindowIsVisibleChanged;
                SetStylusSettings(_responsibleGamingWindow);
            }

            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            _windowToScreenMapper.MapWindow(this);

            ShowTitleBar = !_windowToScreenMapper.IsFullscreen;
            ShowCloseButton = !_windowToScreenMapper.IsFullscreen;
            ShowMinButton = !_windowToScreenMapper.IsFullscreen;
            ShowMaxRestoreButton = !_windowToScreenMapper.IsFullscreen;

            var simulateLcdButtonDeck = properties.GetValue(
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

            var simulateEdgeLight = properties.GetValue(
                HardwareConstants.SimulateEdgeLighting,
                false);
            if (simulateEdgeLight)
            {
                var edgeLightSimulator = new EdgeLightSimulatorView();
                edgeLightSimulator.Show();
            }

            var simulateVirtualButtonDeck = properties.GetValue(
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

            var showTestTool = (string)properties.GetProperty(
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

            CreateAndShowInfoWindow();

            ChangeLanguageSkin(ViewModel.IsPrimaryLanguageSelected);

            ViewModel.OnLoaded();

            // now show the Lobby View Window after all loading is done.
            // This addresses defect VLT-2584.  We do not want to display black window
            // to user when the window is loading.  Only display when done.
            Show();

            // now show the Lobby TopView window here to address defect VLT-2584.
            _topView?.Show();
            _topperView?.Show();
            ShowWithTouch(_responsibleGamingWindow);

            if (_mediaDisplayWindow != null)
            {
                _overlayWindow.Owner = _mediaDisplayWindow;
                _mediaDisplayWindow.Owner = _responsibleGamingWindow ?? (Window)this;
            }
            else
            {
                _overlayWindow.Owner = _responsibleGamingWindow ?? (Window)this;
            }

            _overlayWindow.IsVisibleChanged += OnChildWindowIsVisibleChanged;
            SetStylusSettings(_overlayWindow);
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
                _lobbyWindows.Add(_vbd);
                ShowWithTouch(_vbd);

                _vbdOverlay = new VirtualButtonDeckOverlayView(_vbd) { ViewModel = ViewModel };
                SetStylusSettings(_vbdOverlay);
                _lobbyWindows.Add(_vbdOverlay);
                ShowWithTouch(_vbdOverlay);
            }

            _vbdLoaded = true;
        }

        private void ViewModel_OnDisplayChanged(object sender, EventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                _windowToScreenMapper.MapWindow(this);
                _windowToScreenMapper.MapWindow(_mediaDisplayWindow);
                _windowToScreenMapper.MapWindow(_responsibleGamingWindow);
                _windowToScreenMapper.MapWindow(_overlayWindow);
            });
        }

        private void MediaDisplayWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var isFullscreen = WindowToScreenMapper.GetFullscreen(propertiesManager);
            WindowTools.AssignWindowToPrimaryScreen(sender as Window, isFullscreen);
        }

        private void ViewModelOnCustomEventViewChangedEvent(ViewInjectionEvent ev)
        {
            if (!(ev.Element is UIElement element))
            {
                Logger.Error($"element {ev.Element?.GetType()} passed cannot be cast as UIElement");
                return;
            }

            if (ev.Action == ViewInjectionEvent.ViewAction.Add)
            {
                _customOverlays[ev.DisplayRole].entryAction(element);
            }
            else
            {
                _customOverlays[ev.DisplayRole].exitAction(element);
            }

        }

        private void ViewModel_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsOverlayWindowVisible")
            {
                if (ViewModel.MessageOverlayDisplay.IsOverlayWindowVisible)
                {
                    ShowOverlayWindow();
                }
                else
                {
                    HideOverlayWindow();
                }
            }
        }

        private void LobbyView_OnClosed(object sender, EventArgs e)
        {
            Logger.Debug("Closing LobbyView");

            ViewModel.LanguageChanged -= OnLanguageChanged;
            ViewModel.DisplayChanged -= ViewModel_OnDisplayChanged;

            Logger.Debug("Closing LobbyTopView");
            _topView?.Close();
            _topperView?.Close();

            if (_overlayWindow != null)
            {
                Logger.Debug("Closing OverlayWindow");
                CloseOverlayWindow();
            }

            if (_responsibleGamingWindow != null)
            {
                Logger.Debug("Closing ResponsibleGamingWindow");
                CloseResponsibleGamingWindow();
            }

            if (_infoWindow != null)
            {
                Logger.Debug("Closing InfoWindow");
                CloseInfoWindow();
            }

            if (_disableCountdownWindow != null)
            {
                Logger.Debug("Closing Disable Countdown Window");
                CloseDisableCountdownWindow();
            }

            if (_mediaDisplayWindow != null)
            {
                Logger.Debug("Closing LayoutOverlayWindow");
                CloseLayoutOverlayWindow();
            }

            if (_topMediaDisplayWindow != null)
            {
                Logger.Debug("Closing TopLayoutOverlayWindow");
                CloseTopLayoutOverlayWindow();
            }

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

            _lobbyWindows.Clear();

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
            Dispatcher?.Invoke(() => ChangeLanguageSkin(ViewModel.IsPrimaryLanguageSelected));
        }

        private void ChangeLanguageSkin(bool primaryLanguageSkin)
        {
            _activeSkinIndex = primaryLanguageSkin ? 0 : 1;

            var tmpResource = new ResourceDictionary();
            tmpResource.MergedDictionaries.Add(_skins[_activeSkinIndex]);
            foreach (var wnd in _lobbyWindows)
            {
                wnd.Resources = tmpResource;
            }

            // Resources.Culture = new CultureInfo(ViewModel.ActiveLocaleCode);
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
            if (_responsibleGamingWindow != null)
            {
                _windowToScreenMapper.MapWindow(_responsibleGamingWindow);
            }
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
            window.Activate();
            WpfWindowLauncher.DisableStylus(window);
        }

        private void GameAttract_OnVideoCompleted(object sender, RoutedEventArgs e)
        {
            ViewModel.OnGameAttractVideoCompleted();
        }
    }
}
