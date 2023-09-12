namespace Aristocrat.Bingo.Client.Messages
{
    using System.Threading;
    using System.Threading.Tasks;
    using GamePlay;
    using ServerApiGateway;

    /// <summary>
    ///     The game play service
    /// </summary>
    public interface IGameOutcomeService
    {
        /// <summary>
        ///     Requests a game play
        /// </summary>
        /// <param name="message">The multi-game request message</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>Successfully requested.</returns>
        Task<bool> RequestMultiGame(RequestMultipleGameOutcomeMessage message, CancellationToken token);

        /// <summary>
        ///     Requests a claim win
        /// </summary>
        /// <param name="message">The claim win message</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>Successfully requested.</returns>
        Task<ClaimWinResults> ClaimWin(RequestClaimWinMessage message, CancellationToken token);

        /// <summary>
        ///     Reports multiple game outcomes to the server
        /// </summary>
        /// <param name="message">The report game outcomes message</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The response for the reported outcome</returns>
        Task<GameOutcomeAck> ReportMultiGameOutcome(ReportMultiGameOutcomeMessage message, CancellationToken token);
    }
}