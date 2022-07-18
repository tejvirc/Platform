namespace Aristocrat.Monaco.Sas.Consumers
{
    using Accounting.Contracts.Handpay;
    using AftTransferProvider;
    using Contracts.Client;
    using Contracts.SASProperties;

    /// <summary>
    ///     Handles the <see cref="HandpayStartedEvent" /> event.
    /// </summary>
    public class HandPayStartedConsumer : Consumes<HandpayStartedEvent>
    {
        private readonly AftTransferProviderBase _aftOffTransferProvider;
        private readonly AftTransferProviderBase _aftOnTransferProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HandPayStartedConsumer" /> class.
        /// </summary>
        /// <param name="aftOffTransferProvider">the aft off transfer provider</param>
        /// <param name="aftOnTransferProvider">the aft on transfer provider</param>
        public HandPayStartedConsumer(
            IAftOffTransferProvider aftOffTransferProvider,
            IAftOnTransferProvider aftOnTransferProvider)
        {
            _aftOffTransferProvider = aftOffTransferProvider as AftTransferProviderBase;
            _aftOnTransferProvider = aftOnTransferProvider as AftTransferProviderBase;
        }

        /// <inheritdoc />
        public override void Consume(HandpayStartedEvent theEvent)
        {
            if (_aftOffTransferProvider != null)
            {
                _aftOffTransferProvider.AftState |= AftDisableConditions.CanceledCreditsPending;
            }

            if (_aftOnTransferProvider != null)
            {
                _aftOnTransferProvider.AftState |= AftDisableConditions.CanceledCreditsPending;
            }

            _aftOffTransferProvider?.OnStateChanged();
            _aftOnTransferProvider?.OnStateChanged();
        }
    }
}
