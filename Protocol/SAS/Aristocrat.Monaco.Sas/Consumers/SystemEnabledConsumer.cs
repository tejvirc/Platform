namespace Aristocrat.Monaco.Sas.Consumers
{
    using AftTransferProvider;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;

    /// <summary>
    ///     Handles the <see cref="SystemEnabledEvent" /> event.
    /// </summary>
    public class SystemEnabledConsumer : Consumes<SystemEnabledEvent>
    {
        private readonly ISystemEventHandler _systemEventHandler;
        private readonly AftTransferProviderBase _aftOffTransferProvider;
        private readonly AftTransferProviderBase _aftOnTransferProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemEnabledConsumer" /> class.
        /// </summary>
        public SystemEnabledConsumer(
            ISystemEventHandler systemEventHandler,
            IAftOffTransferProvider aftOffTransferProvider,
            IAftOnTransferProvider aftOnTransferProvider)
        {
            _systemEventHandler = systemEventHandler;
            _aftOffTransferProvider = aftOffTransferProvider as AftTransferProviderBase;
            _aftOnTransferProvider = aftOnTransferProvider as AftTransferProviderBase;
        }

        /// <inheritdoc />
        public override void Consume(SystemEnabledEvent theEvent)
        {
            _systemEventHandler.OnSystemEnabled();
            if(_aftOffTransferProvider!=null)
            {
                _aftOffTransferProvider.AftState &= ~AftDisableConditions.SystemDisabled;
            }

            if (_aftOnTransferProvider != null)
            {
                _aftOnTransferProvider.AftState &= ~AftDisableConditions.SystemDisabled;
            }

            _aftOffTransferProvider?.OnStateChanged();
            _aftOnTransferProvider?.OnStateChanged();
        }
    }
}
