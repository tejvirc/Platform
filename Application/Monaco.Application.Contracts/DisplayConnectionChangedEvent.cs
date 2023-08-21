namespace Aristocrat.Monaco.Application.Contracts
{
    using Cabinet.Contracts;
    using Kernel;

    /// <summary>
    /// Event to signal a display's connection status has changed
    /// </summary>
    public class DisplayConnectionChangedEvent : BaseEvent
    {
        /// <summary>
        ///     The display role
        /// </summary>
        public DisplayRole Display { get; set; }

        /// <summary>
        ///     Is the display connected
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        ///     Creates a DisplayConnectionChangedEvent instance
        /// </summary>
        /// <param name="display">Specifies which <see cref="DisplayRole"/> has changed</param>
        /// <param name="isConnected">Specifies whether the display was connected or disconnected</param>
        public DisplayConnectionChangedEvent(DisplayRole display, bool isConnected)
        {
            Display = display;
            IsConnected = isConnected;
        }
    }
}