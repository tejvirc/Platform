namespace Aristocrat.Monaco.Gaming.UI.Events
{
    using Aristocrat.Monaco.Kernel;

    /// <summary>
    /// Definition of the HandCountTimerOverlayVisibilityChangedEvent class.
    /// </summary>
    public class HandCountTimerOverlayVisibilityChangedEvent : BaseEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HandCountTimerOverlayVisibilityChangedEvent" /> class.
        /// </summary>
        /// <param name="isVisible">True if overlay window is visible</param>
        public HandCountTimerOverlayVisibilityChangedEvent(bool isVisible)
        {
            IsVisible = isVisible;
        }

        /// <summary>
        ///     True if visible
        /// </summary>
        public bool IsVisible { get; }
    }
}
