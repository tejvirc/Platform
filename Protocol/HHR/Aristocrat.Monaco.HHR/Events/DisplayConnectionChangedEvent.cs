namespace Aristocrat.Monaco.Hhr.Events
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

        public DisplayConnectionChangedEvent(DisplayRole display, bool isConnected)
        {
            Display = display;
            IsConnected = isConnected;
        }
    }
}