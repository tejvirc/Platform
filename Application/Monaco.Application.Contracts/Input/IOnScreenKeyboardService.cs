namespace Aristocrat.Monaco.Application.Contracts.Input
{
    using Kernel;

    /// <summary>
    ///     Service to manage state of On-Screen Keyboard
    /// </summary>
    public interface IOnScreenKeyboardService : IService
    {
        /// <summary>
        ///     Set true to disable keyboard from auto-opening
        ///     Set false to re-enable keyboard auto-open
        /// </summary>
        bool DisableKeyboard { get; set; }

        /// <summary>
        ///     Opens the onscreen keyboard
        /// </summary>
        /// <param name="targetControl">The target control of the keyboard</param>
        void OpenOnScreenKeyboard(object targetControl);
    }
}
