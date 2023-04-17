namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using System;
    using Properties;

    /// <summary>Definition of the InspectedEvent class.</summary>
    /// <remarks>
    ///     The Inspected Event is posted by the Reel Controller Service
    /// </remarks>
    [Serializable]
    public class InspectedEvent : ReelControllerBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectedEvent" /> class.
        /// </summary>
        public InspectedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectedEvent" /> class.Initializes a new instance of the
        ///     InspectedEvent class with the reel controller ID.
        /// </summary>
        /// <param name="reelControllerId">The associated reel controller ID.</param>
        public InspectedEvent(int reelControllerId)
            : base(reelControllerId)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Resources.ReelControllerText} {Resources.InspectionFailedText} {Resources.ClearedText}";
        }
    }
}