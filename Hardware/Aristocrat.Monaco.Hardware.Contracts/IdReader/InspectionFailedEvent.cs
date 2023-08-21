namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    using System;

    /// <summary>Definition of the InspectionFailedEvent class.</summary>
    /// <remarks>
    ///     The Inspection Failed Event is posted by the ID reader Service when:
    ///     1. An attempt to open the Communication port failed or
    ///     2. A timeout occurred in the Inspecting state.
    /// </remarks>
    [Serializable]
    public class InspectionFailedEvent : IdReaderBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectionFailedEvent" /> class.
        /// </summary>
        public InspectionFailedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectionFailedEvent" /> class.Initializes a new instance of the
        ///     InspectionFailedEvent class with the ID reader's ID.
        /// </summary>
        /// <param name="idReaderId">The associated ID reader's ID.</param>
        public InspectionFailedEvent(int idReaderId)
            : base(idReaderId)
        {
        }
    }
}
