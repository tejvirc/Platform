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
        ///     Process multiple game outcomes from the server
        /// </summary>
        /// <param name="outcomes">The outcomes to process</param>
        /// <param name="token">The cancellation token for this task</param>
        /// <returns>A whether or not all of the outcomes were handled</returns>
        Task<bool> ProcessGameOutcomes(GameOutcomes outcomes, CancellationToken token);

        /// <summary>
        ///     Process the claim win results from the server
        /// </summary>
        /// <param name="claim">The claim win results to process</param>
        /// <param name="token">The cancellation token for this task</param>
        /// <returns>A whether or not the claim was handled</returns>
        Task<bool> ProcessClaimWin(ClaimWinResults claim, CancellationToken token);

        /// <summary>
        ///     Process a progressive claim win by updating the outcome value with the amount.
        /// </summary>
        /// <param name="amount">The amount of the progressive win</param>
        /// <returns>Whether or not the claim was handled</returns>
        Task<bool> ProcessProgressiveClaimWin(long amount);
    }
}
