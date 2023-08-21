namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Exceptions;
    using Gaming.Contracts;

    /// <summary>
    ///     Handles the <see cref="GameDiagnosticsStartedEvent" /> event.
    /// </summary>
    public class GameDiagnosticsStartedConsumer : Consumes<GameDiagnosticsStartedEvent>
    {
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly IGameProvider _gameProvider;
        private readonly IGameHistory _gameHistory;
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameDiagnosticsStartedConsumer" /> class.
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        /// <param name="gameProvider">An instance of <see cref="IGameProvider"/></param>
        /// <param name="gameHistory">An instance of <see cref="IGameHistory"/></param>
        public GameDiagnosticsStartedConsumer(ISasExceptionHandler exceptionHandler, IGameProvider gameProvider, IGameHistory gameHistory)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
        }

        /// <inheritdoc />
        public override void Consume(GameDiagnosticsStartedEvent theEvent)
        {
            var gameId = _gameProvider.GetGameId(theEvent.GameId, theEvent.Denomination);
            if (!(theEvent.Context is IDiagnosticContext<IGameHistoryLog> context) || !gameId.HasValue)
            {
                return;
            }

            _exceptionHandler.ReportException(new GameRecallEntryDisplayedExceptionBuilder(gameId.Value, _gameHistory.LogSequence - context.Arguments.LogSequence));
        }
    }
}
