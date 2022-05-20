namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using System;
    using static System.FormattableString;

    /// <summary>Definition of the CurrencyEscrowedEvent class.</summary>
    /// <remarks>This event is posted when the NoteAcceptor has escrowed a currency document.</remarks>
    [Serializable]
    public class CurrencyEscrowedEvent : NoteAcceptorBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrencyEscrowedEvent"/> class.
        /// </summary>
        /// <param name="note">The note.</param>
        public CurrencyEscrowedEvent(INote note)
            : this(1, note)
        {
            Note = note;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrencyEscrowedEvent"/> class.
        /// </summary>
        /// <param name="noteAcceptorId">Identifier for the note acceptor.</param>
        /// <param name="note">The note.</param>
        public CurrencyEscrowedEvent(
            int noteAcceptorId,
            INote note)
            : base(noteAcceptorId)
        {
            Note = note;
        }

        /// <summary>Gets the note.</summary>
        /// <value>The note.</value>
        public INote Note { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [Note={Note}]");
        }
    }
}
