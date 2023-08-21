namespace Aristocrat.Monaco.Application.Contracts.Media
{
    /// <summary>
    /// The <see cref="HideMediaPlayerEvent" /> is triggered when a hide media player event occurs
    /// </summary>
    public class HideMediaPlayerEvent : MediaPlayerEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HideMediaPlayerEvent" /> class
        /// </summary>
        /// <param name="mediaPlayer">The media player instance</param>
        public HideMediaPlayerEvent(IMediaPlayer mediaPlayer) : base(mediaPlayer)
        {
        }
    }
}
