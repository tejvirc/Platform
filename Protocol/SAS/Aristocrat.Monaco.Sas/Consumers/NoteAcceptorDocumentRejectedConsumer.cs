namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Contracts.Client;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.NoteAcceptor;

    /// <summary>
    ///     Handles the <see cref="DocumentRejectedEvent" /> event.
    /// </summary>
    public class NoteAcceptorDocumentRejectedConsumer : Consumes<DocumentRejectedEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly ISasNoteAcceptorProvider _sasNoteAcceptorProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorDocumentRejectedConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        /// <param name="sasNoteAcceptorProvider">An instance of <see cref="ISasNoteAcceptorProvider"/></param>
        public NoteAcceptorDocumentRejectedConsumer(ISasExceptionHandler exceptionHandler, ISasNoteAcceptorProvider sasNoteAcceptorProvider)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _sasNoteAcceptorProvider = sasNoteAcceptorProvider ??
                                       throw new ArgumentNullException(nameof(sasNoteAcceptorProvider));
        }

        /// <inheritdoc />
        public override void Consume(DocumentRejectedEvent theEvent)
        {
            if (!_sasNoteAcceptorProvider.DiagnosticTestActive)
            {
                _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.BillRejected));
            }
        }
    }
}
