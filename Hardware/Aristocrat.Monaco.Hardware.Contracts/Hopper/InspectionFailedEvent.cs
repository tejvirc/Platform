namespace Aristocrat.Monaco.Hardware.Contracts.Hopper
{
    using Properties;
    using System;

    /// <summary>Definition of the InspectionFailedEvent class.</summary>
    /// <remarks>
    ///     The Inspection Failed Event is posted by the Hopper Service when:
    ///     1. An attempt to open the Communication port failed or
    ///     2. A timeout occurred in the Inspecting state.
    /// </remarks>
    [Serializable]
    public class InspectionFailedEvent : HopperBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectionFailedEvent" /> class.
        /// </summary>
        public InspectionFailedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectionFailedEvent" /> class.Initializes a new instance of the
        ///     InspectionFailedEvent class with the hopper's ID.
        /// </summary>
        /// <param name="hopperId">The associated hopper's ID.</param>
        public InspectionFailedEvent(int hopperId)
            : base(hopperId)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Hopper {Resources.InspectionFailedText}";
        }
    }
}
