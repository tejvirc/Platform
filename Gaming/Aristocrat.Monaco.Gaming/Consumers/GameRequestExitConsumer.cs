namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Contracts;
    using Kernel;

    /// <summary>
    ///     Handles the GameRequestExitConsumer, which terminates the current game process
    /// </summary>
    public class GameRequestExitConsumer : Gaming.Consumers.Consumes<GameRequestExitEvent>
    {
        private readonly IGameService _gameService;
        private readonly IPropertiesManager _properties;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameRequestExitConsumer" /> class.
        /// </summary>
        /// <param name="gameService">The game service</param>
        /// <param name="properties">An <see cref="IPropertiesManager" /> instance</param>
        public GameRequestExitConsumer(IGameService gameService, IPropertiesManager properties)
        {
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        /// <inheritdoc />
        public override void Consume(GameRequestExitEvent theEvent)
        {
            _gameService.ShutdownBegin();
        }
    }
}
