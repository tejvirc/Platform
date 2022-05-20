namespace Aristocrat.Monaco.Bingo.Handlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.GamePlay;
    using Services.GamePlay;

    public class GameOutcomeResponseHandler : IMessageHandler<GameOutcomeResponse, GameOutcome>
    {
        private readonly IBingoGameOutcomeHandler _outcomeHandler;

        public GameOutcomeResponseHandler(IBingoGameOutcomeHandler outcomeHandler)
        {
            _outcomeHandler = outcomeHandler ?? throw new ArgumentNullException(nameof(outcomeHandler));
        }

        public async Task<GameOutcomeResponse> Handle(GameOutcome outcome, CancellationToken token)
        {
            // We let the long-lived entity handle these short-lived messages,
            // because it tracks stuff over the long haul.
            var result = await _outcomeHandler.ProcessGameOutcome(outcome, token);
            return new GameOutcomeResponse(result ? ResponseCode.Ok : ResponseCode.Rejected);
        }
    }
}
