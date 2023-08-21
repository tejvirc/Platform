namespace Aristocrat.Monaco.Gaming.Commands
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Common.PerformanceCounters;
    using Contracts;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Command handler for the <see cref="BeginGameRound" /> command.
    /// </summary>
    [CounterDescription("Game Start", PerformanceCounterType.AverageTimer32)]
    public class BeginGameRoundCommandHandler : ICommandHandler<BeginGameRound>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IGamePlayState _gamePlayState;
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;
        private readonly IGameRecovery _recovery;
        private readonly IGameDiagnostics _gameDiagnostics;
        private readonly IGameStartConditionProvider _gameStartConditions;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BeginGameRoundCommandHandler" /> class.
        /// </summary>
        public BeginGameRoundCommandHandler(
            IGameRecovery recovery,
            IGameDiagnostics diagnostics,
            IGamePlayState gamePlayState,
            IPropertiesManager properties,
            IEventBus eventBus,
            IGameStartConditionProvider gameStartConditions)
        {
            _recovery = recovery
                ?? throw new ArgumentNullException(nameof(recovery));
            _gamePlayState = gamePlayState
                ?? throw new ArgumentNullException(nameof(gamePlayState));
            _gameDiagnostics = diagnostics
                ?? throw new ArgumentNullException(nameof(diagnostics));
            _properties = properties
                ?? throw new ArgumentNullException(nameof(properties));
            _eventBus = eventBus
                ?? throw new ArgumentNullException(nameof(eventBus));
            _gameStartConditions = gameStartConditions
                ?? throw new ArgumentNullException(nameof(gameStartConditions));
        }

        /// <inheritdoc />
        public void Handle(BeginGameRound command)
        {
            if (!_recovery.IsRecovering && (!_gameStartConditions.CheckGameStartConditions() || !_gamePlayState.Prepare()))
            {
                Logger.Warn("Failed to start game round.");

                command.Success = false;
                _eventBus.Publish(new GameRequestFailedEvent());
                return;
            }

            var (game, _) = _properties.GetActiveGame();

            _properties.SetProperty(GamingConstants.SelectedWagerCategory, game.WagerCategories.FirstOrDefault());

            command.Success = true;

            Logger.Debug("Successfully started game round");
        }
    }
}