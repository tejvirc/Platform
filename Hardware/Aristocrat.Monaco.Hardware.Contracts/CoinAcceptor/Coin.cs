namespace Aristocrat.Monaco.Hardware.Contracts.CoinAcceptor
{
    using System;
    using static System.FormattableString;

    /// <inheritdoc />
    /// <summary>A coin.</summary>
    [Serializable]
    public class Coin : ICoin
    {
        /// <summary>Gets or sets the value.</summary>
        /// <value>The value.</value>
        public long Value { get; set; }

        /// <summary>Assembles and returns a string representation of the event and its data.</summary>
        /// <returns>A string representation of the event and its data</returns>
        public override string ToString()
        {
            return Invariant($"{GetType()} [Value={Value}.]");
        }
    }
}
