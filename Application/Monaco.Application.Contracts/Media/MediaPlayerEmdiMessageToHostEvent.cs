namespace Aristocrat.Monaco.Application.Contracts.Media
{
    /// <summary>
    ///     The <see cref="MediaPlayerEmdiMessageToHostEvent" /> is triggered when media player
    ///     is sending a mediaPlayer:hostToContentMessage to the G2S host.
    /// </summary>
    public class MediaPlayerEmdiMessageToHostEvent : MediaPlayerEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MediaPlayerEmdiMessageToHostEvent" /> class
        /// </summary>
        /// <param name="mediaPlayer">The associated media player instance</param>
        /// <param name="base64Message">Base64-encoded data</param>
        public MediaPlayerEmdiMessageToHostEvent(IMediaPlayer mediaPlayer, string base64Message) : base(mediaPlayer)
        {
            Base64Message = base64Message;
        }

        /// <summary>
        ///     Gets the message
        /// </summary>
        public string Base64Message { get; }
    }
}