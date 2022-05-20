namespace Aristocrat.Monaco.Application.Contracts.Media
{
    /// <summary>
    ///     The <see cref="MediaPlayerDisabledEvent" /> is triggered when a media player is enabled
    /// </summary>
    public class MediaPlayerDisabledEvent : MediaPlayerEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MediaPlayerDisabledEvent" /> class
        /// </summary>
        /// <param name="mediaPlayer">The enabled media player instance</param>
        /// <param name="status">The reason the player was enabled</param>
        public MediaPlayerDisabledEvent(IMediaPlayer mediaPlayer, MediaPlayerStatus status) : base(mediaPlayer)
        {
            Status = status;
        }

        /// <summary>
        ///     Gets the reason the player was enabled
        /// </summary>
        public MediaPlayerStatus Status { get; }
    }
}
