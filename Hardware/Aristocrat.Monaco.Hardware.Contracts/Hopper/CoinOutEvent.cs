namespace Aristocrat.Monaco.Hardware.Contracts.Hopper
{
    using System;
    using CoinAcceptor;
    using Kernel;


    /// <summary>Definition of the Coin out event.</summary>
    [Serializable]
    public class CoinOutEvent : BaseEvent
    {
        /// <summary>
        ///     Coin Out Event.
        /// </summary>
        /// <param name="coin"></param>
        public CoinOutEvent(ICoin coin)
        {
            Coin = coin;
        }

        /// <summary>
        ///     Gets the information on the Coin that was accepted.
        /// </summary>
        public ICoin Coin { get; }
    }
}
