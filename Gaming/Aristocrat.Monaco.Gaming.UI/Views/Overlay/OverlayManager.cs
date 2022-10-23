namespace Aristocrat.Monaco.Gaming.UI.Views.Overlay
{
    using Application.Contracts;
    using Application.Contracts.Media;
    using Cabinet.Contracts;
    using Common;
    using Contracts;
    using Hardware.Contracts.Cabinet;
    using Kernel;
    using Lobby;
    using log4net;
    using MahApps.Metro.Controls;
    using MediaDisplay;
    using Monaco.UI.Common;
    using MVVM;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Interop;
    using Application.UI.Views;
    using ButtonDeck;
    using ViewModels;

    public class OverlayManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPropertiesManager _properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
        private readonly ICabinetDetectionService _cabinetDetectionService = ServiceManager.GetInstance().GetService<ICabinetDetectionService>();

        private LobbyViewModel _viewModel;

        private readonly LobbyView _mainView;
        private readonly LobbyTopView _topView;
        private readonly LobbyTopperView _topperView;
        private VirtualButtonDeckView _vbdView;

        private readonly InfoWindow _infoWindow;
        private readonly OverlayWindow _overlayWindow;
        private readonly ResponsibleGamingWindow _responsibleGamingWindow;
        private readonly DisableCountdownWindow _disableCountdownWindow;
        private VirtualButtonDeckOverlayView _vbdOverlay;

        private LayoutOverlayWindow _mediaDisplayWindow;
        private LayoutOverlayWindow _topMediaDisplayWindow;
        private LayoutOverlayWindow _topperMediaDisplayWindow;

        private readonly List<(DisplayRole display, Window window)> _lobbyWindows = new();

        private readonly Dictionary<DisplayRole, (Action<UIElement> entryAction, Action<UIElement> exitAction)> _customOverlays = new();

        private readonly bool _windowed;
        private bool _compositeDisplay;
        private bool _arrangeDisplay;

        private readonly List<ResourceDictionary> _skins = new();
        private int _activeSkinIndex;

        public OverlayManager(LobbyViewModel lobbyViewModel, LobbyView mainView, LobbyTopView topView, LobbyTopperView topperView)
        {
            ViewModel = lobbyViewModel ?? throw new ArgumentNullException(nameof(lobbyViewModel));
            _mainView = mainView ?? throw new ArgumentNullException(nameof(mainView));
            _topView = topView;
            _topperView = topperView;

            Logger.Debug("Checking Fullscreen");
            var display = (string)_properties.GetProperty(
                Constants.DisplayPropertyKey,
                Constants.DisplayPropertyFullScreen);
            _windowed = display.ToUpperInvariant() != Constants.DisplayPropertyFullScreen;

            _compositeDisplay = bool.Parse(_properties.GetValue("composite", "true"));
            _arrangeDisplay = bool.Parse(_properties.GetValue("arrange", "true"));

            Logger.Debug("Caching skins");
            var config = (LobbyConfiguration)_properties.GetProperty(GamingConstants.LobbyConfig, null);
            foreach (var skinFilename in config.SkinFilenames)
            {
                _skins.Add(SkinLoader.Load(skinFilename));
            }

            Logger.Debug("Creating info window");
            _infoWindow = new InfoWindow();
            _lobbyWindows.Add((DisplayRole.Main, _infoWindow));

            Logger.Debug("Creating overlay view");
            _overlayWindow = new OverlayWindow();
            _lobbyWindows.Add((DisplayRole.Main, _overlayWindow));

            if (lobbyViewModel.Config.ResponsibleGamingTimeLimitEnabled)
            {
                Logger.Debug("Creating RG windows");
                _responsibleGamingWindow = new ResponsibleGamingWindow { ViewModel = lobbyViewModel };
                _lobbyWindows.Add((DisplayRole.Main, _responsibleGamingWindow));

                _disableCountdownWindow = new DisableCountdownWindow { DataContext = lobbyViewModel };
                _lobbyWindows.Add((DisplayRole.Main, _disableCountdownWindow));
            }

            Logger.Debug("Checking CEF");
            var mediaEnabled = _properties.GetValue(ApplicationConstants.MediaDisplayEnabled, false);
            if (mediaEnabled)
            {
                Logger.Debug("Initializing CEF");
                CefHelper.Initialize();

                Logger.Debug("Creating media display");
                _mediaDisplayWindow = new LayoutOverlayWindow(ScreenType.Primary) { Title = "Main Media" };
                _lobbyWindows.Add((DisplayRole.Main, _mediaDisplayWindow));

                _topMediaDisplayWindow = new LayoutOverlayWindow(ScreenType.Glass) { Title = "Top Media" };
                _lobbyWindows.Add((DisplayRole.Top, _topMediaDisplayWindow));

                _topperMediaDisplayWindow = new LayoutOverlayWindow(ScreenType.Glass) { Title = "Topper Media" };
                _lobbyWindows.Add((DisplayRole.Topper, _topperMediaDisplayWindow));
            }

            InitializeCustomOverlays();
            lobbyViewModel.HandleViewInjectionEvent += ViewInjectionEventHandler;

            // Yes, we do assign this twice. It's for safety. The first time ensures it's available
            // to anyone looking for it, and the second is so it will be pushed to all the overlays
            // that have been created.
            ViewModel = lobbyViewModel;

            // These bindings are for the overlay, and one day someone should investigate why they are
            // done like this instead of just being in the XAML.
            //WpfUtil.Bind(_overlayWindow, OverlayWindow.ReplayNavigationBarHeightProperty, lobbyViewModel, nameof(lobbyViewModel.ReplayNavigationBarHeight), BindingMode.OneWayToSource);
            //WpfUtil.Bind(_overlayWindow, OverlayWindow.IsDialogFadingOutProperty, lobbyViewModel.MessageOverlayData, nameof(lobbyViewModel.MessageOverlayData.IsDialogFadingOut), BindingMode.OneWayToSource);
        }

        public LobbyViewModel ViewModel
        {
            set
            {
                _viewModel = value;
                if (_overlayWindow != null)
                {
                    _overlayWindow.ViewModel = value;
                }

                if (_responsibleGamingWindow != null)
                {
                    _responsibleGamingWindow.ViewModel = value;
                }
            }
        }

        private void InitializeCustomOverlays()
        {
            // NOTE: We create the overlay windows even if a device isn't present, because it may
            // be reconnected after we boot and we'll need to have it ready to display.
            Logger.Debug("Initializing main overlay");
            _customOverlays.Add(DisplayRole.Main, (
                element =>
                {
                    var grid = _overlayWindow.FindName("ViewInjectionRoot") as Grid;
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
                element =>
                {
                    var grid = _overlayWindow.FindName("ViewInjectionRoot") as Grid;
                    if (grid?.Children.Count > 0)
                    {
                        if (element != null)
                        {
                            grid.Children.Remove(element);
                            Logger.Debug(
                                $"Cleared the custom view {element.GetType().FullName} from Main Screen, hash: {element.GetHashCode()}");
                        }
                        else
                        {
                            grid.Children.Clear();
                            Logger.Debug($"Cleared all custom views from Main Screen");
                        }
                    }
                }
            ));

            if (_cabinetDetectionService.Family == HardwareFamily.Unknown ||
                _cabinetDetectionService.IsDisplayExpected(DisplayRole.Top))
            {
                Logger.Debug("Initializing Top Overlay");
                _customOverlays.Add(
                    DisplayRole.Top,
                    (
                        element =>
                        {
                            if (_topMediaDisplayWindow == null)
                            {
                                _topMediaDisplayWindow =
                                    new LayoutOverlayWindow(ScreenType.Glass) { Title = "Top Media" };
                                _lobbyWindows.Add((DisplayRole.Top, _topMediaDisplayWindow));
                                ShowOverlay(_topMediaDisplayWindow, _topView);
                            }

                            // Insert the control under a gird instead of the canvas, this let the
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
                        element =>
                        {
                            if (_topMediaDisplayWindow != null)
                            {
                                var grid = _topMediaDisplayWindow.Content as Grid;
                                if (grid?.Children.Count > 0)
                                {
                                    if (element != null)
                                    {
                                        grid.Children.Remove(element);
                                        Logger.Debug(
                                            $"Cleared the custom view {element.GetType().FullName} from Top Screen, hash: {element.GetHashCode()}");
                                    }
                                    else
                                    {
                                        grid.Children.Clear();
                                        _topMediaDisplayWindow.Content = null;
                                        Logger.Debug("Cleared all custom views from Top Screen");
                                    }
                                }
                            }
                        }));
            }

            if (_cabinetDetectionService.Family == HardwareFamily.Unknown ||
                _cabinetDetectionService.IsDisplayExpected(DisplayRole.Topper))
            {
                Logger.Debug("Initializing topper overlay");
                _customOverlays.Add(DisplayRole.Topper, (
                    element =>
                    {
                        if (_topperMediaDisplayWindow == null)
                        {
                            _topperMediaDisplayWindow = new LayoutOverlayWindow(ScreenType.Glass) { Title = "Topper Media" };
                            _lobbyWindows.Add((DisplayRole.Topper, _topperMediaDisplayWindow));
                            ShowOverlay(_topperMediaDisplayWindow, _topperView);
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
                    element =>
                    {
                        if (_topperMediaDisplayWindow != null)
                        {
                            var grid = _topperMediaDisplayWindow.Content as Grid;
                            if (grid?.Children.Count > 0)
                            {
                                if (element != null)
                                {
                                    grid.Children.Remove(element);
                                    Logger.Debug(
                                        $"Cleared the custom view {element.GetType().FullName} from Topper Screen, hash: {element.GetHashCode()}");
                                }
                                else
                                {
                                    grid.Children.Clear();
                                    _topperMediaDisplayWindow.Content = null;
                                    Logger.Debug($"Cleared all custom views from Topper Screen");
                                }
                            }
                        }
                    }
                ));
            }
        }

        internal void LoadVbdOverlay(VirtualButtonDeckView vbdView)
        {
            _vbdView = vbdView;
            _vbdOverlay = new VirtualButtonDeckOverlayView() { ViewModel = _viewModel };
            _lobbyWindows.Add((DisplayRole.VBD, _vbdOverlay));
            //ShowOverlay(_vbdOverlay, GetViewForDisplay(DisplayRole.VBD)); TODO: remove if working
        }

        private void ViewInjectionEventHandler(ViewInjectionEvent ev)
        {
            if (ev.Element == null && ev.Action == ViewInjectionEvent.ViewAction.Remove)
            {
                Logger.Debug("Element is null and action is remove --OK");
            }
            else if (!(ev.Element is UIElement))
            {
                Logger.Error($"Element {ev.Element?.GetType()} passed cannot be cast as UIElement");
                return;
            }

            if (ev.Action == ViewInjectionEvent.ViewAction.Add)
            {
                _customOverlays[ev.DisplayRole].entryAction((UIElement)ev.Element);
            }
            else
            {
                _customOverlays[ev.DisplayRole].exitAction((UIElement)ev.Element);
            }
        }

        public void ShowAndPositionOverlays()
        {
            // now show the Lobby TopView window here to address defect VLT-2584.
            foreach (var displayAndWindow in _lobbyWindows)
            {
                Logger.Debug($"Showing {displayAndWindow.window.GetType()} for display {displayAndWindow.display}");
                ShowOverlay(displayAndWindow.window, GetViewForDisplay(displayAndWindow.display));
            }
        }

        private MetroWindow GetViewForDisplay(DisplayRole display)
        {
            return display switch
            {
                DisplayRole.Main => _mainView,
                DisplayRole.Top => _topView,
                DisplayRole.Topper => _topperView,
                DisplayRole.VBD => _vbdView,
                _ => null,
            };
        }

        private void ShowOverlay(Window window, MetroWindow view, bool withTouch = true)
        {
            if (_arrangeDisplay)
            {
                var tl = view.PointToScreen(new Point(0, 0));
                var br = view.PointToScreen(new Point(view.ActualWidth, view.ActualHeight));
                window.Left = tl.X;
                window.Top = tl.Y;
                window.Width = br.X - tl.X;
                window.Height = br.Y - tl.Y;
            }

            if (_compositeDisplay)
            {
                WpfWindowLauncher.DisableStylus(window);
                window.ResizeMode = ResizeMode.NoResize;
                window.WindowStyle = WindowStyle.None;
                window.AllowsTransparency = true;
                window.Topmost = true;
                window.BorderThickness = new Thickness(0.0);
                window.ShowInTaskbar = _windowed;
            }

            window.Show();

            if (withTouch)
            {
                WpfWindowLauncher.DisableStylus(window);
            }
 
            // Arrange again after showing because WPF will move things once they are displayed.
            if (_arrangeDisplay)
            {
                var tl = view.PointToScreen(new Point(0, 0));
                var br = view.PointToScreen(new Point(view.ActualWidth, view.ActualHeight));
                window.Left = tl.X;
                window.Top = tl.Y;
                window.Width = br.X - tl.X;
                window.Height = br.Y - tl.Y;
            }
        }

        /// <summary>
        ///     Shows the DisableCountdown window.
        /// </summary>
        public void ShowDisableCountdownWindow()
        {
            _disableCountdownWindow.Show();
        }

        /// <summary>
        ///     Closes the DisableCountdown window.
        /// </summary>
        public void HideDisableCountdownWindow()
        {
            _disableCountdownWindow.Hide();
        }

        public void ChangeLanguageSkin(bool primaryLanguageSkin)
        {
            _activeSkinIndex = primaryLanguageSkin ? 0 : 1;

            var tmpResource = new ResourceDictionary();
            tmpResource.MergedDictionaries.Add(_skins[_activeSkinIndex]);
            foreach (var displayAndWindow in _lobbyWindows)
            {
                displayAndWindow.window.Resources = tmpResource;
            }

            // TODO: This is temporary code which will be removed once SingleWindow is created.
            if (_mainView != null)
            {
                _mainView.Resources = tmpResource;
            }

            if (_topView != null)
            {
                _topView.Resources = tmpResource;
            }

            if (_topperView != null)
            {
                _topperView.Resources = tmpResource;
            }

            if (_vbdView != null)
            {
                _vbdView.Resources = tmpResource;
            }
        }

        public void CloseAllOverlays()
        {
            foreach (var displayAndWindow in _lobbyWindows)
            {
                displayAndWindow.window.Close();
            }

/*      _infoWindow;
        _overlayWindow;
        _responsibleGamingWindow;
        _disableCountdownWindow;
        _vbdOverlay;

        _mediaDisplayWindow;
        _topMediaDisplayWindow;
        _topperMediaDisplayWindow;*/
        }
    }
}
