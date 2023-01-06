namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Sas.Client;
    using Contracts.SASProperties;
    using Exceptions;
    using Gaming.Contracts;
    using Kernel;

    /// <summary>
    ///     Handles the <see cref="GameProcessExitedEvent" /> event.
    /// </summary>
    public class GameProcessExitedConsumer : Consumes<GameProcessExitedEvent>
    {
        private const int LobbyAsSelectedGame = 0;
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly IPropertiesManager _propertiesManager;
        private readonly ILobbyStateManager _lobbyStateManager;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameProcessExitedConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        /// <param name="propertiesManager">The properties provider.</param>
        /// <param name="lobbyStateManager">The Lobby State Manager.</param>
        public GameProcessExitedConsumer(ISasExceptionHandler exceptionHandler, IPropertiesManager propertiesManager, ILobbyStateManager lobbyStateManager)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _lobbyStateManager = lobbyStateManager ?? throw new ArgumentNullException(nameof(lobbyStateManager));
        }

        /// <inheritdoc />
        public override void Consume(GameProcessExitedEvent theEvent)
        {
            var lastGameId = _propertiesManager.GetValue(SasProperties.PreviousSelectedGameId, LobbyAsSelectedGame);
            if (lastGameId != LobbyAsSelectedGame && !_lobbyStateManager.IsSingleGame)
            {
                _exceptionHandler.ReportException(new GameSelectedExceptionBuilder(LobbyAsSelectedGame));
                _propertiesManager.SetProperty(SasProperties.PreviousSelectedGameId, LobbyAsSelectedGame);
            }
        }
    }
}
