namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.Touch;

    /// <summary>
    ///     Handles the <see cref="TouchDisplayDisconnectedEvent"/> to report a general tilt message to SAS
    /// </summary>
    public class TouchDisplayDisconnectedConsumer : Consumes<TouchDisplayDisconnectedEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Creates a TouchDisplayDisconnectedConsumer instance
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public TouchDisplayDisconnectedConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(TouchDisplayDisconnectedEvent theEvent)
        {
            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.GeneralTilt));
        }
    }
}