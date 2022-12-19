namespace Aristocrat.Bingo.Client.Messages
{
    using System.Threading;
    using System.Threading.Tasks;
    using Progressives;

    /// <summary>
    ///     The progressive service
    /// </summary>
    public interface IProgressiveService
    {
        /// <summary>
        ///     Gets the progressive information from the server
        /// </summary>
        /// <param name="message">The request progressive information message</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>Returns whether the client progressive information was retrieved from the server</returns>
        Task<bool> RequestProgressiveInfo(ProgressiveInfoRequestMessage message, CancellationToken token);

        /// <summary>
        ///     Gets a progressive update from the server
        /// </summary>
        /// <param name="message">The request progressive update message</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>Returns whether the client progressive update was retrieved from the server</returns>
        Task<bool> ProgressiveUpdates(ProgressiveUpdateRequestMessage message, CancellationToken token);
    }
}
