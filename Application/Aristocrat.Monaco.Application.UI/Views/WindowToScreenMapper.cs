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
    using Cursors = System.Windows.Input.Cursors;

    public class WindowToScreenMapper
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPropertiesManager _properties;
        private readonly IDisplayDevice _device = null;
        private readonly DisplayRole _role;
        private readonly bool _fullScreen;
        private readonly bool _showCursor;

        public WindowToScreenMapper(DisplayRole role, bool? fullScreen = null, bool? showCursor = null)
            : this(
                role,
                fullScreen,
                showCursor,
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<ICabinetDetectionService>())
        {
        }

        private WindowToScreenMapper(
            DisplayRole role,
            bool? fullScreen,
            bool? showCursor,
            IPropertiesManager properties,
            ICabinetDetectionService cabinetDetectionService)
        {
            _role = role;
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _fullScreen = fullScreen ?? properties.IsFullScreen();
            _showCursor = showCursor ?? GetShowCursor(properties);
            var cabinetService = cabinetDetectionService ??
                                 throw new ArgumentNullException(nameof(cabinetDetectionService));
            _device = cabinetService.GetDisplayDeviceByItsRole(role);

            var requestedWidth = int.Parse(properties.GetValue(Constants.WindowedScreenWidthPropertyName, Constants.DefaultWindowedWidth));
            var requestedHeight = int.Parse(properties.GetValue(Constants.WindowedScreenHeightPropertyName, Constants.DefaultWindowedHeight));
            if (role == DisplayRole.Main && !_fullScreen && requestedHeight >= requestedWidth)
            {
                SecondDevice = cabinetDetectionService.GetDisplayDeviceByItsRole(DisplayRole.Top);
                Logger.Debug($"Get second device: {SecondDevice.DeviceName}");
            }
        }

        public IDisplayDevice SecondDevice { get; } = null;

        /// <summary>
        ///     Gets a rectangle representing the screen bounds for the given IDisplayDevice.
        /// </summary>
        /// <param name="displayDevice">The display device.</param>
        /// <param name="secondDevice">Optional second display device to be used along with main one</param>
        /// <returns>The screen bounds. The origin point is at the top left of the virtual screen space (composed of all screens).</returns>
        public static Rectangle GetScreenBounds(IDisplayDevice displayDevice, IDisplayDevice secondDevice = null, bool? fullScreen = null)
        {
            TryGetScreenForDisplayDevice(displayDevice, out var screen);
            Screen secondScreen = null;
            if (secondDevice is not null)
            {
                TryGetScreenForDisplayDevice(secondDevice, out secondScreen);
            }
            return GetScreenBounds(screen, secondScreen);
        }

        /// <summary>
        ///     Gets a rectangle representing the screen bounds.
        /// </summary>
        /// <param name="screen">The screen.</param>
        /// <param name="secondScreen">Optional second screen to use in calculations of a window</param>
        /// <returns>The screen bounds. The origin point is at the top left of the virtual screen space (composed of all screens).</returns>
        public static Rectangle GetScreenBounds(Screen screen, Screen secondScreen = null, bool? fullScreen = null)
        {
            var isFullscreen = fullScreen ?? ServiceManager.GetInstance().GetService<IPropertiesManager>().IsFullScreen();
            Logger.Debug($"GetScreenBounds() primary?{screen.Primary} fullscreen?{isFullscreen}");
            var screenSize = screen.WorkingArea;
            var screenBounds = screen.Bounds;
            if (screen.Primary && secondScreen is not null)
            {
                Logger.Debug("Adding optional second screen...");
                screenSize.Height += secondScreen.WorkingArea.Height;
                screenSize.Y = secondScreen.WorkingArea.Y;
                screenBounds.Height += secondScreen.Bounds.Height;
                screenBounds.Y = secondScreen.Bounds.Y;
            }
            Logger.Debug($"GetScreenBounds() new screen size: {screenSize.Left},{screenSize.Top} {screenSize.Width}x{screenSize.Height}");
            Logger.Debug($"GetScreenBounds() new screen bounds: {screenBounds.Left},{screenBounds.Top} {screenBounds.Width}x{screenBounds.Height}");
            var ratio = screen.Primary && isFullscreen
                ? 1.0
                : Math.Max(
                    (double)screenBounds.Width / screenSize.Width,
                    (double)screenBounds.Height / screenSize.Height);
            Logger.Debug($"GetScreenBounds() ratio={ratio}");
            var rect = screen.Primary && isFullscreen
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

        public static bool GetShowCursor(IPropertiesManager properties)
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

        private static bool TryGetScreenForDisplayDevice(IDisplayDevice displayDevice, out Screen screen)
        {
            screen = Screen.AllScreens.FirstOrDefault(x => x.DeviceName == displayDevice?.DeviceName);
            var isCorrectScreen = screen != null;
            screen = screen ?? Screen.PrimaryScreen;
            return isCorrectScreen;
        }

        private static void SetWindowSize(Window window, Rectangle rect)
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

        private static void SetWindowMode(Window window, Screen screen)
        {
            Logger.Debug("SetWindowMode");
            window.ResizeMode = ResizeMode.CanResize;
            window.WindowStyle = window.AllowsTransparency ? WindowStyle.None : WindowStyle.SingleBorderWindow;

            var rect = GetScreenBounds(screen, fullScreen: false);
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
            var rect = GetScreenBounds(screen, fullScreen: _fullScreen);
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