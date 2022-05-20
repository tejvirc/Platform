namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Gaming.Contracts;

    /// <summary>
    ///     Handles the <see cref="PlayerRequestedDrawEvent" />.
    ///     This event is sent from PokerHandProvider when the game sends draw card information
    /// </summary>
    public class PlayerRequestedDrawConsumer : Consumes<PlayerRequestedDrawEvent>
    {
        private readonly IRteStatusProvider _rteProvider;
        private readonly ISasExceptionHandler _exceptionHandler;
        private const byte SasClient1 = 0;
        private const byte SasClient2 = 1;

        /// <summary>
        ///     Handles the PlayerRequestedDrawEvent
        /// </summary>
        /// <param name="rteProvider">The RealTimeEventReportingEnabled provider</param>
        /// <param name="exceptionHandler">The Sas exception handler</param>
        public PlayerRequestedDrawConsumer(
            IRteStatusProvider rteProvider,
            ISasExceptionHandler exceptionHandler)
        {
            _rteProvider = rteProvider ?? throw new ArgumentNullException(nameof(rteProvider));
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(PlayerRequestedDrawEvent theEvent)
        {
            if (_rteProvider.Client1RteEnabled)
            {
                _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.PlayerHasRequestedDrawCards), SasClient1);
            }

            if (_rteProvider.Client2RteEnabled)
            {
                _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.PlayerHasRequestedDrawCards), SasClient2);
            }
        }
    }
}