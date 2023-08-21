namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using System;
    using System.Globalization;

    /// <summary>Definition of the Note Acceptor NoteUpdatedEvent class.</summary>
    /// <remarks>
    ///     This event is posted by the Note Acceptor Adapter when a note denoms are updated.
    /// </remarks>
    [Serializable]
    public class NoteUpdatedEvent : NoteAcceptorBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteUpdatedEvent" /> class.
        /// </summary>
        public NoteUpdatedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteUpdatedEvent" /> class.
        /// </summary>
        /// <param name="noteAcceptorId">The ID of the note acceptor associated with the event.</param>
        public NoteUpdatedEvent(int noteAcceptorId)
            : base(noteAcceptorId)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}",
                GetType().Name);
        }
    }
}