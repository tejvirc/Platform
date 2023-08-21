namespace Aristocrat.Monaco.Application.Contracts.Media
{
    /// <summary>
    ///     The <see cref="MediaPlayerEmdiMessageFromHostEvent" /> is triggered when mediaPlayer:hostToContentMessage
    ///     is sent from the G2S host, intended for a media player.
    /// </summary>
    public class MediaPlayerEmdiMessageFromHostEvent : MediaPlayerEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MediaPlayerEmdiMessageFromHostEvent" /> class
        /// </summary>
        /// <param name="mediaPlayer">The associated media player instance</param>
        /// <param name="base64Message">Base64-encoded data</param>
        public MediaPlayerEmdiMessageFromHostEvent(IMediaPlayer mediaPlayer, string base64Message)
            : base(mediaPlayer)
        {
            Base64Message = base64Message;
        }

        /// <summary>
        ///     Gets the message
        /// </summary>
        public string Base64Message { get; }
    }
}