namespace Aristocrat.Monaco.Application.Contracts.Media
{
    /// <summary>
    /// The <see cref="RaiseMediaPlayerEvent" /> is triggered when a raise media player event occurs
    /// </summary>
    public class RaiseMediaPlayerEvent : MediaPlayerEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RaiseMediaPlayerEvent" /> class
        /// </summary>
        /// <param name="mediaPlayer">The media player instance</param>
        public RaiseMediaPlayerEvent(IMediaPlayer mediaPlayer) : base(mediaPlayer)
        {
        }
    }
}
