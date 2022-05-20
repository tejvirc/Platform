namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using System.Reflection;
    using Contracts;
    using log4net;

    /// <summary>
    ///     Handles the TerminateProcessConsumer, which terminates the current game process
    /// </summary>
    public class TerminateGameConsumer : Consumes<TerminateGameProcessEvent>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IGameService _gameService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TerminateGameConsumer" /> class.
        /// </summary>
        /// <param name="gameService">The game service</param>
        public TerminateGameConsumer(IGameService gameService)
        {
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
        }

        /// <inheritdoc />
        public override void Consume(TerminateGameProcessEvent theEvent)
        {
            Logger.Info("Ending game process from TerminateGameProcessEvent");
            _gameService.TerminateAny();
        }
    }
}
