namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.ButtonDeck;

    /// <summary>
    ///     Handles the <see cref="ButtonDeckDisconnectedEvent" /> to report a general tilt message to SAS
    /// </summary>
    public class ButtonDeckDisconnectedConsumer : Consumes<ButtonDeckDisconnectedEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Creates a ButtonDeckDisconnectedConsumer instance
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler" /></param>
        public ButtonDeckDisconnectedConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(ButtonDeckDisconnectedEvent theEvent)
        {
            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.GeneralTilt));
        }
    }
}