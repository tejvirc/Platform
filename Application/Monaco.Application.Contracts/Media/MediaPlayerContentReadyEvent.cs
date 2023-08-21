namespace Aristocrat.Monaco.Application.Contracts.Media
{
    using Kernel;

    /// <summary>
    ///     The <see cref="MediaPlayerContentReadyEvent" /> is triggered when a media player has loaded and
    ///     activated some content.
    /// </summary>
    public class MediaPlayerContentReadyEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MediaPlayerContentReadyEvent" /> class
        /// </summary>
        /// <param name="mediaPlayerId">The associated media player ID</param>
        /// <param name="media">The associated media instance</param>
        /// <param name="contentError">Any <see cref="MediaContentError"/> associated with the <see cref="Media"/> instance</param>
        public MediaPlayerContentReadyEvent(int mediaPlayerId, IMedia media, MediaContentError contentError)
        {
            MediaPlayerId = mediaPlayerId;
            Media = media;
            ContentError = contentError;
        }

        /// <summary>
        ///     Gets the media player ID
        /// </summary>
        public int MediaPlayerId { get; }

        /// <summary>
        ///     Gets the media details
        /// </summary>
        public IMedia Media { get; }

        /// <summary>
        ///     Gets the <see cref="MediaContentError"/> value.
        /// </summary>
        public MediaContentError ContentError { get; }
    }
}
