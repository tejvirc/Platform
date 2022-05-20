namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.IO;

    /// <summary>
    ///     Handles the <see cref="ErrorEvent" /> event.
    /// </summary>
    public class IOErrorConsumer : Consumes<ErrorEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IOErrorConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public IOErrorConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(ErrorEvent errorEvent)
        {
            switch (errorEvent.Id)
            {
                case ErrorEventId.InvalidHandle:
                case ErrorEventId.ReadBoardInfoFailure:
                    _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.EePromBadDeviceError));
                    break;

                case ErrorEventId.BatteryStatusFailure:
                    _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.LowBackupBatteryDetected));
                    break;
            }
        }
    }
}
