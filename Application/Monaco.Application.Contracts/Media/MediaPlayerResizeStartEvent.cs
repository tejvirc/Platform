namespace Aristocrat.Monaco.Application.Contracts.Media
{
    using Kernel;

    /// <summary>
    ///     This event should be posted whenever the media display is about to be resized
    /// </summary>
    public class MediaPlayerResizeStartEvent : BaseEvent
    {
        /// <inheritdoc />
        public MediaPlayerResizeStartEvent(int mediaDisplayId)
        {
            MediaDisplayId = mediaDisplayId;
        }

        /// <summary>
        ///     ID of the media display starting resize
        /// </summary>
        public int MediaDisplayId { get; }
    }
}
