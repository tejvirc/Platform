using static System.FormattableString;

namespace Aristocrat.Monaco.Hardware.Contracts.IO
{
    using Hopper;
    using Kernel;

    /// <summary>
    ///     Used for Coin Diverter emulation
    /// </summary>
    public class FakeCoinDivertorEvent : BaseEvent
    {
        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()}");
        }
    }
}
