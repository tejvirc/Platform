namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using System.Reflection;
    using Common;
    using Contracts;
    using Kernel;
    using log4net;
    using Runtime;

    /// <summary>
    ///     Handles the <see cref="GameProcessHungEvent" />, which terminates the current game process.
    ///     We use the Kernel.Consumes because we need this to be on its own consumer context so that we
    ///     will act immediately and not be blocked by other event handling.
    /// </summary>
    public class GameProcessHungConsumer : Kernel.Consumes<GameProcessHungEvent>
    {
        private const string DoNotKillRuntimeKey = "doNotKillRuntime";
        private const string GameProcessHungMiniDumpKey = "gameProcessHungMiniDump";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IGameProcess _gameProcess;
        private readonly IGameService _gameService;
        private readonly IPropertiesManager _properties;

        public GameProcessHungConsumer(
            IEventBus eventBus,
            IGameService gameService,
            IGameProcess gameProcess,
            IPropertiesManager properties)
            : base(eventBus)
        {
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _gameProcess = gameProcess ?? throw new ArgumentNullException(nameof(gameProcess));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public override void Consume(GameProcessHungEvent theEvent)
        {
#if !RETAIL
            var gameProcessHungMinidump =
                _properties.GetValue(GameProcessHungMiniDumpKey, Constants.False).ToUpperInvariant();
            if (gameProcessHungMinidump == Constants.True)
            {
                _gameService.CreateMiniDump();
            }
#endif

            if (_gameProcess.IsRunning(theEvent.ProcessId))
            {
#if !RETAIL
                var doNotKillRuntime = _properties.GetValue(DoNotKillRuntimeKey, Constants.False).ToUpperInvariant();
                if (doNotKillRuntime == Constants.False)
#endif
                {
                    Logger.Info("Ending game process from GameProcessHungEvent");
                    _gameService.Terminate(theEvent.ProcessId);
                }
            }
            else
            {
                Logger.Debug($"Process with id {theEvent.ProcessId} is not running");
            }
        }
    }
}