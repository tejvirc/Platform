namespace Aristocrat.Monaco.Bingo.Services.GamePlay
{
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages.GamePlay;

    /// <summary>
    ///     Handles the bingo outcomes received
    /// </summary>
    public interface IBingoGameOutcomeHandler
    {
        /// <summary>
        ///     Process the game outcomes from the server
        /// </summary>
        /// <param name="outcome">The outcome to process</param>
        /// <param name="token">The cancellation token for this task</param>
        /// <returns>A whether or not the outcome was handled</returns>
        Task<bool> ProcessGameOutcome(GameOutcome outcome, CancellationToken token);

        /// <summary>
        ///     Process the claim win results from the server
        /// </summary>
        /// <param name="claim">The claim win results to process</param>
        /// <param name="token">The cancellation token for this task</param>
        /// <returns>A whether or not the claim was handled</returns>
        Task<bool> ProcessClaimWin(ClaimWinResults claim, CancellationToken token);
    }
}
