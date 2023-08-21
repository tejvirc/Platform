namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    using System;

    /// <summary>Definition of the ID Reader ReadErrorEvent class.</summary>
    /// <remarks>This event is posted when ID Reader becomes disabled.</remarks>
    [Serializable]
    public class ReadErrorEvent : IdReaderBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ReadErrorEvent"/> class.
        /// </summary>
        public ReadErrorEvent()
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReadErrorEvent"/> class.
        /// </summary>
        /// <param name="idReaderId">The associated ID Reader's ID.</param>
        public ReadErrorEvent(int idReaderId)
            : base(idReaderId)
        { }
    }
}
