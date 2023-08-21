namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Application.Contracts.Authentication;
    using Aristocrat.Sas.Client;

    /// <summary>
    ///     Handles the <see cref="LiveAuthenticationFailedEvent" /> event.
    /// </summary>
    public class LiveAuthenticationFailedConsumer : Consumes<LiveAuthenticationFailedEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LiveAuthenticationFailedConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public LiveAuthenticationFailedConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(LiveAuthenticationFailedEvent theEvent)
        {
            // In this system an authentication failure means both: checksum is bad and different.
            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.EePromErrorDifferentChecksum));
            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.EePromErrorBadChecksum));
        }
    }
}
