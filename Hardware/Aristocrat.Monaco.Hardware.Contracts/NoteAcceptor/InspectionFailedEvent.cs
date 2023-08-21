namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using Properties;
    using System;

    /// <summary>Definition of the Note Acceptor InspectionFailedEvent class.</summary>
    /// <remarks>
    ///     This event is posted by the Note Acceptor Service when the Note Acceptor fails to initialize communication
    ///     or fails to get device information before a timeout occurred.
    /// </remarks>
    [Serializable]
    public class InspectionFailedEvent : NoteAcceptorBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectionFailedEvent"/> class.
        /// </summary>
        public InspectionFailedEvent()
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectionFailedEvent"/> class.
        /// </summary>
        /// <param name="noteAcceptorId">The ID of the note acceptor associated with the event.</param>
        public InspectionFailedEvent(int noteAcceptorId)
            : base(noteAcceptorId)
        { }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Resources.NoteAcceptorText} {Resources.InspectionFailedText}";
        }
    }
}
