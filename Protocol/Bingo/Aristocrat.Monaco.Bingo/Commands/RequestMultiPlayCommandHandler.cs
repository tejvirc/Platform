namespace Aristocrat.Monaco.Bingo.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.GamePlay;

    /// <summary>
    ///     Handles the RequestMultiPlayCommand
    /// </summary>
    public class RequestMultiPlayCommandHandler : ICommandHandler<RequestMultiPlayCommand>
    {
        private readonly IGameOutcomeService _gameOutcomeService;

        public RequestMultiPlayCommandHandler(IGameOutcomeService gameOutcomeService)
        {
            _gameOutcomeService = gameOutcomeService ?? throw new ArgumentNullException(nameof(gameOutcomeService));
        }

        /// <inheritdoc />
        public async Task Handle(RequestMultiPlayCommand command, CancellationToken token)
        {
            var multiMessage = new RequestMultipleGameOutcomeMessage(command.MachineSerial, command.GameRequests);
            await _gameOutcomeService.RequestMultiGame(multiMessage, token);
        }
    }
}
