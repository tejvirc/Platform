namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using System;
    using static System.FormattableString;

    /// <summary> Definition of the HardwareFaultEvent class.</summary>
    /// <remarks>
    ///     This event is posted if the note acceptor status bits have the head error bit set.
    ///     This indicates the note acceptor head has failed and requires service.
    /// </remarks>
    [Serializable]
    public class HardwareFaultEvent : NoteAcceptorBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareFaultEvent" /> class.
        /// </summary>
        /// <param name="fault">The fault.</param>
        public HardwareFaultEvent(NoteAcceptorFaultTypes fault)
        {
            Fault = fault;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareFaultEvent" /> class.
        /// </summary>
        /// <param name="noteAcceptorId">The ID of the note acceptor associated with the event.</param>
        /// <param name="fault">The fault.</param>
        public HardwareFaultEvent(int noteAcceptorId, NoteAcceptorFaultTypes fault)
            : base(noteAcceptorId)
        {
            Fault = fault;
        }

        /// <summary>Gets the fault.</summary>
        /// <value>The fault.</value>
        public NoteAcceptorFaultTypes Fault { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            if (NoteAcceptorEventsDescriptor.FaultTexts.ContainsKey(Fault))
            {
                return NoteAcceptorEventsDescriptor.FaultTexts[Fault];
            }

            return Invariant($"{base.ToString()} [Fault={Fault}]");
        }
    }
}
