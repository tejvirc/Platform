namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    using System;

    /// <summary>Definition of the ID reader SelfTestPassedEvent class.</summary>
    /// <remarks>
    ///     This event is posted by the ID reader Service when handling an implementation
    ///     event indicating that a self test has passed.
    /// </remarks>
    [Serializable]
    public class SelfTestPassedEvent : IdReaderBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SelfTestPassedEvent"/> class.
        /// </summary>
        public SelfTestPassedEvent()
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SelfTestPassedEvent"/> class.Initializes a new instance of the
        ///     SelfTestPassedEvent class with the ID reader's ID.
        /// </summary>
        /// <param name="idReaderId">The associated ID reader's ID.</param>
        public SelfTestPassedEvent(int idReaderId)
            : base(idReaderId)
        { }
    }
}
