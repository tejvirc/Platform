namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.Persistence;

    /// <summary>
    ///     Handles the <see cref="PersistentStorageIntegrityCheckFailedEvent" /> event.
    /// </summary>
    public class PersistentStorageIntegrityCheckFailedConsumer : Consumes<PersistentStorageIntegrityCheckFailedEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PersistentStorageIntegrityCheckFailedConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public PersistentStorageIntegrityCheckFailedConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(PersistentStorageIntegrityCheckFailedEvent theEvent)
        {
            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.EePromDataError));
        }
    }
}
