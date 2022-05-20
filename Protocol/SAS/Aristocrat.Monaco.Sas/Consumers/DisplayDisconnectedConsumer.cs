namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.Display;

    /// <summary>
    ///     Handles the <see cref="DisplayDisconnectedEvent"/> to report a general tilt message to SAS
    /// </summary>
    public class DisplayDisconnectedConsumer : Consumes<DisplayDisconnectedEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Creates a DisplayDisconnectedConsumer instance
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public DisplayDisconnectedConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(DisplayDisconnectedEvent theEvent)
        {
            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.GeneralTilt));
        }
    }
}