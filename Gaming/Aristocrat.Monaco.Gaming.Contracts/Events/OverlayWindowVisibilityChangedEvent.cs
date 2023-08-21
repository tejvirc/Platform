namespace Aristocrat.Monaco.Gaming.Contracts.Events
{
    using Kernel;

    /// <summary>
    ///     Event for when the Message Overlay window visibility changes
    /// </summary>
    public class OverlayWindowVisibilityChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="isVisible">True if overlay window is visible</param>
        public OverlayWindowVisibilityChangedEvent(bool isVisible)
        {
            IsVisible = isVisible;
        }

        /// <summary>
        ///     True if visible
        /// </summary>
        public bool IsVisible { get; }
    }
}
