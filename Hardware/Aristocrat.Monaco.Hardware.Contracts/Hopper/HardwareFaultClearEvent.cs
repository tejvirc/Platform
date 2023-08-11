namespace Aristocrat.Monaco.Hardware.Contracts.Hopper
{
    using System;
    using Properties;
    using static System.FormattableString;
    using static HopperEventsDescriptor;

    /// <summary> Definition of the <see cref="HardwareFaultClearEvent"/> class.</summary>
    /// <remarks>
    ///     This event is posted if the hopper faults are cleared.
    /// </remarks>
    [Serializable]
    public class HardwareFaultClearEvent : HopperBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareFaultClearEvent" /> class.
        /// </summary>
        /// <param name="fault">The fault.</param>
        public HardwareFaultClearEvent(HopperFaultTypes fault)
        {
            Fault = fault;
        }

        /// <summary>Gets the Fault associated with the event</summary>
        /// <value>The current Fault.</value>
        public HopperFaultTypes Fault { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            if (FaultTexts.ContainsKey(Fault))
            {
                var result = FaultTexts[Fault].EventText;
                return result != string.Empty ? result + $" {Resources.ClearedText}" : result;
            }

            return Invariant($"{base.ToString()} [Fault={Fault}]");
        }
    }
}
