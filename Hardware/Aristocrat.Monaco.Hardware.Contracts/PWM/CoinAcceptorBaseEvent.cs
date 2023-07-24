namespace Aristocrat.Monaco.Hardware.Contracts.PWM
{
    using Properties;
    using System;
    using Kernel;
    using static System.FormattableString;

    /// <summary>
    /// 
    /// </summary>
    public class CoinAcceptorBaseEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CoinAcceptorBaseEvent"/> class.
        /// </summary>
        protected CoinAcceptorBaseEvent()
        {
            CoinAcceptorId = 1;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CoinAcceptorBaseEvent"/> class.
        /// </summary>
        /// <param name="coinAcceptorId">The ID of the printer associated with the event.</param>
        protected CoinAcceptorBaseEvent(int coinAcceptorId)
        {
            CoinAcceptorId = coinAcceptorId;
        }

        /// <summary>
        ///     Gets the ID of the printer associated with the event.
        /// </summary>
        public int CoinAcceptorId { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"CoinAcceptor {GetType().Name}");
        }
    }
}
