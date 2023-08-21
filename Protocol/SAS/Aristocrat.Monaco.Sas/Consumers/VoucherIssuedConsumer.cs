namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Accounting.Contracts;
    using Contracts.Client;
    using VoucherValidation;

    /// <summary>
    ///     Handles the <see cref="VoucherIssuedEvent" /> event.
    /// </summary>
    public class VoucherIssuedConsumer : Consumes<VoucherIssuedEvent>
    {
        private readonly ISasHost _sasHost;
        private readonly SasValidationHandlerFactory _validationHandlerFactory;
        private readonly IEnhancedValidationProvider _enhancedValidationProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherIssuedConsumer" /> class.
        /// </summary>
        /// <param name="sasHost">The sas host</param>
        /// <param name="validationHandlerFactory">The sas validation handler factory</param>
        /// <param name="enhancedValidationProvider">the enhanced validation provider</param>
        public VoucherIssuedConsumer(ISasHost sasHost, SasValidationHandlerFactory validationHandlerFactory, IEnhancedValidationProvider enhancedValidationProvider)
        {
            _sasHost = sasHost ?? throw new ArgumentNullException(nameof(sasHost));
            _validationHandlerFactory = validationHandlerFactory ?? throw new ArgumentNullException(nameof(validationHandlerFactory));
            _enhancedValidationProvider = enhancedValidationProvider ?? throw new ArgumentNullException(nameof(enhancedValidationProvider));
        }

        /// <inheritdoc />
        public override void Consume(VoucherIssuedEvent theEvent)
        {
            _validationHandlerFactory.GetValidationHandler()?.HandleTicketOutCompleted(theEvent.Transaction);
            _enhancedValidationProvider.HandleTicketOutCompleted(theEvent.Transaction);
            _sasHost.TicketPrinted();
        }
    }
}