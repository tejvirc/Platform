namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Hardware.Contracts.Persistence;

    /// <summary>
    ///     Handles the <see cref="StorageErrorEvent" /> event.
    /// </summary>
    public class StorageErrorConsumer : Consumes<StorageErrorEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StorageErrorConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public StorageErrorConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(StorageErrorEvent @event)
        {
            if (@event.Id == StorageError.ReadFailure || @event.Id == StorageError.WriteFailure)
            {
                _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.EePromDataError));
            }
        }
    }
}
