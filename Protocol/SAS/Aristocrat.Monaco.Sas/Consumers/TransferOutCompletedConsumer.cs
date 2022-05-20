namespace Aristocrat.Monaco.Sas.Consumers
{
    using Accounting.Contracts;
    using AftTransferProvider;
    using Contracts.Client;

    /// <summary>
    ///     Handles the <see cref="TransferOutCompletedEvent" /> event.
    /// </summary>
    public class TransferOutCompletedConsumer : Consumes<TransferOutCompletedEvent>
    {
        private readonly AftTransferProviderBase _aftOffTransferProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransferOutCompletedConsumer" /> class.
        /// </summary>
        public TransferOutCompletedConsumer(IAftOffTransferProvider aftOffTransferProvider)
        {
            _aftOffTransferProvider = aftOffTransferProvider as AftTransferProviderBase;
        }

        /// <inheritdoc />
        public override void Consume(TransferOutCompletedEvent theEvent)
        {
            if (_aftOffTransferProvider.TransferOutPending)
            {
                _aftOffTransferProvider.ResetCashOutRequestState();

                _aftOffTransferProvider.TransactionPending = false;
                _aftOffTransferProvider.TransferOutPending = false;
            }
        }
    }
}
