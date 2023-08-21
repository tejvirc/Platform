namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using System.Reflection;
    using Commands;
    using Contracts;
    using log4net;

    public class GameProcessExitedConsumer : Consumes<GameProcessExitedEvent>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPlayerBank _bank;
        private readonly IGameService _gameService;
        private readonly ICommandHandlerFactory _handlerFactory;

        public GameProcessExitedConsumer(
            IGameService gameService,
            IPlayerBank bank,
            ICommandHandlerFactory handlerFactory)
        {
            _handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
        }

        /// <inheritdoc />
        public override void Consume(GameProcessExitedEvent theEvent)
        {
            if (theEvent.Unexpected)
            {
                Logger.Info("Ending game process from GameProcessExitedEvent");
                _gameService.Terminate(theEvent.ProcessId, false);
            }
            else
            {
                _gameService.ShutdownEnd();
            }

            if (_bank.Balance == 0)
            {
                // Clear the session based local storage when reaching a zero balance
                _handlerFactory.Create<ClearSessionData>().Handle(new ClearSessionData());
            }
        }
    }
}
