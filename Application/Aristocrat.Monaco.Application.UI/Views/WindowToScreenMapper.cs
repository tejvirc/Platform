namespace Aristocrat.Monaco.Application.UI.Views
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Forms;
    using Cabinet.Contracts;
    using Contracts;
    using Hardware.Contracts.Cabinet;
    using Kernel;
    using log4net;
    using Monaco.Common;
    using Models;
    using Cursors = System.Windows.Input.Cursors;

    public class WindowToScreenMapper
    {
        private readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static IDisplayDevice _dualScreenDevice = null;

        private static bool? _isPortrait = null;
        private static bool? _isPortraitOnTwoDisplays = null;

        private readonly IPropertiesManager _properties;
        private readonly IDisplayDevice _device = null;
        private readonly DisplayRole _role;
        private readonly bool _fullScreen;
        private readonly bool _showCursor;

        public WindowToScreenMapper(DisplayRole role, bool forceFullScreen = false, bool? showCursor = null)
            : this(
                role,
                forceFullScreen,
                showCursor,
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<ICabinetDetectionService>())
        {
        }

        private WindowToScreenMapper(
            DisplayRole role,
            bool forceFullScreen,
            bool? showCursor,
            IPropertiesManager properties,
            ICabinetDetectionService cabinetDetectionService)
        {
            _role = role;
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _fullScreen = forceFullScreen || properties.IsFullScreen();
            _showCursor = showCursor ?? GetShowCursor(properties);
            var cabinetService = cabinetDetectionService ??
                                 throw new ArgumentNullException(nameof(cabinetDetectionService));

            _device = cabinetService.GetDisplayDeviceByItsRole(role);
            if (!_fullScreen && IsPortraitOnTwoDisplays(role))
            {
                _device = _dualScreenDevice;
            }
            Logger.Debug($"Construct role:{role} portrait-on-2:{IsPortraitOnTwoDisplays(role)}");
        }

        /// <summary>
        ///     Gets a rectangle representing the screen bounds for the given IDisplayDevice.
        /// </summary>
        /// <param name="displayDevice">The display device.</param>
        /// <returns>The screen bounds. The origin point is at the top left of the virtual screen space (composed of all screens).</returns>
        public Rectangle GetScreenBounds(IDisplayDevice displayDevice)
        {
            TryGetScreenForDisplayDevice(displayDevice, out var screen);
            return GetScreenBounds(screen);
        }

        /// <summary>
        ///     Gets a rectangle representing the screen bounds.
        /// </summary>
        /// <param name="screen">The screen.</param>
        /// <returns>The screen bounds. The origin point is at the top left of the virtual screen space (composed of all screens).</returns>
        public Rectangle GetScreenBounds(Screen screen)
        {
            Logger.Debug($"GetScreenBounds() primary?{screen.Primary} fullscreen?{_fullScreen}");
            var screenSize = _dualScreenDevice != null ? _dualScreenDevice.WorkingArea : screen.WorkingArea;
            var screenBounds = _dualScreenDevice != null ? _dualScreenDevice.Bounds : screen.Bounds;
            Logger.Debug($"GetScreenBounds() new screen size: {screenSize.Left},{screenSize.Top} {screenSize.Width}x{screenSize.Height}");
            Logger.Debug($"GetScreenBounds() new screen bounds: {screenBounds.Left},{screenBounds.Top} {screenBounds.Width}x{screenBounds.Height}");
            var ratio = screen.Primary && _fullScreen
                ? 1.0
                : Math.Max(
                    (double)screenBounds.Width / screenSize.Width,
                    (double)screenBounds.Height / screenSize.Height);
            Logger.Debug($"GetScreenBounds() ratio={ratio}");
            var rect = screen.Primary && _fullScreen
                ? new Rectangle(
                    0,
                    0,
                    (int)SystemParameters.PrimaryScreenWidth,
                    (int)SystemParameters.PrimaryScreenHeight)
                : screenBounds;
            Logger.Debug($"GetScreenBounds() new rect = {rect.Left},{rect.Top} {rect.Width}x{rect.Height}");
            var left = (int)Math.Floor(rect.Left / ratio);
            var top = (int)Math.Floor(rect.Top / ratio);
            var right = (int)Math.Ceiling(rect.Right / ratio);
            var bottom = (int)Math.Ceiling(rect.Bottom / ratio);
            rect.X = left;
            rect.Y = top;
            rect.Width = right - left;
            rect.Height = bottom - top;
            Logger.Debug($"GetScreenBounds() final rect = {rect.Left},{rect.Top} {rect.Width}x{rect.Height}");
            return rect;
        }

        public bool GetShowCursor(IPropertiesManager properties)
        {
            var showMouseCursor = properties.GetValue(Constants.ShowMouseCursor, Constants.False);

            showMouseCursor = showMouseCursor.ToUpperInvariant();

            return showMouseCursor == Constants.True;
        }

        public void MapWindow(Window window)
        {
            if (window == null)
            {
                Logger.Warn($"Invalid arguments for role: {_role}, window is null.");
                return;
            }

            var isCorrectScreen = TryGetScreenForDisplayDevice(_device, out var screen);
            if (!isCorrectScreen)
            {
                Logger.Error($"Invalid arguments for role: {_role} screen is null.");
                return;
            }

            if (!_properties.IsFullScreen())
            {
                SetWindowMode(window, screen);
                return;
            }

            if (!_showCursor)
            {
                window.Cursor = Cursors.None;
                Cursor.Hide();
            }
            else
            {
                window.Cursor = Cursors.Arrow;
                Cursor.Show();
            }

            SetFullscreenMode(window, screen, true);
        }

        /// <summary>
        ///     Gets the portion of the screen which is physically visible. Some VBD screens have only a subsection of the
        ///     screen actually visible to players.
        /// </summary>
        /// <returns>A rectangle for the visible area of the screen. This origin </returns>
        public Rectangle GetVisibleArea()
        {
            var isCorrectScreen = TryGetScreenForDisplayDevice(_device, out var screen);
            return GetVisibleArea(screen, isCorrectScreen);
        }

        public static bool IsPortrait()
        {
            if (_isPortrait.HasValue)
            {
                return _isPortrait.Value;
            }

            var properties = ServiceManager.GetInstance().TryGetService<IPropertiesManager>();
            if (properties == null)
            {
                return false;
            }

            _isPortrait = false;

            if (properties.IsFullScreen())
            {
                var cabinet = ServiceManager.GetInstance().TryGetService<ICabinetDetectionService>();
                if (cabinet == null)
                {
                    return false;
                }

                var main = cabinet.GetDisplayDeviceByItsRole(DisplayRole.Main);
                if ((main.WorkingArea.Width > main.WorkingArea.Height) &&
                    (main.Rotation == DisplayRotation.Degrees90 || main.Rotation == DisplayRotation.Degrees270))
                {
                    _isPortrait = true;
                }
            }
            else
            {
                var width = properties.GetValue(Constants.WindowedScreenWidthPropertyName, Constants.DefaultWindowedWidth);
                var height = properties.GetValue(Constants.WindowedScreenHeightPropertyName, Constants.DefaultWindowedHeight);
                _isPortrait = int.Parse(height) > int.Parse(width);
            }

            return _isPortrait.Value;
        }

        private static bool IsPortraitOnTwoDisplays(DisplayRole role)
        {
            if (!IsPortrait())
            {
                return false;
            }

            if (_isPortraitOnTwoDisplays.HasValue)
            {
                return _isPortraitOnTwoDisplays.Value;
            }

            _isPortraitOnTwoDisplays = false;

            if (role == DisplayRole.Main)
            {
                var cabinet = ServiceManager.GetInstance().TryGetService<ICabinetDetectionService>();
                if (cabinet == null)
                {
                    return false;
                }

                var topDevice = cabinet.GetDisplayDeviceByItsRole(DisplayRole.Top);
                if (topDevice != null)
                {
                    var mainDevice = cabinet.GetDisplayDeviceByItsRole(DisplayRole.Main);

                    if (TryGetScreenForDisplayDevice(topDevice, out var topScreen) &&
                        TryGetScreenForDisplayDevice(mainDevice, out var mainScreen))
                    {
                        _dualScreenDevice = new DoubleDisplayDevice(topDevice, mainDevice, topScreen, mainScreen);
                        _isPortraitOnTwoDisplays = true;
                    }
                }
            }

            return _isPortraitOnTwoDisplays.Value;
        }

        private static bool TryGetScreenForDisplayDevice(IDisplayDevice displayDevice, out Screen screen)
        {
            if (displayDevice is DoubleDisplayDevice doubleDisplayDevice)
            {
                screen = doubleDisplayDevice.DesiredScreen;
                return true;
            }

            screen = Screen.AllScreens.FirstOrDefault(x => x.DeviceName == displayDevice?.DeviceName);
            var isCorrectScreen = screen != null;
            screen = screen ?? Screen.PrimaryScreen;
            return isCorrectScreen;
        }

        private void SetWindowSize(Window window, Rectangle rect)
        {
            Logger.Debug($"Resizing window {window.Title} to {rect}");
            if ((int)window.Top != rect.Top)
            {
                window.Top = rect.Top;
            }

            if ((int)window.Left != rect.Left)
            {
                window.Left = rect.Left;
            }

            if ((int)window.Width != rect.Width)
            {
                window.Width = rect.Width;
            }

            if ((int)window.Height != rect.Height)
            {
                window.Height = rect.Height;
            }
        }

        private void SetWindowMode(Window window, Screen screen)
        {
            Logger.Debug("SetWindowMode");
            window.ResizeMode = ResizeMode.CanResize;
            window.WindowStyle = window.AllowsTransparency ? WindowStyle.None : WindowStyle.SingleBorderWindow;

            var rect = GetScreenBounds(screen);
            Logger.Debug($"SetWindowMode start rect={rect.Left},{rect.Top} {rect.Width}x{rect.Height}");
            Logger.Debug($"SetWindowMode incoming window={window.Width}x{window.Height}");
            rect.X += 10;
            rect.Y += 10;
            rect.Width = Math.Min((int)window.Width, rect.Width) - 20;
            rect.Height = Math.Min((int)window.Height, rect.Height) - 20;
            Logger.Debug($"SetWindowMode final rect={rect.Left},{rect.Top} {rect.Width}x{rect.Height}");

            SetWindowSize(window, rect);
        }

        private void SetFullscreenMode(Window window, Screen screen, bool isCorrectScreen)
        {
            var rect = GetVisibleArea(screen, isCorrectScreen);

            SetWindowSize(window, rect);
            window.ResizeMode = ResizeMode.NoResize;
            window.WindowStyle = WindowStyle.None;
        }

        private Rectangle GetVisibleArea(Screen screen, bool isCorrectScreen)
        {
            var rect = GetScreenBounds(screen);
            if (!isCorrectScreen || _device == null || rect != screen.Bounds)
            {
                return rect;
            }

            var visibleArea = new Rectangle(
                _device.VisibleArea.XPos,
                _device.VisibleArea.YPos,
                _device.VisibleArea.Width,
                _device.VisibleArea.Height);
            if (_device.Rotation == DisplayRotation.Degrees90 || _device.Rotation == DisplayRotation.Degrees270)
            {
                var tmp = visibleArea.Width;
                visibleArea.Width = visibleArea.Height;
                visibleArea.Height = tmp;
            }

            Logger.Debug($"Visible area for {_device.Role} = {visibleArea}, screen bounds = {rect}");
            if (!visibleArea.Size.IsEmpty)
            {
                rect.Size = visibleArea.Size;
            }

            rect.X += visibleArea.X;
            rect.Y += visibleArea.Y;

            return rect;
        }
    }
}
