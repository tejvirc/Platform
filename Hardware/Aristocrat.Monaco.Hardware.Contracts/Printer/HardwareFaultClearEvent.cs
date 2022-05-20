namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using Properties;
    using System;
    using static System.FormattableString;

    /// <summary> Definition of the HardwareFaultClearEvent class.</summary>
    /// <remarks>
    ///     This event is posted if the printer status bits have the head error bit set.
    ///     This indicates the printer head has failed and requires service.
    /// </remarks>
    [Serializable]
    public class HardwareFaultClearEvent : PrinterBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareFaultClearEvent"/> class.
        /// </summary>
        /// <param name="fault">The fault.</param>
        public HardwareFaultClearEvent(PrinterFaultTypes fault)
        {
            Fault = fault;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareFaultClearEvent"/> class.
        /// </summary>
        /// <param name="printerId">The associated printer's ID.</param>
        /// <param name="fault">The fault.</param>
        public HardwareFaultClearEvent(int printerId, PrinterFaultTypes fault)
            : base(printerId)
        {
            Fault = fault;
        }

        /// <summary>Gets the fault.</summary>
        /// <value>The fault.</value>
        public PrinterFaultTypes Fault { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            if (PrinterEventsDescriptor.FaultTexts.ContainsKey(Fault))
            {
                var result = PrinterEventsDescriptor.FaultTexts[Fault];
                return result != string.Empty ? result + $" {Resources.ClearedText}" : result;
            }

            return Invariant($"{base.ToString()} [Fault={Fault}]");
        }
    }
}
