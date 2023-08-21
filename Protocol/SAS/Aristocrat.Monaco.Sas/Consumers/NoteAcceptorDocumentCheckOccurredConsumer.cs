namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Aristocrat.Monaco.Application.Contracts.NoteAcceptorMonitor;

    /// <summary>
    ///     Handles the <see cref="NoteAcceptorDocumentCheckOccurredEvent" /> event.
    /// </summary>
    public class NoteAcceptorDocumentCheckOccurredConsumer : Consumes<NoteAcceptorDocumentCheckOccurredEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorDocumentCheckOccurredConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public NoteAcceptorDocumentCheckOccurredConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(NoteAcceptorDocumentCheckOccurredEvent theEvent)
        {
            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.GeneralTilt));
        }
    }
}