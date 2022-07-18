namespace Aristocrat.Monaco.Sas.Consumers
{
    using Accounting.Contracts;
    using AftTransferProvider;
    using Contracts.Client;

    /// <summary>
    ///     Handles the <see cref="TransactionCompletedEvent" /> event.
    /// </summary>
    public class TransactionCompletedConsumer : Consumes<TransactionCompletedEvent>
    {
        private readonly AftTransferProviderBase _aftOffTransferProvider;
        private readonly IHardCashOutLock _hardCashOutLock;
        private readonly ITransferOutHandler _transferOutHandler;
        private readonly IBank _bank;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TransactionCompletedConsumer" /> class.
        /// </summary>
        public TransactionCompletedConsumer(IAftOffTransferProvider aftOffTransferProvider,
                                            IHardCashOutLock hardCashOutLock,
                                            ITransferOutHandler transferOutHandler,
                                            IBank bank)
        {
            _aftOffTransferProvider = aftOffTransferProvider as AftTransferProviderBase;
            _hardCashOutLock = hardCashOutLock;
            _transferOutHandler = transferOutHandler;
            _bank = bank;
        }

        /// <inheritdoc />
        public override void Consume(TransactionCompletedEvent theEvent)
        {
            if (_aftOffTransferProvider != null && _aftOffTransferProvider.TransactionPending)
            {
                if (MoneyToCashOff)
                {
                    _aftOffTransferProvider.TransferOutPending = true;

                    // setting _cashOutFromGamingMachineRequest to true will cause Aft to fail when
                    // it is initiated by the WAT off handler and go to a voucher.
                    // It will also allow a host started Aft off to succeed, cancelling the lock if there is one.
                    _aftOffTransferProvider.CashOutFromGamingMachineRequest = true;
                    if (!_aftOffTransferProvider.HardCashOutMode || (_aftOffTransferProvider.HardCashOutMode && !_hardCashOutLock.LockupAndCashOut()))
                    {
                        _transferOutHandler.TransferOut();
                    }
                }

                _aftOffTransferProvider.TransactionPending = false;
            }
        }

        private bool MoneyToCashOff => _bank.QueryBalance() > 0;
    }
}
