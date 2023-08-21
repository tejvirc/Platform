namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.Audio;

    /// <summary>
    ///     Handles the <see cref="DisabledEvent" /> to report a general tilt message to SAS
    /// </summary>
    public class AudioDisconnectedConsumer : Consumes<DisabledEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Creates a AudioDisconnectedConsumer instance
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler" /></param>
        public AudioDisconnectedConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(DisabledEvent theEvent)
        {
            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.GeneralTilt));
        }
    }
}