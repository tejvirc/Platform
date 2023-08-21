namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using Properties;
    using System;

    /// <summary>Definition of the ID Reader DisconnectedEvent class.</summary>
    /// <remarks>This event is posted when ID Reader is disconnected from the USB.</remarks>
    [Serializable]
    public class DisconnectedEvent : NoteAcceptorBaseEvent
    {
        /// <summary>
        ///     Note Acceptor Communication Failure text 
        /// </summary>
        public static string NoteAcceptorDisconnectedText =
            $"{Resources.NoteAcceptorText} {Resources.CommunicationFailureText}";

        /// <summary>
        /// Initializes a new instance of the <see cref="DisconnectedEvent"/> class.
        /// </summary>
        public DisconnectedEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisconnectedEvent"/> class with the printer's ID.
        /// </summary>
        /// <param name="noteAcceptorId">The ID of the note acceptor associated with the event.</param>
        public DisconnectedEvent(int noteAcceptorId)
            : base(noteAcceptorId)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return NoteAcceptorDisconnectedText;
        }
    }
}

