namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Gaming.Contracts;

    /// <inheritdoc />
    public class GameAddedConsumer : Consumes<GameAddedEvent>
    {
        private readonly IGameProvider _gameProvider;

        /// <summary>
        ///     Creates a Consumer of the event <see cref="GameAddedConsumer"/>
        /// </summary>
        /// <param name="gameProvider">An <see cref="IGameProvider"/> instance</param>
        public GameAddedConsumer(IGameProvider gameProvider)
        {
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        /// <inheritdoc />
        public override void Consume(GameAddedEvent theEvent)
        {
            _gameProvider.EnableGame(theEvent.GameId, GameStatus.DisabledByBackend);
        }
    }
}