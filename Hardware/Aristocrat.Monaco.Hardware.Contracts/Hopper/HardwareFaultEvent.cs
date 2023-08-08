namespace Aristocrat.Monaco.Hardware.Contracts.Hopper
{
    using System;
    using Kernel;
    using static System.FormattableString;
    using static HopperEventsDescriptor;

    /// <summary> Definition of the <see cref="HardwareFaultEvent"/> class.</summary>
    /// <remarks>
    ///     This event is posted if any fault occurred from hopper.
    /// </remarks>
    [Serializable]
    public class HardwareFaultEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareFaultEvent" /> class.
        /// </summary>
        /// <param name="fault">The fault.</param>
        public HardwareFaultEvent(HopperFaultTypes fault)
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
                return FaultTexts[Fault].EventText;
            }

            return Invariant($"{base.ToString()} [Fault={Fault}]");
        }
    }
}
