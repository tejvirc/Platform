namespace Aristocrat.Monaco.Bingo.Handlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.GamePlay;
    using Services.GamePlay;

    /// <summary>
    ///     Handles the GameOutcomes message and sends the list of game outcomes to
    ///     the CentralHandler which gives a single GameOutcomeResponse for all the outcomes
    /// </summary>
    public class GameOutcomesResponseHandler : IMessageHandler<GameOutcomeResponse, GameOutcomes>
    {
        private readonly IBingoGameOutcomeHandler _outcomeHandler;

        public GameOutcomesResponseHandler(IBingoGameOutcomeHandler outcomeHandler)
        {
            _outcomeHandler = outcomeHandler ?? throw new ArgumentNullException(nameof(outcomeHandler));
        }

        /// <inheritdoc/>
        public async Task<GameOutcomeResponse> Handle(GameOutcomes outcomes, CancellationToken token)
        {
            // We let the long-lived entity handle these short-lived messages,
            // because it tracks stuff over the long haul.
            var result = await _outcomeHandler.ProcessGameOutcomes(outcomes, token);
            return new GameOutcomeResponse(result ? ResponseCode.Ok : ResponseCode.Rejected);
        }
    }
}