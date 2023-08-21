namespace Aristocrat.Monaco.Application.Contracts.Media
{
    using Kernel;

    /// <summary>
    ///     This event should be posted whenever the main view size has changed
    /// </summary>
    public class ViewResizeEvent : BaseEvent
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resizing">True if resizing has started, False if resizing has stopped</param>
        public ViewResizeEvent(bool resizing)
        {
            Resizing = resizing;
        }

        /// <summary>
        /// True if resizing has started, False is resizing has stopped
        /// </summary>
        public bool Resizing { get; }
    }
}
