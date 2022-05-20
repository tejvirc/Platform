namespace Aristocrat.Monaco.Application.Contracts.Media
{
    /// <summary>
    ///     The <see cref="MediaPlayerEnabledEvent" /> is triggered when a media player is enabled
    /// </summary>
    public class MediaPlayerEnabledEvent : MediaPlayerEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MediaPlayerEnabledEvent" /> class
        /// </summary>
        /// <param name="mediaPlayer">The enabled media player instance</param>
        /// <param name="status">The reason the player was enabled</param>
        public MediaPlayerEnabledEvent(IMediaPlayer mediaPlayer, MediaPlayerStatus status)
            : base(mediaPlayer)
        {
            Status = status;
        }

        /// <summary>
        ///     Gets the reason the player was enabled
        /// </summary>
        public MediaPlayerStatus Status { get; }
    }
}
