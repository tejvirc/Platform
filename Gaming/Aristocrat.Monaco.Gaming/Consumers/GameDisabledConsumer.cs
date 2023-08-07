namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using System.Reflection;
    using Contracts;
    using Kernel;
    using log4net;

    public class GameDisabledConsumer : Consumes<GameDisabledEvent>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEventBus _bus;
        private readonly IGamePlayState _gamePlayState;
        private readonly IGameService _gameService;
        private readonly IPropertiesManager _properties;

        public GameDisabledConsumer(
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

        public override void Consume(GameDisabledEvent theEvent)
        {
            var gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
            if (gameId == theEvent.GameId)
            {
                if (!_gamePlayState.Idle)
                {
                    _bus.Subscribe<GameIdleEvent>(
                        this,
                        _ =>
                        {
                            Logger.Info("Ending game process from GameDisabledEvent");
                            _gameService.TerminateAny();

                            _bus.UnsubscribeAll(this);
                        });
                }
                else
                {
                    _gameService.TerminateAny();
                }
            }
        }
    }
}
