namespace Aristocrat.Monaco.Accounting.Contracts.CoinAcceptor
{
    using System;
    using System.Globalization;
    using Hardware.Contracts.CoinAcceptor;
    using Kernel;

    /// <summary>
    ///     Event emitted when a coin-in transaction has been completed.
    /// </summary>
    [Serializable]
    public class CoinInCompletedEvent : BaseEvent
    {

        /// <summary>
        ///     Initializes a new instance of the <see cref="CoinInCompletedEvent" /> class.
        /// </summary>
        public CoinInCompletedEvent(ICoin coin, CoinTransaction transaction)
        {
            CoinTransaction = transaction;
            Coin = coin;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CoinInCompletedEvent" /> class.
        /// </summary>
        private CoinInCompletedEvent()
        {
        }

        /// <summary>
        ///     Gets the information on the Coin that was accepted.
        /// </summary>
        public ICoin Coin { get; }

        /// <summary>
        ///     Gets the transaction that was accepted.
        /// </summary>
        public CoinTransaction CoinTransaction { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                $"{GetType().Name} [Timestamp={Timestamp}]");
        }
    }
}
