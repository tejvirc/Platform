namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using System.Reflection;
    using Contracts;
    using log4net;
    using Runtime;

    /// <summary>
    ///     Handles the EndGameProcessEvent, which terminates the current game process
    /// </summary>
    public class GameShutdownStartedConsumer : Consumes<GameShutdownStartedEvent>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IGameService _gameService;
        private readonly IRuntime _runtime;

        public GameShutdownStartedConsumer(IRuntime runtime, IGameService gameService)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
        }

        public override void Consume(GameShutdownStartedEvent theEvent)
        {
            if (_runtime.Connected)
            {
                _runtime.Shutdown();

                Logger.Debug("Game shutdown invoked.");
            }
            else
            {
                Logger.Warn("Runtime channel is not open. Terminating game process.");

                // In the event of a game shutdown request if runtime service in not connected kill the game process
                Logger.Info("Ending game process from GameShutdownStartedEvent");
                _gameService.TerminateAny();
            }
        }
    }
}