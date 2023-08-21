namespace Aristocrat.Monaco.Application.Contracts.Media
{
    /// <summary>
    ///     The <see cref="MediaPlayerModalChangedEvent" /> is triggered when a media player's modal attribute has changed.
    /// </summary>
    public class MediaPlayerModalChangedEvent : MediaPlayerEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MediaPlayerModalChangedEvent" /> class
        /// </summary>
        /// <param name="mediaPlayer">The associated media player instance</param>
        /// <param name="on">True or false</param>
        public MediaPlayerModalChangedEvent(IMediaPlayer mediaPlayer, bool on)
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