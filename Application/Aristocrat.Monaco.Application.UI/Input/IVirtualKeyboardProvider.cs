namespace Aristocrat.Monaco.Application.UI.Input
{
    using System.Globalization;

    /// <summary>
    ///     Provides virtual keyboard functionality
    /// </summary>
    public interface IVirtualKeyboardProvider
    {
        /// <summary>
        ///     Closes the keyboard
        /// </summary>
        public void CloseKeyboard();

        /// <summary>
        ///     Opens the keyboard
        /// </summary>
        /// <param name="targetControl">The target control of the keyboard</param>
        /// <param name="culture">The culture info to set the keyboard language</param>
        public void OpenKeyboard(object targetControl, CultureInfo culture);
    }
}
