namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.Door;

    /// <inheritdoc />
    public class DoorOpenMeteredConsumer : Consumes<DoorOpenMeteredEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Creates a DoorOpenMeteredConsumer instance for posting door opened exceptions to SAS
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public DoorOpenMeteredConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(DoorOpenMeteredEvent theEvent)
        {
            if ((!theEvent.WhilePoweredDown || !theEvent.IsRecovery) &&
                SasExceptionDoorInfo.DoorExceptionMap.TryGetValue(
                    (DoorLogicalId)theEvent.LogicalId,
                    out var openInfo))
            {
                _exceptionHandler.ReportException(new GenericExceptionBuilder(openInfo.Opened));
            }

            if (theEvent.WhilePoweredDown &&
                SasExceptionDoorInfo.DoorAccessWhilePowerOffMap.TryGetValue(
                    (DoorLogicalId)theEvent.LogicalId,
                    out var exception))
            {
                _exceptionHandler.ReportException(new GenericExceptionBuilder(exception));
            }
        }
    }
}