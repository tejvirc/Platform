namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using System;
    using ProtoBuf;
    using SharedDevice;
    using static System.FormattableString;

    /// <summary>Definition of the Note Acceptor EnabledEvent class.</summary>
    /// <remarks>This event is posted when Note Acceptor becomes Enabled.</remarks>
    [ProtoContract]
    public class EnabledEvent : NoteAcceptorBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EnabledEvent" /> class.
        /// </summary>
        /// <param name="reasons">Reasons for the enabled event.</param>
        public EnabledEvent(EnabledReasons reasons)
        {
            Reasons = reasons;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EnabledEvent" /> class.Initializes a new instance of the EnabledEvent
        ///     class with the printer's ID and enabled reasons.
        /// </summary>
        /// <param name="noteAcceptorId">The ID of the note acceptor associated with the event.</param>
        /// <param name="reasons">Reasons for the enabled event.</param>
        public EnabledEvent(int noteAcceptorId, EnabledReasons reasons)
            : base(noteAcceptorId)
        {
            Reasons = reasons;
        }

        /// <summary>
        /// Parameterless constructor used while deseriliazing
        /// </summary>
        public EnabledEvent()
        { }

        /// <summary>Gets the reasons for the enabled event.</summary>
        [ProtoMember(1)]
        public EnabledReasons Reasons { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} {Reasons}");
        }
    }
}