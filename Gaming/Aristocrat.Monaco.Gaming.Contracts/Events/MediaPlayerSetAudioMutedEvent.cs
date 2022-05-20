namespace Aristocrat.Monaco.Gaming.Contracts.Events
{
    using Kernel;

    /// <summary>
    ///     Event used to mute the media player audio.
    /// </summary>
    public class MediaPlayerSetAudioMutedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MediaPlayerSetAudioMutedEvent" /> class.
        /// </summary>
        public MediaPlayerSetAudioMutedEvent(bool mute)
        {
            Mute = mute;
        }

        /// <summary>
        ///     Indicates whether or not the media player audio is muted.
        /// </summary>
        public bool Mute { get; }
    }
}