namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Contracts;

    /// <summary>
    ///     Handles the EndGameProcessEvent, which terminates the current game process
    /// </summary>
    public class GameConnectedConsumer : Consumes<GameConnectedEvent>
    {
        private readonly IGameService _gameService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameConnectedConsumer" /> class.
        /// </summary>
        /// <param name="gameService">The game service</param>
        public GameConnectedConsumer(IGameService gameService)
        {
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
        }

        /// <inheritdoc />
        public override void Consume(GameConnectedEvent theEvent)
        {
            _gameService.Connected();
        }
    }
}
