namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Contracts.SASProperties;
    using Exceptions;
    using Gaming.Contracts;
    using Kernel;

    /// <summary>
    ///     Handles the GameSelected event, which launches the selected game
    /// </summary>
    public class GameSelectedConsumer : Consumes<GameSelectedEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly IPropertiesManager _propertiesManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameSelectedConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        /// <param name="propertiesManager">The properties provider.</param>
        public GameSelectedConsumer(
            ISasExceptionHandler exceptionHandler,
            IPropertiesManager propertiesManager)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        /// <inheritdoc />
        public override void Consume(GameSelectedEvent theEvent)
        {
            var lastGameId = _propertiesManager.GetValue(SasProperties.PreviousSelectedGameId, 0);
            if (theEvent.GameId == lastGameId)
            {
                return;
            }

            _exceptionHandler.ReportException(new GameSelectedExceptionBuilder(theEvent.GameId));
            _propertiesManager.SetProperty(SasProperties.PreviousSelectedGameId, theEvent.GameId);
        }
    }
}
