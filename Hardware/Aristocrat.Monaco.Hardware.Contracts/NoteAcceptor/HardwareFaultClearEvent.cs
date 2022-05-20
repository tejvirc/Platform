namespace Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor
{
    using Properties;
    using System;
    using static System.FormattableString;

    /// <summary> Definition of the HardwareFaultClearEvent class.</summary>
    /// <remarks>
    ///     This event is posted if the note acceptor status bits have the head error bit set.
    ///     This indicates the note acceptor head has failed and requires service.
    /// </remarks>
    [Serializable]
    public class HardwareFaultClearEvent : NoteAcceptorBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareFaultClearEvent"/> class.
        /// </summary>
        /// <param name="fault">The fault.</param>
        public HardwareFaultClearEvent(NoteAcceptorFaultTypes fault)
        {
            Fault = fault;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareFaultClearEvent"/> class.
        /// </summary>
        /// <param name="noteAcceptorId">The ID of the note acceptor associated with the event.</param>
        /// <param name="fault">The fault.</param>
        public HardwareFaultClearEvent(int noteAcceptorId, NoteAcceptorFaultTypes fault)
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
            if (NoteAcceptorEventsDescriptor.FaultTexts.TryGetValue(Fault, out var result) && !string.IsNullOrEmpty(result))
            {
                return result + $" {Resources.ClearedText}";
            }

            return Invariant($"{base.ToString()} [Fault={Fault}]");
        }
    }
}
