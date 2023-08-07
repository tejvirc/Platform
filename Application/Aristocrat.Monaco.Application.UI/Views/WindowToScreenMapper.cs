namespace Aristocrat.Monaco.Application.UI.Views
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Forms;
    using Cabinet.Contracts;
    using Hardware.Contracts.Cabinet;
    using Kernel;
    using log4net;
    using Monaco.Common;
    using Cursors = System.Windows.Input.Cursors;

    public class WindowToScreenMapper
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IDisplayDevice _device;
        private readonly DisplayRole _role;
        private readonly bool _showCursor;

        public WindowToScreenMapper(DisplayRole role)
            : this(
                role,
                GetFullscreen(ServiceManager.GetInstance().GetService<IPropertiesManager>()),
                GetShowCursor(ServiceManager.GetInstance().GetService<IPropertiesManager>()),
                ServiceManager.GetInstance().GetService<ICabinetDetectionService>())
        {
        }

        public WindowToScreenMapper(DisplayRole role, bool? fullscreen = null, bool? showCursor = null)
            : this(
                role,
                fullscreen ?? GetFullscreen(ServiceManager.GetInstance().GetService<IPropertiesManager>()),
                showCursor ?? GetShowCursor(ServiceManager.GetInstance().GetService<IPropertiesManager>()),
                ServiceManager.GetInstance().GetService<ICabinetDetectionService>())
        {
        }

        private WindowToScreenMapper(
            DisplayRole role,
            bool fullscreen,
            bool showCursor,
            ICabinetDetectionService cabinetDetectionService)
        {
            _role = role;
            _showCursor = showCursor;
            IsFullscreen = fullscreen;
            var cabinetService = cabinetDetectionService ??
                                 throw new ArgumentNullException(nameof(cabinetDetectionService));
            _device = cabinetService.GetDisplayDeviceByItsRole(role);
        }

        public bool IsFullscreen { get; }

        /// <summary>
        ///     Gets a rectangle representing the screen bounds for the given IDisplayDevice.
        /// </summary>
        /// <param name="displayDevice">The display device.</param>
        /// <returns>The screen bounds. The origin point is at the top left of the virtual screen space (composed of all screens).</returns>
        public static Rectangle GetScreenBounds(IDisplayDevice displayDevice)
        {
            TryGetScreenForDisplayDevice(displayDevice, out var screen);
            return GetScreenBounds(screen);
        }

        /// <summary>
        ///     Gets a rectangle representing the screen bounds.
        /// </summary>
        /// <param name="screen">The screen.</param>
        /// <returns>The screen bounds. The origin point is at the top left of the virtual screen space (composed of all screens).</returns>
        public static Rectangle GetScreenBounds(Screen screen)
        {
            var ratio = screen.Primary
                ? 1.0
                : Math.Max(
                    Screen.PrimaryScreen.Bounds.Width / SystemParameters.PrimaryScreenWidth,
                    Screen.PrimaryScreen.Bounds.Height / SystemParameters.PrimaryScreenHeight);
            var rect = screen.Primary
                ? new Rectangle(
                    0,
                    0,
                    (int)SystemParameters.PrimaryScreenWidth,
                    (int)SystemParameters.PrimaryScreenHeight)
                : screen.Bounds;
            var left = (int)Math.Floor(rect.Left / ratio);
            var top = (int)Math.Floor(rect.Top / ratio);
            var right = (int)Math.Ceiling(rect.Right / ratio);
            var bottom = (int)Math.Ceiling(rect.Bottom / ratio);
            rect.X = left;
            rect.Y = top;
            rect.Width = right - left;
            rect.Height = bottom - top;
            return rect;
        }

        public static bool GetFullscreen(IPropertiesManager properties)
        {
            var display = properties.GetValue(Constants.DisplayPropertyKey, Constants.DisplayPropertyFullScreen);

            display = display.ToUpperInvariant();

            return display == Constants.DisplayPropertyFullScreen;
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

            if (!IsFullscreen)
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
            window.ResizeMode = ResizeMode.CanResize;
            window.WindowStyle = window.AllowsTransparency ? WindowStyle.None : WindowStyle.SingleBorderWindow;

            var rect = GetScreenBounds(screen);
            rect.X += 10;
            rect.Y += 10;
            rect.Width = Math.Min((int)window.Width, rect.Width - 10);
            rect.Height = Math.Min((int)window.Height, rect.Height - 10);

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