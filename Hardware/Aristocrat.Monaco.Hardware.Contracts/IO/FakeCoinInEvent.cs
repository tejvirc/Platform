using static System.FormattableString;

namespace Aristocrat.Monaco.Hardware.Contracts.IO
{
    using Kernel;

    /// <summary>
    ///     Used for Coin In emulation.
    /// </summary>
    public class FakeCoinInEvent : BaseEvent
    {
        /// <summary>
        ///     Gets the Denomination.
        /// </summary>
        public long Denomination { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{GetType().Name} [Denomination={Denomination}]");
        }
    }
}
