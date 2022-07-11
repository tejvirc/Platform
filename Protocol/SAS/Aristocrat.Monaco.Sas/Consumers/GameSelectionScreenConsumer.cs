namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Contracts.SASProperties;
    using Exceptions;
    using Gaming.Contracts;
    using Kernel;

    /// <summary>
    ///     Handles the <see cref="GameSelectionScreenEvent" /> event.
    /// </summary>
    public class GameSelectionScreenConsumer : Consumes<GameSelectionScreenEvent>
    {
        private const int SelectionScreenAsSelectedGame = 0;

        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly IPropertiesManager _propertiesManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameSelectionScreenConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        /// <param name="propertiesManager">The properties provider.</param>
        public GameSelectionScreenConsumer(
            ISasExceptionHandler exceptionHandler,
            IPropertiesManager propertiesManager)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        /// <inheritdoc />
        public override void Consume(GameSelectionScreenEvent theEvent)
        {
            // Note that when exiting the selection screen we don't need to do anything, as the
            // game or denomination change will be caught and handled elsewhere.
            if (theEvent.IsEntering)
            {
                var lastGameId = _propertiesManager.GetValue(SasProperties.PreviousSelectedGameId, 0);
                if (SelectionScreenAsSelectedGame != lastGameId)
                {
                    _exceptionHandler.ReportException(new GameSelectedExceptionBuilder(SelectionScreenAsSelectedGame));
                    _propertiesManager.SetProperty(SasProperties.PreviousSelectedGameId, SelectionScreenAsSelectedGame);
                }
            }
        }
    }
}