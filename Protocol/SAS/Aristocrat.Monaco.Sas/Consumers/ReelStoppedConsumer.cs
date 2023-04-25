namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Exceptions;
    using Hardware.Contracts.Reel.Events;

    /// <summary>
    ///     Handles the <see cref="Hardware.Contracts.Reel.Events.ReelStoppedEvent" /> event for reels.
    /// </summary>
    public class ReelStoppedConsumer : Consumes<ReelStoppedEvent>
    {
        private readonly IRteStatusProvider _rteProvider;
        private readonly ISasExceptionHandler _exceptionHandler;

        private const byte SasClient1 = 0;
        private const byte SasClient2 = 1;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelStoppedConsumer" /> class.
        /// </summary>
        /// <param name="rteProvider">An instance of <see cref="IRteStatusProvider"/></param>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public ReelStoppedConsumer(IRteStatusProvider rteProvider, ISasExceptionHandler exceptionHandler)
        {
            _rteProvider = rteProvider ?? throw new ArgumentNullException(nameof(rteProvider));
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(ReelStoppedEvent theEvent)
        {
            if (_rteProvider.Client1RteEnabled)
            {
                ReportReelStopped(theEvent, SasClient1);
            }

            if (_rteProvider.Client2RteEnabled)
            {
                ReportReelStopped(theEvent, SasClient2);
            }
        }

        private void ReportReelStopped(ReelStoppedEvent theEvent, byte clientNumber)
        {
            var exception = new ReelNHasStoppedExceptionBuilder(theEvent.ReelId, theEvent.Step);

            // Exception code will not be filled if exception is malformed
            if (exception.ExceptionCode == GeneralExceptionCode.ReelNHasStopped)
            {
                _exceptionHandler.ReportException(exception, clientNumber);
            }
        }
    }
}