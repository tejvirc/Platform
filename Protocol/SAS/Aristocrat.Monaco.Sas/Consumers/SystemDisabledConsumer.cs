namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using AftTransferProvider;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using VoucherValidation;

    /// <summary>
    ///     Handles the <see cref="SystemDisabledEvent" /> event.
    /// </summary>
    public class SystemDisabledConsumer : Consumes<SystemDisabledEvent>
    {
        private readonly ISystemEventHandler _systemEventHandler;
        private readonly IHostValidationProvider _hostValidationProvider;
        private readonly AftTransferProviderBase _aftOffTransferProvider;
        private readonly AftTransferProviderBase _aftOnTransferProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemDisabledConsumer" /> class.
        /// </summary>
        public SystemDisabledConsumer(
            ISystemEventHandler systemEventHandler,
            IAftOffTransferProvider aftOffTransferProvider,
            IAftOnTransferProvider aftOnTransferProvider,
            IHostValidationProvider hostValidationProvider)
        {
            _systemEventHandler = systemEventHandler ?? throw new ArgumentNullException(nameof(systemEventHandler));
            _hostValidationProvider = hostValidationProvider ?? throw new ArgumentNullException(nameof(hostValidationProvider));
            _aftOffTransferProvider = aftOffTransferProvider as AftTransferProviderBase ?? throw new ArgumentNullException(nameof(aftOffTransferProvider));
            _aftOnTransferProvider = aftOnTransferProvider as AftTransferProviderBase ?? throw new ArgumentNullException(nameof(aftOnTransferProvider));
        }

        /// <inheritdoc />
        public override void Consume(SystemDisabledEvent theEvent)
        {
            _systemEventHandler.OnSystemDisabled();
            _aftOnTransferProvider.AftState |= AftDisableConditions.SystemDisabled;
            _aftOffTransferProvider.OnSystemDisabled();
            _aftOffTransferProvider.OnStateChanged();
            _aftOnTransferProvider.OnStateChanged();
            _hostValidationProvider.OnSystemDisabled();
        }
    }
}
