namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using System;
    using Properties;

    /// <summary>Definition of the reel controller InspectionFailedEvent class.</summary>
    /// <remarks>
    ///     This event is posted by the Reel Controller Service when the Reel Controller fails to initialize communication
    ///     or fails to get device information before a timeout occurred.
    /// </remarks>
    [Serializable]
    public class InspectionFailedEvent : ReelControllerBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectionFailedEvent"/> class.
        /// </summary>
        public InspectionFailedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectionFailedEvent"/> class.
        /// </summary>
        /// <param name="reelControllerId">The ID of the reel controller associated with the event.</param>
        public InspectionFailedEvent(int reelControllerId)
            : base(reelControllerId)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Resources.ReelControllerText} {Resources.InspectionFailedText}";
        }
    }
}