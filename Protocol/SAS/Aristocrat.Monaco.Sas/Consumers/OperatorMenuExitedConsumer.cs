namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Accounting.Contracts;
    using AftTransferProvider;
    using Application.Contracts.OperatorMenu;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Contracts.SASProperties;
    using VoucherValidation;

    /// <summary>
    ///     Handles the <see cref="OperatorMenuExitedEvent" /> event.
    /// </summary>
    public class OperatorMenuExitedConsumer : Consumes<OperatorMenuExitedEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly AftTransferProviderBase _aftOffTransferProvider;
        private readonly AftTransferProviderBase _aftOnTransferProvider;
        private readonly SasVoucherValidation _voucherValidation;
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorMenuExitedConsumer" /> class.
        /// </summary>
        public OperatorMenuExitedConsumer(
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
        public override void Consume(OperatorMenuExitedEvent theEvent)
        {
            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.OperatorMenuExited));

            if (_aftOffTransferProvider != null)
            {
                _aftOffTransferProvider.AftState &= ~AftDisableConditions.OperatorMenuEntered;
                // TODO: check if it is true -> Current set up is that exiting the System Operator Menu skips directly back to the game.
                _aftOffTransferProvider.AftState &= ~AftDisableConditions.GameOperatorMenuEntered;
            }

            if (_aftOnTransferProvider != null)
            {
                _aftOnTransferProvider.AftState &= ~AftDisableConditions.OperatorMenuEntered;
                _aftOnTransferProvider.AftState &= ~AftDisableConditions.GameOperatorMenuEntered;
            }

            _aftOffTransferProvider?.OnStateChanged();
            _aftOnTransferProvider?.OnStateChanged();

            _voucherValidation.InOperatorMenu = false;
        }
    }
}
