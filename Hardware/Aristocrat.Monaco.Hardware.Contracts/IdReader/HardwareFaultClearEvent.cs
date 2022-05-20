namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    using System;
    using static System.FormattableString;

    /// <summary>Definition of the ID reader HardwareErrorClearEvent class.</summary>
    [Serializable]
    public class HardwareFaultClearEvent : IdReaderBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareFaultClearEvent" /> class.
        /// </summary>
        /// <param name="fault">The fault.</param>
        public HardwareFaultClearEvent(IdReaderFaultTypes fault)
        {
            Fault = fault;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareFaultClearEvent" /> class.
        /// </summary>
        /// <param name="idReaderId">The associated ID reader's ID.</param>
        /// <param name="fault">The fault.</param>
        public HardwareFaultClearEvent(int idReaderId, IdReaderFaultTypes fault)
            : base(idReaderId)
        {
            Fault = fault;
        }

        /// <summary>Gets the fault.</summary>
        /// <value>The fault.</value>
        public IdReaderFaultTypes Fault { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [Fault={Fault}]");
        }
    }

}
