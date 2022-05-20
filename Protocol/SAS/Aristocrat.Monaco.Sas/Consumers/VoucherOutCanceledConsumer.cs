namespace Aristocrat.Monaco.Sas.Consumers
{
    using Contracts.Client;
    using Accounting.Contracts;

    /// <summary>
    ///     Handles the <see cref="VoucherOutCanceledEvent" /> event.
    /// </summary>
    public class VoucherOutCanceledConsumer : Consumes<VoucherOutCanceledEvent>
    {
        private readonly ISasHost _sasHost;

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherOutCanceledConsumer" /> class.
        /// </summary>
        public VoucherOutCanceledConsumer(ISasHost sasHost)
        {
            _sasHost = sasHost;
        }

        /// <inheritdoc />
        public override void Consume(VoucherOutCanceledEvent theEvent)
        {
            _sasHost.VoucherOutCanceled();
        }
    }
}
