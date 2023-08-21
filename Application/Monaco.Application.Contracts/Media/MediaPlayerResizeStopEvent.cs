namespace Aristocrat.Monaco.Application.Contracts.Media
{
    using Kernel;

    /// <summary>
    ///     This event should be posted whenever the media display has finished resizing
    /// </summary>
    public class MediaPlayerResizeStopEvent : BaseEvent
    {
        /// <inheritdoc />
        public MediaPlayerResizeStopEvent(int mediaDisplayId)
        {
            MediaDisplayId = mediaDisplayId;
        }

        /// <summary>
        ///     ID of the media display which has finished resizing
        /// </summary>
        public int MediaDisplayId { get; }
    }
}
