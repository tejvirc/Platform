namespace Aristocrat.Monaco.Application.Contracts.Media
{
    /// <summary>
    ///     The <see cref="MediaPlayerGamePlaySuspenseChangedEvent" /> is triggered when a media player's game-play-suspended
    ///     attribute has changed.
    /// </summary>
    public class MediaPlayerGamePlaySuspenseChangedEvent : MediaPlayerEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MediaPlayerGamePlaySuspenseChangedEvent" /> class
        /// </summary>
        /// <param name="mediaPlayer">The associated media player instance</param>
        /// <param name="on">True or false</param>
        public MediaPlayerGamePlaySuspenseChangedEvent(IMediaPlayer mediaPlayer, bool on)
            : base(mediaPlayer)
        {
            On = on;
        }

        /// <summary>
        ///     Gets the <see cref="MediaContentError" /> value.
        /// </summary>
        public bool On { get; }
    }
}