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
        /// <param name="message">The configuration message</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>Returns the task containing the client progressive information results from the server</returns>
        Task<ProgressiveInfoResults> RequestProgressiveInfo(ProgressiveInfoRequestMessage message, CancellationToken token);
    }
}
