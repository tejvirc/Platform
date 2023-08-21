namespace Aristocrat.Monaco.Application.Contracts.Media
{
    using Kernel;

    /// <summary>
    ///     The <see cref="MediaPlayerContentResultEvent" /> is triggered to send content result events to G2S host.
    /// </summary>
    public class MediaPlayerContentResultEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MediaPlayerContentResultEvent" /> class
        /// </summary>
        /// <param name="mediaPlayerId">The associated media player ID</param>
        /// <param name="media">The associated media instance</param>
        public MediaPlayerContentResultEvent(int mediaPlayerId, IMedia media)
        {
            MediaPlayerId = mediaPlayerId;
            Media = media;
        }

        /// <summary>
        ///     Gets the media player ID
        /// </summary>
        public int MediaPlayerId { get; }

        /// <summary>
        ///     Gets the media details
        /// </summary>
        public IMedia Media { get; }
    }
}
