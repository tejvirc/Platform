namespace Aristocrat.Monaco.Bingo.Services.Progressives
{
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages.Progressives;

    /// <summary>
    ///     Handles the progressive claim received
    /// </summary>
    public interface IProgressiveClaimHandler
    {
        /// <summary>
        ///     Process the progressive claim from the server
        /// </summary>
        /// <param name="claim">The progressive claim message to process</param>
        /// <param name="token">The cancellation token for this task</param>
        /// <returns>Whether or not the progressive claim was handled</returns>
        Task<bool> ProcessProgressiveClaim(ProgressiveClaimMessage claim, CancellationToken token);
    }
}
