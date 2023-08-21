namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using System;

    /// <summary>Definition of the Note Acceptor SelfTestFailedEvent class.</summary>
    /// <remarks>
    ///     This event is posted by the Note Acceptor Service when handling an implementation
    ///     event indicating that a self test has failed.
    /// </remarks>
    [Serializable]
    public class SelfTestFailedEvent : NoteAcceptorBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SelfTestFailedEvent"/> class.
        /// </summary>
        public SelfTestFailedEvent()
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SelfTestFailedEvent"/> class.
        /// </summary>
        /// <param name="noteAcceptorId">The ID of the note acceptor associated with the event.</param>
        public SelfTestFailedEvent(int noteAcceptorId)
            : base(noteAcceptorId)
        { }
    }
}
