namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.Printer;

    /// <summary>
    ///     Handles the <see cref="HardwareFaultEvent" /> event.
    /// </summary>
    public class PrinterHardwareFaultConsumer : Consumes<HardwareFaultEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrinterHardwareFaultConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public PrinterHardwareFaultConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(HardwareFaultEvent theEvent)
        {
            switch (theEvent.Fault)
            {
                case PrinterFaultTypes.PrintHeadOpen:
                case PrinterFaultTypes.ChassisOpen:
                case PrinterFaultTypes.OtherFault:
                case PrinterFaultTypes.PrintHeadDamaged:
                    _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.GeneralTilt));
                    break;
                case PrinterFaultTypes.NvmFault:
                case PrinterFaultTypes.FirmwareFault:
                    _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.PrinterCommunicationError));
                    break;
                case PrinterFaultTypes.PaperJam:
                    _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.PrinterCarriageJam));
                    break;
                case PrinterFaultTypes.PaperEmpty:
                    _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.PrinterPaperOutError));
                    break;
            }
        }
    }
}
