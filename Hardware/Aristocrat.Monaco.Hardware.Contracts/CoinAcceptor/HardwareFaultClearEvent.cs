namespace Aristocrat.Monaco.Hardware.Contracts.CoinAcceptor
{
    using System;
    using Properties;
    using static CoinEventsDescriptor;
    using static System.FormattableString;

    /// <summary> Definition of the <see cref="HardwareFaultClearEvent"/> class.</summary>
    /// <remarks>
    ///     This event is posted if the coin acceptor faults are cleared.
    /// </remarks>
    [Serializable]
    public class HardwareFaultClearEvent : CoinAcceptorBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareFaultClearEvent" /> class.
        /// </summary>
        /// <param name="fault">The fault.</param>
        public HardwareFaultClearEvent(CoinFaultTypes fault)
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
                var result = FaultTexts[Fault].EventText;
                return result != string.Empty ? result + $" {Resources.ClearedText}" : result;
            }

            return Invariant($"{base.ToString()} [Fault={Fault}]");
        }
    }
}
