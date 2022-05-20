namespace Aristocrat.Monaco.Hardware.Contracts
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
        ///     Open the on-screen keyboard
        /// </summary>
        void OpenOnScreenKeyboard();
    }
}
