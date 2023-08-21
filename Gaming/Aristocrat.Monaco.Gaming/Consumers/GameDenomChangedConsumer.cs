namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using System.Linq;
    using Contracts;
    using Kernel;

    public class GameDenomChangedConsumer : Consumes<GameDenomChangedEvent>
    {
        private readonly IEventBus _bus;
        private readonly IGamePlayState _gamePlayState;
        private readonly IGameService _gameService;
        private readonly IPropertiesManager _properties;

        public GameDenomChangedConsumer(
            IPropertiesManager properties,
            IGameService gameService,
            IGamePlayState gamePlayState,
            IEventBus bus)
        {
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public override void Consume(GameDenomChangedEvent theEvent)
        {
            var gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
            var denomId = _properties.GetValue(GamingConstants.SelectedDenom, 0L);
            if (gameId == theEvent.GameId && !theEvent.Details.ActiveDenominations.Contains(denomId))
            {
                if (!_gamePlayState.Idle)
                {
                    _bus.Subscribe<GameIdleEvent>(
                        this,
                        _ =>
                        {
                            _gameService.ShutdownBegin();
                            _bus.UnsubscribeAll(this);
                        });
                }
                else
                {
                    _gameService.ShutdownBegin();
                }
            }
        }
    }
}