namespace Aristocrat.Monaco.Kernel.Contracts
{
    /// <summary>
    ///     A MediaAltered Event is posted whenever a media is altered.
    /// </summary>
    public class MediaAlteredEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MediaAlteredEvent" /> class.
        /// </summary>
        /// <param name="mediaType">The media kind being altered</param>
        /// <param name="reasonForChange">Why the media is being altered</param>
        /// <param name="mediaDescriptor">Description details about the media</param>
        public MediaAlteredEvent(string mediaType, string reasonForChange, string mediaDescriptor)
        {
            MediaType = mediaType;
            ReasonForChange = reasonForChange;
            MediaDescriptor = mediaDescriptor;
        }

        /// <summary>
        ///     Gets the MediaType
        /// </summary>
        public string MediaType { get; }

        /// <summary>
        ///     Gets the reason for media alteration
        /// </summary>
        public string ReasonForChange { get; }

        /// <summary>
        ///     Gets the description of the media used for validation
        /// </summary>
        public string MediaDescriptor { get; }
    }
}