namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using System;

    /// <summary>Definition of the Note Acceptor SelfTestPassedEvent class.</summary>
    /// <remarks>
    ///     This event is posted by the Note Acceptor Service when handling an implementation
    ///     event indicating that a self test has passed.
    /// </remarks>
    [Serializable]
    public class SelfTestPassedEvent : NoteAcceptorBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SelfTestPassedEvent"/> class.
        /// </summary>
        public SelfTestPassedEvent()
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SelfTestPassedEvent"/> class.
        /// </summary>
        /// <param name="noteAcceptorId">The ID of the note acceptor associated with the event.</param>
        public SelfTestPassedEvent(int noteAcceptorId)
            : base(noteAcceptorId)
        { }
    }
}
