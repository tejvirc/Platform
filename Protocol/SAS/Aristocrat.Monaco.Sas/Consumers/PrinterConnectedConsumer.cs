namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.Printer;

    /// <summary>
    ///     Handles the <see cref="ConnectedEvent" /> event.
    /// </summary>
    public class PrinterConnectedConsumer : Consumes<ConnectedEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrinterConnectedConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public PrinterConnectedConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(ConnectedEvent theEvent)
        {
            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.PrinterPowerOn));
        }
    }
}
