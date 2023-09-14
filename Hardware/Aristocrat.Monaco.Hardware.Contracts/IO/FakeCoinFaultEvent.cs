using static System.FormattableString;

namespace Aristocrat.Monaco.Hardware.Contracts.IO
{
    using CoinAcceptor;
    using Kernel;

    /// <summary>
    ///     Used for Coin Acceptor emulation
    /// </summary>
    public class FakeCoinFaultEvent : BaseEvent
    {
        /// <summary>Gets or sets the identifier of the Coin In Fault.</summary>
        /// <value>The identifier of the transaction.</value>
        public CoinFaultTypes FaultType { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Invariant($"{GetType()} [CoinFaultTypes={FaultType}]");
        }
    }
}
