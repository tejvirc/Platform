namespace Aristocrat.Bingo.Client.Messages
{
    using System.Threading;
    using System.Threading.Tasks;
    using GamePlay;

    /// <summary>
    ///     The game play service
    /// </summary>
    public interface IGameOutcomeService
    {
        /// <summary>
        ///     Requests a game play
        /// </summary>
        /// <param name="message">The game request message</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>Successfully requested.</returns>
        Task<bool> RequestGame(RequestGameOutcomeMessage message, CancellationToken token);

        /// <summary>
        ///     Requests a claim win
        /// </summary>
        /// <param name="message">The claim win message</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>Successfully requested.</returns>
        Task<ClaimWinResults> ClaimWin(RequestClaimWinMessage message, CancellationToken token);

        /// <summary>
        ///     Reports the game outcome to the server
        /// </summary>
        /// <param name="message">The report game outcome message</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>The response for the reported outcome</returns>
        Task<ReportGameOutcomeResponse> ReportGameOutcome(ReportGameOutcomeMessage message, CancellationToken token);
    }
}