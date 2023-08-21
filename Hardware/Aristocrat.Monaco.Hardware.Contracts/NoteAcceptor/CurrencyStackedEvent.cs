namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using System;
    using static System.FormattableString;

    /// <summary>Definition of the Note Acceptor CurrencyStackedEvent class.</summary>
    /// <remarks>This event is posted when Note Acceptor has stacked a document.</remarks>
    [Serializable]
    public class CurrencyStackedEvent : NoteAcceptorBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrencyStackedEvent"/> class.
        /// </summary>
        /// <param name="note">The note.</param>
        public CurrencyStackedEvent(INote note)
        {
            Note = note;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CurrencyStackedEvent"/> class.
        /// </summary>
        /// <param name="noteAcceptorId">Identifier for the note acceptor.</param>
        /// <param name="note">The note.</param>
        public CurrencyStackedEvent(int noteAcceptorId, INote note)
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
