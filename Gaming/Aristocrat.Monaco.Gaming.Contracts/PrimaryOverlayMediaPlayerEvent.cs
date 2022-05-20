namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>
    ///     This event is triggered when there is a primary overlay media player visibility change
    /// </summary>
    public class PrimaryOverlayMediaPlayerEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor for event
        /// </summary>
        /// <param name="isShowing"></param>
        public PrimaryOverlayMediaPlayerEvent(bool isShowing)
        {
            IsShowing = isShowing;
        }

        /// <summary>Value indicating if the overlay is currently showing or hiding </summary>
        public bool IsShowing { get; }
    }
}