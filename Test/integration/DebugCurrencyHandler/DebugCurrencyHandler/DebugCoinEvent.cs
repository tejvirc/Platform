namespace Vgt.Client12.Testing.Tools
{
    using System;
    using System.Globalization;
    using Aristocrat.Monaco.Kernel;

    /// <summary>
    ///     Post this event to cause an escrowed debug currency value based on the denom value.
    /// </summary>
    [Serializable]
    public class DebugCoinEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the DebugCoinEvent class, setting the 'Denomination' property to 0.
        /// </summary>
        public DebugCoinEvent()
        {
            Denomination = 0;
        }

        /// <summary>
        ///     Initializes a new instance of the DebugCoinEvent class, setting the 'Denomination' property to the one passed-in.
        /// </summary>
        /// <param name="denomination">The bill denomination that was escrowed.</param>
        public DebugCoinEvent(long denomination)
        {
            Denomination = denomination;
        }

        /// <summary>
        ///     Gets the Denomination of the bill that was escrowed.
        /// </summary>
        public long Denomination { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [Denomination={1}]",
                GetType().Name,
                Denomination);
        }
    }
}