namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using AftTransferProvider;
    using Contracts.Client;
    using Kernel;

    /// <summary>
    ///     Handles the <see cref="SystemDisableAddedEvent" /> event.
    /// </summary>
    public class SystemDisableAddedConsumer : Consumes<SystemDisableAddedEvent>
    {
        private readonly AftTransferProviderBase _aftOffTransferProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemDisableAddedConsumer" /> class.
        /// </summary>
        public SystemDisableAddedConsumer(IAftOffTransferProvider aftOffTransferProvider)
        {
            _aftOffTransferProvider = aftOffTransferProvider as AftTransferProviderBase ?? throw new ArgumentNullException(nameof(aftOffTransferProvider));
        }

        /// <inheritdoc />
        public override void Consume(SystemDisableAddedEvent theEvent)
        {
            _aftOffTransferProvider.OnSystemDisabled();
            _aftOffTransferProvider.OnStateChanged();
        }
    }
}

