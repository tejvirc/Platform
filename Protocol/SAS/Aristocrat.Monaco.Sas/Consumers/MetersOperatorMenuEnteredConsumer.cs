namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     Handles the <see cref="MetersOperatorMenuEnteredEvent" /> event.
    /// </summary>
    public class MetersOperatorMenuEnteredConsumer : Consumes<MetersOperatorMenuEnteredEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MetersOperatorMenuEnteredConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public MetersOperatorMenuEnteredConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(MetersOperatorMenuEnteredEvent theEvent)
        {
            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.AttendantMenuEntered));
        }
    }
}
