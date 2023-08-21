namespace Aristocrat.Monaco.Application.Contracts.Media
{
    /// <summary>
    ///     The <see cref="ReleaseContentMediaPlayerEvent" /> is triggered when a media player is told to release content
    /// </summary>
    public class ReleaseContentMediaPlayerEvent : MediaPlayerEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ReleaseContentMediaPlayerEvent" /> class
        /// </summary>
        /// <param name="mediaPlayer">The associated media player instance</param>
        /// <param name="media">The associated media instance</param>
        public ReleaseContentMediaPlayerEvent(IMediaPlayer mediaPlayer, IMedia media)
            : base(mediaPlayer)
        {
            Media = media;
        }

        /// <summary>
        ///     Gets the media details
        /// </summary>
        public IMedia Media { get; }
    }
}
