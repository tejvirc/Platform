namespace Aristocrat.Monaco.Bingo.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.GamePlay;

    public class RequestPlayCommandHandler : ICommandHandler<RequestPlayCommand>
    {
        private readonly IGameOutcomeService _gameOutcomeService;

        public RequestPlayCommandHandler(IGameOutcomeService gameOutcomeService)
        {
            _gameOutcomeService = gameOutcomeService ?? throw new ArgumentNullException(nameof(gameOutcomeService));
        }

        /// <inheritdoc />
        public async Task Handle(RequestPlayCommand command, CancellationToken token)
        {
            var message = new RequestGameOutcomeMessage(
                command.MachineSerial,
                command.BetAmount,
                command.ActiveDenomination,
                command.BetLinePresetId,
                command.LineBet,
                command.Lines,
                command.Ante,
                command.ActiveGameTitles);

            await _gameOutcomeService.RequestGame(message, token);
        }
    }
}
