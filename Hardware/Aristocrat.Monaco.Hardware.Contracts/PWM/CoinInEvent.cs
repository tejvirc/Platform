namespace Aristocrat.Monaco.Hardware.Contracts.PWM
{
    using System;
    using Kernel;
    
    /// <summary>Definition of the Coin acceptor coin in event class.</summary>
    [Serializable]
    public class CoinInEvent : BaseEvent
    {
        /// <summary>
        ///     Coin In Event.
        /// </summary>
        /// <param name="coin"></param>
        public CoinInEvent(ICoin coin)
        {
            Coin = coin;
        }

        /// <summary>
        ///     Gets the information on the Coin that was accepted.
        /// </summary>
        public ICoin Coin { get; }
    }
}
