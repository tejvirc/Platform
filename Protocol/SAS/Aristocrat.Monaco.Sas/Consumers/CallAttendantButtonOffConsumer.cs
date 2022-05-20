namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Gaming.Contracts;

    /// <summary>
    ///     Handles the CallAttendantButtonOffEvent, which sets the cabinet's service lamp status.
    /// </summary>
    public class CallAttendantButtonOffConsumer : Consumes<CallAttendantButtonOffEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CallAttendantButtonOffConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public CallAttendantButtonOffConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(CallAttendantButtonOffEvent theEvent)
        {
            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.ChangeLampOff));
        }
    }
}
