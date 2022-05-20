namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.Reel;

    /// <summary>
    ///     Handles the <see cref="Hardware.Contracts.Reel.DisconnectedEvent" /> event for reels.
    /// </summary>
    public class ReelsDisconnectedConsumer : Consumes<DisconnectedEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelStoppedConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public ReelsDisconnectedConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(DisconnectedEvent _)
        {
            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.ReelMechanismDisconnected));
        }
    }
}