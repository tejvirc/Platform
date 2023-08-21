namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Application.Contracts;
    using Aristocrat.Sas.Client;

    /// <summary>
    ///     Handles the <see cref="SystemDisabledByOperatorEvent" /> event.
    /// </summary>
    public class SystemDisabledByOperatorEventConsumer : Consumes<SystemDisabledByOperatorEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemDisabledByOperatorEventConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of the <see cref="ISasExceptionHandler" />t</param>
        public SystemDisabledByOperatorEventConsumer(
            ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(SystemDisabledByOperatorEvent theEvent)
        {
            _exceptionHandler.ReportException(
                new GenericExceptionBuilder(GeneralExceptionCode.GamingMachineOutOfServiceByOperator));
        }
    }
}