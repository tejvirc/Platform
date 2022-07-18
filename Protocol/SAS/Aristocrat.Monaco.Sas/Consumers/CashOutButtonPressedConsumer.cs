namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Kernel;

    /// <summary>
    ///     Handles the <see cref="CashOutButtonPressedEvent" /> event.
    /// </summary>
    public class CashOutButtonPressedConsumer : Consumes<CashOutButtonPressedEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly IPropertiesManager _propertiesManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CashOutButtonPressedConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        /// <param name="propertiesManager">An instance of <see cref="IPropertiesManager"/></param>
        public CashOutButtonPressedConsumer(ISasExceptionHandler exceptionHandler, IPropertiesManager propertiesManager)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        /// <inheritdoc />
        public override void Consume(CashOutButtonPressedEvent theEvent)
        {
            var sasFeatures = _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures());
            if (sasFeatures.FundTransferType == FundTransferType.Aft && sasFeatures.TransferOutAllowed)
            {
                // Per section 8.8 Cash Out Button Pressed, exception 66 will be sent only if AFT transfers to host are enabled
                _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.CashOutButtonPressed));
            }
        }
    }
}
