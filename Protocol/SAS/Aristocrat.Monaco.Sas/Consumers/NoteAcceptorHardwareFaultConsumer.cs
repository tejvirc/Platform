namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.NoteAcceptor;

    /// <summary>
    ///     Handles the <see cref="HardwareFaultEvent" /> event.
    /// </summary>
    public class NoteAcceptorHardwareFaultConsumer : Consumes<HardwareFaultEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        private static readonly IReadOnlyDictionary<NoteAcceptorFaultTypes, GeneralExceptionCode> SasExceptionMapping =
            new Dictionary<NoteAcceptorFaultTypes, GeneralExceptionCode>
            {
                { NoteAcceptorFaultTypes.StackerDisconnected, GeneralExceptionCode.CashBoxWasRemoved },
                { NoteAcceptorFaultTypes.StackerFull, GeneralExceptionCode.CashBoxFullDetected },
                { NoteAcceptorFaultTypes.StackerJammed, GeneralExceptionCode.BillJam },
                { NoteAcceptorFaultTypes.NoteJammed, GeneralExceptionCode.BillJam },
            };

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorHardwareFaultConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public NoteAcceptorHardwareFaultConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(HardwareFaultEvent theEvent)
        {
            if (theEvent.Fault == NoteAcceptorFaultTypes.None)
            {
                return;
            }

            var generalException = SasExceptionMapping.TryGetValue(theEvent.Fault, out var exception)
                ? exception
                : GeneralExceptionCode.BillAcceptorHardwareFailure;
            _exceptionHandler.ReportException(new GenericExceptionBuilder(generalException));
        }
    }
}
