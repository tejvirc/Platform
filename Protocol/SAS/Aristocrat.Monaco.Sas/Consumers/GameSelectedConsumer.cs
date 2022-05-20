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
        private const int DefaultSelectedGame = 0;
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IGameProvider _gameProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameSelectedConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        /// <param name="propertiesManager">The properties provider.</param>
        /// <param name="gameProvider">An instance of <see cref="IGameProvider"/></param>
        public GameSelectedConsumer(
            ISasExceptionHandler exceptionHandler,
            IPropertiesManager propertiesManager,
            IGameProvider gameProvider)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        /// <inheritdoc />
        public override void Consume(GameSelectedEvent theEvent)
        {
            var selectedGameId = _propertiesManager.GetValue(GamingConstants.SelectedGameId, DefaultSelectedGame);
            var selectedDenom = _propertiesManager.GetValue(GamingConstants.SelectedDenom, 0L);
            var gameId = (int)(_gameProvider.GetGameId(selectedGameId, selectedDenom) ?? DefaultSelectedGame);

            if (gameId == theEvent.GameId)
            {
                return;
            }

            _exceptionHandler.ReportException(new GameSelectedExceptionBuilder(theEvent.GameId));
            _propertiesManager.SetProperty(SasProperties.PreviousSelectedGameId, theEvent.GameId);
        }
    }
}
