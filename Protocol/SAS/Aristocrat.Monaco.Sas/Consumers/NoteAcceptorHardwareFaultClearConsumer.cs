namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.NoteAcceptor;

    /// <summary>
    ///     Handles the <see cref="HardwareFaultClearEvent" /> event.
    /// </summary>
    public class NoteAcceptorHardwareFaultClearConsumer : Consumes<HardwareFaultClearEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorHardwareFaultClearConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public NoteAcceptorHardwareFaultClearConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(HardwareFaultClearEvent theEvent)
        {
            if (theEvent.Fault == NoteAcceptorFaultTypes.StackerDisconnected)
            {
                _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.CashBoxWasInstalled));
            }
        }
    }
}
