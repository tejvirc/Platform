namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using Properties;
    using System;

    /// <summary>Definition of the note acceptor ConnectedEvent class.</summary>
    /// <remarks>This event is posted when note acceptor is connected.</remarks>
    [Serializable]
    public class ConnectedEvent : NoteAcceptorBaseEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedEvent"/> class.
        /// </summary>
        public ConnectedEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedEvent"/> class with the note acceptor ID.
        /// </summary>
        /// <param name="noteAcceptorId">The associated note acceptor ID.</param>
        public ConnectedEvent(int noteAcceptorId)
            : base(noteAcceptorId)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Resources.NoteAcceptorText} {Resources.CommunicationFailureText} {Resources.ClearedText}";
        }
    }
}
