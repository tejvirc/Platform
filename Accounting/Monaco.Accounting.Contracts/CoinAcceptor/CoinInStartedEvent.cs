namespace Aristocrat.Monaco.Accounting.Contracts.CoinAcceptor
{
    using System;
    using Hardware.Contracts.PWM;
    using Kernel;

    /// <summary>
    ///     Event emitted when a coin has been inserted into the coin acceptor and the request
    ///     has been identified eligible for processing.
    /// </summary>
    [Serializable]
    public class CoinInStartedEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="coin"></param>
        public CoinInStartedEvent(ICoin coin)
        {
            Coin = coin;
        }


        /// <summary>Coin</summary>
        public ICoin Coin { get; }
    }
}
