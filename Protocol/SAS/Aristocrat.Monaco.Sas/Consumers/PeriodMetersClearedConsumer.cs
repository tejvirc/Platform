namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Application.Contracts;
    using Aristocrat.Sas.Client;

    /// <summary>
    ///     Handles the <see cref="PeriodMetersClearedEvent" /> event.
    /// </summary>
    public class PeriodMetersClearedConsumer : Consumes<PeriodMetersClearedEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PeriodMetersClearedConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public PeriodMetersClearedConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(PeriodMetersClearedEvent theEvent)
        {
            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.BillValidatorPeriodMetersReset));
        }
    }
}
