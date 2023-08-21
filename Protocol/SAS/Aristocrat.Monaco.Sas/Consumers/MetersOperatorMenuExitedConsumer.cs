﻿namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     Handles the <see cref="MetersOperatorMenuExitedEvent" /> event.
    /// </summary>
    public class MetersOperatorMenuExitedConsumer : Consumes<MetersOperatorMenuExitedEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MetersOperatorMenuExitedConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        public MetersOperatorMenuExitedConsumer(ISasExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
        }

        /// <inheritdoc />
        public override void Consume(MetersOperatorMenuExitedEvent theEvent)
        {
            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.AttendantMenuExited));
        }
    }
}
