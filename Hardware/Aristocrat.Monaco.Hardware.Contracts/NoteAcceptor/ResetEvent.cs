namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using System;

    /// <summary>Definition of the ResetEvent class.</summary>
    /// <remarks>
    ///     This event is posted by the NoteAcceptor. In the case of the NoteAcceptorGDS,
    ///     the event is posted when the Power Up bit is detected in a status response.
    /// </remarks>
    [Serializable]
    public class ResetEvent : NoteAcceptorBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ResetEvent"/> class.
        /// </summary>
        public ResetEvent()
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResetEvent"/> class.
        /// </summary>
        /// <param name="noteAcceptorId">The ID of the note acceptor associated with the event.</param>
        public ResetEvent(int noteAcceptorId)
            : base(noteAcceptorId)
        { }
    }
}
