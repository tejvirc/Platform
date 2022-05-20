namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.Printer;

    /// <summary>
    ///     Handles the <see cref="HardwareWarningEvent" /> event.
    /// </summary>
    public class PrinterHardwareWarningConsumer : Consumes<HardwareWarningEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrinterHardwareWarningConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public PrinterHardwareWarningConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(HardwareWarningEvent theEvent)
        {
            switch (theEvent.Warning)
            {
                case PrinterWarningTypes.PaperLow:
                    _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.PrinterPaperLow));
                    break;

                case PrinterWarningTypes.PaperInChute:
                    // What should we do about this?
                    break;
            }
        }
    }
}
