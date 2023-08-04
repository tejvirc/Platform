namespace Aristocrat.Monaco.Hardware.Contracts.CoinAcceptor
{
    using System;
    using Properties;
    using Kernel;
    using static System.FormattableString;

    /// <summary>Definition of the CoinAcceptorBaseEvent class.</summary>
    /// <remarks>All other coin acceptor events are derived from this event.</remarks>
    [Serializable]
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
        /// <param name="coinAcceptorId">The ID of the coin acceptor associated with the event.</param>
        protected CoinAcceptorBaseEvent(int coinAcceptorId)
        {
            CoinAcceptorId = coinAcceptorId;
        }

        /// <summary>
        ///     Gets the ID of the coin acceptor associated with the event.
        /// </summary>
        public int CoinAcceptorId { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{Resources.CoinAcceptorText} {GetType().Name}");
        }
    }
}
