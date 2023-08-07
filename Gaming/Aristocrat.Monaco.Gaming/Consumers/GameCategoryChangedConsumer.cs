namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using System.Reflection;
    using Contracts;
    using Kernel;
    using log4net;
    using Runtime;

    /// <summary>
    ///     Handles the GameCategoryChangedEvent
    /// </summary>
    public class GameCategoryChangedConsumer : Consumes<GameCategoryChangedEvent>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IGameService _gameService;
        private readonly IGamePlayState _gamePlayState;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IGameProvider _gameProvider;
        private readonly IRuntime _runtime;

        public GameCategoryChangedConsumer(
            IRuntime runtime,
            IGameService gameService,
            IGamePlayState gamePlayState,
            IPropertiesManager propertiesManager,
            IGameProvider gameProvider)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
        }

        /// <inheritdoc />
        public override void Consume(GameCategoryChangedEvent theEvent)
        {
            if (!_gameService.Running || _gamePlayState.InGameRound || !_runtime.Connected)
            {
                return;
            }

            var gameId = (int)_propertiesManager.GetProperty(GamingConstants.SelectedGameId, -1);
            if (gameId == -1)
            {
                Logger.Error("Invalid selected game Id");
                return;
            }

            var game = _gameProvider.GetGame(gameId);

            if (game.GameType == theEvent.GameType)
            {
                _runtime.Shutdown();
            }
        }
    }
}
