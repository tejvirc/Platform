namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    using System;

    /// <summary>Definition of the SystemErrorEvent class.</summary>
    /// <remarks>This event is posted if the ID reader has detected any other error except Jam and Paper Low.</remarks>
    [Serializable]
    public class SystemErrorEvent : IdReaderBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemErrorEvent" /> class.
        /// </summary>
        public SystemErrorEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemErrorEvent" /> class.Initializes a new instance of the
        ///     SystemErrorEvent class with the ID reader's ID.
        /// </summary>
        /// <param name="idReaderId">The associated ID reader's ID.</param>
        public SystemErrorEvent(int idReaderId)
            : base(idReaderId)
        {
        }
    }
}
