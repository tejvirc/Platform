namespace Aristocrat.Bingo.Client.Messages
{
    using System.Threading;
    using System.Threading.Tasks;
    using Progressives;

    /// <summary>
    ///     The progressive claim service
    /// </summary>
    public interface IProgressiveClaimService
    {
        /// <summary>
        ///     Claims a progressive with the server
        /// </summary>
        /// <param name="message">The progressive claim request message</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>Returns the task for claiming a progressive</returns>
        Task<ProgressiveClaimResponse> ClaimProgressive(ProgressiveClaimRequestMessage message, CancellationToken token = default);
    }
}
