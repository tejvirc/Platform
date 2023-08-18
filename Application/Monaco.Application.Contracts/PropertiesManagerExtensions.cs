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
