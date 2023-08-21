namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    using System;

    /// <summary>Definition of the InspectedEvent class.</summary>
    /// <remarks>
    ///     The Initialized Event is posted by the IdReader Service in response
    ///     to an ImplementationEventId.IdReaderFirmwareVersion with a logical state of Inspecting.
    /// </remarks>
    [Serializable]
    public class InspectedEvent : IdReaderBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectedEvent" /> class.
        /// </summary>
        public InspectedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectedEvent" /> class.Initializes a new instance of the
        ///     InspectedEvent class with the ID reader's ID.
        /// </summary>
        /// <param name="idReaderId">The associated ID reader's ID.</param>
        public InspectedEvent(int idReaderId)
            : base(idReaderId)
        {
        }
    }
}
