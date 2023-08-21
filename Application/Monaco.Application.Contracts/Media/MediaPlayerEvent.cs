namespace Aristocrat.Monaco.Application.Contracts.Media
{
    using Kernel;

    /// <summary>
    ///     The <see cref="MediaPlayerEvent" /> is triggered when a media player event occurs
    /// </summary>
    public abstract class MediaPlayerEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MediaPlayerEvent" /> class
        /// </summary>
        /// <param name="mediaPlayer">The media player instance</param>
        protected MediaPlayerEvent(IMediaPlayer mediaPlayer)
        {
            MediaPlayer = mediaPlayer;
        }

        /// <summary>
        ///     Gets the media player
        /// </summary>
        public IMediaPlayer MediaPlayer { get; }
    }
}
