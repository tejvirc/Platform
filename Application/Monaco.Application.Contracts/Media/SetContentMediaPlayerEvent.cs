namespace Aristocrat.Monaco.Application.Contracts.Media
{
    /// <summary>
    ///     The <see cref="SetContentMediaPlayerEvent" /> is triggered when a media player is told to activate content
    /// </summary>
    public class SetContentMediaPlayerEvent : MediaPlayerEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SetContentMediaPlayerEvent" /> class
        /// </summary>
        /// <param name="mediaPlayer">The associated media player instance</param>
        /// <param name="media">The associated media instance</param>
        public SetContentMediaPlayerEvent(IMediaPlayer mediaPlayer, IMedia media)
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