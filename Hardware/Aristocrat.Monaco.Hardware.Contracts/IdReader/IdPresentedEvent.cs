namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    using System;

    /// <summary>Definition of the ID Reader IdPresentedEvent class.</summary>
    /// <remarks>This event is posted when ID Reader is presented with an ID.</remarks>
    [Serializable]
    public class IdPresentedEvent : IdReaderBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="IdPresentedEvent"/> class.
        /// </summary>
        public IdPresentedEvent()
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IdPresentedEvent"/> class.
        /// </summary>
        /// <param name="idReaderId">The associated ID Reader's ID.</param>
        public IdPresentedEvent(int idReaderId)
            : base(idReaderId)
        { }
    }
}
