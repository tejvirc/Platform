namespace Aristocrat.Monaco.Application.Contracts.Media
{
    /// <summary>
    /// The <see cref="ShowMediaPlayerEvent" /> is triggered when a show media player event occurs
    /// </summary>
    public class ShowMediaPlayerEvent : MediaPlayerEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ShowMediaPlayerEvent" /> class
        /// </summary>
        /// <param name="mediaPlayer">The media player instance</param>
        public ShowMediaPlayerEvent(IMediaPlayer mediaPlayer) : base(mediaPlayer)
        { }
    }
}
