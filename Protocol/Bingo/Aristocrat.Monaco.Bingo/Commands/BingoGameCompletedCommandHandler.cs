namespace Aristocrat.Monaco.Bingo.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Services.Reporting;

    public class BingoGameCompletedCommandHandler : ICommandHandler<BingoGameEndedCommand>
    {
        private readonly IGameHistoryReportHandler _gameHistoryHandler;

        public BingoGameCompletedCommandHandler(IGameHistoryReportHandler gameHistoryHandler)
        {
            _gameHistoryHandler = gameHistoryHandler ?? throw new ArgumentNullException(nameof(gameHistoryHandler));
        }

        public Task Handle(BingoGameEndedCommand command, CancellationToken token = default)
        {
            var outcomeMessage = command.Transaction.ToReportGameOutcomeMessage(command.MachineSerial, command.Log);
            if (outcomeMessage is not null)
            {
                _gameHistoryHandler.AddReportToQueue(outcomeMessage);
            }

            return Task.CompletedTask;
        }
    }
}