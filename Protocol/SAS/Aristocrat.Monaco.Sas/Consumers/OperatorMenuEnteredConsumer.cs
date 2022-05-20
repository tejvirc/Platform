namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Accounting.Contracts;
    using AftTransferProvider;
    using Aristocrat.Monaco.Application.Contracts.OperatorMenu;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Contracts.SASProperties;
    using VoucherValidation;

    /// <summary>
    ///     Handles the <see cref="OperatorMenuEnteredEvent" /> event.
    /// </summary>
    public class OperatorMenuEnteredConsumer : Consumes<OperatorMenuEnteredEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly AftTransferProviderBase _aftOffTransferProvider;
        private readonly AftTransferProviderBase _aftOnTransferProvider;
        private readonly SasVoucherValidation _voucherValidation;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorMenuEnteredConsumer" /> class.
        /// </summary>
        public OperatorMenuEnteredConsumer(
            ISasExceptionHandler exceptionHandler,
            IAftOffTransferProvider aftOffTransferProvider,
            IAftOnTransferProvider aftOnTransferProvider,
            IVoucherValidator voucherValidation)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _aftOffTransferProvider = aftOffTransferProvider as AftTransferProviderBase;
            _aftOnTransferProvider = aftOnTransferProvider as AftTransferProviderBase;
            _voucherValidation = voucherValidation as SasVoucherValidation;
        }

        /// <inheritdoc />
        public override void Consume(OperatorMenuEnteredEvent theEvent)
        {
            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.OperatorMenuEntered));
            _aftOffTransferProvider.AftState |= AftDisableConditions.OperatorMenuEntered;
            _aftOnTransferProvider.AftState |= AftDisableConditions.OperatorMenuEntered;
            _aftOffTransferProvider.OnStateChanged();
            _aftOnTransferProvider.OnStateChanged();
            _voucherValidation.InOperatorMenu = true;
        }
    }
}
