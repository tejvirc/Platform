namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using Properties;
    using System;

    /// <summary>Definition of the Note Acceptor InspectedEvent class.</summary>
    /// <remarks>
    ///     This event is posted by the Note Acceptor Service when the Note Acceptor is
    ///     connected and has responded with its information.
    /// </remarks>
    [Serializable]
    public class InspectedEvent : NoteAcceptorBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectedEvent"/> class.
        /// </summary>
        public InspectedEvent()
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectedEvent"/> class.
        /// </summary>
        /// <param name="noteAcceptorId">The ID of the note acceptor associated with the event.</param>
        public InspectedEvent(int noteAcceptorId)
            : base(noteAcceptorId)
        { }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Resources.NoteAcceptorText} {Resources.InspectionFailedText} {Resources.ClearedText}";
        }
    }
}
