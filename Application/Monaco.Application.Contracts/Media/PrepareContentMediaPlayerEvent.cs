namespace Aristocrat.Monaco.Application.Contracts.Media
{
    /// <summary>
    ///     The <see cref="PrepareContentMediaPlayerEvent" /> is triggered when a media player is told to load content
    /// </summary>
    public class PrepareContentMediaPlayerEvent : MediaPlayerEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrepareContentMediaPlayerEvent" /> class
        /// </summary>
        /// <param name="mediaPlayer">The associated media player instance</param>
        /// <param name="media">The associated media instance</param>
        public PrepareContentMediaPlayerEvent(IMediaPlayer mediaPlayer, IMedia media) : base(mediaPlayer)
        {
            Media = media;
        }

        /// <summary>
        ///     Gets the media details
        /// </summary>
        public IMedia Media { get; }
    }
}