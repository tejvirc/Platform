namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Sas.Client;
    using Contracts.SASProperties;
    using Exceptions;
    using Kernel;

    /// <summary>
    ///     Handles the <see cref="DenominationSelectedEvent" /> event.
    /// </summary>
    public class DenominationSelectedConsumer : Consumes<DenominationSelectedEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly IPropertiesManager _propertiesManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DenominationSelectedConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        /// <param name="propertiesManager">The properties provider.</param>
        public DenominationSelectedConsumer(
            ISasExceptionHandler exceptionHandler,
            IPropertiesManager propertiesManager)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        /// <inheritdoc />
        public override void Consume(DenominationSelectedEvent theEvent)
        {
            var selectedDenom = _propertiesManager.GetValue(GamingConstants.SelectedDenom, 0L);

            if (theEvent.Denomination != selectedDenom)
            {
                _exceptionHandler.ReportException(new GameSelectedExceptionBuilder(theEvent.GameId));
                _propertiesManager.SetProperty(SasProperties.PreviousSelectedGameId, theEvent.GameId);
            }
        }
    }
}
