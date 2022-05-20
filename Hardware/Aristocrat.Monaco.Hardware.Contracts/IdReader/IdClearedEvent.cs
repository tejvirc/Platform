namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    using System;

    /// <summary>Definition of the ID Reader IdClearedEvent class.</summary>
    /// <remarks>This event is posted when ID Reader becomes disabled.</remarks>
    [Serializable]
    public class IdClearedEvent : IdReaderBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="IdClearedEvent"/> class.
        /// </summary>
        public IdClearedEvent()
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IdClearedEvent"/> class.
        /// </summary>
        /// <param name="idReaderId">The associated ID Reader's ID.</param>
        public IdClearedEvent(int idReaderId)
            : base(idReaderId)
        { }
    }
}
