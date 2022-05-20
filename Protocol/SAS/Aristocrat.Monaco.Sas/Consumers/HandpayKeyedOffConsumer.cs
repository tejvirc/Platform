namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Accounting.Contracts.Handpay;
    using Contracts.Client;

    /// <inheritdoc />
    public class HandpayKeyedOffConsumer : Consumes<HandpayKeyedOffEvent>
    {
        private readonly ISasHandPayCommittedHandler _handPayCommittedHandler;

        /// <summary>
        ///     Create the HandpayKeyedOff event consumer
        /// </summary>
        /// <param name="handPayCommittedHandler">the sas hand pay committed handler</param>
        public HandpayKeyedOffConsumer(ISasHandPayCommittedHandler handPayCommittedHandler)
        {
            _handPayCommittedHandler = handPayCommittedHandler ?? throw new ArgumentNullException(nameof(handPayCommittedHandler));
        }

        /// <inheritdoc />
        public override void Consume(HandpayKeyedOffEvent theEvent)
        {
            _handPayCommittedHandler.HandPayReset(theEvent.Transaction);
        }
    }
}