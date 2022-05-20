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
        private readonly IEventBus _bus;
        private readonly IPropertiesManager _properties;
        private readonly IGameRecovery _recovery;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BeginGameRoundCommandHandler" /> class.
        /// </summary>
        /// <param name="recovery">An <see cref="IGameRecovery" /> instance.</param>
        /// <param name="gamePlayState"></param>
        /// <param name="properties">An <see cref="IPropertiesManager" /> instance.</param>
        /// <param name="bus">An <see cref="IEventBus" /> instance.</param>
        public BeginGameRoundCommandHandler(
            IGameRecovery recovery,
            IGamePlayState gamePlayState,
            IPropertiesManager properties,
            IEventBus bus)
        {
            _recovery = recovery ?? throw new ArgumentNullException(nameof(recovery));
            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc />
        public void Handle(BeginGameRound command)
        {
            if (!_recovery.IsRecovering && !_gamePlayState.Prepare())
            {
                Logger.Warn("Failed to start game round.");

                command.Success = false;
                _bus.Publish(new GameRequestFailedEvent());
                return;
            }

            var (game, _) = _properties.GetActiveGame();

            _properties.SetProperty(GamingConstants.SelectedWagerCategory, game.WagerCategories.FirstOrDefault());

            command.Success = true;

            Logger.Debug("Successfully started game round");
        }
    }
}