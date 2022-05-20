namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using System;
    using SharedDevice;
    using static System.FormattableString;

    /// <summary>Definition of the Note Acceptor DisabledEvent class.</summary>
    /// <remarks>
    ///     The DisabledEvent is posted by the NoteAcceptorService if the NoteAcceptor is disabled
    ///     or an attempt to enable a disabled note acceptor failed.
    /// </remarks>
    [Serializable]
    public class DisabledEvent : NoteAcceptorBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DisabledEvent"/> class.
        /// </summary>
        /// <param name="reasons">Reasons for the disabled event.</param>
        public DisabledEvent(DisabledReasons reasons)
        {
            Reasons = reasons;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DisabledEvent"/> class.
        /// </summary>
        /// <param name="noteAcceptorId">The ID of the note acceptor associated with the event.</param>
        /// <param name="reasons">Reasons for the disabled event.</param>
        public DisabledEvent(int noteAcceptorId, DisabledReasons reasons)
            : base(noteAcceptorId)
        {
            Reasons = reasons;
        }

        /// <summary>Gets the reasons for the disabled event.</summary>
        public DisabledReasons Reasons { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} {Reasons}");
        }
    }
}
