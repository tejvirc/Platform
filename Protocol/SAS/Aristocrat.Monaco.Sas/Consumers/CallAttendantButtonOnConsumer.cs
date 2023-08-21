namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Gaming.Contracts;

    /// <summary>
    ///     Handles the CallAttendantButtonOnEvent, which sets the cabinet's service lamp status.
    /// </summary>
    public class CallAttendantButtonOnConsumer : Consumes<CallAttendantButtonOnEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CallAttendantButtonOnConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public CallAttendantButtonOnConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(CallAttendantButtonOnEvent theEvent)
        {
            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.ChangeLampOn));
        }
    }
}
