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
        /// <param name="machineSerial">The machine serial number</param>
        /// <param name="progressiveLevelId">The progressive level Id to claim</param>
        /// <param name="progressiveWinAmount">The expected progressive win amount</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>Returns the task for claiming a progressive</returns>
        Task<ProgressiveClaimResponse> ClaimProgressive(string machineSerial, long progressiveLevelId, long progressiveWinAmount, CancellationToken token = default);
    }
}
