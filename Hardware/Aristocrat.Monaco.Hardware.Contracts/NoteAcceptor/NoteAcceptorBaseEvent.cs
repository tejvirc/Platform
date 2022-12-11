namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using Properties;
    using System;
    using Kernel;
    using ProtoBuf;
    using static System.FormattableString;
    using System.Runtime.Serialization;

    /// <summary>Definition of the NoteAcceptorBaseEvent class.</summary>
    /// <remarks>All other note acceptor events are derived from this event.</remarks>
    [ProtoContract]
    public class NoteAcceptorBaseEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorBaseEvent"/> class.
        /// </summary>
        protected NoteAcceptorBaseEvent()
        {
            NoteAcceptorId = 1;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorBaseEvent"/> class.
        /// </summary>
        /// <param name="noteAcceptorId">The ID of the note acceptor associated with the event.</param>
        protected NoteAcceptorBaseEvent(int noteAcceptorId)
        {
            NoteAcceptorId = noteAcceptorId;
        }

        /// <summary>
        ///     Gets the ID of the note acceptor associated with the event.
        /// </summary>
        [ProtoMember(1)]
        public int NoteAcceptorId { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{Resources.NoteAcceptorText} {GetType().Name}");
        }
    }
}
