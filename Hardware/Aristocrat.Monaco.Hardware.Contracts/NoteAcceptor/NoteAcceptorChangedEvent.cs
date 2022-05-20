namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using System;
    using Properties;
    using static System.FormattableString;

    /// <summary>Definition of the NoteAcceptorChangedEvent class.</summary>
    /// <remarks>This event is posted when the note acceptor has been changed</remarks>
    [Serializable]
    public class NoteAcceptorChangedEvent : NoteAcceptorBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorChangedEvent"/> class.
        /// </summary>
        public NoteAcceptorChangedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorChangedEvent"/> class with the note acceptor ID.
        /// </summary>
        /// <param name="noteAcceptorId">The associated note acceptor ID.</param>
        public NoteAcceptorChangedEvent(int noteAcceptorId)
            : base(noteAcceptorId)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{Resources.NoteAcceptorText} was changed");
        }

    }
}