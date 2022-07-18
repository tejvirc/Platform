namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Accounting.Contracts.Handpay;
    using AftTransferProvider;
    using Contracts.Client;
    using Contracts.SASProperties;
    using VoucherValidation;

    /// <summary>
    ///     Handles the <see cref="HandpayCompletedEvent" /> event.
    /// </summary>
    public class HandPayCompletedConsumer : Consumes<HandpayCompletedEvent>
    {
        private readonly ISasHost _sasHost;
        private readonly IEnhancedValidationProvider _enhancedValidation;
        private readonly AftTransferProviderBase _aftOffTransferProvider;
        private readonly AftTransferProviderBase _aftOnTransferProvider;
        private readonly SasValidationHandlerFactory _validationHandlerFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HandPayCompletedConsumer" /> class.
        /// </summary>
        /// <param name="sasHost">The sas host</param>
        /// <param name="aftOffTransferProvider">the aft off transfer provider</param>
        /// <param name="aftOnTransferProvider">the aft on provider</param>
        /// <param name="enhancedValidation">THe enhanced validation provider</param>
        /// <param name="validationHandlerFactory">The sas validation handler factory</param>
        public HandPayCompletedConsumer(
            ISasHost sasHost,
            IAftOffTransferProvider aftOffTransferProvider,
            IAftOnTransferProvider aftOnTransferProvider,
            IEnhancedValidationProvider enhancedValidation,
            SasValidationHandlerFactory validationHandlerFactory)
        {
            _sasHost = sasHost;
            _enhancedValidation = enhancedValidation ?? throw new ArgumentNullException(nameof(enhancedValidation));
            _aftOffTransferProvider = aftOffTransferProvider as AftTransferProviderBase;
            _aftOnTransferProvider = aftOnTransferProvider as AftTransferProviderBase;
            _validationHandlerFactory = validationHandlerFactory ?? throw new ArgumentNullException(nameof(validationHandlerFactory));
        }

        /// <inheritdoc />
        public override void Consume(HandpayCompletedEvent theEvent)
        {
            if (_aftOffTransferProvider != null)
            {
                _aftOffTransferProvider.AftState &= ~AftDisableConditions.CanceledCreditsPending;
            }

            if (_aftOnTransferProvider != null)
            {
                _aftOnTransferProvider.AftState &= ~AftDisableConditions.CanceledCreditsPending;
            }

            _aftOffTransferProvider?.OnStateChanged();
            _aftOnTransferProvider?.OnStateChanged();

            _enhancedValidation.HandPayReset(theEvent.Transaction);
            _sasHost.HandPayValidated();
            _validationHandlerFactory.GetValidationHandler()?.HandleHandpayCompleted(theEvent.Transaction);
        }
    }
}
