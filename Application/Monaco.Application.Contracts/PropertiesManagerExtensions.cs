namespace Aristocrat.Monaco.Application.Contracts
{
    using Common;
    using Hardware.Contracts.Cabinet;
    using Kernel;

    /// <summary>
    ///     Extension methods for the <see cref="IPropertiesManager" /> interface.
    /// </summary>
    public static class PropertiesManagerExtensions
    {
        private static bool? _isPortrait = null;

        /// <summary>
        ///     An <see cref="IPropertiesManager" /> extension method that gets whether the main window is portrait mode.
        /// </summary>
        /// <returns>Whether main window is portrait mode.</returns>
        public static bool IsPortrait(this IPropertiesManager @this)
        {
            if (_isPortrait.HasValue)
            {
                return _isPortrait.Value;
            }

            _isPortrait = false;

            if (@this.IsFullScreen())
            {
                var cabinet = ServiceManager.GetInstance().GetService<ICabinetDetectionService>();
                var main = cabinet.GetDisplayDeviceByItsRole(Cabinet.Contracts.DisplayRole.Main);
                if ((main.WorkingArea.Width > main.WorkingArea.Height) &&
                    (main.Rotation == Cabinet.Contracts.DisplayRotation.Degrees90 || main.Rotation == Cabinet.Contracts.DisplayRotation.Degrees270))
                {
                    _isPortrait = true;
                }
            }
            else
            {
                var width = @this.GetValue(Constants.WindowedScreenWidthPropertyName, Constants.DefaultWindowedWidth);
                var height = @this.GetValue(Constants.WindowedScreenHeightPropertyName, Constants.DefaultWindowedHeight);
                _isPortrait = int.Parse(height) > int.Parse(width);
            }

            return _isPortrait.Value;
        }

        /// <summary>
        ///     An <see cref="IPropertiesManager" /> extension method that gets the full-screen flag.
        /// </summary>
        /// <returns>Whether displays are being run in full screen mode.</returns>
        public static bool IsFullScreen(this IPropertiesManager @this)
        {
            var display = @this.GetValue(Constants.DisplayPropertyKey, Constants.DisplayPropertyFullScreen);

            display = display.ToUpperInvariant();

            return display == Constants.DisplayPropertyFullScreen;
        }
    }
}
