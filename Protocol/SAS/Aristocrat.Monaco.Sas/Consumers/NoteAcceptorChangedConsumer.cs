namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.NoteAcceptor;

    /// <summary>
    ///     Handles the <see cref="NoteAcceptorChangedEvent" /> event.
    ///     This event is sent by the NoteAcceptorAdapter when
    ///     the inspection indicates a different note acceptor
    ///     is present compared to the last inspection.
    /// </summary>
    public class NoteAcceptorChangedConsumer : Consumes<NoteAcceptorChangedEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Creates a NoteAcceptorChangedConsumer instance for posting
        ///     note acceptor changed exceptions to SAS
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public NoteAcceptorChangedConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(NoteAcceptorChangedEvent theEvent)
        {
            _exceptionHandler.ReportException(
                new GenericExceptionBuilder(GeneralExceptionCode.BillAcceptorVersionChanged));
        }
    }
}