namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.Battery;

    /// <summary>
    ///     Handles the <see cref="BatteryLowEvent" /> event.
    /// </summary>
    public class BatteryLowConsumer : Consumes<BatteryLowEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BatteryLowConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler" /></param>
        public BatteryLowConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(BatteryLowEvent batteryLowEvent)
        {
            _exceptionHandler.ReportException(
                new GenericExceptionBuilder(GeneralExceptionCode.LowBackupBatteryDetected));
        }
    }
}