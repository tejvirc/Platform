namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    using System;

    /// <summary>Definition of the SystemOkEvent class.</summary>
    /// <remarks>This event is posted if a previous system error is now clear.</remarks>
    [Serializable]
    public class SystemOkEvent : IdReaderBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemOkEvent" /> class.
        /// </summary>
        public SystemOkEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemOkEvent" /> class.Initializes a new instance of the
        ///     SystemOkEvent
        ///     class with the ID reader's ID.
        /// </summary>
        /// <param name="idReaderId">The associated ID reader's ID.</param>
        public SystemOkEvent(int idReaderId)
            : base(idReaderId)
        {
        }
    }
}
