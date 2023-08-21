namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using AftTransferProvider;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Gaming.Contracts;

    /// <summary>
    ///     Handles the <see cref="GameOperatorMenuExitedEvent" /> event.
    /// </summary>
    public class GameOperatorMenuExitedConsumer : Consumes<GameOperatorMenuExitedEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly AftTransferProviderBase _aftOffTransferProvider;
        private readonly AftTransferProviderBase _aftOnTransferProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameOperatorMenuExitedConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        /// <param name="aftOffTransferProvider">An instance of <see cref="IAftOffTransferProvider"/></param>
        /// <param name="aftOnTransferProvider">An Instance of <see cref="IAftOnTransferProvider"/></param>
        public GameOperatorMenuExitedConsumer(
            ISasExceptionHandler exceptionHandler,
            IAftOffTransferProvider aftOffTransferProvider,
            IAftOnTransferProvider aftOnTransferProvider)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _aftOffTransferProvider = aftOffTransferProvider as AftTransferProviderBase;
            _aftOnTransferProvider = aftOnTransferProvider as AftTransferProviderBase;
        }

        /// <inheritdoc />
        public override void Consume(GameOperatorMenuExitedEvent theEvent)
        {
            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.OperatorMenuExited));
            _aftOffTransferProvider.AftState &= ~AftDisableConditions.GameOperatorMenuEntered;
            _aftOnTransferProvider.AftState &= ~AftDisableConditions.GameOperatorMenuEntered;

            _aftOffTransferProvider.OnStateChanged();
            _aftOnTransferProvider.OnStateChanged();
        }
    }
}
