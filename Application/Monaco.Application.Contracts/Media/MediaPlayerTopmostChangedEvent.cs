namespace Aristocrat.Monaco.Application.Contracts.Media
{
    /// <summary>
    ///     The <see cref="MediaPlayerTopmostChangedEvent" /> is triggered when a media player's topmost attribute has changed.
    /// </summary>
    public class MediaPlayerTopmostChangedEvent : MediaPlayerEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MediaPlayerTopmostChangedEvent" /> class
        /// </summary>
        /// <param name="mediaPlayer">The associated media player instance</param>
        /// <param name="on">True or false</param>
        public MediaPlayerTopmostChangedEvent(IMediaPlayer mediaPlayer, bool on)
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