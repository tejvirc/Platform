namespace Aristocrat.Monaco.Sas.Consumers
{
    using AftTransferProvider;
    using Contracts.Client;
    using Kernel;

    /// <summary>
    ///     Handles the <see cref="SystemDisableRemovedEvent" /> event.
    /// </summary>
    public class SystemDisabledRemovedConsumer : Consumes<SystemDisableRemovedEvent>
    {
        private readonly AftTransferProviderBase _aftOffTransferProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemDisabledRemovedConsumer" /> class.
        /// </summary>
        public SystemDisabledRemovedConsumer(IAftOffTransferProvider aftOffTransferProvider) {
            _aftOffTransferProvider = aftOffTransferProvider as AftTransferProviderBase;
        }

        /// <inheritdoc />
        public override void Consume(SystemDisableRemovedEvent theEvent)
        {
            _aftOffTransferProvider?.OnSystemDisabled();
            _aftOffTransferProvider?.OnStateChanged();
        }
    }
}