namespace Aristocrat.Monaco.Application.UI.Input
{
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
        public void OpenKeyboard(object targetControl);
    }
}
