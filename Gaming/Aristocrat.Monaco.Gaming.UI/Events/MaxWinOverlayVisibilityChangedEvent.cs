namespace Aristocrat.Monaco.Gaming.UI.Events
{
    using Aristocrat.Monaco.Kernel;

    public class MaxWinOverlayVisibilityChangedEvent : BaseEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaxWinOverlayVisibilityChangedEvent" /> class.
        /// </summary>
        /// <param name="isVisible">True if overlay window is visible</param>
        public MaxWinOverlayVisibilityChangedEvent(bool isVisible)
        {
            IsVisible = isVisible;
        }

        /// <summary>
        ///     True if visible
        /// </summary>
        public bool IsVisible { get; }
    }
}
