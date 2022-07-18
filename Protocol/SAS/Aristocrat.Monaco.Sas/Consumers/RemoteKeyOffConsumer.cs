namespace Aristocrat.Monaco.Sas.Consumers
{
    using Accounting.Contracts.Handpay;
    using AftTransferProvider;
    using Contracts.Client;
    using Kernel;

    /// <summary>
    ///     Handles the <see cref="RemoteKeyOffEvent" /> event.
    /// </summary>
    public class RemoteKeyOffConsumer : Consumes<RemoteKeyOffEvent>
    {
        private readonly IHardCashOutLock _hardCashOutLock;
        private readonly AftTransferProviderBase _aftOffTransferProvider;
        private readonly ISystemDisableManager _systemDisableManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RemoteKeyOffConsumer" /> class.
        /// </summary>
        public RemoteKeyOffConsumer(IHardCashOutLock hardCashOutLock,
                                    IAftOffTransferProvider aftOffTransferProvider,
                                    ISystemDisableManager systemDisableManager)
        {
            _hardCashOutLock = hardCashOutLock;
            _aftOffTransferProvider = aftOffTransferProvider as AftTransferProviderBase;
            _systemDisableManager = systemDisableManager;
        }

        /// <inheritdoc />
        public override void Consume(RemoteKeyOffEvent theEvent)
        {
            if (ImmediateSystemDisable)
            {
                return;
            }

            if (_hardCashOutLock.WaitingForKeyOff)
            {
                _hardCashOutLock.OnKeyedOff();
            }

            if (_aftOffTransferProvider != null && _aftOffTransferProvider.WaitingForKeyOff)
            {
                _aftOffTransferProvider.OnKeyedOff();
            }
        }

        private bool ImmediateSystemDisable => _systemDisableManager.IsDisabled && _systemDisableManager.DisableImmediately;
    }
}