namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using System;
    using Properties;
    using static System.FormattableString;

    /// <summary>Definition of the Note Acceptor DocumentRejectedEvent class. </summary>
    /// <remarks>
    ///     This event is posted when the rejected bit is sensed.
    ///     This implementation posts this only when rejected is while stacking.
    /// </remarks>
    [Serializable]
    public class DocumentRejectedEvent : NoteAcceptorBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DocumentRejectedEvent" /> class.
        /// </summary>
        public DocumentRejectedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DocumentRejectedEvent" /> class with the printer's ID.
        /// </summary>
        /// <param name="noteAcceptorId">The ID of the note acceptor associated with the event.</param>
        public DocumentRejectedEvent(int noteAcceptorId)
            : base(noteAcceptorId)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{Resources.NoteAcceptorText} - {Resources.DocumentRejected}");
        }
    }
}