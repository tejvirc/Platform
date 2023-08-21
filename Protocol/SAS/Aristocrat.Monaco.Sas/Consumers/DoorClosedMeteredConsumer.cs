namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.Door;

    /// <inheritdoc />
    public class DoorClosedMeteredConsumer : Consumes<DoorClosedMeteredEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Creates a DoorClosedMeteredConsumer instance for posting door closed exceptions to SAS
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public DoorClosedMeteredConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(DoorClosedMeteredEvent theEvent)
        {
            if (SasExceptionDoorInfo.DoorExceptionMap.TryGetValue(
                (DoorLogicalId)theEvent.LogicalId,
                out var closedInfo))
            {
                _exceptionHandler.ReportException(new GenericExceptionBuilder(closedInfo.Closed));
            }
        }
    }
}