namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    using System;

    /// <summary>Definition of the ID reader SelfTestFailedEvent class.</summary>
    /// <remarks>
    ///     This event is posted by the ID reader Service when handling an implementation
    ///     event indicating that a self test has failed.
    /// </remarks>
    [Serializable]
    public class SelfTestFailedEvent : IdReaderBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SelfTestFailedEvent"/> class.
        /// </summary>
        public SelfTestFailedEvent()
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SelfTestFailedEvent"/> class.Initializes a new instance of the
        ///     SelfTestFailedEvent class with the ID reader's ID.
        /// </summary>
        /// <param name="idReaderId">The associated ID reader's ID.</param>
        public SelfTestFailedEvent(int idReaderId)
            : base(idReaderId)
        { }
    }
}
