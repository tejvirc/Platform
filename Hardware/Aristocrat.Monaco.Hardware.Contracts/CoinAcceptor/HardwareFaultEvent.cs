namespace Aristocrat.Monaco.Hardware.Contracts.CoinAcceptor
{
    using System;
    using Kernel;
    using static System.FormattableString;
    using static CoinEventsDescriptor;

    /// <summary> Definition of the <see cref="HardwareFaultEvent"/> class.</summary>
    /// <remarks>
    ///     This event is posted if the any fault occurred from coin acceptor.
    /// </remarks>
    [Serializable]
    public class HardwareFaultEvent : CoinAcceptorBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareFaultEvent" /> class.
        /// </summary>
        /// <param name="fault">The fault.</param>
        public HardwareFaultEvent(CoinFaultTypes fault)
        {
            Fault = fault;
        }

        /// <summary>Gets the Fault associated with the event</summary>
        /// <value>The current Fault.</value>
        public CoinFaultTypes Fault { get; }

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
