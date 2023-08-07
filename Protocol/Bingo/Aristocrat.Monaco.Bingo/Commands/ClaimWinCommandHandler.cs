namespace Aristocrat.Monaco.Bingo.Commands
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.GamePlay;
    using log4net;
    using Services.GamePlay;

    public class ClaimWinCommandHandler : ICommandHandler<ClaimWinCommand>
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IGameOutcomeService _gameOutcomeService;
        private readonly IBingoGameOutcomeHandler _gameOutcomeHandler;

        public ClaimWinCommandHandler(
            IGameOutcomeService gameOutcomeService,
            IBingoGameOutcomeHandler gameOutcomeHandler)
        {
            _gameOutcomeService = gameOutcomeService ?? throw new ArgumentNullException(nameof(gameOutcomeService));
            _gameOutcomeHandler = gameOutcomeHandler ?? throw new ArgumentNullException(nameof(gameOutcomeHandler));
        }

        /// <inheritdoc />
        public async Task Handle(ClaimWinCommand command, CancellationToken token)
        {
            try
            {
                var message = new RequestClaimWinMessage(
                    command.MachineSerial,
                    command.GameSerial,
                    command.CardSerial);

                var results = await _gameOutcomeService.ClaimWin(message, token);
                await _gameOutcomeHandler.ProcessClaimWin(results, token);
            }
            catch (Exception e)
            {
                Logger.Error("An error occured when trying to claim the GEW", e);
            }
        }
    }
}
