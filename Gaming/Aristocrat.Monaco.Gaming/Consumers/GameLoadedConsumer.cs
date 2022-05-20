namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Commands;
    using Contracts;
    using Kernel;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     Handles the GameLoadedEvent, when a game just finished loading
    /// </summary>
    public class GameLoadedConsumer : Consumes<GameLoadedEvent>
    {
        private readonly IEventBus _eventBus;
        private readonly IGameDiagnostics _gameDiagnostics;
        private readonly IGameRecovery _gameRecovery;
        private readonly IPropertiesManager _properties;
        private readonly IOperatorMenuLauncher _operatorMenu;
        private readonly ICommandHandlerFactory _handlerFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameLoadedConsumer" /> class.
        /// </summary>
        /// <param name="eventBus">An <see cref="IEventBus" /> instance.</param>
        /// <param name="gameDiagnostics">An <see cref="IGameDiagnostics" /> instance.</param>
        /// <param name="gameRecovery">An <see cref="IGameRecovery" /> instance.</param>
        /// <param name="properties">An <see cref="IPropertiesManager" /> instance.</param>
        /// <param name="operatorMenu">An <see cref="IOperatorMenuLauncher" /> instance.</param>
        /// <param name="handlerFactory">An <see cref="ICommandHandlerFactory" /> instance.</param>
        public GameLoadedConsumer(
            IEventBus eventBus,
            IGameDiagnostics gameDiagnostics,
            IGameRecovery gameRecovery,
            IPropertiesManager properties,
            IOperatorMenuLauncher operatorMenu,
            ICommandHandlerFactory handlerFactory)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gameDiagnostics = gameDiagnostics ?? throw new ArgumentNullException(nameof(gameDiagnostics));
            _gameRecovery = gameRecovery ?? throw new ArgumentNullException(nameof(gameRecovery));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _operatorMenu = operatorMenu ?? throw new ArgumentNullException(nameof(operatorMenu));
            _handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
        }

        /// <inheritdoc />
        public override void Consume(GameLoadedEvent theEvent)
        {
            // Replay gets all data from blob.  Do not record events etc.
            if (!_gameDiagnostics.IsActive)
            {
                _handlerFactory.Create<NotifyGameStarted>().Handle(new NotifyGameStarted());
            }
            else
            {
                _handlerFactory.Create<NotifyGameReplayStarted>().Handle(new NotifyGameReplayStarted());
            }

            // Keep it disabled if we are replaying, or recovering where required.
            if (!_gameDiagnostics.IsActive &&
                !(_gameRecovery.IsRecovering && _properties.GetValue(GamingConstants.OperatorMenuDisableDuringGame, false)))
            {
                _operatorMenu.EnableKey(GamingConstants.OperatorMenuDisableKey);
            }

            // Need to post this in replay because this is what triggers lobby to show game.
            // If we do not like this, we can make a new GameReplayInitializationCompletedEvent.
            _eventBus.Publish(new GameInitializationCompletedEvent());
        }
    }
}
